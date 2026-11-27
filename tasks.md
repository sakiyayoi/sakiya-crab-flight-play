# カニとばし 計画表

元にする要件シート：spec.md v1 / 2026-10-28 19:17 JST / SHA256 812cb8be9479f502d93693cd079bce00ab1f501badea863204190ec5ac061a1e。
元テンプレ：Slack F0CCCCZF25D。計画と制作はリリー、統括・要件確認はマツリ。共有制御ファイルの書込担当はこの既存担当一人。親がSlack提出と質問を担当する。
時刻の締切は置かず、条件がそろった部品から提出する。M1〜M8が先、M12〜M15/M17の仕組みとM9〜M11/M16の素材・クレジットは分離して準備可。値の仕上げと細部は通し版のあと。

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| 端から端まで遊べる通し版 | S0：M1〜M8、進行不能P1〜P7を0にし、同版本体ZIP・SHA・全PASSログ・通し録画を作る | リリー（既存の単独書込担当） | なし | qa/playable-v1で `npx playwright test` 全件PASS、強20/弱20を5投単位で再読込しメモリ記録、3回連続一巡、Game Studio game-playtest、通し録画1本 | 時刻なし・合格条件達成時 | 確認中 |
| SAK-16 / 1件目 | mall表示の重複Part生成除去・既存template再利用。追加poolは別段階 | リリー（今回の直接exec） | 2026-11-11クロエ判断・Linear計画539a4ebc読戻し | mall50対象の表示/破壊部品/Collider同一、同条件WebGL再計測。C-7/連鎖下限旧3不変。診断をstrict10合格にしない | 時刻なし | 着手・部分実装。99本C#型検査exit0/error0/warning17。実階層/実画面/再計測は未実施。2・3件目未着手 |
| S1 演出の仕組み | M12〜M15/M17：FeelConfig・feel.json・3preset・7演出・点滅制限・揺れオフ。専用consumerを接続し初回正式21検査12PASS/3FAIL/6SKIP。2原因修正後2PASS。正規再審査で表示build許可、r27c正式演出22PASS。実録で見つけたC8の開始表示も局所修正、同実投の時系列PASS | リリー（未分担、親が既存担当へ渡す場合は専用ファイルのみ） | なし（値の仕上げはS0後） | preset3本の同入力録画、M14全1秒窓3回以下、M15揺れ/蹴り0・OS軽減時点滅0、M17実測、`npx playwright test` | 時刻なし・S0と仕組みのみ並行可 | 進行中 |
| S2 提供音声・クレジット | M9〜M11/M16：ツンツン4種のみをvoice/へ分離、README保持、NOTICE・CREDITS・ローカル公開ページ文・クレジット画面を準備 | リリー（既存担当。親が音声とC1〜C8原典を提供） | なし（共有UI統合はS3） | 指定ZIP SHA57b0faf3…一致・CRC9entries、README音声SHA8件一致、voice外の音声0、normal/unused0、3表記とお願い文を検査 | 時刻なし・部品の合格時 | 進行中 |
| S3 同版統合と公開前検査 | M1〜M17を同一版へ統合し、機能・安全・クレジットを実物で確認。release_review_sheet.mdはカニ係へ証跡を渡す | リリー（統合単独）／カニ係（進捗・確認シート） | S0,S1,S2 | `npx playwright test`、Game Studio実画面/不具合一覧、M13の3録画、M14/M15/M17実測、ZIP SHA読戻し | 条件達成時・10/29公開版準備 | 着手前 |
| S4 正式公開の条件確認 | 本人15:33統合承認で未合格preview許可済み、正式topは全M1〜M17合格後だけ更新。X投稿は親担当 | 親／マツリ（依頼窓口） | S3 | preview/r27cとlatestの対象版/配布SHA/起動を確認。正式topは全gate条件、X実投稿は親が確認 | 時刻なし・承認後 | 着手前 |

## NEON 第2段（この隔離コピーのみ）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| NEON stage2 | モール背景暗転、WebGL-stage2、ローカルブラウザ検証、NEON-REPORT-STAGE2.md | Codex（今回の直接依頼） | NEON-BRIEF-STAGE2.md、第1段実物 | 74対象、3220.0295m/46破壊/差0、区間外不変、build errors0、browser2pass | 指定なし | 第2段完了：Native契約一致、WebGL errors0、Chrome 2 passed。NEON-REPORT-STAGE2.md参照 |

## NEON 第3段（この隔離コピーのみ）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| NEON stage3 | 採用済み通し版を維持し回転渦・全区間暗転・看板を実装、検証・ビルド・配信・報告 | Codex | NEON-BRIEF-STAGE3.md、第2段実物 | 74対象、3220.0295m/46破壊/差0、全9区間監査、build errors0、browser2pass、8145読戻し | 指定なし | 停止・未完了：Native成功、WebGL復旧1回で成功。browser1pass/1fail（演出オフscale厳密一致）。再試行なし、8145 HTTP読戻し未実施。配信スクリプト・NEON-REPORT-STAGE3.md保存済み |

## 条件と確認の対応

- M1：自動suite全部PASS。既存r25はworld-boundでFAILのため採用せず、現在のr23入力契約を持つr24 de13c3aの動作基準からS0を分離する。r25実験・ログは保持し、旧比較ZIPやr18へ戻さない。
- M2/M3：版名 kani-v0.1-playable-r26、本体ZIPとSHA読戻し。5か所一致は公開後の条件だが版の対応を記録する。
- M4/M5：タイトル→あそぶ→発射→結果→もう一投の連続録画。3回連続結果到達を自動試験し、人による最終確認とは区別する。
- M6/M7：強20/弱20の全40、physicsFault/300秒未決着/5m未満/全console等のエラー0。失敗行を除外せず、5投ごと再読込してJS/WASMメモリ量を記録。旧r25の結果を新S0のPASSへ足さない。
- M8：1280x800・390x844でタイトル→照準、入力枠が画面内。実iPhoneとemulationを区別する。
- M9/M10/M11/M16：専用voice/、NOTICE、CREDITS、README/公開ページ用ローカル文、タイトルからのクレジット画面。既存LICENSE本文の削除・ライセンス変更はしない。
- M12/M13/M14/M15/M17：専用演出モジュールの仕組みだけ並行、C1〜C8原文はdocs/reference/C1-C8-matsuri.mdへ保存済み。GPT Proとの値決めは未実施、値の仕上げはS0後。安全2条件を変更しない。
- #14背景/#15モール、旧v0.3物理C-7、侵入/Collider残差は履歴を保って公開後の優先へ。未解消を直ったことにしない。specのdesign-quality C7と旧物理C-7を混同しない。

## ファイルの受渡しと単独書込

- S0/統合担当のみ：CrabFlightGame.cs、FlightEventStream.cs、ImpactStop.cs、FloorBuildR10.cs、web-template/host.js、index.html、既存共有シーン、Playwright共通harness。
- 分離して渡せる演出部品：新規FeelConfig.cs、Assets/StreamingAssets/feel.json、専用MashimashiPresentation.cs、web-template/feel.js。イベントgeneration/id/type/objectId/material/position/normal/velocity/strength/secondary/weight/physicsTimeを受け、物理値/score/破壊数へ書かない。表示は実時間、presetオフ/揺れオフ/OS軽減を受け、M14の全フラッシュを一つの窓で制限する。共有controllerのpatchは統合担当へ返す。
- 分離して渡せるクレジット部品：voice/NOTICE.md、提供READMEと4種WAV/OGG、CREDITS.md、web-template/credits.js、README/公開ページ用の文案。共有index/hostは変更せず接続関数とpatchを返す。外部公開・ライセンス変更はしない。
- 新担当を増やさない。現在、専用部品の他担当への書込み委任は未実施。
- 状態の引継ぎは既存案件HANDOFF一箇所。sakiya-kbはread/writeしない。個人情報は扱わない。

## 同じ版の現物と分離部品（2026-10-28 UTC）

- T01の先行検収用はマツリの追加許可で未合格のまま保存済み。Library libfile_86db5886ee2481918344166f8be37fae v0、20,628,749 bytes、SHA256 6bda82b94e9b2e1d75ea242f380f30a4d8a6689ac049c023b4c52b1ab343ea2a。Build本体はr24基準・source0cf63b0のまま。添付は17/40途中snapshot、M1〜M8の完全合格ではない。
- S0の全40投は継続中。2画面起動は同版で通過、主一巡の初回は録画用FFmpeg1013未導入でゲーム前FAIL。既存1011で直接録画する修復を準備、現在の40/production sourceへ混入しない。完了済みの同版PASSを再作成しない。
- S1の分離部品はtask-5/parts/feel-v1。全flash共通rolling gate・局所clock・camera安全の7100assert PASS、設定/OS/保存拒否の26assert PASS。実ゲームのM12〜M17、映像とfpsは未検査。値はたたき台、GPT Pro相談未実施。
- S2の分離部品はtask-5/parts/voice-credits-v1。原提供README保持・4種8音声SHA一致、NOTICE/クレジット画面consumer/公開ページ用ローカル文/既存第三者NOTICEを準備。現在の実行物には未統合、実聴/同期は未確認。効果音39WAVは原制作MANIFESTと一致。
- 既存第三者3DはKenney Food Kit 2.0（料理/皿、CC0）とmethodical pixelの比較用カニ（CC0）を区別し、自作著作権へ含めない。残る街packの個別原制作照合は未完。既存LICENSEを削除/変更しない。
- M12/M13の接続不足：現行r24の#kickはCSSで非表示、host.inputHeldは空、runtime.KickDownはuseRotorBursts時は拒否。実PERFECT確認にはS0後に既存3回キックの接続を復帰する必要がある（判定幅維持）。既存不具合ID SPEC-V1-KICK-RECONNECT、親から仕様の現状との差をマツリへ渡す。


