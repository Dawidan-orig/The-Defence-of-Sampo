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

            //IDEA : Просто создать ещё один Mesh поверх Terrain об его Height'ы,
            // И использовать шейдер травы на нём.
            // Но по факту, ничем от текстуры не отличается.
            Texture2D heightMapTexture = new(grassAmount.x, grassAmount.y);

            float[,] heightMap = data.GetHeights(0,0, data.heightmapResolution, data.heightmapResolution);

            for(int i = 0; i < grassAmount.x; i++) 
            {
                for(int j = 0; j < grassAmount.y; j++) 
                {                    
                    Vector2 scaledIndex = new((float)i/grassAmount.x * data.heightmapResolution,
                        (float)j/grassAmount.y * data.heightmapResolution);
                    // По каким-то причинам надо домножать на это, чтобы terrain нормально сочетался с травой.
                    float value = heightMap[(int)scaledIndex.x,(int) scaledIndex.y] * 10;
                    Color col = new(value, value, value, value);
                    Debug.DrawRay(transform.position + new Vector3(scaledIndex.x,0,scaledIndex.y), Vector3.up * value * data.heightmapResolution, Color.black, 3);
                    heightMapTexture.SetPixel(i,j, col);
                }
            }

            heightMapTexture.Apply();

            _computeInstance.SetTexture(kernelId, "_texMap", heightMapTexture);
            _computeInstance.SetBool("_terrainDep", true);
        }
    }
}