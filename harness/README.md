# カニとばし ハーネス（harness/）

リリーが「全部を計画 → 上から順に最後まで作る → ゲートで ◯／✕ → 直す → 全部 ◯ で【提出】」を回すための道具一式。
**ゲートの条件はゆるめない。** テストが間違っていると思ったら【進捗】でマツリに書く（PROCESS.md 手順 5）。

## 入れ方（T01 で 1 回だけ）
1. このフォルダをリポジトリ直下に `harness/` として置く（`harness/scripts/gate.sh` になる形）。
2. `AGENTS_addition.md` の中身を AGENTS.md の最後に足す。
3. リポジトリ直下に `VERSION`（版名 1 行）を作る。ZIP は `dist/<版名>.zip`（別の場所なら `KANI_ZIP=…`）。
4. リポジトリの .gitignore に `qa-out/` `test-results/` `playwright-report/` `dist/` を入れる（入れないと 2 回目から G-GIT が ✕）。いるもの：node、python3、`@playwright/test` と Chromium（今の `qa/` のテストと同じ）、git。
5. （任意）リポジトリ直下の package.json の scripts に `"gate": "bash harness/scripts/gate.sh"` → `npm run gate -- T01`。

## ファイル
| ファイル | 中身 |
|---|---|
| PLAN.md | 計画表（25 項目、順番・依存・状態・JST 時刻）と、M1〜M17・#1〜#17・C1〜C8・①〜⑦ の対応表 |
| PROCESS.md | 1 項目の進め方（計画→GPT Pro 相談→作る→ゲート→直すループ→動画→【提出】）、相談文、提出の形 |
| SUBMISSION_RULES.md | 自動で差し戻しになる決まり R1〜R12・P1 |
| CHECKLIST.md | 動画で見るチェック（C1〜C8、①〜⑦、点滅、3 本比較） |
| TEST_HOOKS.md | ゲームに入れるテスト用フック `window.__kaniTest`・画面の id・feel.json の形 |
| FEEL_DECISIONS.md | 手触りの値の「たたき台 → 決めた値 → 理由」の記録 |
| AGENTS_addition.md | AGENTS.md に足す短い決まり（始めると終わるときの手順） |
| scripts/gate.sh・gate.js | ゲート本体 |
| scripts/plan.js | `next`／`status`／`start`／`submit`／`set`（状態を変える唯一の道） |
| scripts/check_submission.js | 【提出】文を R1〜R12 で確かめる |
| scripts/record_playthrough.js・record_compare.js（record.js） | 通し動画・比較動画 3 本を同じ入力で撮る |
| scripts/lib.js | 共通の部品（PLAN の読み書き、点滅の数え方、リンクの形） |
| tests/flow.spec.js・feel.spec.js・kani-helpers.js | ハーネスのテスト（名前の違いは kani-helpers.js だけで直す） |
| playwright.harness.config.js | ハーネスのテストの設定（ポート 8010。既存テストは 8000 のまま） |
| evidence/links.json | ZIP・動画のリンク（GitHub／Drive）。fps_mobile.example.json は実機 fps の記録の見本 |
| out/ | ゲートの出力（gate_<ID>.txt、submission_<ID>.md、pw.log、suite.log、throws.json…）。コミットしない |

## ゲートのしくみ
- `bash harness/scripts/gate.sh <ID>`：毎回のチェック ＋ その項目のチェック ＋ すでに提出済／合格の項目のチェック（壊れていないか）。`--all` は全部（＝R01）。
- ZIP を `harness/out/build/` に解き、**その中身を**テストする（出した ZIP と試したものが同じになる）。
- 既存テスト（`npx playwright test`、40 投）は ZIP の SHA256 とコミットが同じなら前の結果を使う（1 時間かかるため）。
- 同じチェックが 3 回続けて ✕ だと `STOP-AND-REPORT` が出る（マツリに報告）。
- 終了コード：全部 ◯ で 0、✕ があれば 1。
- 環境変数：`KANI_ZIP`、`KANI_SUITE_CMD`（既定 `npx playwright test`）、`KANI_THROWS_JSON`（既定 `qa-out/throws.json`）、`KANI_HEADED=1`（画面を出して走らせる）。

