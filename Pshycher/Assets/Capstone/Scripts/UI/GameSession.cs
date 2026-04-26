public static class GameSession
{
    public static int CurrentSlot = -1;

    public static bool HasValidSlot()
    {
        return CurrentSlot >= 0;
    }
}