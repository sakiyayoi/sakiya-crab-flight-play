# AreasR5c — 3エリア・大型障害物の第1実装（非公開比較候補）

版 kani-areas-r5c。入力CalmR4 source5753eea2e629c5a8d55812e434d83f13e87e9928、engine de187649f1e51d551595218476cfb815a9ba2008。今回engine0c0e4ec57f3af30db8c13a54ecaa11384ef02d36、C# SHA aa502ac305998e4ac63751e7b09f9d2c7fde371371c5184c0c120533200a96eb。最終snapshot/tree・配布SHAはdelivery/receipt.json。

## 実装

モールを28–62mぬいぐるみ/雑貨、62–100mお菓子、100–135mガチャへ。3案すべての本人採用を維持。既存50対象を同じID/位置/chainGroupで置換し、左右それぞれ9か所の大型対象を置く。小物を間に配し、幅広/低い/背高の大きさに差を付ける。変更前後ともCity接触対象315。色替えだけでなく、棚/熊/ポーチ、菓子容器/渦巻き飴、窓/丸ハンドル/取出口/二色カプセルを自作。既存R18の面取り/回転体コード・粒状マップと既存CityShaderを再利用した。元r35の失われたモデルソースを回収したとは呼ばない。

表示だけを旧Colliderへ引き伸ばさず、新しい造形のgroup AABBから接触・破片の記述を同時に出力。無傷時はopaque1draw（窓がある場合はglass追加）、破壊後は既存の最大6物理片。City20/全48/装飾12、CalmR4別物理Scene、カニ/floor/rotor/得点/揺れOFF/保存互換は保持。AABBは複合モデルの近似であり、精密MeshColliderや全出生重なりの証明ではない。

## 同条件比較

同seed42043、同じ合法発射receipt、初期bodyのbyte一致、1280×800/DPR1、隔離Chrome156/Metal。録画なしの一投比較（通常入力検査とは別）。

|指標|CalmR4|最終R5c|
|---|---:|---:|
|距離m|164.068710|155.442764|
|実破壊|28|23|
|実二次破壊|9|6|
|C#Tick平均ms|3.178850|2.711740|
|C#Tick最大ms|39.099899|31.700100|
|rAF平均ms|16.923854|16.793719|
|rAF最大ms|50.0|66.6|
|rAF95%ms|16.8|16.8|

初回R5は売場が初めて見えるx19m付近で883.3ms/対応C#6.9ms。専用新shaderを既存CityShader+パステルatlasへ替えたR5bで100ms/C#7.1ms、ロード時にmesh.UploadMeshData(false)を行うR5cで66.6ms。旧FAILを置換せず全記録を保存。旧版50msとの差は残るので「負荷完全維持」「処理落ち全解消」「厳密10投PASS」とは判定しない。今回3方式で止め、軽微な初回表示残差として保持。初投を意図的に遅くせず、隠れた投球・warmup投球・丸め許容・選別・測定窓縮小は無し。API原典: https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/mesh/uploadmeshdata 。

## 実検証

- Unity6000.3.23f1、KingBoldBuildR2.Web: 0 errors /19 warnings。新旧実行物は別フォルダ。
- `CALM_RUN=areas-r5-evidence/baseline01 CALM_VERSION=kani-king-calm-r4 CALM_WEB=.../web-calm-r4 node measure-front-areas-r5.cjs`: 1 PASS /0 FAIL。
- `CALM_RUN=areas-r5-evidence/candidate03 CALM_VERSION=kani-areas-r5c CALM_WEB=.../web-areas-r5c node measure-front-areas-r5.cjs`: 起動→同入力投球→結果→再投1 PASS /0 FAIL。性能50ms比較の未達は上表のまま。
- `CRAB_TARGET_DIRECTORY=.../web-areas-r5c node functional-areas-r5.cjs`: 通常Enter充填/発射2投 PASS、266.592102m（26破壊/8二次）と162.463959m（28破壊/7二次）、Browser error0、physicsFault空、shake=false/cameraMaxOffset0/zoom1。第1投のVFR canvas実映像を保存。これは実手操作/実聴/性能証拠ではない。
- 同コマンド `--saved-state-readback`: 追加投球0、ベスト266.592102mと完了2投の保存距離/ID/版一致 PASS。
- 新3売場のdestruction objectId/secondary/位置/材質はnatural-contact-witnesses.json。実際のゲーム接触経路。タイマー/文字/スコアだけで破壊を数えない。
- 静止Nativeは同一カメラ/1280×800で旧版と新3売場の実描画。カニ位置は比較用に置いた静止poseで、自然接触の証明とは別。最終mesh/materialのbyteはNative06と同一。比較PNGは縮小並置、原寸6枚も保存。別に実動画の2/3/4.5/6秒を補間なし抽出し実見。
- 初期fixtureの失敗（遠景用merge形式、複数sceneのstatic描画cache寿命、ページload前view参照、意図的pauseとdriver自動resumeの競合）は記録維持。原因に対応した変更後のみ次処理。通常操作を壊れたと偽らず、失敗をPASSに上書きしない。

## 未完・未確認

今回完了は3エリア/大型前景の最初の可逆実装と比較実物。旧灰色の高所設備/背景香水売場は残り、実在感・仕上げは途中。密度を無限に足さず、次工程で密集/抜けの配置と大物の連鎖を詰める。人・鳥は現状装飾/驚きのみで、接触吹き飛び・実得点は未実装。破片の命中点/法線別速度と回転を並進+回転合計energyBudget内で配分する案は次工程、未適用。大きいカニ/派手な局所演出は維持したが、本人の楽しさ採用は別。元の床微小侵入/精密Collider残差、厳密10投、40投、M14、実iPhone、実聴、左向き実走の新検証は今回未実施。既存の不合格/未確認を解消扱いにしない。

最新TEAM.mdは制作/既知の親/復旧先に見つからず本文未読、AGENTS/採用記録/現行GLOBALと最新本人指示の範囲で継続。公開R2b/正式トップ/旧r33、CalmR4/bundle/4添付は不変。新公開/push/認証/課金/素材購入/他担当起動は0。

実行ZIPは標準解凍後 `python3 serve.py`、http://127.0.0.1:8765/。同梱Brotliを復元するため通常http.serverは不可。ソースはcode/assets ZIPを同じ親へ解凍してsource/を合体。各SHAと全量読戻しはreceipt。本人へ酔いの再現を要求する検品ではない。
