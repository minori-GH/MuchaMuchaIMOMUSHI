using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EncyclopediaManagerに「この蝶タイプは発見済み？」と聞いて、
/// その結果に応じて9枚の画像を白または黒にするクラス
/// </summary>
public sealed class EncyclopediaCoverView : MonoBehaviour
{
    [SerializeField] private EncyclopediaManager encyclopediaManager;

    [SerializeField] private Image smallRedImage;
    [SerializeField] private Image smallGreenImage;
    [SerializeField] private Image smallBlueImage;

    [SerializeField] private Image mediumRedImage;
    [SerializeField] private Image mediumGreenImage;
    [SerializeField] private Image mediumBlueImage;

    [SerializeField] private Image largeRedImage;
    [SerializeField] private Image largeGreenImage;
    [SerializeField] private Image largeBlueImage;

    private void Awake()
    {
        if (encyclopediaManager == null)
        {
            Debug.LogError("[EncyclopediaCoverView] EncyclopediaManager is not assigned.", this);
            enabled = false;
            return;
        }

        if (smallRedImage == null)
        {
            Debug.LogError("[EncyclopediaCoverView] Small Red Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (smallGreenImage == null)
        {
            Debug.LogError("[EncyclopediaCoverView] Small Green Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (smallBlueImage == null)
        {
            Debug.LogError("[EncyclopediaCoverView] Small Blue Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (mediumRedImage == null)
        {
            Debug.LogError("[EncyclopediaCoverView] Medium Red Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (mediumGreenImage == null)
        {
            Debug.LogError("[EncyclopediaCoverView] Medium Green Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (mediumBlueImage == null)
        {
            Debug.LogError("[EncyclopediaCoverView] Medium Blue Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (largeRedImage == null)
        {
            Debug.LogError("[EncyclopediaCoverView] Large Red Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (largeGreenImage == null)
        {
            Debug.LogError("[EncyclopediaCoverView] Large Green Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (largeBlueImage == null)
        {
            Debug.LogError("[EncyclopediaCoverView] Large Blue Image is not assigned.", this);
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        if (encyclopediaManager == null)
        {
            return;
        }

        // 「EncyclopediaManager の OnEntriesLoaded が発生したら、RefreshCover() を実行する」
        encyclopediaManager.OnEntriesLoaded += RefreshCover;

        if (encyclopediaManager.IsLoaded)
        {
            RefreshCover();
        }
    }

    private void OnDisable()
    {
        if (encyclopediaManager != null)
        {
            encyclopediaManager.OnEntriesLoaded -= RefreshCover;
        }
    }

    private void RefreshCover()
    {
        ApplyDiscoveryState(smallRedImage, ButterflySizeType.Small, ButterflyColorType.Red);
        ApplyDiscoveryState(smallGreenImage, ButterflySizeType.Small, ButterflyColorType.Green);
        ApplyDiscoveryState(smallBlueImage, ButterflySizeType.Small, ButterflyColorType.Blue);

        ApplyDiscoveryState(mediumRedImage, ButterflySizeType.Medium, ButterflyColorType.Red);
        ApplyDiscoveryState(mediumGreenImage, ButterflySizeType.Medium, ButterflyColorType.Green);
        ApplyDiscoveryState(mediumBlueImage, ButterflySizeType.Medium, ButterflyColorType.Blue);

        ApplyDiscoveryState(largeRedImage, ButterflySizeType.Large, ButterflyColorType.Red);
        ApplyDiscoveryState(largeGreenImage, ButterflySizeType.Large, ButterflyColorType.Green);
        ApplyDiscoveryState(largeBlueImage, ButterflySizeType.Large, ButterflyColorType.Blue);
    }

    // 「発見状態を表示に反映するメソッド」
    private void ApplyDiscoveryState(
        Image image,
        ButterflySizeType sizeType,
        ButterflyColorType colorType)
    {
        // 「EncyclopediaManager に「このサイズと色は発見済み？」と聞く」
        bool isDiscovered = encyclopediaManager.HasDiscoveredButterflyType(sizeType, colorType);
        // [A ? B : C : 条件A ? trueの場合B : falseの場合C]
        image.color = isDiscovered ? Color.white : Color.black;
    }
}
