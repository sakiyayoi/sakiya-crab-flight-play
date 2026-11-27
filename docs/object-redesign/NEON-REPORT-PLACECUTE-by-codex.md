# オブジェクト再設計 第2便：配置・拡大・顔

2026-11-28 JST。対象 `object-redesign`、基点 `b8f8bb59d58589c06db9d46bd957e35aba4597a3`。指定のソース修正とオフライン検証の報告。Unity実画面・固定入力リプレイの合格とは分ける。

## 配置と全種の当たり点検

| ID | 旧中心 (x,y,z) | 新中心 (x,y,z) |
|---|---|---|
| restaurant-brake | (18,12,0) | (19,20,0) |
| mall-brake-a | (64,9,0) | (64,20,0) |

それ以外の14個の中心は不変。+x 16個、−x側は既存の鏡像生成による16個、合計32個で不変。brake 20 < death 26を保持。作用量、個数、基礎物理、既存カメラ・ヒットストップの新値は不変。

点検は全位相の移動範囲とBoxColliderのAABB。半辺 `1.15 × 1.6 = 1.84m`、一辺 `2.3 → 3.68m`。spinは回転正方形なのでX/Y最大半幅 `1.84√2 = 2.602153m`。移動振幅は旧値のまま、周期・方式も不変。左右とも同じY範囲になる。

**静的検査の限界**：本人提示のピーク約14〜15mを、まず世界Y `[0,15]` の設計帯として比較する。これは固定入力の再シミュレーションではない。既存 `State.height` は `Body.position.y − restHeight` であり、カニの形状・姿勢を含む占有範囲とは違う。そこで旧設計のカニ余裕3.5mを加えた帯 `[-3.5,18.5]` との交差も別列に残す。標準投げの「絶対に当たらない」を静的PASSにしていない。

| ID / 種 | 中心 (x,y) | 全位相X範囲 | 全位相Y範囲 | 0〜15m帯 | カニ余裕+3.5m | 判定 |
|---|---|---|---|---|---|---|
| restaurant-entry / boost | (8,3) | 5.510〜10.490 | 1.160〜4.840 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| restaurant-bounce / bounce | (14,2) | 12.160〜15.840 | -0.390〜4.390 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| restaurant-brake / brake | (19,20) | 17.160〜20.840 | 17.810〜22.190 | 掛からない | 掛かる可能性 | 指定高所へ移動・実接触はNativeで確認 |
| restaurant-spin / spin | (20,5) | 17.398〜22.602 | 2.398〜7.602 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| restaurant-death / death | (22,26) | 19.260〜24.740 | 24.160〜27.840 | 掛からない | 掛からない | 高所危険・中心不変 |
| restaurant-exit / blast | (24,3) | 22.160〜25.840 | 0.980〜5.020 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| mall-entry / boost | (32,4) | 29.510〜34.490 | 2.160〜5.840 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| mall-bounce-a / bounce | (43,2) | 41.160〜44.840 | -0.390〜4.390 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| mall-spin-a / spin | (55,7) | 52.398〜57.602 | 4.398〜9.602 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| mall-brake-a / brake | (64,20) | 62.160〜65.840 | 17.810〜22.190 | 掛からない | 掛かる可能性 | 指定高所へ移動・実接触はNativeで確認 |
| mall-boost / boost | (75,4) | 72.510〜77.490 | 2.160〜5.840 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| mall-bounce-b / bounce | (86,2) | 84.160〜87.840 | -0.390〜4.390 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| mall-spin-b / spin | (98,7) | 95.398〜100.602 | 4.398〜9.602 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |
| mall-brake-b / brake | (108,9) | 106.160〜109.840 | 6.810〜11.190 | 掛かる | 掛かる可能性 | 未指定の3個目。既存の危険を保持、要Native確認 |
| mall-death / death | (114,26) | 111.260〜116.740 | 24.160〜27.840 | 掛からない | 掛からない | 高所危険・中心不変 |
| mall-exit / blast | (124,4) | 122.160〜125.840 | 1.980〜6.020 | 掛かる | 掛かる可能性 | 意図した正作用の接触帯 |

brake以外では、boost/bounce/spin/blastの帯との交差は元から意図した正作用の接触。deathは拡大・全移動位相・カニ余裕を含めても帯外。今回の静的範囲で、拡大によって新たに避けるべき危険となる非brake個体は見つからず、追加移設は行っていない。

