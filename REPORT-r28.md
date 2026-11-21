# 【進捗】r28 演出改修プレビュー公開（未合格・未採用）

試遊URL：https://sakiyayoi.github.io/sakiya-crab-flight-play/preview/r28/
latest：https://sakiyayoi.github.io/sakiya-crab-flight-play/preview/latest/
ダウンロード：https://raw.githubusercontent.com/sakiyayoi/sakiya-crab-flight-play/9e613149fb221d8d8f63b003290bfd5fa21c2d9f/preview/r28/CrabFlight-kani-v0.3-impact-r28-WebGL.zip
ZIP：20,282,932 bytes / SHA256 `247bd44df03fd22305ae571f31de33eaa5bdfef696ff89c46872671333637312`
30秒実映像：https://sakiyayoi.github.io/sakiya-crab-flight-play/preview/r28/r28-impact-excerpt-30s.webm

版 `kani-v0.3-impact-r28` / feel `feel-r28-impact-1`。
実行コードcommit `42b480f6210b4edaf419576ee66e18e658371c2a`、production source SHA `ec27a7929d166f1277cb230169ac2d447cdde0f18769fc5329a51ef5b3b379b5`。
公開commit `883528b4698efbfb7672686c61547e11f9761531`、Pages run [40735790654](https://github.com/sakiyayoi/sakiya-crab-flight-play/actions/runs/40735790654) はbuild/deploy success。

## どこをどう変えたか

- 命中：細い同心円だけの表示を、太い不規則な漫画的シルエットと欠けた弧へ。ガラスは大きな反応を保持。
- 破片：ガラスの尖った大片・曲がる金属片・木の細長いささくれ・陶器と食品を別形状にし、大中小と飛散方向、回転、落下へ差を付けた。粒数/poolは増量せず。
- 残像：固定楕円3個を、甲羅・ハサミ・脚を持つ姿勢付き残像へ。実画像で甲羅を覆ったため薄く後方へ修正。
- 文字：中央固定の+1/連鎖/旧擬音を、接触位置の大見出し＋実際の+nの一組へ。即ピーク→短い保持→退場とし、ガラスには「生簀、退場！」。
- 安全：旧擬音の演出OFF漏れ、OS軽減時の文字移動/拡大/回転・結果countupを停止。既存flash3回/秒・shake/off gateと初期マシマシを保持。

Burrito Bison公式作者記事（https://juicybeast.com/2017/08/03/in-case-of-emergency-make-burrito-bison-3/）のパンチ形状と素早い伸縮、削れ跡で進行を見せる説明を参照。公式PNGとGIF先頭frameは実見、全GIF/ゲーム操作は当担当未実施。素材とコードは流用していない。

## 今回の実検査

| コマンド／入力 | 実結果 | 範囲 |
|---|---|---|
| `python3 flight-floor-first-r21/tools/r28_build03.py` | exit0 | 最終Native42b480f/WebGL |
| `python3 flight-floor-first-r21/tools/r28_qa04.py` 内 `npx playwright test -c harness/playwright.harness.config.js --grep 'マシマシ・ふつうで全部'` | 13PASS/0FAIL/exit0 | 修正後①〜⑦、通常、声、値一致、timeScale等 |
| qa03の安全4件＋UI2件 | 6PASS、同run全体は10PASS/1FAIL/8SKIPを保持 | その後はcue速度参照だけの修正、gate/文字/flashの不変を読戻して再利用。単一全suite PASSではない |
| `python3 flight-floor-first-r21/tools/r28_playtest02.py` | exit0、608.0272m/実破壊30/実PERFECT1/fault空/ブラウザエラー0 | 70/40°右、1280x800/DPR1、1投＋再投/390viewport/保存復帰 |
| `python3 flight-floor-first-r21/tools/r28_public_smoke01.py` | exit0、200.3066m/実破壊25/fault空/再投成功/エラー0 | 実URL。preview level1/coins63を引継ぎ、正式save不変、reload保持、8主要資産/ZIP全量照合 |

最初のqa01の必須+n欠落とqa03の発射前Ghost速度参照は実FAILとして保存し、原因修正後の該当検査で通過。検査閾値・試行数・エラー除外を変更していない。公的一巡のconsole/pageerror/requestfailed/HTTPerrorは全0、faviconも正常。390公開wrapperの追加起動は別receiptへ記録。

protected-contract-final.jsonでFlightPhysics/FeelSafety/FeelObjectVisual/launch-input/progressionとfeel events/presets/safety/camera値がr27cと不変。物理軌道の一致を保証した意味ではない。ローカル608mと公開200mは入力/強化/位相/画面寸法が異なり比較できない。旧r27c名目70/40°とも位相bit一致ではなく、距離差を演出の物理改善としない。

## 実物確認と残件

同版のガラス・木片・初接触・結果・再投・390px設定の実画像を当担当が見て、最初に見つけた文字clip/HUD接近/灰色大面flash/cyan甲羅被覆を局所修正し再撮影。comparison-gallery.htmlとvisual-review.jsonへ保存。final r28独立実画像レビューは未実施、親提供のr27独立指摘は反映した。面白さ・好み採用は本人未確認。

- 全M1〜M17未合格。r28全40投は未実施、旧r27は強10/弱0で切断中断し全PASSではない。
- 同位相の最新3preset動画とM14全ソースframe映像検証は未完。今回telemetryのrolling最大flash2だが、CDP12fpsの155秒映像は全sourceframeではない。公開は発射直前から30秒の命中イベント抜粋、再生速度変更なし。冒頭30秒ではチャージだけだったため、既存映像を切り直し実フレームでガラス突破を確認した。タイトルは静止画のみでM4完全通し録画とはしない。
- ゲーム音graphの収録/復号/RMSは確認、実聴・物理iPhone・実機FPSは未確認。browser390pxを実機としない。
- 既知床侵入/Collider残差/強入力距離短縮はこの演出差分で解消認定しない。G-POWER、モール反復、mobile下端の重複帯、結果camera空白等は保持。
- 独自著作権表記と「配信・動画はご自由にどうぞ」、第三者NOTICE/VOICEVOX表記を保持。正式topと他版は不変、r27cは復帰/比較用に保持。Library新保存・private push・r26prerelease・X投稿は今回実施なし。
- 次のN01〜06は未着手。カメラ追従原因（body移動まで平滑化/反跳時lookahead方向）を現ソースと実物で確認し、N01→引き画角N02→装備/地域N03N04→成長N05→派手で笑える全体N06へ。今回V01の公開結果を先に返す区切りで、本人の案選択待ちは追加しない。

## 正式PLAN状態

担当は実装全項目リリー、R02は親／カニ係（公開判断は本人）。以下は正式plan.jsの着手/提出/合格状態で、コードの実装有無と同義ではない。V01は実装・preview公開済みでも独立レビュー等が残るので作業中を維持。勝手にgate/submit/合格へ書換えていない。

```text
=== PLAN STATUS ===
出力：2026-10-29 02:19 JST
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
| N01 | 次候補 | 未着手 | — | ok |
| N02 | 次候補 | 未着手 | — | ok |
| N03 | 次候補 | 未着手 | — | ok |
| N04 | 次候補 | 未着手 | — | ok |
| N05 | 次候補 | 未着手 | — | ok |
| N06 | 次候補 | 未着手 | — | ok |
| V01 | 演出最優先 | 作業中 | 10/29 01:30 | ok |
集計：未着手 26／作業中 6／提出済 0／差し戻し 0／合格 0
PLAN-CHECK: ◯
=== END PLAN STATUS ===
```

## 保存・読戻し

同版public-publish-receipt.json/current-receipt.json、build03/qa04、public-smoke01、playtest02、protected-contract-final.json、visual-review.jsonを既存evidence/r28へ保存。HANDOFF.mdの今回節へ入力/commit/hash/試行/到達/次工程を集約。既存r26/固定版/正式topの巻戻し・削除・新課金・新認証は0。


最終配信枠修正：public883528bはgame/全ファイル・ZIP不変、390pxヘッダー収まりと動画だけ修正。公開一巡は直前9e61314、最終883528bは390起動→照準・1280寸法84/716維持・動画全量SHA/正式root全量byte不変を確認（public-mobile03 exit0/errors0）。ゲーム/ZIPに関係ない一巡全反復はしない。ZIPダウンロードは実取得済み9e61314固定URL、両public commitで同一blob。新動画5,373,464bytes/SHA516ccb3ffea5d82ac99fb7b9c94592c50f395f3e18afd4f2cf5f7d597bdde038。mobile01検査待機null失敗とmobile02で検出したheader87.47>84の残差、修正後mobile03のheader104.94内paragraph97.94通過を履歴保持。
