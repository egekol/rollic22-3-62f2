using System;

namespace Core.Scripts.Gameplay.Events
{
    public static class GameEvents
    {
        public static event Action<int> OnMoveCountChanged;
        public static event Action<int> OnCollectableCountChanged;

        public static void InvokeMoveCountChanged(int remainingMoves)
        {
            OnMoveCountChanged?.Invoke(remainingMoves);
        }

        public static void InvokeCollectableCountChanged(int collectedCount)
        {
            OnCollectableCountChanged?.Invoke(collectedCount);
        }
    }
}

