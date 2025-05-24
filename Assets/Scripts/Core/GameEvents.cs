public static class GameEvents
{
    public static event System.Action<int> OnItemAcquired;
    public static event System.Action OnPirateKilled;
    public static event System.Action<int> OnItemRemoved;

    public static void ItemAcquired(int itemId)
    {
        OnItemAcquired?.Invoke(itemId);
    }

    public static void ItemRemoved(int itemId)
    {
        OnItemRemoved?.Invoke(itemId);
    }

    public static void PirateKilled()
    {
        OnPirateKilled?.Invoke();
    }
}
