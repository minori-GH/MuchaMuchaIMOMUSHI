using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 芋虫が移動している間、頭と体節を順番に伸び縮みさせる
/// </summary>

public sealed class PlayerMoveSquashAnimator : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CaterpillarBody caterpillarBody;
    [SerializeField] private Transform headVisualRoot;
    [SerializeField, Min(0f)] private float horizontalScaleAmount = 0.08f; // X方向の伸び縮みの強さ（8％）
    [SerializeField, Min(0f)] private float verticalScaleAmount = 0.06f; // Y方向の伸び縮みの強さ（6％）
    [SerializeField, Min(0f)] private float waveSpeed = 4f; // 伸び縮みの周期（4fは1秒につき4周期）
    [SerializeField, Min(0f)] private float segmentDelay = 0.08f; // 後ろの体節のタイミングをずらす
    [SerializeField, Min(0.01f)] private float returnSpeed = 8f; // 停止したときに元の大きさへ戻す速さ

    // 「対象のTransformと、その元の大きさをセットで保存する辞書」
    private readonly Dictionary<Transform, Vector3> initialScales = new Dictionary<Transform, Vector3>();
    // 「現在のアニメーション対象を、順番に保存するリスト」
    private readonly List<Transform> currentTargets = new List<Transform>();
    // 「辞書から削除する対象を、一時的に集めるリスト」
    private readonly List<Transform> targetsToRemove = new List<Transform>();
    // 「現在の速度倍率を保存する変数」
    private float waveSpeedMultiplier = 1f;

    // ＊必要な参照が設定されているか確認
    private void Awake()
    {
        if (playerMovement == null)
        {
            Debug.LogError("[PlayerMoveSquashAnimator] PlayerMovement is not assigned.", this);
            enabled = false;
            return;
        }

        if (caterpillarBody == null)
        {
            Debug.LogError("[PlayerMoveSquashAnimator] CaterpillarBody is not assigned.", this);
            enabled = false;
            return;
        }

        if (headVisualRoot == null)
        {
            Debug.LogError("[PlayerMoveSquashAnimator] Head Visual Root is not assigned.", this);
            enabled = false;
            return;
        }
    }

    // ＊毎フレームの処理をまとめる
    private void LateUpdate()
    {
        // 「1．現在の頭と体節をアニメーション対象として集める」
        BuildCurrentTargets();

        // 「2．移動中なら伸び縮みさせる」
        if (playerMovement.IsMoving)
        {
            ApplyMoveWave();
        }
        // 「3．移動していなければ元の大きさへ戻す」
        else
        {
            ReturnToInitialScale();
        }

        // 「4．破棄されたTransformの記録を削除する」
        RemoveMissingTargets();
    }

    // ＊コンポーネントが無効になったとき、元の大きさへ戻す
    private void OnDisable()
    {
        // 「辞書 initialScales から1組ずつ取り出し、その組を initialScale という名前で使う」
        //  [KeyValuePair<Transform, Vector3> : Transform と Vector3 がセットになった型]
        //  [initialScale : 今回取り出した1組に付けた変数名]
        //  [in initialScales : initialScales の中から順番に取り出す]
        //  [initialScale.Key : 対象の Transform]
        //  [initialScale.Value  : 保存した元の大きさ Vector3]
        foreach (KeyValuePair<Transform, Vector3> initialScale in initialScales)
        {
            // 「対象のTransformが存在するとき、」
            if (initialScale.Key != null)
            {
                // 「対象の現在の拡大率を、保存してある元の拡大率に戻す」
                initialScale.Key.localScale = initialScale.Value;
            }
        }
    }

    // ＊外部のスクリプトからアニメーション速度倍率を変更するためのメソッド
    public void SetWaveSpeedMultiplier(float multiplier)
    {
        waveSpeedMultiplier = Mathf.Max(0.01f, multiplier);
    }

    // ＊PowerUpが終了したとき、速度倍率を通常の1倍へ戻すためのメソッド
    public void ResetWaveSpeedMultiplier()
    {
        waveSpeedMultiplier = 1f;
    }

    // ＊1．頭と現在の体節を集める
    private void BuildCurrentTargets()
    {
        // 「前のフレームの対象リストを空にする」
        currentTargets.Clear();
        // 「頭を登録する」
        AddTarget(headVisualRoot);
        // 「体節を1つずつ取り出して以下の処理を繰り返す」
        foreach (CaterpillarSegment segment in caterpillarBody.Segments)
        {
            // 「変形するTransformを取得する」
            Transform segmentVisualRoot = GetSegmentVisualRoot(segment);
            // 「そのTransformを登録する」
            AddTarget(segmentVisualRoot);
        }
    }

    // ＊対象を登録し、元の大きさを保存する
    private void AddTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        // 「現在の対象リストに追加する」
        currentTargets.Add(target);

        //  [initialScales.ContainsKey(target) : .NETのDictionary。指定したキーが辞書に登録されているか調べる]
        //  [target.localScale : 対象の拡大率を取得・設定するプロパティ]
        // 「辞書にまだ登録されていない対象なら、」
        if (!initialScales.ContainsKey(target))
        {
            // 「その対象と元の大きさを辞書へ追加する」
            initialScales.Add(target, target.localScale);
        }
    }

    // ＊体節を受け取り、変形するTransformを返す
    private static Transform GetSegmentVisualRoot(CaterpillarSegment segment)
    {
        if (segment == null)
        {
            return null;
        }

        // 「体節の SpriteRenderer があるなら、その画像が付いているTransformを返す」
        if (segment.SpriteRenderer != null)
        {
            return segment.SpriteRenderer.transform;
        }

        // 「SpriteRenderer がなければ、体節自身のTransformを返す」
        return segment.transform;
    }

    // ＊2．対象を順番に伸び縮みさせる
    private void ApplyMoveWave()
    {
        // 「対象を先頭から順番に処理する」
        for (int i = 0; i < currentTargets.Count; i++)
        {
            // 「その順番のTransformを取り出す」
            Transform target = currentTargets[i];

            // 「対象がない、または、元の大きさを辞書から取得できない場合は飛ばして次へ」
            if (target == null || !initialScales.TryGetValue(target, out Vector3 initialScale))
            {
                continue;
            }

            // 「体節ごとにタイミングをずらす」
            //  [Time.time : ゲーム内の経過時間]
            float delayedTime = Time.time - segmentDelay * i;
            // 「何周期分進んだかを、ラジアンの角度に変換」
            //  [Mathf.Sin() : 入力に応じて−1から1までの間を滑らかに往復する]
            //  [Mathf.PI : 円周率π（約3.14159）を表す定数]
            // 「※2f は float型の数値。円周率に掛けて1周期分の 2π にするための数値]
            float wave = Mathf.Sin(delayedTime * waveSpeed * waveSpeedMultiplier * Mathf.PI * 2f);
            float xScale = 1f + wave * horizontalScaleAmount;
            float yScale = 1f - wave * verticalScaleAmount;

            // 「保存してある元の大きさに、↑の倍率を掛ける」
            target.localScale = new Vector3(
                initialScale.x * xScale,
                initialScale.y * yScale,
                initialScale.z
            );
        }
    }

    // ＊3．元の大きさへ戻す
    private void ReturnToInitialScale()
    {
        // 「元のスケールへどれだけ近づけるかを計算」
        float maxDelta = returnSpeed * Time.deltaTime;

        // 「現在の対象を順番に処理する」
        foreach (Transform target in currentTargets)
        {
            // 「対象がない、または、元の大きさを辞書から取得できない場合は飛ばして次へ」
            if (target == null || !initialScales.TryGetValue(target, out Vector3 initialScale))
            {
                continue;
            }
            // 「現在のスケールから元のスケールへ最大 maxDelta の距離だけ近づける」
            target.localScale = Vector3.MoveTowards(target.localScale, initialScale, maxDelta);
        }
    }

    // ＊4．破棄された対象の記録を削除する
    private void RemoveMissingTargets()
    {
        // 「削除候補のリストを空にする」
        targetsToRemove.Clear();

        // 「辞書 initialScales から1組ずつ取り出し、その組を initialScale という名前で使う」
        foreach (KeyValuePair<Transform, Vector3> initialScale in initialScales)
        {
            // 「対象のTransformが存在しないとき、」
            if (initialScale.Key == null)
            {
                // 「削除候補に追加する」
                targetsToRemove.Add(initialScale.Key);
            }
        }

        // 「集めた削除候補を1つずつ辞書から削除する」
        foreach (Transform target in targetsToRemove)
        {
            initialScales.Remove(target);
        }
    }
}

