using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    public enum eMatchDirection
    {
        NONE,
        HORIZONTAL,
        VERTICAL,
        ALL
    }

    private int boardSizeX;

    private int boardSizeY;

    private Cell[,] m_cells;

    private Transform m_root;

    private int m_matchMin;

    private BottomCell bottomCells;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;

        m_matchMin = gameSettings.MatchesMin;

        this.boardSizeX = gameSettings.BoardSizeX;
        this.boardSizeY = gameSettings.BoardSizeY;

        m_cells = new Cell[boardSizeX, boardSizeY];
        bottomCells = new BottomCell(transform);

        CreateBoard();
    }

    private void CreateBoard()
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y);

                m_cells[x, y] = cell;
            }
        }

        //set neighbours
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (y + 1 < boardSizeY) m_cells[x, y].NeighbourUp = m_cells[x, y + 1];
                if (x + 1 < boardSizeX) m_cells[x, y].NeighbourRight = m_cells[x + 1, y];
                if (y > 0) m_cells[x, y].NeighbourBottom = m_cells[x, y - 1];
                if (x > 0) m_cells[x, y].NeighbourLeft = m_cells[x - 1, y];
            }
        }

    }

    internal void Fill()
    {
        // Tạo danh sách các loại item có thể sử dụng
        List<NormalItem.eNormalType> availableTypes = new List<NormalItem.eNormalType>();
        foreach (NormalItem.eNormalType type in Enum.GetValues(typeof(NormalItem.eNormalType)))
        {
            availableTypes.Add(type);
        }

        // Tính số lượng item cần cho mỗi loại (phải chia hết cho 3)
        int totalCells = boardSizeX * boardSizeY;
        int numTypes = availableTypes.Count;

        // Tính số lượng item cho mỗi loại, đảm bảo chia hết cho 3
        int itemsPerType = (totalCells / numTypes);
        itemsPerType = (itemsPerType / 3) * 3; // Làm tròn xuống bội số của 3

        // Tạo danh sách các item cần điền vào bảng
        List<NormalItem.eNormalType> itemsToFill = new List<NormalItem.eNormalType>();

        for (int i = 0; i < numTypes; i++)
        {
            for (int j = 0; j < itemsPerType; j++)
            {
                itemsToFill.Add(availableTypes[i]);
            }
        }

        while (itemsToFill.Count < totalCells)
        {
            // Chọn ngẫu nhiên một loại item và thêm 3 item cùng loại
            NormalItem.eNormalType randomType = availableTypes[UnityEngine.Random.Range(0, numTypes)];
            for (int i = 0; i < 3; i++)
            {
                itemsToFill.Add(randomType);
                if (itemsToFill.Count >= totalCells) break;
            }
        }

        // Xáo trộn danh sách để tạo bảng ngẫu nhiên
        for (int i = itemsToFill.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = itemsToFill[i];
            itemsToFill[i] = itemsToFill[j];
            itemsToFill[j] = temp;
        }

        // Điền các item vào bảng
        int itemIndex = 0;
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (itemIndex >= itemsToFill.Count) break;

                Cell cell = m_cells[x, y];
                NormalItem item = new NormalItem();

                item.SetType(itemsToFill[itemIndex]);
                item.SetView();
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(false);

                itemIndex++;
            }
        }
    }

    //internal void Shuffle()
    //{
    //    List<Item> list = new List<Item>();
    //    for (int x = 0; x < boardSizeX; x++)
    //    {
    //        for (int y = 0; y < boardSizeY; y++)
    //        {
    //            list.Add(m_cells[x, y].Item);
    //            m_cells[x, y].Free();
    //        }
    //    }

    //    for (int x = 0; x < boardSizeX; x++)
    //    {
    //        for (int y = 0; y < boardSizeY; y++)
    //        {
    //            int rnd = UnityEngine.Random.Range(0, list.Count);
    //            m_cells[x, y].Assign(list[rnd]);
    //            m_cells[x, y].ApplyItemMoveToPosition();

    //            list.RemoveAt(rnd);
    //        }
    //    }
    //}


    //internal void FillGapsWithNewItems()
    //{
    //    for (int x = 0; x < boardSizeX; x++)
    //    {
    //        for (int y = 0; y < boardSizeY; y++)
    //        {
    //            Cell cell = m_cells[x, y];
    //            if (!cell.IsEmpty) continue;

    //            NormalItem item = new NormalItem();

    //            item.SetType(Utils.GetRandomNormalType());
    //            item.SetView();
    //            item.SetViewRoot(m_root);

    //            cell.Assign(item);
    //            cell.ApplyItemPosition(true);
    //        }
    //    }
    //}

    internal void ExplodeAllItems()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.ExplodeItem();
            }
        }
    }

    //public void Swap(Cell cell1, Cell cell2, Action callback)
    //{
    //    Item item = cell1.Item;
    //    cell1.Free();
    //    Item item2 = cell2.Item;
    //    cell1.Assign(item2);
    //    cell2.Free();
    //    cell2.Assign(item);

    //    item.View.DOMove(cell2.transform.position, 0.3f);
    //    item2.View.DOMove(cell1.transform.position, 0.3f).OnComplete(() => { if (callback != null) callback(); });
    //}

    public List<Cell> GetHorizontalMatches(Cell cell)
    {
        List<Cell> list = new List<Cell>();
        list.Add(cell);

        //check horizontal match
        Cell newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourRight;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourLeft;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        return list;
    }


    //public List<Cell> GetVerticalMatches(Cell cell)
    //{
    //    List<Cell> list = new List<Cell>();
    //    list.Add(cell);

    //    Cell newcell = cell;
    //    while (true)
    //    {
    //        Cell neib = newcell.NeighbourUp;
    //        if (neib == null) break;

    //        if (neib.IsSameType(cell))
    //        {
    //            list.Add(neib);
    //            newcell = neib;
    //        }
    //        else break;
    //    }

    //    newcell = cell;
    //    while (true)
    //    {
    //        Cell neib = newcell.NeighbourBottom;
    //        if (neib == null) break;

    //        if (neib.IsSameType(cell))
    //        {
    //            list.Add(neib);
    //            newcell = neib;
    //        }
    //        else break;
    //    }

    //    return list;
    //}

    //internal void ConvertNormalToBonus(List<Cell> matches, Cell cellToConvert)
    //{
    //    eMatchDirection dir = GetMatchDirection(matches);

    //    BonusItem item = new BonusItem();
    //    switch (dir)
    //    {
    //        case eMatchDirection.ALL:
    //            item.SetType(BonusItem.eBonusType.ALL);
    //            break;
    //        case eMatchDirection.HORIZONTAL:
    //            item.SetType(BonusItem.eBonusType.HORIZONTAL);
    //            break;
    //        case eMatchDirection.VERTICAL:
    //            item.SetType(BonusItem.eBonusType.VERTICAL);
    //            break;
    //    }

    //    if (item != null)
    //    {
    //        if (cellToConvert == null)
    //        {
    //            int rnd = UnityEngine.Random.Range(0, matches.Count);
    //            cellToConvert = matches[rnd];
    //        }

    //        item.SetView();
    //        item.SetViewRoot(m_root);

    //        cellToConvert.Free();
    //        cellToConvert.Assign(item);
    //        cellToConvert.ApplyItemPosition(true);
    //    }
    //}


    //internal eMatchDirection GetMatchDirection(List<Cell> matches)
    //{
    //    if (matches == null || matches.Count < m_matchMin) return eMatchDirection.NONE;

    //    var listH = matches.Where(x => x.BoardX == matches[0].BoardX).ToList();
    //    if (listH.Count == matches.Count)
    //    {
    //        return eMatchDirection.VERTICAL;
    //    }

    //    var listV = matches.Where(x => x.BoardY == matches[0].BoardY).ToList();
    //    if (listV.Count == matches.Count)
    //    {
    //        return eMatchDirection.HORIZONTAL;
    //    }

    //    if (matches.Count > 5)
    //    {
    //        return eMatchDirection.ALL;
    //    }

    //    return eMatchDirection.NONE;
    //}

    //internal List<Cell> FindFirstMatch()
    //{
    //    List<Cell> list = new List<Cell>();

    //    for (int x = 0; x < boardSizeX; x++)
    //    {
    //        for (int y = 0; y < boardSizeY; y++)
    //        {
    //            Cell cell = m_cells[x, y];

    //            var listhor = GetHorizontalMatches(cell);
    //            if (listhor.Count >= m_matchMin)
    //            {
    //                list = listhor;
    //                break;
    //            }

    //            var listvert = GetVerticalMatches(cell);
    //            if (listvert.Count >= m_matchMin)
    //            {
    //                list = listvert;
    //                break;
    //            }
    //        }
    //    }

    //    return list;
    //}

    //public List<Cell> CheckBonusIfCompatible(List<Cell> matches)
    //{
    //    var dir = GetMatchDirection(matches);

    //    var bonus = matches.Where(x => x.Item is BonusItem).FirstOrDefault();
    //    if(bonus == null)
    //    {
    //        return matches;
    //    }

    //    List<Cell> result = new List<Cell>();
    //    switch (dir)
    //    {
    //        case eMatchDirection.HORIZONTAL:
    //            foreach (var cell in matches)
    //            {
    //                BonusItem item = cell.Item as BonusItem;
    //                if (item == null || item.ItemType == BonusItem.eBonusType.HORIZONTAL)
    //                {
    //                    result.Add(cell);
    //                }
    //            }
    //            break;
    //        case eMatchDirection.VERTICAL:
    //            foreach (var cell in matches)
    //            {
    //                BonusItem item = cell.Item as BonusItem;
    //                if (item == null || item.ItemType == BonusItem.eBonusType.VERTICAL)
    //                {
    //                    result.Add(cell);
    //                }
    //            }
    //            break;
    //        case eMatchDirection.ALL:
    //            foreach (var cell in matches)
    //            {
    //                BonusItem item = cell.Item as BonusItem;
    //                if (item == null || item.ItemType == BonusItem.eBonusType.ALL)
    //                {
    //                    result.Add(cell);
    //                }
    //            }
    //            break;
    //    }

    //    return result;
    //}

    //internal List<Cell> GetPotentialMatches()
    //{
    //    List<Cell> result = new List<Cell>();
    //    for (int x = 0; x < boardSizeX; x++)
    //    {
    //        for (int y = 0; y < boardSizeY; y++)
    //        {
    //            Cell cell = m_cells[x, y];

    //            //check right
    //            /* example *\
    //              * * * * *
    //              * * * * *
    //              * * * ? *
    //              * & & * ?
    //              * * * ? *
    //            \* example  */

    //            if (cell.NeighbourRight != null)
    //            {
    //                result = GetPotentialMatch(cell, cell.NeighbourRight, cell.NeighbourRight.NeighbourRight);
    //                if (result.Count > 0)
    //                {
    //                    break;
    //                }
    //            }

    //            //check up
    //            /* example *\
    //              * ? * * *
    //              ? * ? * *
    //              * & * * *
    //              * & * * *
    //              * * * * *
    //            \* example  */
    //            if (cell.NeighbourUp != null)
    //            {
    //                result = GetPotentialMatch(cell, cell.NeighbourUp, cell.NeighbourUp.NeighbourUp);
    //                if (result.Count > 0)
    //                {
    //                    break;
    //                }
    //            }

    //            //check bottom
    //            /* example *\
    //              * * * * *
    //              * & * * *
    //              * & * * *
    //              ? * ? * *
    //              * ? * * *
    //            \* example  */
    //            if (cell.NeighbourBottom != null)
    //            {
    //                result = GetPotentialMatch(cell, cell.NeighbourBottom, cell.NeighbourBottom.NeighbourBottom);
    //                if (result.Count > 0)
    //                {
    //                    break;
    //                }
    //            }

    //            //check left
    //            /* example *\
    //              * * * * *
    //              * * * * *
    //              * ? * * *
    //              ? * & & *
    //              * ? * * *
    //            \* example  */
    //            if (cell.NeighbourLeft != null)
    //            {
    //                result = GetPotentialMatch(cell, cell.NeighbourLeft, cell.NeighbourLeft.NeighbourLeft);
    //                if (result.Count > 0)
    //                {
    //                    break;
    //                }
    //            }

    //            /* example *\
    //              * * * * *
    //              * * * * *
    //              * * ? * *
    //              * & * & *
    //              * * ? * *
    //            \* example  */
    //            Cell neib = cell.NeighbourRight;
    //            if (neib != null && neib.NeighbourRight != null && neib.NeighbourRight.IsSameType(cell))
    //            {
    //                Cell second = LookForTheSecondCellVertical(neib, cell);
    //                if (second != null)
    //                {
    //                    result.Add(cell);
    //                    result.Add(neib.NeighbourRight);
    //                    result.Add(second);
    //                    break;
    //                }
    //            }

    //            /* example *\
    //              * * * * *
    //              * & * * *
    //              ? * ? * *
    //              * & * * *
    //              * * * * *
    //            \* example  */
    //            neib = null;
    //            neib = cell.NeighbourUp;
    //            if (neib != null && neib.NeighbourUp != null && neib.NeighbourUp.IsSameType(cell))
    //            {
    //                Cell second = LookForTheSecondCellHorizontal(neib, cell);
    //                if (second != null)
    //                {
    //                    result.Add(cell);
    //                    result.Add(neib.NeighbourUp);
    //                    result.Add(second);
    //                    break;
    //                }
    //            }
    //        }

    //        if (result.Count > 0) break;
    //    }

    //    return result;
    //}

    //private List<Cell> GetPotentialMatch(Cell cell, Cell neighbour, Cell target)
    //{
    //    List<Cell> result = new List<Cell>();

    //    if (neighbour != null && neighbour.IsSameType(cell))
    //    {
    //        Cell third = LookForTheThirdCell(target, neighbour);
    //        if (third != null)
    //        {
    //            result.Add(cell);
    //            result.Add(neighbour);
    //            result.Add(third);
    //        }
    //    }

    //    return result;
    //}

    //private Cell LookForTheSecondCellHorizontal(Cell target, Cell main)
    //{
    //    if (target == null) return null;
    //    if (target.IsSameType(main)) return null;

    //    //look right
    //    Cell second = null;
    //    second = target.NeighbourRight;
    //    if (second != null && second.IsSameType(main))
    //    {
    //        return second;
    //    }

    //    //look left
    //    second = null;
    //    second = target.NeighbourLeft;
    //    if (second != null && second.IsSameType(main))
    //    {
    //        return second;
    //    }

    //    return null;
    //}

    //private Cell LookForTheSecondCellVertical(Cell target, Cell main)
    //{
    //    if (target == null) return null;
    //    if (target.IsSameType(main)) return null;

    //    //look up        
    //    Cell second = target.NeighbourUp;
    //    if (second != null && second.IsSameType(main))
    //    {
    //        return second;
    //    }

    //    //look bottom
    //    second = null;
    //    second = target.NeighbourBottom;
    //    if (second != null && second.IsSameType(main))
    //    {
    //        return second;
    //    }

    //    return null;
    //}

    //private Cell LookForTheThirdCell(Cell target, Cell main)
    //{
    //    if (target == null) return null;
    //    if (target.IsSameType(main)) return null;

    //    //look up
    //    Cell third = CheckThirdCell(target.NeighbourUp, main);
    //    if (third != null)
    //    {
    //        return third;
    //    }

    //    //look right
    //    third = null;
    //    third = CheckThirdCell(target.NeighbourRight, main);
    //    if (third != null)
    //    {
    //        return third;
    //    }

    //    //look bottom
    //    third = null;
    //    third = CheckThirdCell(target.NeighbourBottom, main);
    //    if (third != null)
    //    {
    //        return third;
    //    }

    //    //look left
    //    third = null;
    //    third = CheckThirdCell(target.NeighbourLeft, main); ;
    //    if (third != null)
    //    {
    //        return third;
    //    }

    //    return null;
    //}

    //private Cell CheckThirdCell(Cell target, Cell main)
    //{
    //    if (target != null && target != main && target.IsSameType(main))
    //    {
    //        return target;
    //    }

    //    return null;
    //}

    //internal void ShiftDownItems()
    //{
    //    for (int x = 0; x < boardSizeX; x++)
    //    {
    //        int shifts = 0;
    //        for (int y = 0; y < boardSizeY; y++)
    //        {
    //            Cell cell = m_cells[x, y];
    //            if (cell.IsEmpty)
    //            {
    //                shifts++;
    //                continue;
    //            }

    //            if (shifts == 0) continue;

    //            Cell holder = m_cells[x, y - shifts];

    //            Item item = cell.Item;
    //            cell.Free();

    //            holder.Assign(item);
    //            item.View.DOMove(holder.transform.position, 0.3f);
    //        }
    //    }
    //}

    public void Clear()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.Clear();

                GameObject.Destroy(cell.gameObject);
                m_cells[x, y] = null;
            }
        }
    }

    public bool MoveItemToBottom(Cell cell)
    {
        if (cell.IsEmpty || !(cell.Item is NormalItem)) return false;

        Cell emptyBottomCell = bottomCells.GetFirstEmptyCell();
        if (emptyBottomCell == null) return false;

        Item item = cell.Item;
        cell.Free();
        emptyBottomCell.Assign(item);
        item.View.DOMove(emptyBottomCell.transform.position, 0.3f);

        CheckBottomMatches();
        return true;
    }

    private void CheckBottomMatches()
    {
        Cell[] cells = bottomCells.cells;
        List<Cell> matchedCells = new List<Cell>();

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].IsEmpty || !(cells[i].Item is NormalItem)) continue;

            NormalItem currentItem = cells[i].Item as NormalItem;
            matchedCells.Clear();
            matchedCells.Add(cells[i]);

            // Kiểm tra các ô bên phải
            for (int j = i + 1; j < cells.Length; j++)
            {
                if (cells[j].IsEmpty || !(cells[j].Item is NormalItem)) break;

                NormalItem nextItem = cells[j].Item as NormalItem;
                if (nextItem.ItemType == currentItem.ItemType)
                {
                    matchedCells.Add(cells[j]);
                }
                else
                {
                    break;
                }
            }

            // Nếu tìm thấy 3 item giống nhau nằm cạnh nhau
            if (matchedCells.Count >= 3)
            {
                foreach (Cell cell in matchedCells)
                {
                    cell.ExplodeItem();
                }

                return; // Thoát sau khi xóa các item đầu tiên tìm thấy
            }
        }
    }

    public bool MoveItemBackToBoard(Cell bottomCell)
    {
        if (bottomCell.IsEmpty || !(bottomCell.Item is NormalItem)) return false;

        // Tìm ô trống đầu tiên trên Board từ dưới lên
        Cell emptyBoardCell = null;
        for (int y = 0; y < boardSizeY; y++)
        {
            for (int x = 0; x < boardSizeX; x++)
            {
                if (m_cells[x, y].IsEmpty)
                {
                    emptyBoardCell = m_cells[x, y];
                    break;
                }
            }
            if (emptyBoardCell != null) break;
        }

        if (emptyBoardCell == null) return false;

        Item item = bottomCell.Item;
        bottomCell.Free();
        emptyBoardCell.Assign(item);
        item.View.DOMove(emptyBoardCell.transform.position, 0.3f);

        return true;
    }

    public bool IsBoardEmpty()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (!m_cells[x, y].IsEmpty) return false;
            }
        }
        return true;
    }

    public bool IsBottomFull()
    {
        return bottomCells.IsFull();
    }

}