## T01検査追補版 v1

Libraryは同じlibfile_86db5886ee2481918344166f8be37faeをv1へ追補。native file_00000000e26481f595ef22540ed1c19e、38,631,332 bytes、SHA256 077a7330a1ff2fbb6546ace1b23631a2acd7be2f7e9caa38a76611776c0cde8d。全量読戻し/129entry CRC/全40行/動画SHA一致。v0保持、実行Buildの全byte不変。

S0はマツリ検収待ちとして確認中。初回 `npx playwright test` は4PASS/1FAIL（録画環境のみ）、修復後 `npx playwright test --grep-invert floor-stop` は4PASS/0FAIL。受理済み同版40は保持し再実行0、最新5項目はPASSだが、修復後の単一全件コマンドPASSログは無い。この合成証跡のM1扱いを親からマツリへ返す。

M4通し録画130.333秒/1564全frame復号、M5三回連続、M6自動40投のfault/未発射/未決着/5m未満0、M7記録されたbrowser errors0、M8二画面起動を確認。Game Studio手順はtouch emulation/音39decode/飛行pause-resume/retry/軽減と記録復元を実行し画像視認。実iPhone/実FPS/実聴/作者の楽しさ・採用は未確認。Skill自動読込み未確認と本文手動適用を区別。

今回の幾何支持最小-0.102473676mを残件として明記。旧exact入力の27.87cm native侵入、Collider近似、同入力差、旧二次0、背景反復/モール感の解消は認定しない。M9〜M17は引き続き未合格。T01確認用候補として納品可、公開版/完成版として不可。新規公開/push/ACL変更/導入0。


## r27 復旧後の結果 2026-10-28T14:17:29.420108+00:00

M9/M10/M11/M16の部品：声4種8ファイルは原文SHA保持。ゲーム内クレジットを1280×800/390×844で実見、閉じる/フォーカス復帰、初期ミュートを確認。既存r27aのSoundPack40ファイル梱包漏れを別copyで直した1回の再検査は、390×844で4decode・ガラス/新記録発音receipt・browser errors0。これは人の実聴や自然接触音同期ではない。公開場所は未更新、全3D原制作とownerのLICENSE判断も未確認。

M12〜M15/M17：WIP機構を実装、最終値は未採用。r27aのLoad前初期化とhold時計2原因は正式検査2PASSで修正確認。r27bの実物白化/描画子ノックバックのbuild03は自動承認レビューが外観/VFX STOPと衝突と判定し、プロセス作成前に拒否。別経路・同条件再送0で親へ確認待ち。新candidateの全正式検査、実一巡、同入力3比較動画、実映像の点滅計数、スマホFPSは未完。正式G-LINKは本人確認待ちのまま、gate/submit/push/release/upload/公開0。


## r27c 正規再審査後の現状（2026-10-28 UTC）

compiled/native03c874171c59ec2f6eb633ba66c8a3681d6667f1、wrapper a5bcc3f01895af1564981b729330c6d78becffe7。r27b build拒否は追加本人委任原文と時系列で同じ正規審査へ1回、許可後build03 exit0。旧拒否/FAILは保持。

S1：独立5指摘を直し再review、Native局所1PASS（35visual/13Collider不変）。同版r27cの正式feel+credit22PASS/0FAIL、M14/M15/OFF機構と実一投を確認。3preset初期発射状態を一致させた音付き実録（215.891/273.586/273.586m、実PERFECT各1）、再投・保存復帰・実画像確認を実施。C8の旧逆戻りも実録で発見、JS-only修正後の実投は0.0→218.2→259.3→259.4、1cue、fault空。旧3動画はC8最終修正前と明示し、最新動画にすり替えない。

S2：4ツンツン声・credits・初期ミュート・自然接触発音receiptは接続済み。実聴/全3D個別原制作照合/公開credit更新/owner LICENSE判断は未完。S0受入/G-LINK・GPT Pro最終値・M13最終版3動画・M14映像全窓・M17物理スマホは未完。全M合格/提出/公開にしない。背景・モデル等のSTOPは継続、glass梁の遮蔽・固定pop位置・resize直後の下端重複帯を既知として保持。

実行物・動画・全ログは既存案件flight-floor-first-r21/evidence/r27とREPORT-r27.mdへ保管。コード変更はこのbranchのみ、r26/formal/公開版不変。gate/submit/push/release/upload/公開/費用/認証/新install/KB参照0。


16:15 UTC本人4点是正：正式PLAN（harness/PLAN.md）全31項目と担当/P1ブロック、preview/r27c別名、同版r27c実行ZIPのGitHub download、favicon404/consoleゼロを先に完了。その後N01〜N06。r26 t01-r26とr27cのZIP/releaseを混ぜない。最新40はMac切断で強10/弱0まで、全PASSではない。


## r28 V01の返却区切り（2026-10-28 UTC）
本人16:21の演出最優先を実装→実画像修正→同版build→qa04 13PASS→公開一巡→390配信枠修正へ。実行コード42b480f、公開883528b/Pages40735790654 success。REPORT-r28.mdと既存HANDOFF/evidence/r28へ全根拠を保持。全M未合格のpreviewであり、V01の正式gate/独立最終reviewは残るため作業中。次は既存PLAN N01/N02から6項目、本人の小案選択待ちは追加しない。private push/r26prerelease/全40/3preset動画/実機/実聴は今回未実施。


## r29 通し候補の計画
| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| 端から端まで遊べる通し版 | N01〜N06を既存gameに一巡接続 | リリー単独統合 | r28保存・配送済 | 開始/装備/充填/発射/地域接触/結果/購入/再投/保存復帰 | 条件達成時 | 実装中 |
| camera | body移動追従＋compositionの平滑化、速度方向の先読みと引き | リリー | 通し基準 | Native移動/反転fixture＋実画面/縦横 | 条件達成時 | 実装済・未検査 |
| loadout/growth | 既存財布・3段強化の装備解放、選択保存 | リリー | camera契約 | 実選択/Collider13/購入/再投/旧save不変 | 条件達成時 | 実装済・未検査 |
| route | 7区間に38対象・13種の地域固有接触セット | リリー | 共通接触/破壊 | 自然到達した実破壊IDと明示fixtureを区別/実画面 | 条件達成時 | 実装済・未検査 |
| verification/release | 同版全suite/40・映像安全/OFF・短版名preview/ZIP | リリー | 統合build | 実ログ/失敗保持/SHA/公開実操作/正式top不変 | 条件達成時 | 未実施 |


### r29 2026-10-28 18:57UTC 実装・試遊観測
N01〜N06はcode4da90a9(Nativef739)で統合。強20/弱20 fault0（31保存＋9復旧、warmup重複0）、異なる36ケース＋UI-only修正2PASS。public99aa023/Pages40749457557success、preview/r29/latestで147.8745m/21破壊と再投・保存/主要11資産・ZIP SHA22a92ea7…読戻し。全sourceframeM14/M17/実聴/本人採用・G-POWER(要求実差28.1644)未合格、floor支持点min-0.13329m残。正式status/閾値不変。担当状態/実コマンド/動画/残件はREPORT-r29.md。旧r26はprivate t01-r26へ別SHA077a7330…で読戻し済、r29混入0。


## r30 G-POWER定義確定・修正方針
出典：マツリ Slack C0CBSU8TA2W / thread1793159062.741849 / message1793214382.783659（親のtrusted引継ぎ）。勢いは充填終了後に射出する目標値。充填前baseではない。±5閾値は不変。入力→充填状態→目標→実射出は各投に記録。r29再集計は合格4/40・不合格36/40（逆ではない）。全40Release受領成功、現在のCoreLaunchPowerの充填再換算が全40の実値を浮動小数誤差内で説明。目標不足ではなく受領後の実射出ズレとして修正する。旧ログ/旧試行番号/旧判定を保持。床支持点min-0.13329mは独立して調査し、形状変換/実接触を照合する。閾値・安全停止を削らず、最終物理版で一貫した全40へ収束。公開r29/正式top不変、次候補r30で修正。


