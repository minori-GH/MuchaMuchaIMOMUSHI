using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// GameScene で Clear を検知し、結果を保存して ClearScene へフェードで遷移する
/// </summary>

public sealed class GameClearTransitionController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private CaterpillarBody caterpillarBody;
    [SerializeField] private PlayerColorModel playerColorModel;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Image whiteFadeImage;
    [SerializeField] private string clearSceneName = "ClearScene";
    [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

    private bool isTransitioning;

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError("[GameClearTransitionController] GameManager is not assigned.", this);
            enabled = false;
            return;
        }

        if (caterpillarBody == null)
        {
            Debug.LogError("[GameClearTransitionController] CaterpillarBody is not assigned.", this);
            enabled = false;
            return;
        }

        if (playerColorModel == null)
        {
            Debug.LogError("[GameClearTransitionController] PlayerColorModel is not assigned.", this);
            enabled = false;
            return;
        }

        if (playerMovement == null)
        {
            Debug.LogError("[GameClearTransitionController] PlayerMovement is not assigned.", this);
            enabled = false;
            return;
        }

        if (whiteFadeImage == null)
        {
            Debug.LogError("[GameClearTransitionController] White Fade Image is not assigned.", this);
            enabled = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(clearSceneName))
        {
            Debug.LogError("[GameClearTransitionController] Clear scene name is empty.", this);
            enabled = false;
            return;
        }

        SetWhiteFadeAlpha(0f);

    }

    // [OnEnable :このコンポーネントが有効になったときに呼ばれる]
    private void OnEnable()
    {
        if (gameManager != null)
        {
            // 「GameManager の状態が変わったら、 HandleGameStateChanged を呼ぶ」
            gameManager.OnStateChanged += HandleGameStateChanged;
        }
    }

    // [OnDisable :このコンポーネントが無効になったときに呼ばれる]
    private void OnDisable()
    {
        if (gameManager != null)
        {
            // 「登録したイベントを解除する」
            gameManager.OnStateChanged -= HandleGameStateChanged;
        }
    }

    // ＊GameManager の状態が変更されたとき、その新しい状態を受け取るメソッド
    private void HandleGameStateChanged(GameState state)
    {
        // 「ゲーム状態がClearではない、または、すでにシーン遷移中なら何もしない」
        if (state != GameState.Clear || isTransitioning)
        {
            return;
        }

        // 「Scene 遷移処理状態にする」
        isTransitioning = true;

        // 「GameResultDataクラスの新しいインスタンスを作成する」
        GameResultData resultData = new GameResultData(
            caterpillarBody.CurrentSegmentCount, // 体節数
            playerColorModel.TotalRed, // 赤色累計値
            playerColorModel.TotalGreen, // 緑色累計値
            playerColorModel.TotalBlue, // 青色累計値
            playerColorModel.CurrentColor // 最終的なプレイヤーの色
        );

        // 「Playerと全Enemyを永久停止させる」
        playerMovement.StopPermanently();
        EnemyMovement.StopAllPermanently();

        // 「GameResultHolder に↑で保存した resultData を預ける」
        GameResultHolder.SetResult(resultData);

        // 「FadeOutAndLoadClearScene という Coroutine を開始する」
        //  [Coroutine : 処理の途中で一時中断し、後のフレームから再開できる仕組み]
        StartCoroutine(FadeOutAndLoadClearScene());
    }

    // ＊白フェードでシーンを切り替える
    //  [IEnumerator : Coroutine で使える戻り値の型]
    private IEnumerator FadeOutAndLoadClearScene()
    {
        // 「白フェード用のImageを有効にする」
        whiteFadeImage.gameObject.SetActive(true);

        // 「フェード時間が0以下の場合」
        if (fadeDuration <= 0f)
        {
            // 「一瞬で真っ白にする」
            SetWhiteFadeAlpha(1f);
            // 「ClearSceneを読み込む」
            SceneManager.LoadScene(clearSceneName);
            //  [yield break; : このCoroutineをここで終了する]
            yield break;
        }

        // 「フェード開始から何秒経過したかを記録する変数」
        float elapsedTime = 0f;

        // 「経過時間がフェード時間より短い間、繰り返す。」
        //  [while : 条件がtrueの間、繰り返す]
        while (elapsedTime < fadeDuration)
        {
            // 「1フレームごとに経過時間を加算する」
            elapsedTime += Time.deltaTime;
            // 「経過時間を0〜1の進行度に変換する」
            //  [Mathf.Clamp01(...) : 値を0〜1の範囲に制限する]
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            // 「白いImageのAlphaを t にする」
            SetWhiteFadeAlpha(t);
            // 「次のフレームまで処理を中断する」
            yield return null;
        }

        // 「while が終了したら、最後にAlphaを確実に1へ設定する」
        SetWhiteFadeAlpha(1f);

        // 「ClearScene を読み込む」
        SceneManager.LoadScene(clearSceneName);
    }

    // ----------フェード色の設定など----------

    // ＊透明度の変更
    private void SetWhiteFadeAlpha(float alpha)
    {
        // 「現在の色を取得する」
        Color color = whiteFadeImage.color;
        // 「colorの透明度を、引数で受け取ったalphaへ変更する」
        color.a = alpha;
        // 「変更したColorをImageへ設定する」
        whiteFadeImage.color = color;
    }
}

