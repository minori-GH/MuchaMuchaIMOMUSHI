
/// <summary>
/// Scene ‚ğ‚Ü‚½‚®ŠÔ‚¾‚¯ GameResultData ‚ğ static ‚Åˆê•Û‚·‚é
/// </summary>
public static class GameResultHolder
{
    public static GameResultData CurrentResult { get; private set; }

    public static void SetResult(GameResultData result)
    {
        CurrentResult = result;
    }

    public static void Clear()
    {
        CurrentResult = null;
    }
}
