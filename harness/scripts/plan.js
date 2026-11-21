#!/usr/bin/env node
'use strict';
// 計画表（PLAN.md）の状態を変える道具。手で書き換えず、これを使う。
//   node harness/scripts/plan.js status            … 状態表を出す（【提出】【進捗】に貼る）
//   node harness/scripts/plan.js next              … 次に取る項目を出す
//   node harness/scripts/plan.js start <ID> [--reason "…"]   … 作業中にする
//   node harness/scripts/plan.js submit <ID>       … ゲートが全部 ◯ のときだけ 提出済 にし、【提出】文を作る
//   node harness/scripts/plan.js set <ID> 合格|差し戻し --by マツリ|カニ係   … マツリ・カニ係だけが使う
const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const lib = require('./lib');

const H = path.resolve(__dirname, '..');
const ROOT = path.resolve(H, '..');
const PLAN = path.join(H, 'PLAN.md');
const OUT = path.join(H, 'out');

function die(msg) { console.error('✕ ' + msg); process.exit(1); }
function argVal(name) { const i = process.argv.indexOf(name); return i > 0 ? process.argv[i + 1] : undefined; }
function head() { try { return cp.execSync('git rev-parse HEAD', { cwd: ROOT, encoding: 'utf8' }).trim(); } catch { return ''; } }

const [cmd, id] = process.argv.slice(2);
const plan = lib.readPlan(PLAN);
const row = id ? plan.rows.find(r => r.id === id) : null;
if (id && !row && cmd !== 'status' && cmd !== 'next') die(`項目 ${id} は PLAN.md に無い`);

function candidates() {
  const back = plan.rows.filter(r => r.status === '差し戻し');
  const out = [];
  const streams = [...new Set(plan.rows.map(r => r.stream))];
  for (const s of streams) {
    const first = plan.rows.find(r => r.stream === s && r.status === '未着手' && lib.depsMet(plan, r));
    if (first) out.push(first);
  }
  return { back, out };
}

