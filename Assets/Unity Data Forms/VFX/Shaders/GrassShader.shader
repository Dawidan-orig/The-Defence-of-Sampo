Shader "Grass/DefaultGrass"
{
    Properties
    {
        _BottomColor ("Base Color", Color) = (1,1,1,1)
        _TipColor ("Tip Color", Color) = (1,1,1,1)
        _localPatchPos("pos", vector) = (0,0,0)
        _localOffsetMap("texture", 2D) = "" {}
        //TODO : RandomJitter чтобы уменьшить шаблонность
    }
    SubShader
    {
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
        LOD 100
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}

            //CGPROGRAM
            HLSLPROGRAM
            // Signal this shader requires a compute buffer
            //#pragma prefer_hlslcc gles
            //#pragma exclude_renderers d3d11_9x

            // Lighting and shadow keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex vert
            #pragma fragment frag

            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            //#include "UnityCG.cginc"

            sampler2D _localOffsetMap;
            int2 _localPatchAmount;
            float3 _localPatchPos;
            
            float4 _BottomColor;
            float4 _TipColor;

            float3x3 AngleAxis3x3(float angle, float3 axis) //TODO : —делать helper-функцию дл€ таких вещей
            {
                float c, s;
                sincos(angle, s, c);

                float t = 1 - c;
                float x = axis.x;
                float y = axis.y;
                float z = axis.z;

                return float3x3(
                t * x * x + c, t * x * y - s * z, t * x * z + s * y,
                t * x * y + s * z, t * y * y + c, t * y * z - s * x,
                t * x * z - s * y, t * y * z + s * x, t * z * z + c
                );
            }

            float3 adjustNormal(float3 normal, float3 viewDir)
            {
                if(saturate(dot(normal.xz, viewDir.xz)) < 0)
                    normal = -normal;

                return normal;
            }
            float easeOut(float t) 
            {
                return sin((t*PI)/2);
            }
            float toTipEaseIn(float t) 
            {
                return t*t*t;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 anchorPos : TEXCOORD1;                
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normalWSLeft : NORMAL;
                float3 normalWSRight : NORMAL1;
                float3 positionWS : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                UNITY_SETUP_INSTANCE_ID(v);

                v2f o;
                
                float3 anchorPosWS = GetVertexPositionInputs(v.anchorPos.xyz).positionWS;                

                VertexPositionInputs posIn = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = posIn.positionCS;
                o.positionWS = posIn.positionWS;

                float2 textureConverted = float2(
                floor((anchorPosWS.x - _localPatchPos.x)*10),
                floor((anchorPosWS.z - _localPatchPos.z)*10));
                //TODO : если разница с сосед€ми слишком велика в высоте - сбросить данные
                //TODO : refactor кода, ибо он убитый уже
                //o.positionWS = float3(textureConverted.x, 0, textureConverted.y);
                int2 resolution = int2(1000,1000);

                float heightTexOffset = tex2Dlod(_localOffsetMap,
                float4(textureConverted.y/resolution.y,textureConverted.x/resolution.x, 0.0f, 0.0f));
                //heightTexOffset.zx = heightTexOffset.xz; //текстура Terrain'а сама по себе смещена, так что это нужно сделать

                //o.positionWS.xyz += heightTexOffset.xyz;
                float HEIGHT_MULTIPLY_VAL = 10.5f;
                o.positionWS.y += heightTexOffset * HEIGHT_MULTIPLY_VAL;

                o.uv = v.uv;
                float3 vertNormal = GetVertexNormalInputs(v.normal).normalWS;

                float3 viewDir = normalize(GetCameraPositionWS() - o.positionWS);
                vertNormal = adjustNormal(vertNormal, viewDir);

                float3 yNormal = float3(0,1,0);
                float angle = 30 * 2 * PI / 360;

                float3x3 rotationMatrix = AngleAxis3x3(angle, yNormal);
                o.normalWSLeft = mul(rotationMatrix, vertNormal);
                rotationMatrix = AngleAxis3x3(-angle, yNormal);
                o.normalWSRight = mul(rotationMatrix, vertNormal);

                float mixFactor = o.uv.x;
                float3 resultNormal = normalize(lerp(o.normalWSLeft,o.normalWSRight, mixFactor));

                //SimonDev ref. –азворот травинки к камере по максимуму
                //TODO? : Ќестабильна€ работа.
                // —зади не работает.
                // — одной стороны - криво-косо.
                // Ќо в целом - нормуль
                float3x3 localXDirMatrix = AngleAxis3x3(90, yNormal);
                float3 xDirectionOS = mul(localXDirMatrix, vertNormal);
                float grassWidth = 0.1f;
                float viewDotNormal = saturate(dot(resultNormal.xz, viewDir.xz));
                float viewSpaceThickenFactor = easeOut(viewDotNormal);
                viewSpaceThickenFactor *= smoothstep(-0.2, 0.2, viewDotNormal)/2;
                o.vertex.x += viewSpaceThickenFactor * xDirectionOS * grassWidth;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = lerp(_BottomColor,_TipColor, toTipEaseIn(i.uv.y));

                //Ref SimonDev, Special Ambient Occlusion

                // „тобы не плодить точки, удваива€ работу,
                // я мен€ю направление нормалей на лету.
                // “ак они получают свет как бы с двух сторон
                float3 viewDir = /*UNITY_MATRIX_IT_MV[2].xyz;*/ normalize(GetCameraPositionWS() - i.positionWS);
                //i.normalWSLeft = adjustNormal(i.normalWSLeft, viewDir);
                //i.normalWSRight = adjustNormal(i.normalWSRight, viewDir);

                // Rasterizer не очень хорошо двигает нормали, так что лучше это сделать напр€мую вот здесь
                float mixFactor = i.uv.x;
                float3 resultNormal = normalize(lerp(i.normalWSLeft,i.normalWSRight, mixFactor));
                
                //DEBUG
                //col.rgb = (1+resultNormal)/2;
                col.rgb = i.positionWS.xyz;

                InputData lightingInput = (InputData) 0;
                lightingInput.positionWS = i.positionWS;
                lightingInput.normalWS = resultNormal;
                lightingInput.viewDirectionWS = viewDir;

                SurfaceData surfaceInput = (SurfaceData) 0;            
                surfaceInput.albedo = col.rgb;
                surfaceInput.alpha = col.a;
                surfaceInput.specular = 0;
                surfaceInput.smoothness = 0;

                return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
            }
            ENDHLSL
            //ENDCG
        }
    }
}
