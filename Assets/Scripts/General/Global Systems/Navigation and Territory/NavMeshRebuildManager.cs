using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshRebuildManager : MonoBehaviour
{
    static NavMeshRebuildManager _instance;
    public static NavMeshRebuildManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<NavMeshRebuildManager>();

            return _instance;
        }
    }

    public LayerMask _navMeshRebuild = 32768;
    public LayerMask _ground = 8;

    NavMeshSurface surf;

    private void Awake()
    {
        _instance = Instance;
        surf = GetComponent<NavMeshSurface>();
    }

    public void Rebuild() 
    {
        surf.layerMask = _navMeshRebuild;

        surf.BuildNavMesh();

        for(int i = 0; i < transform.childCount; i++) 
        {
            transform.GetChild(i).gameObject.layer = _ground;
        }
    }
}
