using Unity.Mathematics;
using UnityEngine;

namespace Sampo.Core.Shaderworks
{
    [ExecuteAlways]
    [SelectionBase]
    [RequireComponent(typeof(TerrainCollider))]
    public class TerrainGrass : GrassTile
    {
        public override void OnValidate()
        {
            base.OnValidate();

            Terrain terra = GetComponent<Terrain>();
            patchSize = new Vector2(terra.terrainData.size.x, terra.terrainData.size.z);
        }

        protected override void SetupConstraintsAndBuffers()
        {
            base.SetupConstraintsAndBuffers();
            _grassShaderInstance.SetVector("_localPatchPos", transform.position);
            _grassShaderInstance.SetVector("_localPatchAmount", new Vector2(grassAmount.x, grassAmount.y));
            if (_computeInstance)
                SetTerrainData();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        void SetTerrainData()
        {
            Terrain terra = GetComponent<Terrain>();
            TerrainData data = terra.terrainData;

            _computeInstance.SetVector("_terrainSize", data.bounds.size);
            _computeInstance.SetVector("_terrainMin", data.bounds.min);
            _computeInstance.SetInt("_terrainResolution", data.heightmapResolution);

            //IDEA : ������ ������� ��� ���� Mesh ������ Terrain �� ��� Height'�,
            // � ������������ ������ ����� �� ��.
            // �� �� �����, ����� �� �������� �� ����������.
            Texture2D heightMapTexture = new(grassAmount.x, grassAmount.y);

            float[,] heightMap = data.GetHeights(0,0, data.heightmapResolution, data.heightmapResolution);

            for(int i = 0; i < grassAmount.x; i++) 
            {
                for(int j = 0; j < grassAmount.y; j++) 
                {                    
                    Vector2 scaledIndex = new((float)i/grassAmount.x * data.heightmapResolution,
                        (float)j/grassAmount.y * data.heightmapResolution);
                    // �� �����-�� �������� ���� ��������� �� ���, ����� terrain ��������� ��������� � ������.
                    float value = heightMap[(int)scaledIndex.x,(int) scaledIndex.y] * patchSize.x;
                    Color col = new(value, value, value, value);
                    heightMapTexture.SetPixel(i,j, col);
                }
            }

            heightMapTexture.Apply();
            //TODO : ������������ �������� ����� 5 MB. ��� ����� �����! �� 100 Terrain'�� ��� ������ ��� ����� �������� ���������!
            _grassShaderInstance.SetTexture("_localOffsetMap", heightMapTexture);
        }
    }
}