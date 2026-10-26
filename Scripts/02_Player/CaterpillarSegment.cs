using UnityEngine;

// ***イモムシの体節（1マス分の体）を操作するためのスクリプト
public sealed class CaterpillarSegment : MonoBehaviour
{
    // [SpriteRenderer:画像を保存する箱]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // 「segment.SpriteRendererと書くと、内部のSpriteRendererを取得する」
    public SpriteRenderer SpriteRenderer => spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    // *移動先の座標を受け取る
    public void SetFollowPosition(Vector2 position)
    {
        transform.position = position;
    }

    // *カラーを受け取って変更する
    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}
