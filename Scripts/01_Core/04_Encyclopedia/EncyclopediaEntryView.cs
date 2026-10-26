using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ButterflyDexEntry を1匹受け取って、PagePannel内に表示する
/// </summary>
public sealed class EncyclopediaEntryView : MonoBehaviour
{
    [SerializeField] private Image butterflyImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text createdDateText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private ButterflyDescriptionData descriptionData;

    [SerializeField] private Sprite smallRedSprite;
    [SerializeField] private Sprite smallGreenSprite;
    [SerializeField] private Sprite smallBlueSprite;

    [SerializeField] private Sprite mediumRedSprite;
    [SerializeField] private Sprite mediumGreenSprite;
    [SerializeField] private Sprite mediumBlueSprite;

    [SerializeField] private Sprite largeRedSprite;
    [SerializeField] private Sprite largeGreenSprite;
    [SerializeField] private Sprite largeBlueSprite;

    private void Awake()
    {
        if (butterflyImage == null)
        {
            Debug.LogError("[EncyclopediaEntryView] Butterfly Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (nameText == null)
        {
            Debug.LogError("[EncyclopediaEntryView] Name Text is not assigned.", this);
            enabled = false;
            return;
        }

        if (createdDateText == null)
        {
            Debug.LogError("[EncyclopediaEntryView] Created Date Text is not assigned.", this);
            enabled = false;
            return;
        }

        if (descriptionText == null)
        {
            Debug.LogError("[EncyclopediaEntryView] Description Text is not assigned.", this);
            enabled = false;
            return;
        }

        if (descriptionData == null)
        {
            Debug.LogError("[EncyclopediaEntryView] Description Data is not assigned.", this);
            enabled = false;
            return;
        }
    }

    // 「外部から渡された蝶1匹を表示する」
    public void Show(ButterflyDexEntry entry)
    {
        if (entry == null)
        {
            Debug.LogError("[EncyclopediaEntryView] ButterflyDexEntry is null.", this);
            return;
        }

        // 「名前・日付を表示」
        nameText.text = entry.Name;
        createdDateText.text = entry.CreatedDate;
        descriptionText.text = descriptionData.GetDescription(entry.SizeType, entry.ColorType);
        // 「GetSprite でSpriteを選んで表示」
        Sprite selectedSprite = GetSprite(entry.SizeType, entry.ColorType);

        if (selectedSprite == null)
        {
            Debug.LogError(
                $"[EncyclopediaEntryView] Sprite is not assigned. Size: {entry.SizeType}, ColorType: {entry.ColorType}",
                this
            );
            // 「蝶画像が未設定の場合は、蝶画像だけ非表示にする」
            butterflyImage.gameObject.SetActive(false);
            return;
        }

        // 「スプライトと色を反映し表示させる」
        butterflyImage.sprite = selectedSprite;
        butterflyImage.color = entry.Color;
        butterflyImage.gameObject.SetActive(true);
    }

    // 「SizeTypeとColorTypeの組み合わせからSpriteを返すメソッド」
    private Sprite GetSprite(ButterflySizeType sizeType, ButterflyColorType colorType)
    {
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

            _ => null
        };
    }
}

