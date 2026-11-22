# 別系統パネルのBlender化 作業報告

2026-11-23 JST / Codex / branch `blender-panels`

**40件に対応する19種類の無傷モデルと6種類の破片モデルをBlenderで制作し、描画だけを差し替えるコードを保存した。ただし、必須のNative検証はUnityライセンス不認識でBLOCKED。この依頼全体は未完了で、「全破壊対象がBlender製・残数0を実証済み」とは判定していない。**

## 版・保存状態

- 基準branch：`blender-rollout`、commit `8fb3d25c26e2d1f8a7bdf6e26ec77b2826606932`。
- 候補版名：`blender-panels-1`。実装・25原稿・25FBX・再生成/検証スクリプト・本報告の写しを`blender-panels`へコミットする。確定commitは同フォルダの`commit-receipt.json`と最終応答に記録。
- Unity `6000.3.23f1` / Built-in Render Pipeline。実際に生成・開き直したBlenderは `4.2.23 LTS`。
- 原稿・再生成：`source/tools/blender/panel_*.blend`、`build_panels.py`、`inventory_panels.py`、`review_panels.py`。
- Unity表示資産：`source/games/flight/crab-flight/Assets/BlenderModels/Resources/BlenderModels/panel_*.fbx`。
- 証拠：`evidence/blender-panels/`。主要記録のGit保存先：`source/docs/blender-panels/`。
- 開始時の未コミット`FlightField.unity`はSHA256 `058be0b3456083199edfc5b9650b233cbb63209249d00f4c1a764443c68fb5bb`を維持。同byteの退避を`evidence/blender-panels/FlightField-at-start.unity`へ保存し、コミットから除外した。
- push、外部送信、公開、WebGL配布物更新、認証・ライセンス設定変更は行っていない。

## 棚卸し：ソース確認とNative確認を分ける

前報告の40件をそのまま転記せず、現行`RestaurantPhysics.cs`、`OverdriveRoute.cs`、`CoreDistrictContactsR7.cs`の生成呼出を展開した。Scene設定は`cityLayout=2 / useOverdrive=1 / useOverdriveRoute=1 / useCoreDestruction=1`。各呼出の正負side、外壁row 0〜2、位置、寸法、色、閾値、出典行を`panels-families.json`に記録した。

| 系統 | ソースから列挙した有効ID | 内訳 |
|---|---:|---|
| Restaurant | 20 | 生簀ガラス2、皿積み2、外壁6、天井2、料理トレイ2、椅子2、テーブル2、厨房機器2 |
| Overdrive | 16 | 入口皿・椅子脇料理・皿タワー・サービステーブル・サービス料理・テーブル脇皿・厨房脇料理・出口テーブルを各2 |
| Core | 4 | モールガラス（x=±112）2、オフィスガラス（x=±205）2 |
| **計** | **40** | 左右20ずつ、19無傷モデル。外壁上段モデルだけ4件が共用 |

旧`ActiveCityLayout==0`専用の街の木箱2・金属ゴミ箱2は現Sceneでは生成されず、除外理由付きで別記した。Editor専用の同時衝突probeは製品生成経路ではない。Overdriveだけ・店内だけで40件とはしていない。

**Native上の登録リスト再集計は未実施。** 初回Unity起動はゲームのロード前にライセンスで終了し、`panels.json`を生成できなかった。ソース上の40件と前版の登録総数の差分は整合するが、それを今回の動的実体確認の代わりにしていない。

## 描画経路と破壊判定の結び付き

全40件は`RigidCrabFlight.AddPanel` → `RestaurantVisuals.CreateBreakableVisual`を経由する。旧表示はC#生成の箱・回転面・枠等。`BreakPanel`のID/type/位置/size/threshold、同rootのBoxCollider群、`breakByCollider`で接触と結び付く。

接触impulseと既存閾値を満たすと`ApplyBreaks`がrootを非表示にしてイベントを記録し、既存の破片数・寸法・Rigidbody・質量・速度で破片を作る。再投は既存`ResetRestaurant`が同rootを再表示する。

今回の既存C#変更は`RestaurantPhysics.cs`内の表示呼出2か所だけ：

1. 元の無傷表示を生成した直後に`BlenderPanelOverride.Apply`。旧Rendererを無効化し、IDに対応するBlender MeshFilter/MeshRendererを子として追加する。
2. 元の破片のCollider/Rigidbody/質量/初速度を設定した後に`ApplyFragment`。元の破片形状のローカル表示範囲へ、Blenderの表示子だけを合わせる。

