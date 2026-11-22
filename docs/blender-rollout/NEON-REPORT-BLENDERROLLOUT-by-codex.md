# Blenderモデル全区間展開 報告

2026-11-23 JST / Codex / branch `blender-rollout`

**パイロットのBlenderFamilyOverrideを、9区画・正負両方向の実効City 41家族・1,407件へ展開した。新規19家族をBlenderで制作し、18区分すべてで残った未モデル家族0。** 同入力の飛距離、粉砕イベント、route.csv、break-events.jsonl、全1,407件の物理署名はパイロット基準と完全一致した。

これはCity家族機構の展開結果である。別の描画方式を使う店内等40件と、接触する人物22件・鳥12件は未変更で、「世界の物をすべてBlender化」という全体方針に対しては残る。背景・非破壊の静的装飾も今回の対象外。ゲーム全体の完成・本人採用・公開の判定は行っていない。

## 版と保存先

- 基準：`blender-pilot` / `f3911b0473d84b7aece4f08c493c3dcd170abb11`。
- 実装・モデル・検証証拠のcommit：`825a34f327833a71925d30d86516e23d62201a22`。報告追加commitはこの子commitで、実装とモデルは同じ。実装commit内の147ファイルを読み戻し、保存済みSHA一覧との不一致0を確認した。
- 実行版：`blender-rollout-1`、Native最終は `evidence/blender-rollout/native-final02/`。Unity `6000.3.23f1`、Built-in Render Pipeline、Blender `4.2.23 LTS`。
- C# 281ファイルの連結SHA256：`b3d7ebd335bd3fd2360a36293017ab1284863f2629981feaf36500d1bec8c2bc`。
- コード・FBX・原稿等147ファイルのSHA一覧：`evidence/blender-rollout/validated-content.json`、一覧SHA256 `57b078b0c78c1f5af3a287baf82d613b59eb52d0b057325262fae7303560f543`。
- Git内の証拠一覧：`source/docs/blender-rollout/evidence-sha256.json`、一覧SHA256 `c9993d4eff7a6458fe3fa9ebb9059d41fa197b5dd73ee1563e87bca160a56bd2`。Native生記録、画像、Playwright記録を同フォルダにも保存。
- 開始時から未コミットだった `FlightField.unity` はSHA256 `058be0b3456083199edfc5b9650b233cbb63209249d00f4c1a764443c68fb5bb` のまま保護し、commitから除外した。同一byteをローカルの `evidence/blender-rollout/FlightField-at-start.unity` に保存。これは検証時のSceneで、基準commitのSceneとの差を勝手に取り消していない。
- 今回はソース・モデル・証拠のローカル納品。WebGL配布物の作成・更新、Git push、公開、外部送信は行っていない。

## 実体からの棚卸し

`City2Data.Build`で有効配置の選択、MallAreasR5等の家族変換・追加を終え、実際に登録された `CityVisualBreakable` からplacement ID、実効asset ID、位置、scale、物理署名を収集した。未変換の配置JSONや非破壊装飾の件数を足していない。区画は現行 `WorldRouteR11`、方向は配置xの符号で分けた。

基準では74件のみBlender表示。最終は1,407件すべてが1個のBlender無傷モデルを持ち、登録漏れ0・二重置換0。新規19家族の配置は624件、既存22家族の配置は783件。正方向1,251件、負方向156件。家族は区画をまたぐため、区画別家族数の和は世界の家族総数ではない。

| 区画 | 両方向の異なる家族数 | 正方向 家族数 / 件数 | 負方向 家族数 / 件数 | 合計件数 | 残った未モデル家族 |
|---|---:|---:|---:|---:|---:|
| 生簀 `tank` | 1 | 1 / 3 | 0 / 0 | 3 | 0 |
| 店内 `restaurant` | 6 | 6 / 17 | 3 / 3 | 20 | 0 |
| 商業施設 `mall` | 22 | 22 / 74 | 15 / 39 | 113 | 0 |
| オフィス街 `office` | 12 | 12 / 240 | 11 / 33 | 273 | 0 |
| 港市場 `port` | 14 | 14 / 145 | 12 / 20 | 165 | 0 |
| 高架 `viaduct` | 11 | 11 / 169 | 9 / 14 | 183 | 0 |
| 国・広域 `country` | 13 | 13 / 273 | 9 / 17 | 290 | 0 |
| 上空への発射区画 `launchrange` | 12 | 12 / 195 | 8 / 15 | 210 | 0 |
| 宇宙・終端仮配置 `orbit` | 11 | 11 / 135 | 7 / 15 | 150 | 0 |
| **世界の重複除外合計** | **41** | **1,251件** | **156件** | **1,407** | **0** |

