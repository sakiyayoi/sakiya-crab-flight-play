# 【進捗】カニ r30（未合格の比較試作）

更新UTC：2026-10-28T20:14:54.450198+00:00

試遊URL：https://sakiyayoi.github.io/sakiya-crab-flight-play/preview/r30/
latest：https://sakiyayoi.github.io/sakiya-crab-flight-play/preview/latest/
ダウンロード：https://raw.githubusercontent.com/sakiyayoi/sakiya-crab-flight-play/5b8035a5ceb81253ec179b9630da3b2c513bbfe6/preview/r30/CrabFlight-kani-v0.5-floor-r30-WebGL.zip
版：kani-v0.5-floor-r30。実行ソース：6a8bdc2c0279484c3e1d5eb004d67d9f72b8d451。
private candidate/floor-r30へ通常push、remote同SHA読戻し済。公開commit：5b8035a5ceb81253ec179b9630da3b2c513bbfe6。
ZIP：20238312 bytes、SHA256 `0d905116298c0f424fa9e8548e273134f346ec5c675f420174e9d67e18495489`。88entryのCRC・全量SHA一致。
実行ソースSHA256：aeddbbb7e33d050c88ac19a0d3dd8c3cc89e6174446881d84ea74f750ba00e78。

## G-POWERの分母訂正と原因

**r29は合格4/40・不合格36/40です。「合格36・不合格4」ではありません。**
マツリのSlack C0CBSU8TA2W / thread1793159062.741849 / message1793214382.783659をspec/tasks/PLANへ保存・読戻し。「勢い」は充填後の射出目標、許容差±5を維持しました。

全40投でConfigureとReleaseは受領・許可され、受領時の目標は55/100でした。36不合格はいずれも受領後のCoreLaunchPowerによる充填再換算で、目標不足・未発射ではありません。旧式は40投の実値を最大残差4.53e-6で説明し、最大の目標差は28.1644でした。全行の原試行番号と修正前の値を `r29-power-cause.json` と派生CSVへ保持しています。親の前報告はこの分母で訂正してください。

修正：正規に受理した射出目標をそのままLaunchへ渡し、chargeは回転作業量・回転速度へ残しました。未充填の拒否、合法入力範囲、投ごとの入力→充填→目標→実射出記録は維持。receiptにtargetPowerを独立記録し、Prepareで消去、同じまま再投は保持。閾値・過去ログの変更0。

Nativeの合法低充填／高充填×目標55/100で4ケースとも実射出55/100、実Rigidbody初速25.36186/46.11247m/s、未充填はcharge-not-readyを確認。正式40の結果は後掲です。

## 床とモデル・Colliderの原因／修正／残差

r29 worst weak#16は支持点-0.13329178m。保存された受理入力とflightTick列をNativeへ読み込みましたが、初期body/jointのULP差があるため、この13.3cmを同一初期状態で完全再現したとは言いません。Native旧PGSでは支持点-30.1538mmとComputePenetration-30.079mmが一致し、表示だけの誤検出ではないことを確認しました。

同じNative初期body/jointでPGS0→TGS1だけを変える対照：

|保存入力|PGS支持点|TGS支持点|PGS距離|TGS距離|実破壊数PGS→TGS|
|---|---:|---:|---:|---:|---:|
|weak16|-30.1538mm|-3.4145mm|124.4576m|125.5775m|23→22|
|strong2|-10.3632mm|-2.3430mm|225.3296m|164.0899m|21→23|

侵入軽減を支持しますが、strong2距離短縮を残し、万能改善・強入力の全短縮解消・本人採用とはしません。r30はTGSを候補へ採用。質量、摩擦・反発、outer1/240、床安全閾値-0.45m、連鎖定義は変更せず、ワープ・判定削除・カウンタ補填0。

低充填境界で発射時Collider凍結の変換不整合を検出したため、非一様scale＋部位回転を分解せず、authored全Affine行列で全頂点をroot座標へ移しました。元の0.1mm守備範囲は全変換頂点で確認（最大0.000482mm）。PhysX cookedAABBの0.169〜0.217mm差は別診断へ保存し、幾何の一致と混同しません。旧guardだけを削って通した変更ではありません。

Native最終witness：16,544可視頂点／Collider13、p95残差0、可視点のClosestPoint外側最大7.342mm、global方向支持overreach11.339mm／underreach0。近似残差は0ではありません。旧3.74cmとは版・形状・測定条件を併記し、数値だけで同一量の改善とは扱いません。
Native最終の保存2入力：weak16支持点-8.8503mm／125.7882m、strong2-1.28195mm／181.4691m、fault空。これは射出目標修正とAffine凍結を含む候補の観測で、単一変数対照とは別です。

