# オブジェクト再設計 ストーリー層 実装報告

2026-11-27 JST。本人の「オブジェクト再設計 ストーリー層 実装指示（Stage 1 の追いジョブ）」の確定6形・確定6文言を実装した。今回の到達範囲はソース実装、指定のオフライン検証、既存ブランチへのローカルコミット、報告書保存。

## 対象版・保全

- worktree: `../object-redesign`、branch: `object-redesign`。
- 基点: `06df91585f52edefeb133cef041f64139edbf987`（Stage 1最終報告）。開始時に未コミット変更なし。
- 実装・検証コミット: `8ad7595191b3a88f6a9db39b3df684707ded06de`。本報告と最終tasksは後続のdocsコミットに分離する。実装コミットの61ファイルをGitから読戻し、全件が作業実体とbyte一致した（[commit-readback.json](story/commit-readback.json)）。
- 実装8ファイルのSHA256: [story/source-sha256.json](story/source-sha256.json)。型検査記録は同じ最終C#133ソースのSHA256を保持。
- `panel_*.fbx.meta` 25件＋`FlightField.unity` の26件は変更なし。元sourceの実ファイルSHA256、対象worktreeの実在ファイル／sparse対象Git blobのSHA256を、それぞれ開始／終了で全26件照合。一覧は [protected-before.json](story/protected-before.json) / [protected-after.json](story/protected-after.json)。ステージ／コミット対象に含めていない。
- 全配置座標、Colliderの生成・大きさ・位置、作用の効果量、物理処理、既存ルートの移動・回転・点滅は維持。`ObjectRules.cs` は形名6文字列だけ更新。`ObjectRuntime.cs` は描画子の回転参照・呼出しだけ追加し、それらを除いた本文はStage 1とbyte一致することを契約で確認。

## 6種の形の内訳

生成元は `Assets/ObjectRedesign/ObjectVisual.cs`。各作用のroot、Stage 1の共通Material、root BoxColliderを維持し、子のプリミティブ構成を変更した。プリミティブ付属Colliderは生成直後に無効化して破棄する。パーツ別Materialを増やさず、白・黒・寿司のアクセントだけMaterialPropertyBlockを使う。

| 種 | 新しい形・内訳 | 色・動き |
|---|---|---|
| boost | 業務用扇風機。円筒の台と支柱、16分割の円形ガード、縦横10本の格子、4枚羽根、中央ハブ | 青 `#0000FF`、Stage 1の前後スライド。羽根だけ毎秒720度の連続回転を追加 |
| bounce | 料理長のフライパン。平たい円筒の皿面＋横に伸びる角柱の柄。全体を12度傾斜し、皿面を見せる向きに配置 | 緑 `#00FF00`、Stage 1の上下動 |
| spin | 回転寿司のレーン。短いベルト、5本の継ぎ目、円筒の皿、白いシャリ、寿司色の小片 | 黄 `#FFFF00`、Stage 1のroot回転を維持。皿＋寿司はレーンの子として毎秒180度回転。寿司の小片は既存オレンジのトークン `#FF8000` を使用 |
| brake | 巨大もち。横に潰した白い球＋16分割の紫の発光縁 | 指定どおり本体は白、作用色と縁は紫 `#8000FF`。Stage 1の浮遊 |
| blast | 打ち上げ花火の玉。大きな球＋傾いた円筒の導火線＋小さな点火端 | オレンジ `#FF8000`、Stage 1の上下動と発光変調 |
| death | かに漁師。左に簡略人型、右に大きく張り出す網。人型はカプセルの胴、球の頭、帽子の冠と幅広のつば、腕、左右2本の長靴（筒＋つま先）。網は長い柄、16分割の円形枠、縦横10本の開いた格子 | マゼンタ `#FF00FF`＋黒。帽子・頭・長靴・柄に黒、胴と網にマゼンタ。Stage 1の速い前後スライド |

漁師は「網＋帽子＋長靴だけ」の代替ではなく、頭・胴・腕・脚もある人型と網を左右に分離した構成。即死の読み分けをこの輪郭に担わせる。実際のUnity描画で一目で読めるかは今回のオフライン検査では未確認。

