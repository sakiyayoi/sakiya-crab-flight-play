# カニ飛ばし Blenderモデル差し替えパイロット報告

2026-11-23 JST / Codex。**モールの破壊対象22家族74配置をBlenderモデルへ差し替え、指定のNative固定入力条件を通過。** ゲーム全体の完成・本人採用・公開とは区別する。Game Studio `game-playtest` は現セッションのcatalogに検出されず、Skill手順はNOT_RUN。

## 対象版と保存

- ブランチ：`blender-pilot`。`placement-fix` の `5b3e6272be86a23faa853554c426e42a18fd7cae` から作成。
- 実装・モデル・検証記録コミット：`9499746463a3e06f3655269c0d52621068437a85`。このレポートは後続の文書コミットにも保存する。
- 実行版識別：`BlenderFamilyOverride.Revision = blender-pilot-1`。既存NeonProtoやUIの版文字列は変更していない。
- Native最終：`evidence/blender-pilot/native-final/`。起動ログ・全C#署名：`evidence/blender-pilot-final/`。
- C# 280ファイルの連結SHA256：`728fb5475ea103d27bd0f5113d0cc3fd967a58ce587dcef66f130caa32c30b87`。
- モデル・コード・Blender原稿の個別SHA一覧：`evidence/blender-pilot/validated-content.json`。一覧自体のSHA256：`c496f3c8b45b3536a3402970fafd05e86e360f4b5dee5d9dab41a74f8bd9a92c`。実行後に全記載ファイルを読戻して不一致0。
- 指定のルート直下に本報告を保存し、同文を `source/docs/blender-pilot/` にGit保存。画像・主要Native記録・入力原本の写しもGit保存。
- 開始時から `Assets/FlightField.unity` に未コミット変更があった。今回のNativeは既存sceneを開くが保存しない方式とし、その内容を一字も変更せず、コミット対象から除外した。

## 対象家族の一覧と造形

母集団は **x=28〜135mの有効な破壊対象**。非破壊の静的背景・装飾は含めない。`NeonDensity/Placements.json` のモール追加は0件で、モールは既存Core/Atrium/RouteR29とMallAreasR5の家族変換、および `MallDensityProto/Placements.json` から組み立てられる。

- MallDensityProtoのJSON直置き：33件・12家族。
- その他の有効配置：41件。MallAreasR5が `street3-*` の家族をgift/sweets/capsuleへ変換するため、未変換JSONを単純に足した数を採用しない。
- 有効な合計：74件・22家族。新しいNative基準の実体から再集計し、74件すべての元JSON・元家族・位置・scale・yawも追跡した。詳細は `source/docs/blender-pilot/placement-inventory.json`。
- 全74件の位置・scale・yaw、家族ID、対象数、物理資料は維持。正方向モールだけを差し替え、負方向モールを含む他区画は従来表示。

| 家族ID | 件数 | 造形ポイント・監査処方箋への対応 |
|---|---:|---|
| `blade_sign` | 4 | 突き出し看板。支柱とブラケットを残し、T字を湯気つきカップ1個へ。 |
| `hvac_unit` | 1 | 室外機。大きな円形ファン・羽根・別列の通気ルーバー・据付脚。 |
| `market_crate` | 1 | 木箱。分離した横板・隙間・斜め補強・荷札。 |
| `vending_unit` | 1 | 自販機。暗いガラス面、3段×3本の飲料、選択ボタン、硬貨部、取出口。灰色型の判別欠落を避ける。 |
| `road_barrier` | 1 | 道路バリケード。幅広の脚、横梁、黄と藍の斜め縞。 |
| `facade_break_panel` | 2 | 壊れるタイル壁パネル。6面の目地と濃色カップ看板。薄い板の寸法を維持。 |
| `street_awning` | 2 | ひさし。4支柱、厚い縞の屋根、垂れ幕で日除けと読ませる。 |
| `steel_drum` | 1 | ドラム缶。太い明色リブ2本、濃い上下縁、凹んだ蓋、注入口。 |
| `street_bench` | 2 | ベンチ。座面と背板を分離し、肘掛けと脚を付ける。 |
| `mall_r5_gift_large` | 5 | ぬいぐるみ売場。2段棚、耳・顔・手足のあるクマ、リボン付き箱。 |
| `mall_r5_gift_small` | 7 | クマのぬいぐるみ。丸耳、目、鼻、口元、足裏、首リボン。 |
| `mall_r5_sweets_large` | 3 | 菓子売場。マカロンの厚いクリーム挟み線、棚、渦巻きキャンディ2本。 |
| `mall_r5_sweets_small` | 11 | ロリポップ。正面の太い渦巻き、円盤、長い棒を維持。 |
| `mall_r5_capsule_large` | 5 | 2連ガチャ。カプセル列のある窓、斜め反射線、回転ノブ、硬貨口、取出口。 |
| `mall_r5_capsule_small` | 12 | 小型ガチャ。曖昧だった半球を、カプセル窓・回転ノブ・受取口のある機械として造形。外寸と配置数は維持。 |
| `mall_r5_density_can` | 4 | 増量ドラム缶。steel_drumと同じ判別部、独立した家族FBX。 |
| `mall_r5_density_crate` | 4 | 増量木箱。market_crateと同じ板・斜め補強・荷札。 |
| `mall_r5_density_vending` | 3 | 増量自販機。全配置で飲料列・ガラス面・取出口を持つ。 |
| `mall_r5_density_stall` | 2 | 屋台。縞ひさし、支柱、商品トレイ、カップ看板のあるカウンター。 |
| `mall_r5_density_van` | 1 | 配送バン。荷室と運転席の段差、フロント/側面窓、4輪、ドア取手、濃色カップ印。 |
| `mall_r5_density_vertical_sign2` | 1 | 縦看板。旧14mの縦長寸法を維持し、3つのカップ絵記号へ。 |
| `mall_r5_density_hanging_sign2` | 1 | 吊り看板。吊り材2本と枠、カップ絵記号1個。 |