一次資料：Unity6000.3 Physics Manager https://docs.unity3d.com/6000.3/Documentation/Manual/class-PhysicsManager.html 、UnityCsReference PhysicsManagerInspector、PhysX5.4.1 Rigid Body Dynamics https://nvidia-omniverse.github.io/PhysX/physx/5.4.1/docs/RigidBodyDynamics.html 。第三者コードの導入・実行なし。

## 実コマンドと成否

|対象|実コマンド／記録|結果|
|---|---|---|
|Native原因・境界|Unity6000.3.23f1 `-batchmode -projectPath … -runTests -testPlatform PlayMode -testFilter R30AcceptedPowerTarget|R22ColliderWitness|R30FloorCause`（正確な引数はnative07-receipt.json）|3PASS/0FAIL、exit0|
|WebGL|Unity `-executeMethod FloorBuildR10.R27Feel -quit`、build01-receipt.json|exit0、ソース6a8bdc2|
|同版全Playwright＋強20弱20|`npx playwright test -c qa/final-r27.playwright.config.js`、qa01-receipt.json／qa01-output/pw.json|TERMINAL：36PASS/0FAIL、強20/弱20、exit0|
|M14全PNG読戻し|`node flight-floor-first-r21/tools/r30_fullframe_readback01.cjs`＋02|2,890PNG全decode/SHA、1,445新Nativeカウンタに非blank画像・欠番0|
|比較動画|`python3 flight-floor-first-r21/tools/r30_compare01.py`|PROCEDURE_COMPLETED|
|公開実操作|`node flight-floor-first-r21/tools/r30_public_smoke01.cjs`（実env/ログをreceiptへ）|PUBLIC_ONE_LOOP_VERIFIED|

失敗も保持：native01はテスト型参照のcompile失敗、native02はstale0byte lockのprecheckで未実行、native03旧PGS原因1PASS、native04単一solver対照1PASS、native05は2PASS/1FAIL（低充填時凍結差）、native06は0PASS/3FAIL（cookedAABBと実頂点の区別不足）、実変更後native07は3PASS/0FAIL。隠蔽、同原因の無変更再送、無限リトライなし。正確な実引数・全XML・失敗メッセージは各receipt/logへ保持。

