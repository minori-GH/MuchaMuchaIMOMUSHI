using UnityEngine;

// ＊セル座標とワールド座標を変換するスクリプト
// [public static class:GameObjectに付けるスクリプトではなく、どこからでも呼べる便利関数集]
// プロジェクト内で共有の値とか仕組みとか
public static class GridUtility
{
    // [(Vector2Int cell):Vector2Int型のcellを受け取る]
    public static Vector2 CellToWorld(Vector2Int cell)
    {
        // [return:次の値を返す]
        return new Vector2(cell.x + 0.5f, cell.y + 0.5f);
    }

    public static Vector2Int WorldToCell(Vector2 position)
    {
        // [FloorToInt:小数点以下切り捨て]
        return new Vector2Int(
            Mathf.FloorToInt(position.x),
            Mathf.FloorToInt(position.y)
        );
    }
}
