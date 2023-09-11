using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static NavMeshCalculations;

public class NavMeshCalculations : MonoBehaviour
{
    private static NavMeshCalculations _instance;

    [Min(0)]
    public float MINIMUM_AREA = 5;
    [Min(0)]
    public float MAXMIMUM_AREA = 20;
    [Range(0, 100)]
    public float MAX_VERTS_IN_COMPLEX = 50;

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
        protected List<Cell> _neighbors = new List<Cell>();
        protected Vector3[] _vectorFormers;

        public void AddNeighbor(Cell neighbor)
        {
            if (neighbor == this)
                return;

            _neighbors.Add(neighbor);
        }

        public void RemoveNeighbor(Cell neighbor)
        {
            if (neighbor == this)
                return;

            _neighbors.Remove(neighbor);
        }
        public void AddNeighbors(List<Cell> neighbors)
        {
            List<Cell> temp = new List<Cell>(neighbors);

            if (neighbors.Contains(this))
                temp.Remove(this);

            _neighbors.AddRange(temp);
        }

        public Vector3 Center() 
        {
            Vector3 sum = Vector3.zero;

            foreach (Vector3 former in _vectorFormers)
                sum += former;

            return sum / _vectorFormers.Length;
        }

        public Vector3 NavMeshCenter()
        {
            Physics.Raycast(Center(), Vector3.down, out var hit, 100);

            return hit.point;
        }

        public Vector3[] Formers()
        {
            return _vectorFormers;
        }

        public virtual void DrawGizmo() 
        {

        }        

