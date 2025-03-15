using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomCell : MonoBehaviour
{
    public Cell[] cells { get; private set; }
    private int maxCells = 5;
    private Transform root;

    public BottomCell(Transform transform)
    {
        root = transform;
        cells = new Cell[maxCells];
        CreateBottomCells();
    }

    private void CreateBottomCells()
    {
        Vector3 origin = new Vector3(-maxCells * 0.5f + 0.5f, -4f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int i = 0; i < maxCells; i++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.position = origin + new Vector3(i, 0, 0f);
            go.transform.SetParent(root);

            Cell cell = go.GetComponent<Cell>();
            cell.Setup(i, -1); // Y = -1 để phân biệt với các ô trên bảng

            cells[i] = cell;
        }
    }

    public bool IsFull()
    {
        foreach (Cell cell in cells)
        {
            if (cell.IsEmpty) return false;
        }
        return true;
    }

    public bool HasEmptyCell()
    {
        foreach (Cell cell in cells)
        {
            if (cell.IsEmpty) return true;
        }
        return false;
    }

    public Cell GetFirstEmptyCell()
    {
        foreach (Cell cell in cells)
        {
            if (cell.IsEmpty) return cell;
        }
        return null;
    }

    public List<Cell> GetMatchingCells(NormalItem.eNormalType type)
    {
        List<Cell> matchingCells = new List<Cell>();
        foreach (Cell cell in cells)
        {
            if (!cell.IsEmpty && cell.Item is NormalItem)
            {
                NormalItem item = cell.Item as NormalItem;
                if (item.ItemType == type)
                {
                    matchingCells.Add(cell);
                }
            }
        }
        return matchingCells;
    }

    public void Clear()
    {
        foreach (Cell cell in cells)
        {
            cell.Clear();
        }
    }
}
