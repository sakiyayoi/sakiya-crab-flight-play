# 別系統パネルのBlender化 作業報告

2026-11-23 JST / Codex / branch `blender-panels`

**40件に対応する19種類の無傷モデルと6種類の破片モデルをBlenderで制作し、描画だけを差し替えるコードを保存した。ただし、必須のNative検証はUnityライセンス不認識でBLOCKED。この依頼全体は未完了で、「全破壊対象がBlender製・残数0を実証済み」とは判定していない。**

## 版・保存状態

- 基準branch：`blender-rollout`、commit `8fb3d25c26e2d1f8a7bdf6e26ec77b2826606932`。
- 候補版名：`blender-panels-1`。実装・25原稿・25FBX・再生成/検証スクリプトのcommit：`58f6b7efe528cd49dd7fa958dd242e054af1740f`。91ファイルをGitから読み戻し、保存SHAとの不一致0を確認。本報告と保存確認の更新は後続commit。`evidence/blender-panels/commit-receipt.json`にも記録。
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


## 2026-11-23 追記：再投後のパネル検証ハーネス修正

今回の依頼範囲（Editorハーネス修正・型検査・ローカルコミット・本報告追記）を実施した。Unity実走は依頼どおりクロエの担当として未実施。上記のライセンスBLOCKEDは初回制作時の履歴であり、復旧後の `licensed-baseline02` / `licensed-candidate02` はともにゲーム実行後、検証ハーネスの例外でexit 1になっている。ゲーム全体の合格やNative比較の合格は宣言しない。

### 原因と修正

- 両方の `unity.log` で `BlenderPanelsReview.Field:128` → `PanelInventory:130` → `Run:70` のNullReferenceExceptionを確認。`Run` の再投処理 `game.Prepare("")` 後の `panels-retry.json` 棚卸しで発生している。
- `CrabFlightGame.Prepare` は `world` を再構築した後、設計どおり `simulation=null` に戻す。`ReviewSimulation` はその `simulation` を返し、パネル登録リストは `world` が保持する。従来ハーネスはnullを `Field(sim,"panels")` に渡していた。
- `PanelInventory` の参照元を `game.ReviewSimulation ?? (RigidCrabFlight)Field(game,"world")` に変更した。飛行中の参照を優先し、nullのときだけ現在の `world` を読む。ゲーム側の状態・物理・配置・表示・UIには書き込まない。
- 両方がnullなら `InvalidOperationException` で停止する。例外本文は `PanelInventory (panels-retry.json): both CrabFlightGame.ReviewSimulation and world are null; no panel inventory is available.`（括弧内は実際の棚卸しファイル名）。`Field` にnullを渡す前に原因を明示する。
- 変更したコードは `games/flight/crab-flight/Assets/BlenderModels/Editor/BlenderPanelsReview.cs` の4行追加・1行削除のみ。

### コミット先とbaseline / candidateへの反映

修正コミット：`30d1a20c3195703cbea6081ca10dee809886ecbe`（`fix: retain panel inventory access after retry`）。branchは開始時の **`port-prototype`** のまま。修正ファイルのSHA256：`22ed98da6c332b98f48029eab4c5dcfa96875d5800f846ce137f42e2071fe9e8`。

`tools/blender/run_native_panels.py` の複製方式を確認した。baselineは `start.json` の `baseCommit`（`8fb3d25c26e2d1f8a7bdf6e26ec77b2826606932`）を `git archive` で展開した後、**呼び出し元の現在のsourceから `BlenderPanelsReview.cs` を読み、revision文字列だけbaseline用に置換してコピーする**。candidateは現在のsourceを直接使用する。したがって、このsourceの `port-prototype` に修正を置けば、新しいbaselineとcandidateの両方が修正済みハーネスを使う。`blender-rollout` / `blender-panels` のbranchや既存実行コピーを変更する必要はない。Pythonランナーも変更していない。

baselineの `execution.json.sourceCommit` は旧ゲーム本体のbaseCommitを示すため、使用した検証ハーネスの版は上記コミットとSHAで区別する。既存 `licensed-*-02` の失敗ログ・コピーは保持した。

### 開始時から存在した未コミット差分

