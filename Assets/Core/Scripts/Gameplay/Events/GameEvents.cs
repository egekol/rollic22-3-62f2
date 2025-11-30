using System;

namespace Core.Scripts.Gameplay.Events
{
    public static class GameEvents
    {
        public static event Action<int> OnMoveCountChanged;

        public static void InvokeMoveCountChanged(int remainingMoves)
        {
            OnMoveCountChanged?.Invoke(remainingMoves);
        }
    }
}

