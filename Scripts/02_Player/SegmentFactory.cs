using UnityEngine;

public sealed class SegmentFactory : MonoBehaviour
{

    // 「セグメント画像を設定」
    [SerializeField] private CaterpillarSegment segmentPrefab;
    // 「セグメントの直径」
    [SerializeField] private float segmentWorldDiameter = 0.8f;

    // 「外部からsegmentWorldDiameterを読めるようにする」
    public float SegmentWorldDiameter => segmentWorldDiameter;

    // 「生成したセグメントを返す（親オブジェクト、何番目の体節か、生成位置）
    public CaterpillarSegment CreateSegment(Transform parent, int index, Vector2 position)
    {
        // 「Prefabが設定されていなければデバックログを返す」
        if (segmentPrefab == null)
        {
            Debug.LogError($"{nameof(SegmentFactory)} requires a segment prefab.", this);
            return null;
        }

        // [Instantiate:Prefabを複製して生成する]
        // [Quaternion.identity:回転なし]
        // 「プレハブを、生成位置に、回転なしで、生成する」
        CaterpillarSegment segment = Instantiate(segmentPrefab, position, Quaternion.identity, parent);
        segment.name = $"Segment_{index:00}";
        segment.SetFollowPosition(position);

        return segment;
    }

}