正式40集計：{"version": "kani-v0.5-floor-r30", "sourceCommit": "6a8bdc2c0279484c3e1d5eb004d67d9f72b8d451", "command": ["npx", "playwright", "test", "-c", "qa/final-r27.playwright.config.js"], "exitCode": 0, "passed": 36, "failed": 0, "strong": 20, "weak": 20, "powerThreshold": 5, "G_POWER_pass": 40, "G_POWER_fail": 0, "targetActualMaxDifference": 0, "receiptTargetAllSame": true, "abnormalStops": 0, "notResult": 0, "shorterThan5m": 0, "worstSupport": {"kind": "strong", "i": 2, "target": 100, "charge": 0.6844692230224609, "receiptTarget": 100, "actual": 100, "targetDifference": 0, "angle": 40, "direction": 1, "phase": "result", "spinPhase": 0.32380491495132446, "distance_m": 274.9993896484375, "fault": "", "minSupport_m": -0.021335840225219727, "groundStill_s": 0.31666654348373413, "version": "kani-v0.5-floor-r30", "sourceCommit": "6a8bdc2c0279484c3e1d5eb004d67d9f72b8d451"}, "originalJSONsha256": "cc570ebd8a5749a9216467f3682fef7db26aae9d1bc611d4d6d8c40425fa52e5", "derivedCSVsha256": "b3b0f6061f3b7d0f0edd1c4905dff9390e05e23b1a669dfb60b0bf9661f1acf1", "conditions": [{"kind": "strong", "angle": 20, "direction": 1, "n": 3, "distanceRange_m": [830.995849609375, 929.0249633789062], "minimumSupport_m": -0.014296770095825195, "allTargetsEqualActual": true}, {"kind": "strong", "angle": 40, "direction": 1, "n": 3, "distanceRange_m": [274.9993896484375, 340.50201416015625], "minimumSupport_m": -0.021335840225219727, "allTargetsEqualActual": true}, {"kind": "strong", "angle": 55, "direction": 1, "n": 3, "distanceRange_m": [317.9960632324219, 610.4633178710938], "minimumSupport_m": -0.01054394245147705, "allTargetsEqualActual": true}, {"kind": "strong", "angle": 70, "direction": 1, "n": 3, "distanceRange_m": [269.27264404296875, 838.8448486328125], "minimumSupport_m": -0.0172271728515625, "allTargetsEqualActual": true}, {"kind": "strong", "angle": 20, "direction": -1, "n": 2, "distanceRange_m": [849.9426879882812, 929.7056884765625], "minimumSupport_m": -0.003213822841644287, "allTargetsEqualActual": true}, {"kind": "strong", "angle": 40, "direction": -1, "n": 2, "distanceRange_m": [275.30084228515625, 922.3024291992188], "minimumSupport_m": -0.002514392137527466, "allTargetsEqualActual": true}, {"kind": "strong", "angle": 55, "direction": -1, "n": 2, "distanceRange_m": [386.05621337890625, 713.55078125], "minimumSupport_m": -0.008530616760253906, "allTargetsEqualActual": true}, {"kind": "strong", "angle": 70, "direction": -1, "n": 2, "distanceRange_m": [231.89833068847656, 390.616455078125], "minimumSupport_m": -0.013063311576843262, "allTargetsEqualActual": true}, {"kind": "weak", "angle": 20, "direction": 1, "n": 3, "distanceRange_m": [132.1692657470703, 164.5504608154297], "minimumSupport_m": -0.004325807094573975, "allTargetsEqualActual": true}, {"kind": "weak", "angle": 40, "direction": 1, "n": 3, "distanceRange_m": [122.23316192626953, 151.91189575195312], "minimumSupport_m": -0.008278965950012207, "allTargetsEqualActual": true}, {"kind": "weak", "angle": 55, "direction": 1, "n": 3, "distanceRange_m": [110.10417175292969, 180.80601501464844], "minimumSupport_m": -0.004377245903015137, "allTargetsEqualActual": true}, {"kind": "weak", "angle": 70, "direction": 1, "n": 3, "distanceRange_m": [109.0225601196289, 128.33750915527344], "minimumSupport_m": -0.018236160278320312, "allTargetsEqualActual": true}, {"kind": "weak", "angle": 20, "direction": -1, "n": 2, "distanceRange_m": [154.6390838623047, 193.04623413085938], "minimumSupport_m": -0.016347408294677734, "allTargetsEqualActual": true}, {"kind": "weak", "angle": 40, "direction": -1, "n": 2, "distanceRange_m": [143.3385772705078, 154.4656524658203], "minimumSupport_m": -0.007163867354393005, "allTargetsEqualActual": true}, {"kind": "weak", "angle": 55, "direction": -1, "n": 2, "distanceRange_m": [136.4470672607422, 149.74896240234375], "minimumSupport_m": -0.006132304668426514, "allTargetsEqualActual": true}, {"kind": "weak", "angle": 70, "direction": -1, "n": 2, "distanceRange_m": [125.92595672607422, 135.1163787841797], "minimumSupport_m": -0.006354689598083496, "allTargetsEqualActual": true}], "comparisonCaveat": "same legal request matrix, phase/charge/frame clocks recorded per throw; heterogeneous inputs are not averaged, strong/weak distances not phase-matched causal pairs", "supportZeroClaim": false}

## M14/M15と実画面の検査範囲

1280×720/DPR1、Chrome Metal。requestAnimationFrameの各callback直後にread-only Report、losslessPNG、Native Time.renderedFrameCountを記録。先頭の既存baselineとblank post-composite画像も捨てず保持し、以降すべての新規カウンタに非blank画像があることを照合しました。
マシ376／ふつう206／オフ184／軽減219／揺れオフ460＝1,445新規カウンタ。全2,890PNGをdecode＋SHA読戻し。flash69イベントの直前／当該／直後画像を作者側で実見。局所衝撃と白化、OFFで演出文字なし、揺れオフoffset0、軽減点滅0／文字の移動0を記録。全1秒窓のtelemetry最大は3/3/0/0/3。物理破片は実物として残ります。

**このcounterはUnityCsReferenceでundocumentedのpublic bindingです。GPUの全Presentを保証するAPIではありません。** 有限の記録区間での連続性と画像読戻しの根拠で、臨床的安全認定・実機FPS・人の全動画実聴とは別です。同期PNG化の負荷があるため、この録画の時間を実性能へ使いません。過去の間引き録画だけを「全描画frame」と言い換えていません。カニ周囲の破片の密集、モール什器の反復、画面美観・好みは改善完了とはしません。

## PLAN状態と担当

正式状態を勝手に合格へ上げていません。担当はR02が親／カニ係（公開判断は本人許可）、ほか31項目はリリー。正式T01が作業中のため、依存する未着手行と、既存r29でコード統合済み・実物観測済みのN01〜N06を区別します。N01追従/N02引き/N03装備/N04地域38対象13種/N05既存財布3段強化接続/N06コミカル反応はr29から保持、r30で新たな装飾を増やしていません。正式提出・マツリ合格ではありません。

