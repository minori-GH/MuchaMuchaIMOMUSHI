using System;
using UnityEngine;

/// <summary>
/// ClearScene が始まったら GameResultHolder からプレイ結果を受け取り、その結果から ButterflyData を1匹分作る
/// </summary>

public sealed class ButterflyEvolutionController : MonoBehaviour
{
    // [readonly :一度入れたインスタンスをあとから別のものに入れ替えない]
    private readonly ButterflyEvolutionService evolutionService = new ButterflyEvolutionService();

    // [event : 何かが起きたことを他のクラスに知らせるための仕組み]
    // [Action : 呼び出すメソッドを登録しておける仕組み]
    // 「蝶が完成したことを他のクラスに通知し、 ButterflyData を渡す」
    public event Action<ButterflyData> OnButterflyCreated;

    // [{ get; private set; } :外から読めるが、書き換えはこのクラスの中だけ]
    public ButterflyData CurrentButterfly { get; private set; }

    // [Start() : このコンポーネントが動き始めたときに基本的に1回呼ばれる]
    private void Start()
    {
        // 「GameResultHolder に預けられていた今回のプレイ結果を取り出して、gameResult という名前で使えるようにする」
        GameResultData gameResult = GameResultHolder.CurrentResult;

        if (gameResult == null)
        {
            Debug.LogError("[ButterflyEvolutionController] GameResultData was not found.", this);
            return;
        }

        ButterflySizeType sizeType = evolutionService.DetermineSize(gameResult.SegmentCount);
        ButterflyColorType colorType = evolutionService.DetermineColorType(
            gameResult.TotalRed,
            gameResult.TotalGreen,
            gameResult.TotalBlue
        );

        CurrentButterfly = new ButterflyData(
            sizeType,
            colorType,
            gameResult.FinalColor
        );

        // [?. : null じゃなかったら実行する]
        // [Invoke : 登録されているメソッドを実行する]
        // 「OnButterflyCreated が null でなければ Invoke を実行する」
        OnButterflyCreated?.Invoke(CurrentButterfly);
    }
}
