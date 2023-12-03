using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static NavMeshCalculations;

public class NavMeshCalculations : MonoBehaviour
{
    //TODO : Сделать вычисление ещё в Editor через корутины, как это было в рогалике про культистов!

    private static NavMeshCalculations _instance;

    [Min(0)]
    public float MINIMUM_AREA = 30;
    [Min(0)]
    public float MAXMIMUM_AREA = 50;
    [Range(0, 100)]
    public float MAX_VERTS_IN_COMPLEX = 50;

    [SerializeField]
    private Cell center;

    public static NavMeshCalculations Instance
    {
        get
        {
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
    public class Cell
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
            Draw(Vector3.zero, color, duration);
        }
        public void Draw(Vector3 offset, Color color, float duration)
        {
            int i = 0;
            Vector3 prev = Vector3.zero;
            foreach (var former in _vectorFormers)
            {
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
        if (_instance)
            _instance.Initialize();
        MAXMIMUM_AREA = Mathf.Clamp(MAXMIMUM_AREA, MINIMUM_AREA, int.MaxValue);
    }

    private void OnEnable()
    {
        Initialize();
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
        // Объединение слишком маленьких треугольников
        while (trianglesToCombine.Count > 0)
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
        // Разбиение слишком больших треугольников

        //TODO : Подсчёт "Центра"
        center = _cellsList[0];

        _cells = _cellsList.ToArray();
    }

    public Cell GetCell(Vector3 pointNear)
    {
        // Его TODO : Можно посчитать заранее, при инициализации
        // TODO : Json-файл для хранения ShootMesh'а, чтобы сохранить всё ещё в Editor'е.

        return FindByAllCheck(pointNear);
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
        foreach(Cell cell in _cells) 
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

    public Cell DijkstraFindCell(Cell start, Vector3 pointNear ) 
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
   
/*    public Cell NavMeshPathFindCell(Vector3 start,Vector3 pointNear) 
    {
        NavMeshPath path = new();
        NavMesh.CalculatePath(start, pointNear, ~0, path);

        return path.corners[path.corners.Length - 1];
    }*/

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

    private void OnDrawGizmos()
    {
        DrawCells();
    }


}
