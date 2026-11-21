# r32m 連投性能修正の途中成果（未合格）

試遊URL：https://sakiyayoi.github.io/sakiya-crab-flight-play/preview/r31a/ （既存試遊版。今回の診断r32/r32mは未公開、正式トップも変更なし）
ダウンロード：今回の診断実行ZIPは未配送。保存済みの比較物であり、A案作り直し版ではない。検査資料はFINAL receiptでLibrary IDを返す。

## 実装した修正

- Nativeの長い投記録を40msごとに再送していた。第2投の充填中だけで124,814文字×413回=51,548,182文字（r30 baseline-normal01）。変更時のみ送信し、ブラウザは同じ記録の参照を保持。完了後Prepareで最新記録をクリアし、全履歴のIndexedDB保管は残す。中断時も記録を捨てず保存。
- 入力受領の32ackを毎Reportで全セッション配列から線形探索していた。IDのMap参照へ。記録件数・拒否理由・全履歴は減らさない。
- 10連投の厳密ゲートG-PERF10を全項目の毎版BASEへ追加。生RAF値から平均/最悪を再計算、後続が一つでも初投より大きければFAIL。許容差・丸め・外れ値除外なし。
- r32mは初期WASM予約だけ32→288MiBの比較。r32で第2投250,085,376→300,154,880bytesへ拡張したため、観測済み286.25MiBを次の16MiB単位へ切り上げた。r32mは全10投301,989,888bytesで拡張0。最大2048MiBと既存成長設定は保持。起動時予約が増えるコスト・物理iPhone未確認あり。全遅延の解決とはしない。

## 10投の合否（生値・許容差0）

測定端末は同じMac、Chrome156 headless / ANGLE Metal AMD Radeon Pro5500M、390×844/DPR1。gameplay暖機0、タイトルの資産readyのみ。Lv0/gear0、マシマシ、揺れON、音OFF。毎投もう一投→合法Enter充填10秒→100/40°右→結果。RAF wall intervalをStart/Retry直前からflightを経て最初のresultまで採取。録画・スクショ・強制GCなし、JSON書出しは区間後。OS全体の他負荷を完全隔離した証明やGPU全Present・物理スマホFPSではない。

| 投 | 平均ms（判定に使った生値） | 最悪ms（同左） | 初投比 |
|---|---|---|---|
| 1 | 17.745808468314863 | 383.3000000000029 | 基準 |
| 2 | 17.13314393939394 | 283.1999999999971 | PASS |
| 3 | 17.566716362281028 | 233.30000000001746 | PASS |
| 4 | 17.339951475940147 | 200 | PASS |
| 5 | 17.03346303501944 | 150.10000000000582 | PASS |
| 6 | 18.02834224598931 | 183.29999999998836 | FAIL |
| 7 | 17.365044018643204 | 283.3999999999651 | PASS |
| 8 | 17.068657718120836 | 149.90000000002328 | PASS |
| 9 | 17.238596491228087 | 233.20000000001164 | PASS |
| 10 | 17.758890290037815 | 166.70000000001164 | FAIL |

r32mは6投目・10投目の平均が初投を超えてFAIL。最悪値は全後続で初投以下でも平均条件が未達。r30元版の有効10投FAIL7件、audit修正r32の有効10投FAIL4件も保存。入力の回転位相と衝突経路が異なるため、この件数減を同一経路の因果的な改善率とは扱わない。r32mの位相66.3726°→344.9039°等の生値、距離、実破壊/二次数、全投入dt/初期剛体をrawとCSVに保管。新たな回転/物理係数や距離補正はしていない。6投目40破壊/842m・10投目21破壊/360mの負荷差を、無変更の再測定で合格にすることはしない。

## 残留と機能検査

- 主windowのJS timer残留は全10投でRAF3・interval1・timeout0、DOM popupは各結果0。Native Scene2/PhysicsMaterial9。Meshは初投903→2投目916、以降一定。Material251〜257、GameObject/剛体数は実破壊に応じて変動し、累積増加を今回の範囲では観測しない。全種類のリークがないという保証ではない。
- 結果の破片/痕は壊した経路を残すため寿命内で存在する。10投後2秒では粒/輪/残像/速度線0、痕1（残寿命0.881秒）、物理破片38。結果で全部削除して測定を軽くする修正はしていない。Nativeイベント購読はconstructor += / Dispose -=をソース確認、実購読者数は未計測。
- `CRAB_FINAL_EVIDENCE=<protocol03> KANI_GPU=1 KANI_PORT=8061 KANI_BUILD_DIR=<r32m> npx playwright test -c qa/final-r27.playwright.config.js --grep 'retry audit'`：1PASS/0FAIL。同版の完了/中断の各recordが各一回だけ送信・履歴保存、Prepare後に前投cacheと射出値が消える、未充填Releaseはcharge-not-ready、全259入力ack一致、エラー0。
- 音ONの上の機能診断で再準備後のpack再生/loop/pendingWall/pendingVoice、procedural音、voice activeは全0。実聴ではない。
- protocol01はテストのnull待機漏れで起動前FAIL、protocol02はJsonUtilityがnullクラスをserial0等の既定objectとして送る仕様を誤ってnullと期待してFAIL。実ゲームは変えず、前投のserial/power/source/settings/bodyがすべて空であることを具体値で確認するテストに修正してprotocol03。失敗ログは保持し、正式条件を弱めたものではない。
- `node --test harness/scripts/perf10.test.js`：7PASS/0FAIL。単一worst超過、meanのみ超過、最小表現差、旧版、旧ZIP、1frameの古いresult、同じ投の使い回しを棄却する。無効pilotのraw PASSは別の棄却記録を付けて保持。
- Chrome内蔵CPU記録は音ON機能診断から取得（新ソフト導入0、合否用10投とは別）。主要WASMサンプルが関数番号のみで、公開最適化buildには対応symbol表が無く、Native関数別の残り原因は未特定。新profiler導入や架空の関数名帰属はしていない。

