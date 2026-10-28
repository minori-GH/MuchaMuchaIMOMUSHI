# むしゃむしゃ いもむし

Unity・C#で個人開発した、スマートフォン向けの2Dアクションゲームです。
芋虫を操作し、フルーツや葉っぱを食べて成長させ、蝶になることを目指します。
蝶の形態はプレイ内容によって変化します。

## ファイル構成

| フォルダ | 内容 |
| --- | --- |
| [Scripts/](Scripts/) | C#スクリプト |
| [docs/](docs/) | WebGL版の起動ページ、実行用ファイル |

## ゲームをプレイする

| 配信先 | 公開URL |
| --- | --- |
| WebGL版（GitHub Pages） | [ゲームをプレイする](https://minori-gh.github.io/MuchaMuchaIMOMUSHI/) |

## 開発環境

| 項目 | 内容 |
| --- | --- |
| ゲームエンジン | Unity 6 |
| 開発言語 | C# |
| 開発体制 | 個人開発 |

## 操作方法

| プレイ環境 | 操作方法 |
| --- | --- |
| スマートフォン版 | 上下左右にスワイプ |
| Web版（PC） | 矢印キー（↑ ↓ ← →） |

最初の入力でゲームが始まり、入力した方向に芋虫の進行方向を変更できます。

## ゲーム詳細

‐ スタート画面のStartボタンを押すとゲーム画面へ進みます。
- ゲーム画面では、最初にスワイプをすると移動を開始します。
- スワイプで上下左右の移動方向を変えながら通路を進みます。
- 食べ物に触れると、その食べ物に設定されたRGB値とカロリーが加算されます。
- 累計1000calごとに体節が1つ増え、RGBの累計値に応じて芋虫の体色が変化します。
- クッキーに触れると、移動速度アップ・無敵（透明化）・敵破壊のいずれかの特殊効果がランダムで発動します。
- 通常時に体節が敵（鳥）に触れると、触れた体節から後ろが欠けます。頭が触れるとゲームオーバーになります。
- 累計10000calに達するとゲームクリアになります。最終的な節数とRGB累計値から、3サイズ×3色系統の9タイプの蝶に変化します。
- 蝶は任意の名前を入力して登録され、スタート画面から図鑑を開くことで、発見状況や登録した蝶を確認することができます。

## 主な実装機能

| 機能 | 内容 |
| --- | --- |
| 状態・進行管理 | 開始待ち、プレイ中、ポーズ、ゲームオーバー、クリア状態の管理 |
| プレイヤー操作 | スワイプによる上下左右のグリッド移動、速度変更、一時停止 |
| 体節追従 | 頭の移動経路をたどる体節の追従を実装しています。 |
| 体節追加 | 食べたもののカロリー加算と、一定カロリーごとの体節追加 |
| 体色変化 | 食べたもののRGB値の累積と、頭と体節の表示色への反映 |
| 敵AI | プレイヤーを追跡して移動、逆走の抑制や袋小路でのUターン、プレイヤーとの距離に応じた停止・再開 |
| 接触ダメージ | 頭への接触によるゲームオーバー、体節への接触による欠損・一時停止・無敵時間・点滅 |
| 特殊効果 | 速度アップ、無敵（透明化）、敵破壊のランダム効果の抽選、残り時間と再取得時の切り替えの管理 |
| マップ | 部屋Prefabをランダムで抽選し進行に応じて上方向へ追加、部屋内のスポーンポイントにより敵やアイテムの出現を抽選 |
| 蝶への進化 | クリア結果から蝶のサイズ・色系統を決め、個体ごとの色を反映して表示 |
| 図鑑 | 名前・登録日・個体色をJSON形式で保存・読み込み、9タイプの発見状況の表示、登録した蝶の記録（画像・名前・日付・説明文）表示 |
| 演出 | ゲーム開始時のスワイプチュートリアル、プレイヤー移動時の体節の伸縮、特殊効果中の透明度・輪郭・終了前点滅など |

## スクリプト構成

### ゲーム状態・進行管理

| スクリプト | 主な役割 |
| --- | --- |
| [GameManager.cs](Scripts/01_Core/GameManager.cs) | ゲーム状態の保持、状態変更の通知 |
| [ClearConditionChecker.cs](Scripts/01_Core/03_GameClear/ClearConditionChecker.cs) | 累計カロリーによるクリア条件の判定 |
| [GameClearTransitionController.cs](Scripts/01_Core/03_GameClear/GameClearTransitionController.cs) | クリア結果の記録、ClearScene遷移 |

### プレイヤー

| スクリプト | 主な役割 |
| --- | --- |
| [SwipeInputReader.cs](Scripts/01_Core/02_GameScene/SwipeInputReader.cs) | スワイプ・矢印キー入力を上下左右の方向へ変換 |
| [PlayerController.cs](Scripts/02_Player/PlayerController.cs) | 移動方向をPlayerMovementへ渡す |
| [PlayerMovement.cs](Scripts/02_Player/PlayerMovement.cs) | マス目に沿った移動、方向転換、速度変更、一時停止 |
| [CaterpillarBody.cs](Scripts/02_Player/CaterpillarBody.cs) | 体節の追加・削除、頭の移動履歴を使った体節の追従 |
| [PlayerStats.cs](Scripts/02_Player/PlayerStats.cs) | カロリー値の管理 |
| [PlayerColorModel.cs](Scripts/02_Player/PlayerColorModel.cs) | RGB値の累積と芋虫の表示色の計算 |
| [BodyDamageHandler.cs](Scripts/02_Player/BodyDamageHandler.cs) | 体節への敵接触時の欠損・一時停止・無敵時間・点滅処理 |

### 敵

| スクリプト | 主な役割 |
| --- | --- |
| [EnemyAI.cs](Scripts/03_Enemies/EnemyAI.cs) | プレイヤーとの距離を計算し進行方向を決定、逆走の抑制や袋小路でのUターン、距離に応じた行動の制御 |
| [EnemyMovement.cs](Scripts/03_Enemies/EnemyMovement.cs) | マス目移動、一時停止・再開 |
| [PlayerCollisionHandler.cs](Scripts/02_Player/PlayerCollisionHandler.cs) | 接触判定、特殊効果に応じた反応の管理 |

### アイテム

| スクリプト | 主な役割 |
| --- | --- |
| [Fruit.cs](Scripts/04_Items/Fruit.cs) | フルーツ取得時にカロリーとRGBを加算 |
| [CookieItem.cs](Scripts/04_Items/CookieItem.cs) | クッキー取得時にPowerUpControllerへ特殊効果の抽選を依頼 |
| [PowerUpController.cs](Scripts/04_Items/PowerUpController.cs) | 特殊効果の抽選、切り替え、効果時間と欠損後の無敵時間の管理 |

### マップ生成

| スクリプト | 主な役割 |
| --- | --- |
| [MapGenerator.cs](Scripts/05_Map/MapGenerator.cs) | 部屋Prefabのランダム抽選、プレイヤーの進行に合わせて上方向へ追加 |
| [RoomSpawner.cs](Scripts/05_Map/RoomSpawner.cs) | 部屋内のスポーンポイントごとに、敵やアイテムの配置抽選 |

### 蝶への進化・図鑑

| スクリプト | 主な役割 |
| --- | --- |
| [ButterflyEvolutionControlle.cs](Scripts/06_Butterfly/ButterflyEvolutionControlle.cs) | クリア結果から蝶を生成 |
| [ButterflyEvolutionService.cs](Scripts/06_Butterfly/ButterflyDate/ButterflyEvolutionService.cs) | 記録した節数とRGB累計値から蝶のサイズと色系統を判定 |
| [ButterflyVisual.cs](Scripts/06_Butterfly/ButterflyVisual.cs) | 蝶の画像と個体色を設定し、白いシルエットから色が現れる演出を再生 |
| [ButterflyNamingController.cs](Scripts/06_Butterfly/ButterflyNamingController.cs) | 名前を入力して図鑑記録を作成し、保存処理を呼び出してスタート画面へ戻る |
| [SaveManager.cs](Scripts/06_Butterfly/SaveManager.cs) | JSONファイルへ保存・読み込み |
| [EncyclopediaManager.cs](Scripts/01_Core/04_Encyclopedia/EncyclopediaManager.cs) | 保存済みの蝶一覧を読み込み、タイプごとの発見状況を判定 |
| [EncyclopediaCoverView.cs](Scripts/01_Core/04_Encyclopedia/EncyclopediaCoverView.cs) | 9タイプの発見状況に応じて、表紙の蝶を通常表示または黒いシルエットで表示 |
| [EncyclopediaPageController.cs](Scripts/01_Core/04_Encyclopedia/EncyclopediaPageController.cs) | 表紙と個体ページの切り替え、ページ送り、左右ボタンの表示 |
| [EncyclopediaEntryView.cs](Scripts/01_Core/04_Encyclopedia/EncyclopediaEntryView.cs) | 1匹分の画像・色・名前・登録日・説明文の表示 |

### UI・その他

| スクリプト | 主な役割 |
| --- | --- |
| [StartSceneController.cs](Scripts/01_Core/01_StartScene/StartSceneController.cs) | スタート画面からゲーム画面・図鑑へ遷移 |
| [SwipeStartTutorialController.cs](Scripts/01_Core/02_GameScene/SwipeStartTutorialController.cs) | チュートリアルの指アイコン表示、プレイヤーと鳥の移動停止 |
| [PausePanel.cs](Scripts/07_UI/PausePanel.cs) | ポーズ画面の表示、再開、リトライ、スタート画面へ戻る操作 |
| [GameOverPanel.cs](Scripts/07_UI/GameOverPanel.cs) | ゲームオーバー画面の表示、リトライ、スタート画面へ戻る操作 |
