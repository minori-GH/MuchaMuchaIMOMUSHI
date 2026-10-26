using System;
using UnityEngine;

/// <summary>
/// 食べたフルーツの色を記録して現在の体色を管理する
/// </summary>

public sealed class PlayerColorModel : MonoBehaviour
{
    // 「どれだけ色を集めたら100％の濃さになるか」
    [SerializeField] private float colorAmountForFullStrength = 1000f;
    // 「灰色をどれだけ明るくするか」
    [Range(0f, 1f)]
    [SerializeField] private float neutralGrayValue = 0.5f;

    // 「色が変わったことを通知する」
    public event Action<Color> OnColorChanged;

    // 「今まで集めた赤緑青の色数」
    public int TotalRed { get; private set; }
    public int TotalGreen { get; private set; }
    public int TotalBlue { get; private set; }
    // 「現在の体色、初期値は白」
    public Color CurrentColor { get; private set; } = Color.white;

    // *フルーツを食べたときに色が追加するという関数
    public void AddFruitColor(FruitData fruitData)
    {
        if (fruitData == null)
        {
            return;
        }

        AddColor(fruitData.RedValue, fruitData.GreenValue, fruitData.BlueValue);
    }

    // *実際に色ポイントを加算する関数
    public void AddColor(int red, int green, int blue)
    {
        TotalRed += Mathf.Max(0, red);
        TotalGreen += Mathf.Max(0, green);
        TotalBlue += Mathf.Max(0, blue);

        // 「現在の色を計算」
        CurrentColor = CalculateDisplayColor();
        // 「色が変わったことを通知する」
        OnColorChanged?.Invoke(CurrentColor);
    }

    // *色の反映に関するあれこれ
    private Color CalculateDisplayColor()
    {
        // *トータルで一番多い色を判定
        int total = TotalRed + TotalGreen + TotalBlue;

        if (total <= 0)
        {
            return Color.white;
        }

        float maxValue = Mathf.Max(TotalRed, Mathf.Max(TotalGreen, TotalBlue));
        float minValue = Mathf.Min(TotalRed, Mathf.Min(TotalGreen, TotalBlue));

        // *カラー比率で色を混ぜる
        Color targetColor = new Color(
            TotalRed / maxValue,
            TotalGreen / maxValue,
            TotalBlue / maxValue,
            1f
        );

        // *灰色補正
        if (minValue > 0f)
        {
            float grayBlend = minValue / maxValue;
            Color neutralGray = new Color(neutralGrayValue, neutralGrayValue, neutralGrayValue, 1f);
            targetColor = Color.Lerp(targetColor, neutralGray, grayBlend);
        }

        // *濃度
        float strength = Mathf.Clamp01(total / Mathf.Max(1f, colorAmountForFullStrength));
        // *最終的な色
        return Color.Lerp(Color.white, targetColor, strength);
    }
}