**残る2点**：移設したbrakeは下端17.81mで名目帯と2.81m離れるが、旧カニ余裕を含む上端18.5mとは0.69m重なるため、指定座標だけで実接触なしを保証しない。またコードには3個目の `mall-brake-b (108,9,0)` があり、今回指定された2個に含まれないため維持した。このもちのY範囲6.81〜11.19mは帯に掛かる。既存配置時から掛かっていた個体で、解消済みとはしない。固定入力を同版で再生し、後述の作用ログで3個とも実接触を確認する。

[再現スクリプト](placecute/audit.py)／[全16個の数値JSON](placecute/hit-map.json)。判定帯は検証専用で、本編の条件分岐には使っていない。

## 拡大と顔・丸みの内訳

`ObjectRules.ObjectScale = 1.6` を共通倍率にする。見た目はルート内のstory全体に掛け、コライダはHalfSizeへ同じ倍率を掛ける。rootの鏡像・配置・移動には倍率を掛けない。衝突処理 `ObjectRuntime.cs` はbyte不変。

| 種 | 顔の位置・表情 | 丸み |
|---|---|---|
| 扇風機 | 固定ハブを広げて、縦目と小さな笑顔。網と羽より手前 | 4枚の羽を楕円球、ガード・格子も丸い細材 |
| フライパン | カメラ側を向く鍋面、少し傾いた笑い目と口 | 平たい円盤＋楕円の取っ手。既存の本体傾斜−12°を保持 |
| 回転寿司 | 回るネタと別の皿前縁、細い目と小さな口 | レーン・スラット・米・ネタを丸い形へ。皿とネタの回転は維持 |
| もち | 白い体の前面、丸い目2つと小さなw口 | 横長の白い球体と紫縁を保持。新しい頬色などは追加しない |
| 花火玉 | 玉の前面、縦目と驚いた丸い口 | 球・丸い導火線先端を保持 |
| かに漁師 | 黒い頭の前に既存マゼンタの小さな顔面、暖墨の目と口 | 丸い帽子、カプセルの胴と両腕、丸い長靴。別体の柄・格子網を保持 |

目と口は `#241E2E`、小さな球を薄く潰した形で表面から少し浮かせる。顔のみEmission=0、既存の共通MaterialをPropertyBlockで上書きし追加Materialを作らない。全primitiveの描画用Colliderを無効化・破棄する既存方式を保持。顔を含め新しい物理形状を足さない。6色・Emission=4・花火の既存2Hz発光・作用VFX量は不変。振幅も変更なし。

[ソース形状プレビュー](placecute/shape-preview.png)を実見した。実 `ObjectVisual.Build` を記録用Unityダブルで実行して得た[全形状](placecute/shape-record.json)をBlenderへ写した診断レンダー。6種の顔と漁師の人型を確認。カメラ方向はゲームの `(0,5.3,-16)` と同方向。**Unity画面ではなく、照明・色変換・見かけの発光・スケール感のゲーム実証ではない**。画像の左上から寿司・フライパン・扇風機、左下から漁師・花火・もち。

## action-events.jsonl

既存の開口部用PortNativeReviewは旧branchで未追跡の注入ファイルだったため、現行build copyの本文を読み、同じクラス・Run入口を `Assets/PortPrototype/Editor/PortNativeReview.cs` に版管理した。[元本文](placecute/native-entry-baseline.cs.txt)と[由来・SHA](placecute/native-entry-provenance.json)を保存。固定入力・seed42043・カメラ・既存撮影・既存結果・再投手順はbyte比較で不変。

出力は `PORT_REVIEW_OUTPUT/camon/action-events.jsonl`。RunOneをcamoffでも呼ぶ場合は同じくcamoff配下に出る。`ReviewSimulation.ObjectHits` の権威ある消費履歴を、`game.Prepare("")` が消す前に全件出す。UIの32件リングに依存しない。1行1作用、`kind`（小文字の種名）、`position`（発動した瞬間の世界座標x/y/z、動いた位置・鏡像込み）、`time`（既存のシミュレーション秒）。0件なら空ファイル。

