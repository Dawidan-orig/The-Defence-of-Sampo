using UnityEditor;
using UnityEngine;

namespace Sampo.Core.Shaderworks
{
    [ExecuteAlways]
    [SelectionBase]
    public class GrassTile : MonoBehaviour
    {
        public Vector2 patchSize = Vector2.one;
        public Vector2Int grassAmount = Vector2Int.one * 5;
        public ComputeShader grassCompute;
        public Material grassMaterial;

        public float grassHeight = 0.6f;
        public float grassWidth = 0.05f;
        public float maxBendAngle = Mathf.PI / 4;
        public float grassCurvature = 0.45f;
        public int segmentCount = 5;
        public string meshSavingPath = "Assets/Unity Data Forms/VFX/Meshes/grass/";
        public string assetName = "new grass Mesh";

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
        [SerializeField]
        protected ComputeShader _computeInstance;

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
                limitedBladesAmount = Mathf.CeilToInt((float)MAX_VERTS_BUFFER_LENGTH / (segmentCount * 2 + 1));

            limitedVertsAmount = limitedBladesAmount * (segmentCount * 2 + 1);
            indicesLimitedSize = limitedBladesAmount * (segmentCount * 2 - 1) * 3;
        }

        public virtual void OnEnable()
        {
            _computeInstance = Instantiate(grassCompute);

            if (_computeInstance)
            {
                SetupConstraintsAndBuffers();
                DisplaceGrass();
            }
            else
                OnDisable();
        }

        public virtual void OnDisable()
        {
            vertsBuffer?.Release();
            indicesBuffer?.Release();
            DestroyImmediate(_computeInstance);
        }

        protected virtual void SetupConstraintsAndBuffers()
        {
            vertsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, limitedVertsAmount, VERTEX_STRIDE);
            indicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, indicesLimitedSize, INT_FLOAT_STRIDE);

            Vector3 grassSize = new Vector3(patchSize.x,grassHeight/2,patchSize.y);
            //TODO : Instance для шейдера
            //TODO : Установка Instance этого шейдера во все элементы LODGroup
            // Такой шейдер должен быть один на все патчи этого gameObject!
            // _localPatchAmount _localPatchPos - тоже по одному для всего этого gameObject
            // В сущности, в туда передаётся напрямую просто patchSize и grassAmount

            kernelId = _computeInstance.FindKernel("CSMain");

            _computeInstance.SetBuffer(kernelId, "_resVerts", vertsBuffer);
            _computeInstance.SetBuffer(kernelId, "_resIndices", indicesBuffer);

            _computeInstance.SetFloat("_width", grassWidth);
            _computeInstance.SetFloat("_height", grassHeight);
            _computeInstance.SetFloat("_curvature", grassCurvature);
            _computeInstance.SetFloat("_maxBendAngleRad", maxBendAngle);
            _computeInstance.SetInt("_segmentCount", segmentCount);
        }

        protected virtual void DisplaceGrass()
        {
            // TODO : Размещение LOD-патчей травинок через OnEnabled.
            // Сделать банальным instantiate
            // Размеры определять через Bounds Mesh'а и стараться разместить как можно более равномерно
        }

        public void SaveMeshFromComputeShader()
        {
            _computeInstance.GetKernelThreadGroupSizes(kernelId, out uint numThreads, out _, out _);

            // TODO : Это всё лишнее, надо просто определить максимальый patch для Mesh'а
            // Сделать Mesh по этому максимальному размеру
            // И влепить его везде.
            // Дальше пусть шейдер сам определяет высоты по текстуре как есть.
            int limitedSquareSideSize = Mathf.FloorToInt(Mathf.Sqrt(limitedBladesAmount));
            Vector2Int dispatches = new(Mathf.CeilToInt(grassAmount.x / limitedSquareSideSize),
                                         Mathf.CeilToInt(grassAmount.y / limitedSquareSideSize));

            Vector2 step = new(patchSize.x / (grassAmount.x), patchSize.y / (grassAmount.y));

            Vector2 localPatchSize = new(step.x * limitedSquareSideSize, step.y * limitedSquareSideSize);
            Vector2Int localGrassAmount = new(limitedSquareSideSize, limitedSquareSideSize);

            _computeInstance.SetVector("_patchSize", new Vector2(patchSize.x, patchSize.y));
            Vector2 toPass = new Vector2(localGrassAmount.x, localGrassAmount.y);
            _computeInstance.SetVector("_bladesAmount", toPass);
            _computeInstance.SetVector("_patchPos", Vector3.zero);
            _computeInstance.SetInt("_clusterID", 1);

            dispatchSize = Mathf.CeilToInt((float)limitedBladesAmount / numThreads);
            _computeInstance.Dispatch(kernelId, dispatchSize, 1, 1);

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

            Mesh initialMesh = (Mesh) AssetDatabase.LoadAssetAtPath(meshSavingPath, typeof(Mesh));
            Mesh grassSourceMesh;

            grassSourceMesh =
                initialMesh == null ? new Mesh() : initialMesh;

            grassSourceMesh.SetVertices(generatedPoints);
            grassSourceMesh.SetUVs(0, UVs);
            grassSourceMesh.SetNormals(normals);
            grassSourceMesh.SetIndices(generatedIndices, MeshTopology.Triangles, 0, true);
            grassSourceMesh.Optimize();

            const string ASSET = ".asset";

            if (initialMesh != null)
            {
                initialMesh.Clear();
                EditorUtility.CopySerialized(grassSourceMesh, initialMesh);
            }
            else
                AssetDatabase.CreateAsset(grassSourceMesh, meshSavingPath + assetName + ASSET);
            
            AssetDatabase.SaveAssets();
        }
    }
}