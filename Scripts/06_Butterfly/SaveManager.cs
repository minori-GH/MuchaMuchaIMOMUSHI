using System;
using System.Collections.Generic;
using System.IO; // ファイルの読み書き
using UnityEngine;

 /// <summary>
 /// データの保存と読み込みを行うクラス
 /// </summary>
 
public sealed class SaveManager
{
    // [const : 定数（後から変更しない値）]
    private const string ButterflyDexFileName = "butterfly_dex.json";

    // 「SaveFilePath が読まれたら、右側の結果を返す」
    // [Application.persistentDataPath : ゲームのセーブデータを保存するフォルダのようなもの。Unityの機能」
    // [Path.Combine() : フォルダの場所とファイル名を安全につなげる処理」
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, ButterflyDexFileName);

    // 「外部から、保存されている蝶リストを読み込むメソッド」
    public List<ButterflyDexEntry> LoadButterflyDexEntries()
    {
        // 「保存データ全体を読み込み、ButterflyDexSaveData saveData に入れる」
        ButterflyDexSaveData saveData = LoadButterflyDexSaveData();
        // 「外部から読み込んだ蝶データのリストを新しいリストにして返す」
        return new List<ButterflyDexEntry>(saveData.Entries);
    }

    // 「新しい蝶1匹を図鑑に追加するメソッド」
    public void AddButterflyDexEntry(ButterflyDexEntry entry)
    {
        // 「保存する蝶が存在するか確認」
        if (entry == null)
        {
            Debug.LogError("[SaveManager] Cannot save null ButterflyDexEntry.");
            return;
        }

        // 「まず今まで保存していた蝶を読み込む」
        ButterflyDexSaveData saveData = LoadButterflyDexSaveData();
        // 「新しい蝶をリストの最後に追加する」
        saveData.Entries.Add(entry);
        // 「saveData を 別のメソッド SaveButterflyDexSaveData に渡す」
        SaveButterflyDexSaveData(saveData);
    }

    // 「保存データを読み込むときのメソッド」
    private ButterflyDexSaveData LoadButterflyDexSaveData()
    {
        // 「保存ファイルがない場合、空の図鑑を新しく作って返す」
        if (!File.Exists(SaveFilePath))
        {
            Debug.Log($"[SaveManager] Save file does not exist yet. Starting with an empty butterfly dex. Path: {SaveFilePath}");
            return new ButterflyDexSaveData();
        }

        // [try : とりあえずこの処理を試してみる」
        try
        {
            // [File.ReadAllText(...) : ファイルの中身を全部文字列として読み込むメソッド」
            string json = File.ReadAllText(SaveFilePath);

            // 「JSONファイルが空の場合、空の図鑑として扱う」
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ButterflyDexSaveData();
            }

            // [A ?? B : Aが null なら、Bを使う]
            // 「JSON文字列を ButterflyDexSaveData に戻す」
            ButterflyDexSaveData saveData = JsonUtility.FromJson<ButterflyDexSaveData>(json) 
                ?? new ButterflyDexSaveData();
            // 「読み込み件数を表示する」
            Debug.Log($"[SaveManager] Butterfly dex loaded. Count: {saveData.Entries.Count}. Path: {SaveFilePath}");
            return saveData;
        }
        // 「読み込み途中でエラーが発生したら catch に入る」
        catch (Exception exception)
        {
            // 「何が原因で失敗したのか表示する」
            Debug.LogError($"[SaveManager] Failed to load butterfly dex. Path: {SaveFilePath}\n{exception}");
            // 「読み込み失敗 → ゲームをクラッシュさせず、空の図鑑として扱う」
            return new ButterflyDexSaveData();
        }
    }

    // 「ButterflyDexSaveData をJSONファイルに保存するメソッド」
    private void SaveButterflyDexSaveData(ButterflyDexSaveData saveData)
    {
        try
        {
            // 「保存フォルダを取得」
            string directoryPath = Path.GetDirectoryName(SaveFilePath);

            // 「保存先フォルダがなければ作る」
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // [FromJson : JSON → C# ]
            // [ToJson : C# → JSON ]
            // [true : JSONを読みやすく整形して出力する機能をonにする]
            string json = JsonUtility.ToJson(saveData, true);
            // 「json の文字列を SaveFilePath に書き込む」
            File.WriteAllText(SaveFilePath, json);

            Debug.Log($"[SaveManager] Butterfly saved. Count: {saveData.Entries.Count}. Path: {SaveFilePath}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[SaveManager] Failed to save butterfly dex. Path: {SaveFilePath}\n{exception}");
        }
    }
}
