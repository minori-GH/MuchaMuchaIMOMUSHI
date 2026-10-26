using System;
using UnityEngine;

/// <summary>
/// プレイヤーの成長を管理するスクリプト
/// </summary>

public sealed class PlayerStats : MonoBehaviour
{
    [SerializeField] private CaterpillarBody caterpillarBody;
    [SerializeField] private int growthCalorieStep = 1000;

    // [event:イベント通知システム]
    // [Action<int>:intを受け取る関数]
    public event Action<int> OnCalorieChanged;
    public event Action<CaterpillarSegment, int> OnSegmentAdded;

    // [{ get; private set; }:get(読む)のはpublic、set(書き込む)のはprivate]
    public int TotalCalorie { get; private set; }
    public int NextGrowthCalorieThreshold { get; private set; } = 1000;
    public int MaxSegmentCount { get; private set; } = 3;

    private void Awake()
    {
        if (growthCalorieStep <= 0)
        {
            Debug.LogError($"{nameof(PlayerStats)} growth calorie step must be greater than 0.", this);
            enabled = false;
            return;
        }

        if (caterpillarBody == null)
        {
            caterpillarBody = GetComponentInParent<CaterpillarBody>();
        }

        if (caterpillarBody == null)
        {
            Debug.LogError($"{nameof(PlayerStats)} requires a CaterpillarBody reference for growth.", this);
        }

        NextGrowthCalorieThreshold = growthCalorieStep;
    }

    private void Start()
    {
        // *現在の体節数を記録
        if (caterpillarBody != null)
        {
            MaxSegmentCount = caterpillarBody.CurrentSegmentCount;
        }
    }

    // *カロリーを追加
    public void AddCalorie(int calorie)
    {
        if (calorie <= 0)
        {
            return;
        }

        TotalCalorie += calorie;
        // [?.Invoke:通知する]
        OnCalorieChanged?.Invoke(TotalCalorie);
        GrowForReachedThresholds();
    }

    // *成長判定
    private void GrowForReachedThresholds()
    {
        // 「条件を満たしている間ずっと繰り返す」
        while (TotalCalorie >= NextGrowthCalorieThreshold)
        {
            // 「caterpillarBodyがあるならAddSegment、空ならnull」
            CaterpillarSegment addedSegment = caterpillarBody != null
                ? caterpillarBody.AddSegment()
                : null;

            if (addedSegment != null)
            {
                // 「MaxSegmentCountをどちらか大きい方の値に更新する」
                MaxSegmentCount = Mathf.Max(MaxSegmentCount, caterpillarBody.CurrentSegmentCount);
                // 「体節が増えたことを通知する」
                OnSegmentAdded?.Invoke(addedSegment, caterpillarBody.CurrentSegmentCount);
            }

            NextGrowthCalorieThreshold += growthCalorieStep;
        }
    }
}
