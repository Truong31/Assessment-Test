using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    private Camera m_cam;

    private bool m_gameOver;
    private bool m_gameWin;

    private bool m_isDragging;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    public bool IsGameOver => m_gameOver;
    public bool IsGameWin => m_gameWin;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);

        Fill();
    }

    private void Fill()
    {
        m_board.Fill();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_WIN:
                m_gameWin = true;
                IsBusy = true;
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver || m_gameWin) return;
        if (IsBusy) return;

        // Kiểm tra điều kiện thắng/thua
        if (CheckGameCondition())
        {
            m_gameManager.SetState(GameManager.eStateGame.GAME_WIN);
        }
        else if (m_gameOver)
        {
            m_gameManager.SetState(GameManager.eStateGame.GAME_OVER);
        }

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null)
                {
                    if (cell.BoardY >= 0) // Xử lý click vào ô trên Board
                    {
                        IsBusy = true;
                        m_board.MoveItemToBottom(cell);
                        IsBusy = false;
                    }
                    else if (m_gameManager.timerMode)
                    {
                        if (cell.BoardY == -1) // Xử lý click vào BottomCell
                        {
                            IsBusy = true;
                            m_board.MoveItemBackToBoard(cell);
                            IsBusy = false;
                        }

                    }
                }
                else
                {
                    m_isDragging = true;
                    m_hitCollider = hit.collider;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetRayCast();
        }
        
    }

    private void ResetRayCast()
    {
        m_isDragging = false;
        m_hitCollider = null;
    }

    //private void FindMatchesAndCollapse(Cell cell1, Cell cell2)
    //{
    //    if (cell1.Item is BonusItem)
    //    {
    //        cell1.ExplodeItem();
    //        StartCoroutine(ShiftDownItemsCoroutine());
    //    }
    //    else if (cell2.Item is BonusItem)
    //    {
    //        cell2.ExplodeItem();
    //        StartCoroutine(ShiftDownItemsCoroutine());
    //    }
    //    else
    //    {
    //        List<Cell> cells1 = GetMatches(cell1);
    //        List<Cell> cells2 = GetMatches(cell2);

    //        List<Cell> matches = new List<Cell>();
    //        matches.AddRange(cells1);
    //        matches.AddRange(cells2);
    //        matches = matches.Distinct().ToList();

    //        if (matches.Count < m_gameSettings.MatchesMin)
    //        {
    //            m_board.Swap(cell1, cell2, () =>
    //            {
    //                IsBusy = false;
    //            });
    //        }
    //        else
    //        {
    //            OnMoveEvent();

    //            CollapseMatches(matches, cell2);
    //        }
    //    }
    //}

    //private void FindMatchesAndCollapse()
    //{
    //    List<Cell> matches = m_board.FindFirstMatch();

    //    if (matches.Count > 0)
    //    {
    //        CollapseMatches(matches, null);
    //    }
    //    else
    //    {
    //        m_potentialMatch = m_board.GetPotentialMatches();
    //        if (m_potentialMatch.Count > 0)
    //        {
    //            IsBusy = false;

    //            m_timeAfterFill = 0f;
    //        }
    //        else
    //        {
    //            //StartCoroutine(RefillBoardCoroutine());
    //            StartCoroutine(ShuffleBoardCoroutine());
    //        }
    //    }
    //}

    //private List<Cell> GetMatches(Cell cell)
    //{
    //    List<Cell> listHor = m_board.GetHorizontalMatches(cell);
    //    if (listHor.Count < m_gameSettings.MatchesMin)
    //    {
    //        listHor.Clear();
    //    }

    //    List<Cell> listVert = m_board.GetVerticalMatches(cell);
    //    if (listVert.Count < m_gameSettings.MatchesMin)
    //    {
    //        listVert.Clear();
    //    }

    //    return listHor.Concat(listVert).Distinct().ToList();
    //}

    //private void CollapseMatches(List<Cell> matches, Cell cellEnd)
    //{
    //    for (int i = 0; i < matches.Count; i++)
    //    {
    //        matches[i].ExplodeItem();
    //    }

    //    if(matches.Count > m_gameSettings.MatchesMin)
    //    {
    //        m_board.ConvertNormalToBonus(matches, cellEnd);
    //    }

    //    StartCoroutine(ShiftDownItemsCoroutine());
    //}

    //private IEnumerator ShiftDownItemsCoroutine()
    //{
    //    m_board.ShiftDownItems();

    //    yield return new WaitForSeconds(0.2f);

    //    m_board.FillGapsWithNewItems();

    //    yield return new WaitForSeconds(0.2f);

    //    //FindMatchesAndCollapse();
    //}

    //private IEnumerator RefillBoardCoroutine()
    //{
    //    m_board.ExplodeAllItems();

    //    yield return new WaitForSeconds(0.2f);

    //    m_board.Fill();

    //    yield return new WaitForSeconds(0.2f);

    //    //FindMatchesAndCollapse();
    //}

    //private IEnumerator ShuffleBoardCoroutine()
    //{
    //    m_board.Shuffle();

    //    yield return new WaitForSeconds(0.3f);

    //    //FindMatchesAndCollapse();
    //}


    //private void SetSortingLayer(Cell cell1, Cell cell2)
    //{
    //    if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
    //    if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    //}

    //private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    //{
    //    return cell1.IsNeighbour(cell2);
    //}

    internal void Clear()
    {
        m_board.Clear();
    }

    public bool CheckGameCondition()
    {
        // Kiểm tra xem còn item trên bảng không
        bool hasItemsOnBoard = !m_board.IsBoardEmpty();

        // Kiểm tra xem bottom cells có đầy không
        bool isBottomFull = m_board.IsBottomFull();

        // Nếu còn item trên bảng và bottom cells đầy -> thua
        if (hasItemsOnBoard && isBottomFull)
        {
            m_gameOver = true;
            return false;
        }
        // Nếu không còn item trên bảng -> thắng
        else if (!hasItemsOnBoard)
        {
            m_gameWin = true;
            return true;
        }
        //Hết thời gian nhưng vẫn không clear được -> thua
        else if (m_gameManager.isOutOfTime && hasItemsOnBoard)
        {
            m_gameOver = true;
            return false;
        }
        //Còn thời gian nhưng đã clear bảng -> thắng
        else if (!m_gameManager.isOutOfTime && !hasItemsOnBoard)
        {
            m_gameWin = true;
            return true;
        }

        return false;
    }

    //private void ShowHint()
    //{
    //    m_hintIsShown = true;
    //    foreach (var cell in m_potentialMatch)
    //    {
    //        cell.AnimateItemForHint();
    //    }
    //}

    //private void StopHints()
    //{
    //    m_hintIsShown = false;
    //    foreach (var cell in m_potentialMatch)
    //    {
    //        cell.StopHintAnimation();
    //    }

    //    m_potentialMatch.Clear();
    //}

    // Phương thức này cho phép các lớp khác xử lý cell click
    public void ProcessCellClick(Cell cell)
    {
        if (m_gameOver || m_gameWin) return;
        
        if (cell != null)
        {
            if (cell.BoardY >= 0) // Xử lý click vào ô trên Board
            {
                IsBusy = true;
                m_board.MoveItemToBottom(cell);
                IsBusy = false;
                
                // Thông báo rằng một lượt đi đã được thực hiện
                OnMoveEvent();
            }
            else if (m_gameManager.timerMode)
            {
                if (cell.BoardY == -1) // Xử lý click vào BottomCell
                {
                    IsBusy = true;
                    m_board.MoveItemBackToBoard(cell);
                    IsBusy = false;
                    
                    // Thông báo rằng một lượt đi đã được thực hiện
                    OnMoveEvent();
                }
            }
        }
    }
}
