using UnityEngine;
using System;

public class TurnSystem : MonoBehaviour
{
    public int movesLeft { get; private set; } = 5;

    public event Action OnMovesChanged;
    public event Action OnMovesExhausted;
    public event Action OnPlayerMoved;

    public void ConsumeMove()
    {
        movesLeft--;
        OnMovesChanged?.Invoke();

        if (movesLeft <= 0)
        {
            OnMovesExhausted?.Invoke();
        }
        else
        {
            OnPlayerMoved?.Invoke();
        }
    }

    public void ResetMoves(int amount = 5)
    {
        movesLeft = amount;
        OnMovesChanged?.Invoke();
    }

    public void AddMoves(int amount)
    {
        movesLeft += amount;
        OnMovesChanged?.Invoke();
    }
}
