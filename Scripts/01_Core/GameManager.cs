using System;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    [SerializeField] private GameState initialState = GameState.Playing;

    // *ƒQ[ƒ€‚Ìó‘Ô‚ª•ÏX‚µ‚½‚±‚Æ‚ğ’Ê’m‚·‚é
    public event Action<GameState> OnStateChanged;

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        CurrentState = initialState;
    }

    public void SetState(GameState state)
    {
        if (CurrentState == state)
        {
            return;
        }

        CurrentState = state;
        OnStateChanged?.Invoke(CurrentState);
    }
}
