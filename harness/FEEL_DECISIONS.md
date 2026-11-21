# 手触りの値の決めた記録（FEEL_DECISIONS.md）

feel.json の `version` を変えるたびに、下に 1 つ見出しを足す（古いものは消さない）。見出しは **`## feel.json 版：<version>`** の形のまま（ゲートの G-FEEL-RECORD がこの文字を探す）。
spec.md の 5-2／5-2b の値も同じに直し、spec.md のどこかに `feel.json 版：<version>` と書く。【提出】の「変えた値」にはこの版の表を貼る。

<!-- 書き方の見本（この見本はゲートに数えない。見出しの頭に ## を付けたものだけ数える）
## feel.json 版：（見本。ここに version）
相談：GPT Pro（PROCESS.md の相談文）、10/28 21:10 JST
| 出来事 | 値 | たたき台 | 決めた値 | 理由 |
|---|---|---|---|---|
| ガラス突破（マシマシ） | ヒットストップ | 12 フレーム | 10 フレーム | 12 だと動画で重い。ガラスが一番長いのは変えない |
| 発射（マシマシ） | 揺れ振幅 | 0.2 m | 0.25 m | 方向「もっと強く」 |
-->

## feel.json 版：feel-r27-candidate-1

決定：リリー、2026-10-28 UTC。同版検証の候補値固定。親の現行spec裁量指示を適用。GPT Pro相談を行ったとは記録しない。本人の気持ちよさ採用は別。数値変更0、mechanism-3からstatus/versionだけ更新。

| 出来事 | 値 | たたき台 | 今回の固定候補 | 根拠・限界 |
|---|---|---|---|---|
|charge|particles|5|5|実装/正式値照合で観測、好み未採用|
|charge|squash|0.1|0.1|実装/正式値照合で観測、好み未採用|
|ready|particles|12|12|実装/正式値照合で観測、好み未採用|
|ready|squash|0.15|0.15|実装/正式値照合で観測、好み未採用|
|ready|shakeM|0.05|0.05|実装/正式値照合で観測、好み未採用|
|ready|shakeMs|120|120|実装/正式値照合で観測、好み未採用|
|launch|stopFrames|2|2|実装/正式値照合で観測、好み未採用|
|launch|shakeM|0.1|0.1|実装/正式値照合で観測、好み未採用|
|launch|shakeMs|150|150|実装/正式値照合で観測、好み未採用|
|launch|kickM|0.2|0.2|実装/正式値照合で観測、好み未採用|
|launch|particles|20|20|実装/正式値照合で観測、好み未採用|
|launch|squash|0.2|0.2|実装/正式値照合で観測、好み未採用|
|tankGlass|stopFrames|8|8|実装/正式値照合で観測、好み未採用|
|tankGlass|maxMashimashiStopFrames|12|12|実装/正式値照合で観測、好み未採用|
|tankGlass|shakeM|0.3|0.3|実装/正式値照合で観測、好み未採用|
|tankGlass|shakeMs|200|200|実装/正式値照合で観測、好み未採用|
|tankGlass|flashPhases|1|1|実装/正式値照合で観測、好み未採用|
|tankGlass|flashPriority|5|5|実装/正式値照合で観測、好み未採用|
|tankGlass|particles|60|60|実装/正式値照合で観測、好み未採用|
|tankGlass|residueMs|10000|10000|実装/正式値照合で観測、好み未採用|
|tankGlass|slowScale|0.3|0.3|実装/正式値照合で観測、好み未採用|
|tankGlass|slowMs|300|300|実装/正式値照合で観測、好み未採用|
|destruction|stopFrames|4|4|実装/正式値照合で観測、好み未採用|
|destruction|shakeM|0.12|0.12|実装/正式値照合で観測、好み未採用|
|destruction|shakeMs|150|150|実装/正式値照合で観測、好み未採用|
|destruction|flashPhases|1|1|実装/正式値照合で観測、好み未採用|
|destruction|flashPriority|1|1|実装/正式値照合で観測、好み未採用|
|destruction|particles|16|16|実装/正式値照合で観測、好み未採用|
|destruction|visualKnockbackM|0.15|0.15|実装/正式値照合で観測、好み未採用|
|destruction|residueMs|10000|10000|実装/正式値照合で観測、好み未採用|
|chain2|stopFrames|4|4|実装/正式値照合で観測、好み未採用|
|chain2|shakeM|0.12|0.12|実装/正式値照合で観測、好み未採用|
|chain2|textPeak|1.3|1.3|実装/正式値照合で観測、好み未採用|
|chain2|textRest|1.1|1.1|実装/正式値照合で観測、好み未採用|
|chain2|shakeMs|150|150|実装/正式値照合で観測、好み未採用|
|chain4|stopFrames|6|6|実装/正式値照合で観測、好み未採用|
|chain4|shakeM|0.18|0.18|実装/正式値照合で観測、好み未採用|
|chain4|textPeak|1.45|1.45|実装/正式値照合で観測、好み未採用|
|chain4|textRest|1.18|1.18|実装/正式値照合で観測、好み未採用|
|chain4|shakeMs|150|150|実装/正式値照合で観測、好み未採用|
|chain7|stopFrames|7|7|実装/正式値照合で観測、好み未採用|
|chain7|maxMashimashiStopFrames|10|10|実装/正式値照合で観測、好み未採用|
|chain7|shakeM|0.25|0.25|実装/正式値照合で観測、好み未採用|
|chain7|flashPhases|1|1|実装/正式値照合で観測、好み未採用|
|chain7|flashPriority|2|2|実装/正式値照合で観測、好み未採用|
|chain7|textPeak|1.6|1.6|実装/正式値照合で観測、好み未採用|
|chain7|textRest|1.25|1.25|実装/正式値照合で観測、好み未採用|
|chain7|shakeMs|150|150|実装/正式値照合で観測、好み未採用|
|landing|stopFrames|3|3|実装/正式値照合で観測、好み未採用|
|landing|shakeM|0.08|0.08|実装/正式値照合で観測、好み未採用|
|landing|shakeMs|100|100|実装/正式値照合で観測、好み未採用|
|landing|particles|8|8|実装/正式値照合で観測、好み未採用|
|landing|squash|0.15|0.15|実装/正式値照合で観測、好み未採用|
|perfect|stopFrames|4|4|実装/正式値照合で観測、好み未採用|
|perfect|shakeM|0.1|0.1|実装/正式値照合で観測、好み未採用|
|perfect|shakeMs|150|150|実装/正式値照合で観測、好み未採用|
|perfect|flashPhases|1|1|実装/正式値照合で観測、好み未採用|
|perfect|flashPriority|3|3|実装/正式値照合で観測、好み未採用|
|perfect|particles|16|16|実装/正式値照合で観測、好み未採用|
|perfect|squash|0.2|0.2|実装/正式値照合で観測、好み未採用|
|good|stopFrames|2|2|実装/正式値照合で観測、好み未採用|
|good|shakeM|0.05|0.05|実装/正式値照合で観測、好み未採用|
|good|shakeMs|100|100|実装/正式値照合で観測、好み未採用|
|good|squash|0.1|0.1|実装/正式値照合で観測、好み未採用|
|miss|stopFrames|0|0|実装/正式値照合で観測、好み未採用|
|miss|shakeM|0|0|実装/正式値照合で観測、好み未採用|
|miss|particles|0|0|実装/正式値照合で観測、好み未採用|
|newRecord|flashPhases|1|1|実装/正式値照合で観測、好み未採用|
|newRecord|flashPriority|4|4|実装/正式値照合で観測、好み未採用|
|newRecord|confetti|60|60|実装/正式値照合で観測、好み未採用|
|flightFast|speedThreshold|20|20|実装/正式値照合で観測、好み未採用|
|flightFast|ghostIntervalMs|90|90|実装/正式値照合で観測、好み未採用|
|flightFast|lineIntervalMs|90|90|実装/正式値照合で観測、好み未採用|

