using UnityEngine;
using UnityEngine.UIElements;

namespace Sampo.Core.Shaderworks
{
    [ExecuteAlways]
    public class GrassTile : MonoBehaviour
    {
        public Vector2 patchSize = Vector2.one;
        public Vector2Int grassAmount = Vector2Int.one * 5;
        public Material grassShaderMaterial;
        public ComputeShader grassCompute;

        public float grassHeight = 0.6f;
        public float grassWidth = 0.15f;
        public float maxBendAngle = 45 * Mathf.Deg2Rad;
        public float grassCurvature = 0.2f;
        public int maxBladeSegments = 7;

        private Mesh grassSourceMesh;

        private ComputeBuffer vertsBuffer;
        private ComputeBuffer indicesBuffer;
        private int kernelId;
        private int dispatchSize;

        private const int VECTOR_STRIDE = sizeof(float) * 3; //3 раза по размеру float - это вектор трёх измерений.
        private const int FLOAT_STRIDE = sizeof(float);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct GeneratedVertex
        {
            public Vector3 positionOS;
            public Vector3 normalOS;
            public Vector2 uv;
        }

        private void OnValidate()
        {


            OnDisable();
            OnEnable();
        }

        void OnEnable()
        {
            vertsBuffer = new ComputeBuffer(grassAmount.x * grassAmount.y*3, VECTOR_STRIDE * 2 + FLOAT_STRIDE*2, ComputeBufferType.Append);
            indicesBuffer = new ComputeBuffer(grassAmount.x * grassAmount.y * 3, FLOAT_STRIDE, ComputeBufferType.Append);

            if (grassCompute)
            {
                kernelId = grassCompute.FindKernel("CSMain");

                grassCompute.SetBuffer(kernelId, "_resVerts", vertsBuffer);
                grassCompute.SetBuffer(kernelId, "_resIndices", indicesBuffer);
                grassCompute.SetVector("_patchSize", patchSize);
                Vector2 toPass = new Vector2(grassAmount.x, grassAmount.y);
                grassCompute.SetVector("_bladesAmount", toPass);
                grassCompute.SetVector("_centralizedWS", transform.position - new Vector3(patchSize.x, 0, patchSize.y) / 2);

                grassCompute.SetFloat("_width", grassWidth);
                grassCompute.SetFloat("_height", grassHeight);
                grassCompute.SetFloat("_curvature", grassCurvature);
                grassCompute.SetFloat("_maxBendAngleRad", maxBendAngle);
                grassCompute.SetInt("_maxSegmentCount", maxBladeSegments);

                grassCompute.GetKernelThreadGroupSizes(kernelId, out uint numThreads, out _, out _);
                dispatchSize = Mathf.CeilToInt((float)grassAmount.x * grassAmount.y / numThreads);      

                DisplaceGrass();
            }
            else
                OnDisable();
        }

        private void OnDisable()
        {
            if (vertsBuffer != null)
            {
                vertsBuffer.Release();
                indicesBuffer.Release();
            }
        }

        void LateUpdate()
        {
            /*if (!Application.isPlaying)
            {
                OnDisable();
                OnEnable();
            }*/

            /*
            RenderParams rParams = new RenderParams
            {
                material = grassShaderMaterial,
                receiveShadows = true,
                worldBounds = new Bounds(transform.position - new Vector3(patchSize.x, 0, patchSize.y) / 2,
                                         new Vector3(patchSize.x, grassHeight, patchSize.y))
            };

            Graphics.RenderMeshPrimitives(rParams, grassSourceMesh, 0, grassAmount.x * grassAmount.y);*/
        }

        void DisplaceGrass()
        {
            if (grassCompute)
            {
                grassCompute.Dispatch(kernelId, dispatchSize, 1, 1); //Вот в этом месте получаем заполненный буффер

                int vertsAmount = grassAmount.x * grassAmount.y * 3;
                GeneratedVertex[] resultVertices = new GeneratedVertex[vertsAmount];
                int[] resultIndices = new int[vertsAmount];
                vertsBuffer.GetData(resultVertices);
                indicesBuffer.GetData(resultIndices);

                Vector3[] generatedPoints = new Vector3[vertsAmount];
                Vector3[] normals = new Vector3[vertsAmount];
                Vector2[] UVs = new Vector2[vertsAmount];

                for (int i = 0; i < vertsAmount; i++) 
                {
                    var v = resultVertices[i];
                    generatedPoints[i] = OffsetToCenter(v.positionOS);
                    normals[i] = v.normalOS;
                    UVs[i] = v.uv;
                }
                grassSourceMesh = new Mesh();
                grassSourceMesh.SetVertices(generatedPoints);
                grassSourceMesh.SetUVs(0,UVs);
                grassSourceMesh.SetNormals(normals);
                grassSourceMesh.SetIndices(resultIndices, MeshTopology.Triangles, 0, true);
                grassSourceMesh.Optimize();

                GetComponent<MeshFilter>().mesh = grassSourceMesh;
            }
        }

        Vector3 OffsetToCenter(Vector3 pointToOffset) 
        {
            return pointToOffset - new Vector3(patchSize.x, 0, patchSize.y) / 2;
        }

        Vector3 ConvertToWorldspace(Vector3 objectspace) 
        {
            return transform.position + OffsetToCenter(objectspace);
        }
    }
}