        public List<Cell> Neighbors { get => _neighbors; set => _neighbors = value; }
    }

    //Вариант решения: Треугольные Cell'ы, и другие. У других площадь считается как сумма треугольных, и состоят они из треугольных.
    private class TriangleCell : Cell 
    {
        public bool draw = true;
        public TriangleCell(Vector3[] formers)
        {
            _vectorFormers = new Vector3[3];
            _vectorFormers = formers;
        }

        public override void DrawGizmo()
        {
            if(!draw)
                return;

            Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            for(int i = 0; i < 3; i++)
            {
                Gizmos.DrawLine(_vectorFormers[i], _vectorFormers[(i+1)%3]);
            }

            Gizmos.DrawRay(Center(), Vector3.up);
        }

        public void Draw(Color color, float duration = 0) 
        {
            for (int i = 0; i < 3; i++)
            {
                Debug.DrawLine(_vectorFormers[i], _vectorFormers[(i + 1) % 3], color, duration);
            }
        }
    }
    private class ComplexCell : Cell 
    {
        private List<TriangleCell> _trianglesFormers = new();
        public void Consume(TriangleCell cell)
        {
            cell.draw = false;

            if(_trianglesFormers.Count == 0) 
            {
                _vectorFormers = new Vector3[3];

                for (int i = 0; i < 3; i++)
                    _vectorFormers[i] = cell.Formers()[i];
            }
            else 
            {
                //Ищем ту вершину, которая отсутствует в списке формирующих вершин

                Vector3 res = Vector3.zero; // Цель поиска
                int left=-1; // Расположение левой вершины в массиве, после которой и надо ставить новую

                // Цель ставим между левой и правой вершинами
                
                int iteration;
                foreach(Vector3 triangleFormer in cell.Formers()) 
                {
                    iteration = 0;
                    foreach(Vector3 former in _vectorFormers)
                    {                        
                        if (triangleFormer == former)
                        {
                            if (left == -1)
                                left = iteration;
                        }
                        iteration++;
                    }
                    res = triangleFormer;
                }

                if (left == -1)
                {
                    // Иногда треугольник уже может быть целиков включён в ComplexCell
                    // Иногда он даже не является соседом
                    // В любом случае, тогда рассматривать его бессмыслено, а потому
                    cell.Draw(Color.yellow, 2);
                    Draw(Color.red, 2);
                    return;

                    //throw new Exception("Левая вершина ребра не нашлась");
                }

                if (res == Vector3.zero)
                {
                    cell.Draw(Color.red);
                    Draw(Color.yellow);
                    throw new Exception("При поглощении нарисованного треугольника не нашлось внешней вершины (Error Pause, чтобы увидеть)");                    
                }

                int newArrayLen = _vectorFormers.Length + 1;
                Vector3[] newFormers = new Vector3[newArrayLen];

                int offset = 0;
                for(int i = 0; i < _vectorFormers.Length; i++) 
                {
                    newFormers[i + offset] = _vectorFormers[i];
                    if (i == left) 
                    {
                        newFormers[i+1] = res;
                        offset = 1;
                    }                    
                }

                _vectorFormers = newFormers;
            }

            _trianglesFormers.Add(cell);
        }

        public override void DrawGizmo()
        {
            Gizmos.color = Mathf.CorrelatedColorTemperatureToRGB(CellCount() * 1000);
            Vector3 prev = Vector3.zero;
            foreach (var former in _vectorFormers)
            {
                if (prev == Vector3.zero) { prev = former; continue; }

                Gizmos.DrawLine(prev, former);
                prev = former;
            }

            Gizmos.DrawLine(_vectorFormers[_vectorFormers.Length - 1], _vectorFormers[0]);

            Gizmos.DrawRay(Center(), Vector3.up);
        }

        public void Draw(Color color, float duration = 0)
        {
            Vector3 prev = Vector3.zero;
            foreach (var former in _vectorFormers)
            {
                if (prev == Vector3.zero) { prev = former; continue; }

                Debug.DrawLine(prev, former, color, duration);
                prev = former;
            }
        }
        public float GetArea() 
        {
            float res = 0;
            foreach (TriangleCell triangle in _trianglesFormers)
                res += TriangleArea(triangle.Formers());
            return res;
        }
    }

    private struct Edge
    {
        Vector3 former1;
        Vector3 former2;

        public Edge(Vector3 former1, Vector3 former2)
        {
            this.former1 = former1;
            this.former2 = former2;
        }

        public void Draw(Color color, float duration = 0) 
        {
            Debug.DrawLine(former2, former1, color, duration);
        }

        
        public override bool Equals(object obj)
        {
            if (!(obj is Edge)) return false;

            Edge other = (Edge)obj;

            return other.former1 == former1 && other.former2 == former2
            || other.former1 == former2 && other.former2 == former1;
        }

        public override int GetHashCode()
        {
            return former1.GetHashCode() ^ former2.GetHashCode();
        }
    }

    private Cell[] _cells;

    private void OnValidate()
    {
        if(_instance)
            _instance.Initialize();
        MAXMIMUM_AREA = Mathf.Clamp(MAXMIMUM_AREA, MINIMUM_AREA, int.MaxValue);
    }

    public void Initialize()
    {
        //TODO? : Большой простор для оптимизации, хотя поскольку тут у нас инициализация - то особенно без разницы.

        List<Cell> _cellsList = new();
        List<Cell> trianglesToCombine = new();
        Dictionary<Edge, List<Cell>> links = new(); //Каждому ребру соответствуют какие-то Cell'ы

        List<Edge> edges = new();

        var triangulation = NavMesh.CalculateTriangulation();

        //Формирование треугольников
        for (int i = 0; i < triangulation.indices.Length; i += 3)
        {
            Vector3[] triangle = new Vector3[3];

            triangle[0] = triangulation.vertices[triangulation.indices[i]];
            triangle[1] = triangulation.vertices[triangulation.indices[i + 1]];
            triangle[2] = triangulation.vertices[triangulation.indices[i + 2]];

            Edge[] triEdges = new Edge[3];
            for (int j = 0; j < 3; j++)
            {
                triEdges[j] = new Edge(triangle[j], triangle[(j+1)%3]);
                //triEdges[j].Draw(new Color(1, 1, 1, 0.3f), 20);
                if (!links.ContainsKey(triEdges[j]))
                    links.Add(triEdges[j], new List<Cell>());
            }

            Cell cell = new TriangleCell(triangle);

            for (int j = 0; j < 3; j++)
                links[triEdges[j]].Add(cell);

            if (TriangleArea(triangle) < MINIMUM_AREA)
            {
                trianglesToCombine.Add(cell);
                continue;
            }

            _cellsList.Add(cell);            
        }        
        //Соединение соседей
        foreach (KeyValuePair<Edge, List<Cell>> kvp in links)
        {
            foreach (Cell c in kvp.Value)
            {
                List<Cell> neighbors = kvp.Value;
                c.AddNeighbors(neighbors);
            }
        }
        //Редуцирование треугольников
        while(trianglesToCombine.Count > 0)
        {
            //Есть вот какой-то начальный треугольник. Берём его за основу.
            // Создаем на его основе ComplexCell, который дальше начинает расширяться за счёт соседей.
            // Как только соседи кончились, и расширять некуда - новая итерация с новым ComplexCell
            ComplexCell consumer = new ComplexCell();
            LinkedList<TriangleCell> toConsume = new();
            _cellsList.Add(consumer);

            TriangleCell first = (TriangleCell)trianglesToCombine[0];

            foreach (Cell neighbor in first.Neighbors)
            {
                neighbor.RemoveNeighbor(first);
                neighbor.AddNeighbor(consumer);
                if (neighbor is TriangleCell && trianglesToCombine.Contains(neighbor) && !toConsume.Contains((TriangleCell)neighbor))
                    toConsume.AddFirst((TriangleCell)neighbor);
            }

            consumer.Consume(first);
            trianglesToCombine.Remove(first);            

            while(toConsume.Count >0) 
            {
                if (consumer.GetArea() > MINIMUM_AREA)
                    break;

                if(consumer.Formers().Length > MAX_VERTS_IN_COMPLEX)                
                    break;                

                TriangleCell cell = toConsume.First.Value;
                toConsume.RemoveFirst();

                foreach (Cell neighbor in cell.Neighbors)
                {
                    neighbor.RemoveNeighbor(cell);
                    neighbor.AddNeighbor(consumer);
                    if (neighbor is TriangleCell && trianglesToCombine.Contains(neighbor) && !toConsume.Contains((TriangleCell) neighbor))
                        toConsume.AddFirst((TriangleCell)neighbor);
                }

                consumer.Consume(cell);
                trianglesToCombine.Remove(cell);                
            }
        }

        _cells = _cellsList.ToArray();
    }

    public Cell GetCell(int index)
    {
        return _cells[index];
    }

    public Cell GetCell(Vector3 pointNear)
    {
        //TODO: Это просто необходимо оптимизировать, например через систему чанков.
        Cell res = _cells[0];
        float bestDistance = 100000;
        foreach (Cell cell in _cells)
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

    public void DrawCells()
    {
        foreach (Cell cell in _cells)
        {
            cell.DrawGizmo();
        }
    }

    public static int CellCount() => _instance._cells.Length;

    private static float TriangleArea(Vector3[] triangle)
    {
        Vector3 line1 = triangle[0] - triangle[1];
        Vector3 line2 = triangle[0] - triangle[2];

        return (Vector3.Cross(line2, line1).magnitude) / 2;
    }

    private void OnDrawGizmos()
    {
        DrawCells();
    }
}
