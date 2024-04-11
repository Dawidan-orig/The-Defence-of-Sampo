using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

[Serializable]
public class Pallete
// ��� ������� ��������.
/* ������� ������� ������������� ��������, ������� � ������� ����� ��������� ���� ����� ������
 * ��� ��������� � ����� ���� 1. � ���� ������ ������� ����� ���������� ������
 * ���� ����� ������ ������� �������������� ����� Editor
 */
{
    [SerializeField]
    List<PalleteObject> objectsSummary = new();

    public void Clear() => objectsSummary.Clear();

    public int GetPalleteSize() => objectsSummary.Count;



    public void Validate(PalleteObject value) 
    {
        Debug.Log($"Validating {value} on {value.index}");

        if (value.index > 0)
        { 
            PalleteObject neigh = objectsSummary[value.index - 1];
            neigh.right = value.left;
            UpdateValue(value.index - 1, neigh);
        }
        if (value.index < objectsSummary.Count-1)
        {
            PalleteObject neigh = objectsSummary[value.index +1];
            neigh.left = value.right;
            UpdateValue(value.index+1, neigh);
        }

        string res = "Objects:\n";
        foreach (PalleteObject obj in objectsSummary)
        {
            res += obj.ToString() + "\n";
        }
        Debug.Log(res);
    }

    public int GetIndexOfPass(float val) 
    {
        int i = 0;
        foreach (PalleteObject pObj in objectsSummary)
        {
            if (pObj.left < val && pObj.right > val)
                return i;

            i++;
        }

        return -1;
    }

    public UnityEngine.Object Pass(float val) 
    {
        //��, ��� ����� ��������������, �� � ���� ���� ������. �������� ����� ��� - ��� ���� �� ����� �� ���� ������ 100 ��������
        foreach(PalleteObject pObj in objectsSummary) 
        {
            if (pObj.left < val && pObj.right > val)
                return pObj.obj;
        }

        return null;
    }

    public UnityEngine.Object GetLeftPass(float val) 
    {
        int i = 0;
        foreach (PalleteObject pObj in objectsSummary)
        {
            if (pObj.left < val && pObj.right > val)
            {
                if (i == 0) return null;
                return objectsSummary[i - 1].obj;
            }
            i++;
        }

        return null;
    }

    public UnityEngine.Object GetRightPass(float val)
    {
        int i = 0;
        foreach (PalleteObject pObj in objectsSummary)
        {
            if (pObj.left < val && pObj.right > val)
            {
                if (i == objectsSummary.Count -1) return null;
                return objectsSummary[i + 1].obj;
            }
            i++;
        }

        return null;
    }

    public void AddNew(UnityEngine.Object toAdd, float probability) 
    {
        if (probability < 0 || probability > 1)
            Debug.LogError($"����������� � ������� � {toAdd.GetType()} �����������: {probability}");

        if (objectsSummary.Count == 0)
        {
            objectsSummary.Add(new PalleteObject(0, 1, toAdd, 0));
            return;
        }

        objectsSummary.Insert(0,new PalleteObject(0,probability,toAdd, 0));
        float reBuild = probability;

        float decreasingK = 1 - probability;
        
        for (int i =1; i < objectsSummary.Count; i++) 
        {
            PalleteObject obj = objectsSummary[i];
            float spaceUsed = obj.right - obj.left;
            float newSpaceUsed = spaceUsed * decreasingK;
            obj.left = reBuild;            
            obj.right = obj.left + newSpaceUsed;
            reBuild = obj.right;
            UpdateValue(i, obj);
            obj.index = i;
        }
    }

    public List<PalleteObject> GetPalleteObjectsRaw() 
    {
        return new List<PalleteObject>(objectsSummary);
    }

    private void UpdateValue(int i, PalleteObject obj) 
    {
        obj.index = i;
        objectsSummary.RemoveAt(i);
        objectsSummary.Insert(i, obj);
    }

    #region Unity-Adjusters
#endregion
}