r30 solver比較：baseline r29の同じNative初期body/joint bitsでPGS0→TGS1の単一変数比較を実施。weak16 min支持-30.15mm→-3.41mm、strong2 -10.36mm→-2.34mm。実Collider.ComputePenetrationもbaselineで同深さを示し、表示だけの誤検出ではない。strong2距離225.33→164.09mの差を保持し、TGSを万能改善/採用済みとしない。r30は射出target保持修正とTGSを比較候補へ統合し、同版40と実画面・neighborで再評価。形状/摩擦/反発/outer timestep/安全閾値/warp変更0。公式Unity6000.3 Physics ManagerとUnityCsReference、PhysX5.4.1 Rigid Body Dynamicsのconvergence資料を適合確認、第三者コード導入/実行0。
出典: https://docs.unity3d.com/6000.3/Documentation/Manual/class-PhysicsManager.html / https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/PhysicsEditor/PhysicsManagerInspector.cs / https://nvidia-omniverse.github.io/PhysX/physx/5.4.1/docs/RigidBodyDynamics.html


## r30 同版検証と試遊公開（2026-10-28 UTC）
実行source6a8bdc2c0279484c3e1d5eb004d67d9f72b8d451、kani-v0.5-floor-r30。全Playwright36PASS/0FAILの単一実行で強20/弱20を完了、G-POWER40PASS/0FAIL・目標実射出差最大0。旧r29は合格4/不合格36（逆ではない）、全不合格は受領後の充填再換算。定義Slack1793214382.783659/閾値±5/旧行は保持。
支持点最小-0.021335840225m（strong2,40°右）を残件、異常停止0と侵入全解消は別。TGSの距離tradeoff、Collider外側7.342mm近似、結果パネルがカニ一部を覆う課題、M17実機/実聴、F05値相談、正式全ゲート/受入は未完。正式状態は勝手に提出/合格へ変えない。
公開5b8035a5ceb81253ec179b9630da3b2c513bbfe6/Pages40760987245success、preview/r30とlatestで221.83597m/26破壊→結果→再投、主要11資産/ZIP/3動画全量一致、console等0、390枠/保存復帰/正式top不変。ZIP20,238,312bytes SHA0d905116298c0f424fa9e8548e273134f346ec5c675f420174e9d67e18495489。試遊は未合格、本人採用ではない。REPORT-r30.mdとdocs/r30のCSV/validationへ根拠を保存。新たな物理/視覚変更なし、再build/40再試行0。


## 本人22:56UTC更新：連投性能→全体作り直し
出典Sentinel_765934d26e788191954b26b619f1a303（親trusted引継ぎ）。部分的な背景/材質/カメラ/モデル/VFX改修は保存停止。最優先はr30の2投目の重さの原因修正。破片/粒/残像/文字/音/イベント/タイマーの投前後残存数・寿命・解除とframe timeを比較する。
毎版同一版10連投を必須ゲートに追加。各投の平均/最悪frame timeを生値で保存し、2〜10投のどれかの平均または最悪が1投目を超えればFAIL（安全側の既定解釈）。許容差/丸め/試行削減/都合のよい再試行で通さない。測定誤差で成立しなければ値と理由をマツリへ返す。ウォームアップ/区間/端末/ブラウザ/入力/採取条件を明記。
次にBurrito Bisonの正規公開の実プレイ動画を実見し、命中/飛行/着地と結果/文字UI/色背景/カニ造形の6場面×3点=18差を時刻・位置・色・出入り時間等の実観測で記録する。公式GIFのみや未見フレーム数を証拠にしない。素材流用なし。18差を埋める一貫した通し制作へ既存N01〜N06を包含し、上から最後まで。①〜⑦/初期マシマシ/点滅・揺れ安全/保存を保持。preview短版/latest/ZIP SHA/比較動画/PLAN/10投ログを同版で返す。正式topは全ゲート合格まで不変。

連投性能PF01（ownerリリー、依存なし、期限なし）を最優先。完成条件は厳密10投ゲート。r30 normal01は7投でFAIL、旧完了auditのready再送（第2投413回・約5155万文字）と全履歴ack線形探索を確認。修正は差分送信とID索引、物理/演出の値変更なし。参考動画差18点の通し再構成はこの後。


## 本人追加：見た目3案選択後に全体実装（親trusted引継ぎ）
Sentinel_74639ed6a1588191a1acbbe3da619cfb。性能修正PF01が最優先のまま。外観の確定と実装は、親が同じ場面の静止画3案（色数・線幅・背景/前景差を明示）を生成→本人選択→選定方向で全体作り直し。担当側で重複生成/選択前の見た目確定を行わない。参考18差の研究と現物不具合の原因解析は継続可能。
人/鳥は背景だけでなくカニの接触で吹っ飛ぶ対象とし、非グロテスクな誇張した飛び方・表情・点数反応を通し体験へ含める。既存LifeActorsは驚く演出主体で、接触破壊/点数の実装を完了扱いしない。新接触・得点は実イベントを根拠にし、連鎖/破壊数を水増ししない。背景はイラスト的ぼかしも候補、前景のカニ/壊せる物を明瞭に分離。N04/N06と結び、選定方向待ちはこの外観確定工程だけ。実装未着手、担当リリー、依存PF01/本人3案選択、確認=実接触1回→反応/実得点→再投で再生成。


### A案採用の更新（2026-10-28 23:42UTC）
本人Sentinel_7345a6925c6c81918f42b586256d830cの「Aかな」で3案選択待ちは解消。Library libfile_c9f73e349de88191999cdc531f546a84 v0を現行正規materializeし、当担当も画像の最上段Aを実見。local画像SHA2acd8a944a772b175acf3060cddb3a0e009135566e97eff9165d2ff0736d0acc、2,985,124bytes。赤いカニと黄の衝撃を軸に、淡いミント/生成りの低情報量イラスト背景、控えめな輪郭とくっきりした立体前景。画面全体のぼかしは禁止。人/鳥への実接触→非グロな吹っ飛び/驚き顔/実得点を通しの反応へ。静止画の方向採用であり、現行ゲームの品質合格/正式公開許可の解除ではない。PF01が最優先、A実装は未着手。


r32m比較：r32固定10投の厳密FAIL2/8/9/10を保持。heapは起動/初投250085376bytesから第2投300154880bytesへ増加し以後一定。別の2投診断でPrepare受領183.8msの直後に拡張を記録したが、最悪flight216.7msは約19秒後であり、全遅延の原因と断定しない。Unity6000.3公式Memory/WebPlayerSettingsに従い、初期heapだけ32→288MiB（観測済み最大286.25MiBを次の16MiB単位へ切上げ）で一変数比較。最大2048/幾何拡張の設定は保持。既存物理・外観値は不変。初期予約が増えるコストと実iPhone未確認を記録し、無効なら正式採用しない。次の10投は新しい実変更後の比較で、旧失敗の都合よい再試行/置換ではない。
出典：https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/develop/memory / https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/develop/class-player-settings-web-gl


A実装前の現コード照合：FarWorldLifeR10（far-world-life-r10-candidate-1）はBirdX8×左右×3=48羽、人PeopleX11×左右=22人。3動的meshで羽ばたき/近接驚きのみ、ソースにhitboxes/score/force/destruction無しと明記、実際にもCollider/剛体生成はない。LifeActorsR20の6モデルは現sourceAssetsに未統合。新要件「当たって吹っ飛ぶ・反応・点数」は未実装で、装飾を存在済み扱いして完了にしない。N04/N06の受入は接触ID一回→人物/鳥の非グロ反応と実得点一回→再投で対象/得点状態が復元、安全設定保持。性能修正後にこの既存常駐3meshと専任モデルの役割を分けて接続する。


### 2026-11-01 本人の処理落ち・酔い報告による優先変更
担当リリー。6要素追加（3エリア全採用、大型障害物、素材感、密度、大破片、人鳥接触）は保留で維持、製品実装変更前。現工程は新カニR2bの感触を守るcamera defaultOFF/launchzoomOFF/追従安定と同seed42043・同実発射receipt・1280x800DPR1性能比較。公開R2b/top/r33不変。診断入力比較を通常操作の合格と混同せず、最後に通常の起動→投球→結果→再投。TEAM.md所在未確認、親への不足として保持。実iPhone/実聴は未確認。

### CalmR4 分離候補（非公開・2026-11-01）
担当リリー。CalmR3揺れOFF/通常2巡PASSの私的ZIPは保存済み、性能は未解消。profile01は721frame、最悪233.4msのうちPhysics.Simulate209.6ms/contact1.3ms/break1.1ms。最悪区間は床用細分化の間に25awake破片も繰返しsimulate。カニの床substep/形状/solver32・16/8mm予算は不変とし、既にカニとの衝突除外だった破片だけ別PhysicsSceneへ。全静的接触形状のproxy・破壊enabled同期・本来の二次破壊経路を保ち、破片は外側既存1/240秒で進める仮説。独立sceneは既存実装/Unity公式multi-scene方式を参照、2D OSSは3D用途不適合で導入なし。実WebGLで同seed/receipt/1280x800比較、proxy/連鎖fixture・通常一巡後に判断。未測定、公開/新6要素追加はしない。編集modeは元物理経路fallbackで、この候補の証拠にしない。