旧ColliderやRigidbodyを削除・再生成していない。閾値、接触辞書、物理計算、配置データ、root scale、UI、手触りには変更を加えていない。料理破片の旧CC0 prefab名と計数は既存の帳簿として保持するが、可視MeshはBlender製へ差し替える実装。実行時の不変性はNative比較待ち。

## 造形

全原稿はメートル実寸・底面中心原点。頂点色と既存`PilotFlat`を使う、ベタ塗り＋2段陰。新しい光沢やグラデーションは追加しない。親Transformは変更せず、旧描画の底面offsetを表示子で継承する。OverdriveGroundPileの既存追加offsetも変更しない。

| モデル | 対応件数 | 特徴部 |
|---|---:|---|
| `panel_tank_glass` | 2 | 中央を空けたガラス枠、上下レール、対角の反射線、角金具。両面に特徴。 |
| `panel_dish_stack` | 2 | 反った縁と凹んだ中央を持つ皿。中段にミント色の皿を1枚。 |
| `panel_outer_wall_low` | 2 | テラコッタの壁、目地、上下笠木、木製腰壁と横桟。 |
| `panel_outer_wall_upper` | 4 | テラコッタの壁、目地、上下笠木。 |
| `panel_ceiling` | 2 | 水平の天井板、裏面の梁と板継ぎ。 |
| `panel_food_tray` | 2 | 皿・米・魚の切身・海苔帯・切身の筋・添え物を分離した寿司トレイ。 |
| `panel_wood_chair` | 2 | 4脚・座面・背柱・背板・上桟を分離。 |
| `panel_wood_table` | 2 | 4脚・天板・幕板・板継ぎ・布ランナー。 |
| `panel_metal_kitchen` | 2 | 厨房オーブン。2口バーナー、操作ノブ、窓付き扉と取手、据付脚。 |
| `panel_r5_restaurant_entry_dishes` | 2 | 反った縁と凹んだ中央を持つ皿。中段にミント色の皿を1枚。 |
| `panel_r5_restaurant_chair_food` | 2 | 皿・米・魚の切身・海苔帯・切身の筋・添え物を分離した寿司トレイ。 |
| `panel_r5_restaurant_dish_tower` | 2 | 反った縁と凹んだ中央を持つ皿。中段にミント色の皿を1枚。 |
| `panel_r5_restaurant_service_table` | 2 | 4脚・天板・幕板・板継ぎ・布ランナー。 |
| `panel_r5_restaurant_service_food` | 2 | 皿・米・魚の切身・海苔帯・切身の筋・添え物を分離した寿司トレイ。 |
| `panel_r5_restaurant_table_dishes` | 2 | 反った縁と凹んだ中央を持つ皿。中段にミント色の皿を1枚。 |
| `panel_r5_restaurant_kitchen_food` | 2 | 皿・米・魚の切身・海苔帯・切身の筋・添え物を分離した寿司トレイ。 |
| `panel_r5_restaurant_exit_table` | 2 | 4脚・天板・幕板・板継ぎ・布ランナー。 |
| `panel_core_mall_glass` | 2 | 中央を空けたガラス枠、上下レール、対角の反射線、角金具。両面に特徴。 |
| `panel_core_office_glass` | 2 | 中央を空けたガラス枠、上下レール、対角の反射線、角金具。両面に特徴。 |

破片はglass/wall/tableware/food/wood/metalの6種類。1m正規化原稿を旧破片の表示範囲へ適用し、破片rootのscaleとColliderを維持する。既存City41家族のモデルは再生成していない。人物・鳥は対象外。

[Blender原稿の一覧画像](evidence/blender-panels/models-contact-sheet.png) は新規19無傷モデルの実レンダー。全19枚を一覧で視認し、ガラス・外壁・天井がほぼ線だけに見えた初回の検査カメラを、特徴面が見える角度へ修正した。モデルの位置・寸法・ゲームカメラはその修正で変えていない。**これはBlender画像であり、Unity Native画像の代用ではない。**

## 検証結果

