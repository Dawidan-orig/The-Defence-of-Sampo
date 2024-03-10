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

        protected const int MAX_VERTS_BUFFER_LENGTH = 4096;

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
            dispatchAmount = Mathf.CeilToInt((float)vertsAmount / MAX_VERTS_BUFFER_LENGTH);
            limitedBladesAmount = bladesAmount;
            if (dispatchAmount > 1)
                limitedBladesAmount = Mathf.CeilToInt((float)MAX_VERTS_BUFFER_LENGTH / (segmentCount*2+1));

            limitedVertsAmount = limitedBladesAmount * (segmentCount * 2 + 1);
            indicesLimitedSize = limitedBladesAmount * (segmentCount * 2 - 1) * 3;

            OnDisable();
            OnEnable();
        }

        public virtual void OnEnable()
        {
            if (grassCompute)
            {
                SetupConstraintsAndBuffers();
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

        private void LateUpdate()
        {
            if(EditorApplication.isPlaying)
                DisplaceGrass();
        }

        protected virtual void SetupConstraintsAndBuffers() 
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

            int limitedSquareSideSize = Mathf.FloorToInt(Mathf.Sqrt(limitedBladesAmount));
            Vector2Int dispatches = new (Mathf.CeilToInt(grassAmount.x / limitedSquareSideSize),
                                         Mathf.CeilToInt(grassAmount.y / limitedSquareSideSize));

            Vector2 step = new(patchSize.x / (grassAmount.x), patchSize.y / (grassAmount.y));
            Matrix4x4[] resultMatrices = new Matrix4x4[dispatches.x * dispatches.y];

            Vector2 localPatchSize = new(step.x * limitedSquareSideSize, step.y * limitedSquareSideSize);
            Vector2Int localGrassAmount = new(limitedSquareSideSize, limitedSquareSideSize);

            //TODO : Вернуть создание GameObject'ов, не получится обойтись без этого.
            // Итак, проблемы которые надо бы сочетать друг с другом.
            // 1: GPU Instance. Его очень желательно использовать.
            // 2: Высота через Terrain, получение через Compute Shader.
            // 1 и 2 не сочетаются при кластерной работе. Надо разбивать на травинки.
            // Допустим, я сделал травинку тут, в CPU.
            // Тогда в ComputeShader бросаются данные Terrain'а. Там делается SamplePosition.
            // ComputeShader же возвращает позиции и повороты для каждой травинки.
            // Здесь в CPU оно рендерится.
            // 3: LOD. NMG сделал его в ComputeShader. Это делает каждую травинку уникальной, что не сочетается с 1 СОВЕРШЕННО.
            // Есть вариант просто не использовать GPU Instancing: https://docs.unity3d.com/Manual/GPUInstancing.html
            // И тупо не париться обо всех проблемах. Так и поступлю, пожалуй.
            for (int clusterX = 0; clusterX < dispatches.x; clusterX++) 
            {
                for(int clusterY = 0; clusterY < dispatches.y; clusterY++) 
                {
                    int currentDispatch = clusterX * dispatches.x + clusterY;

                    Vector3 resPos = transform.TransformPoint(Vector3.forward * clusterX * (localPatchSize.x))
                                   + transform.TransformPoint(Vector3.right * clusterY * (localPatchSize.y));

                    resultMatrices[currentDispatch] = new Matrix4x4();
                    resultMatrices[currentDispatch].SetTRS(resPos, Quaternion.identity, Vector3.one);
                }
            }

            grassCompute.SetVector("_patchSize", new Vector2(localPatchSize.x, localPatchSize.y));
            Vector2 toPass = new Vector2(localGrassAmount.x, localGrassAmount.y);
            grassCompute.SetVector("_bladesAmount", toPass);
            grassCompute.SetInt("_clusterID", 1);

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
                generatedPoints[i] = v.positionOS;
                normals[i] = v.normalOS;
                UVs[i] = v.uv;
            }
            grassSourceMesh = new Mesh();
            grassSourceMesh.SetVertices(generatedPoints);
            grassSourceMesh.SetUVs(0, UVs);
            grassSourceMesh.SetNormals(normals);
            grassSourceMesh.SetIndices(generatedIndices, MeshTopology.Triangles, 0, true);
            grassSourceMesh.Optimize();

            Graphics.DrawMeshInstanced(grassSourceMesh, 0, grassMaterial, resultMatrices);
        }
    }
}