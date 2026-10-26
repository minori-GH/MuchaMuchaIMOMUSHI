using UnityEngine;

/// <summary>
/// スワイプやキーボード入力を移動方向に変換するスクリプト
/// </summary>
public sealed class SwipeInputReader : MonoBehaviour
{
    // 「最低何ピクセル動いたらスワイプと認めるか」
    [SerializeField] private float minimumSwipeDistance = 50f;

    // 「指を置いた場所を保存」
    private Vector2 touchStartPosition;
    // 「触っているかどうかを判定」
    private bool isTouching;
　　// [out:結果を書きこむ箱]
    public bool TryGetDirection(out Vector2Int direction)
    {
        // 「方向なし」
        direction = Vector2Int.zero;


        // 「キーボード入力があればその方向を返して終了」
#if UNITY_EDITOR
        if (TryGetKeyboardDirection(out direction))
        {
            return true;
        }
#endif

        return TryGetSwipeDirection(out direction);
    }

#if UNITY_EDITOR
    // *キーボード入力の設定
    private bool TryGetKeyboardDirection(out Vector2Int direction)
    {
        // 「方向を初期化」
        direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow)) // ↑
        {
            direction = Vector2Int.up;
            return true;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) // ↓
        {
            direction = Vector2Int.down;
            return true;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow)) // ←
        {
            direction = Vector2Int.left;
            return true;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow)) // →
        {
            direction = Vector2Int.right;
            return true;
        }

        return false;
    }
#endif

    // *スワイプ入力の設定
    private bool TryGetSwipeDirection(out Vector2Int direction)
    {
        // 「方向を初期化」
        direction = Vector2Int.zero;

        // 「画面を触っている指の本数が0本の場合はそこで終了」
        if (Input.touchCount == 0)
        {
            isTouching = false;
            return false;
        }

        // 「1本目の指の情報を取得」
        Touch touch = Input.GetTouch(0);

        // 「触り始めた瞬間」
        if (touch.phase == TouchPhase.Began)
        {
            // 「タッチした場所を記録」
            touchStartPosition = touch.position;
            isTouching = true;
            return false;
        }

        if (!isTouching)
        {
            return false;
        }

        // 「まだスワイプ途中」
        if (touch.phase != TouchPhase.Ended　   //「離した状態でない」
            &&                                  //「かつ」
            touch.phase != TouchPhase.Canceled) //「キャンセル状態でもない」
        {
            return false;
        }

        // 「スワイプ距離(swipeDelta)はtouch.position - touchStartPosition」
        isTouching = false;
        Vector2 swipeDelta = touch.position - touchStartPosition;

        // [magnitude:ベクトルの長さ]
        if (swipeDelta.magnitude < minimumSwipeDistance)
        {
            // 「ミニマム距離よりも短ければスワイプと判定しない」
            return false;
        }

        // *横方向の距離と縦方向の距離のどっちが長いか調べている
        // [Mathf.Abs:絶対値]（-100→100）
        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            // [(三項演算子)条件 ? A : B　:条件がtrueならA、条件がfalseならB](if文を短くしたやつ)
            // 「横の方が大きい場合、右なら→、左なら←」
            direction = swipeDelta.x > 0f // 「条件」 
            ? Vector2Int.right 　　　　　 // 「条件がtrue(?)なら→」
            : Vector2Int.left;            // 「条件がfalse(:)なら←」
        }
        else
        {
            direction = swipeDelta.y > 0f ? Vector2Int.up : Vector2Int.down;
        }

        return true;
    }
}
