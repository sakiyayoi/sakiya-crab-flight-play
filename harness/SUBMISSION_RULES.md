# 【提出】が自動で差し戻しになる決まり（SUBMISSION_RULES.md）

下の R1〜R12 に 1 つでも当たる【提出】は、マツリもカニ係も中身を読まずに差し戻します（理由の番号だけ返します）。
機械で確かめられます：`node harness/scripts/check_submission.js <提出文のファイル>`（標準入力でも可）。当たったら `REJECT R番号 …`、全部通れば `ACCEPT-FOR-REVIEW`。
ACCEPT-FOR-REVIEW は「読んでもらえる形になっている」という意味で、合格ではありません。合格はマツリが動画と中身を見て付けます。
いちばん簡単に守る方法：`node harness/scripts/plan.js submit <ID>` が作った文を使い、「（書く。…）」の 2 か所だけ埋める。

| 番号 | 差し戻しになるとき |
|---|---|
| R1 | ゲート出力が無い：本文に `=== KANI GATE ===` の行と `GATE-RESULT:` の行のどちらかが無い |
| R2 | ゲートに ✕ がある：ゲート出力の表に結果 `✕` の行が 1 つでもある、または `GATE-RESULT: ALL-PASS` でない、または `CHECKS:` の行の ✕ の数が 0 でない |
| R3 | 項目が合わない：1 行目 `【提出】カニとばし／<ID>`、`項目ID：<ID>`、ゲートの `TARGET: <ID>` の 3 つが同じでない |
| R4 | 版名が無い・違う：`版名：` が空、またはゲートの `VERSION:` と同じでない |
| R5 | SHA256 が無い・違う：`SHA256：` が 64 文字の 16 進でない、またはゲートの `ZIP-SHA256:` と同じでない |
| R6 | リンクが違う：`ダウンロード：` が GitHub（raw.githubusercontent.com、github.com/…/releases/download/…、github.com/…/raw/…）か Google Drive（drive.google.com/file/d/…、uc?…id=…、open?id=…）のファイルのリンクでない、またはゲートの `ZIP-URL:` と同じでない、または本文のどこかに slack.com のリンクがある（Slack の添付は取れない） |
| R7 | PLAN 状態が無い：`=== PLAN STATUS ===` から `=== END PLAN STATUS ===` までの塊が無い |
| R8 | 順番を理由なしで飛ばした：PLAN 状態の塊に `PLAN-CHECK: ✕` がある、または「✕ 飛ばした・理由なし」の行がある |
| R9 | 状態が合わない：PLAN 状態の表で、その項目の状態が「提出済」でない |
| R10 | 動画が足りない：T02 は 1 本以上、F10 は 3 本以上、R01 は 4 本以上の GitHub／Drive の動画リンクが `動画：` の行に無い |
| R11 | 書いていない欄がある：`変えた値：` か `まだできていないこと：` が空、または「（書く。」が残っている（無いときは「なし」と書く） |
| R12 | コミットが合わない：`コミット：` がゲートの `GIT-COMMIT:` と同じでない、またはゲートが `GIT-DIRTY: yes` |

## 【進捗】の決まり
【進捗】（途中の報告）にも PLAN 状態の塊（`=== PLAN STATUS ===` 〜 `=== END PLAN STATUS ===`）を必ず付ける。無い【進捗】は「PLAN 状態を付けて出し直し」とだけ返す（P1）。
`node harness/scripts/plan.js start <ID>` と `node harness/scripts/plan.js status` が塊を出す。

## 差し戻しのあと
差し戻しは `plan.js set <ID> 差し戻し --by マツリ`（またはカニ係）で付き、`plan.js next` の一番先に出る。直して同じ手順（PROCESS.md）でもう一度【提出】する。