### CalmR4 実検証の結果（2026-11-01）
担当リリー、エンジンde187649f1e51d551595218476cfb815a9ba2008。WebGL build0errors/19warnings。同環境R3 control CPUmean4.746/max204.5ms、rAFmax216.7ms→R4mean3.432/max42.6ms、rAFmax50.0ms。同発射body byte一致、距離170.417→164.069m、破壊26→28・実二次6→9。床substep精度不変、minHeight-9.182→-4.638mmはこの1入力のみ。static proxy2318/mismatch0、別scene実衝突による二次破壊とreset0 PASS。通常入力2巡157.158/214.640m、faultempty/browser0/defaultshakefalse/offset0/effectzoom1。第1投に外部visibilitychangeで安全pause→通常resumeを記録、隠蔽しない。原動画はraw保存、冒頭約5秒の再encodeをdemoに限定（性能/M14資料ではない）。
診断10投は7投完了。第6投rAF83.4msが初投50.1msを超過しFAIL、対応Tick5.7ms/全投内max30.6msでC#Tick外の原因は未特定。第8投visibilityhidden・pause時sim4.579sで180秒timeout、残3投未完。rawを保持し、合格・物理停止・ゲーム処理落ちと混同しない。新しい同条件の盲目再試行なし。次は非表示にならない隔離測定枠とbrowser/native traceから待ち原因を特定する。40投/M14/実iPhone/実聴/本人手触りは未確認。R4は非公開比較候補、正式採用/公開なし、6要素追加は保留で保持。

### 83.4ms残差の限定確認と20MB配布（2026-11-01）
担当リリー、本人最新「起きやすさ×困る度、低いものはメモ」に従う。通常2巡は再利用し、新たな40投/M14は行わない。旧第6投83.4ms/pairedTick5.7msは削除せず、別ブラウザ前面維持6投traceでV8GC最大4.508ms・Wasm301989888byte全投一定、次の記録機能なし6投は非表示0/エラー0・全投164.068710m/28実破壊/9二次/41破片、平均/最大フレーム間隔が各投とも初投以下。元83.4msの厳密原因は断定せず、再現0の単発残差としてメモし、推測の追加物理変更はしない。これは旧10投の合格化や全端末保証ではない。
配布は元Wasm/data/frameworkと展開後SHAが一致するBrotli+標準ZIP、18,277,802byte。Mac ditto86file全byte一致、解凍実物のlocalhost圧縮ヘッダー/起動/投球/結果/再投を1巡だけ確認する（配布変更の必要smoke）。ゲーム版はCalmR4のままで、検査のみを新実装版と呼ばない。ソース・ログを分割保存して各SHAとLibrary全量readbackを返す。公開R2b変更/push/新認証/課金なし。

限定確認の終端：記録機能なし6投は全て非表示0、平均/最大の初投超過0、物理異常0、距離/破壊/破片一致。元の83.4msは再現せず原因未断定のメモ扱い。compact実行ZIP18,277,802bytesはMac標準解凍86file一致、展開後Wasm/data/frameworkのSHAが元playerと一致。compact-smoke01は起動→結果164.068710m→再投成功だが取消Fetch1件で元スクリプトの総合FAILを保持。負荷のないload-only診断はerrors0、本体3資産200/正しいbr・MIME。URL未記録の最初の取消を特定済みにせず、再現しない軽微残差として保存し、同じ試験の再反復はしない。修正候補提出可（非公開・本人採用/作品全体完成とは別）。


### AreasR5 追加6要素再開（2026-11-01）
担当リリー、入力CalmR4 5753eea2。第1差分=3採用売場を28–62/62–100/100–135mへ配置、既存接触対象の置換で大型化、同視点実画像/実走一巡/同版ZIP。原物理solver/別scene破片budget/揺れOFF/保存維持。今回=エリア/大きさ進行中、形と材質を伴う前景造形進行中、密度強弱と破片爽快さは実物確認後、人鳥接触は未着手。旧6要素保留は親の最新再開で解除。TEAM所在不明は記録し独立制作継続。公開禁止。

R5初実行: 同seed/body一致、CalmR4 164.068710m/28破壊/9二次からR5 155.442764m/23破壊/6二次、C#平均3.179→2.889msだが最初の新売場表示x19.4mでrAF883.3ms/pairedTick6.9ms。軽さを保持したとは判定せず、専用新シェーダーを使わず既存R2Surfaceと専用パステルatlasへ変更したR5bを比較中。削除した専用shaderはevidenceへ保管。通常操作fixture01はページ初期化前view参照で0投のままFAIL、typeofガード修正したfixture02へ。旧記録の書換なし。
親からの次候補：既存City2は並進共通/初期回転0。次の破壊表現工程で命中点/法線別の片速度・回転を並進+回転の既存energyBudget内で配分し、監査も合計へ。破片6/City20/総48/装飾12・別scene・揺れOFFを維持。現R5bには未適用、Burrito/Unityの調査だけで改善扱いしない。

### AreasR5c完了とLife/ScatterR6開始（2026-11-01）
担当リリー。R5cの3エリア/大型前景は私的比較実物として納品/同版読戻し済み。code9ffa91da、残初表示66.6msと見た目仕上げを維持。次=既存6役の人鳥をactual contact targetsへ接続、1接触→1反応/得点→再投復帰。旧FarWorldLifeの同じ個体を置換し無関係な70体追加にしない。次に同破片数で命中点/法線別速度/回転、実inertiaを含むenergyBudget監査。既存Count/energy/連鎖・保存・揺れOFF保持。代表操作/一巡と同カメラ実映像、実機/M14/全ゲート合格とは区別。次版のみprivate保存、R5c/公開不変。

R6実装: 元22人+12鳥を近景実接触へ置換(総人数22/総鳥48維持)。本番接触閾値/1target1event/得点1回/whole-body吹き飛び、口/手足/羽反応、再投resetへ接続。City2破片は同shape/mass/countで命中点別方向と回転、実inertiaの並進+回転予算監査。Native1巡179.030762m/実target36人1鳥1/energyExcess0/fragment最大41・city20/resetPASS。NativeとWebGLは区別、最終C1skinnedhook後の本番比較/映像/通常ループを次に実施。

R6 final scoped WebGL: measure02 title→same-input→result→retry PASS, 193.084457m/33actual events/11secondary/people1/birds2. Actual City2 scatter28events/21with rotational energy/max excess0; caps6event/41global/20city observed; reset counts/fragments0. Ordinary Enter charge/release2 loops PASS (167.782303m/38events,132.745895m/28events), second presetOFF+reduced no admittedflash34→34, both shakefalse/offset0, browser0errors. Browser restart save-readback PASS. Same-input R5c video and R6 video captured, initialBody identical. Packaging fault measure01 default Unity index→exact source wrapper repair preserved, no gameplay alteration/rebuild.

Performance is NOT passed: R6 one-case Tickmean4.2741ms/max34ms, rAFmean22.1029/max616.6ms; old R5c 2.7117/31.7 and16.7937/66.6, under different external MUSE/WindowServer activity and changed contact route. Neither recording nor a repeated chosen run proves improvement. Old strict10FAIL/floor/M14/physical iPhone/actual listening/owner taste unchanged. No new software/auth/push/publication. Remaining six-feature work: materials/gray-background repetition/density/readability and polish remain open; this R6 stage adds real NPC/bird reactions and energy-bounded individual scatter, not total completion.


### GiftStoreR7：雑貨売場の質感と視覚密度（既存6要素の続き）
stream=visuals / task=代表雑貨28–62mを棚3variant+商品比率+木/布/陶器/石で実物化 / owner=existing runtime Lily / depends_on=保存R6・性能未解決注意 / acceptance_check=同視点before/after、接触/launch/budget不変、代表一巡・再投・保存、同版ZIP/source/readback / deadline=未指定 / status=IMPLEMENTING。3エリア/人鳥/飛散/揺れOFF/セーブを保持、新規body0、既存Shader/mesh author利用、非公開。

R7 Native最終06/07:全Collider2183/hash一致、body1/contact349/launchbody一致、新prefabphysics0、同camera全景/雑貨↔菓子境界/菓子未変更を実描画。棚3variant/連続tube/色材質map/石床壁まで実装。静止実物を目視、次は同版WebGL代表一巡・保存と配送。性能PASSではない。

R7 WebGL終端:最終build02 0error/4warning、provisionalbuild01とWASM/framework全SHA一致、.data内識別をR7へ修理。通常1投156.761093m/26破壊/fault空/再投PASS/保存再起動PASS、runtime revision/productVersion=R7、sourceIdentity=4e7f91c/bf539d（path-string ordinal）。同WASM代表193.084457m/33破壊/11二次/人1鳥2、browser0errors。R7単発rAF最大50msは性能合格や旧616解消とはしない。前面UI検査録画終了/MUSEへ返却、残る配送は非表示load-onlyとbyte readback。


