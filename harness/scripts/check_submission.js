#!/usr/bin/env node
'use strict';
// 【提出】／【進捗】の文を SUBMISSION_RULES.md の R1〜R12・P1 で機械的に確かめる。
//   node harness/scripts/check_submission.js <ファイル>   または   … | node harness/scripts/check_submission.js -
// 出力：当たった決まりごとに「REJECT R? 理由」。何も当たらなければ「ACCEPT-FOR-REVIEW」。終了コード：通れば 0、差し戻しなら 1。
const fs = require('fs');
const lib = require('./lib');
const src = process.argv[2] && process.argv[2] !== '-' ? fs.readFileSync(process.argv[2], 'utf8') : fs.readFileSync(0, 'utf8');
const t = src.replace(/\r\n/g, '\n');
const rej = [];
const R = (n, m) => rej.push(`REJECT ${n} ${m}`);
const field = name => { const m = t.match(new RegExp('^' + name + '[：:]\\s*(.*)$', 'm')); return m ? m[1].trim() : null; };
const gv = name => { const m = t.match(new RegExp('^' + name + ':\\s*(.*)$', 'm')); return m ? m[1].trim() : null; };
const planBlock = (t.match(/=== PLAN STATUS ===[\s\S]*?=== END PLAN STATUS ===/) || [])[0];

if (/^【進捗】/m.test(t) && !/^【提出】/m.test(t)) {
  if (!planBlock) R('P1', 'PLAN 状態の塊が無い（PLAN 状態を付けて出し直し）');
  else if (/PLAN-CHECK: ✕/.test(planBlock)) R('P1', 'PLAN-CHECK が ✕');
  console.log(rej.length ? rej.join('\n') : 'ACCEPT-FOR-REVIEW');
  process.exit(rej.length ? 1 : 0);
}

const head = (t.match(/^【提出】カニとばし／([A-Z]{1,2}\d{2})/m) || [])[1];
const itemId = field('項目ID');
const gateStart = t.indexOf('=== KANI GATE ===');
const gate = gateStart >= 0 ? t.slice(gateStart, (t.indexOf('GATE-RESULT:', gateStart) + 1 || t.length) + 60) : '';
// R1
if (gateStart < 0 || !/^GATE-RESULT:/m.test(t)) R('R1', 'ゲート出力（=== KANI GATE === と GATE-RESULT:）が無い');
// R2
if (gateStart >= 0) {
  const gateText = t.slice(gateStart, planBlock ? Math.max(gateStart, t.indexOf('=== PLAN STATUS ===')) : t.length);
  const xRows = gateText.split('\n').filter(l => /^\|.*\|\s*✕\s*\|/.test(l));
  const res = gv('GATE-RESULT');
  const checks = (t.match(/^CHECKS: \d+ ran, \d+ ◯, (\d+) ✕/m) || [])[1];
  if (xRows.length || res !== 'ALL-PASS' || checks === undefined || Number(checks) !== 0) R('R2', `ゲートに ✕（✕ の行 ${xRows.length}、GATE-RESULT: ${res}、CHECKS の ✕ ${checks}）`);
}
// R3
const target = gv('TARGET');
if (!head || !itemId || !target || head !== itemId || itemId !== target) R('R3', `項目が合わない（1 行目 ${head}、項目ID ${itemId}、TARGET ${target}）`);
// R4
const ver = field('版名');
if (!ver || ver !== gv('VERSION')) R('R4', `版名 ${ver} ≠ ゲートの VERSION ${gv('VERSION')}`);
// R5
const sha = field('SHA256');
if (!sha || !/^[0-9a-f]{64}$/i.test(sha) || sha.toLowerCase() !== String(gv('ZIP-SHA256') || '').toLowerCase()) R('R5', `SHA256 が無いか形が違うか ゲートの ZIP-SHA256 と違う（${sha}）`);
// R6
const dl = field('ダウンロード');
if (!lib.isDownloadLink(dl) || dl !== gv('ZIP-URL')) R('R6', `ダウンロードのリンクが GitHub／Drive でないか ZIP-URL と違う（${dl}）`);
if (/https?:\/\/[^\s]*slack\.com/i.test(t)) R('R6', '本文に slack.com のリンクがある');
// R7
if (!planBlock) R('R7', 'PLAN 状態の塊（=== PLAN STATUS === 〜 === END PLAN STATUS ===）が無い');
else {
  // R8
  if (/PLAN-CHECK: ✕/.test(planBlock) || /✕ 飛ばした・理由なし/.test(planBlock)) R('R8', '順番を理由なしで飛ばした／PLAN-CHECK が ✕');
  // R9
  const row = planBlock.split('\n').find(l => itemId && new RegExp('^\\|\\s*' + itemId + '\\s*\\|').test(l));
  const hdr = (planBlock.split('\n').find(l => /^\|\s*ID\s*\|/.test(l)) || '').split('|').map(s => s.trim());
  const si = hdr.indexOf('状態');
  const st = row && si > 0 ? row.split('|').map(s => s.trim())[si] : null;
  if (st !== '提出済') R('R9', `PLAN 状態で ${itemId} が「${st}」（提出済 でない）`);
}
// R10
const need = { T02: 1, F10: 3, R01: 4 }[itemId] || 0;
if (need) {
  const vline = field('動画') || '';
  const n = (vline.match(/https:\/\/[^\s、,）)]+/g) || []).filter(lib.isDownloadLink).length;
  if (n < need) R('R10', `動画リンクが ${n} 本（${itemId} は ${need} 本以上）`);
}
// R11
for (const f of ['変えた値', 'まだできていないこと']) {
  const v = field(f);
  if (!v || /（書く。/.test(v)) R('R11', `「${f}」が空か「（書く。」が残っている`);
}
// R12
const cm = field('コミット');
if (!cm || cm !== gv('GIT-COMMIT') || gv('GIT-DIRTY') !== 'no') R('R12', `コミット ${cm} ≠ GIT-COMMIT ${gv('GIT-COMMIT')}、または GIT-DIRTY: ${gv('GIT-DIRTY')}`);

console.log(rej.length ? rej.join('\n') : 'ACCEPT-FOR-REVIEW');
process.exit(rej.length ? 1 : 0);
