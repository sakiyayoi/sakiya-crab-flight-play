## カニとばし：作業の決まり（AGENTS.md の最後に足す）

**始めるとき（毎回）**
1. `harness/PLAN.md` を開き、`node harness/scripts/plan.js next` で次の項目を見る（上から順。差し戻しが先）。
2. `node harness/scripts/plan.js start <ID>` で状態を「作業中」にする。出た【進捗】文を #cloe-dot に出す。

**終わるとき（毎回）**
3. `bash harness/scripts/gate.sh <ID>` を走らせ、出力を全部【提出】か【進捗】に貼る。
4. 【提出】は全部 ◯（`GATE-RESULT: ALL-PASS`）のときだけ。`node harness/scripts/plan.js submit <ID>` が作った文で出す。

進め方の中身は `harness/PROCESS.md`、差し戻しになる形は `harness/SUBMISSION_RULES.md`。