生簀の負方向はCity家族実体0件。写真は同じ入口を撮影しているが、正方向の3件を負方向に数え直していない。区画別の全placement ID、家族別件数、元GeometryとそのSHAは [inventory.json](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-rollout/inventory.json) と `source/tools/blender/rollout-families.json` に保存した。

世界の物理登録総数は1,481件。既存 `CityContactTargets=1,441` はCity 1,407件にLifeContactsの人物・鳥34件を加えた値で、モデル家族実体の件数とは異なる。残る40件はRestaurant/Core/Overdriveの別系統パネル。**「未モデル0」はCity家族についての集計であり、1,481件すべてをモデル化したという意味ではない。**

## 家族ごとの造形ポイント

全モデルはベタ色の頂点色と既存 `PilotFlat` 材質を使い、2段の陰を維持。プラスチック光沢やグラデーションは追加していない。新規の藁色は `#D6B568`、航路標識・警告の落ち着いた錆朱は `#C66B4E`。カニ専用の `#FF3131` は物体モデルには使っていない。

原稿は実寸メートル、原点は底面中心。旧Geometryの外寸へ原稿を合わせ、既存の非対称中心や非ゼロ底面はパイロットと同じ描画子offsetで再現する。配置データ・親Transformのscale/yawは変更していない。破壊片は旧shard番号と旧支点に対応させ、Collider・質量・閾値・物理計算は旧Geometry/Proposalsをそのまま使う。

### 今回制作した19家族

| 家族ID | 全世界の件数 | 造形ポイント・監査処方箋への対応 |
|---|---:|---|
| `cargo_container_neon6` | 33 | 波板の縦リブ、両開き扉、扉線、ロック棒と取っ手、角金具 |
| `crate_stack` | 4 | 独立した木箱3個、板の隙間、斜め補強、上下の積み重なり |
| `delivery_van` | 10 | 荷室と運転席、窓、4輪、ドア取手、カップ印（pilotと共通） |
| `dish_antenna_neon6` | 40 | 本物の凹面のお椀、縁、中心受信棒と受信部、傾斜ヒンジ |
| `farm_fence_neon6` | 62 | 尖った支柱、横木2段、斜めの木の補強 |
| `flywheel_accumulator_r12` | 40 | 開いた軸穴、スポーク付き重い輪、巻きばね、軸受支柱（蓄力機の家族意味を保持） |
| `harbor_buoy_neon6` | 32 | 浮体、錆朱と白の横縞、上部の小ライトとケージ、係留環 |
| `hay_roll_neon6` | 62 | 両端面の太い渦巻き、黄土の藁色、結束バンド2本、藁の筋 |
| `irrigation_tank_r12` | 50 | 給水口、給水タンクのドーム、ポンプ、垂れたホース、水位計 |
| `junction_cabinet` | 12 | 扉とヒンジ、通気口、取手、稲妻入り電気警告表示 |
| `market_stall` | 4 | 縞の日除け、柱、商品トレイ、カウンター（pilotと共通） |
| `oxygen_tank_neon6` | 22 | 2本の肩付きボンベ、細い首、手回しバルブ、固定帯、O表示 |
| `port_lattice_loader_r10` | 8 | トラスの門形クレーン、斜材、巻上げ台車、ケーブルとフック |
| `propellant_tank_neon6` | 40 | 丸い肩、手回しバルブ、圧力計、黄と藍の警告帯 |
| `rail_glass_planter_display_r10` | 36 | ガラス枠、斜めの反射線、鉢、土、大きな葉、2段棚 |
| `road_sign_neon6` | 39 | 道路標識の枠と支柱、意味のある大きな方向矢印1個 |
| `satellite_neon6` | 23 | 金色の機器本体、両翼の太陽電池格子、通信アンテナ |
| `solar_array_r12` | 68 | 支柱、左右のパネル、太陽電池セルと導体線 |
| `traffic_signal_neon6` | 39 | 3色の丸い灯器、各灯の庇、縦長の筐体と支柱 |

