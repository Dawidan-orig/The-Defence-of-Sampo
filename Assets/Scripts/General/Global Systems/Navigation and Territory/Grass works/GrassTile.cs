using UnityEditor;
using UnityEngine;

namespace Sampo.Core.Shaderworks
{
    [ExecuteAlways]
    public class GrassTile : MonoBehaviour
    {
        public Vector2 patchSize = Vector2.one;
        public Vector2Int grassAmount = Vector2Int.one * 5;
        public ComputeShader grassCompute;
        public Material grassMaterial;

        public float grassHeight = 0.6f;
        public float grassWidth = 0.05f;
        public float maxBendAngle = Mathf.PI / 4;
        public float grassCurvature = 0.2f;
        public int segmentCount = 5;

        private Mesh grassSourceMesh;

        private GraphicsBuffer vertsBuffer;
        private GraphicsBuffer indicesBuffer;
        protected int kernelId;
        [Header("Lookonly")]
        [SerializeField]
        private int dispatchSize;

        [SerializeField]
        protected int bladesAmount;
        [SerializeField]
        int dispatchAmount;
        [SerializeField]
        int limitedBladesAmount;
        [SerializeField]
        int limitedVertsAmount;
        [SerializeField]
        int indicesLimitedSize;

        protected const int VECTOR_STRIDE = sizeof(float) * 3; //3 раза по размеру float - это вектор трёх измерений.
        protected const int INT_FLOAT_STRIDE = sizeof(float);
        protected const int VERTEX_STRIDE = VECTOR_STRIDE * 2 + INT_FLOAT_STRIDE * 2;

        protected const int MAX_VERTS_BUFFER = 4096;

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        protected struct GeneratedVertex
        {
            public Vector3 positionOS;
            public Vector3 normalOS;
            public Vector2 uv;
        }

        public virtual void OnValidate()
        {
            bladesAmount = grassAmount.x * grassAmount.y;
            int vertsAmount = bladesAmount * (segmentCount * 2 + 1);
            dispatchAmount = Mathf.CeilToInt((float)vertsAmount / MAX_VERTS_BUFFER);
            limitedBladesAmount = bladesAmount;
            if (dispatchAmount > 1)
                limitedBladesAmount = Mathf.CeilToInt((float)MAX_VERTS_BUFFER / (segmentCount*2+1));

            limitedVertsAmount = limitedBladesAmount * (segmentCount * 2 + 1);
            indicesLimitedSize = limitedBladesAmount * (segmentCount * 2 - 1) * 3;
        }

        public virtual void OnEnable()
        {
            if (grassCompute)
            {
                SetupConstraintsBuffers();
                DisplaceGrass();
            }
            else
                OnDisable();
        }

        public virtual void OnDisable()
        {
            if (vertsBuffer != null)
                vertsBuffer.Release();
            if(indicesBuffer != null)
                indicesBuffer.Release();
        }

        protected void SetupConstraintsBuffers() 
        {
            vertsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, limitedVertsAmount, VERTEX_STRIDE);
            indicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, indicesLimitedSize, INT_FLOAT_STRIDE);

            kernelId = grassCompute.FindKernel("CSMain");