```text
=== PLAN STATUS ===
出力：2026-10-29 04:55 JST
| ID | 流れ | 状態 | 更新（JST） | 順番 |
|---|---|---|---|---|
| T01 | 通し版 | 作業中 | 10/29 01:18 | ok |
| T02 | 通し版 | 未着手 | — | ok |
| T03 | 通し版 | 未着手 | — | ok |
| B01 | 不具合 | 未着手 | — | ok |
| B02 | 不具合 | 未着手 | — | ok |
| B03 | 不具合 | 未着手 | — | ok |
| B04 | 不具合 | 未着手 | — | ok |
| B05 | 不具合 | 未着手 | — | ok |
| CR01 | クレジット | 未着手 | — | ok |
| CR02 | クレジット | 作業中 | 10/29 01:19 | ok |
| CR03 | クレジット | 作業中 | 10/29 01:19 | ok |
| CR04 | クレジット | 作業中 | 10/29 01:19 | ok |
| CR05 | クレジット | 未着手 | — | ok |
| F01 | 手触り | 作業中 | 10/28 22:55 | ok |
| F02 | 手触り | 未着手 | — | ok |
| F03 | 手触り | 未着手 | — | ok |
| F04 | 手触り | 未着手 | — | ok |
| F05 | 手触り | 未着手 | — | ok |
| F06 | 手触り | 未着手 | — | ok |
| F07 | 手触り | 未着手 | — | ok |
| F08 | 手触り | 未着手 | — | ok |
| F09 | 手触り | 未着手 | — | ok |
| F10 | 手触り | 未着手 | — | ok |
| R01 | 公開 | 未着手 | — | ok |
| R02 | 公開 | 未着手 | — | ok |
| N01 | 次候補 | 作業中 | 10/29 02:38 | ok |
| N02 | 次候補 | 作業中 | 10/29 02:53 | ok |
| N03 | 次候補 | 作業中 | 10/29 02:53 | ok |
| N04 | 次候補 | 作業中 | 10/29 02:53 | ok |
| N05 | 次候補 | 作業中 | 10/29 02:53 | ok |
| N06 | 次候補 | 作業中 | 10/29 02:53 | ok |
| V01 | 演出最優先 | 作業中 | 10/29 01:30 | ok |
集計：未着手 20／作業中 12／提出済 0／差し戻し 0／合格 0
PLAN-CHECK: ◯
=== END PLAN STATUS ===
```

## 残件・停止境界・次工程

- M17物理iPhone／実FPS／実聴：この担当に物理端末・聴覚確認toolが提供されていません。viewport／decodeで代用せずNOT_RUN。`M17-PHYSICAL-CHECK-r30.md`に必要な端末・版・設定・一巡・保存・FPS計測方法・実音量／残留確認を記載。親からマツリへ実機確認手段を相談できます。認証や費用要求を勝手に追加しません。
- M14は記録区間の全Nativecounter/Canvas PNGとDOMpopup状態の検査を済ませた範囲を提示。ブラウザ全画面の全GPU Present保証・独立監査は未確認として保持。F05のGPT Proとの最終値相談、F10のCHECKLIST全項目認定、全Mのマツリ受入も未実施。
- 結果カードがカニの一部を隠す既存表示課題をcompare01/mashimashi-result.pngで実見。今回の物理/入力修正の合格へ混ぜず、次の局所UI修正として残す。
- 支持点の残侵入・Collider近似・TGSでの強入力距離tradeoffは個別残件。40投異常停止0をそれら全解消へ流用しません。次は最悪行の接触／姿勢から局所原因を絞り、目的のない全40反復はしません。
- M13の3動画は初期receipt11項目が完全一致し、同じ条件式の実Space入力を1回ずつ送信。全3presetで398.3073m・19破壊・PERFECT1/GOOD0。実wallclockのキー時刻まで一致したとせず、実入力policyと各sample/結果を保持。動画は358/227/200全encoded frameをPNGへ復号し、3音声は全量decode/非zeroを確認。音を聞いた品質判定ではありません。最初のffmpeg null出力は既存binaryにmuxerが無く失敗、能力表でPNG/image2対応を確認して全frame復号へ変更した実記録も保持。
- 旧C-7／旧連鎖下限3／gap1基準は履歴保持、現在M1〜M17の公開gateと混同しません。全区間撮影漏れ／反復は旧FAILを消さず、今回無関係な背景修正で物理作業を増やしません。
- 正式top、旧r29、preview保存prefixは保護。r26 t01-r26は既に別版SHAで読戻し済みで再作成0。本人の好み採用・全M合格・正式公開・X投稿はしていません。

根拠は同じ既存 `flight-floor-first-r21/evidence/r30`、状態更新と再開入力は既存HANDOFF一箇所へ保存・読戻し。新台帳・新担当・料金・新認証・権限拡張・KB参照0。
