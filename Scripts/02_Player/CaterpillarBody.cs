using System;
// [Collections.Generic:Listを使うためのもの]
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// イモムシの胴体を頭が通った軌跡に沿って追従させるスクリプ
/// </summary>

[RequireComponent(typeof(SegmentFactory))]
public sealed class CaterpillarBody : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform segmentParent;
    [SerializeField] private SegmentFactory segmentFactory;
    [SerializeField] private BodyDamageHandler bodyDamageHandler;
    [SerializeField] private int initialBodySegmentCount = 2;
    // [Range(A,B):A～Bまでしか入力できない]
    // [float:小数型]
    [Range(0.2f, 0.3f)]
    [SerializeField] private float overlapRatioOfRadius = 0.25f;
    [SerializeField] private float minHistorySampleDistance = 0.03f;
    [SerializeField] private int maxHistoryPoints = 512;

    // [List:複数入る箱]
    // [readonly:別のListを代入できなくする。専用の保存箱。]
    private readonly List<CaterpillarSegment> segments = new List<CaterpillarSegment>();
    private readonly List<Vector2> headPositionHistory = new List<Vector2>();
    private float segmentSpacing;

    public event Action<CaterpillarSegment> OnSegmentAdded;

    // [IReadOnlyList:読むだけできるList]
    public IReadOnlyList<CaterpillarSegment> Segments => segments;
    public int CurrentSegmentCount => 1 + segments.Count;

    private void Awake()
    {
        if (segmentFactory == null)
        {
            segmentFactory = GetComponent<SegmentFactory>();
        }

        if (segmentParent == null)
        {
            segmentParent = transform;
        }

        if (bodyDamageHandler == null)
        {
            Debug.LogError("[CaterpillarBody] BodyDamageHandler is not assigned.", this);
            enabled = false;
            return;
        }

    }

    private void Start()
    {
        if (head == null)
        {
            // [Debug.LogError():コンソールのデバックログに()と表示させる]
            // [nameof(A):Aという文字列を表示させる]
            // [$"B":Bという文字列を表示]
            // [this:今動いているコンポーネント自身]
            // [enabled = false;このスクリプトを停止させる]
            Debug.LogError($"{nameof(CaterpillarBody)} requires a Head transform.", this);
            enabled = false;
            return;
        }

        // 「CalculateSegmentSpacing(...)というセグメントを呼ぶ」
        segmentSpacing = CalculateSegmentSpacing(segmentFactory.SegmentWorldDiameter);
        InitializeHistory(); // 「Historyを初期化」
        CreateInitialSegments(); // 「最初の体節を作る」
        UpdateSegmentPositions(); // 「体節の位置を更新する」
    }

    private void LateUpdate()
    {
        // 「頭の位置を記録する」
        RecordHeadPosition();
        // 「記録した位置を使って体を動かす」
        UpdateSegmentPositions();
    }

    private void InitializeHistory()
    {
        // 「headPositionHistoryを消去する」
        headPositionHistory.Clear();

        // 「頭の現在位置を保存する」
        Vector2 headPosition = head.position;
        // 「反対方向を返す」
        Vector2 backDirection = GetInitialBackDirection();
        // [Mathf.Max(A,B):A,Bの大きい方を返す]
        // 「requiredPointは最低４以上」
        int requiredPoints = Mathf.Max(initialBodySegmentCount + 2, 4);

        // [for (①int i=A; ②i >= B; ③i--/++) { 繰り返す処理 } :同じ処理を何回も繰り返すための文法]
        // ①初期化[int i=A:初期値Aからスタート]
        // ②条件[i >= B:iがB以上なら続ける]
        // ③更新[i++/--:iを１ずつ増やす/減らす]
        for (int i = requiredPoints - 1; i >= 0; i--)
        {
            headPositionHistory.Add(headPosition + backDirection * segmentSpacing * i);
        }
    }

    // ＊初期体節の生成
    private void CreateInitialSegments()
    {
        segments.Clear();

        for (int i = 0; i < initialBodySegmentCount; i++)
        {
            // 「どの位置に生成をするか→頭の後ろ」
            Vector2 segmentPosition = GetPositionBehindHead(segmentSpacing * (i + 1));
            // 「segmentFactoryにお願いしてセグメントを生成する」
            CaterpillarSegment segment = segmentFactory.CreateSegment(segmentParent, i + 1, segmentPosition);
            if (segment == null)
            {
                enabled = false;
                return;
            }

            segments.Add(segment);
            InitializeSegmentHitDetector(segment);
        }
    }

    // *新しい体節を1つ追加し、作った体節を返す
    public CaterpillarSegment AddSegment()
    {
        // * segmentIndex は現在の体節に1を足した数（0,1,2ではなく1,2,3で数えたいから）
        int segmentIndex = segments.Count + 1;
        // * segmentPosition は Head から（体節同士の間隔 × 体節番号）後ろ
        Vector2 segmentPosition = GetPositionBehindHead(segmentSpacing * segmentIndex);
        // * 新しい体節は segmentFactory に親オブジェクト、体節番号、体節の位置）を指定して作る
        CaterpillarSegment segment = segmentFactory.CreateSegment(segmentParent, segmentIndex, segmentPosition);

        if (segment == null)
        {
            // * ここでの return は処理を終了し、呼び出し元へ「作れませんでした」と返す
            return null;
        }

        // * segmentsList の末尾へ新しく作った体節を追加する
        segments.Add(segment);
        // * 新しく作った体節を渡して、この体節の SegmentHitDetector を初期化する
        InitializeSegmentHitDetector(segment);
        // * [ ?. : 受け取り相手がいるときだけ実行する]
        // * [ Invoke() : イベントを発生させ ( ) を渡す]
        // * 新しい体節が追加されたことと、追加された segment を渡す
        OnSegmentAdded?.Invoke(segment);
        // * AddSegment() を呼んだ相手へ新しく作った体節を返す
        return segment;
    }

    // * この体節がListの何番目に入っているか調べる
    public int GetSegmentIndex(CaterpillarSegment segment)
    {
        // * [ Index0f : List が持っている標準メソッド]
        // * segments の中からこの segment を探し位置を返す
        return segments.IndexOf(segment);
    }

    public bool RemoveSegmentsFrom(CaterpillarSegment segment)
    {
        if (segment == null)
        {
            Debug.LogWarning("[CaterpillarBody] Cannot remove segments because segment is null.", this);
            return false;
        }

        // * segments の中からこの segment の番号を調べて startIndex へ入れる
        int startIndex = segments.IndexOf(segment);

        if (startIndex < 0)
        {
            Debug.LogWarning($"[CaterpillarBody] Segment is not in the body list: {segment.name}", this);
            return false;
        }

        if (startIndex >= segments.Count)
        {
            return false;
        }

        // * リストの一番後ろから startIndex まで1つずつ戻りながら繰り返す
        for (int i = segments.Count - 1; i >= startIndex; i--)
        {
            // * 今消す体節を取り出す
            CaterpillarSegment segmentToRemove = segments[i];
            // * List から消す
            segments.RemoveAt(i);

            if (segmentToRemove != null)
            {
                // * Body の GameObject を削除する
                Destroy(segmentToRemove.gameObject);
            }
        }

        return true;
    }

    // * 渡された体節についている SegmentHitDetector を初期化する
    private void InitializeSegmentHitDetector(CaterpillarSegment segment)
    {
        // * この体節に付いている SegmentHitDetector を探して hitDetector に入れる
        SegmentHitDetector hitDetector = segment.GetComponent<SegmentHitDetector>();

        if (hitDetector != null)
        {
            // * 見つかった SegmentHitDetector の Initialize() を呼ぶ
            hitDetector.Initialize(segment, bodyDamageHandler);
        }
    }

    // *頭の位置を記録
    private void RecordHeadPosition()
    {
        // 「現在の頭の位置を取得してheadPositionに保存」
        Vector2 headPosition = head.position;

        if (headPositionHistory.Count > 0
            && Vector2.Distance(headPositionHistory[headPositionHistory.Count - 1], headPosition) < minHistorySampleDistance)
        {
            return;
        }

        // 「現在位置を履歴へ追加」
        headPositionHistory.Add(headPosition);

        if (headPositionHistory.Count > maxHistoryPoints)
        {
            headPositionHistory.RemoveAt(0);
        }
    }

    // *Segmentの位置を更新
    private void UpdateSegmentPositions()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            // 「頭からどれだけ後ろか」
            float distanceBehindHead = segmentSpacing * (i + 1);
            Vector2 followPosition = GetPositionBehindHead(distanceBehindHead);
            // [segments[i]:Listから[i]を取り出す]
            segments[i].SetFollowPosition(followPosition);
        }
    }

    // *頭の移動履歴をたどって、指定距離だけ後ろの座標を返す
    private Vector2 GetPositionBehindHead(float distanceBehindHead)
    {
        if (headPositionHistory.Count == 0)
        {
            return head.position;
        }

        // [remaining:残り]
        float remainingDistance = distanceBehindHead;

        // 「最新から過去へ向かって調べる」
        for (int i = headPositionHistory.Count - 1; i > 0; i--)
        {
            Vector2 newerPoint = headPositionHistory[i];
            Vector2 olderPoint = headPositionHistory[i - 1];
            // 「1つ前の履歴との距離をはかる」
            float segmentLength = Vector2.Distance(newerPoint, olderPoint);

            if (segmentLength <= 0.0001f)
            {
                // [continue;今回の処理は飛ばして次のループへ進む]
                continue;
            }

            // 「目的の位置がこの区間の中にあるなら～」
            if (remainingDistance <= segmentLength)
            {
                float t = remainingDistance / segmentLength;
                // [Lerp:2点の間を補間する]
                return Vector2.Lerp(newerPoint, olderPoint, t);
            }

            remainingDistance -= segmentLength;
        }

        return headPositionHistory[0];
    }

    // *ゲーム開始時にどちらが後ろか決める
    private Vector2 GetInitialBackDirection()
    {
        PlayerMovement movement = head.GetComponent<PlayerMovement>();

        // [Vector2Int.zero:(0,0)]
        if (movement != null && movement.CurrentDirection != Vector2Int.zero)
        {
            return -(Vector2)movement.CurrentDirection;
        }

        return Vector2.down;
    }

    // *体節同士を少し重ねるための間隔
    private float CalculateSegmentSpacing(float diameter)
    {
        float radius = diameter * 0.5f;
        return diameter - radius * overlapRatioOfRadius;
    }
}