## A案と全体計画

Library libfile_c9f73e349de88191999cdc531f546a84を現行公式経路でMacに取得し、xattr一致・実画像を見てAを確認。SHA2acd8a944a772b175acf3060cddb3a0e009135566e97eff9165d2ff0736d0acc。本人23:42UTC「Aかな」で選択待ちは解消。赤カニ/黄衝撃、淡ミント/生成りのイラスト背景、控えめな輪郭、前景は形と陰影で明瞭、全画面ぼかし禁止。静止画方向採用とゲーム品質は別。

現行FarWorldLifeR10の鳥48/人22はhitbox/得点/破壊なし、近接驚きだけ。LifeActorsR20は現sourceに未統合。新要件は実接触でコミカルに吹っ飛ぶ→表情/実得点→再投復元まで。背景を人鳥に差し替えただけで完了にしない。

REFERENCE-r32.mdは原作2011短尺、LaunchaLibre Rutube（親の38.26s結果・52.99s強化の実見）、作者一次資料とr30の6場面18件を区別して保存。自分のRutube再生は未成立、精密hitstopフレーム数は未計測。参考動画やその素材をゲームに転載しない。PF01の性能→A画面契約（N01/02）/固有対象（N04、人鳥）→装備/成長（N03/05）→命中から結果再投（V01/N06）へ既存PLANを一続きに更新。現在Aのコード/造形実装は未着手。

## 保持した未合格・未確認

計測定義：minHeight/minimumSupportはauthored Collider.sharedMesh頂点の各substep世界最下点で、Rendererの見た目やPhysX Cooked侵入量そのものではない。7.34mmは見た目からCookedへの別の最近傍残差。

G-PERF10 FAIL、残りの負荷原因、A全体実装と比較動画、最終同版全Playwright/強弱各20、M14新同版実映像、本人実iPhone/実聴、床支持点21.3mm・Collider7.34mm・TGS距離tradeoff・弱入力短距離・旧モール反復の履歴。r32mの5投目は20.554m/実破壊8/二次0、全入力の連鎖3保証にはしない。今回モデル/Collider/物理係数/VFX値の変更0。r30原本・r31/r31a部分見た目・公開r31aと正式トップを保護。

次の工程は、6/10の位相/接触/投入dtログとNative処理負荷を突き合わせ、残る平均差を修正する。必要なら同じ最適化条件のシンボル付き非公開診断buildで関数別帰属を得る。許容差の変更、初投を故意に遅くする、都合よい再試行、破片を消して世界を縮める方法は使わない。A実装は性能優先を守って続行。料金0・新認証0・権限変更0・新担当0。

実行コードcommit：1d1a4ada1fd06990af1452b3668b43be5c1ef458。
源SHA256：b2dca9b2e2a2e0e497b819b1fa67d175d14fca9ee5c9abcf883a602f96af20e5。
診断ZIP：20,289,553bytes、SHA256 1a83622598414620a2d43e1fd0479d7908cbd1fc4c09752c465e89a29c745a18。
資料・最終HEAD・remote readback・Library receiptは既存HANDOFFとFINAL-RESULT-r32.jsonを正とする。未合格なので【進捗】であり【提出】ではない。


PF01公式ゲートの実結果（commit25dfd28ab3e92264ecac52d7b62948940c565225）
`KANI_ZIP=<r32m ZIP> bash harness/scripts/gate.sh PF01`：6件中4◯/2✕。G-PLAN/G-GIT/G-VER/G-ZIP◯、G-PERF10✕（6/10投平均超過）、G-LINK✕（診断ZIP未配送、架空URLを作らない）。40投/他ゲートはscope外、PASSを主張しない。初回はgate.js99行の20MB単一同期readがETIMEDOUTで停止、archiveの既知SHA/全量CRCは先に成功済み。256KiBずつの全量hashへ実変更した正規1回で同じSHAと88ファイルを確認。判定内容/閾値/試行数を変えず、初回I/O失敗も記録。資料だけの後続commitではゲームtree不変。