### FinishR8：現行6要素のまとまりと性能診断（2026-11-01）
stream=finish / task=端から端まで遊べる現行範囲の完成候補 / owner=Lily single writer / depends_on=R7 preserved dbd557f / acceptance_check=同期stamp付き代表投球で原因と限界、3売場/大カニ/人鳥/破片/揺れOFF/結果再投保存の実画面、重要未達だけ修正し同版成果物 / deadline=未指定 / status=IN_PROGRESS。
親の①〜③完成判断案は整理案で本人新仕様ではない。新機能/全面作直し/公開なし。最初はloopback opt-inのUpdate/LateUpdate/Report入口出口とrAFを同一performance.now時計へ記録、固定typed buffer・書出しは結果後。診断負荷は合否測定と分離、再現しなくても解消としない。R7/baseは不変。


本人最新修正16時台: 屋内3区画だけで完成としない。核は外へ出て、進むほど規模が大きくなること。既存WorldRouteR11の9macro区間0–3220mと実RouteR12/29を正本実装として照合し、後半仮配置は完了扱いにしない。親の具体的な街/車/建物順の案は未採用、背景全置換も行わない。性能同期診断を継続しながら、屋内→街の移行/後半の固有大型対象と次への連鎖を実物で確認し、重要未達へ可逆差分。

R8重要未達（Native9区間実見）: 後半の破壊物が手前のカニ/背景に対して小さく、規模拡大が背景だけに見える。既存の街344m車両/港750.8m荷役設備/高架1058m屋根/広域1605m貯水設備/発射2110m設備/軌道2740mパネルを各区間の大型接触ランドマークへ。対象ID/個数/得点/有限回収/破片上限不変、表示とColliderは同じplacement scale。新しい地域順の発明ではなく既存物の段階的規模を実物化。出生重なり/連鎖/局所接触/再投を確認してから候補へ、後半placeholderを無条件完了にしない。

R8同版一巡: keyboard/pointer2投→結果再投/距離固定PASS。追加の獲得済204coinから30/50/70でLv3を正規購入、通常swipeで1303.827m/30破壊/人4鳥3・7macro区間。着地push1投1415.174m/40破壊/8二次/人5鳥4/PERFECT1/miss1/結果再投PASS。全fault空、強化/保存の水増し0。実画像で縦結果のroot viewportX=.8で大型蟹右側欠けを確認、portraitのみ前の破壊への横bias0へ局所修理。既定揺れOFF/物理/飛行カメラ不変。新投のarchive revisionをApplication.versionへ結び旧r32m固定を解消、過去archive値は書換えない。最終UI修正版の縦結果/保存readbackを次の限定確認へ。

R8確認用完成候補の最終確認: engine6bdd00dではportrait結果を中央へ（先行243m例X=.8→最終例約.5、全身実画像確認）。最終縦一巡1808.314m/46実破壊/10二次/人6鳥3/fault空/targetPower100=actual100/新archive版R8/結果→再投、off+reduced境界cameraOffset0/active0、別再起動で全6投とbest1808.314・Lv3・coins346の保存復帰PASS。縦第1試験は画面外に出るdesktop用driverのCancelAimで未発射、失敗保存・ゲーム変更せずviewport対応へ。第2試験の充填Enter操作中に正規発射済みなので縦pointer単独PASSとは呼ばない。横pointer/購入/landingPushは71d7db7の実結果で、最終の物理/入力/飛行カメラ/強化コードbyte一致、全確認を最終全PASSにまとめない。9区間撮影とlocal6接触・画像前後を保持。Playwright導入版はlocal不在でno-install照会DNS失敗（追加install0）、game-studioローカル欠落のため公式公開game-playtest本文を回収して既存CDP自動操作/実画像の同手順を適用。独立レビュー/作者採用/光学M14/実iPhone/実聴/旧616解消/正式公開は未確認。原案の屋内→屋外→拡大を3売場へ縮小せず、後半仮配置/素材反復は残件を明記して確認用候補を非公開配送。


### LateWorldR9：後半仮配置の景観仕上げ
stream=world / task=既存country-launch-orbitを固有の段丘/農業設備・発射組立/格納庫・地球縁/軌道構造へ / owner=Lily / depends_on=R8保護0b5d3265 / acceptance_check=同camera前後実画像、既存Collider/launch/placement不変、通常プレイ/結果/再投/保存、同版配送 / deadline=未指定 / status=IN_PROGRESS。既存全ルート/3売場/大カニ/接触/飛散/shakeOFFを保護。renderer-only・単一palette材質/6meshへbatch。後半3仮配置フラグを検査のためfalseへ書換えない。同期616原因不足のため物理最適化は混ぜない。

R9 Native最終: separate-process Before03/After04 4同camera実画像、Collider2183/targets349/body1/launch-body/hash全一致。構築材質submesh780→758、全active triangles834216→836612。FPS改善とは未測定。初回同一Editor複数sceneの既存mesh-cache参照FAIL、旧画像の床端/投影FAILを保持し、比較fixtureとrendererだけ修正。後半3placeholderフラグは保持。次=同版WebGL通常一巡/保存/私的配送。

R9確認用候補: 後半renderer差分実装/4同camera Native接触hash維持、R9 source版表示をApplication.versionへ同期、横resultの蟹をviewport.38へ局所修理（縦.5・飛行不変）。通常3投919.640m/1105.260m(push)/1443.273m、1443でcountry到達・35破壊/人6鳥3。先行2投は充填中録画でspin/位相が異なるためR8の同条件比較としない。最終通常 1427.10595703125m/実破壊45、desktop/portrait構図/結果/再投/browser0/fault空、保存再起動の全10投ID/距離/版一致。旧616/実iPhone/M14/実聴・作者採用は未確認、後半の自然到達全区間は未実証。公開/GitHubpush0。status=PRIVATE_CANDIDATE_DELIVERABLE_PENDING_LIBRARY（全体正式完成ではない）。


### R10 通常進行の局所仕上げ（2026-11-01）
| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| 通し版 | 実保存Lv3から通常UI/キーで屋内→屋外→country→launch/orbitへ自然到達、結果と再投 | リリー | R9実物/PLAN N03,N04,N05 | replay/teleportを到達証拠にせず実launch receipt・routeSamples・画像を結ぶ | 現工程 | 作業中 |
対象数を増やさず後半3m蓄電対象をcountry2.5/launch3.5/orbit4.5倍へ段階拡大。描画/Colliderに同じplacementを使用。1対象9000J/有限資源/速度cap/摩擦/床精度/揺れOFFを維持。R9通常2条件1854.756/2006.604mを修正前根拠に保持、任意の入力で全区間到達の保証や全ゲーム完成はしない。既存616.6ms/M14/実iPhone/実聴は未解消のまま、新公開0。

R10代表確認完了: 通常UI装備1/Lv3/40°/勢い100/Enter100ms(実70回)、spin27.6933/位相103.4336、実発射→2838.137m/51実破壊/13二次/人6鳥4/14回収、launch2050/orbit2680自然進入、結果→再投/shakeOFF/browser0。再起動全13記録ID/距離/版一致。これはN03/N04/N05の代表到達根拠で旧正式gate合格や全体完成ではない。末端3220・高飛行時の後半景観の画面収まり/616.6ms/M14/実iPhone/実聴/作者採用は未確認または未解消。R9と入力パラメータ/回転は一致するが位相は異なるため厳密同初期状態の距離改善としない。公開/push0。

R11既存完成範囲: R10をprivate backup/r10-20261101-8a98f549/remote復元一致で保護。Launch直後はsimulation.Vxが未更新0のままUpdateCameraを呼ぶため、steadyFlightSpeedR3を0固定→意図した引き/先読みが効かない原因を修理。剛体の実linearVelocity.xから初回だけ読む、連続hitごとのzoom変動は戻さない。物理/対象/回収量/世界/機能は不変。通常20°+既存3回pushで終端3220への進行と後半ランドマーク近傍の実画面、同時計SyncProbeの代表区間を確認。既存616.6msの未再現は解消とはしない。実iPhone/実聴は未確認、低影響floorはmemoのみ。

R11通常2条件は20°1911.833m/40°1441.150mで終端未達、結果再投/fault空/browser0を保持。回転/位相差があるのでcamera修理の物理回帰とは断定しない。カメラ最大17.1569でzero-cache固定解消。既存Sync診断は完了、記録・不合格を保持し性能全解決としない。次は残回転があるのに旧automatic ground bounce上限3に達したかを有限resource台帳/接触ログで局所診断する。値だけ先に変えず、上限が実際の阻害なら既存route内の制限調整候補へ。


R12 finish current scope: owner=Lily, depends=R10 private backup + R11 preserved; status=IN_PROGRESS. Nine existing sectors and endpoint3220 only, no world/function additions. R11 camera cached-speed bug repaired. Same accepted Native20deg launch shows two eligible landings blocked solely by legacy3-auto-bounce cap while7867J remained; candidate finite allowance9*3=27 keeps identical pulse cost/cooldown/guards and manual3 kicks. Existing route65s guard fired at3034m while still moving66.8m/s, so align it with existing spec M6 300s (short world30 retained). After02 Native ends3220.324m/67.803s,13auto,56breaks,remaining45723.91J; not normal WebGL proof. Support array allocation removed with identical arithmetic/substep/query count, bit checks+same Native trajectory pending. Web-only loaded-shader preparation runs before runtimeReady, startup cost is measured; no FPS/quality/physics precision reduction. Same version ordinary end route/camera/result/retry/save and synchronized windowed diagnosis remain required. R10 original backed up; R11 normal endpoint FAILs/old616.6/M14/iPhone/listening unconfirmed retained. No public deploy.