検証用の形式例（実投ログではない）：
```json
{"kind":"brake","position":{"x":19.0,"y":20.0,"z":0.0},"time":0.5}
```

既存本編の履歴生成・発動・加速度・状態・表示イベントには一切書き込まない。Editor専用のファイルI/O。新規契約は本番writerを抽出して実行し、全6種を含む48件・順番・世界座標・時刻・元リスト不変・空投を確認。シリアライズはテスト用System.Text.Jsonアダプタで、別途実Unity JsonUtilityを参照したEditor型検査を通す。

**次のNative検証では、旧 `/tmp/PortNativeReviewOpening.cs` をコピーして上書きしないこと。** 今回commit済みのEditor helperをそのまま使い、`PORT_REVIEW_INPUT` は既存accepted-input.json、`PORT_REVIEW_OUTPUT` は新規フォルダを指定し、`-executeMethod PortNativeReview.Run` で既存の検証経路を実行する。旧成果物・公開物を上書きしない。


## オフライン検証結果

| 検査 | 結果 | 根拠 |
|---|---|---|
| Editor / WebGL条件分岐型チェック（136ソース、実Unity参照・Effekseer 1.80.8） | errors 0 / 0、既存warnings 18 / 17、Effekseer warnings 0 | [typecheck.log](placecute/typecheck.log) / [全ソースhash](placecute/compile.json) |
| 新規Editor専用PortNativeReview＋実BlenderPilotImport | errors 0 / warnings 0 | [native-typecheck.log](placecute/native-typecheck.log) / [実行契約](placecute/contracts.log) |
| 既存ObjectRules / Stage 1境界 | 85 + 235 passed / 0 failed | [Rules](placecute/rules-contracts.log) / [境界JSON](placecute/contracts.json) |
| 既存Effekseer / Juice | 45 + 33 passed / 0 failed | [Bridge](placecute/bridge-contracts.log) / [Juice](placecute/legacy-juice-contracts.log) |
| 既存ストーリー形状 / 境界 | 46 + 13 passed / 0 failed | [形状](placecute/shapes-contracts.log) / [境界](placecute/story-contracts.json) |
| 既存blast回収・消費 / 境界 | 258 + 9 passed / 0 failed | [実行](placecute/unit-contracts.log) / [境界](placecute/blast-contracts.json) |
| 既存PortRules | 30 passed / 0 failed | [port-contracts.log](placecute/port-contracts.log) |
| 既存カメラ・ヒットストップ / 境界 | 151 + 10 passed / 0 failed | [実行](placecute/camhit-unit-contracts.log) / [境界](placecute/camhit-contracts.json) |
| 新規拡大・全6顔・丸み | 66 passed / 0 failed | [faces-contracts.log](placecute/faces-contracts.log) |
| 新規JSONL実出力 | 51 passed / 0 failed | [action-log-contracts.log](placecute/action-log-contracts.log) |
| 新座標・16配置点検・本編不変・Native接続 | 22 passed / 0 failed | [placecute-contracts.json](placecute/placecute-contracts.json) |
| Playwright（既存35＋新規1） | **36件すべて合格確認、未解消failed 0 / skipped 0 / flaky 0** | [初回35合格＋1タイムアウト](placecute/playwright-initial.log) / [復旧後その1件合格](placecute/playwright-recovery.log) / [同版の網羅記録](placecute/playwright-coverage.json) |
| 保護26ファイルSHA256 | main実体26 / branch実体またはsparse Git26、前後一致・変更0 | [前](placecute/protected-before.json) / [後](placecute/protected-after.json) |
| カメラ・停止・物理・色・演出設定の保全 | 変更対象外368ファイルの実体を旧基点とbyte照合＋現tipの12主要ソースbyte不変 | [camhit境界](placecute/camhit-contracts.json) / [placecute境界](placecute/placecute-contracts.json) |

旧テスト件数は減らしていない。shapeのレーン期待形をCube→Sphereへ更新し、旧Rules完全不変の比較には今回の2座標と正確な1.6倍定数だけを例外として戻し照合する。カメラ境界では変更対象Visualを形状実行契約で検査し、変更外の全ファイルは実体のまま照合。旧ログは上書きしていない。

