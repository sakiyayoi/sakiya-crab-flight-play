# オブジェクト再設計 カメラ・ヒットストップ修正報告

2026-11-28 JST。本人判定（2026-11-27 23:44）の「カメラを引く」「6作用の瞬間にヒットストップ」の2点を実装し、指定オフライン検証を完了。派手さ、作用量、配置、個数、色、発光、基礎物理は変更していない。

## 対象版

- worktree `../object-redesign`、branch `object-redesign`、開始時clean。
- 基点 `6f5bb2e84e902b32f10a136095d9915368f92315`。
- **実装・検証根拠コミット `75e262376bc0715b76eca3e9ad3e17d660223938`**。本報告と最終tasks、読戻し記録は後続docsコミット。
- 上記コミットの68ファイルをGitから読み戻し、作業実体と全件byte一致。[読戻し記録](camhit/commit-readback.json)。
- 保護26ファイル（25 panel FBX meta＋FlightField.unity）はmain実体とbranch実体／sparse対象Git blobを各々前後照合し、**両方26一致・変更0・コミット対象0**。[前](camhit/protected-before.json)／[後](camhit/protected-after.json)。mainとbranchは元から別の値であり、互いを同一とは扱わない。
- ソース・試験入口18件のSHA256は [source-sha256.json](camhit/source-sha256.json)。型検査した136ソースの同版SHA256は [compile.json](camhit/compile.json)。

## カメラの変更値

このカメラは正投影。以下は角度ではなく、`orthographicSize`（画面の縦半分に相当するワールドm）。同じ縦横比なら画面に入る幅・高さに比例する。

| 設定 | 旧 | 新 |
|---|---:|---:|
| 照準・充填時の基準（既存aspect等の倍率を掛ける前） | 3.7 | 4.81（×1.3） |
| 低速の下限・横画面 | 10 | 13（×1.3） |
| 低速の下限・縦画面 | 12 | 15.6（×1.3） |
| 動的ズーム上限 | 30 | 51（×1.7） |
| landmark補正 | 動的値の×1.15、最大34.5 | ×1.15を保持、最終51以内に制限 |
| ズーム追従係数 | 2/s | 2/s（不変） |
| 位置追従係数 | 3/s | 3/s（不変） |
| 縦位置追従係数 | 1.2/s | 1.2/s（不変） |
| 飛行への導入時間 | 0.32秒 | 0.32秒（不変） |

値は `Assets/PortPrototype/PortRules.cs` 内の `PortCameraSettings` に集約。旧PortRules.Zoomの速度・高度による進行率を、新しい下限〜上限へ写す。**redesignだけ**新しい倍率を使い、legacyのZoom/Lead計算を維持する。`usePortCamera=false` の旧カメラも従来のまま。

`ObjectCameraTargets.NextObjectCameraTarget` は消費されていない作用物の現在の描画位置を読み、進行方向の最も近い物を選ぶ。物理・Collider・配置へ書かない。`PortCameraSettings.FitNext` でカニ側3.5m／対象側2mの余白を取り、先読み位置と縦中心も調整する。最大51を超えて対象を追わず、landmark倍率で無制限に小さくしない。

契約は320×568、390×844、844×390、1280×800相当のaspectで、速度0〜10000の単調性・上下限、距離3/6/12/24m・投影縦差0/7/24mのカニ＋次対象の幾何学的収まりを確認。これは目標画角の計算検査で、追従途中を含めた実ゲーム全フレームやカニの視認性の合格ではない。

## ヒットストップの実装経路

| 種別 | 旧・作用専用停止 | 新 |
|---|---:|---:|
| boost | なし | 70ms |
| bounce | なし | 70ms |
| spin | なし | 70ms |
| brake | なし | 90ms |
| blast | なし | 140ms |
| death | なし | 200ms |

値は `ObjectHitStopRules` に種別定数として集約した。