R12 warm-all startup rejected before title: prolonged wait plus GPU sampler errors, zero throws. Use only scene renderer material keywords/Normal+ForwardBase+ForwardAdd valid variants, count/time before-ready telemetry. No hidden/all-loaded permutations. Preserve failed functional01/build01 and first raw player. Physics unchanged.


R12 same-version representative checks complete, private confirmation candidate only (not formal all-gates/performance PASS). Build02 engine6ebafb42c95a5deece9bc1b4430b8d30044f825f/C#e8a6b703f4fd765317880675d805884ff36c790242977ce15bf0922799c6448a. Actual ordinary UI/keys: earnedLv3/gear1,20deg/power100,57Enter presses at100ms,spin24.823406/phase42.586487,manualSpace3 ->3220.231689m/86.502411s/55breaks/13secondary/people4birds3/14unique recharges/auto17of27/remaining64750.71094J, faultempty/browser0/result/retry. Nine regions in actual result; launch2100/orbit2740 screenshots viewed; actual wider flight maximum18.825798. No replay/seed/body/progress fabrication for this throw. Native source replay and unrecorded seed42043 timing are separately labelled. Save restart16archives exact, best3220.231689/Lv3 persisted; off+reduced boundary/defaultshake/audioOFF checked. Portrait result root central but a lower leg overlaps the route strip; minor UI note, no new scope.
Performance unresolved: fresh windowed same accepted input/seed42043 without recorder ->3220.006m/faultempty/browser0.10344 sync stamps/overflow0. maxcallback330.4ms versus prior369; corresponding Update7.7ms with GetShaderiv158.363+56.703 and presentation wait78.106ms. One max metric decrease is not PASS; common0-25s render batching differs, GPU waits persist, background WindowServer75% in snapshot, not blamed as cause. Actual scene21variant startup340ms ordinary/352ms diagnostic; all-loaded warmup rejected and preserved. Exact Box allocation removal has128bit checks and identical Native physical result/substeps, no floor precision reduction. Old616.6/strict10/M14/iPhone/listening/owner adoption remain unresolved/unconfirmed. Current task uses representative scope per latest parent, no new40/M14 gate. Low floor intrusion2.636mm memo only. Public pages/originals unchanged.

| 通し版 | R13 shader-query stall cause→minimal startup repair→same-input prefix/full-loop/save verification | Lily | R12 9cc6dd05 protected | Same clocks, actual program identity, no quality/physics reduction; artifacts and readback | current | 実装・Native一致・WebGLビルド済み。同条件10投の判定は同版evidenceを参照 |

R13 shader-only repair 2026-11-01T12:12:34.435667+00:00: same accepted-body initial6s probe R12 shader-cause01 identified GLSL83/86/89/92 as R2Surface/BoutiqueGlass/BoutiqueSurface/BrokenMetalVisibility; each fragment adds only unity_SHAr/SHAg/SHAb vs startup53/41/62/68. UnityCG.cginc:20 official installed source gates these byLIGHTPROBE_SH; current warmupForwardBase hasDIRECTIONAL but omits it. Add only ForwardBase+DIRECTIONAL+LIGHTPROBE_SH variant per scene material, preserve original variants. All other214 C# byte exact except builder; physics/quality/feature0change. Initial6s probe no whole-loop PASS. Slow string difflib cancelled exit130, replaced by bounded uniform set comparison; no GPU load/process left. NativeCompile/build1 planned, same wrappers+accepted launch compare; then normal loop/save/delivery.


R13 shader stall repair confirmed (private confirmation candidate OK, Lily; not whole performance/all-gates PASS). Runtime only StartupShaderPreparationR12 adds ForwardBase+DIRECTIONAL+LIGHTPROBE_SH; all other214 C# except builder byte exact toR12, all original shader/material/model assets unchanged. Actual R12 first6s programs84/87/90/93 mapped by exact vertex+fragment GLSL SHA toR13 startup75/60/87/96; flight program creation4→0/compileShader calls8→0, startup27variants675ms. Full original accepted body/seed42043 same asR12, recorderOFF/1280x800DPR1 ChromeMetal: GetShaderiv8→0; mean callback40.1508→17.8896ms/p9594.6→23.2/max330.4→158.7.20528sync stamps/overflow0/4366Update spans; detailedCPU4096 cap misses onlyframes4187–4456(270), no invented detail; entireclock coversrun. Remaining153.7ms Update/102.6Physics/support31ms atsim41.3426 isseparate diagnostic CPU spike, unchanged floor fidelity and profiler cost, not solved; oldunsync616.6 remainsseparate.
Normal earnedLv3/gear1/20deg/power100 actual62Enter100ms/spin25.885662/phase206.225189/manualSpace3 ->3220.077637m/74.313721s/58actualbreaks/13secondary/people5birds3/15unique recharges/auto12of27/remaining73626.39J, faultempty/browser0/result/retry. Save restart17archives +best/Lv3/coins1424/gear1 exact. Shake/audio defaultsOFF, off+reduced boundary checked; imageslaunch2100/orbit2741/result viewed. Video originalVFR notFPS/M14/audio proof. No world/function/physics change, minorfloor-1.798mm memo only. PhysicaliPhone/listening/M14/owner adoption/officialPlaywright unavailable remainunconfirmed. Source closure will bind finalgeneratedscene/identity/R13version; publicpages unchanged.
Recovery: exec transport CreateProcesserror nonetheless hadstarted97650/97702/97703, all absentandnotitle/result atreturn; partial attempt preserved, same source rerun onlyunfinished diagnostic; not performance selection.

| 通し版 | R14 fixed collider metadata cache; exact support and same-input performance comparison | Lily | R13 retained | Native1,196,247points exact; accepted replay endpoint + normal UI/result/retry/save18records; not whole-performance PASS | current | 実装・確認済み、非公開成果物保存中 |


| 通し版 | R15 Prepare局所診断→支配箇所修理→正本同条件10投 | Lily | preserved R14 + full PF10 FAIL | no physics/quality/chain/input/window changes; actual subspan proof then same10 | current | 実装・Native一致・WebGLビルド済み。同条件10投の判定は同版evidenceを参照 |

| 通し版 | R17元のfresh-world初期化を保持した支配負荷の局所修理 | Lily | R15b80e9a8/R16未採用 | 連続2投の初期状態・接触順序・軌道全文一致、同画質/物理精度/連鎖、実改善後だけ正本10投 | current | 局所診断着手、R16方式は不採用 |

R17 Native連続2投は初期/接触/破壊/回収/ticks/終了/34actorMeshの全文一致。34描画meshのexact-matrix再利用だけ。毎投新規PhysicsSceneを保持、R16コード不採用。WebGL改善と同版正本10投は未確認、全合格や公開とはしない。

R17 Web同記録発射/seed42043比較: launchBody・最終physical値一致3220.006348m、life27.3999→5.1ms、Prepare138.1→119.0ms/両run errors0。これは局所改善でG-PERF10合格ではない。接触構築56.3ms未解消。同版正本10投を次に測定。

| 通し版 | R18 fresh-world準備のframe分割（実験） | Lily | R17保護2195b5bc | 原版連続2投全文一致。元のshape/Scene/順序・画質・資源維持。待ち時間を全てG-PERF10窓へ含め、初投遅延/窓縮小なし。総CPU時間短縮は保証せずready壁時間も記録 | current | 隔離実装開始、未採用 |

R18 Native分割経路の元版2投全文一致、入力ACK/設定順/同条件再投/Pause/古い充填消去を確認。Native分割数19はEditor内同期駆動のunit数で、実描画frame/FPSではない。WebGL同条件比較・ready壁時間・正本10投は未確認。3msは作業分配目安で受入閾値ではなく、正本G-PERF10は無変更。

R18追加境界修理: 準備中PauseでRetryLaunchが成功ACKなのに未発射になる枝を、理由付き不受理/Ready/充填消去へ修正。NativePausedRetry PASS。build01は同一内容の生成DLL重複mscorlibでFAIL、own Libraryを可逆退避し再生成後*2.dll0。旧FAILログ/生成物保持、build02へ。

R18 Web同一記録発射/seed42043：発射・最終物理値はR17一致。再投準備wall867.3ms/最大RAF383.2msの負例を保持。1回の同期traceではwall481ms/最大RAF17.7ms、原因未確定・解消扱いなし。新runtime修正は根拠不足で行わず、初回正本10投で同版を判定する。閾値・測定窓・初投の処理は不変。


