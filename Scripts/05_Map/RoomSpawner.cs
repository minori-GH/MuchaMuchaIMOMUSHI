using UnityEngine;

/// <summary>
/// 生成された1つのRoomについて、各SpawnPointを抽選し、必要ならPrefabを生成するクラス
/// </summary>
public sealed class RoomSpawner : MonoBehaviour
{
    // [Header : Inspector上に見出しを表示するUnityの機能]
    [Header("Spawn Rates")]
    [SerializeField, Min(0f)] private float emptyRate = 30f;
    [SerializeField, Min(0f)] private float fruitRate = 55f;
    [SerializeField, Min(0f)] private float cookieRate = 5f;
    [SerializeField, Min(0f)] private float enemyRate = 10f;

    [Header("Fruit Weights")]
    [SerializeField, Min(0f)] private float appleWeight = 1f;
    [SerializeField, Min(0f)] private float blueberryWeight = 1f;
    [SerializeField, Min(0f)] private float leafWeight = 1f;

    [Header("Prefabs")]
    [SerializeField] private GameObject applePrefab;
    [SerializeField] private GameObject blueberryPrefab;
    [SerializeField] private GameObject leafPrefab;
    [SerializeField] private GameObject cookiePrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Scene References")]
    [SerializeField] private Transform playerHead;
    [SerializeField] private PowerUpController powerUpController;

    private const float ExpectedTotalSpawnRate = 100f;
    private const float RateTolerance = 0.01f;

    private void Awake()
    {
        // 「4種類の出現率を全部足して、totalSpawnRate に入れる」
        float totalSpawnRate = emptyRate + fruitRate + cookieRate + enemyRate;

        // [Mathf.Abs(...) : 絶対値]
        // 「100から0.01より大きく離れているか？」
        if (Mathf.Abs(totalSpawnRate - ExpectedTotalSpawnRate) > RateTolerance)
        {
            Debug.LogError(
                $"[RoomSpawner] Spawn rates must total 100. Current total: {totalSpawnRate}",
                this
            );
            enabled = false;
            return;
        }

        if (applePrefab == null)
        {
            Debug.LogError("[RoomSpawner] apple Prefab is not assigned.", this);
            enabled = false;
            return;
        }

        if (blueberryPrefab == null)
        {
            Debug.LogError("[RoomSpawner] Blueberry Prefab is not assigned.", this);
            enabled = false;
            return;
        }

        if (leafPrefab == null)
        {
            Debug.LogError("[RoomSpawner] Leaf Prefab is not assigned.", this);
            enabled = false;
            return;
        }

        if (cookiePrefab == null)
        {
            Debug.LogError("[RoomSpawner] Cookie Prefab is not assigned.", this);
            enabled = false;
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("[RoomSpawner] Enemy Prefab is not assigned.", this);
            enabled = false;
            return;
        }

        if (playerHead == null)
        {
            Debug.LogError("[RoomSpawner] Player Head is not assigned.", this);
            enabled = false;
            return;
        }

        if (powerUpController == null)
        {
            Debug.LogError("[RoomSpawner] PowerUpController is not assigned.", this);
            enabled = false;
            return;
        }
    }

    // 「生成済みRoomを1個受け取って、その中にSpawnするメソッド」
    public void SpawnInRoom(GameObject roomInstance)
    {
        // 「RoomSpawnerが無効になっているならSpawnしない」
        if (!enabled)
        {
            return;
        }

        if (roomInstance == null)
        {
            Debug.LogError("[RoomSpawner] Room instance is null.", this);
            return;
        }

        // 「このRoomとその子GameObjectの中から、RoomSpawnPointコンポーネントを全部探す」
        RoomSpawnPoint[] spawnPoints = roomInstance.GetComponentsInChildren<RoomSpawnPoint>();

        // 「spawnPointsの中からRoomSpawnPointを1個ずつ取り出して処理する」
        foreach (RoomSpawnPoint spawnPoint in spawnPoints)
        {
            // 「どこに出すか、生成物を誰の子にするかを受け取る」
            SpawnAtPoint(spawnPoint.transform, roomInstance.transform);
        }
    }

    private void SpawnAtPoint(Transform spawnPoint, Transform parent)
    {
        // 「ChooseSpawnPrefab の結果を GameObject prefab に入れる」
        GameObject prefab = ChooseSpawnPrefab();

        if (prefab == null)
        {
            return;
        }

        // 「抽選された Prefab を SpawnPoint の位置に、元 Prefab の回転で、Room の子として生成する」
        GameObject spawnedObject = Instantiate(prefab, spawnPoint.position, prefab.transform.rotation, parent);

        if (prefab == enemyPrefab)
        {
            EnemyAI enemyAI = spawnedObject.GetComponent<EnemyAI>();

            if (enemyAI == null)
            {
                Debug.LogError("[RoomSpawner] EnemyAI is not found on the spawned Enemy Prefab root.", spawnedObject);
                return;
            }

            enemyAI.SetPlayerHead(playerHead);
        }

        if (prefab == cookiePrefab)
        {
            CookieItem cookieItem = spawnedObject.GetComponent<CookieItem>();

            if (cookieItem == null)
            {
                Debug.LogError("[RoomSpawner] CookieItem is not found on the spawned Cookie Prefab root.", spawnedObject);
                return;
            }

            cookieItem.SetPowerUpController(powerUpController);
        }

    }

    // 「第1抽選」
    private GameObject ChooseSpawnPrefab()
    {
        float roll = Random.Range(0f, ExpectedTotalSpawnRate);

        if (roll < emptyRate)
        {
            return null;
        }

        if (roll < emptyRate + fruitRate)
        {
            return ChooseFruitPrefab();
        }

        if (roll < emptyRate + fruitRate + cookieRate)
        {
            return cookiePrefab;
        }

        return enemyPrefab;
    }

    // 「フルーツ抽選」
    private GameObject ChooseFruitPrefab()
    {
        float totalFruitWeight = GetTotalFruitWeight();
        float roll = Random.Range(0f, totalFruitWeight);

        if (roll < appleWeight)
        {
            return applePrefab;
        }

        if (roll < appleWeight + blueberryWeight)
        {
            return blueberryPrefab;
        }

        return leafPrefab;
    }

    private float GetTotalFruitWeight()
    {
        return appleWeight + blueberryWeight + leafWeight;
    }
}