負方向の実画面を見て、新規 `delivery_van` に反対側の窓・取手・荷室の線・カップ印、`junction_cabinet` に裏側の点検扉・通気口・取手、`solar_array_r12` と `satellite_neon6` に両面の太陽電池格子を追加した。既存の180度yawを変えずに特徴が見えるようにした。

`flywheel_accumulator_r12` は現行家族の「蓄力機」の意味を維持した。旧表示のリブ付き円筒をケーブルドラムと決めつけず、軸穴・スポーク・巻きばねを付けた。

### 再利用したパイロット22家族

下表は元パイロットの造形を全区画へ適用したもの。22 `.blend` / FBXは基準commitとbyte単位で一致し、正方向モール74件の描画署名も一致している。

| 家族ID | 全世界の件数 | 維持した造形ポイント |
|---|---:|---|
| `blade_sign` | 190 | 突き出し看板。支柱とブラケットを残し、T字を湯気つきカップ1個へ。 |
| `facade_break_panel` | 16 | 壊れるタイル壁パネル。6面の目地と濃色カップ看板。薄い板の寸法を維持。 |
| `hvac_unit` | 14 | 室外機。大きな円形ファン・羽根・別列の通気ルーバー・据付脚。 |
| `mall_r5_capsule_large` | 8 | 2連ガチャ。カプセル列のある窓、斜め反射線、回転ノブ、硬貨口、取出口。 |
| `mall_r5_capsule_small` | 17 | 小型ガチャ。曖昧だった半球を、カプセル窓・回転ノブ・受取口のある機械として造形。外寸と配置数は維持。 |
| `mall_r5_density_can` | 4 | 増量ドラム缶。steel_drumと同じ判別部、独立した家族FBX。 |
| `mall_r5_density_crate` | 4 | 増量木箱。market_crateと同じ板・斜め補強・荷札。 |
| `mall_r5_density_hanging_sign2` | 1 | 吊り看板。吊り材2本と枠、カップ絵記号1個。 |
| `mall_r5_density_stall` | 2 | 屋台。縞ひさし、支柱、商品トレイ、カップ看板のあるカウンター。 |
| `mall_r5_density_van` | 1 | 配送バン。荷室と運転席の段差、フロント/側面窓、4輪、ドア取手、濃色カップ印。 |
| `mall_r5_density_vending` | 3 | 増量自販機。全配置で飲料列・ガラス面・取出口を持つ。 |
| `mall_r5_density_vertical_sign2` | 1 | 縦看板。旧14mの縦長寸法を維持し、3つのカップ絵記号へ。 |
| `mall_r5_gift_large` | 8 | ぬいぐるみ売場。2段棚、耳・顔・手足のあるクマ、リボン付き箱。 |
| `mall_r5_gift_small` | 11 | クマのぬいぐるみ。丸耳、目、鼻、口元、足裏、首リボン。 |
| `mall_r5_sweets_large` | 6 | 菓子売場。マカロンの厚いクリーム挟み線、棚、渦巻きキャンディ2本。 |
| `mall_r5_sweets_small` | 17 | ロリポップ。正面の太い渦巻き、円盤、長い棒を維持。 |
| `market_crate` | 160 | 木箱。分離した横板・隙間・斜め補強・荷札。 |
| `road_barrier` | 24 | 道路バリケード。幅広の脚、横梁、黄と藍の斜め縞。 |
| `steel_drum` | 210 | ドラム缶。太い明色リブ2本、濃い上下縁、凹んだ蓋、注入口。 |
| `street_awning` | 19 | ひさし。4支柱、厚い縞の屋根、垂れ幕で日除けと読ませる。 |
| `street_bench` | 12 | ベンチ。座面と背板を分離し、肘掛けと脚を付ける。 |
| `vending_unit` | 55 | 自販機。暗いガラス面、3段×3本の飲料、選択ボタン、硬貨部、取出口。灰色型の判別欠落を避ける。 |

