using UnityEngine;

public sealed class ButterflyData
{
    public ButterflySizeType SizeType { get; }
    public ButterflyColorType ColorType { get; }
    public Color Color { get; }

    public ButterflyData(
        ButterflySizeType sizeType,
        ButterflyColorType colorType,
        Color color)
    {
        SizeType = sizeType;
        ColorType = colorType;
        Color = color;
    }
}