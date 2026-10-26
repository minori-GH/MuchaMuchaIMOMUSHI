using UnityEngine;

/// <summary>
/// マップのRoomを選び、正しい位置に生成するクラス
/// </summary>
public sealed class MapGenerator : MonoBehaviour
{
    // [const : ゲーム実行中に値を変更しない]
    private const float RoomHeight = 30f;
    private const int InitialRoomCount = 3;
    // 「Playerより先に何Room分のマップを用意しておくか = 2」
    private const int RoomsAheadOfPlayer = 2;

    // [ [] :配列]
    [SerializeField] private GameObject[] randomRoomPrefabs;
    [SerializeField] private Transform randomRoomStartPoint;
    [SerializeField] private Transform playerHead;
    [SerializeField] private RoomSpawner roomSpawner;

    // 「最初のRandomRoomを置く位置を覚えておく変数」
    private Vector3 firstRoomPosition;
    // 「今まで何Room生成したか。初期値0」
    private int generatedRoomCount;

    private void Awake()
    {
        // [Lengrh : 配列の要素数]
        if (randomRoomPrefabs == null || randomRoomPrefabs.Length == 0)
        {
            Debug.LogError("[MapGenerator] Random Room Prefabs are not assigned.", this);
            enabled = false;
            return;
        }

        // 「配列を一つずつ取り出して確認する」
        for (int i = 0; i < randomRoomPrefabs.Length; i++)
        {
            // 「配列のi番目の要素を取り出す。配列に空欄がない確認する」
            if (randomRoomPrefabs[i] == null)
            {
                // [$"... {i} ..." : 文字列補間。$"..." にすると {変数} を文章に入れられる]
                Debug.LogError($"[MapGenerator] Random Room Prefab at index {i} is not assigned.", this);
                enabled = false;
                return;
            }
        }

        if (randomRoomStartPoint == null)
        {
            Debug.LogError("[MapGenerator] Random Room Start Point is not assigned.", this);
            enabled = false;
            return;
        }

        if (playerHead == null)
        {
            Debug.LogError("[MapGenerator] Player Head is not assigned.", this);
            enabled = false;
            return;
        }

        if (roomSpawner == null)
        {
            Debug.LogError("[MapGenerator] Room Spawner is not assigned.", this);
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // 「RandomRoomStartPointの現在のワールド座標を、firstRoomPositionに保存する」
        firstRoomPosition = randomRoomStartPoint.position;
        GenerateInitialRooms();
    }

    private void Update()
    {
        // 「毎フレームRoomが足りてるか確認する」
        GenerateRoomsIfNeeded();
    }

    private void GenerateInitialRooms()
    {
        // 「GenerateNextRoom() を3回呼ぶ」
        for (int i = 0; i < InitialRoomCount; i++)
        {
            GenerateNextRoom();
        }
    }

    private void GenerateRoomsIfNeeded()
    {
        // 「現在生成済みマップの上端Y座標」
        float generatedTopY = firstRoomPosition.y + RoomHeight * generatedRoomCount;
        // 「Playerのために、ここまではマップを作っておきたいY座標」
        // 「プレイヤーの頭の位置＋30×2（60Unit）」
        float requiredTopY = playerHead.position.y + RoomHeight * RoomsAheadOfPlayer;

        // [while : 条件がtrueの間、繰り返す]
        while (generatedTopY < requiredTopY)
        {
            GenerateNextRoom();
            // 「GenerateNextRoom()でRoomを1個増やしたので、「現在のマップ上端」も新しい位置に更新する」
            // 　※ここで更新しないとwhileがずっとtrueとなり無限にRoomが作られてしまう
            generatedTopY = firstRoomPosition.y + RoomHeight * generatedRoomCount;
        }
    }

    private void GenerateNextRoom()
    {
        // 「generatedRoomCountを使い、次に生成するRoomの位置を30Unitずつ上へずらす」
        Vector3 roomPosition = firstRoomPosition + new Vector3(0f, RoomHeight * generatedRoomCount, 0f);
        // 「GetRandomRoomPrefab()を実行して、返ってきたGameObjectを roomPrefab という変数に入れる」
        GameObject roomPrefab = GetRandomRoomPrefab();
        // [Instantiate() :生成するもの、生成する場所、回転するか、誰の子にするか]
        // 「randomRoomPrefab を、roomPosition の位置に、回転なしで、MapGeneratorの子として生成する」    
        GameObject roomInstance = Instantiate(roomPrefab, roomPosition, Quaternion.identity, transform);
        roomSpawner.SpawnInRoom(roomInstance);
        // 「Roomを生成した後に置き、Roomを何個作ったかを記録していく」
        generatedRoomCount++;
    }

    // 「GameObject : GameObjectを1個返すメソッド」
    private GameObject GetRandomRoomPrefab()
    {
        // [Random.Range(0, 3) : 0～2のどれかがランダムで返る]
        int randomIndex = Random.Range(0, randomRoomPrefabs.Length);
        return randomRoomPrefabs[randomIndex];
    }
}
