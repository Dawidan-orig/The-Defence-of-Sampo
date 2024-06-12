using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Sampo.Core.Shaderworks
{
    [ExecuteAlways]
    [SelectionBase]
    public class GrassTile : MonoBehaviour
    {
        #region parameters
        //TODO : [Tooltip]-ы
        [Header("Patch data")]
        public Material grassMaterial;
        public GameObject grassPrefab;
        public Vector2 patchSize = Vector2.one;
        public float sizingOffset = 0;
        [Header("Mesh asset creation")]
        //TODO : Добавить BladeAnchorPos в структуру вывода. https://youtu.be/6SFTcDNqwaA?si=5zmE0-Pwrpj9C-pv&t=979
        public ComputeShader grassCompute;        
        public Vector2Int grassAmount = Vector2Int.one * 5;
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
        int vertsAmount;
        [SerializeField]
        int indicesAmount;
        [SerializeField]
        protected ComputeShader _computeInstance;
        [SerializeField]
        protected Material _grassShaderInstance;

        private Renderer highPolyGrass;
        private Mesh highPolyMesh;

        protected Matrix4x4[] _grassPositions;

        protected const int VECTOR_STRIDE = sizeof(float) * 3; //3 раза по размеру float - это вектор трёх измерений.
        protected const int INT_FLOAT_STRIDE = sizeof(float);
        protected const int VERTEX_STRIDE = VECTOR_STRIDE * 3 + INT_FLOAT_STRIDE * 2;

        //TODO : Не-а, проблема не в буфферах. А в Mesh'е. У него максимум - 65534 точек. Это и есть ограничение, с ним и можно работать.
        protected const int MAX_VERTS_BUFFER_LENGTH = 4096;

        protected Logger _logger;

        #endregion

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        protected struct GeneratedVertex
        {
            public Vector3 positionOS;
            public Vector3 normalOS;
            public Vector3 anchorOS;
            public Vector2 uv;
        }

        public virtual void OnValidate()
        {
            bladesAmount = grassAmount.x * grassAmount.y;
            vertsAmount = bladesAmount * (segmentCount * 2 + 1);
            indicesAmount = (bladesAmount * (segmentCount * 2 - 1) * 3);

            OnDisable();
            OnEnable();
        }

        public virtual void OnEnable()
        {
            _computeInstance = Instantiate(grassCompute);
            _grassShaderInstance = Instantiate(grassMaterial);
            //TODO : JournalLogger сюда

            if (_computeInstance)
            {
                highPolyGrass = grassPrefab.GetComponent<LODGroup>().GetLODs()[0].renderers[0];
                highPolyMesh = highPolyGrass.GetComponent<MeshFilter>().sharedMesh; ;

                Bounds renderBounds = highPolyGrass.bounds;
                Vector2 size = new Vector2(renderBounds.size.x - sizingOffset, renderBounds.size.z - sizingOffset);

                SetupConstraintsAndBuffers();
                DisplaceGrass(size);
            }
            else
                OnDisable();
        }

        public virtual void OnDisable()
        {
            vertsBuffer?.Release();
            indicesBuffer?.Release();
            DestroyImmediate(_computeInstance);
            DestroyImmediate(_grassShaderInstance);
        }

        private void LateUpdate()
        {
            if (highPolyMesh == null || _grassShaderInstance == null)
                return;

            RenderParams rParams = new RenderParams(_grassShaderInstance);
            
            //matProps.SetInteger("_InstanceIDOffset", i + j * patches.x);
            //TODO : Manual LOD, сделать несколько RenderMesh'ей в зависимости от дальности до камеры, взять из _grassPositions.getColumn(3) (Это позиция)
            Graphics.RenderMeshInstanced(rParams, highPolyMesh, 0, _grassPositions);
        }

        protected virtual void SetupConstraintsAndBuffers()
        {
            vertsBuffer = new GraphicsBuffer(Target.Append, (int)vertsAmount, VERTEX_STRIDE);
            indicesBuffer = new GraphicsBuffer(Target.Append, indicesAmount, INT_FLOAT_STRIDE);

            Vector3 grassSize = new Vector3(patchSize.x,grassHeight/2,patchSize.y);

            kernelId = _computeInstance.FindKernel("CSMain");

            _computeInstance.SetBuffer(kernelId, "_resVerts", vertsBuffer);
            _computeInstance.SetBuffer(kernelId, "_resIndices", indicesBuffer);

            _computeInstance.SetFloat("_width", grassWidth);
            _computeInstance.SetFloat("_height", grassHeight);
            _computeInstance.SetFloat("_curvature", grassCurvature);
            _computeInstance.SetFloat("_maxBendAngleRad", maxBendAngle);
            _computeInstance.SetInt("_segmentCount", segmentCount);
        }

        protected virtual void DisplaceGrass(Vector2 grassBunchSize)
        {            
            Vector2Int patches = new Vector2Int( //Floor, Чтобы трава была как можно более плотной
                Mathf.FloorToInt(patchSize.x / grassBunchSize.x),
                Mathf.FloorToInt(patchSize.y / grassBunchSize.y));

            patches = new Vector2Int(Mathf.Max(patches.x, 1), Mathf.Max(patches.y,1));

            _grassPositions = new Matrix4x4[patches.x * patches.y];

            for(int i = 0; i < patches.x; i++)
            {
                for(int j = 0; j < patches.y; j++) 
                {
                    //TODO TIDY : Тут многое можно вынести за пределы цикла
                    Vector4 offsetPos = new Vector4(i * grassBunchSize.x, 0, j * grassBunchSize.y, 0) + transform.localToWorldMatrix.GetColumn(3);
                    Matrix4x4 usedMatrix = new();
                    usedMatrix.SetTRS(offsetPos, transform.rotation, transform.lossyScale);
                    _grassPositions[i + j * patches.x] = usedMatrix;
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Создаёт Asset mesh'а травы, должен быть использован только в Editor
        /// </summary>
        public void SaveMeshFromComputeShader()
        {
            _computeInstance.GetKernelThreadGroupSizes(kernelId, out uint numThreads, out _, out _);

            // TODO TIDY : Убрать всё лишнее отсюда
            int limitedSquareSideSize = Mathf.FloorToInt(Mathf.Sqrt(bladesAmount));
            Vector2Int dispatches = new(Mathf.CeilToInt(grassAmount.x / limitedSquareSideSize),
                                         Mathf.CeilToInt(grassAmount.y / limitedSquareSideSize));

            Vector2 step = new(patchSize.x / (grassAmount.x), patchSize.y / (grassAmount.y));

            Vector2 localPatchSize = new(step.x * limitedSquareSideSize, step.y * limitedSquareSideSize);
            Vector2Int localGrassAmount = new(limitedSquareSideSize, limitedSquareSideSize);

            _computeInstance.SetVector("_patchSize", new Vector2(patchSize.x, patchSize.y));
            Vector2 toPass = new Vector2(grassAmount.x, grassAmount.y);
            _computeInstance.SetVector("_bladesAmount", toPass);
            _computeInstance.SetVector("_patchPos", Vector3.zero);
            _computeInstance.SetInt("_clusterID", 1);

            dispatchSize = Mathf.CeilToInt((float)bladesAmount / numThreads);
            _computeInstance.Dispatch(kernelId, dispatchSize, 1, 1);

            GeneratedVertex[] generatedVertices = new GeneratedVertex[vertsAmount];
            int[] generatedIndices = new int[indicesAmount];

            vertsBuffer.GetData(generatedVertices);
            indicesBuffer.GetData(generatedIndices);

            Vector3[] generatedPoints = new Vector3[vertsAmount];
            Vector3[] normals = new Vector3[vertsAmount];
            Vector3[] anchors = new Vector3[vertsAmount];
            Vector2[] UVs = new Vector2[vertsAmount];

            for (int i = 0; i < vertsAmount; i++)
            {
                var v = generatedVertices[i];
                generatedPoints[i] = v.positionOS;
                normals[i] = v.normalOS;
                anchors[i] = v.anchorOS;
                UVs[i] = v.uv;
            }

            Mesh initialMesh = (Mesh) AssetDatabase.LoadAssetAtPath(meshSavingPath, typeof(Mesh));
            Mesh grassSourceMesh;

            grassSourceMesh =
                initialMesh == null ? new Mesh() : initialMesh;

            //grassSourceMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //Отменяем некоторые Android-платформы, но зато получаем 4 миллиарда точек.
            grassSourceMesh.SetVertices(generatedPoints);
            grassSourceMesh.SetUVs(1, anchors); // Используем второй канал у TEXCOORD, так можно поставить custom-информацию
            grassSourceMesh.SetUVs(0, UVs);
            grassSourceMesh.SetNormals(normals);
            grassSourceMesh.SetIndices(generatedIndices, MeshTopology.Triangles, 0, true);
            grassSourceMesh.Optimize(); 

            const string ASSET_FILETYPE = ".asset";

            if (initialMesh != null)
            {
                initialMesh.Clear();
                EditorUtility.CopySerialized(grassSourceMesh, initialMesh);
            }
            else
                AssetDatabase.CreateAsset(grassSourceMesh, meshSavingPath + assetName + ASSET_FILETYPE);
            
            AssetDatabase.SaveAssets();
        }

#endif
    }
}