Shader "Grass/DefaultGrass"
{
    Properties
    {
        _BottomColor ("Base Color", Color) = (1,1,1,1)
        _TipColor ("Tip Color", Color) = (1,1,1,1)
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
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

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

            
            float4 _BottomColor;
            float4 _TipColor;

            //#define PI 3.14159f

            float3x3 AngleAxis3x3(float angle, float3 axis) //TODO : Сделать helper-функцию для таких вещей
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normalWSLeft : NORMAL;
                float3 normalWSRight : NORMAL1;
                float3 positionWS : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs posIn = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = posIn.positionCS;// UnityObjectToClipPos(v.vertex);
                o.positionWS = posIn.positionWS;
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

                //SimonDev ref. Разворот травинки к камере по максимуму
                //TODO? : Нестабильная работа.
                // Сзади не работает.
                // С одной стороны - криво-косо.
                // Но в целом - нормуль
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

                // Чтобы не плодить точки, удваивая работу,
                // Я меняю направление нормалей на лету.
                // Так они получают свет как бы с двух сторон
                float3 viewDir = /*UNITY_MATRIX_IT_MV[2].xyz;*/ normalize(GetCameraPositionWS() - i.positionWS);
                //i.normalWSLeft = adjustNormal(i.normalWSLeft, viewDir);
                //i.normalWSRight = adjustNormal(i.normalWSRight, viewDir);

                // Rasterizer не очень хорошо двигает нормали, так что лучше это сделать напрямую вот здесь
                float mixFactor = i.uv.x;
                float3 resultNormal = normalize(lerp(i.normalWSLeft,i.normalWSRight, mixFactor));
                
                //DEBUG
                //col.rgb = (1+resultNormal)/2;

                InputData lightingInput = (InputData) 0;
                lightingInput.positionWS = i.positionWS;
                lightingInput.normalWS = resultNormal;
                lightingInput.viewDirectionWS =viewDir;

                SurfaceData surfaceInput = (SurfaceData) 0;            
                surfaceInput.albedo = col.rgb;
                surfaceInput.alpha = col.a;
                surfaceInput.specular = 0;
                surfaceInput.smoothness = 0;

                //TODO : По другую сторону, если света нет, всё окрасится в чёрный. Это не очень хорошо.
                // В идеале сделать тот же свет, что и на более освещённой стороне, но просто менее яркий
                // Либо можно просто создать второй источник света для имитации этого эффекта. Это имеет смысл.

                return UniversalFragmentBlinnPhong(lightingInput, surfaceInput);
            }
            ENDHLSL
            //ENDCG
        }
    }
}