追加の羽根／皿の回転は既存シミュレーション時計に従い、停止時もその時計に追随する。既存 `ObjectReducedMotion` で追加回転を静止させる。rootのStage 1運動およびBlastの発光式は変更していない。共通Emission=4、Glossiness=.35も維持。

## キャプション実装経路

`ConsumeObject → ObjectHits → DrainObjectEvents → FlightState.objectCaption → FlightReport → host.jsの既存JuiceHud.update(s) → juice-hud.js`。

作用発動時の既存 `action` イベントは維持し、同じ発動処理で最新1件の `sequence / kind / visible` を状態送信に添える。花火で大量破壊が同時発生すると32件の既存イベント履歴からactionが押し出されるため、キャプション専用の最新値を保持した。キューは作らず、同時多発は最後の1件に上書きする。新しいヒットや物理作用を発生させない。

| 種 | 確定文言 |
|---|---|
| boost | 扇風機、強すぎる！ |
| bounce | フライパンで跳ね上げられた！ |
| spin | 回転寿司のレーンに乗った！ |
| brake | もちにくっついた… |
| blast | 花火、点火済みだった！ |
| death | かに漁師だ！ |

- 表示は実時間1000ms。単一DOM要素と単一タイマーで、次の作用時に本文とタイマーを置換する。同じsequenceを再受信しても時間延長・再表示しない。
- 発動時は `FeelEffectsR27 / ReducedMotion / feelSettingsR27.reducedMotion / paused` で抑制。ブラウザでも既存 `FlightFeel.settings()`、OS reduced-motion、bodyのreduced属性、状態のreduced/off/pauseを照合。抑制中のsequenceも消費し、オンへ戻しても昔のキャプションを出さない。
- 飛行中は粉砕HUDの下に専用の表示域を置く。既存feel.jsのポップ上端をその下へ制限し、下端は表示中のJet/Dive操作または既存Kick操作の上とする。粉砕数の値・音程・表示ポップ時間は変更していない。
- deathではRESULTへ即座に移るため、同じ要素を結果パネルの先頭に配置し、結果遷移で文言が消えないようにした。1000ms終了・再投・世代更新・一時停止で消す。
- 新しい文字アニメーションや点滅は追加していない。装飾的な短文はaria-hiddenで、複数の短文が読み上げ待ちに残らない。

## 検証結果（最終同版）

| 検査 | 結果 | 根拠 |
|---|---|---|
| Editor型検査 | errors 0、ゲームwarnings 18／Effekseer warnings 0 | [compile.json](story/compile.json) |
| WebGL条件分岐型検査 | errors 0、ゲームwarnings 17／Effekseer warnings 0 | 同上 |
| Stage 1実ObjectRules契約 | **85 passed / 0 failed** | [rules-contracts.log](story/rules-contracts.log) |
| Stage 1配置・基礎物理・保存byte・同版型検査契約 | **232 passed / 0 failed** | [contracts.json](story/contracts.json) |
| 既存Effekseer契約 | **45 passed / 0 failed** | [bridge-contracts.log](story/bridge-contracts.log) |
| 既存JuiceRules/clock/burst契約 | **33 passed / 0 failed** | [legacy-juice-contracts.log](story/legacy-juice-contracts.log) |
| 新規の実ObjectVisual生成契約 | **46 passed / 0 failed** | [shapes-contracts.log](story/shapes-contracts.log) |
| 新規の物理・配置不変、表示経路・フラグ、26件保全契約 | **13 passed / 0 failed** | [story-contracts.json](story/story-contracts.json) |
| 新規Playwright（6文言、1000ms、最新1件、抑制、再表示防止、3画面幅、RESULT） | **16 passed / 0 failed / 0 skipped** | [playwright.json](story/playwright.json) |
| 既存Playwright（Stage 1の10件＋既存音声/HUD8件） | **18 passed / 0 failed / 0 skipped** | [legacy-playwright.json](story/legacy-playwright.json) |

