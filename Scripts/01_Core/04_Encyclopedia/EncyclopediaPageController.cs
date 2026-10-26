using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CoverPannelとPagePannelの切り替え、およびページ番号の管理
/// </summary>
public sealed class EncyclopediaPageController : MonoBehaviour
{
    [SerializeField] private EncyclopediaManager encyclopediaManager;
    [SerializeField] private EncyclopediaEntryView encyclopediaEntryView;

    [SerializeField] private GameObject coverPannel;
    [SerializeField] private GameObject pagePannel;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    // 「intフィールドは初期値0」
    private int currentPage;

    private void Awake()
    {
        if (encyclopediaManager == null)
        {
            Debug.LogError("[EncyclopediaPageController] EncyclopediaManager is not assigned.", this);
            enabled = false;
            return;
        }

        if (encyclopediaEntryView == null)
        {
            Debug.LogError("[EncyclopediaPageController] EncyclopediaEntryView is not assigned.", this);
            enabled = false;
            return;
        }

        if (coverPannel == null)
        {
            Debug.LogError("[EncyclopediaPageController] CoverPannel is not assigned.", this);
            enabled = false;
            return;
        }

        if (pagePannel == null)
        {
            Debug.LogError("[EncyclopediaPageController] PagePannel is not assigned.", this);
            enabled = false;
            return;
        }

        if (leftButton == null)
        {
            Debug.LogError("[EncyclopediaPageController] Left Button is not assigned.", this);
            enabled = false;
            return;
        }

        if (rightButton == null)
        {
            Debug.LogError("[EncyclopediaPageController] Right Button is not assigned.", this);
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        // 「左ボタンを押したら HandleLeftButtonClicked を実行」
        leftButton.onClick.AddListener(HandleLeftButtonClicked);
        // 「右ボタンを押したら HandleRightButtonClicked を実行」
        rightButton.onClick.AddListener(HandleRightButtonClicked);
        // 「図鑑リストの読み込みが終わったら ShowCoverPage を実行」
        encyclopediaManager.OnEntriesLoaded += ShowCoverPage;

        // 「もしイベント購読する前に読み込みが終わっていたら、今すぐ表紙を表示する」
        if (encyclopediaManager.IsLoaded)
        {
            ShowCoverPage();
        }
    }

    private void OnDisable()
    {
        if (leftButton != null)
        {
            leftButton.onClick.RemoveListener(HandleLeftButtonClicked);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveListener(HandleRightButtonClicked);
        }

        if (encyclopediaManager != null)
        {
            encyclopediaManager.OnEntriesLoaded -= ShowCoverPage;
        }
    }

    private void HandleLeftButtonClicked()
    {
        // 「表紙より前には戻れない」
        if (currentPage <= 0)
        {
            return;
        }

        // 「ページ番号を1減らして、RefreshPage で画面表示を更新」
        currentPage--;
        RefreshPage();
    }

    private void HandleRightButtonClicked()
    {
        // 「登録済み蝶の数を取る」
        int entryCount = GetEntryCount();

        // 「最大ページ以上には進めない」
        if (currentPage >= entryCount)
        {
            return;
        }

        // 「ページ番号を1増やして、RefreshPage で画面表示を更新」
        currentPage++;
        RefreshPage();
    }

    private void ShowCoverPage()
    {
        // 「ページ番号を0にして、RefreshPage で画面表示を更新」
        currentPage = 0;
        RefreshPage();
    }
    
    private void RefreshPage()
    {
        // 「保存済みの蝶一覧を取得する」
        IReadOnlyList<ButterflyDexEntry> entries = encyclopediaManager.ButterflyEntries;
        int entryCount = entries == null // 条件「entriesがnull？」
            ? 0 　　　　　　　　　　　　 // true「Yesの場合は0」
            : entries.Count;　　　　　　 // false「Noの場合はentries.Countになる」

        // 「安全策として、currentPage が entryCount を上回ってしまったときに補正する」
        if (currentPage > entryCount)
        {
            currentPage = entryCount;
        }

        bool isCoverPage = currentPage == 0; // 「currentPage == 0かどうか？」
        coverPannel.SetActive(isCoverPage);　// 「trueならcoverPannelを表示」
        pagePannel.SetActive(!isCoverPage);　// 「falseならpagePannelを表示」

        // 「今が表紙ではなく、個体ページなら、どの蝶を表示するか決める」
        if (!isCoverPage)
        {
            int entryIndex = entryCount - currentPage;

            // 「蝶一覧entriesが存在しない、またはList番号がマイナス、またはListの範囲を超えているの場合」
            if (entries == null || entryIndex < 0 || entryIndex >= entryCount)
            {
                Debug.LogError("[EncyclopediaPageController] Could not find the butterfly entry for the current page.", this);
            }
            else
            {
                // 「表示対象の蝶1匹を取り出してencyclopediaEntryView.Show(...)に渡す」
                encyclopediaEntryView.Show(entries[entryIndex]);
            }
        }

        // 「UpdateButtonVisibilityにentryCountを渡す」
        UpdateButtonVisibility(entryCount);

    }

    // 「entryCountを受け取って、左右のボタンを出すか消すか決めるメソッド」
    private void UpdateButtonVisibility(int entryCount)
    {
        // 「左ボタンを、表紙より先のページにいる場合は表示、currentPage=0のときは表紙なので非表示にする」
        leftButton.gameObject.SetActive(currentPage > 0);
        // 「右ボタンを、最終ページにいる場合は非表示にする」
        rightButton.gameObject.SetActive(currentPage < entryCount);
        // 「蝶が0匹のときは、左も右も非表示になる」
    }

    // 「登録されている蝶が何匹いるか返す」
    private int GetEntryCount()
    {
        // 「EncyclopediaManagerから保存済み蝶一覧を受け取る」
        IReadOnlyList<ButterflyDexEntry> entries = encyclopediaManager.ButterflyEntries;
        return entries == null // 「entriesはnullか？」
            ? 0　　　　　　　　// 「Yes→0」
            : entries.Count;　 // 「No→entries.Countを入れる」
    }
}