配色は既存R5の生成り・ミント・黄、濃い凹部色、既存Neonの藍系を役割別に使用。カニの赤 `#FF3131` をモデルに使用しない。ベタ色＋2段の明暗で面を分け、鏡面反射・プラスチック光沢・理由のないグラデーション・全面発光は付けない。ガラスは暗い窓と斜めの反射線で表現し、透明描画による重なり問題を増やさない。

Blenderの22枚は512×512。各家族の `.blend`、再生成用 `build_mall.py`、寸法と破壊片番号の `families.json` を `source/tools/blender/` に保存。外部モデルの流用なし。制作物の著作権はさきや。造形作者・使用ツールは同フォルダREADMEに記載。

[Blenderコンタクトシート](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-pilot/contact-sheet.png)

## 差し替え機構

従来経路：`City2Data.Load` がGeometry/Proposalsを併合 → `City2Data.Build` が有効配置を選択・家族変換・追加 → `CrabCityDensityR2.CityDensityPack.Build` がJSON頂点とUVから無傷メッシュ、固定部、破壊片を生成 → 従来の外観処理 → `City2Contacts` が当たり判定・破壊登録を行う。

追加したのは外観処理末尾の `BlenderFamilyOverride.Apply(built)`。FBXは `Assets/BlenderModels/Resources/BlenderModels/<家族ID>.fbx` にあり、モデルが見つからない家族は従来の表示をそのまま使う。見つかる場合だけx範囲を照合し、旧rendererを非表示にしてBlender製の無傷メッシュを描画する。元の物理資料・描画支点は残す。

- **原点と寸法**：Blender/FBXは底面中心・実寸メートル。生成時に旧家族の外寸へ合わせる。旧メッシュが持っていた非ゼロ底面や非対称中心は、描画子へのoffsetだけで再現する。配置JSONや配置Transformのscaleは変更しない。Unity取り込み後も22家族すべてで寸法と原点を照合した。
- **無傷と破壊**：FBX内に統合済み `Intact` と従来と同番号の `Part_n` を格納。無傷は1 renderer、破壊片は既存 `Shard_n` 支点へ接続する。Blenderで作った表面を旧破壊片領域に分割し、破壊片の数・番号・順序と支点を維持。旧固定部のノードもそのまま使う。
- **当たり判定**：`City2Contacts` は従来Proposalsのshard center/size、Geometryの固定部寸法からBoxColliderを生成し続ける。FBXにCollider/Rigidbodyを持たせず、新しい見た目からmass・閾値・慣性・判定寸法を再計算しない。
- **材質**：頂点色＋共有 `PilotFlat` 材質。Built-in用の専用シェーダで2段の陰影だけを計算する。既存の破壊時の材質反応は継承する。
- **軸変換**：初回Native画像で正面反転を発見し、Blender側の座標対応を修正。最終はBlender正面−Y → Unity正面−Z。自販機の前面頂点が−Zにあることを自動検査にも追加した。