初回Native型検査はUnityEditorの重複参照でCS0433、余計な参照を除去して成功。[初回ログ](placecute/native-typecheck-initial.log)を保持。本編コードの不合格ではない。Playwright初回のカメラ契約1件は、iCloudの未取得依存21ファイルを読み出す間に160秒でタイムアウト。OSの標準取得APIで対象のみ取得し、全21実体を読戻し後、**失敗した1件だけ**再実行して合格した。閾値・timeoutを延長していない。[未取得一覧](placecute/dataless-inventory.json) / [取得受付](placecute/hydration.log) / [実体読戻し](placecute/hydration-readback.json)。その他35件は同じソース版で初回合格した結果を再利用する。

Playwrightの画像は既存HTML部品の合成状態。Unity/WebGL本体の描画確認ではない。今回の新規部品画像も上記Blender診断レンダーであり、3Dゲームの実画面証拠へ読み替えない。

### 再実行（worktreeルート）

```sh
export PYTHONDONTWRITEBYTECODE=1
export KANI_OBJECT_EVIDENCE_DIR=docs/object-redesign/placecute
export KANI_EFFEKSEER_EVIDENCE_DIR=docs/object-redesign/placecute
export KANI_JUICE_EVIDENCE_DIR=docs/object-redesign/placecute
export KANI_STORY_EVIDENCE_DIR=docs/object-redesign/placecute
export KANI_CAMHIT_EVIDENCE_DIR=docs/object-redesign/placecute
export KANI_BLAST_EVIDENCE_DIR=docs/object-redesign/placecute
python3 docs/object-redesign/typecheck.py
python3 docs/object-redesign/story/contracts.py
python3 docs/object-redesign/blastfix/contracts.py
python3 docs/object-redesign/camhit/port-contracts.py
NODE_PATH=/Users/konomichiharu/.npm/_npx/e41f203b7505f1fb/node_modules npx --no-install playwright test -c docs/object-redesign/placecute/playwright.config.cjs
```

新規契約はPlaywrightから `python3 docs/object-redesign/placecute/contracts.py` を呼ぶ。今回の証跡の読戻しは [verify-evidence.py](placecute/verify-evidence.py)（今回の35+1回復記録用） / [結果](placecute/verify-evidence.log)。将来の再実行時は既存証跡を保存してから別出力先を使う。

## 保存版・残件

**実装・検証根拠コミット `bd33c3e40b27804dde23a35fdf52d9686ea4cf0b`**。72ファイルをGitから読み戻し、作業実体と全件byte一致（[読戻し記録](placecute/commit-readback.json)）。報告書・最終tasks・診断画像は後続docsコミット。対象 `object-redesign`、ローカル保存のみ。保護26件をstage/commitへ含めない。ソースと検証入口のSHAは [source-sha256.json](placecute/source-sha256.json)。

指定の配置・拡大・顔・検証専用ログのソースとオフライン契約を提出する。**Native同版固定入力の再実測、Unity/WebGL再ビルド、実画面・実聴・物理iPhone・FPS・game-playtestはNOT_RUN**。標準投げの新しい距離・破壊数・作用履歴をこの報告で創作しない。前版1266.590576171875m／80粉砕も今回の結果ではない。試遊版の公開・本人採用・ゲーム全体の完成とは区別する。

残件は、同commitを既存担当の完全checkoutで再生し、(1) 新しい2個のもちと既存3個目の実接触、(2) 大きくした6種の実ゲーム視認性、(3) 結果→再投を確認すること。移設で標準軌道自体が変わるため、先の「高さ14〜15m」を新軌道の保証に使わない。新しいJSONLで実接触を確定させる。今回のworktreeはsparseで、既存ビルドの上書きや公開はしていない。

リスクの優先度は、配置による望まない減速＝起きやすさ3＋影響3、拡大による近接接触＝2＋2、顔の実画面での小ささ／遮蔽＝2＋2。前2件には全16静的マップと実測用ログ、顔には実builder契約と診断レンダーを用意。既知の床侵入・Collider近似残差・モール反復FAILは解消扱いにしない。

既存HANDOFF一箇所とtasksへ保存。TEAM.mdは案件固有KB読取禁止により未確認。Studio指定入口は現在のnative catalog未接続。Reasonのobject検索に別稼働担当はなく、別Unity Editorの起動もなかった。外部送信・Linear/KB更新・Git push・公開・課金・新認証・他セッション再起動は実施していない。
