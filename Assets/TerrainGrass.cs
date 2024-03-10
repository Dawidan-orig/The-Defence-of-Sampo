using Unity.Mathematics;
using UnityEngine;

namespace Sampo.Core.Shaderworks
{
    [ExecuteAlways]
    [RequireComponent(typeof(TerrainCollider))]
    public class TerrainGrass : GrassTile
    {
        private GraphicsBuffer positionOffsetsBuffer;

        public override void OnValidate()
        {
            base.OnValidate();

            Terrain terra = GetComponent<Terrain>();
            patchSize = new Vector2(terra.terrainData.size.x, terra.terrainData.size.z);
        }

        public override void OnEnable()
        {
            positionOffsetsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, bladesAmount, VECTOR_STRIDE);

            SetTerrainOffsets(positionOffsetsBuffer);

            base.OnEnable();
        }

        protected override void SetupConstraintsAndBuffers()
        {
            base.SetupConstraintsAndBuffers();
            if (grassCompute)
                grassCompute.SetBuffer(kernelId, "_offsets", positionOffsetsBuffer);
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (positionOffsetsBuffer != null)
            {
                positionOffsetsBuffer.Release();
            }
        }

        void SetTerrainOffsets(GraphicsBuffer toBuffer)
        {
            //TODO! : Убрать в шейдер, найдя/сделав копирку функции Terrain'а.
            Terrain terra = GetComponent<Terrain>();
            Vector3[,] data = new Vector3[grassAmount.x, grassAmount.y];

            float stepX = patchSize.x / grassAmount.x;
            float stepY = patchSize.y / grassAmount.y;

            for (int i = 0; i < grassAmount.x; i++)
            {
                for (int j = 0; j < grassAmount.y; j++)
                {
                    Vector3 worldBladePos = transform.TransformPoint(new Vector3(j * stepY, 0, patchSize.x - i * stepX));
                    data[i, j] = new Vector3(0, transform.position.y + terra.SampleHeight(worldBladePos), 0);
                }
            }
            toBuffer.SetData(data);
        }

        //TODO : Перенос этого в HLSL-файл, вместе с функцией SetTerrainOffsets()
        //https://discussions.unity.com/t/how-is-terrain-sampleheight-implemented/224833/3
        public void SampleHeight(ref float3 worldPos, TerrainData data)
        {
            Bounds terrainAABB = data.bounds;
            int heightMapResolution = data.heightmapResolution;
            float[,] heightMap = data.GetHeights(0, 0, heightMapResolution, heightMapResolution);

            float3 localPos = worldPos - (float3)terrainAABB.min;
            float2 sampleValue = new float2(
                localPos.x / terrainAABB.size.x,
                localPos.z / terrainAABB.size.z);
            float2 samplePos = new float2(
                 sampleValue.x * (heightMapResolution - 1),
                 sampleValue.y * (heightMapResolution - 1));
            int2 sampleFloor = new int2(
                (int)samplePos.x,
                (int)samplePos.y);
            float2 sampleDecimal = new float2(
                samplePos.x - sampleFloor.x,
                samplePos.y - sampleFloor.y);
            int upperLeftTri = sampleDecimal.y > sampleDecimal.x ? 1 : 0;

            float3 v0 = GetVertexLocalPos(sampleFloor.x, sampleFloor.y, terrainAABB, heightMapResolution, heightMap);
            float3 v1 = GetVertexLocalPos(sampleFloor.x + 1, sampleFloor.y + 1, terrainAABB, heightMapResolution, heightMap);
            int upperLeftOrLowerRightX = sampleFloor.x + 1 - upperLeftTri;
            int upperLeftOrLowerRightY = sampleFloor.y + upperLeftTri;
            float3 v2 = GetVertexLocalPos(upperLeftOrLowerRightX, upperLeftOrLowerRightY, terrainAABB, heightMapResolution, heightMap);
            float3 n = math.cross(v1 - v0, v2 - v0);
            // based on plane formula: a(x - x0) + b(y - y0) + c(z - z0) = 0
            float localY = ((-n.x * (localPos.x - v0.x) - n.z * (localPos.z - v0.z)) / n.y) + v0.y;
            worldPos.y = localY + terrainAABB.min.y;
        }

        float3 GetVertexLocalPos(int x, int y, Bounds terrainAABB, int heightMapResolution, float[,] heightMap)
        {
            float heightValue = heightMap[x,y];
            return new float3(
                (float)x / (heightMapResolution - 1) * terrainAABB.size.x,
                heightValue * terrainAABB.size.y,
                (float)y / (heightMapResolution - 1) * terrainAABB.size.z);
        }
    }
}