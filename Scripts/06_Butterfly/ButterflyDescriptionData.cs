using UnityEngine;

[CreateAssetMenu(fileName = "ButterflyDescriptionData", menuName = "Caterpillar Game/Butterfly Descriptions")]
public sealed class ButterflyDescriptionData : ScriptableObject
{
    [SerializeField, TextArea(2, 4)] private string smallRedDescription = "¬‚³‚È‰H‚ÅAŒ³‹C‚¢‚Á‚Ï‚¢‚É•‘‚¤’±B";
    [SerializeField, TextArea(2, 4)] private string smallGreenDescription = "Žá—t‚Ì‚»‚Î‚ðAŒy‚â‚©‚É”ò‚Ñ‰ñ‚é’±B";
    [SerializeField, TextArea(2, 4)] private string smallBlueDescription = "¬‚³‚ÈÂ‚¢‰H‚ªA‚»‚Á‚Æ•—‚É—h‚ê‚é’±B";

    [SerializeField, TextArea(2, 4)] private string mediumRedDescription = "‚ ‚½‚½‚©‚ÈF‚Ì‰H‚ÅA‚Ì‚Ñ‚Ì‚Ñ‚Æ•‘‚¤’±B";
    [SerializeField, TextArea(2, 4)] private string mediumGreenDescription = "—Î‚Ì‰H‚ðL‚°A–Ø˜R‚ê“ú‚Ì’†‚ði‚Þ’±B";
    [SerializeField, TextArea(2, 4)] private string mediumBlueDescription = "Â‚¢‰H‚ð‚«‚ç‚ß‚©‚¹A‰¸‚â‚©‚É•‘‚¤’±B";

    [SerializeField, TextArea(2, 4)] private string largeRedDescription = "‘å‚«‚ÈÔ‚¢‰H‚ÅA“°X‚Æ‹ó‚ð•‘‚¤’±B";
    [SerializeField, TextArea(2, 4)] private string largeGreenDescription = "–L‚©‚È—Î‚Ì‰H‚ðL‚°A‚ä‚Á‚½‚è‚Æ”ò‚Ô’±B";
    [SerializeField, TextArea(2, 4)] private string largeBlueDescription = "‘å‚«‚ÈÂ‚¢‰H‚ÅA‹ó‚ð•ï‚Þ‚æ‚¤‚É•‘‚¤’±B";

    public string GetDescription(ButterflySizeType sizeType, ButterflyColorType colorType)
    {
        return (sizeType, colorType) switch
        {
            (ButterflySizeType.Small, ButterflyColorType.Red) => smallRedDescription,
            (ButterflySizeType.Small, ButterflyColorType.Green) => smallGreenDescription,
            (ButterflySizeType.Small, ButterflyColorType.Blue) => smallBlueDescription,

            (ButterflySizeType.Medium, ButterflyColorType.Red) => mediumRedDescription,
            (ButterflySizeType.Medium, ButterflyColorType.Green) => mediumGreenDescription,
            (ButterflySizeType.Medium, ButterflyColorType.Blue) => mediumBlueDescription,

            (ButterflySizeType.Large, ButterflyColorType.Red) => largeRedDescription,
            (ButterflySizeType.Large, ButterflyColorType.Green) => largeGreenDescription,
            (ButterflySizeType.Large, ButterflyColorType.Blue) => largeBlueDescription,

            _ => string.Empty
        };
    }
}
