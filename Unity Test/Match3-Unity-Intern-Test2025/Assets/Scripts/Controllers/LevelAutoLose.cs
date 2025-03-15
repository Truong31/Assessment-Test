using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelAutoLose : LevelCondition
{
    private BoardController m_boardController;
    private GameManager m_gameManager;
    private float m_moveDelay = 0.5f;
    private float m_timer;
    private Camera m_cam;
    private bool m_isProcessingMove = false;
    
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
            MakeBadMove();
        }
    }
    
    private void MakeBadMove()
    {
        // Chiến lược để chơi thua: Ưu tiên đưa item xuống bottom row, tạo ra các bottom cell đầy nhanh nhất
        
        List<Cell> cells = FindPrioritizedCellsForLosing();
        
        if (cells.Count > 0)
        {
            m_isProcessingMove = true;
            
            // Chọn cell ưu tiên để click
            Cell cell = cells[Random.Range(0, cells.Count)];
            
            // Gọi phương thức ProcessCellClick mới thêm vào BoardController
            m_boardController.ProcessCellClick(cell);
            
            // Đặt timer để reset trạng thái sau khi xử lý xong
            StartCoroutine(ResetProcessingState());
        }
    }
    
    private IEnumerator ResetProcessingState()
    {
        yield return new WaitForSeconds(0.1f);
        m_isProcessingMove = false;
    }
    
    private List<Cell> FindPrioritizedCellsForLosing()
    {
        List<Cell> result = new List<Cell>();
        
        // Tìm tất cả các Cell trong scene
        Cell[] allCells = Object.FindObjectsOfType<Cell>();
        
        // Ưu tiên các Cell có phần tử và ở hàng dưới cùng
        // Điều này sẽ tạo ra các bottom cells đầy nhanh nhất
        foreach (Cell cell in allCells)
        {
            // Ưu tiên các cell ở hàng dưới cùng của bảng (Y = 0) để tạo ra các bottom cell đầy nhanh hơn
            if (cell.BoardY == 0 && !cell.IsEmpty)
            {
                result.Add(cell);
            }
        }
        
        // Nếu không có cell ở hàng dưới, thì chọn bất kỳ cell nào khác
        if (result.Count == 0)
        {
            foreach (Cell cell in allCells)
            {
                if (cell.BoardY >= 0 && !cell.IsEmpty)
                {
                    result.Add(cell);
                }
            }
        }
        
        return result;
    }
}
