using UnityEngine;

public sealed class GameResultData
{
    public int SegmentCount { get; }
    public int TotalRed { get; }
    public int TotalGreen { get; }
    public int TotalBlue { get; }
    public Color FinalColor { get; }

    public GameResultData(
        int segmentCount,
        int totalRed,
        int totalGreen,
        int totalBlue,
        Color finalColor)
    {
        SegmentCount = segmentCount;
        TotalRed = totalRed;
        TotalGreen = totalGreen;
        TotalBlue = totalBlue;
        FinalColor = finalColor;
    }
}

