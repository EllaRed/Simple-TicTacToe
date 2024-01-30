using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState{none, cross, circle}

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{

}

public class TicTacToeAI : MonoBehaviour
{

	int _aiLevel;

	TicTacToeState[,] boardState;

	[SerializeField]
	private bool _isPlayerTurn;

	[SerializeField]
	private int _gridSize = 3;
	
	[SerializeField]
	private TicTacToeState playerState = TicTacToeState.cross;
	[SerializeField]
	private TicTacToeState aiState = TicTacToeState.circle;

	[SerializeField]
	private GameObject _xPrefab;

	[SerializeField]
	private GameObject _oPrefab;

	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
	public WinnerEvent onPlayerWin;

	ClickTrigger[,] _triggers;
	
	private void Awake()
	{
		if(onPlayerWin == null){
			onPlayerWin = new WinnerEvent();
		}
	}

	public void StartAI(int AILevel){
		_aiLevel = AILevel;
		StartGame();
	}

	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	private void StartGame()
	{
		_triggers = new ClickTrigger[3,3];
		boardState = new TicTacToeState[_gridSize, _gridSize];
		if(playerState==TicTacToeState.circle)
			aiState = TicTacToeState.cross;
		else
		{
			aiState = TicTacToeState.circle;
		}
		onGameStarted.Invoke();
		_isPlayerTurn = true;
	}

	public void PlayerSelects(int coordX, int coordY){

		SetVisual(coordX, coordY, playerState);
		_isPlayerTurn = false;
		AiTurn(_aiLevel);

	}

