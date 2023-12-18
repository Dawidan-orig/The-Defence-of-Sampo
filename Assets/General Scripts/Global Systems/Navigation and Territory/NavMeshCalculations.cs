using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshCalculations : MonoBehaviour
{
    //TODO : Сделать вычисление ещё в Editor через корутины, как это было в рогалике про культистов!

    private static NavMeshCalculations _instance;

    [Min(0)]
    public float MINIMUM_AREA = 30;
    [Min(0)]
    public float MAXMIMUM_AREA = 50;
    [Min(0)]
    public int CLUSTER_SIZE = 30;
    [Range(0, 100)]
    public float MAX_VERTS_IN_COMPLEX = 50;
    public Bounds octTreeBounds = new Bounds();

    [Header("Lookonly")]
    [SerializeReference]
    OctTree octreeCells;
    [SerializeField]
    private List<Cluster> _clusters = new();
    [SerializeField]
    private Cell[] _cells;
    [SerializeField]
    private GameObject _cellTransformContainer;

    [Header("Debug")]
    public bool drawOctTree = false;

    public static NavMeshCalculations Instance
    {
        get
        {    
            if(_instance == null)
                _instance = FindObjectOfType<NavMeshCalculations>();

            if (_instance == null)
            {                
                GameObject go = new("NM Calculations");
                _instance = go.AddComponent<NavMeshCalculations>();
                _instance.Initialize();
            }

            /*
            if (EditorApplication.isPlaying)
            {
                _instance.transform.parent = null;
                DontDestroyOnLoad(_instance.gameObject);
            }*/

            return _instance;
        }
    }
    [Serializable]
    public abstract class Cell
    {
        protected List<Cell> _neighbors = new List<Cell>();
        protected Vector3[] _vectorFormers;

        [SerializeField]
        protected Vector3 center;

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
            return center;
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

        public abstract void DrawGizmo();

        public List<Cell> Neighbors { get => _neighbors; }
    }
    [Serializable]
    private class TriangleCell : Cell
    {
        public bool draw = true;
        public TriangleCell(Vector3[] formers)
        {
            _vectorFormers = new Vector3[3];
            _vectorFormers = formers;

            Vector3 sum = Vector3.zero;

            foreach (Vector3 former in _vectorFormers)
                sum += former;

            center = sum / _vectorFormers.Length;
        }

        public override void DrawGizmo()
        {
            if (!draw)
                return;

            Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            for (int i = 0; i < 3; i++)
            {
                Gizmos.DrawLine(_vectorFormers[i], _vectorFormers[(i + 1) % 3]);
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
    [Serializable]
    private class ComplexCell : Cell
    {
        private List<TriangleCell> _trianglesFormers = new();
        public void Consume(TriangleCell cell)
        {
            cell.draw = false;

            RemoveNeighbor(cell);

            if (_trianglesFormers.Count == 0) // Треугольник поглощаем целиком
            {
                _vectorFormers = new Vector3[3];

                for (int i = 0; i < 3; i++)
                    _vectorFormers[i] = cell.Formers()[i];
            }
            else
            {
                //Ищем ту вершину, которая отсутствует в списке формирующих вершин

                Vector3 res = Vector3.zero; // Цель поиска
                int first = -1; // Расположение левой вершины в массиве, после которой находится новая
                int second = -1;

                #region assignes
                for (int iteration = 0; iteration < _vectorFormers.Length; iteration++)
                {
                    foreach (Vector3 triangleFormer in cell.Formers())
                    {
                        if (triangleFormer == _vectorFormers[iteration]) //_vectorFormers.Contains(triangleFormer):
                        {
                            if (iteration == 0) // Если это самая первая итерация, то возможно, что результат - последняя вершина, а не следующая
                                break;

                            first = iteration; // первая найденная в фигуре
                            second = (iteration + 1) % _vectorFormers.Length; // Вторая найденная в фигуре

                            // Теперь надо из треугольника извлечь обе эти, и оставшаяся будет res
                            foreach (Vector3 resCheck in cell.Formers())
                                if (!(resCheck == _vectorFormers[first] || resCheck == _vectorFormers[second]))
                                {
                                    res = resCheck;
                                    break;
                                }

                            break;
                        }
                    }

                    if (first != -1) //Нашли всё, выходим.
                        break;
                }

                #endregion

                #region checks

                if (first == -1 && second == -1)
                {
                    // Не нашлось. Скорее всего, Это попытка добавить не-соседа.
                    cell.Draw(Color.yellow, 2);
                    Draw(Color.yellow, 2);
                    return;
                }
                if (res == Vector3.zero)
                {
                    Debug.Log(first + " " + second);

                    int i = 0;
                    if (EditorApplication.isPlaying)
                        foreach (Vector3 vector in _vectorFormers)
                            Utilities.CreateFlowText(i++.ToString(), 1, vector);

                    cell.Draw(Color.red, 100);
                    Draw(Color.red, 100);
                    Debug.LogError("При поглощении нарисованного треугольника не нашлось внешней вершины (Error Pause, чтобы увидеть)");
                    return;
                }
                if (_vectorFormers.Contains(res)) //TODO?? : LINQ-Check, дорого!
                {
                    // Добавляемый треугольник полностью соответствует тому, что уже есть в фигуре

                    //int i = 0;
                    //foreach (Vector3 vector in _vectorFormers)
                    //    Utilities.CreateFlowText(i++.ToString(), 1, res + Vector3.up *_vectorFormers.Length - Vector3.down * 0.3f, Color.red);
                    return;
                }

                #endregion

                #region arrayUpdate

                int newArrayLen = _vectorFormers.Length + 1;
                Vector3[] newFormers = new Vector3[newArrayLen];

                int offset = 0;
                for (int i = 0; i < _vectorFormers.Length; i++)
                {
                    newFormers[i + offset] = _vectorFormers[i];
                    if (i == first)
                    {
                        newFormers[first + 1] = res;
                        offset = 1;
                    }
                }

                #endregion

                _vectorFormers = newFormers;
            }

            _trianglesFormers.Add(cell);

            Vector3 sum = Vector3.zero;

            foreach (Vector3 former in _vectorFormers)
                sum += former;

            center = sum / _vectorFormers.Length;
        }

        public override void DrawGizmo()
        {
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
            Draw(Vector3.zero, color, duration);
        }
        public void Draw(Vector3 offset, Color color, float duration)
        {
            int i = 0;
            Vector3 prev = Vector3.zero;
            foreach (var former in _vectorFormers)
            {
                if (EditorApplication.isPlaying)
                    Utilities.CreateFlowText(i++.ToString(), 1, former + offset, color);

                if (prev == Vector3.zero) { prev = former; continue; }

                Debug.DrawLine(offset + prev, offset + former, color, duration);
                prev = former;
            }
            Debug.DrawLine(offset + prev, offset + _vectorFormers[0], color, duration);
        }
        public float GetArea()
        {
            float res = 0;
            foreach (TriangleCell triangle in _trianglesFormers)
                res += TriangleArea(triangle.Formers());
            return res;
        }
    }
    [Serializable]
    private class Cluster
    {
        private List<Cell> _included = new();
        public Vector3 center { get; private set; } = Vector3.zero;

        public void AddCell(Cell cell)
        {
            _included.Add(cell);

            Vector3 v = Vector3.zero;
            foreach (Cell c in _included)
            {
                v += c.Center();
            }
            center = v / _included.Count;
        }

        public void Clear() => _included.Clear();

        /// <summary>
        /// Находит ближайшую к данной точке клетку через алгоритм дийсктры.
        /// Находит её только в пределах одного графа, из-за чего надо выбирать точки правильно
        /// </summary>
        /// <param name="pointNear"></param>
        /// <returns></returns>
        public Cell FindByAllCheck(Vector3 pointNear)
        {
            //Всегда даёт точный результат, но дорого!
            Debug.LogWarning("Избегайте использование алгоритма поиска ближайшей вершины через прямой перебор");

            Cell res = _included[0];
            float bestDistance = 999999;
            foreach (Cell cell in _included)
            {
                float dist = Vector3.Distance(cell.Center(), pointNear);
                if (dist < bestDistance)
                {
                    res = cell;
                    bestDistance = dist;
                }
            }

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

    private void OnValidate()
    {
        MAXMIMUM_AREA = Mathf.Clamp(MAXMIMUM_AREA, MINIMUM_AREA, int.MaxValue);
    }

    private void OnEnable()
    {
        Initialize();
    }

    /*private void LateUpdate()
    {
        octreeCells.LateUpdate();
    }*/

    public void Initialize()
    {
        //TODO? : Большой простор для оптимизации, хотя поскольку тут у нас инициализация - то особенно без разницы.

        _clusters = new();
        List<Cell> cellsList = new();
        List<Cell> trianglesToCombine = new();
        Dictionary<Edge, List<Cell>> links = new(); //Каждому ребру соответствуют какие-то Cell'ы

        var triangulation = NavMesh.CalculateTriangulation();

        #region form triangles
        for (int i = 0; i < triangulation.indices.Length; i += 3)
        {
            Vector3[] triangle = new Vector3[3];

            triangle[0] = triangulation.vertices[triangulation.indices[i]];
            triangle[1] = triangulation.vertices[triangulation.indices[i + 1]];
            triangle[2] = triangulation.vertices[triangulation.indices[i + 2]];

            Edge[] triEdges = new Edge[3];
            for (int j = 0; j < 3; j++)
            {
                triEdges[j] = new Edge(triangle[j], triangle[(j + 1) % 3]);
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

            cellsList.Add(cell);
        }
        #endregion

        #region connect neighbors
        foreach (KeyValuePair<Edge, List<Cell>> kvp in links)
        {
            foreach (Cell c in kvp.Value)
            {
                List<Cell> neighbors = kvp.Value;
                c.AddNeighbors(neighbors);
            }
        }
        #endregion

        #region unify small triangles
        while (trianglesToCombine.Count > 0)
        {
            //Есть вот какой-то начальный треугольник. Берём его за основу.
            // Создаем на его основе ComplexCell, который дальше начинает расширяться за счёт соседей.
            // Как только соседи кончились, и расширять некуда - новая итерация с новым ComplexCell
            ComplexCell consumer = new ComplexCell();
            LinkedList<TriangleCell> toConsume = new();
            cellsList.Add(consumer);

            TriangleCell first = (TriangleCell)trianglesToCombine[0];

            foreach (Cell neighbor in first.Neighbors)
            {
                neighbor.RemoveNeighbor(first);
                neighbor.AddNeighbor(consumer);
                consumer.AddNeighbor(neighbor);

                if (neighbor is TriangleCell && trianglesToCombine.Contains(neighbor) && !toConsume.Contains((TriangleCell)neighbor))
                    toConsume.AddFirst((TriangleCell)neighbor);
            }

            consumer.Consume(first);
            trianglesToCombine.Remove(first);

            while (toConsume.Count > 0)
            {
                if (consumer.GetArea() > MINIMUM_AREA)
                    break;

                if (consumer.Formers().Length > MAX_VERTS_IN_COMPLEX)
                    break;

                TriangleCell cell = toConsume.First.Value;
                toConsume.RemoveFirst();

                foreach (Cell neighbor in cell.Neighbors)
                {
                    neighbor.RemoveNeighbor(cell);
                    neighbor.AddNeighbor(consumer);
                    consumer.AddNeighbor(cell);

                    if (neighbor is TriangleCell && trianglesToCombine.Contains(neighbor) && !toConsume.Contains((TriangleCell)neighbor))
                        toConsume.AddFirst((TriangleCell)neighbor);
                }

                consumer.Consume(cell);
                trianglesToCombine.Remove(cell);
            }
        }
        #endregion

        #region subdivide big triangles

        #endregion

        #region create clusters
        //TODO : Лучше сделать много маленьких terrain'ов. А ещё лучше - плоскостей, заменяющих Terrain'ы
        int amount = 0;
        Cluster used = new();
        foreach (Cell c in cellsList)
        {
            used.AddCell(c);

            if (amount++ > CLUSTER_SIZE)
            {
                amount = 0;
                _clusters.Add(used);
                used = new();
            }
        }
        #endregion

        if (EditorApplication.isPlaying)
            Destroy(_cellTransformContainer);
        else
            DestroyImmediate(_cellTransformContainer);
        _cellTransformContainer = new("Cells positions");
        _cellTransformContainer.transform.parent = transform;

        List<Transform> transforms = new List<Transform>();
        foreach (Cell cell in cellsList)
        {
            GameObject cellGo = new(cell.Center().ToString());
            var comp = cellGo.AddComponent<TransfromCellBehavior>();
            comp.Aligned = cell;
            cellGo.transform.position = cell.Center();
            cellGo.transform.parent = _cellTransformContainer.transform;
            transforms.Add(cellGo.transform);
        }

        octreeCells = new(octTreeBounds);
        octreeCells.AddRangeToProcess(transforms);

        _cells = cellsList.ToArray();
    }

    public Cell GetCell(Vector3 pointNear)
    {
        // Его TODO : Можно посчитать заранее, при инициализации
        // TODO : Json-файл для хранения ShootMesh'а, чтобы сохранить всё ещё в Editor'е.

        return FindByOctTree(pointNear);
    }

    public Cell FindByOctTree(Vector3 pointNear)
    {
        var t = octreeCells.FindClosestObjectInTree(pointNear);
        return t.GetComponent<TransfromCellBehavior>().Aligned;
    }

    /// <summary>
    /// Использует кластеры, которые используются для разбиения всего NavMesh на меньшие участки
    /// </summary>
    /// <returns></returns>
    public Cell FindByClusters(Vector3 pointNear)
    {
        Cluster res = null;
        float bestDistance = 999999;
        foreach (Cluster cluster in _clusters)
        {
            float dist = Vector3.Distance(cluster.center, pointNear);
            if (dist < bestDistance)
            {
                res = cluster;
                bestDistance = dist;
            }
        }

        return res.FindByAllCheck(pointNear);
    }

    /// <summary>
    /// Находит ближайшую к данной точке клетку через алгоритм дийсктры.
    /// Находит её только в пределах одного графа, из-за чего надо выбирать точки правильно
    /// </summary>
    /// <param name="pointNear"></param>
    /// <returns></returns>
    public Cell FindByAllCheck(Vector3 pointNear)
    {
        //Всегда даёт точный результат, но дорого!
        Debug.LogWarning("Избегайте использование алгоритма поиска ближайшей вершины через прямой перебор");

        Cell res = _cells[0];
        float bestDistance = 999999;
        foreach (Cell cell in _cells)
        {
            float dist = Vector3.Distance(cell.Center(), pointNear);
            if (dist < bestDistance)
            {
                res = cell;
                bestDistance = dist;
            }
        }

        return res;
    }

    public Cell DijkstraFindCell(Cell start, Vector3 pointNear)
    {
        float bestDistance = 100000;
        List<Cell> toCheck = new()
        {
            start
        };
        Cell res = null;

        string debug_log = "Called a GetCell().\n";
        int iteraitions = 0;
        int depth = 0;

        while (toCheck.Count > 0)
        {
            iteraitions++;
            Cell current = toCheck[0];
            toCheck.RemoveAt(0);
            float dist = Vector3.Distance(current.Center(), pointNear);
            foreach (Cell neighbor in current.Neighbors)
            {
                float neighborDist = Vector3.Distance(neighbor.Center(), pointNear);
                if (neighborDist < dist)
                    toCheck.Add(neighbor);
            }

            if (dist < bestDistance)
            {
                depth++;
                bestDistance = dist;
                res = current;
            }
        }

        debug_log += $"It has {iteraitions} iterations.\n";
        debug_log += $"It has proceeded further {depth} times.";

        Debug.Log(debug_log);

        return res;
    }

    /// <summary>
    /// Находит ближайшую к данной точке клетку через NavMesh.CalculatePath()
    /// </summary>
    /// <param name="pointNear"></param>
    /// <returns></returns>

    public void DrawCells()
    {
        if (_cells == null)
            return;

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

    private void OnDrawGizmosSelected()
    {
        DrawCells();

        Gizmos.color = Color.cyan;

        foreach (Cluster cluster in _clusters)
        {
            Gizmos.DrawRay(cluster.center, Vector3.up * 100);
        }

        Gizmos.color = new Color(0, 0.6f, 0);
        if (octreeCells != null && drawOctTree)
            octreeCells.DrawGizmo();
    }
}