`ConsumeObject`（変更なし）→ `ObjectHits` → 既存 `DrainPhysicsEvents` → `DrainObjectEvents` → `PresentObjectHitStop(hit.kind)` → カメラに付けた `ObjectHitStopFrame.Request` → `OnRenderImage` で命中時の描画結果を1枚保持する。

**物理は止めず、最終カメラ画像の表示だけを止める実装**。Transform/Rigidbody、Time.timeScale、固定dt、入力、accumulator、既存feelClockへ書かない。シミュレーションと音・HTML UIは進み、終了後はその時点の実画像へ戻る。作用のキャプション・粒子・Effekseerの既存発動経路はそのまま実行する。裏側の進行を停止する方式ではないため、解除時には進んだ位置へ復帰する。この見え方の実機判定は別工程。

同じ描画フレームの複数要求は `pending = Max(pending, seconds)`、保持中は `until = Max(until, now + pending)`。短い要求で長い残時間を縮めず、二重に加算しない。初回の実描画から計時し、長いCPUフレームでも未表示のうちに停止時間を消費しない。再ヒットでは最新の作用フレームを取り直す。

ヒーロー／ランドマーク／10連鎖の既存節目演出は `JuicePresentation.cs`、`JuiceRules.cs`、`FeelDirectorR27.cs` をbyte不変で保持。新しい表示停止は同時に進む別時計なので、節目の後へ追加待ちを積まない。

deathでは実際のphase・終了集計を従来どおり確定させた後、200msの表示停止中は結果カメラを開始しない。WebGLの `Report` 用コピーだけ `phase=flight`、resultOverview非表示とし、結果パネルを待たせる。**`State()` 自体とNativeの結果観測は不変**。停止解除後の通常Reportで結果画面へ進む。

off／reduced motion（両設定）／pause／legacy／照準中は作用停止を要求しない。reset、無効化、破棄で所有バッファを解放。縦横変更・描画フォーマット変更・描画バッファ消失時は再確保する。元画面のgraphics format／色空間を引き継ぎ、色処理用shaderは追加しない。保持はカメラ1枚分に限定。

## 物理結果の保護

- 既存の物理・配置・設定・材質等、変更対象外370ファイルを基点Git blobとbyte照合。`ObjectRules`、`ObjectRuntime`、RigidCrabFlight、床処理、散布、ApplyBreaks、集計、入力は不変。
- 固定ステップを進める **Tick全文** と、Nativeが読む **State全文** を基点と照合しbyte一致。新しいカメラ対象取得も読み取りだけ。
- 本人指定の現行リプレイ基準 **1266.590576171875m／80粉砕** は保持条件として記録。**今回Native固定入力リプレイを再実行して得た値ではない**。新規契約に同じ数字を入れた箇所は、表示用Reportが距離・粉砕数を書き換えないためのsentinelで、PhysXリプレイの代替証拠ではない。

## オフライン検証

| 検査 | 結果 | 根拠 |
|---|---|---|
| Editor型チェック（実Unity参照＋Effekseer 1.80.8） | errors 0、ゲームwarnings 18／Effekseer 0 | [typecheck.log](camhit/typecheck.log) |
| WebGL条件分岐型チェック | errors 0、ゲームwarnings 17／Effekseer 0 | [compile.json](camhit/compile.json) |
| 既存ObjectRules | 85 passed / 0 failed | [rules-contracts.log](camhit/rules-contracts.log) |
| 既存Stage 1配置・物理・同版型検査 | 235 passed / 0 failed | [contracts.json](camhit/contracts.json) |
| 既存Effekseer／Juice | 45 + 33 passed / 0 failed | [bridge](camhit/bridge-contracts.log)／[juice](camhit/legacy-juice-contracts.log) |
| 既存ストーリー形状／不変境界 | 46 + 13 passed / 0 failed | [形状](camhit/shapes-contracts.log)／[境界](camhit/story-contracts.json) |
| 既存blast回収・消費／静的境界 | 258 + 9 passed / 0 failed | [実行](camhit/unit-contracts.log)／[境界](camhit/blast-contracts.json) |
| 既存PortRules | 30 passed / 0 failed | [port-contracts.log](camhit/port-contracts.log) |
| 新規カメラ・6種発動・画像保持・重複・解除 | 151 passed / 0 failed | [camhit-unit-contracts.log](camhit/camhit-unit-contracts.log) |
| 新規不変境界・発動接続 | 10 passed / 0 failed | [camhit-contracts.json](camhit/camhit-contracts.json) |
| Playwright・既存34＋新規1 | **35 passed / 0 failed / 0 skipped / 0 flaky** | [playwright.log](camhit/playwright.log)／[JSON](camhit/playwright.json) |
| 保護ファイル／型検査ソース読戻し | main26＋branch26一致／136ソースSHA一致 | [verify-evidence.py](camhit/verify-evidence.py) |