| 診断 | R18 prepare clock attribution repair (private, 2026-11-04) | Lily | source7065e442 / engine571e1e1a retained | Opt-in active/wait partitions + first-work delay; exact Native2 physical streams and clock negative control. No gameplay/performance acceptance claim. | current | CPU8PASS / Native2exact; WebGL383.2 unresolved / strict10 NOT_RUN |

既存 wall 計測の意味を保持し、yield間待機をactiveWallMs/waitMsに分離。旧365.5ms接触区間をCPU占有と断定しない。原ログ867.3−507.6=359.7ms残差は最初のiterator開始前+最後の末尾であり、全383.2msの原因未確定。Native値をWebGLの改善根拠としない。正式checker/10投/0許容差/全窓/通常Pause/first-delayなしは不変。今回WebGLbuild、画面試走、公開、外部送信、Linear更新なし。

R18 timing continuation 2026-11-04: 私的な診断WebGL c86cec4をビルド成功。ECC new_page(background:true,isolatedContext)+initScript/evaluate_scriptで原本571と診断版を各1投(保存済み合法入力replay、40度/gear0/target100)→結果→通常再投。発射receipt全文・最終physical値・390x844/DPR1/AMD5500M・shakeOFF一致。生RAF2343/2332、元worst34.01/診断32.925ms、候補firstWorkDelay3.105ms/active296.7201/wait826.705ms取得。旧383.2/68.261s条件と異なる短い13.437s条件で、未再現を解消としない。新しいゲーム最適化・正式10投・公開はなし。前面専有は使わず、sharedCPU/GPU非競合と正本連続10投の適合は未証明。生成Scene/SourceStamp/ProjectSettingsをこのprivate診断版の復元用snapshotとして保持。

| diagnosis | 本体/破片Scene Simulate内訳を既存総ticksと同期保存 | Lily | R18 source83 | 物理順序/dt保護・ticks和一致・全窓欠落0・原版結果一致、正式10投とは別 | 2026-11-04 JST | 診断取得完了・性能未合格 |

| diagnosis | 表示Primitive破棄待ちColliderのみの寿命/Scene/接触を追跡 | Lily | source5ee6 | 物理不変・対象だけの短試験・低負荷取得/保存、同情報の強入力再試行なし | 2026-11-04 JST | 診断確認済み／表示候補の本体参加0・性能未解消 |

| diagnosis | Prepare受信/State/Report/receipt/queuedと初回MoveNextの不足同期spanのみ追加 | Lily | SAK-16/c8e6f80 | 固定buffer/非yield区間/既存unit再利用/原版保持/静的・純CPUのみ、Unityと実測は未実施 | current | 診断差分作成・純CPU記録器11PASS/observer3PASS、実Unity/実測NOT_RUN |

| SAK-16 item1 | mall intact重複Part生成除去の検証 | Lily direct exec | 156f9195 / plan539a4ebc | actual50:456->284GO/406->234Renderer; mesh/material/transform/collider exact; WebGL and fixed input compare | 未指定 | 2026-11-12: runtime変更維持、Editor・WebGL成功、候補3220.00634765625m/54破壊・通常Retry取得、基底を保存bundleから独立再build中。2/3/4未変更 |


## モール増量の隔離試作 2026-11-13
| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| density-proto | 端から端まで遊べる通し版を維持し、右モール107mだけ増量 | Codex | 本人直接依頼・隔離コピー | C# error0・WebGL exit0・3視点・既存固定入力1回 | 指定なし | ローカル提出：C# error0・WebGL exit0・3視点・指定入力・ブラウザ再投確認。採用/公開は未実施 |


## 2026-11-13 isolated mall density prototype — local submission

- Version: kani-density-proto-20261113. Right mall x28–135m only. Added 13 small, 5 medium, 1 giant target and 20 static decor groups. Existing mall targets 41→60. Density interpreted as additions; original placement retained.
- Unity C# errors 0; WebGL Succeeded, exit 0. Native fixed-input: 2680.2878m / 46 breaks / no physics fault, giant counted. Browser normal input: 52.4799m / 27 breaks, 5 added objects broken including secondary chains; retry ready.
- Original 1930 files unchanged. Physics sources byte-exact. No upload, publication, purchase, Linear or KB write. Full evidence, warnings, exclusions and startup in ../PROTO-REPORT.md.


## モール中層増量 第2イテレーション 2026-11-14
| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| density-proto2 | v1を維持し中層14対象と飾り14組を追加 | Mac Codex（本人直接依頼） | 隔離コピーv1実物 | C# error0／Native候補03・支持形状・同入力1投／WebGL再build・ハッシュ一致 | 指定なし | 停止：author02でUnityライセンス初期化失敗（74.85秒）。素材生成・C#検査未到達。コード／配置JSON保存済み、Native／固定入力／WebGL未実施。詳細は ../PROTO-REPORT2.md。外部投稿なし |

| neon-stage3-fix2 | 背景取りこぼしのruntime特定・暗転・画素検証・再build | Mac Codex | NEON-BRIEF-STAGE3-FIX2 / 既存stage3 | 固定入力3220.0295m/46・74個・全区間画素<.30・新規WebGL・browser2PASS | 指定なし | FIX2完了：Native3220.029541m/46/差0、画素36PASS、WebGL初回成功、Chrome2PASS、HTTP200/配布hash一致。物理/入力/エネルギー不変、外部更新なし |


| neon-stage4 | 弧削除・指定7区間の既存破壊対象増量・Native/WebGL/ブラウザ/報告 | Mac Codex | NEON-BRIEF-STAGE4 / stage3 FIX2 | bands0・火花/グロー・74個維持・完走faultなし・build errors0・browser2PASS・8146一致 | 指定なし | 実装・Native・browser2PASS・8146・報告書済。総合未達：WebGL SucceededだがTypeDB errors6458、復旧1回使用済。再開条件と証跡は ../NEON-REPORT-STAGE4.md |

## NEON 第5段（この隔離コピーのみ）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| NEON stage5 | 小破片整理・後半5区間造形・WebGL-stage5・8149配信 | Codex | NEON-BRIEF-STAGE5.md / stage46 | Native現行差0、732対象、物理部品0、WebGL errors0、browser2pass、旧版SHA不変 | 指定なし | 第5段指定ゲート全合格。Native現行差0・732対象・物理部品0、WebGL errors0、browser02 2passed/0failed、8149 HTTP200、旧589ファイルSHA・元8876エントリ不変。初回ブラウザ中断と原因／復旧はNEON-REPORT-STAGE5.md参照 |

| neon-stage6 | 破壊10種類追加・逓減密度・通し版維持 | Mac Codex | NEON-BRIEF-STAGE6 / 第5段 | Native登録・実破壊・signature / 固定入力2500m以上 / WebGL / PCスマホFPS / 8150配信 | 指定なし | ローカル提出完了：10種実破壊・1481登録・逓減密度・2798.21m/result/fault空・WebGL errors0・2passed・FPS PC60.01/30.00 mobile59.93/20.00・8150 HTTP/SHA一致。公開/Linear/VMコピーは親 |

## NEON 第6.5段 差し替え・実画面チューニング

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| NEON stage65 tune | ?tune=1 の19スライダー、実送信、JSON書出し、WebGLと実画確認 | Codex（今回の直接依頼） | 旧feeltest中止・原値復元の実測一致 | native差0、build errors0、通常タッチ結果、粒子40→96、書出し6、物理反映、Revision復元 | 指定なし | 工程実行・報告書作成終了、合格留保。Native初回差0、修正版build02 errors0、通常A PASS、物理60実投/再投確認、B粒子全件96 assert FAIL（既存上限512）。Revision stage6復元・配布tune維持。NEON-REPORT-STAGE65-TUNE.md参照 |

## NEON 配置修正（2026-11-22）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| placement-fix | 監査準拠の支持面スナップ・回廊・孤立・orbit看板修正 | Codex（今回の直接依頼、単独書込） | ../PLACEMENT-AUDIT.md全文、ui-simplify d288db2 clean | Unity外再生成・同条件前後集計・浮遊0/孤立0/密度上限・保護差分確認・commit。Unity/固定入力/実画面はクロエ別工程 | 指定なし | ソース修正・再生成・静的検査完了。支持なし0、孤立0、回廊302/1059。Python9PASS、Playwright2PASS、C#型検査error0/warning17。2144既存ファイル/既存stage65の85ファイル不変。Unity/固定入力/新実画面NOT_RUN、ゲーム合格は留保。報告は ../NEON-REPORT-PLACEMENTFIX-by-codex.md |

## Blenderモデル差し替えパイロット（2026-11-23 本人直接指示）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| blender-pilot | モール x=28〜135 の22家族74個をBlender製へ差替え | Codex（単独書込） | placement-fix 5b3e627、判別監査・世界brief | 全家族blend/FBX/コンタクトシート、Native照準/飛行、seed42043 accepted-inputで2500m/96以上、従来物理署名一致、配置/他区画/UI不変、Playwright、commit | 指定なし | 22家族74配置の実装・Native確認済み。3220.0815m/145粉砕、基準と軌跡/破壊イベント/1407物理署名一致、他1333描画署名一致。Playwright4PASS/0FAIL。既存FlightField.unityを保護。通常ブラウザ操作・game-playtestはNOT_RUN（現catalog未接続）、ゲーム全体の完成判定なし |