参考にした公式仕様：[Blender 4.2 FBX](https://docs.blender.org/manual/en/4.2/addons/import_export/scene_fbx.html)、[Unity ModelImporter](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/modelimporter)。仕様の読取りと、このモデルの実取り込み成功は別々に確認した。

## 検証結果

| 検査 | 差し替え無効の新基準 | 最終版 | 判定 |
|---|---:|---:|---|
| seed | 42043 | 42043 | 一致 |
| 飛距離 | 3220.0815429688m | 3220.0815429688m | 2500m以上、差0 |
| 粉砕イベント数 | 145 | 145 | 96以上、差0 |
| 1/60秒のTick数 | 3668 | 3668 | 一致 |
| result到達 / physicsFault | 到達 / 空 | 到達 / 空 | PASS |
| 結果→Prepare→ready | 成功 | 成功 | PASS |
| 全City対象の物理署名 | 1407件 | 1407件 | 全件一致 |
| モール外の描画署名 | 1333件 | 1333件 | 全件一致 |
| 差し替え対象 | 0 | 74件・22家族 | 範囲外0 |
| route.csv / break-events.jsonl | 基準 | 最終 | 両ファイル全文一致 |
| Playwright照合 | — | 4 passed / 0 failed / 0 skipped | PASS |

入力 `accepted-input.json` SHA256：`c952b51c82b3ceb7838471da2d5f2bc93f4aa007f1e8cc560c046bf1036b4125`。原本は不変で、同一byteの写しを `source/tools/blender/accepted-input.json` に保存した。提示された従来値3,220m/126粉砕に対して、本検証手順の基準は145粉砕。過去のレビューにはポーズ・区画撮影等の事前処理があり、今回は同じ新しいレビュー手順のON/OFF比較を行った。**126→145をモデルによる改善とは扱わない。**

保護対象356ファイルのうち変更は外観接続の `City2Data.cs` だけ。全配置・Geometry・Proposals・物理/手触り設定・既存C#・ProjectSettings・開始時のsceneは不変。UIのファイル変更も0。既存WebGL配布物は今回ビルド・上書きしていない。

最終Nativeの終了コード0、C#コンパイルエラー0。既存 `WorldRouteR11` のedit-mode `Destroy` 診断は基準/最終でともに76件あり、ゼロエラーの通常ゲーム試験と偽らない。新規のFBX・shader・署名検査の失敗は最終で0。初回Playwrightは未導入の `@playwright/test` 入口を参照して開始前に失敗し、既存 `playwright/test` 入口へ修正した。新規インストールはしていない。

Native画像は `evidence/blender-pilot/native-final/` の照準・モール3地点・実飛行5地点・結果・再投。横1280×800、縦390×844を出力。Blender全家族のコンタクトシート、Nativeモール3地点、横飛行105m、縦照準/82m/105m、再投画像を実見した。縦の撮影時だけ既存カメラのaspect対応を再計算し、その後カメラ状態を復元する検証専用処理であり、製品カメラ/UIは変更していない。

[モール・ぬいぐるみ売場](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-pilot/native-final/mall-overview-43.png) / [菓子売場](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-pilot/native-final/mall-overview-79.png) / [ガチャ売場](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-pilot/native-final/mall-overview-118.png) / [実飛行105m](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-pilot/native-final/flight-105.png) / [縦の実飛行105m](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/evidence/blender-pilot/native-final/flight-105-portrait.png)

## 残った課題・判定の範囲

- **Game Studio `game-playtest` Skill手順：NOT_RUN**。現catalogに該当Skillが検出されず、別版を同じ手順として代用していない。通常のスワイプ/タップ、ブラウザのHUD・入力一巡、WebGL新ビルド、実iPhone、実聴、実FPS、本人の手触り/採用は未検証。今回のPlaywrightはNative証拠の照合であり、ブラウザを操作するテストではない。
- モデル自体は判別部を持つことを実画像で確認。ただし縦390pxの遠くにある物や密集時の重なりは残る。特に既存scale3.6の巨大ガチャは画面を大きく占める。今回の禁止範囲である配置/scaleを変えていないため、世界全体の見やすさを解決済みとはしない。
- 黒い丸い装飾、背景の反復、背景の人物・鳥、モールの静的装飾は従来のまま。74件の破壊家族の差し替えと、背景を含む全物のBlender化は別工程。
- 描画の外寸は従来と一致するが、細かな凹凸と旧BoxColliderは同一形状ではない。これは「旧当たり判定を維持」という指定どおりで、既存のCollider近似残差や床侵入を解消したとはしない。
- リスク評価：向き/寸法不一致は起きやすさ2＋影響3＝5として実画像・数値で修正確認済み。物理波及は1＋3＝4として全物理署名と同入力の全文一致を確認。繰り返しや画面内の重なりは2＋2＝4、既存配置を維持した今回の範囲外残件として記録。

## 再生成・再検証

Blender 4.2.23 LTS、Unity 6000.3.23f1、Built-in Render Pipelineを使用。Unityを終了した状態で、プロトタイプルートから：

```sh
PYTHONDONTWRITEBYTECODE=1 ~/Applications/Blender.app/Contents/MacOS/Blender \
  --background --factory-startup --python source/tools/blender/build_mall.py
~/.local/bin/codex-creative python source/tools/blender/contact_sheet.py
BLENDER_PILOT_OUTPUT="$PWD/evidence/blender-pilot/native-new" \
  python3 tools/stage65-unity.py blender-pilot-new BlenderPilotReview.Run
```

出力名は未使用のものを指定する。比較用基準は `BLENDER_PILOT_BASELINE=1` を付け、別プロセス・別出力先で採る。Nativeは保存済みsceneを開くだけで、`BuildFlight.PrepareScene` のscene保存は呼ばない。

今回実行したPlaywright：

```sh
cd source
NODE_PATH='/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules' \
  npx --offline --prefix .git/placement-fix-qa/runtime --no-install \
  playwright test --config qa/blender-pilot/playwright.config.cjs
```

4 passed / 0 failed / 0 skipped。生記録：`evidence/blender-pilot/playwright-final.log` と `playwright.json`。別Native出力を照合するときは `BLENDER_PILOT_CANDIDATE=native-new` を指定する。

外部送信・Git push・公開・課金・認証変更なし。案件のKB参照禁止に従いTEAM本文は未確認。Studio共通入口も現catalogでは未接続。既存HANDOFFとtasksに状態を記録した。
