using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

//Referenced from https://www.gamedev.net/tutorials/programming/general-and-gameplay-programming/introduction-to-octrees-r3529/

/// <summary>
/// ������ ������ OctTree, ���������� ��� ����� � ������������.
/// </summary>
[Serializable]
public class OctTree
{
    Queue _pendingInsertion = new Queue();

    bool _treeReady = false; //� ������ ���� ��������� ��������, ������� ��� ���� Insert-����
    bool _treeBuilt = false; //there is no pre-existing tree yet.
    List<Transform> allObjects = new();
    OctTreeJoint _root;

    private class OctTreeJoint
    {
        OctTree treeObject;
        public Bounds _region;
        public List<Transform> _objects { get; private set; }
        // ��� ����� ����� ������������ Transform'�, ����� ������������ �����-��� ����� ������ �������.

        OctTreeJoint[] _childNodes = new OctTreeJoint[8];

        // ������� ��������, ������������ ������� �������.
        byte _activeNodes = 0;

        // ������ ������������ ����.
        const int MIN_SIZE = 20;

        int _maxLifespan = 8; // ����������� ����� ����� ����� � ������. ��� ����������������� ��� ����� ����������� ������ �� 64-�.
        int _curLife = -1; // ������� ������� �������� �� �������� �����?

        OctTreeJoint _parent;

        bool HasChildren
        {
            get => _activeNodes != 0;
        }

        #region constructors
        private OctTreeJoint(Bounds region, List<Transform> objList)
        {
            _region = region;
            _objects = new List<Transform>(objList);
            _curLife = -1;
        }

        public OctTreeJoint(OctTree treeObject)
        {
            this.treeObject = treeObject;
            _objects = new List<Transform>();
            _region = new Bounds(Vector3.zero, Vector3.zero);
            _curLife = -1;
        }

        public OctTreeJoint(OctTree treeObject, Bounds region)
        {
            this.treeObject = treeObject;
            _region = region;
            _objects = new List<Transform>();
            _curLife = -1;
        }
        #endregion

        #region init-s
        // � ��� ��� ��� ����� � ���� ����������� ���.
        // �����, ��� ��� "�������" �������. �� � ����� ���� ���.
        public IEnumerator BuildTree() //complete & tested
        {
            //string log = $"����� � {_objects.Count}";
            treeObject._treeBuilt = false;

            //������ � �����, � ����, �����������.
            if (_objects.Count <= 1)
            {
                //Debug.Log(log + "\n����, ������� � " + _objects.Count);
                yield break;
            }

            Vector3 dimensions = _region.max - _region.min;

            // ���������, ��� ������� ���� ������
            if (dimensions == Vector3.zero)
            {
                _region.min = Vector3.zero;
                _region.max = Vector3.one * 100;
                dimensions = _region.max - _region.min;
            }

            //�������� �� ����������� �����������, ����� �� ��������� �� Stack-overflow'�
            if (dimensions.x <= MIN_SIZE && dimensions.y <= MIN_SIZE && dimensions.z <= MIN_SIZE)
            {
                //Debug.Log(log + "\n��������� ������, ������� � " + _objects.Count);
                yield break;
            }

            Vector3 center = _region.center;

            //��������� �� �������, �������� ���� �������� �������.
            Bounds[] octant = new Bounds[8];
            octant[0] = GetBoundsByMyMax(_region.min, center); // -x -y -z
            octant[1] = GetBoundsByMyMax(new Vector3(center.x, _region.min.y, _region.min.z), new Vector3(_region.max.x, center.y, center.z)); // x -y -z
            octant[2] = GetBoundsByMyMax(new Vector3(center.x, _region.min.y, center.z), new Vector3(_region.max.x, center.y, _region.max.z)); // x -y z
            octant[3] = GetBoundsByMyMax(new Vector3(_region.min.x, _region.min.y, center.z), new Vector3(center.x, center.y, _region.max.z)); // -x -y z
            octant[4] = GetBoundsByMyMax(new Vector3(_region.min.x, center.y, _region.min.z), new Vector3(center.x, _region.max.y, center.z)); // -x y -z
            octant[5] = GetBoundsByMyMax(new Vector3(center.x, center.y, _region.min.z), new Vector3(_region.max.x, _region.max.y, center.z)); // x y -z
            octant[6] = GetBoundsByMyMax(center, _region.max); // x y z
            octant[7] = GetBoundsByMyMax(new Vector3(_region.min.x, center.y, center.z), new Vector3(center.x, _region.max.y, _region.max.z)); // -x y z

            //������ ������� ������� ���� ���� �������, �� ���� ���������.
            List<Transform>[] octList = new List<Transform>[8];

            for (int i = 0; i < 8; i++)
                octList[i] = new List<Transform>();

            //��� ����� ��� �������� ��� ��������, ��� ������ ����� ������.
            List<Transform> delist = new List<Transform>();

            foreach (Transform t in _objects)
            {
                if (t == null)
                {
                    delist.Add(t);
                    continue;
                }

                // ��������� ��� �������
                for (int a = 0; a < 8; a++)
                {
                    if (octant[a].Contains(t.position))
                    {
                        octList[a].Add(t);
                        delist.Add(t);
                        break; // �����������, ������ ��� � ���� ������� �������� ������ ������, � ������ ������ ���� �������.
                    }
                }
            }

            foreach (Transform t in delist)
                _objects.Remove(t);

            //log += $"\n����� � {_objects.Count}";
            //Debug.Log(log);

            //������ �������� � ��� ������, ��� ���� �������
            for (int a = 0; a < 8; a++)
            {
                if (octList[a].Count != 0)
                {
                    _childNodes[a] = CreateNode(octant[a], octList[a]);
                    _activeNodes |= (byte)(1 << a);
                    yield return _childNodes[a].BuildTree();
                }
            }

            treeObject._treeBuilt = true;
            treeObject._treeReady = true;
        }
        public Bounds GetBoundsByMyMax(Vector3 min, Vector3 max)
        {
            Bounds res = new();
            res.min = min;
            res.max = max;
            return res;
        }