## Blender全区間展開（2026-11-23 本人直接指示）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| blender-rollout | 全区画・両方向の有効家族をBlenderへ展開 | Codex（単独書込） | blender-pilot f3911b0・現行実体棚卸し | 区画別未モデル0、同入力route/events完全一致、全City物理署名一致、blend/FBX/画像、Playwright、報告とcommit | 指定なし | City実効41家族1407件（新規19家族）を9区画・両方向へ実装し区分別未モデル0。seed42043は3220.08154296875m/145粉砕/3668ticks、pilotとのroute/events全文・1407物理署名一致。Playwright6PASS/0FAIL。19blend/FBX・9区画コンタクトシート・Native18区分を保存。別系統40+34件は未変更、通常操作/game-playtestはNOT_RUN。既存Scene差分保護。実装commit 825a34f、報告は ../NEON-REPORT-BLENDERROLLOUT-by-codex.md と docs/blender-rollout に保存 |

## Blender別系統パネル（2026-11-23 本人直接指示）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| blender-panels | Restaurant/Core/Overdriveの実体を棚卸しし全破壊対象の外観をBlender化 | Codex（単独書込） | blender-rollout 8fb3d25、現行実体 | 同入力route/events完全一致、全City物理署名一致、系統別未モデル0、Native画像、Playwright、報告・commit | 指定なし | 実装・保存済み、Native必須検証BLOCKED（Unity license、起動exit198）。ソース20+16+4=40、19無傷+6破片モデル、25原稿読戻しPASS、C# errors0、Playwright2PASS/0FAIL/6SKIP。Native登録/物理署名/同入力/画像/残数0は未確認。Scene既存差分保護。正規ライセンス復旧後README-panels手順で再開 |

## 港区画プロトタイプ（2026-11-23 本人直接指示）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| port-prototype | +x港の8ビート再配置・役・C1〜C8カメラ・視覚v2 | Codex（単独書込） | blender-panels 539acb1、briefs全5点 | 全登録数/他区画/Scene保護、接地・密度・高度・量子化監査、C#型検査、UnityなしPlaywright、報告・commit | 指定なし | 実装・オフライン検証済み。港145→145/世界1481、接地0、CV0.565054、顔39.3103%/相方20%/高度24.8276%、他区画・−x配置/Scene不変。C# errors0/warnings18、純粋計算30PASS、Playwright9PASS/0FAIL/4SKIP。Unity依存の映像/固定入力/WebGL/game-playtestは本人指示でライセンス待ち。実装commit 6609e17、報告はdocs/port-prototypeと親フォルダ。作品合格/採用/公開ではない |


## Aquarium water overlay fix（2026-11-23 本人直接指示）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| port-waterfix | 発射後に水槽オーバーレイが追従する1件のみ修正 | Codex（単独書込） | port-prototype 5be0ca4 / WATERFIX依頼 | ready/charge限定・Sample欠落時もLateUpdateで非表示・描画のみ・C# compile・報告2部と同branch commit | 指定なし | 描画3ファイル修正・型検査121ソースerror0/warning18・Playwrightオフライン2PASS/0FAIL。保護364ソース/シェーダー/JSON・既存Scene/meta26件不変。Native/固定入力/実画面はNOT_RUN。実装6286e32・報告2部を保存、同branchへcommit。報告docs/port-prototype/NEON-REPORT-WATERFIX-by-codex.md参照 |

| GRAMMAR-OPENING | 原方式の生簀・店・商業施設へ密度波・境界目印・倒壊目玉を配置 | Codex（単独書込） | port-prototype 2d0d849 / 本人確定の会議第一手 | 3区間CV>=0.4、総数±15%、floating/isolated=0、静止契約・型検査・既存契約PASS、他区間/物理/入力/カメラ/26保護対象不変、日本語レポートcommit | 時刻指定なし | ソース実装・配置監査・型検査完了。Playwrightオフライン4PASS/0FAIL、Native/buildは本人指定でクロエ別工程 |


## JUICE-LAYER（2026-11-25 本人直接指示）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| JUICE-LAYER | 原方式の破壊に爆散・連鎖音程・HUDポップ・山場停止/規模別揺れを重ねる | Codex（単独書込） | grammar-opening 342cd9f、本人の第二手4点・視覚v2 | 配置/物理/PortCamera/26保護対象不変、オフライン型検査・契約試験・日本語レポート・commit。Unityはクロエ別工程 | 時刻指定なし | ソース実装・オフライン検証済み。C#127ソースerrors0/warnings18、実コード契約33PASS、Playwright12PASS/0FAIL/0SKIP、FBX19モデル/40ID色表再現一致。配置/物理/PortCamera/結果構成/元26保護対象不変。Unity/実聴/FPS/正規入力一巡は本人指定でクロエ別工程。指定会議/調査本文はMac未所在・未確認。日本語報告 docs/juice-layer/NEON-REPORT-JUICE-by-codex.md |


## OBJECT-REDESIGN Stage 1（2026-11-27 本人直接指示）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| object-redesign | 端から端まで遊べる通し版を維持して6作用・帯配置・飛行入力・最大連鎖を実装 | Codex（単独書込） | effekseer-proto 68c007f / 今回の確定仕様 | Editor/WebGL型検査0・新契約/既存契約PASS・26SHA一致・commit/報告 | 指定なし | ソース実装・オフライン確認完了：Editor/WebGL errors0、新規85+静的232PASS、bridge45/juice33PASS、Playwright10+既存8PASS、保護26SHA一致。Unity/build/game-playtestは本人指定NOT_RUN。報告docs/object-redesign/NEON-REPORT-OBJECT-by-codex.md |


## OBJECT-REDESIGN ストーリー層（2026-11-27 本人直接指示）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| object-story | 6種のストーリー形・発動キャプションを既存作用へ接続 | Codex（単独書込） | object-redesign 06df915 / 今回の確定表 | Editor/WebGL errors0、既存85+232+45+33、新規形/文言/抑制契約、保護26SHA一致、報告とcommit | 指定なし | 指定オフライン範囲完了：実装8ad7595、Editor/WebGL errors0、既存85+232+45+33PASS、新規形46/保全13PASS、Playwright新16+既存18PASS、26SHA一致。報告docs/object-redesign/NEON-REPORT-STORY-by-codex.md。Unity実画面・ゲーム合格は別工程 |


## OBJECT-REDESIGN blast修正（2026-11-27 本人直接指示）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| object-blastfix | blast経路の停止原因修正と同一対象1回破壊の確認 | Codex（単独書込） | object-redesign 75cb1a1 | Editor/WebGL errors0・Stage 1/ストーリー契約全PASS・新規重複/消費契約・保護26SHA・commit/報告 | 指定なし | 指定オフライン範囲PASS：Editor/WebGL errors0、Stage 1 85+232/bridge45/juice33、story46+13、新規258+静的9、Playwright34、保護main/branch各26SHA一致。修正commit 8631b74。報告docs/object-redesign/NEON-REPORT-BLASTFIX-by-codex.mdへ保存。指定オフライン範囲完了、Nativeはクロエの別工程 |


## OBJECT-REDESIGN カメラ・ヒットストップ（本人 2026-11-27 23:44）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| object-camhit | 通し版を維持しカメラ引きと6作用の描画停止のみ修正 | Codex（単独書込） | object-redesign 6f5bb2e・本人手触り判定 | Editor/WebGL errors0・既存/新規契約PASS・26SHA前後一致・物理不変境界・報告/commit | 指定なし | 指定オフライン範囲PASS：Editor/WebGL errors0、既存契約全PASS、新151+静的10、Playwright35/0fail、main/branch各26SHA一致。物理370ファイル・Tick/State不変。実装commit 75e2623、報告docs/object-redesign/NEON-REPORT-CAMHIT-by-codex.mdへ保存。指定オフライン範囲完了、Native再実測/実画面はNOT_RUN |


## OBJECT-REDESIGN 配置・拡大・顔（本人 2026-11-27 23:46–49）

| stream | task | owner | depends_on | acceptance_check | deadline (JST) | status |
|---|---|---|---|---|---|---|
| object-placecute | 通し版を維持しbrake配置・全6種1.6倍と顔・Native作用ログを修正 | Codex（単独書込） | b8f8bb5・本人確定仕様 | 全16当たり点検・Editor/WebGL errors0・既存/新規契約PASS・26SHA・報告/commit | 指定なし | 指定オフライン範囲完了：実装 bd33c3e、Editor/WebGL errors0・全既存契約PASS・新66+51+22PASS、Playwright36件確認（初回35＋取得復旧後1）、main/branch26SHA一致。全16当たり点検と3個目brake残件を報告docs/object-redesign/NEON-REPORT-PLACECUTE-by-codex.mdへ保存。Native実接触/実ゲーム画面はNOT_RUN |
