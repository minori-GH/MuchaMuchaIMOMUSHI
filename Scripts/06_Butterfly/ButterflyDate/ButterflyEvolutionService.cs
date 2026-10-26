public sealed class ButterflyEvolutionService
{
    public ButterflySizeType DetermineSize(int segmentCount)
    {
        if (segmentCount <= 5)
        {
            return ButterflySizeType.Small;
        }

        if (segmentCount <= 8)
        {
            return ButterflySizeType.Medium;
        }

        return ButterflySizeType.Large;
    }

    public ButterflyColorType DetermineColorType(int red, int green, int blue)
    {
        if (green >= red && green >= blue)
        {
            return ButterflyColorType.Green;
        }

        if (red >= blue)
        {
            return ButterflyColorType.Red;
        }

        return ButterflyColorType.Blue;
    }
}