| 検査 | 結果 |
|---|---|
| ソースの有効生成経路 | 40 ID・19無傷モデルへの対応、重複IDなし。Native登録確認ではない |
| Blender生成 | 25 `.blend` / 25 FBX / 25 PNGを実生成 |
| Blender保存物読戻し | 25原稿を再オープン、実寸・原点・頂点色PASS。境界誤差は各0.00001m未満 |
| C#ランタイム型検査 | Unity同梱Roslyn、exit0 / errors0 / warnings18 |
| C# Editor検査コード型検査 | 同Roslyn、exit0 / errors0 / warnings0 |
| 既存ソースの変更範囲 | 表示呼出2か所を除くと`RestaurantPhysics.cs`全文が基準commitと一致 |
| 保護356ファイル | 上記1ファイルを除く355ファイルは開始時SHA一致。Scene差分も保持 |
| `npx playwright test` | **2 passed / 0 failed / 6 skipped**。Native依存6件はlicense BLOCKEDを明記。全件合格ではない |
| Unity Native基準起動 | **exit198**。ライセンス不認識、ゲーム開始前に終了 |
| seed42043 / accepted-input の同入力比較 | **NOT_RUN** |
| 飛距離 / 粉砕 / route.csv / break-events.jsonl完全一致 | **NOT_RUN** |
| 全City 1,407件の物理署名一致 | **NOT_RUN**。ソース不変を実測一致へ読み替えない |
| 全登録系統の未Blender残数0 | **UNVERIFIED** |
| 店内・Core区画のNativeスクリーンショット | **NOT_RUN / 未生成** |
| Game Studio game-playtest | **NOT_RUN**。現セッションのnative skill catalogに接続入口なし |

Unityの実ログ：`evidence/blender-panels-baseline/unity.log`。主因は `No valid Unity Editor license found. Please activate your license.`。Access token unavailable / entitlement not foundも出ている。別起動方法でライセンス制限を迂回せず、本人へ正規のUnity Hubでの復旧を依頼した。同条件のUnity再試行はしていない。

実行したPlaywrightコマンド（sourceから）：

```sh
BLENDER_PANELS_NATIVE_BLOCKED=license NODE_PATH='/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules' npx --offline --prefix .git/placement-fix-qa/runtime --no-install playwright test --config qa/blender-panels/playwright.config.cjs
```

型検査は既存Unity rspの参照・define・source一覧を保ち、今回の2 C#を追加して独立保存先へ出力したもの。Unityでのasset import・shader描画・PlayMode・ゲーム実行の合格ではない。rsp/logは証拠フォルダに保存。

## 系統別の残数と対象外

| 系統 | 対象数の根拠 | ソース対応の未割当 | 今回Nativeで確認した未Blender残数 |
|---|---|---:|---|
| City | 前版Native 1,407件。対応コード・既存モデルは変更なし | 0（前版対応維持） | 未測定 |
| Restaurant | 現行ソース20件 | 0 | 未測定 |
| Overdrive | 現行ソース16件 | 0 | 未測定 |
| Core | 現行ソース4件 | 0 | 未測定 |
| **破壊対象計** | **想定1,447件** | **0** | **残数0の実証は未達** |
| 人物 | 前版Native22件 | 対象外 | 今回未変更・未再集計 |
| 鳥 | 前版Native12件 | 対象外 | 今回未変更・未再集計 |
| 背景・非破壊の静的装飾 | 今回は数えない | 対象外 | 今回未変更 |

## 残件と再開

1. 正規のUnity Hubで既存ライセンスの認識を復旧する。こちらでは認証情報・契約・ライセンス設定を変更していない。
2. `run_native_panels.py`で真のblender-rollout基準コピーと候補を実行し、保存済みrollout最終記録とも比較する。再開コマンドは`source/tools/blender/README-panels.md`に記載。
3. Native実体の系統別件数・残数、全City物理署名、固定入力全byte一致、破片・再投をPlaywrightで確認する。検証カメラは正負各7/17/25/112/205mを用意済み。出力画像を実見し、必要な外観修正後に該当範囲だけ再確認する。
4. 本報告とHANDOFFへ実結果を追記し、必須条件を満たした同版を確定する。

リスク：今回のUnity未実行によるimport/向き/動的登録漏れ（起きやすさ2＋影響3=5）は必須確認待ち。既存の巨大家具寸法・店内浮き・Collider近似残差・床一時侵入・モール反復FAILは解消扱いにしない。実聴・物理iPhone・実FPS・本人の手触り採用も未確認。モデル制作・ソース保存と、ゲーム全体完成・採用・公開を分ける。

案件のKB参照禁止を維持しTEAMは未確認。Studio入口も現catalogでは未接続。状態は既存HANDOFF一箇所とtasks.mdへ記録し、外部送信・Linear更新は行っていない。