        private OctTreeJoint CreateNode(Bounds region, List<Transform> objList) //complete & tested
        {
            if (objList.Count == 0)
                return null;

            OctTreeJoint ret = new OctTreeJoint(region, objList);
            ret._parent = this;
            ret.treeObject = treeObject;
            treeObject.allObjects.AddRange(objList);
            return ret;
        }

        private OctTreeJoint CreateNode(Bounds region, Transform item)
        {
            List<Transform> objList = new List<Transform>(1)
        {
            item
        }; //sacrifice potential CPU time for a smaller memory footprint. ���, �������, �� ������ ��� ����������� �������������.
            OctTreeJoint ret = new OctTreeJoint(region, objList);
            ret._parent = this;
            ret.treeObject = treeObject;
            treeObject.allObjects.Add(item);
            return ret;
        }
        #endregion

        /// <summary>
        /// ������ Unity Update, ����������� ��� ��������������� ����. ������� ������������ � LateUpdate, ����� ����� ����� ������������ ����������� ��-�� ������ ���������.
        /// </summary>
        public void Update()
        {
            if (treeObject._treeBuilt == true && treeObject._treeReady == true) // ���� ������ ��� ���� ����������������
            {
                // ���������� ��� ������ ��� �������, � ������� ��� �������� ������.
                // ���� �� ���������� ������� - �������. ���� �� ������������ - ��������� �����.
                // ��� ��������� �� ������� ������� ������ ������� (� ������ � �� ��������� �� ������ ���) � ���� ����� ���������� �������� � �������.
                if (_objects.Count == 0)
                {
                    if (HasChildren == false)
                    {
                        if (_curLife == -1)
                            _curLife = _maxLifespan;
                        else if (_curLife > 0)
                        {
                            _curLife--;
                        }
                    }
                }
                else
                {
                    if (_curLife != -1)
                    {
                        if (_maxLifespan <= 64)
                            _maxLifespan *= 2;
                        _curLife = -1;
                    }
                }

                List<Transform> movedObjects = new List<Transform>(_objects.Count);

                foreach (Transform gameObj in _objects)
                {
                    //we should figure out if an object actually moved so that we know whether we need to update this node in the tree.
                    if (gameObj.hasChanged)
                    {
                        movedObjects.Add(gameObj);
                    }
                }

                //������� ������������ ������� �� ������
                int listSize = _objects.Count;
                for (int a = 0; a < listSize; a++)
                {
                    if (_objects[a] == null)
                    {
                        if (movedObjects.Contains(_objects[a]))
                            movedObjects.Remove(_objects[a]);
                        _objects.RemoveAt(a--);
                        listSize--;
                    }
                }

                //������� ������ ����� � ������ ������
                for (int flags = _activeNodes, index = 0; flags > 0; flags >>= 1, index++)
                    if ((flags & 1) == 1 && _childNodes[index]._curLife == 0)
                    {
                        if (_childNodes[index]._objects.Count > 0)
                        {
                            //throw new Exception("Tried to delete a used branch!");
                            _childNodes[index]._curLife = -1;
                        }
                        else
                        {
                            _childNodes[index] = null;
                            _activeNodes ^= (byte)(1 << index);       //������� ��� �������-��� �� ��������.
                        }
                    }

                //���������� ��������� ��� ����� ������.
                for (int flags = _activeNodes, index = 0; flags > 0; flags >>= 1, index++)
                {
                    if ((flags & 1) == 1)
                    {
                        if (_childNodes != null && _childNodes[index] != null)
                            _childNodes[index].Update();
                    }
                }

                //���� ������ �������� - ���� ��� ��������� ����������� ���� ���� � ���������
                foreach (Transform movedObj in movedObjects)
                {
                    OctTreeJoint current = this;

                    //������� �������, ��� ������ ����� �� ������ ������� ����������� ������
                    // ��� � ���� ������������ ����� ����, ��� ��� ��� �������� �� ��� ����������� ���������.

                    if (current._parent != null)
                    {
                        current = current._parent;
                    }
                    else
                    {
                        //�������� ������� �� ����� ��������� ������, ��� ��� ������������� ������ ��
                        // ������, ��� ������ ����������� �����, ��� ��� ����� ���� ����� ���������.
                        List<Transform> tmp = new List<Transform>(treeObject.allObjects);
                        treeObject.UnloadContent();
                        treeObject._pendingInsertion.Enqueue(tmp);//add to pending queue

                        return;
                    }


                    //now, remove the object from the current node and insert it into the current containing node.
                    _objects.Remove(movedObj);
                    current.Insert(movedObj);   //this will try to insert the object as deep into the tree as we can go.
                }
            }
            else
            {
                if (treeObject._pendingInsertion.Count > 0)
                {
                    //ProcessPendingItems();
                    Update();
                }
            }
        }