## 画像

- [新規19家族のコンタクトシート](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-rollout/new-families-contact-sheet.png)：Blenderによる各家族の512×512レンダーを合成。
- [Native正方向・9区画一覧](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-rollout/native-positive-overview.png) / [Native負方向・9区画一覧](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-rollout/native-negative-overview.png)：最終Unity描画18枚の一覧。画像生成による代用ではない。
- 区画別Blenderコンタクトシート：`evidence/blender-rollout/<区画ID>-contact-sheet.png` 全9枚。各家族の実体件数を正負別に記載。
- Native原寸：`evidence/blender-rollout/native-final02/sector-<区画ID>-positive.png` / `negative.png`、各1280×800。各区画の特徴物と既存背景が一緒に読める検証用カメラを使い、製品カメラは変更していない。
- 同版の照準、実飛行11地点、結果、再投も保存。実飛行の各地点は横1280×800と縦390×844。HTMLのHUDや通常ブラウザの操作画面ではない。

実見した範囲：新規19家族コンタクトシート、初回展開版の全18区画画像、最終版の正負9区画一覧、最終版の負オフィス/負宇宙/負店内/正広域/正発射区画の原寸、1370m/2800mの縦飛行、結果・再投。全画像の存在・物理寸法を自動照合し、画像を見ていない部分を視認合格とはしていない。

## 検証結果

基準は開始時の `blender-pilot` を新しいUnityプロセスで再実行した `native-baseline`。最終版をOFFにして基準扱いにはしていない。基準のroute.csv、break-events.jsonl、targets.jsonは保存済みpilot最終証拠ともbyte一致した。

| 検査 | pilot基準 | rollout最終 | 結果 |
|---|---:|---:|---|
| seed | 42043 | 42043 | 一致 |
| accepted-input.json | 同一ファイル | 同一ファイル | SHA一致 |
| 飛距離 | 3220.08154296875m | 3220.08154296875m | 完全一致 |
| 粉砕イベント | 145 | 145 | 完全一致 |
| 1/60秒Tick | 3668 | 3668 | 完全一致 |
| route.csv | 基準 | 最終 | 全byte一致 |
| break-events.jsonl | 基準 | 最終 | 全byte一致 |
| City物理署名 | 1,407件 | 1,407件 | 不一致0 |
| 家族ID・位置・scale | 1,407件 | 1,407件 | 不一致0、yawも物理署名内で一致 |
| 正方向モール74件の描画署名 | 基準 | 最終 | 不一致0 |
| Blender置換済み | 74件 | 1,407件 | 全City対象に1個ずつ |
| FBX寸法・底面原点・破壊片番号 | — | 全41家族 | PASS |
| 物理・配置・feel・UI・Scene等の保護ファイル | 開始時356ファイル | 変更0 | SHA一致 |
| 既存パイロットblend/FBX | 22家族 | 変更0 | byte一致 |
| 結果 / physicsFault / 再投ready | 到達 / 空 / 成功 | 到達 / 空 / 成功 | PASS |
| PlaywrightによるNative証拠照合 | — | 6 passed / 0 failed / 0 skipped | PASS |

入力SHA256：`c952b51c82b3ceb7838471da2d5f2bc93f4aa007f1e8cc560c046bf1036b4125`。
route.csv SHA256：`797e6958bca944205255cc15675eee82a8895944b7e66bd54e17941656dc1892`。
break-events.jsonl SHA256：`a86c7bbc8086731ae5ee2dc85bfb07af1919b36a4378b2d4dedb28bf0bd896ae`。

Native最終実行は2026-11-23 03:09:24〜03:10:04 JST、終了コード0、C# compiler error 0。既存のedit-mode Destroy診断は基準と最終の両方に76件ある。「全ログ警告0」とは報告しない。

