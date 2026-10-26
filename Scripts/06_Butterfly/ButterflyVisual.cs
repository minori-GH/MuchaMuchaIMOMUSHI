using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ClearSceneで完成した蝶の画像と色を設定し、
/// 蝶が「ぱっ」と現れるアニメーションを実行するクラス
/// </summary>
/// 
public sealed class ButterflyVisual : MonoBehaviour
{
    [SerializeField] private ButterflyEvolutionController butterflyEvolutionController;
    
    // [Image : Canvas 上で Sprite を表示する場所]
    [SerializeField] private Image butterflyImage;

    // [Sprite : 画像素材そのもの]
    [SerializeField] private Sprite smallRedSprite;
    [SerializeField] private Sprite smallGreenSprite;
    [SerializeField] private Sprite smallBlueSprite;

    [SerializeField] private Sprite mediumRedSprite;
    [SerializeField] private Sprite mediumGreenSprite;
    [SerializeField] private Sprite mediumBlueSprite;

    [SerializeField] private Sprite largeRedSprite;
    [SerializeField] private Sprite largeGreenSprite;
    [SerializeField] private Sprite largeBlueSprite;

    // 「アニメーション用の設定値」
    [SerializeField, Min(0f)] private float appearDuration = 1f;
    [SerializeField] private Material whiteRevealMaterial;

    private static readonly int RevealAmountId = Shader.PropertyToID("_RevealAmount");
    private Material originalMaterial;
    private Material revealMaterialInstance;
    //  [Coroutine : StartCoroutine() が返す実行中 Coroutine への参照を保存する]
    private Coroutine appearCoroutine;

    // 「蝶の登場アニメーションが終わったことを、他のクラスへ通知するイベント」
    public event Action OnAppearAnimationCompleted;

    private void Awake()
    {
        if (butterflyEvolutionController == null)
        {
            Debug.LogError("[ButterflyVisual] ButterflyEvolutionController is not assigned.", this);
            enabled = false;
            return;
        }

        if (butterflyImage == null)
        {
            Debug.LogError("[ButterflyVisual] Butterfly Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (whiteRevealMaterial == null || !whiteRevealMaterial.HasProperty(RevealAmountId))
        {
            Debug.LogError("[ButterflyVisual] Assign a material using the UI/ButterflyWhiteReveal shader.", this);
            enabled = false;
            return;
        }

        originalMaterial = butterflyImage.material;
        revealMaterialInstance = new Material(whiteRevealMaterial);

        //  [SetActive(false) : GameObject を非表示・無効状態にする]
        // 「butterflyImage がついている gameObject を最初は非表示にする」
        butterflyImage.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (butterflyEvolutionController != null)
        {
            butterflyEvolutionController.OnButterflyCreated += Show;
        }
    }

    private void OnDisable()
    {
        if (butterflyEvolutionController != null)
        {
            butterflyEvolutionController.OnButterflyCreated -= Show;
        }

        if (appearCoroutine != null)
        {
            StopCoroutine(appearCoroutine);
            appearCoroutine = null;
        }

        if (butterflyImage != null && revealMaterialInstance != null)
        {
            butterflyImage.material = originalMaterial;
        }
    }

    private void OnDestroy()
    {
        if (revealMaterialInstance != null)
        {
            Destroy(revealMaterialInstance);
        }
    }

    // ＊ButterflyData を1匹分受け取り、蝶の画像・色を設定して表示させる
    public void Show(ButterflyData butterflyData)
    {
        if (butterflyData == null)
        {
            Debug.LogError("[ButterflyVisual] ButterflyData is null.", this);
            return;
        }

        // 「蝶のサイズと色の系統から表示するSpriteを GetSprite で取得」
        Sprite selectedSprite = GetSprite(butterflyData.SizeType, butterflyData.ColorType);

        if (selectedSprite == null)
        {
            Debug.LogError(
                $"[ButterflyVisual] Sprite is not assigned. Size: {butterflyData.SizeType}, ColorType: {butterflyData.ColorType}",
                this
            );
            return;
        }

        // 「蝶の画像と色を設定し、Imageを有効にする」
        butterflyImage.sprite = selectedSprite;
        butterflyImage.color = butterflyData.Color;
        butterflyImage.material = revealMaterialInstance;
        SetRevealAmount(0f);
        butterflyImage.gameObject.SetActive(true);

        // 「すでに登場アニメーションが実行されていたら、停止する」
        if (appearCoroutine != null)
        {
            StopCoroutine(appearCoroutine);
        }

        // 「新しいアニメーションの Coroutine を開始する
        appearCoroutine = StartCoroutine(PlayAppearAnimation());
    }

    // ＊登場アニメーション全体の進行管理
    private IEnumerator PlayAppearAnimation()
    {
        // 透明部分を保った白いシルエットから、完成した色と模様へ戻す。
        float elapsedTime = 0f;

        while (elapsedTime < appearDuration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / appearDuration);
            SetRevealAmount(t);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        // 最後は元の描画へ戻し、通常表示と完全に一致させる。
        butterflyImage.material = originalMaterial;
        appearCoroutine = null;
        OnAppearAnimationCompleted?.Invoke();
    }

    private void SetRevealAmount(float amount)
    {
        revealMaterialInstance.SetFloat(RevealAmountId, amount);
        // UI Maskで複製された描画用Materialにも同じ値を渡す。
        butterflyImage.materialForRendering.SetFloat(RevealAmountId, amount);
    }

    // 「処理した結果として Sprite を1つ返すメソッド」
    private Sprite GetSprite(ButterflySizeType sizeType, ButterflyColorType colorType)
    {
        // 「この後の switch で決まった Sprite を、呼び出し元に返す」
        return (sizeType, colorType) switch
        {
            (ButterflySizeType.Small, ButterflyColorType.Red) => smallRedSprite,
            (ButterflySizeType.Small, ButterflyColorType.Green) => smallGreenSprite,
            (ButterflySizeType.Small, ButterflyColorType.Blue) => smallBlueSprite,

            (ButterflySizeType.Medium, ButterflyColorType.Red) => mediumRedSprite,
            (ButterflySizeType.Medium, ButterflyColorType.Green) => mediumGreenSprite,
            (ButterflySizeType.Medium, ButterflyColorType.Blue) => mediumBlueSprite,

            (ButterflySizeType.Large, ButterflyColorType.Red) => largeRedSprite,
            (ButterflySizeType.Large, ButterflyColorType.Green) => largeGreenSprite,
            (ButterflySizeType.Large, ButterflyColorType.Blue) => largeBlueSprite,

            // [ _ :それ以外全部]
            _ => null
        };
    }
}
