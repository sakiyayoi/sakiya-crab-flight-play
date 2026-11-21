'use strict';
// ハーネス共通の部品（gate.js・plan.js・テストから使う）。外部パッケージは使わない。
const fs = require('fs');
const path = require('path');

const STATUSES = ['未着手', '作業中', '提出済', '差し戻し', '合格'];
const DONE = ['提出済', '合格'];
const FLASH_MAX_PER_SEC = 3; // M14：固定。リリーは変えない
const ID_RE = /^[A-Z]{1,2}\d{2}$/;

function jstNow() {
  const f = new Intl.DateTimeFormat('ja-JP', { timeZone: 'Asia/Tokyo', year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', hour12: false });
  const p = Object.fromEntries(f.formatToParts(new Date()).map(x => [x.type, x.value]));
  return { full: `${p.year}-${p.month}-${p.day} ${p.hour}:${p.minute} JST`, short: `${p.month}/${p.day} ${p.hour}:${p.minute}` };
}

// PLAN.md の「計画表」を読む。列の名前で探すので、列の順番は変えてよい（名前は変えない）。
const REQUIRED_COLS = ['ID', '流れ', '中身', '依存', '状態', '更新（JST）', '順番を飛ばした理由'];
function splitRow(line) {
  const t = line.trim();
  if (!t.startsWith('|')) return null;
  return t.replace(/^\|/, '').replace(/\|$/, '').split('|').map(s => s.trim());
}
function readPlan(planPath) {
  const text = fs.readFileSync(planPath, 'utf8');
  const lines = text.split('\n');
  let header = null, headerIdx = -1;
  for (let i = 0; i < lines.length; i++) {
    const c = splitRow(lines[i]);
    if (c && c[0] === 'ID' && c.includes('状態') && c.includes('流れ')) { header = c; headerIdx = i; break; }
  }
  if (!header) throw new Error('PLAN.md に計画表（「| ID | 流れ | … | 状態 |」の表）が見つからない');
  for (const k of REQUIRED_COLS) if (!header.includes(k)) throw new Error(`PLAN.md の計画表に列「${k}」が無い`);
  const col = Object.fromEntries(header.map((h, i) => [h, i]));
  const rows = [];
  for (let i = headerIdx + 2; i < lines.length; i++) {
    const c = splitRow(lines[i]);
    if (!c) break;
    if (!ID_RE.test(c[col['ID']] || '')) continue;
    if (c.length !== header.length) throw new Error(`PLAN.md ${i + 1} 行目：列の数が ${c.length}（見出しは ${header.length}）。セルの中に「|」を書かない`);
    const deps = (c[col['依存']] || '').split(/[、,，\s]+/).filter(x => ID_RE.test(x));
    rows.push({ id: c[col['ID']], stream: c[col['流れ']], what: c[col['中身']], deps, status: c[col['状態']], updated: c[col['更新（JST）']], reason: c[col['順番を飛ばした理由']], lineIdx: i, cells: c });
  }
  return { text, lines, header, col, rows, planPath };
}
function writePlan(plan) {
  for (const r of plan.rows) {
    r.cells[plan.col['状態']] = r.status;
    r.cells[plan.col['更新（JST）']] = r.updated;
    r.cells[plan.col['順番を飛ばした理由']] = r.reason;
    plan.lines[r.lineIdx] = '| ' + r.cells.join(' | ') + ' |';
  }
  fs.writeFileSync(plan.planPath, plan.lines.join('\n'));
}
function depsMet(plan, row) {
  const byId = Object.fromEntries(plan.rows.map(r => [r.id, r]));
  return row.deps.every(d => byId[d] && DONE.includes(byId[d].status));
}
function emptyReason(s) { return !s || s === '—' || s === '-' || s === 'なし'; }
// 同じ流れの中で、前の項目（依存がそろっている）がまだ「未着手」なのに後ろを始めた項目。理由が空なら違反。
function orderProblems(plan) {
  const out = [];
  for (const r of plan.rows) {
    if (r.status === '未着手') continue;
    const earlier = plan.rows.filter(e => e.stream === r.stream && plan.rows.indexOf(e) < plan.rows.indexOf(r) && e.status === '未着手' && depsMet(plan, e));
    if (earlier.length) out.push({ id: r.id, skipped: earlier.map(e => e.id), reason: r.reason, ok: !emptyReason(r.reason) });
  }
  return out;
}
function validatePlan(plan) {
  const errs = [];
  const seen = new Set();
  for (const r of plan.rows) {
    if (seen.has(r.id)) errs.push(`${r.id} が 2 回ある`);
    seen.add(r.id);
    if (!STATUSES.includes(r.status)) errs.push(`${r.id} の状態「${r.status}」は使えない（${STATUSES.join('／')} のどれか）`);
    if (r.status !== '未着手' && !/^\d{2}\/\d{2} \d{2}:\d{2}$/.test(r.updated)) errs.push(`${r.id} の更新（JST）が「MM/DD HH:MM」の形でない`);
    for (const d of r.deps) if (!seen.has(d) && !plan.rows.some(x => x.id === d)) errs.push(`${r.id} の依存 ${d} が計画表に無い`);
    if (r.status !== '未着手') {
      const bad = r.deps.filter(d => { const x = plan.rows.find(y => y.id === d); return !x || ['未着手', '作業中'].includes(x.status); });
      if (bad.length) errs.push(`${r.id} は依存 ${bad.join('、')} が一度も提出されていないのに ${r.status} になっている`);
    }
  }
  for (const p of orderProblems(plan)) if (!p.ok) errs.push(`${p.id} は同じ流れの ${p.skipped.join('、')} より先に始めたのに「順番を飛ばした理由」が空`);
  return errs;
}
function planStatusBlock(plan) {
  const now = jstNow();
  const lines = ['=== PLAN STATUS ===', `出力：${now.full}`, '| ID | 流れ | 状態 | 更新（JST） | 順番 |', '|---|---|---|---|---|'];
  const probs = Object.fromEntries(orderProblems(plan).map(p => [p.id, p]));
  for (const r of plan.rows) {
    const p = probs[r.id];
    const order = !p ? 'ok' : p.ok ? `飛ばした（理由：${r.reason}）` : `✕ 飛ばした・理由なし（${p.skipped.join('、')} が先）`;
    lines.push(`| ${r.id} | ${r.stream} | ${r.status} | ${r.updated || '—'} | ${order} |`);
  }
  const count = s => plan.rows.filter(r => r.status === s).length;
  lines.push(`集計：${STATUSES.map(s => `${s} ${count(s)}`).join('／')}`);
  const errs = validatePlan(plan);
  lines.push(errs.length ? `PLAN-CHECK: ✕ ${errs.length} 件（${errs.join('；')}）` : 'PLAN-CHECK: ◯');
  lines.push('=== END PLAN STATUS ===');
  return lines.join('\n');
}

// 点滅：どの 1 秒（1000 ms の窓）でも 3 回まで。entries は __kaniTest.events。
function flashEntries(events) { return (events || []).filter(e => e && e.fx === 'flash'); }
function maxFlashesPerSecond(events) {
  const ts = flashEntries(events).map(e => Number(e.t)).sort((a, b) => a - b);
  let max = 0;
  for (let i = 0; i < ts.length; i++) {
    let n = 0;
    for (let j = i; j < ts.length && ts[j] < ts[i] + 1000; j++) n++;
    max = Math.max(max, n);
  }
  return max;
}

// 公開してよいリンクの形：GitHub raw／Release、Google Drive のファイル。Slack の添付は不可。
const LINK_RE = /^https:\/\/(raw\.githubusercontent\.com\/[^\s]+|github\.com\/[^/\s]+\/[^/\s]+\/(releases\/download|raw)\/[^\s]+|drive\.google\.com\/(file\/d\/[^\s]+|uc\?[^\s]*id=[^\s]+|open\?id=[^\s]+))$/;
function isDownloadLink(u) { return typeof u === 'string' && LINK_RE.test(u.trim()) && !/slack\.com/.test(u); }

module.exports = { STATUSES, DONE, FLASH_MAX_PER_SEC, ID_RE, jstNow, readPlan, writePlan, depsMet, orderProblems, validatePlan, planStatusBlock, flashEntries, maxFlashesPerSecond, isDownloadLink, emptyReason };
