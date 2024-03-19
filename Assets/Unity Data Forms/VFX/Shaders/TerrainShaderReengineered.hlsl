float3 _terrainSize;
float3 _terrainMin;
int _terrainResolution;

//#ifndef TerrainShaderReengineered
//#define TerrainShaderReengineered

Texture2D _texMap;

//https://discussions.unity.com/t/how-is-terrain-sampleheight-implemented/224833/3
        float3 GetVertexLocalPos(int x, int y)
        {
            float heightValue = _texMap[int2(y,x)].x;
            return float3(
                (float)x / (_terrainResolution - 1) * _terrainSize.x,
                heightValue * _terrainSize.y,
                (float)y / (_terrainResolution - 1) * _terrainSize.z);
        }

        float3 SampleHeight(float3 worldPos)
        {
            float3 localPos = worldPos - _terrainMin;
            float2 sampleValue = float2(
                localPos.x / _terrainSize.x,
                localPos.z / _terrainSize.z);
            float2 samplePos = float2(
                 sampleValue.x * (_terrainResolution - 1),
                 sampleValue.y * (_terrainResolution - 1));
            int2 sampleFloor = int2(
                (int)samplePos.x,
                (int)samplePos.y);
            float2 sampleDecimal = float2(
                samplePos.x - sampleFloor.x,
                samplePos.y - sampleFloor.y);
            int upperLeftTri = sampleDecimal.y > sampleDecimal.x ? 1 : 0;

            float3 v0 = GetVertexLocalPos(sampleFloor.x, sampleFloor.y);
            float3 v1 = GetVertexLocalPos(sampleFloor.x + 1, sampleFloor.y + 1);
            int upperLeftOrLowerRightX = sampleFloor.x + 1 - upperLeftTri;
            int upperLeftOrLowerRightY = sampleFloor.y + upperLeftTri;
            float3 v2 = GetVertexLocalPos(upperLeftOrLowerRightX, upperLeftOrLowerRightY);
            float3 n = cross(v1 - v0, v2 - v0);
            // based on plane formula: a(x - x0) + b(y - y0) + c(z - z0) = 0
            float localY = ((-n.x * (localPos.x - v0.x) - n.z * (localPos.z - v0.z)) / n.y) + v0.y;
            worldPos.y = localY + _terrainMin.y;
            //worldPos.y += _heightMap[(int)worldPos.x] * _terrainSize.y;

            return worldPos;
        }