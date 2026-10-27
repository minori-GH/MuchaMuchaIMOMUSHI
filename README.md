# むしゃむしゃ いもむし

Unity・C#で個人開発した、2Dの芋虫ゲームです。
フルーツを食べながら芋虫を育て、蝶になることを目指します。
育てた芋虫の節数や色に応じて生まれた蝶に名前を付け、図鑑へ登録できます。

## リポジトリについて

このリポジトリでは、C#のソースコードとWebGL版の実行用ファイルを公開しています。

| フォルダ | 内容 |
| --- | --- |
| [Scripts/](Scripts/) | ゲームに使用しているC#スクリプトを置いています。 |
| [docs/](docs/) | WebGL版の起動ページと実行用ファイルを置いています。 |

## ゲームをプレイする

| 配信先 | 公開URL |
| --- | --- |
| WebGL版（GitHub Pages） | [ゲームをプレイ](https://minori-gh.github.io/MuchaMuchaIMOMUSHI/) |

## 開発環境

| 項目 | 内容 |
| --- | --- |
| ゲームエンジン | Unity 6 |
| 開発言語 | C# |
| 開発体制 | 個人開発 |
| 主な使用パッケージ | Unity UI（uGUI）、TextMeshPro |

## ルールと操作方法

- 最初のスワイプで移動を開始します。その後も指定した方向へ進み、スワイプで上下左右の移動方向を変えます。壁のある方向には進めません。
- フルーツに触れると、そのフルーツに設定されたカロリーが加算されます。累計1000calごとに体節が1つ増えます。
- 食べたもののRGB累計値に応じて、芋虫の頭と体節の色が変わります。
- 通常時に頭が鳥に触れると、ゲームオーバーです。
- 通常時に体節が鳥に触れると、触れた体節から後ろが欠けます。ゲームオーバーにはならず、接触後は短時間無敵になります。累計カロリーは減りません。
- クッキーを取ると、移動速度アップ・無敵・頭で触れた鳥を消す効果のいずれかが一定時間続きます。同時に発動する効果は1種類で、再取得すると新しい効果へ切り替わります。
- 無敵中は頭・体節への鳥の接触を無効にします。鳥を消す効果中は体節が欠けず、鳥を消せるのは頭で触れた場合です。
- 累計10000calに達するとクリアです。記録した節数とRGB累計値から、3サイズ×3色系統の9タイプの蝶に分類します。

| 操作 | 動作 |
| --- | --- |
| 上下左右へスワイプ（タッチ操作） | 最初の入力でゲームを開始し、芋虫の進む方向を指定します。 |
| 矢印キー（Unity Editor） | スワイプと同じように、開始と移動方向の指定を行います。 |
| Pause／Resumeボタン | ゲームを一時停止・再開します。 |
| Retry／Backボタン | ポーズ・ゲームオーバー画面から、やり直しまたはスタート画面へ戻ります。 |
| 名前入力・登録ボタン | クリア後の蝶に名前を付けて図鑑へ登録します。 |
| 図鑑の左右ボタン | 表紙と、保存した蝶の個体ページを切り替えます。 |

## ゲームの流れ

1. スタート画面のStartボタンでGameSceneへ進みます。指アイコンを表示して待機し、最初のスワイプでゲームを開始します。
2. フルーツを集め、カロリーを増やしながら芋虫を成長させます。上へ進むと、部屋が追加されます。
3. 鳥を避けながら、クッキーの特殊効果を活用してクリアを目指します。
4. クリア条件を満たすと芋虫と鳥が停止し、画面が白くなった後にClearSceneへ進みます。
5. 今回の蝶が表示され、登場演出が終わると名前入力欄と登録ボタンを表示します。
6. 名前を入力して登録すると、図鑑記録の保存処理を行い、スタート画面へ戻ります。
7. スタート画面から図鑑を開き、9タイプの発見状況や、登録した蝶を新しい順に確認できます。

ゲームオーバー時はRetryボタンでGameSceneを読み直し、開始待ちからやり直せます。Backボタンではスタート画面へ戻ります。

## 主な実装機能

| 機能 | 内容 |
| --- | --- |
| プレイヤー操作・体節追従 | スワイプによる上下左右のグリッド移動と、頭の移動経路をたどる体節の追従を実装しています。 |
| ゲーム進行・状態管理 | 開始待ち、プレイ中、ポーズ、ゲームオーバー、クリアを管理します。 |
| フルーツ・成長 | フルーツごとのカロリー加算と、一定カロリーごとの体節追加を実装しています。 |
| 体色 | 食べたもののRGBを累積し、頭と体節の表示色へ反映します。 |
| 鳥の移動・AI | 通行可能な方向からプレイヤーへ近づく候補を選びます。逆走の抑制、袋小路でのUターン、Y方向の距離による停止・再開に対応しています。 |
| 接触・ダメージ | 頭への接触によるゲームオーバーと、体節への接触による欠損・一時停止・無敵時間・点滅を実装しています。 |
| クッキー | 速度アップ、無敵、頭で触れた鳥の消滅を抽選し、残り時間と再取得時の切り替えを管理します。 |
| マップ・配置 | 部屋Prefabをランダムに選び、進行に応じて上方向へ追加します。部屋内のスポーンポイントで、敵やアイテムの出現を抽選します。 |
| 蝶への進化 | クリア結果から蝶のサイズ・色系統を決め、個体ごとの色を反映して表示します。 |
| 図鑑 | 名前・登録日・個体色などをJSON形式で保存・読み込みします。9タイプの発見表示と、個体ページの画像・名前・日付・説明文の表示に対応しています。 |
| UI・演出 | 開始時のスワイプ案内、カロリー表示、上下カメラ追従、移動時の体節の伸縮、PowerUp中の透明度・輪郭・終了前点滅を実装しています。 |

## スクリプト構成

### ゲーム全体の管理

| スクリプト | 主な役割 |
| --- | --- |
| [GameManager.cs](Scripts/01_Core/GameManager.cs) | ゲームの状態を保持し、状態の変更を通知します。 |
| [ClearConditionChecker.cs](Scripts/01_Core/03_GameClear/ClearConditionChecker.cs) | 累計カロリーがクリア条件に達したかを判定します。 |
| [GameClearTransitionController.cs](Scripts/01_Core/03_GameClear/GameClearTransitionController.cs) | クリア結果を記録し、白いフェードの後にClearSceneへ移動します。 |

### プレイヤー・成長システム

| スクリプト | 主な役割 |
| --- | --- |
| [SwipeInputReader.cs](Scripts/01_Core/02_GameScene/SwipeInputReader.cs) | スワイプとEditor用の矢印キー入力を、上下左右の方向へ変換します。 |
| [PlayerController.cs](Scripts/02_Player/PlayerController.cs) | 最初の入力でゲームを開始し、移動方向をPlayerMovementへ渡します。 |
| [PlayerMovement.cs](Scripts/02_Player/PlayerMovement.cs) | マス目に沿った移動、方向転換、速度変更、一時停止を担当します。 |
| [CaterpillarBody.cs](Scripts/02_Player/CaterpillarBody.cs) | 頭の移動履歴を使った体節の追従と、体節の追加・削除を担当します。 |
| [PlayerStats.cs](Scripts/02_Player/PlayerStats.cs) | 累計カロリーと、成長に必要な次のカロリー値を管理します。 |
| [PlayerColorModel.cs](Scripts/02_Player/PlayerColorModel.cs) | 食べたもののRGBを累積し、芋虫の表示色を計算します。 |
| [BodyDamageHandler.cs](Scripts/02_Player/BodyDamageHandler.cs) | 体節への接触時に、欠損・一時停止・無敵時間・点滅を処理します。 |

### 敵・アイテム

| スクリプト | 主な役割 |
| --- | --- |
| [EnemyAI.cs](Scripts/03_Enemies/EnemyAI.cs) | プレイヤーを追う方向を選び、方向転換と距離による行動の停止を制御します。 |
| [EnemyMovement.cs](Scripts/03_Enemies/EnemyMovement.cs) | 鳥のマス移動と、一時停止・再開を担当します。 |
| [PlayerCollisionHandler.cs](Scripts/02_Player/PlayerCollisionHandler.cs) | 頭と鳥の接触を判定し、特殊効果に応じてゲームオーバーや鳥の消滅を処理します。 |
| [Fruit.cs](Scripts/04_Items/Fruit.cs) | フルーツ取得時にカロリーとRGBを加算します。 |
| [CookieItem.cs](Scripts/04_Items/CookieItem.cs) | クッキー取得時にPowerUpControllerへ特殊効果の抽選を依頼します。 |
| [PowerUpController.cs](Scripts/04_Items/PowerUpController.cs) | 特殊効果の抽選、切り替え、効果時間と欠損後の無敵時間を管理します。 |

### マップ生成

| スクリプト | 主な役割 |
| --- | --- |
| [MapGenerator.cs](Scripts/05_Map/MapGenerator.cs) | 部屋Prefabを選び、プレイヤーの進行に合わせて上方向へ追加します。 |
| [RoomSpawner.cs](Scripts/05_Map/RoomSpawner.cs) | 部屋内のスポーンポイントごとに、敵・アイテム・空のいずれかを抽選します。 |
| [RoomSpawnPoint.cs](Scripts/05_Map/RoomSpawnPoint.cs) | 生成位置を示し、Sceneビューに目印を表示します。 |

### 蝶への進化・図鑑

| スクリプト | 主な役割 |
| --- | --- |
| [ButterflyEvolutionControlle.cs](Scripts/06_Butterfly/ButterflyEvolutionControlle.cs) | クリア結果から今回の蝶を生成します。クラス名はButterflyEvolutionControllerです。 |
| [ButterflyEvolutionService.cs](Scripts/06_Butterfly/ButterflyDate/ButterflyEvolutionService.cs) | 記録した節数とRGB累計値から、蝶のサイズと色系統を判定します。 |
| [ButterflyVisual.cs](Scripts/06_Butterfly/ButterflyVisual.cs) | 蝶の画像と個体色を設定し、白いシルエットから色が現れる演出を再生します。 |
| [ButterflyNamingController.cs](Scripts/06_Butterfly/ButterflyNamingController.cs) | 名前を確認して図鑑記録を作成し、保存処理を呼び出してStartSceneへ戻ります。 |
| [SaveManager.cs](Scripts/06_Butterfly/SaveManager.cs) | 蝶の図鑑記録をJSONファイルへ保存・読み込みします。 |
| [EncyclopediaManager.cs](Scripts/01_Core/04_Encyclopedia/EncyclopediaManager.cs) | 保存済みの蝶一覧を読み込み、タイプごとの発見状況を判定します。 |
| [EncyclopediaCoverView.cs](Scripts/01_Core/04_Encyclopedia/EncyclopediaCoverView.cs) | 9タイプの発見状況に応じて、表紙の蝶を通常表示または黒いシルエットで表示します。 |
| [EncyclopediaPageController.cs](Scripts/01_Core/04_Encyclopedia/EncyclopediaPageController.cs) | 表紙と個体ページの切り替え、ページ送り、左右ボタンの表示を担当します。 |
| [EncyclopediaEntryView.cs](Scripts/01_Core/04_Encyclopedia/EncyclopediaEntryView.cs) | 1匹分の画像・色・名前・登録日・説明文を表示します。 |

### UI・その他

| スクリプト | 主な役割 |
| --- | --- |
| [StartSceneController.cs](Scripts/01_Core/01_StartScene/StartSceneController.cs) | スタート画面からゲーム画面または図鑑へ移動します。 |
| [SwipeStartTutorialController.cs](Scripts/01_Core/02_GameScene/SwipeStartTutorialController.cs) | 開始待ち中の指アイコンを表示し、プレイヤーと鳥の移動を止めます。 |
| [PausePanel.cs](Scripts/07_UI/PausePanel.cs) | ポーズ画面の表示、再開、やり直し、スタート画面へ戻る操作を担当します。 |
| [GameOverPanel.cs](Scripts/07_UI/GameOverPanel.cs) | ゲームオーバー画面の表示、やり直し、スタート画面へ戻る操作を担当します。 |