## チェック一覧
| チェック | 中身 | 対応 | 項目 | どこで見るか |
|---|---|---|---|---|
| G-PLAN | PLAN.md の状態表が正しく、この項目が作業中 | 全部 | 毎回 | gate.js（ファイル・記録を読む） |
| G-GIT | コミット済み（作業中の変更が残っていない） | 全部 | 毎回 | gate.js（ファイル・記録を読む） |
| G-VER | VERSION に版名が 1 つ | M2 | 毎回 | gate.js（ファイル・記録を読む） |
| G-ZIP | ZIP があり、名前に版名、中に index.html | M2・M3 | 毎回 | gate.js（ファイル・記録を読む） |
| G-LINK | ZIP のリンクが GitHub／Drive | M3 | 毎回 | gate.js（ファイル・記録を読む） |
| G-SUITE-RAN | 既存テスト（npx playwright test）が走り 40 投の記録が出た | M1 | T01 | 既存テスト（npx playwright test）と qa-out/throws.json |
| G-SUITE-PASS | 既存テストが全部 pass | M1・M7 | B03・B04 | 既存テスト（npx playwright test）と qa-out/throws.json |
| G-40-LAUNCH | 40 投で未発射・300 秒止まり 0 | M6 | B01 | 既存テスト（npx playwright test）と qa-out/throws.json |
| G-40-STOP | 40 投で床停止・5 m 未満 0 | M6 | B03 | 既存テスト（npx playwright test）と qa-out/throws.json |
| G-POWER | 40 投で勢いの差 5 以内 | #4 | B02 | 既存テスト（npx playwright test）と qa-out/throws.json |
| G-T-3RUNS | 3 回続けて結果まで | M5 | T02 | tests/flow.spec.js |
| G-VIDEO-PLAY | 通し録画とリンク | M4 | T02 | gate.js（ファイル・記録を読む） |
| G-T-START | PC・スマホで照準まで | M8 | T03 | tests/flow.spec.js |
| G-T-ERR | エラー 0 件 | M7 | B04 | tests/flow.spec.js |
| G-T-BOOT15 | 「あそぶ」まで 15 秒以内 | #1 | B05 | tests/flow.spec.js |
| G-VOICE | 声ツンツン 4 本が voice/ だけ、ノーマル 0 | M11 | CR01 | gate.js（ファイル・記録を読む） |
| G-NOTICE | voice/NOTICE.md に規約 URL 2 つ | M11 | CR01 | gate.js（ファイル・記録を読む） |
| G-CR-VOICEPLAY | ガラス突破・新記録でツンツンが鳴る | M11 | CR01 | tests/feel.spec.js |
| G-LICENSE | LICENSE／著作権の書き分け | M11 | CR02 | gate.js（ファイル・記録を読む） |
| G-CREDITS | CREDITS.md（ずんだもん・効果音・3D） | M9 | CR03 | gate.js（ファイル・記録を読む） |
| G-CR-SCREEN | ゲーム内クレジット画面 | M16・M9 | CR04 | tests/flow.spec.js |
| G-README | 公開ページに表記と配信のお願い | M9・M10 | CR05 | gate.js（ファイル・記録を読む） |
| G-F-HOOK | テスト用フック | M12 | F01 | tests/feel.spec.js |
| G-FEELJSON | feel.json（3 プリセット・初期マシマシ） | M12 | F02 | gate.js（ファイル・記録を読む） |
| G-F-DEF | 初期はマシマシ・揺れオン | M12 | F02 | tests/feel.spec.js |
| G-F-MATCH | 出た値＝feel.json の値 | M12 | F02 | tests/feel.spec.js |
| G-F-UI | 設定 2 項目・保存 | M12・M15 | F03 | tests/feel.spec.js |
| G-S-FLASH | 点滅 1 秒 3 回まで（重なり） | M14 | F04 | tests/feel.spec.js |
| G-S-SHAKEOFF | 揺れオフで揺れ・蹴り 0 | M15 | F04 | tests/feel.spec.js |
| G-S-REDUCED | 視差効果を減らすで揺れ 0・点滅なし | M15 | F04 | tests/feel.spec.js |
| G-F-OFF | 演出オフで全部消える | M13 | F04 | tests/feel.spec.js |
| G-FEEL-RECORD | 決めた値を spec.md と FEEL_DECISIONS.md に記録 | M12 | F05 | gate.js（ファイル・記録を読む） |
| G-F-FUTSUU | ふつうで C1〜C6・C8 | M12 | F06 | tests/feel.spec.js |
| G-F-TS | timeScale を 0 にしない | M12 | F06 | tests/feel.spec.js |
| G-F-M1 | ① 衝撃波リング | M12 | F07 | tests/feel.spec.js |
| G-F-M2 | ② スピードライン | M12 | F07 | tests/feel.spec.js |
| G-F-M3 | ③ 壊した数のポップ | M12 | F07 | tests/feel.spec.js |
| G-F-M4 | ④ 色つきフラッシュ | M12 | F07 | tests/feel.spec.js |
| G-F-M5 | ⑤ 残像 | M12 | F07 | tests/feel.spec.js |
| G-F-M6 | ⑥ 大きくなる連鎖の吹き出し | M12 | F07 | tests/feel.spec.js |
| G-F-M7 | ⑦ スロー＋ずんだもん | M12 | F08 | tests/feel.spec.js |
| G-F-STRONGER | マシマシ＞ふつう | M12 | F07 | tests/feel.spec.js |
| G-F-GLASS | ガラス突破が一番派手 | M12 | F08 | tests/feel.spec.js |
| G-F-REAL | 実際の 1 投で演出・点滅 | M12・M14 | F08 | tests/feel.spec.js |
| G-FPS | スマホ実機でマシマシの fps | M17 | F09 | gate.js（ファイル・記録を読む） |
| G-VIDEO-CMP | 比較動画 3 本（同じ入力）とリンク | M13 | F10 | gate.js（ファイル・記録を読む） |
| G-M14-LOG | 比較動画の記録で点滅 1 秒 3 回まで | M14 | F10 | gate.js（ファイル・記録を読む） |
| G-CHECKLIST | CHECKLIST.md のリリーの列が全部 ◯ | M12・M13 | F10 | gate.js（ファイル・記録を読む） |

## 毎版G-PERF10（本人22:56UTC追加）
`harness/evidence/perf10.json` に現VERSION・実行ZIP SHA256・同一ブラウザの全10投生RAF区間を保存する。gateは平均と最悪を再集計し、後続がどちらかで初投を上回ればFAIL。許容差、外れ値除外、丸めは無い。RAFはブラウザwall intervalで、GPU全描画や物理iPhoneFPSの証明ではない。初回を含め起動直後のタイトル→準備/充填→飛行→結果、以後もう一投→同じ流れ。seenFlightが無い古いresultの瞬間サンプルは不適合。端末/ブラウザ/暖機/入力/採取区間を付記。ログ欠落・古い版・ZIP不一致もFAIL。