プリセット倍率・UI時間・描画寿命/予算・camera・flashはfeel.jsonの全項目を変更せず固定。M14上限3、M15オフ0、mobile目標30fps/粒子下限0.6を弱化しない。単色オレンジ1phaseは機構候補のまま、安全計数と動画を確認し、白→黄の細目との違いを隠さない。


## feel-r28-impact-1（本人16:21演出全面改修の制作候補）
物理・入力・presentation-clock・安全gate・プリセット倍率・hitstop/slow値はr27と同byte/同値。描画だけ変更。particleSize .08→.11m、particleLife650→780ms、ringLife320→280ms、ringRadius2.4→3m、ghostLife220→180ms、flashAlpha .20→.24（全面washから端の4楔へ変更）。popIn120→65ms、hold600→200ms、out200→180ms、rise24→52px、font24→26px、chain32→37px。形状・材質・重力/回転・方向性・太さの階層を作り、粒数増量は0。固定楕円残像から姿勢付き甲羅/ハサミ/脚のsilhouetteへ。旧host擬音はconsumer起動後に抑止。OSreducedはpopupの移動/拡大/回転とcountupを止める。本人好み採用・実機/実聴PASSではない。


実画像修正r28-2：文字がclip-pathで欠けるため背景spanだけをclipし、文字の安全幅とHUD下端を基準に配置。四隅の灰色面は接触点の漫画的閃光へ置換し、gate/count/タイミング不変。cyan残像が甲羅を覆ったためalpha .24→.11、実速度方向の後方0.5mへ寄せる。食品片を陶器から分ける。既存1次映像は旧shapeのまま保護し、修正後実物を比較する。
