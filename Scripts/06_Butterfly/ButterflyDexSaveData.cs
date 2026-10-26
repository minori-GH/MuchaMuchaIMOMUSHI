using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 図鑑に登録した蝶を複数まとめて持つための保存用クラス
/// </summary>

// [[Serializable] : このクラスは保存用データとして扱えることをC#やUnity側に伝えるための属性」
[Serializable]
public sealed class ButterflyDexSaveData
{
    // 「ButterflyDexEntry型 を複数入れられる entries というリストをつくる」
    [SerializeField] private List<ButterflyDexEntry> entries = new List<ButterflyDexEntry>();

    // 「外部から使うためのプロパティ」
    public List<ButterflyDexEntry> Entries
    {
        get
        {
            if (entries == null)
            {
                entries = new List<ButterflyDexEntry>();
            }

            return entries;
        }
    }
}