既存395契約の件数は減らしていない。`RulesContracts.cs` 内の6形名の期待値のみ、`arrow → industrial-fan`、`spring → frying-pan`、`propeller → sushi-conveyor`、`cloud → mochi`、`barrel → firework-ball`、`spikes → crab-fisher` へ更新。色・効果量・動きの期待値は不変。旧証跡を保存するため検証スクリプトに出力先環境変数を追加したが、既存の検査式・件数・閾値は変えていない。

形の契約は実ObjectVisualコードを記録用Unity APIダブル上で実行し、構成・寸法・親子関係・素材共有・子回転・Collider無効化を検査するもの。UnityレンダラーやPhysXの実行ではない。型検査は実Unity参照assemblyと既存Effekseer 1.80.8ソースを用いたRoslyn検査であり、Unity import、IL2CPP、WebGLビルドの合格ではない。音声試験もグラフスタブであり実聴ではない。

ブラウザで390×844、320×568、844×390のHTML部品を実描画し、[小画面](story/caption-small.png)、[縦](story/caption-portrait.png)、[横](story/caption-landscape.png)、[死亡結果・小画面](story/death-small.png)、[死亡結果・横](story/death-landscape.png)を確認。これらは合成状態を供給したHUDで、画像内にも「Unity未起動」と明記している。3Dゲーム画面の証拠には使わない。

初回の形契約は人型ノードとstoryルートの名前重複を検出したため、人型を `fisher-person` に分けた。修正途中の旧コンパイル記録とのSHA不一致も検出し、最終ソースを再コンパイルして232件全通過を確認した。初回ログは削除せず保存した。

### 再実行コマンド

worktreeのルートで実行する。

```sh
KANI_OBJECT_EVIDENCE_DIR=docs/object-redesign/story python3 docs/object-redesign/typecheck.py
KANI_OBJECT_EVIDENCE_DIR=docs/object-redesign/story python3 docs/object-redesign/contracts.py
KANI_EFFEKSEER_EVIDENCE_DIR=docs/object-redesign/story python3 docs/effekseer-proto/contracts.py
python3 docs/object-redesign/story/contracts.py
NODE_PATH=/Users/konomichiharu/.npm/_npx/e41f203b7505f1fb/node_modules npx --no-install playwright test -c docs/object-redesign/story/playwright.config.cjs
KANI_OBJECT_EVIDENCE_DIR=docs/object-redesign/story KANI_EFFEKSEER_EVIDENCE_DIR=docs/object-redesign/story KANI_JUICE_EVIDENCE_DIR=docs/object-redesign/story NODE_PATH=/Users/konomichiharu/.npm/_npx/e41f203b7505f1fb/node_modules npx --no-install playwright test -c docs/object-redesign/story/legacy-playwright.config.cjs
```

MacにあるUnity 6000.3.23f1のRoslyn、Stage 1で使用した `/tmp/kani-effekseer-1808-reference`、元sourceのLibrary参照、既存PlaywrightとChromeを使う。新規課金・認証・インストールなし。

## 未確認・次工程

指定オフライン範囲の実装と検証は完了。Unity起動/import/build、3Dの視認性・発光の実際の見え方、実入力から6種に当たる通しプレイ、実聴、物理iPhone、実FPS/GC、game-playtestは **NOT_RUN**。完成ゲーム・本人採用・公開を意味しない。

次は既存担当がこのコミットの完全な隔離checkoutを用意し、既存Stage 1の設定手順で `objectMode=redesign` を有効にして、両方向の6種・人＋網の即死判別・花火大量破壊時の文字・即RESULT・設定off/reduced・再投を同版で実地確認する。編集worktreeはsparseで、完全なUnityプロジェクトではない。既存r22の床一時侵入5.26cm、Collider近似残差、モール反復FAILの扱いは変えていない。

既存HANDOFFとtasksを使用。案件のKB禁止に従いTEAM.md未確認。Studio入口snapshotの本文は参照したが、現行native catalog/sourceとの照合は未接続。Reason一覧では本件の別稼働担当を確認せず、重複着手はしていない。作業中のiCloud未ダウンロードファイルの待ちは、既存内容を保護したうえで読戻しまで確認した。他セッションの再起動なし。外部送信、Linear/KB更新、Git push、公開は行っていない。