Stage 1は従来232件に新規3ソースの同版hash検査が加わり235件。既存の検査閾値・合格条件は緩めていない。blastの入口は出力先指定だけ追加し、旧証跡を上書きしていない。

新規実行契約は本番のカメラ計算・Window・画像保持コンポーネントと、抽出した実Drain/Reportメソッドを使う。Graphics/RenderTexture等は記録用ダブルであり、GPU描画ではない。Playwright画像も既存HTML部品の合成状態であり、Unity実画面の証拠とは扱わない。

初回は既存34pass／新規1fail。説明コメント中の `accumulator` に文字検査が反応した検査側の誤検出。コメントを除いてコードを検査するよう修正し全35件pass。初回ログは [playwright-initial.log](camhit/playwright-initial.log)、[contracts-initial.log](camhit/contracts-initial.log) に保持。rawログの末尾空白は加工していない。

### 再実行（worktreeルート）

```sh
KANI_OBJECT_EVIDENCE_DIR=docs/object-redesign/camhit python3 docs/object-redesign/typecheck.py
KANI_BLAST_EVIDENCE_DIR=docs/object-redesign/camhit python3 docs/object-redesign/blastfix/contracts.py
KANI_STORY_EVIDENCE_DIR=docs/object-redesign/camhit python3 docs/object-redesign/story/contracts.py
python3 docs/object-redesign/camhit/port-contracts.py
KANI_OBJECT_EVIDENCE_DIR=docs/object-redesign/camhit KANI_EFFEKSEER_EVIDENCE_DIR=docs/object-redesign/camhit KANI_JUICE_EVIDENCE_DIR=docs/object-redesign/camhit KANI_STORY_EVIDENCE_DIR=docs/object-redesign/camhit NODE_PATH=/Users/konomichiharu/.npm/_npx/e41f203b7505f1fb/node_modules npx --no-install playwright test -c docs/object-redesign/camhit/playwright.config.cjs
python3 docs/object-redesign/camhit/verify-evidence.py
```

## 残件・受渡し境界

指定されたソース修正・オフライン検証・ローカルコミットの範囲は完了。**Native固定入力再実測、Unity/WebGLビルド、実画面・実聴・物理iPhone・FPS・game-playtestはNOT_RUN**。ゲーム全体の完成・本人採用・公開の判定ではない。

次工程は既存担当が同実装commitの完全checkoutで固定入力を1回再生し1266.590576171875m／80粉砕を照合し、縦横でカニと次の作用物の視認性、6種の間、death→結果、再投、off/reduced/pauseを実確認すること。今回のworktreeはsparse。新WebGL配布物・試遊URLの更新はない。既知の床侵入／Collider残差／モール反復FAILも解消扱いにしていない。

TEAM.mdは案件固有KB読取禁止に従い未確認、Studio入口はnative catalog未接続。既存HANDOFF一箇所とtasksへ記録。外部送信、Linear/KB更新、Git push、公開、課金、新認証、他セッションの再起動は実施していない。
