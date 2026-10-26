using UnityEngine;

// ***プレイヤーの移動範囲のチェック
public sealed class MovementCollisionChecker : MonoBehaviour
{
    // 「壁として認識するかレイヤー(LayerMask)を設定」
    [SerializeField] private LayerMask wallLayerMask;
    // 「壁判定に使う四角形の大きさ」
    [SerializeField] private Vector2 checkBoxSize = new Vector2(0.8f, 0.8f);

    // 「座標から移動可能かtrueかfalseで返す」
    public bool CanMoveTo(Vector2 targetPosition)
    {
        // [Physics2D.OverlapBox:四角形を置いて何かに当たっているか調べる関数]
            Collider2D wall = Physics2D.OverlapBox(
            targetPosition,　// 「どこで判定するか」
            checkBoxSize, // 「チェックボックスのサイズ」
            0f, // 「回転角度（0fなので回転なし）
            wallLayerMask // 「どのレイヤーを壁として調べるか」
        );

        // 「壁がないなら移動できる(true)」
        return wall == null;
    }

    // 「Sceneビューで選択した時だけ表示する」
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, checkBoxSize);
    }
}