            grassCompute.SetBuffer(kernelId, "_offsets", new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, VECTOR_STRIDE));
            grassCompute.SetBuffer(kernelId, "_resVerts", vertsBuffer);
            grassCompute.SetBuffer(kernelId, "_resIndices", indicesBuffer);

            float time;
            if (EditorApplication.isPlaying)
                time = Shader.GetGlobalVector("_Time").w;
            else
                time = (float)EditorApplication.timeSinceStartup;
            grassCompute.SetFloat("_Time", time);

            grassCompute.SetFloat("_width", grassWidth);
            grassCompute.SetFloat("_height", grassHeight);
            grassCompute.SetFloat("_curvature", grassCurvature);
            grassCompute.SetFloat("_maxBendAngleRad", maxBendAngle);
            grassCompute.SetInt("_segmentCount", segmentCount);
        }

        protected virtual void DisplaceGrass()
        {
            grassCompute.GetKernelThreadGroupSizes(kernelId, out uint numThreads, out _, out _);

            if(transform.childCount > dispatchAmount) 
            {
                for (int i = transform.childCount - 1; i >= dispatchAmount; i--)
                {
                    if(!EditorApplication.isPlaying)
                        DestroyImmediate(transform.GetChild(i).gameObject);
                    else
                        Destroy(transform.GetChild(i).gameObject);
                }
            }

            Vector2Int limitedGrassAmount = grassAmount;
            limitedGrassAmount.y = Mathf.CeilToInt(limitedBladesAmount / limitedGrassAmount.x);

            Vector2 step = new(patchSize.x / (grassAmount.x), patchSize.y / (grassAmount.y));

            Vector2 localPatchSize = new(step.x * limitedGrassAmount.x, step.y * limitedGrassAmount.y);

            for (int currentDispatch = 0; currentDispatch < dispatchAmount; currentDispatch++)
            {
                //TODO : Смещение PatchSize, так как послений кусочек не влезает ровно
                //TODO : Поправка PatchSize
                int remainingBlades = bladesAmount - currentDispatch * limitedBladesAmount;

                float usedPatchLength = localPatchSize.y;
                // Для последнего крайнего кусочка
                if (remainingBlades < limitedBladesAmount)
                {
                    limitedGrassAmount.y = remainingBlades / limitedGrassAmount.x + 1;
                    usedPatchLength = step.y * limitedGrassAmount.y; 
                }

                grassCompute.SetVector("_patchSize", new Vector2(localPatchSize.x, usedPatchLength/dispatchAmount));
                Vector2 toPass = new Vector2(limitedGrassAmount.x, limitedGrassAmount.y);
                grassCompute.SetVector("_bladesAmount", toPass);

                dispatchSize = Mathf.CeilToInt((float)limitedBladesAmount / numThreads);
                grassCompute.Dispatch(kernelId, dispatchSize, 1, 1);

                GeneratedVertex[] generatedVertices = new GeneratedVertex[limitedVertsAmount];
                int[] generatedIndices = new int[indicesLimitedSize];
                vertsBuffer.GetData(generatedVertices);
                indicesBuffer.GetData(generatedIndices);

                Vector3[] generatedPoints = new Vector3[limitedVertsAmount];
                Vector3[] normals = new Vector3[limitedVertsAmount];
                Vector2[] UVs = new Vector2[limitedVertsAmount];

                for (int i = 0; i < limitedVertsAmount; i++)
                {
                    var v = generatedVertices[i];
                    generatedPoints[i] = v.positionOS; //OffsetToCenter()
                    normals[i] = v.normalOS;
                    UVs[i] = v.uv;
                }
                grassSourceMesh = new Mesh();
                grassSourceMesh.SetVertices(generatedPoints);
                grassSourceMesh.SetUVs(0, UVs);
                grassSourceMesh.SetNormals(normals);
                grassSourceMesh.SetIndices(generatedIndices, MeshTopology.Triangles, 0, true);
                grassSourceMesh.Optimize();

                Transform correlatedChild =
                    transform.childCount-1 < currentDispatch ?
                    null : transform.GetChild(currentDispatch);

                MeshFilter meshFilter;
                if (!correlatedChild)
                {
                    GameObject grassMeshHolder = new();
                    grassMeshHolder.transform.parent = transform;
                    grassMeshHolder.name = "dispatch#" + currentDispatch + " grass";
                    meshFilter = grassMeshHolder.AddComponent<MeshFilter>();
                    MeshRenderer renderer = grassMeshHolder.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = grassMaterial;
                }
                else 
                {
                    meshFilter = correlatedChild.GetComponent<MeshFilter>();
                }

                meshFilter.transform.position = transform.TransformPoint(Vector3.forward * currentDispatch * localPatchSize.y);
                meshFilter.mesh = grassSourceMesh;
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