if (cmd === 'status') {
  console.log(lib.planStatusBlock(plan));
} else if (cmd === 'next') {
  const { back, out } = candidates();
  if (back.length) console.log('まず直す（差し戻し）：' + back.map(r => `${r.id} ${r.what.slice(0, 40)}`).join(' ／ '));
  const working = plan.rows.filter(r => r.status === '作業中');
  if (working.length) console.log('作業中：' + working.map(r => r.id).join('、'));
  if (!out.length) console.log('今すぐ始められる未着手の項目はない（依存待ち、または全部終わり）');
  for (const r of out) console.log(`次（${r.stream}）：${r.id}　${r.what}\n　終わりの条件：${r.cells[plan.col['終わりの条件（測れる形）']] || ''}\n　始める：node harness/scripts/plan.js start ${r.id}`);
} else if (cmd === 'start') {
  if (!['未着手', '差し戻し'].includes(row.status)) die(`${id} は今「${row.status}」。始められるのは 未着手 か 差し戻し だけ`);
  if (!lib.depsMet(plan, row)) {
    const notYet = row.deps.filter(d => !lib.DONE.includes((plan.rows.find(r => r.id === d) || {}).status));
    die(`${id} は依存 ${notYet.join('、')} が 提出済／合格 になっていないので始められない（通し版が先の決まり）`);
  }
  const idx = plan.rows.indexOf(row);
  const earlier = plan.rows.filter((r, i) => i < idx && r.stream === row.stream && r.status === '未着手');
  const startable = earlier.filter(r => lib.depsMet(plan, r));
  const reason = argVal('--reason');
  if (startable.length && !reason) die(`同じ流れ「${row.stream}」の ${startable.map(r => r.id).join('、')} が先です。飛ばすなら理由を書く：node harness/scripts/plan.js start ${id} --reason "理由"`);
  if (reason) row.reason = reason.replace(/\|/g, '／');
  else if (earlier.length) row.reason = `自動：${earlier.map(r => r.id).join('、')} は依存待ちだった`;
  row.status = '作業中';
  row.updated = lib.jstNow().short;
  lib.writePlan(plan);
  const p2 = lib.readPlan(PLAN);
  const block = lib.planStatusBlock(p2);
  const msg = `【進捗】カニとばし／${id} を始めます\n中身：${row.what}\n終わりの条件：${row.cells[plan.col['終わりの条件（測れる形）']] || ''}\n${row.reason && !lib.emptyReason(row.reason) ? '順番を飛ばした理由：' + row.reason + '\n' : ''}\`\`\`\n${block}\n\`\`\``;
  fs.mkdirSync(OUT, { recursive: true });
  fs.writeFileSync(path.join(OUT, `progress_${id}.md`), msg + '\n');
  console.log(msg);
  console.log(`\n（この文は harness/out/progress_${id}.md にも保存した。#cloe-dot にそのまま出す）`);
} else if (cmd === 'submit') {
  if (row.status !== '作業中') die(`${id} は今「${row.status}」。提出できるのは 作業中 の項目だけ（先に start）`);
  const gf = path.join(OUT, `gate_${id}.txt`);
  if (!fs.existsSync(gf)) die(`ゲートの出力 ${path.relative(ROOT, gf)} が無い。先に bash harness/scripts/gate.sh ${id}`);
  const g = fs.readFileSync(gf, 'utf8');
  if (!/^TARGET: (\S+)$/m.test(g) || g.match(/^TARGET: (\S+)$/m)[1] !== id) die('ゲートの出力の TARGET が違う');
  if (!/^GATE-RESULT: ALL-PASS$/m.test(g)) die('ゲートに ✕ がある。直して gate.sh をやり直す（【提出】はできない）');
  const gc = (g.match(/^GIT-COMMIT: (\S+)$/m) || [])[1];
  if (!gc || gc !== head()) die(`ゲートを走らせた後にコミットが変わった（ゲート ${gc} ／今 ${head()}）。gate.sh をやり直す`);
  row.status = '提出済';
  row.updated = lib.jstNow().short;
  lib.writePlan(plan);
  const block = lib.planStatusBlock(lib.readPlan(PLAN));
  const val = k => (g.match(new RegExp('^' + k + ': (.*)$', 'm')) || [])[1] || '';
  let links = {};
  try { links = JSON.parse(fs.readFileSync(path.join(H, 'evidence', 'links.json'), 'utf8')); } catch {}
  const vids = [links.playthrough && `通し録画 ${links.playthrough}`, links.compare && links.compare.mashimashi && `マシマシ ${links.compare.mashimashi}`, links.compare && links.compare.futsuu && `ふつう ${links.compare.futsuu}`, links.compare && links.compare.off && `オフ ${links.compare.off}`].filter(Boolean);
  const msg = [
    `【提出】カニとばし／${id} ${row.what.slice(0, 60)}`,
    `項目ID：${id}`,
    `版名：${val('VERSION')}`,
    `SHA256：${val('ZIP-SHA256')}`,
    `ダウンロード：${val('ZIP-URL')}`,
    `コミット：${gc}`,
    `動画：${vids.length ? vids.join(' ／ ') : 'なし（この項目は動画不要）'}`,
    `変えた値：（書く。手触りの項目は FEEL_DECISIONS.md の今回の表を貼る。それ以外は「なし」）`,
    `満たした条件：${row.cells[plan.col['対応（M・条件）']] || ''}`,
    `まだできていないこと：（書く。無ければ「なし」）`,
    `次の担当：マツリ（確認）`,
    'ゲート出力：', '```', g.trim(), '```',
    'PLAN 状態：', '```', block, '```'
  ].join('\n');
  fs.writeFileSync(path.join(OUT, `submission_${id}.md`), msg + '\n');
  console.log(msg);
  console.log(`\n（harness/out/submission_${id}.md に保存した。「（書く。…）」の 2 か所を埋めてから #cloe-dot に出す。PLAN.md の変更もコミットする）`);
} else if (cmd === 'set') {
  const st = process.argv[4];
  const by = argVal('--by');
  if (!['合格', '差し戻し'].includes(st)) die('set で付けられるのは 合格 か 差し戻し だけ');
  if (!['マツリ', 'カニ係'].includes(by)) die('set は マツリ かカニ係だけが使う（--by マツリ ／ --by カニ係）');
  const humanOnly = id === 'R02'; // 通治さんの「公開OK」。ゲートが無いので、カニ係が依存（R01）を見て直接付ける
  if (humanOnly && !lib.depsMet(plan, row)) die('R02 は R01 が 提出済／合格 になってから');
  if (!humanOnly && row.status !== '提出済' && !(st === '差し戻し' && row.status === '合格')) die(`${id} は今「${row.status}」。合格・差し戻しは 提出済 の項目に付ける`);
  row.status = st;
  row.updated = lib.jstNow().short;
  lib.writePlan(plan);
  console.log(lib.planStatusBlock(lib.readPlan(PLAN)));
} else {
  console.log('使い方：node harness/scripts/plan.js status | next | start <ID> [--reason "…"] | submit <ID> | set <ID> 合格|差し戻し --by マツリ|カニ係');
  process.exit(2);
}