初回Playwrightは5 passed / 1 failed。原因は検査側がLifeContactsを含む既存City集計1,441を家族実体1,407と同一視したこと。元ソースの加算を確認し、既存集計はpilot基準との比較、家族の被覆は1,407個の実体ID・登録・物理署名との比較に分けた。その後6 passed。さらに負面造形を修正しNativeを再実行、最終版でも6 passed。基準値や件数を減らして未モデルを隠したものではない。初回ログも保存した。

## 再生成・再検証

家族単位のスクリプトと `.blend` は `source/tools/blender/`。詳細は [README-rollout.md](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/tools/blender/README-rollout.md)。外部モデルや課金サービスは使用していない。

Unityを終了し、`source`で新規19家族を生成する：

```sh
PYTHONDONTWRITEBYTECODE=1 ~/Applications/Blender.app/Contents/MacOS/Blender \
  --background --factory-startup --python tools/blender/build_rollout.py
```

1家族だけなら末尾に `-- hay_roll_neon6` などを指定する。既存22原稿は保護し、このスクリプトの再生成対象にしていない。

コンタクトシートを更新する：

```sh
~/.local/bin/codex-creative python tools/blender/contact_rollout.py
```

プロトタイプのルートで、新しい保存先へNativeを実行する：

```sh
BLENDER_ROLLOUT_OUTPUT="$PWD/evidence/blender-rollout/native-new" \
  python3 tools/stage65-unity.py blender-rollout-new BlenderRolloutReview.Run
```

同じScene、seed42043、accepted-input.json、feelで動かす。保存済みの基準を保護し、新基準が必要ならpilot commitの隔離コピーと上記Scene写しを使う。原本Sceneを上書きして比較しない。

`source`で再集計・検査する：

```sh
python3 tools/blender/inventory_rollout.py ../evidence/blender-rollout/native-new/targets.json
BLENDER_ROLLOUT_CANDIDATE=native-new \
  NODE_PATH='/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules' \
  npx --offline --prefix .git/placement-fix-qa/runtime --no-install \
  playwright test --config qa/blender-rollout/playwright.config.cjs
```

今回の最終実行コマンドも同じで、candidateは `native-final02`。これはNativeの生記録をPlaywrightで照合する試験であり、ブラウザを開いてスワイプした試験ではない。

## 残った課題と検証境界

| 項目 | 状態・根拠 | 起きやすさ+影響 | 次の一手 |
|---|---|---:|---|
| 別描画系の40パネルと人物22・鳥12 | 未変更。City家族のゼロ集計に含めていない | 全体方針の残件 | 別系統のrender/破片/rig接続を特定してBlender化する工程が必要 |
| 負方向で見える旧パイロットモデルの裏面 | sweetsの渦巻き、看板の記号等が裏から弱い。モデル未適用ではない | 3+1=4 | 既存モデルの裏側造形を個別調整するなら正方向74件への影響も再確認 |
| 既存のscale・重なり・反復 | 巨大な蓄力機/太陽電池、負方向店内の高い配置、薄い密度等を維持 | 3+1=4 | 配置変更を含む別工程。本依頼では配置・scale不変を優先 |
| 通常スワイプ/タップ・HUD一巡 | NOT_RUN。Nativeのaccepted-input再生のみ | 未確認 | 通常操作を同版で実施 |
| Game Studio game-playtest | NOT_RUN。現セッションのnative skill catalogに接続入口なし | 未確認 | 正規の接続済みSkillで実物確認。旧cached版で実行済みとはしない |
| 実聴・物理iPhone・実FPS・本人の手触り採用 | NOT_RUN | 未確認 | それぞれ対象機器・同版で確認 |
| 既存の床一時侵入・Collider近似残差・モール反復FAIL | 未解消。今回は物理/配置を変更せず同入力一致を確認 | 既存記録を継続 | 直った扱いにせず、許可された該当工程で扱う |

物理を変更していないため弱20投・強20投の再試行は行っていない。同入力完全一致を、全入力の物理問題解消・性能保証へ拡大解釈しない。背景・静的装飾の造形品質も今回の判定に含めない。

案件のKB参照禁止を維持しTEAMは未確認。Studio共通入口も現catalogでは未接続。既存HANDOFF一箇所とtasks.mdへ結果を記録。本人採用・公開・外部送信・Linear更新を完了扱いにしていない。
