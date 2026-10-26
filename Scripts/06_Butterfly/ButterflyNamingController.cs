using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 1．蝶の登場アニメーションが終わったあとに命名UIを表示する
/// 2．プレイヤーが入力した名前と蝶の情報を図鑑へ保存して、StartSceneへ戻す
/// </summary>

public sealed class ButterflyNamingController : MonoBehaviour
{
    [SerializeField] private ButterflyEvolutionController butterflyEvolutionController;
    [SerializeField] private ButterflyVisual butterflyVisual;
    [SerializeField] private GameObject namingUiRoot;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button registerButton;
    [SerializeField] private string startSceneName = "StartScene";
    [SerializeField, Min(0f)] private float showDelayAfterButterflyAppears = 0.3f; // 蝶の登場完了後、UI表示まで待つ秒数

    // 「保存処理を担当するSaveManagerのインスタンスを作り、このクラスで使用する」
    private readonly SaveManager saveManager = new SaveManager();
    // 「Coroutineを保存する変数」
    private Coroutine showNamingUiCoroutine;

    private void Awake()
    {
        if (butterflyEvolutionController == null)
        {
            Debug.LogError("[ButterflyNamingController] ButterflyEvolutionController is not assigned.", this);
            enabled = false;
            return;
        }

        if (butterflyVisual == null)
        {
            Debug.LogError("[ButterflyNamingController] ButterflyVisual is not assigned.", this);
            enabled = false;
            return;
        }

        if (namingUiRoot == null)
        {
            Debug.LogError("[ButterflyNamingController] Naming UI Root is not assigned.", this);
            enabled = false;
            return;
        }

        if (nameInputField == null)
        {
            Debug.LogError("[ButterflyNamingController] Name Input Field is not assigned.", this);
            enabled = false;
            return;
        }

        if (registerButton == null)
        {
            Debug.LogError("[ButterflyNamingController] Register Button is not assigned.", this);
            enabled = false;
            return;
        }

        // 「InspectorでStartSceneの名前を消してしまった場合などに、シーン遷移でエラーになる前に検知する」
        if (string.IsNullOrWhiteSpace(startSceneName))
        {
            Debug.LogError("[ButterflyNamingController] Start scene name is empty.", this);
            enabled = false;
            return;
        }

        // 「命名UIを非表示にする」
        namingUiRoot.SetActive(false);
    }

    // ＊2種類のイベントにメソッドを登録
    private void OnEnable()
    {
        // ＊ButterflyVisualの登場完了→HandleButterflyAppearAnimationCompletedを実行
        if (butterflyVisual != null)
        {
            butterflyVisual.OnAppearAnimationCompleted += HandleButterflyAppearAnimationCompleted;
        }

        // ＊登録ボタンクリック→HandleRegisterButtonClickedを実行
        if (registerButton != null)
        {
            //  [AddListener(...) : そのイベントが起きたときに実行するメソッドを登録する]
            //  ※今回は、Unityの Button.onClick というイベントを登録するため AddListener を使う
            //  ※+= は、自分たちで作ったC#のイベントを登録するときに使う
            registerButton.onClick.AddListener(HandleRegisterButtonClicked);
        }
    }

    // ＊イベント解除、Coroutine停止
    private void OnDisable()
    {
        if (butterflyVisual != null)
        {
            butterflyVisual.OnAppearAnimationCompleted -= HandleButterflyAppearAnimationCompleted;
        }

        if (registerButton != null)
        {
            // [RemoveListener(...) : 登録を解除する]
            registerButton.onClick.RemoveListener(HandleRegisterButtonClicked);
        }

        // 「Coroutineへの参照が保存されていれば、」
        if (showNamingUiCoroutine != null)
        {
            // 「Coroutineを停止し、」
            StopCoroutine(showNamingUiCoroutine);
            // 「参照をクリアする」
            showNamingUiCoroutine = null;
        }
    }

    // ＊命名UI表示のCoroutineを開始
    private void HandleButterflyAppearAnimationCompleted()
    {
        // 「すでに待機Coroutineがあれば停止する」
        if (showNamingUiCoroutine != null)
        {
            StopCoroutine(showNamingUiCoroutine);
        }

        // 「新しいCoroutineを開始」
        showNamingUiCoroutine = StartCoroutine(ShowNamingUiAfterDelay());
    }

    // ＊少し待ってから命名UIを表示させる
    private IEnumerator ShowNamingUiAfterDelay()
    {
        // 「経過時間の初期化」
        float elapsedTime = 0f;

        // 「条件がtrueの間、繰り返す」
        while (elapsedTime < showDelayAfterButterflyAppears)
        {
            // 「毎フレーム経過時間を加算」
            elapsedTime += Time.deltaTime;
            // 「次のフレームまで待つ」
            yield return null;
        }

        // 「命名UIのObjectを有効にし表示させる」
        namingUiRoot.SetActive(true);
        // 「Coroutineの参照をクリア」
        showNamingUiCoroutine = null;
    }

    // ＊登録ボタン押下時、名前と蝶のデータを図鑑に保存する
    private void HandleRegisterButtonClicked()
    {

        // 「入力された名前を取得する」
        //  [.Trim() : 文字列の前後の空白を削除するメソッド]
        string butterflyName = nameInputField.text.Trim();

        //  [string.IsNullOrEmpty() : その文字列が null または空文字かを調べる]
        if (string.IsNullOrEmpty(butterflyName))
        {
            Debug.LogWarning("[ButterflyNamingController] Butterfly name is empty.", this);
            return;
        }

        // 「butterflyEvolutionController から CurrentButterfly を取得」
        ButterflyData butterflyData = butterflyEvolutionController.CurrentButterfly;

        if (butterflyData == null)
        {
            Debug.LogError("[ButterflyNamingController] CurrentButterfly is null.", this);
            return;
        }

        // 「登録日を取得する」
        //  [DateTime : C#に用意されている日付や時刻を扱うための型]
        //  [Now : 現在の日付と時刻を取得する]
        //  [ToString() : 日付を文字列に変換する]
        string createdDate = DateTime.Now.ToString("yyyy-MM-dd");

        // 「新しい ButterflyDexEntry のインスタンスを作る」
        ButterflyDexEntry entry = new ButterflyDexEntry(
            butterflyName,
            butterflyData.SizeType,
            butterflyData.ColorType,
            butterflyData.Color,
            createdDate
        );

        Debug.Log(
            $"[ButterflyNamingController] ButterflyDexEntry created. Name: {entry.Name}, Size: {entry.SizeType}, ColorType: {entry.ColorType}, CreatedDate: {entry.CreatedDate}",
            this
        );

        // 「完成した蝶を図鑑に保存する」
        saveManager.AddButterflyDexEntry(entry);
        // 「StartSceneを読み込む」
        SceneManager.LoadScene(startSceneName);
    }
}