        #region ports
        // � ��� ����� � ��������� ����, ��� ��� ������.
        public Transform FindClosestObject(Vector3 point)
        {
            //Debug.Log($"������ � �����, � ������ �������� {_region} - {_objects.Count} ��������");

            Transform closest = null;

            //������� ���� ��������� ������� � ���� ������
            // ��� ����� ������� � ���, ��� ��� ��������������, ��� �� �� �����.
            float bestDistance = 999999;
            foreach (Transform t in _objects)
            {
                float dist = Vector3.Distance(t.position, point);
                if (dist < bestDistance)
                {
                    closest = t;
                    bestDistance = dist;
                }
            }

            // ���� ���������� � ����� ������� ��������� �����.
            // ���� � ���� ������� ���-�� ���� - ���������� ���������� ������.
            Vector3 direction = (point - _region.center).normalized;
            int directionIndex = 0;
            if (direction.y > 0) // ������� 4 5 6 7
                directionIndex += 4;
            if(direction.x > 0) // ��� ������� ������ �������, ��� ��� ������� ������� �� ����� �����������. 
            {
                if (direction.z > 0)
                    directionIndex += 2;
                else
                    directionIndex += 1;
            }
            else
            {
                if (direction.z > 0)
                    directionIndex += 3;
                else
                    directionIndex += 0;
            }

            //���� � ���� ����� ������? ����� ��������� ����.
            if (_childNodes[directionIndex] != null)
            {
                //Debug.DrawLine(_region.center, _childNodes[directionIndex]._region.center, Color.white, 2);
                Transform otherResult = _childNodes[directionIndex].FindClosestObject(point);
                if (closest == null)
                    return otherResult;

                if (Vector3.Distance(otherResult.position, point) < Vector3.Distance(closest.position, point))
                    closest = otherResult;
            }
            else //� ��������� ����������� ������� ���, ������ ���� ��������� ���� �������� � ������� �������� �������.
            {
                foreach (OctTreeJoint otj in _childNodes)
                {
                    if (otj == null)
                        continue;
                    //Debug.DrawLine(_region.center, otj._region.center, Color.white);
                    Transform otherResult = otj.FindClosestObject(point);
                    if (closest == null)
                    {
                        closest = otherResult;
                        continue;
                    }

                    if (Vector3.Distance(otherResult.position, point) < Vector3.Distance(closest.position, point))
                        closest = otherResult;
                }
            }    

            return closest; // � ���� ������� �������� ����� ������� �����, ���������� � ����.
        }

