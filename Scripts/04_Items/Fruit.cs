using UnityEngine;

public sealed class Fruit : MonoBehaviour
{
    [SerializeField] private FruitData fruitData;

    // 「何かがTriggerに入った瞬間呼ばれる」
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 「PlayerMovementがついていないならそこで終了」
        if (other.GetComponent<PlayerMovement>() == null)
        {
            return;
        }

        // 「フルーツデータがない場合はデバックログを返して終了」
        if (fruitData == null)
        {
            Debug.LogError($"{nameof(Fruit)} requires FruitData.", this);
            return;
        }

        // 「GetComponentする」
        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
        PlayerColorModel playerColorModel = other.GetComponentInParent<PlayerColorModel>();

        // 「どちらかがついてない場合はエラーを返して終了」
        if (playerStats == null || playerColorModel == null)
        {
            Debug.LogError($"{nameof(Fruit)} could not find PlayerStats or PlayerColorModel on the player.", this);
            return;
        }

        // 「成功したらカロリー・色を取得し、自身は破壊される」
        playerStats.AddCalorie(fruitData.Calorie);
        playerColorModel.AddFruitColor(fruitData);
        Destroy(gameObject);
    }
}
