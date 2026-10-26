using UnityEngine;

/// <summary>
/// プレイヤーのHeadより少し上を目標地点にして、カメラのY座標だけを滑らかに追従させるクラス
/// </summary>
public sealed class VerticalCameraFollow : MonoBehaviour
{
    // 「Transform型　playerHead.position からHeadの現在位置を取得できる」
    [SerializeField] private Transform playerHead;
    // 「verticalOffset をプラスするとCameraの中心がHeadより上に来る」
    [SerializeField] private float verticalOffset = 1.5f;
    // 「smoothTimeというfloat変数　初期値は0.2、最低値は0.01」
    [SerializeField, Min(0.01f)] private float smoothTime = 0.2f;

    // 「滑らかさを保つために Mathf.SmoothDamp() がどれくらいの速度でY方向へ動いているかを覚えておく変数」
    private float currentVelocityY;
    // 「ゲーム開始時のCameraのX・Z座標を維持」
    private float initialCameraX;
    private float initialCameraZ;

    private void Awake()
    {
        if (playerHead == null)
        {
            Debug.LogError("[VerticalCameraFollow] Player Head is not assigned.", this);
            enabled = false;
            return;
        }

        // [transform.position : このスクリプトが付いているGameObjectの現在位置]
        // 「currentPosition に、transform.position のXYZ座標を入れる」
        Vector3 currentPosition = transform.position;
        initialCameraX = currentPosition.x;
        initialCameraZ = currentPosition.z;
    }

    // ※LateUpdate() を使う理由は、Playerの移動処理が終わった後にCameraを動かすため。
    // 　移動とカメラ更新の順番が安定しやすくなり、ガタつきが出にくくなる。
    private void LateUpdate()
    {
        // 「verticalOffset をプラスするとCameraの中心がHeadより上に来る」
        float targetCameraY = playerHead.position.y + verticalOffset;
        float currentCameraY = transform.position.y;
        // [Mathf.SmoothDamp() : 現在値から目標値へなめらかに近づける関数]
        // [ref : この変数そのものを渡して、中身を書き換えてもらう]
        // [currentVelocityY : 滑らかさを保つために内部速度を覚えておく変数]
        // 「現在位置から目標位置へ滑らかに近づけた、次のY座標を計算する」
        float nextCameraY = Mathf.SmoothDamp(
            currentCameraY, // 「今どこにいるか」
            targetCameraY, // 「どこへ行きたいか」
            ref currentVelocityY, // 「今どのくらいの速度で動いているか」
            smoothTime // 「どのくらい滑らかに追従するか」
        );

        transform.position = new Vector3(initialCameraX, nextCameraY, initialCameraZ);
    }
}