- `panel_*.fbx.meta` は25ファイル。全件で元の `fileFormatVersion` とGUIDを維持し、同一の `ModelImporter` 設定109行だけが追加されていた（`serializedVersion: 24300`、`isReadable: 1`、`bakeAxisConversion: 1`、`preserveHierarchy: 1`、アニメーション・カメラ・ライト・Colliderのimport無効等）。既存 `BlenderPilotImport.OnPreprocessModel` の設定と一致し、Unityのimportによる設定の書き出しと整合する。GUIDの再生成やFBX本体の変更ではない。diffだけでは書込プロセスの同一性までは断定しない。
- 別途、開始時から `FlightField.unity` にGameObject/FileIDの変更と `FlightTuningPanel` コンポーネント削除の差分があった。SHA256は既存報告と同じ `058be0b3456083199edfc5b9650b233cbb63209249d00f4c1a764443c68fb5bb`。
- 以上26ファイルを破棄・stage・commitせず保持。修正後のSHA照合で開始時からの変更0件。今回の依頼の変更範囲に従い、tasks.mdや別の管理記録への書込みは追加せず、本追記に状態と再開手順をまとめた。

### 今回の検証と範囲

sourceで既存の型検査手順を順番に実行した。

```sh
python3 tools/port-prototype/typecheck.py
python3 tools/port-prototype/typecheck.py --editor
```

- ランタイム：exit 0 / **errors 0 / warnings 18**（既存警告）。Editor参照DLLを先に生成するための実行で、ランタイムコードは変更していない。
- Editor：exit 0 / **errors 0 / warnings 0**。コンパイル入力の `editor-sources.json` に、上記SHAの修正済み `BlenderPanelsReview.cs` が含まれることを確認。
- `git diff --check`：変更したハーネスは問題なし。
- 証拠は `evidence/blender-panels/harness-fix01/managed-compile/` と同フォルダの `runtime-command.log` / `editor-command.log`。型検査スクリプトの既定出力先にあった以前の記録は実行前のbyteへ戻し、今回分をこの別ディレクトリに保存した。

追加で既存Playwrightのソース棚卸し1件を選択した（ブラウザ・Unityを起動しない検査）。

```sh
NODE_PATH='/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules' \
  npx --offline --prefix .git/placement-fix-qa/runtime --no-install \
  playwright test --config qa/blender-panels/playwright.config.cjs \
  --grep 'source enumeration covers' --reporter list \
  --output ../evidence/blender-panels/harness-fix01/playwright02-results
```

結果：**0 passed / 1 failed**。`panels-families.json.sourceHashes` が期待する `RestaurantPhysics.cs` のSHA `578fe7d5425d55de6fd60c871f7b8c074398003c27f59428de9d52b6ae9feedb` と、現行SHA `bc96138ed7d60b48667a48127704ed128ddacb892013141cfec48379b6d8fc15` が不一致。修正前の `8fa2855a4eaa624e5464289925e0e0857a9fc537` の同ファイルも現行と同SHAであり、**今回の変更前からある不一致**と確認した。証拠：`playwright02.log` / `preexisting-playwright-mismatch.json`。期待値やゲームコードは変更していない。

初回の絞り込み `--grep '^source enumeration'` はフルテスト名に一致せず `No tests found`（exit 1）だったため、上記の絞り込みへ訂正した。初回ログも保存。全8件のsuiteは未実施、全件PASSとは扱わない。旧検証のライセンスSKIP指定も使っていない。

### クロエへの再開情報

今回の修正版についてUnity Native baseline / candidate、実画像・実操作は **NOT_RUN（依頼による担当分担）**。新しい実行名で、sourceから次を実行できる。

```sh
python3 tools/blender/run_native_panels.py baseline --name panels-harness-fixed-baseline03
python3 tools/blender/run_native_panels.py candidate --name panels-harness-fixed-candidate03
```

確認対象は、再投後に `panels-retry.json` が生成され、当該NullReferenceExceptionがなく、receiptと実際の棚卸し件数が確認できること。candidateは既存の港試作を含む `port-prototype` なので、旧rolloutとの距離・軌跡・配置の完全一致検査は別途版の適用範囲を照合する必要がある。今回のnull参照修正だけをもって旧パネルsuite全件合格・ゲーム完成とはしない。

push・外部送信・公開・Unityライセンス設定変更は行っていない。修正コミットはハーネス1ファイルのみ、本追記は後続の報告コミットに保存する。