	public void AiSelects(int coordX, int coordY){

		SetVisual(coordX, coordY, aiState);
		_isPlayerTurn = true;
	}

	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);
		SetBoardState(coordX, coordY, targetState);
	}

    private void SetBoardState(int coordX, int coordY, TicTacToeState targetState)
    {
        boardState[coordX, coordY] = targetState;
		CheckForWin();
        
    }
	private void CheckForWin()
	{
        if (CheckForWinInRow(playerState) || CheckForWinInColumn(playerState) || CheckForWinInDiagonal(playerState))
		{
            onPlayerWin.Invoke(0);
        }
        else if (CheckForWinInRow(aiState) || CheckForWinInColumn(aiState) || CheckForWinInDiagonal(aiState))
		{
            onPlayerWin.Invoke(1);
        }
        else if (isBoardFull())
		{
            onPlayerWin.Invoke(-1);
        }
    }

    private bool CheckForWinInDiagonal(TicTacToeState playerState)
    {
        return (boardState[0, 0] == playerState && boardState[1, 1] == playerState && boardState[2, 2] == playerState) ||
			(boardState[0, 2] == playerState && boardState[1, 1] == playerState && boardState[2, 0] == playerState);
    }

    private bool CheckForWinInColumn(TicTacToeState state)
    {
        return FindStateInColumn(state, 0) == 3 || FindStateInColumn(state, 1) == 3 || FindStateInColumn(state, 2) == 3;
    }

    private bool CheckForWinInRow(TicTacToeState state)
    {
        return FindStateInRow(state, 0) == 3 || FindStateInRow(state, 1) == 3 || FindStateInRow(state, 2) == 3;
    }

    private void AiTurn(int diff)
	{
        var move = (-1,-1);
		if(diff == 0)
			move = FindBlockingMove();
        else
            move = FindBestMove();
		
			Debug.Log(move);
			if (move.Item1 == -1 || move.Item2 == -1)
			{
                _isPlayerTurn = true;
				return;
            }
			AiSelects(move.Item1, move.Item2);
            

        
		
	}
    private (int x, int y) FindBlockingMove()
    {	Debug.Log("FindBlockingMove");
        for (int x = 0; x < _gridSize; x++)
        {	if(FindStateInColumn(playerState, x) == 2 && EmptyCellOnColumn(x))
			{
				
				return FindEmptyCellonColumn(x);
			}
            for (int y = 0; y < _gridSize; y++)
            {
                if (FindStateInRow(playerState, y) == 2 && EmptyCellOnRow(y))
				{
				
                    return FindEmptyCellonRow(y);
                }
				
            }
        }
		//if there is one empty cell between two player cells, return that cell
		return MakeRandomMove();
        
    }
    private (int x, int y) FindBestMove()
    { Debug.Log("FindBestMove");
        int bestVal = -1000;
        (int x, int y) bestMove = (-1, -1);

        for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                // Check if cell is empty
                if (boardState[i, j] == TicTacToeState.none)
                {
                    // Make the move
                    boardState[i, j] = aiState;

                    // Compute evaluation function for this move
                    int moveVal = Minimax(0, false);

                    // Undo the move
                    boardState[i, j] = TicTacToeState.none;

                    // If the value of the current move is more than the best value, update best
                    if (moveVal > bestVal)
                    {
                        bestMove = (i, j);
                        bestVal = moveVal;
                    }
                }
            }
        }

        return bestMove;
    }

    private (int x, int y) MakeRandomMove()
	{
        // Find all empty cells
        var emptyCells = new List<(int x, int y)>();
        for (int x = 0; x < _gridSize; x++)
		{
            for (int y = 0; y < _gridSize; y++)
			{
                if (boardState[x, y] == TicTacToeState.none)
				{
                    emptyCells.Add((x, y));
                }
            }
        }
		// Pick a random empty cell
		if (emptyCells.Count != 0)
		{
			var randomIndex = UnityEngine.Random.Range(0, emptyCells.Count);
			var randomCell = emptyCells[randomIndex];
			
			return randomCell;
		}
        return (-1, -1);
    }	



    #region helpers
    private (int x, int y) FindEmptyCellonColumn(int column)
	{
        for (int y = 0; y < _gridSize; y++)
		{
            if (boardState[column, y] == TicTacToeState.none)
			{
                return (column, y);
            }
        }
        return (-1, -1);
    }
	private (int x, int y) FindEmptyCellonRow(int row)
	{
        for (int x = 0; x < _gridSize; x++)
		{
            if (boardState[x, row] == TicTacToeState.none)
			{
                return (x, row);
            }
        }
        return (-1, -1);
    }
	//find number of state in row
	private int FindStateInRow(TicTacToeState state, int row)
	{
        int count = 0;
        for (int x = 0; x < _gridSize; x++)
		{
            if (boardState[x, row] == state)
			{
                count++;
            }
        }
        return count;
    }
	//find number of state in column
	private int FindStateInColumn(TicTacToeState state, int column)
	{
        int count = 0;
        for (int y = 0; y < _gridSize; y++)
		{
            if (boardState[column, y] == state)
			{
                count++;
            }
        }
        return count;
    }
	private bool EmptyCellOnRow(int row)
	{
        for (int x = 0; x < _gridSize; x++)
		{
            if (boardState[x, row] == TicTacToeState.none)
			{
                return true;
            }
        }
        return false;
    }
	private bool EmptyCellOnColumn(int column)
	{
		  for (int y = 0; y < _gridSize; y++)
		{
            if (boardState[column, y] == TicTacToeState.none)
			{
                return true;
            }
        }
        return false;
	}

    #endregion
    public bool IsPlayerTurn()
	{
        return _isPlayerTurn;
    }

	private bool isBoardFull()
	{
        for (int x = 0; x < _gridSize; x++)
		{
            for (int y = 0; y < _gridSize; y++)
			{
                if (boardState[x, y] == TicTacToeState.none)
				{
                    return false;
                }
            }
        }
        return true;
    }

    private int Minimax(int depth, bool isMaximizing)
    {
        int score = EvaluateBoard();

        // Check for terminal states (win, lose, draw)
        if (score == 10) return score - depth;
        if (score == -10) return score + depth;
        if (isBoardFull()) return 0;

        if (isMaximizing)
        {
            int best = -1000;
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    // Check if cell is empty
                    if (boardState[x, y] == TicTacToeState.none)
                    {
                        // Make the move
                        boardState[x, y] = aiState;
                        // Recur and choose the maximum value
                        best = Math.Max(best, Minimax(depth + 1, !isMaximizing));
                        // Undo the move
                        boardState[x, y] = TicTacToeState.none;
                    }
                }
            }
            return best;
        }
        else
        {
            int best = 1000;
            for (int x = 0; x < _gridSize; x++)
            {
                for (int y = 0; y < _gridSize; y++)
                {
                    // Check if cell is empty
                    if (boardState[x, y] == TicTacToeState.none)
                    {
                        // Make the move
                        boardState[x, y] = playerState;
                        // Recur and choose the minimum value
                        best = Math.Min(best, Minimax(depth + 1, !isMaximizing));
                        // Undo the move
                        boardState[x, y] = TicTacToeState.none;
                    }
                }
            }
            return best;
        }
    }

    private int EvaluateBoard()
    {
        if (CheckForWinInRow(aiState) || CheckForWinInColumn(aiState) || CheckForWinInDiagonal(aiState))
        {
            return 10;
        }
        else if (CheckForWinInRow(playerState) || CheckForWinInColumn(playerState) || CheckForWinInDiagonal(playerState))
        {
            return -10;
        }
        else
        {
            return 0;
        }
    }
}
