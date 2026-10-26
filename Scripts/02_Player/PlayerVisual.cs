using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 芋虫の現在の状態を確認し、頭とすべての体節に正しい見た目を反映する
/// </summary>
public sealed class PlayerVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer headRenderer;
    [SerializeField] private CaterpillarBody caterpillarBody;
    [SerializeField] private PlayerColorModel colorModel;
    [SerializeField, Min(0f)] private float damageBlinkDuration = 1f; // 点滅全体の継続時間（ダメージ時）
    [SerializeField, Min(0.01f)] private float damageBlinkInterval = 0.1f; // 表示と非表示を切り替える間隔（ダメージ時）
    [SerializeField, Min(0f)] private float powerUpBlinkStartRemainingTime = 3f; // PowerUp残り時間何秒から点滅を開始するか
    [SerializeField, Min(0.01f)] private float powerUpBlinkInterval = 0.2f; // 表示と非表示を切り替える間（PowerUp終了前）
    [SerializeField, Range(0f, 1f)] private float powerUpBlinkAlphaMultiplier = 0.35f; // 点滅時に元のAlphaへ掛ける倍率
    [SerializeField, Range(0f, 1f)] private float invincibleAlpha = 0.45f; // Invincible中のAlpha値
    [SerializeField] private Color enemyDestroyOutlineColor = new Color(0.55f, 0f, 1f, 1f); // EnemyDestroy中の輪郭の色
    [SerializeField, Range(0f, 1f)] private float enemyDestroyOutlineAlpha = 0.5f; // 輪郭の透明度
    [SerializeField, Min(1f)] private float enemyDestroyOutlineScaleMultiplier = 1.15f; // 拡大倍率

    //  [List : 複数のデータをまとめて保存するための入れ物]
    //  [Dictionaly : キーと値をセットで保存する入れ物]
    //  [readonly : この変数に別のListやDictionaryを再代入できないようにする]
    private readonly List<SpriteRenderer> currentRenderers = new List<SpriteRenderer>();
    private readonly List<SpriteRenderer> outlinesToRemove = new List<SpriteRenderer>();
    private readonly Dictionary<SpriteRenderer, SpriteRenderer> outlineRenderers = new Dictionary<SpriteRenderer, SpriteRenderer>();
    private Coroutine damageBlinkCoroutine; // 実行中のダメージ点滅Coroutineを記録
    private PowerUpType currentPowerUpType; // 現在のPowerUpの種類
    private float currentPowerUpRemainingTime; // 現在のPowerUpの残り時間
    private bool isDamageBlinking; // ダメージ点滅中かどうか
    private bool currentBlinkVisible = true; // ダメージ点滅の現在の表示状態
    private bool isPowerUpVisualActive; // PowerUpの視覚演出が有効かどうか


    // ----------初期化・イベント登録・解除----------

    private void Awake()
    {
        if (caterpillarBody == null)
        {
            caterpillarBody = GetComponentInParent<CaterpillarBody>();
        }

        if (colorModel == null)
        {
            colorModel = GetComponentInParent<PlayerColorModel>();
        }
    }

    // ＊色変更・体節追加イベントを登録
    private void OnEnable()
    {
        if (colorModel != null)
        {
            colorModel.OnColorChanged += ApplyColor;
        }

        if (caterpillarBody != null)
        {
            caterpillarBody.OnSegmentAdded += ApplyColorToSegment;
        }
    }

    // ＊ゲーム開始時の色を ApplyCurrentAppearance() で反映
    private void Start()
    {
        if (colorModel != null)
        {
            ApplyColor(colorModel.CurrentColor);
        }
    }

    // ＊点滅停止・演出解除・イベント登録解除
    private void OnDisable()
    {
        // 「点滅Coroutineが実行中なら停止する」
        if (damageBlinkCoroutine != null)
        {
            StopCoroutine(damageBlinkCoroutine);
            damageBlinkCoroutine = null;
        }

        isDamageBlinking = false; // ダメージ点滅状態を初期化
        currentBlinkVisible = true; // 表示状態をtrueにする
        ClearPowerUpVisual(); // PowerUp演出を解除
        SetPlayerVisible(true); // Playerを表示状態にする

        if (colorModel != null)
        {
            colorModel.OnColorChanged -= ApplyColor;
        }

        if (caterpillarBody != null)
        {
            caterpillarBody.OnSegmentAdded -= ApplyColorToSegment;
        }
    }


    // ----------色や体節の変更を受け取る----------

    // ＊色変更イベントを受けて見た目を更新
    private void ApplyColor(Color color)
    {
        ApplyCurrentAppearance();
    }

    // ＊体節追加イベントを受けて見た目を更新
    private void ApplyColorToSegment(CaterpillarSegment segment)
    {
        ApplyCurrentAppearance();
    }


    // ----------ダメージ点滅----------

    // ＊敵からダメージを受けたときに外部から呼び出される、ダメージ点滅を時間経過で実行する
    public void PlayDamageBlink()
    {
        // 「すでに点滅Coroutineが動いていれば停止する」
        if (damageBlinkCoroutine != null)
        {
            StopCoroutine(damageBlinkCoroutine);
        }

        // 「新しいCoroutineを開始する」
        damageBlinkCoroutine = StartCoroutine(DamageBlinkRoutine());
    }

    // ＊時間経過を伴う点滅処理
    private IEnumerator DamageBlinkRoutine()
    {
        float elapsedTime = 0f; // 経過時間を0にする
        isDamageBlinking = true; // ダメージ点滅中にする
        currentBlinkVisible = true; // 最初の表示状態をtrueにする

        // 「経過時間が点滅継続時間より短い間、処理を繰り返す」
        while (elapsedTime < damageBlinkDuration)
        {
            // 「true → false → true → false → true というように切り替わる」
            currentBlinkVisible = !currentBlinkVisible;
            // 「表示状態を頭と体節に反映させる」
            ApplyCurrentAppearance();

            // 「ここでCoroutineを一時停止し、damageBlinkInterval 秒待つ」
            yield return new WaitForSeconds(damageBlinkInterval);
            // 「経過時間を増やす」
            elapsedTime += damageBlinkInterval;
        }
        
        // 点滅が終了

        isDamageBlinking = false; // ダメージ点滅の状態を無効にする
        currentBlinkVisible = true; // 最初の表示状態をtrueにする
        ApplyCurrentAppearance(); // 表示状態を頭と体節に反映
        damageBlinkCoroutine = null; // Coroutine を削除する
    }


    // ----------パワーアップ演出----------

    // ＊PowerUpの種類と残り時間を受け取る
    public void ApplyPowerUpVisual(PowerUpType powerUpType, float remainingTime)
    {
        // 「PowerUp演出を有効状態にする」
        isPowerUpVisualActive = true;
        // 「PowerUpの種類と残り時間を保存」
        currentPowerUpType = powerUpType;
        currentPowerUpRemainingTime = remainingTime;
        // 「ApplyCurrentAppearance() で反映させる」
        ApplyCurrentAppearance();
    }

    // ＊PowerUp演出を解除する
    public void ClearPowerUpVisual()
    {
        // 「PowerUp演出を無効状態にする」
        isPowerUpVisualActive = false;
        // 「残り時間を0に戻す」
        currentPowerUpRemainingTime = 0f;
        // 「ApplyCurrentAppearance() で見た目を元に戻す」
        ApplyCurrentAppearance();
        // 「RemoveAllOutlines() で輪郭を全て削除する」
        RemoveAllOutlines();
    }


    // ----------見た目の反映----------

    // ＊現在の見た目（頭・体節の色、Alpha、表示状態、輪郭）をまとめて更新
    private void ApplyCurrentAppearance()
    {
        // 「BuildCurrentRenderers() で頭と体節のSpriteRendererを一覧にまとめる」
        BuildCurrentRenderers();
        // 「GetCurrentAlpha() で現在の透明度を計算して取得する」
        float alpha = GetCurrentAlpha();

        // 「currentRenderers に保存されているSpriteRendererを、1つずつ取り出して処理する」
        foreach (SpriteRenderer spriteRenderer in currentRenderers)
        {
            // 「SpriteRenderer が存在しない場合はスキップして次の処理へ進む」
            if (spriteRenderer == null)
            {
                continue;
            }

            // 「colorModel が存在するなら、 colorModel.CurrentColor を使い、」
            // 「存在しないなら、 SpriteRenderer の現在の色を使う」
            Color color = colorModel != null ? colorModel.CurrentColor : spriteRenderer.color;
            // 「Color の Alpha だけを変更」
            color.a = alpha;
            // 「ダメージ点滅中ではない、または、現在の点滅状態が表示中なら SpriteRenderer を有効にする」
            spriteRenderer.color = color;
            spriteRenderer.enabled = !isDamageBlinking || currentBlinkVisible;
        }

        // 「PowerUp演出が有効で、現在の種類がEnemyDestroyなら true を ApplyEnemyDestroyOutline へ渡す」
        ApplyEnemyDestroyOutline(isPowerUpVisualActive && currentPowerUpType == PowerUpType.EnemyDestroy);
    }

    // ＊無効化時に呼ばれ、頭と体節の表示・非表示を切り替える
    private void SetPlayerVisible(bool isVisible)
    {
        // 「BuildCurrentRenderers() で頭と体節のSpriteRendererを一覧にまとめる」
        BuildCurrentRenderers();

        // 「currentRenderers の中にあるSpriteRendererを1つずつ取り出して処理をする」
        foreach (SpriteRenderer spriteRenderer in currentRenderers)
        {
            // 「SpriteRenderer が存在していれば、enabled に isVisible を代入する」
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = isVisible;
            }
        }
    }


    // ----------頭と体節を一覧にする----------

    // ＊頭と体節のSpriteRendererを一覧にまとめる
    private void BuildCurrentRenderers()
    {
        currentRenderers.Clear(); // まず一覧を空にする
        AddRenderer(headRenderer); // 頭のSpriteRendererを追加する

        if (caterpillarBody == null)
        {
            return;
        }

        // 「SpriteRendererが存在する場合だけ、1つずつListへ追加する」
        foreach (CaterpillarSegment segment in caterpillarBody.Segments)
        {
            if (segment != null)
            {
                AddRenderer(segment.SpriteRenderer);
            }
        }
    }

    // ＊SpriteRendererを一覧へ追加
    private void AddRenderer(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer != null)
        {
            currentRenderers.Add(spriteRenderer);
        }
    }

    // ----------透明度の計算----------

    // ＊現在の透明度を計算
    private float GetCurrentAlpha()
    {
        // 「PowerUpがInvincibleなら、invincibleAlpha、それ以外なら 1f を使用する」
        float alpha = isPowerUpVisualActive && currentPowerUpType == PowerUpType.Invincible ? invincibleAlpha : 1f;

        // 「PowerUpの残り時間が3秒以下、かつ、ShouldUsePowerUpBlinkAlpha() がtrueなら、」
        if (isPowerUpVisualActive
            && currentPowerUpRemainingTime <= powerUpBlinkStartRemainingTime
            && ShouldUsePowerUpBlinkAlpha())
        {
            // 「Alphaに倍率を掛ける」
            alpha *= powerUpBlinkAlphaMultiplier;
        }

        return alpha;
    }

    // ＊今がPowerUp点滅の暗いタイミングか判定
    private bool ShouldUsePowerUpBlinkAlpha()
    {
        // 「残り時間を0.2秒で区切り、偶数と奇数でAlphaを切り替える」
        //  [Mathf.FloorToInt : 小数を切り捨てて整数にするメソッド]
        //  [% : 割り算の余りを求める演算子]
        int blinkStep = Mathf.FloorToInt(currentPowerUpRemainingTime / powerUpBlinkInterval);
        return blinkStep % 2 == 0;
    }


    // ----------輪郭の管理----------

    // ＊紫色の輪郭を表示・更新する
    private void ApplyEnemyDestroyOutline(bool shouldShow)
    {
        // 「輪郭を表示しない場合、輪郭をすべて削除し、終了」
        if (!shouldShow)
        {
            RemoveAllOutlines();
            return;
        }

        // 「頭と体節のSpriteRendererを順番に取得して処理をする」
        foreach (SpriteRenderer spriteRenderer in currentRenderers)
        {
            if (spriteRenderer == null)
            {
                continue;
            }

            // 「GetOrCreateOutline() で輪郭を取得、または新しく生成する」
            SpriteRenderer outlineRenderer = GetOrCreateOutline(spriteRenderer);

            outlineRenderer.sprite = spriteRenderer.sprite; // 輪郭に元のSpriteを設定
            outlineRenderer.sortingLayerID = spriteRenderer.sortingLayerID; // どのSorting Layerに所属するか
            outlineRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // 同じSorting Layer内での描画順
            outlineRenderer.transform.localScale = Vector3.one * enemyDestroyOutlineScaleMultiplier; // 輪郭を拡大
           
            Color outlineColor = enemyDestroyOutlineColor; // Inspectorで設定した輪郭色を変数へコピー
            outlineColor.a = enemyDestroyOutlineAlpha; // Alphaを設定
            outlineRenderer.color = outlineColor; // 実際の輪郭用SpriteRendererへ色を反映
            outlineRenderer.enabled = spriteRenderer.enabled; // 輪郭の表示状態を合わせる
        }

        // 「不要な輪郭を削除」
        RemoveUnusedOutlines();
    }

    // ＊輪郭を取得または生成する
    private SpriteRenderer GetOrCreateOutline(SpriteRenderer sourceRenderer)
    {
        // 「sourceRenderer をキーとしてDictionaryを検索し、
        //   Dictionaryに登録されていて、なおかつ取得した輪郭が存在する場合、
        //   outlineRenderer に取り出す」
        if (outlineRenderers.TryGetValue(sourceRenderer, out SpriteRenderer outlineRenderer)
            && outlineRenderer != null)
        {
            // 「既存の輪郭を返し、メソッドを終了」
            return outlineRenderer;
        }

        // 既存の輪郭が見つからなかった場合

        // 「新しいGameObjectを生成する」
        GameObject outlineObject = new GameObject("PowerUpOutline");

        //  [SetParent : GameObjectの親子関係を設定するメソッド]
        outlineObject.transform.SetParent(sourceRenderer.transform, false); // 元のSpriteRendererの子にする
        outlineObject.transform.localPosition = Vector3.zero; // 親から見た位置を0にする
        outlineObject.transform.localRotation = Quaternion.identity; // 親から見た回転をなしにする
        outlineObject.transform.localScale = Vector3.one * enemyDestroyOutlineScaleMultiplier; // 親から見た大きさを適用

        //  [AddComponent<SpriteRenderer>() : GameObject に SpriteRenderer コンポーネントを追加するメソッド]
        outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        // 「元の SpriteRenderer をキーにして、輪郭 SpriteRenderer を Dictionary へ保存する
        outlineRenderers[sourceRenderer] = outlineRenderer;
        // 「生成した輪郭を返す」
        return outlineRenderer;
    }

    // ＊不要になった体節の輪郭を削除
    private void RemoveUnusedOutlines()
    {
        // 「削除対象のSpriteRendererを一時的に保存するListの中身をすべて削除するメソッド」
        outlinesToRemove.Clear();

        foreach (KeyValuePair<SpriteRenderer, SpriteRenderer> outlinePair in outlineRenderers)
        {
            if (outlinePair.Key == null || !currentRenderers.Contains(outlinePair.Key))
            {
                outlinesToRemove.Add(outlinePair.Key);
            }
        }

        foreach (SpriteRenderer sourceRenderer in outlinesToRemove)
        {
            if (outlineRenderers.TryGetValue(sourceRenderer, out SpriteRenderer outlineRenderer)
                && outlineRenderer != null)
            {
                Destroy(outlineRenderer.gameObject);
            }

            outlineRenderers.Remove(sourceRenderer);
        }
    }

    // ＊すべての輪郭を削除
    private void RemoveAllOutlines()
    {
        foreach (KeyValuePair<SpriteRenderer, SpriteRenderer> outlinePair in outlineRenderers)
        {
            if (outlinePair.Value != null)
            {
                Destroy(outlinePair.Value.gameObject);
            }
        }

        outlineRenderers.Clear();
    }


    // -----------Inspectorの値の補正----------

    // ＊Inspectorの値を補正
    //  [OnValidate() : UnityEditorで Inspector の値を変更したときなどに呼び出されるメソッド]
    private void OnValidate()
    {
        // 「powerUpBlinkInterval が0.01未満にならないように補正する
　　　  //   ※点滅間隔が0になってしまうことを防ぐため」
        powerUpBlinkInterval = Mathf.Max(0.01f, powerUpBlinkInterval);
    }
}
