// [using:他の機能を使えるようにする宣言]
// 「UnityEngineを使えるようにするよ」
using UnityEngine;

/// <summary>
/// SwipeInputReader からの入力を受け取って PlayerMovement に指示するスクリプト
/// </summary>



// *属性(Attribute)
// 「このスクリプトには必ずこのコンポーネントが必要」
[RequireComponent(typeof(SwipeInputReader))]
[RequireComponent(typeof(PlayerMovement))]

// [public:公開する（他のスクリプトから使える）]
// [class:設計図（〇〇の設計図をつくる）]
// [sealed:拡張禁止※大規模プロジェクトでは必要かもだけど個人開発レベルではいらないらしい]
public sealed class PlayerController : MonoBehaviour
{
    // [private:このクラスだけで使える]
    // [[SerializeField]:privateをインスペクター上で操作できる]
    // [型（何を入れられる箱か）＋変数名（その箱の名前）:]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private SwipeInputReader inputReader;
    [SerializeField] private PlayerMovement movement;

    // [void:戻り値なし（処理するだけ）]
    // [Awake:ゲーム開始時に最初に呼ばれる関数]
    private void Awake()
    {

        // [if (A == null):もしAが空なら]
        if (inputReader == null)
        {
            // [GetComponent:同じGameObjectについているコンポーネントを取得してアタッチする]
            // 「inputReadeに同じGameObjectについているSwipeInputReaderを探してアタッチする」
            inputReader = GetComponent<SwipeInputReader>();
        }

        if (movement == null)
        {
            movement = GetComponent<PlayerMovement>();
        }

        if (gameManager == null)
        {
            Debug.LogError("[PlayerController] GameManager is not assigned.", this);
            enabled = false;
            return;
        }
    }

    // [Update:毎フレーム呼ばれる（30FPSなら1秒間に30回）]
    private void Update()
    {
        if (gameManager.CurrentState != GameState.Playing
            && gameManager.CurrentState != GameState.WaitingToStart)
        {
            return;
        }

        if (!inputReader.TryGetDirection(out Vector2Int direction))
        {
            return;
        }

        if (gameManager.CurrentState == GameState.WaitingToStart)
        {
            gameManager.SetState(GameState.Playing);
        }

        movement.SetRequestedDirection(direction);
    }
}
