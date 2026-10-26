using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EncyclopediaScene が始まったら、SaveManager から保存済みの蝶一覧を受け取って、自分の中に保持しておく
/// </summary>
public sealed class EncyclopediaManager : MonoBehaviour
{
    // [readonly : このフィールドに入れた参照を、後から別のものに入れ替えない]
    private readonly SaveManager saveManager = new SaveManager();
    private List<ButterflyDexEntry> butterflyEntries = new List<ButterflyDexEntry>();

    // 「図鑑データの読み込みが終わったことを外部に通知するイベント」
    public event Action OnEntriesLoaded;

    // [IReadOnlyList : 外部には読み取り専用の一覧として見せる]
    // 「ButterflyEntries を読まれたら、内部の butterflyEntries を返す」
    public IReadOnlyList<ButterflyDexEntry> ButterflyEntries => butterflyEntries;

    // 「読み込みが終わったかの判定。初期値はfalse」
    public bool IsLoaded { get; private set; }

    // 「指定されたサイズと色系統の蝶が、保存済み List の中に1匹でもいるか調べる。いれば true 、いなければ false」
    public bool HasDiscoveredButterflyType(
       ButterflySizeType sizeType,
       ButterflyColorType colorType)
    {
        // [foreach : 一つずつ調べる]
        // 「butterflyEntries の中にいる ButterflyDexEntry を、1匹ずつ entry に入れて調べる」
        foreach (ButterflyDexEntry entry in butterflyEntries)
        {
            // 「もし今回取り出した蝶データが null なら、その蝶は無視して次へ行く」
            if (entry == null)
            {
                continue;
            }

            if (entry.SizeType == sizeType && entry.ColorType == colorType)
            {
                return true;
            }
        }

        return false;
    }

    private void Start()
    {
        // 「図鑑Sceneを開いたら保存データを読み込む」
        butterflyEntries = saveManager.LoadButterflyDexEntries();

        if (butterflyEntries == null)
        {
            butterflyEntries = new List<ButterflyDexEntry>();
        }

        Debug.Log($"[EncyclopediaManager] Loaded butterfly entries. Count: {butterflyEntries.Count}", this);
        // 「読み込みが完了したらtrueに変更」
        IsLoaded = true;
        // 「外部に読み込みが終わったことを通知」
        OnEntriesLoaded?.Invoke();
    }
}
