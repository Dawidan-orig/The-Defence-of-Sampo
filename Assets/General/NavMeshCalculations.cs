using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static NavMeshCalculations;

public class NavMeshCalculations : MonoBehaviour
{
    private static NavMeshCalculations _instance;

    public const float MINIMUM_AREA = 5;
    public static bool DRAW = true;

    public static NavMeshCalculations Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = new GameObject().AddComponent<NavMeshCalculations>();
                // name it for easy recognition
                _instance.name = _instance.GetType().ToString();
                // mark root as DontDestroyOnLoad();
                DontDestroyOnLoad(_instance.gameObject);

                _instance.Initialize();
            }
            return _instance;
        }
    }

    public class Cell // Как если бы мы делали A* на сетке, используя структуры.
    {
        private List<Cell> _neighbors = new List<Cell>();
        private Vector3[] _formers = new Vector3[3];

        public Cell(Vector3[] triangleVertices)
        {
            _formers = triangleVertices;
        }

        public void AddNeighbor(Cell neighbor)
        {
            if (neighbor == this)
                return;

            _neighbors.Add(neighbor);
        }
        public void AddNeighbors(List<Cell> neighbors)
        {
            List<Cell> temp = new List<Cell>(neighbors);

            if(neighbors.Contains(this))
                temp.Remove(this);

            _neighbors.AddRange(temp);
        }

        public Vector3 Center()
        {
            return (_formers[0] + _formers[1] + _formers[2]) / 3;
        }

        public Vector3 NavMeshCenter() 
        {
            Physics.Raycast(Center(), Vector3.down, out var hit, 100);

            return hit.point;
        }

        public Vector3[] Formers() 
        {
            return _formers;
        }

        public void Draw(float duration = 0) 
        {
            Color color = new Color(0.3f, 0.3f, 1, 0.3f);

            Debug.DrawLine(_formers[0], _formers[1], color, duration);
            Debug.DrawLine(_formers[1], _formers[2], color, duration);
            Debug.DrawLine(_formers[2], _formers[0], color, duration);

            Debug.DrawRay(Center(), Vector3.up, color, duration);
        }

        public void DrawNeighbors(float duration = 0) 
        {
            foreach (Cell neighbor in _neighbors)
            {
                Debug.DrawLine(Center(), neighbor.Center(), new Color(0.5f, 0.5f, 0.5f, 0.6f), duration);
            }
        }

        public List<Cell> Neighbors { get => _neighbors; set => _neighbors = value; }
    }
    private static Cell[] _cells;

    public void Initialize()
    {
        //TODO : Разбиение больших треугольников на маленькие, и работа уже с ними.
        //TODO : Большой простор для оптимизации, хотя поскольку тут у нас инициализация - то особенно без разницы.

        List<Cell> _cellsList = new List<Cell>();
        Dictionary<Vector3, List<Cell>> links = new Dictionary<Vector3, List<Cell>>();

        var triangulation = NavMesh.CalculateTriangulation();
        for (int i = 0; i < triangulation.vertices.Length; i++)
        {
            if (links.ContainsKey(triangulation.vertices[i]))
                continue;

            links.Add(triangulation.vertices[i], new List<Cell>());
        }

        for (int i = 0; i < triangulation.indices.Length; i+=3)
        {
            Vector3[] triangle = new Vector3[3];

            triangle[0] = triangulation.vertices[triangulation.indices[i]];
            triangle[1] = triangulation.vertices[triangulation.indices[i+1]];
            triangle[2] = triangulation.vertices[triangulation.indices[i+2]];

            //TODO : Объединение этих мелких в большие, а не их игнор.
            if (TriangleArea(triangle) < MINIMUM_AREA)
                continue;

            Cell cell = new Cell(triangle);
            cell.Draw(10);

            _cellsList.Add(cell);
            for (int j = 0; j < 3; j++)
                links[triangle[j]].Add(cell);
        }

        _cells = _cellsList.ToArray();

        foreach(KeyValuePair<Vector3, List<Cell>> kvp in links) 
        {
            foreach(Cell c in kvp.Value) 
            {
                List<Cell> neighbors = kvp.Value;
                c.AddNeighbors(neighbors);
            }
        }

        if (DRAW)
            DrawCells(1000);
    }

    public Cell GetCell(int index) 
    {
        return _cells[index];
    }

    public Cell GetCell(Vector3 pointNear) 
    {
        Cell res = _cells[0];
        float bestDistance = 100000;
        foreach(Cell cell in _cells) 
        {
            float distance = Vector3.Distance(cell.Center(), pointNear);
            if (distance < bestDistance) 
            {
                bestDistance = distance;
                res = cell;
            }
        }

        return res;
    }

    public void DrawCells(float duration = 0)
    {
        foreach (Cell cell in _cells)
        {
            cell.Draw(duration);
        }
    }

    public static int CellCount() => _cells.Length;

    private static float TriangleArea(Vector3[] triangle) 
    {
        Vector3 line1 = triangle[0] - triangle[1];
        Vector3 line2 = triangle[0] - triangle[2];

        return (Vector3.Cross(line2, line1).magnitude) / 2;
    }
}
