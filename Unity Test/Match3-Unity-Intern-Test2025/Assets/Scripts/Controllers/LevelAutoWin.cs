using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelAutoWin : LevelCondition
{
    private BoardController m_boardController;
    private GameManager m_gameManager;
    private float m_moveDelay = 0.5f;
    private float m_timer;
    private Camera m_cam;
    private bool m_isProcessingMove = false;
    
    // Lưu trữ thông tin về BottomCell hiện tại để phân tích
    private Dictionary<NormalItem.eNormalType, List<Cell>> m_bottomCellsByType = new Dictionary<NormalItem.eNormalType, List<Cell>>();
    
    public override void Setup(float value, Text txt, GameManager mngr)
    {
        base.Setup(value, txt, mngr);
        m_gameManager = mngr;
        m_timer = 0f;
        m_cam = Camera.main;
    }
    
    public override void Setup(float value, Text txt, BoardController board)
    {
        base.Setup(value, txt, board);
        m_boardController = board;
        m_timer = 0f;
        m_cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController == null || m_boardController.IsBusy || m_boardController.IsGameWin || m_boardController.IsGameOver || m_isProcessingMove)
            return;

        m_timer += Time.deltaTime;
        
        if (m_timer >= m_moveDelay)
        {
            m_timer = 0f;
            MakeGoodMove();
        }
    }
    
    private void MakeGoodMove()
    {
        // Cập nhật thông tin về trạng thái BottomCell hiện tại
        UpdateBottomCellsInfo();
        
        // Tìm lượt đi tốt nhất dựa trên phân tích BottomCell
        Cell bestCell = FindBestMoveForWinning();
        
        if (bestCell != null)
        {
            m_isProcessingMove = true;
            
            // Gọi phương thức ProcessCellClick của BoardController
            m_boardController.ProcessCellClick(bestCell);
            
            // Đặt timer để reset trạng thái sau khi xử lý xong
            StartCoroutine(ResetProcessingState());
        }
        else
        {
            // Nếu không tìm thấy lượt đi tốt nhất, thực hiện một lượt đi ngẫu nhiên
            List<Cell> cells = FindAllClickableCells();
            
            if (cells.Count > 0)
            {
                m_isProcessingMove = true;
                
                // Chọn cell ngẫu nhiên để click
                Cell cell = cells[Random.Range(0, cells.Count)];
                
                // Ghi log việc nhấp chuột
                string itemType = cell.Item is NormalItem ? (cell.Item as NormalItem).ItemType.ToString() : "Unknown";
                
                // Gọi phương thức ProcessCellClick
                m_boardController.ProcessCellClick(cell);
                
                // Đặt timer để reset trạng thái sau khi xử lý xong
                StartCoroutine(ResetProcessingState());
            }
        }
    }
    
    private void UpdateBottomCellsInfo()
    {
        m_bottomCellsByType.Clear();
        
        // Tìm tất cả BottomCell trong scene
        Cell[] allCells = Object.FindObjectsOfType<Cell>();
        
        foreach (Cell cell in allCells)
        {
            // Chỉ xét các BottomCell (BoardY == -1)
            if (cell.BoardY == -1 && !cell.IsEmpty && cell.Item is NormalItem)
            {
                NormalItem item = cell.Item as NormalItem;
                NormalItem.eNormalType itemType = item.ItemType;
                
                if (!m_bottomCellsByType.ContainsKey(itemType))
                {
                    m_bottomCellsByType[itemType] = new List<Cell>();
                }
                
                m_bottomCellsByType[itemType].Add(cell);
            }
        }
        
        // Debug thông tin về BottomCell
        foreach (var pair in m_bottomCellsByType)
        {
            string cellsInfo = string.Join(", ", pair.Value.Select(c => c.BoardX).ToArray());
        }
    }
    
    private Cell FindBestMoveForWinning()
    {
        // Tìm item trên Board phù hợp nhất để di chuyển xuống BottomCell
        // dựa trên phân tích BottomCell hiện tại
        
        // Ưu tiên 1: Tìm item loại mà đã có 2 item cùng loại liền kề nhau trong BottomCell
        foreach (var pair in m_bottomCellsByType)
        {
            NormalItem.eNormalType itemType = pair.Key;
            List<Cell> cells = pair.Value;
            
            // Nếu đã có 2 item cùng loại trong BottomCell
            if (cells.Count == 2)
            {
                // Kiểm tra xem hai item này có liền kề nhau không
                Cell cell1 = cells[0];
                Cell cell2 = cells[1];
                
                // Sắp xếp theo BoardX tăng dần
                if (cell1.BoardX > cell2.BoardX)
                {
                    Cell temp = cell1;
                    cell1 = cell2;
                    cell2 = temp;
                }
                
                // Kiểm tra xem có liền kề nhau không
                if (cell2.BoardX - cell1.BoardX == 1)
                {
                    // Tìm item cùng loại trên bảng để di chuyển xuống
                    Cell matchingCell = FindItemByType(itemType);
                    if (matchingCell != null)
                    {
                        return matchingCell;
                    }
                }
                // Hoặc có thể tạo bộ 3 nếu có một ô trống ở giữa (BoardX + 2)
                else if (cell2.BoardX - cell1.BoardX == 2)
                {
                    // Kiểm tra ô ở giữa có trống không
                    Cell middleCell = FindBottomCellByX(cell1.BoardX + 1);
                    if (middleCell != null && middleCell.IsEmpty)
                    {
                        // Tìm item cùng loại trên bảng để di chuyển xuống
                        Cell matchingCell = FindItemByType(itemType);
                        if (matchingCell != null)
                        {
                            return matchingCell;
                        }
                    }
                }
            }
            // Nếu đã có 3 item cùng loại thì tìm thêm để tạo bộ 3 liền kề
            else if (cells.Count >= 3)
            {
                // Sắp xếp cells theo BoardX
                cells.Sort((a, b) => a.BoardX.CompareTo(b.BoardX));
                
                // Tìm 3 cells liền kề nhau
                for (int i = 0; i < cells.Count - 2; i++)
                {
                    if (cells[i+1].BoardX - cells[i].BoardX == 1 && 
                        cells[i+2].BoardX - cells[i+1].BoardX == 1)
                    {
                        // Vẫn return matching cell để tiếp tục di chuyển items xuống
                        return FindItemByType(itemType);
                    }
                }
                
                // Nếu chưa có 3 cells liền kề, tìm 2 cells liền kề và thêm 1
                for (int i = 0; i < cells.Count - 1; i++)
                {
                    if (cells[i+1].BoardX - cells[i].BoardX == 1)
                    {
                        // Tìm ô trống bên cạnh 2 cells này
                        Cell leftEmpty = FindBottomCellByX(cells[i].BoardX - 1);
                        Cell rightEmpty = FindBottomCellByX(cells[i+1].BoardX + 1);
                        
                        if ((leftEmpty != null && leftEmpty.IsEmpty) || 
                            (rightEmpty != null && rightEmpty.IsEmpty))
                        {
                            Cell matchingCell = FindItemByType(itemType);
                            if (matchingCell != null)
                            {
                                return matchingCell;
                            }
                        }
                    }
                }
            }
        }
        
        // Ưu tiên 2: Nếu đã có 1 item trong BottomCell, tìm thêm item cùng loại
        foreach (var pair in m_bottomCellsByType)
        {
            if (pair.Value.Count == 1)
            {
                Cell matchingCell = FindItemByType(pair.Key);
                if (matchingCell != null)
                {
                    return matchingCell;
                }
            }
        }
        
        // Nếu BottomCell trống hoặc không tìm thấy item phù hợp, trả về một item ngẫu nhiên
        return null;
    }
    
    private Cell FindBottomCellByX(int x)
    {
        // Tìm BottomCell có BoardX = x
        Cell[] allCells = Object.FindObjectsOfType<Cell>();
        
        foreach (Cell cell in allCells)
        {
            if (cell.BoardY == -1 && cell.BoardX == x)
            {
                return cell;
            }
        }
        
        return null;
    }
    
    private Cell FindItemByType(NormalItem.eNormalType targetType)
    {
        // Tìm một item trên bảng có loại trùng với targetType
        Cell[] allCells = Object.FindObjectsOfType<Cell>();
        List<Cell> matchingCells = new List<Cell>();
        
        foreach (Cell cell in allCells)
        {
            if (cell.BoardY >= 0 && !cell.IsEmpty && cell.Item is NormalItem)
            {
                NormalItem item = cell.Item as NormalItem;
                if (item.ItemType == targetType)
                {
                    matchingCells.Add(cell);
                }
            }
        }
        
        // Nếu có nhiều item cùng loại, ưu tiên các item ở hàng dưới cùng (BoardY thấp nhất)
        if (matchingCells.Count > 0)
        {
            matchingCells.Sort((a, b) => a.BoardY.CompareTo(b.BoardY));
            return matchingCells[0];
        }
        
        return null;
    }
    
    private IEnumerator ResetProcessingState()
    {
        yield return new WaitForSeconds(0.1f);
        m_isProcessingMove = false;
    }
    
    private List<Cell> FindAllClickableCells()
    {
        List<Cell> result = new List<Cell>();
        
        // Tìm tất cả các Cell trong scene
        Cell[] allCells = Object.FindObjectsOfType<Cell>();
        
        foreach (Cell cell in allCells)
        {
            // Chỉ xét các cell trên bảng (không phải bottom cells)
            if (cell.BoardY >= 0 && !cell.IsEmpty)
            {
                result.Add(cell);
            }
        }
        
        return result;
    }
}
