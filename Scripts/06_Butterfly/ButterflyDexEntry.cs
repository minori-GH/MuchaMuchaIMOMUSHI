using System;
using UnityEngine;

/// <summary>
/// 図鑑に登録する蝶1匹分の情報をまとめて持っておくスクリプト
/// ※実データはprivateフィールドに持たせて、publicプロパティ経由で外に見せる
/// </summary>

[Serializable]
public sealed class ButterflyDexEntry
{
    // 「保存対象になるフィールド」
    [SerializeField] private string name;
    [SerializeField] private ButterflySizeType sizeType;
    [SerializeField] private ButterflyColorType colorType;
    [SerializeField] private Color color;
    [SerializeField] private string createdDate;

    // 「プロパティ」
    public string Name => name;
    public ButterflySizeType SizeType => sizeType;
    public ButterflyColorType ColorType => colorType;
    public Color Color => color;
    public string CreatedDate => createdDate;

    // 「外から渡された5つの引数を一時的に受け取る ButterflyDexEntry のコンストラクタ」
    public ButterflyDexEntry(
        string name,
        ButterflySizeType sizeType,
        ButterflyColorType colorType,
        Color color,
        string createdDate)
    {
        // 「this.name は、この ButterflyDexEntry 自身が持っているフィールドの name 」
        // 「name は、コンストラクタで受け取った引数の name 」
        // 「→このオブジェクトの name フィールドに、引数として受け取った name を代入する」
        this.name = name;
        this.sizeType = sizeType;
        this.colorType = colorType;
        this.color = color;
        this.createdDate = createdDate;
    }
}
