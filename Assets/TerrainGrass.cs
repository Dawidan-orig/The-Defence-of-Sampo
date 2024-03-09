using UnityEngine;

namespace Sampo.Core.Shaderworks
{
    [ExecuteAlways]
    [RequireComponent(typeof(TerrainCollider))]
    public class TerrainGrass : GrassTile
    {
        //TODO : Разбиение на кластеры травинок размерами 64X64, буферы не выдерживают большое количество травы.

        private GraphicsBuffer positionOffsetsBuffer;

        public override void OnValidate()
        {
            base.OnValidate();

            Terrain terra = GetComponent<Terrain>();
            patchSize = new Vector2(terra.terrainData.size.x, terra.terrainData.size.z);
        }

        public override void OnEnable()
        {
            base.OnEnable();

            positionOffsetsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, bladesAmount, VECTOR_STRIDE);

            SetTerrainOffsets(positionOffsetsBuffer);

            if(grassCompute)
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

            for (int i = 0; i < grassAmount.x * grassAmount.y; i++)
            {
                int indexX = (i % grassAmount.x);
                int indexY = grassAmount.y - (int)Mathf.Floor((float)i / grassAmount.x)-1;

                Vector3 worldBladePos = transform.TransformPoint(new Vector3(indexY * stepY,0, patchSize.x - indexX * stepX));
                data[indexX, indexY] = new Vector3(0, transform.position.y + terra.SampleHeight(worldBladePos), 0);
            }
            toBuffer.SetData(data);
        }
    }
}