        /// <summary>
        /// ������ ��� �������, ��� ��� ������� �������� ����� ������ ��� ������ �����������.
        /// </summary>
        /// <param name="item">��, ��� �� � ����� ���������</param>
        public bool Insert(Transform item)
        {
            // ���� �� � ����� - ������ ���������.
            if (_objects.Count == 0 && _activeNodes == 0)
            {
                _objects.Add(item);
                return true;
            }

            //��������� �����������.
            //���� ������ �� � ����� ������������ ������� - �������, �� ����� ������ ���� ��� �� �������.
            Vector3 dimensions = _region.max - _region.min;
            if (dimensions.x <= MIN_SIZE && dimensions.y <= MIN_SIZE && dimensions.z <= MIN_SIZE)
            {
                _objects.Add(item);
                return true;
            }

            // ��� ��� ��� ��� �������� ������� �������, ��� ������� �� ����� ����������� � ��������������� ����.

            // � ��� ��� ������� ��� ��� ������� ��������� ������� �� ��������� - � �������.

            // ��������� � ����� ������� ����������� ������������ �������� (� ���� - ����� � ������������), �� ��� ��� �� �� �����.

            //"���� ������ ����� �� ���������, ���� ���������� ��. � ����� ������ ������ ���� ��������� ������������."
            return false;
        }
        #endregion

        public void DrawGizmo()
        {
            Gizmos.DrawWireCube(_region.center, _region.size);
            foreach (OctTreeJoint j in _childNodes)
            {
                if (j != null)
                {
                    j.DrawGizmo();
                }
            }
        }
    }

    #region constructors

    public OctTree()
    {
        _root = new OctTreeJoint(this);
    }

    /// <summary>
    /// ������ ������ � ��������� ��������.
    /// ���� ���-�� � ���� ������ �� ���������� - ������ ������������� �������� ������.
    /// </summary>
    /// <param name="region">������, � ������� ������ ���������� ������������</param>
    public OctTree(Bounds region)
    {
        _root = new OctTreeJoint(this, region);
    }
    #endregion

    public void LateUpdate()
    {
        _root.Update();
    }

    public void AddRangeToProcess(List<Transform> toAdd)
    {
        foreach (Transform t in toAdd)
            _pendingInsertion.Enqueue(t);

        UpdateTree();
    }

    public Transform FindClosestObjectInTree(Vector3 point)
    {
        return _root.FindClosestObject(point);
    }

    #region privates
    private void UpdateTree() //complete & tested 
    {
        if (!_treeBuilt)
        {
            while (_pendingInsertion.Count != 0)
                _root._objects.Add((Transform)_pendingInsertion.Dequeue());
            EditorCoroutineUtility.StartCoroutine(_root.BuildTree(), this);
        }
        else
        {
            while (_pendingInsertion.Count != 0)
                _root.Insert((Transform)_pendingInsertion.Dequeue());
        }
        _treeReady = true;
    }

    private void UnloadContent()
    {
        _root = new OctTreeJoint(this, _root._region);
    }
    #endregion

    public void DrawGizmo()
    {
        _root.DrawGizmo();
    }
}