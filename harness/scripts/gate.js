#!/usr/bin/env node
'use strict';
// カニとばし ゲート。1 つのコマンドで、今ある合格チェックと新しいチェックを全部走らせ、◯／✕ の表を出す。
// ✕ が 1 つでもあれば終了コードは 1（【提出】できない）。
//   bash harness/scripts/gate.sh <項目ID>    例：bash harness/scripts/gate.sh T01
//   bash harness/scripts/gate.sh --all       （＝ R01。全部のチェック）
// 見る範囲：その項目のチェック ＋ すでに 提出済／合格 の項目のチェック（前に通ったものが壊れていないか）＋ 毎回のチェック。
// 環境変数：KANI_ZIP（既定 dist/<版名>.zip）、KANI_SUITE_CMD（既定 npx playwright test）、KANI_THROWS_JSON（既定 qa-out/throws.json）
const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const crypto = require('crypto');
const lib = require('./lib');

const H = path.resolve(__dirname, '..');
const ROOT = path.resolve(H, '..');
const OUT = path.join(H, 'out');
const EVD = path.join(H, 'evidence');
fs.mkdirSync(OUT, { recursive: true });

let target = (process.argv[2] || '').trim();
if (!target) { console.error('使い方：bash harness/scripts/gate.sh <項目ID>（例 T01）／ bash harness/scripts/gate.sh --all'); process.exit(2); }
if (target === '--all') target = 'R01';

// ---- 項目 → チェック ----
const BASE = ['G-PLAN', 'G-GIT', 'G-VER', 'G-ZIP', 'G-LINK', 'G-PERF10'];
const ITEM = {
  PF01: ['G-PERF10'],
  T01: ['G-SUITE-RAN'], T02: ['G-T-3RUNS', 'G-VIDEO-PLAY'], T03: ['G-T-START'],
  B01: ['G-40-LAUNCH'], B02: ['G-POWER'], B03: ['G-40-STOP', 'G-SUITE-PASS'], B04: ['G-T-ERR', 'G-SUITE-PASS'], B05: ['G-T-BOOT15'],
  CR01: ['G-VOICE', 'G-NOTICE', 'G-CR-VOICEPLAY'], CR02: ['G-LICENSE'], CR03: ['G-CREDITS'], CR04: ['G-CR-SCREEN'], CR05: ['G-README'],
  F01: ['G-F-HOOK'], F02: ['G-FEELJSON', 'G-F-DEF', 'G-F-MATCH'], F03: ['G-F-UI'], F04: ['G-S-FLASH', 'G-S-SHAKEOFF', 'G-S-REDUCED', 'G-F-OFF'],
  F05: ['G-FEEL-RECORD'], F06: ['G-F-FUTSUU', 'G-F-TS'], F07: ['G-F-M1', 'G-F-M2', 'G-F-M3', 'G-F-M4', 'G-F-M5', 'G-F-M6', 'G-F-STRONGER'],
  F08: ['G-F-M7', 'G-F-GLASS', 'G-F-REAL'], F09: ['G-FPS'], F10: ['G-VIDEO-CMP', 'G-M14-LOG', 'G-CHECKLIST'],
};
ITEM.R01 = [...new Set(Object.values(ITEM).flat())];
const LABEL = {
  'G-PERF10': ['同一版10連投、後続の平均・最悪が1投目を一度も超えない', '本人22:56・毎版'],
  'G-PLAN': ['PLAN.md の状態表が正しく、この項目が作業中', '全部'], 'G-GIT': ['コミット済み（作業中の変更が残っていない）', '全部'],
  'G-VER': ['VERSION に版名が 1 つ', 'M2'], 'G-ZIP': ['ZIP があり、名前に版名、中に index.html', 'M2・M3'], 'G-LINK': ['ZIP のリンクが GitHub／Drive', 'M3'],
  'G-SUITE-RAN': ['既存テスト（npx playwright test）が走り 40 投の記録が出た', 'M1'], 'G-SUITE-PASS': ['既存テストが全部 pass', 'M1・M7'],
  'G-40-LAUNCH': ['40 投で未発射・300 秒止まり 0', 'M6'], 'G-40-STOP': ['40 投で床停止・5 m 未満 0', 'M6'], 'G-POWER': ['40 投で勢いの差 5 以内', '#4'],
  'G-T-3RUNS': ['3 回続けて結果まで', 'M5'], 'G-VIDEO-PLAY': ['通し録画とリンク', 'M4'], 'G-T-START': ['PC・スマホで照準まで', 'M8'],
  'G-T-ERR': ['エラー 0 件', 'M7'], 'G-T-BOOT15': ['「あそぶ」まで 15 秒以内', '#1'],
  'G-VOICE': ['声ツンツン 4 本が voice/ だけ、ノーマル 0', 'M11'], 'G-NOTICE': ['voice/NOTICE.md に規約 URL 2 つ', 'M11'], 'G-CR-VOICEPLAY': ['ガラス突破・新記録でツンツンが鳴る', 'M11'],
  'G-LICENSE': ['LICENSE／著作権の書き分け', 'M11'], 'G-CREDITS': ['CREDITS.md（ずんだもん・効果音・3D）', 'M9'], 'G-CR-SCREEN': ['ゲーム内クレジット画面', 'M16・M9'],
  'G-README': ['公開ページに表記と配信のお願い', 'M9・M10'],
  'G-F-HOOK': ['テスト用フック', 'M12'], 'G-FEELJSON': ['feel.json（3 プリセット・初期マシマシ）', 'M12'], 'G-F-DEF': ['初期はマシマシ・揺れオン', 'M12'],
  'G-F-MATCH': ['出た値＝feel.json の値', 'M12'], 'G-F-UI': ['設定 2 項目・保存', 'M12・M15'],
  'G-S-FLASH': ['点滅 1 秒 3 回まで（重なり）', 'M14'], 'G-S-SHAKEOFF': ['揺れオフで揺れ・蹴り 0', 'M15'], 'G-S-REDUCED': ['視差効果を減らすで揺れ 0・点滅なし', 'M15'],
  'G-F-OFF': ['演出オフで全部消える', 'M13'], 'G-FEEL-RECORD': ['決めた値を spec.md と FEEL_DECISIONS.md に記録', 'M12'],
  'G-F-FUTSUU': ['ふつうで C1〜C6・C8', 'M12'], 'G-F-TS': ['timeScale を 0 にしない', 'M12'],
  'G-F-M1': ['① 衝撃波リング', 'M12'], 'G-F-M2': ['② スピードライン', 'M12'], 'G-F-M3': ['③ 壊した数のポップ', 'M12'], 'G-F-M4': ['④ 色つきフラッシュ', 'M12'],
  'G-F-M5': ['⑤ 残像', 'M12'], 'G-F-M6': ['⑥ 大きくなる連鎖の吹き出し', 'M12'], 'G-F-M7': ['⑦ スロー＋ずんだもん', 'M12'],
  'G-F-STRONGER': ['マシマシ＞ふつう', 'M12'], 'G-F-GLASS': ['ガラス突破が一番派手', 'M12'], 'G-F-REAL': ['実際の 1 投で演出・点滅', 'M12・M14'],
  'G-FPS': ['スマホ実機でマシマシの fps', 'M17'], 'G-VIDEO-CMP': ['比較動画 3 本（同じ入力）とリンク', 'M13'], 'G-M14-LOG': ['比較動画の記録で点滅 1 秒 3 回まで', 'M14'],
  'G-CHECKLIST': ['CHECKLIST.md のリリーの列が全部 ◯', 'M12・M13'],
};
const PW = new Set(['G-T-3RUNS', 'G-T-START', 'G-T-ERR', 'G-T-BOOT15', 'G-CR-SCREEN', 'G-CR-VOICEPLAY', 'G-F-HOOK', 'G-F-DEF', 'G-F-MATCH', 'G-F-UI', 'G-S-FLASH', 'G-S-SHAKEOFF', 'G-S-REDUCED', 'G-F-OFF', 'G-F-FUTSUU', 'G-F-TS', 'G-F-M1', 'G-F-M2', 'G-F-M3', 'G-F-M4', 'G-F-M5', 'G-F-M6', 'G-F-M7', 'G-F-STRONGER', 'G-F-GLASS', 'G-F-REAL']);
const SUITE = new Set(['G-SUITE-RAN', 'G-SUITE-PASS', 'G-40-LAUNCH', 'G-40-STOP', 'G-POWER']);

// ---- 準備 ----
const results = {}; // id -> {ok, detail}
const set = (id, ok, detail = '') => { results[id] = { ok: !!ok, detail: String(detail).replace(/\s+/g, ' ').slice(0, 300) }; };
const read = p => { try { return fs.readFileSync(p, 'utf8'); } catch { return null; } };
const readJson = p => { try { return JSON.parse(fs.readFileSync(p, 'utf8')); } catch { return null; } };
const sh = (cmd, opts = {}) => cp.spawnSync('bash', ['-lc', cmd], { cwd: ROOT, encoding: 'utf8', maxBuffer: 1 << 28, ...opts });

let plan = null, planErr = '';
try { plan = lib.readPlan(path.join(H, 'PLAN.md')); } catch (e) { planErr = e.message; }
if (plan && !plan.rows.find(r => r.id === target)) { console.error(`✕ 項目 ${target} は PLAN.md に無い`); process.exit(2); }
if (!ITEM[target]) { console.error(`✕ 項目 ${target} にはゲートのチェックが無い（人が見る項目）`); process.exit(2); }
const scopeItems = [target, ...(plan ? plan.rows.filter(r => lib.DONE.includes(r.status) && r.id !== target && ITEM[r.id]).map(r => r.id) : [])];
const scope = [...new Set([...BASE, ...scopeItems.flatMap(i => ITEM[i])])];
const allIds = Object.keys(LABEL);

const runAt = lib.jstNow().full;
const version = ((read(path.join(ROOT, 'VERSION')) || '').split('\n')[0] || '').trim();
const links = readJson(path.join(EVD, 'links.json')) || {};
const zipPath = path.resolve(ROOT, process.env.KANI_ZIP || path.join('dist', `${version}.zip`));
const gitHead = (sh('git rev-parse HEAD').stdout || '').trim();
const dirtyLines = (sh('git status --porcelain').stdout || '').split('\n').filter(l => l.trim() && !/harness\/PLAN\.md$/.test(l) && !/harness\/out\//.test(l));

// G-PLAN
if (!plan) set('G-PLAN', false, planErr);
else {
  const errs = lib.validatePlan(plan);
  const me = plan.rows.find(r => r.id === target);
  if (me.status !== '作業中') errs.push(`${target} の状態が「${me.status}」（先に node harness/scripts/plan.js start ${target}）`);
  set('G-PLAN', errs.length === 0, errs.join('；') || 'ok');
}
// G-GIT
set('G-GIT', /^[0-9a-f]{40}$/.test(gitHead) && dirtyLines.length === 0, !gitHead ? 'git のリポジトリでない' : dirtyLines.length ? `コミットしていない変更 ${dirtyLines.length} 件：${dirtyLines.slice(0, 5).join(', ')}` : gitHead.slice(0, 12));
// G-VER
set('G-VER', /^[A-Za-z0-9][A-Za-z0-9._-]{2,63}$/.test(version), version || 'VERSION ファイルが無いか空');
// G-ZIP（ZIP を解いて、その中身をテストする）
let zipSha = '', buildDir = '', zipList = [];
if (fs.existsSync(zipPath)) {
  // Bound each read: the host file bridge can time out on one large synchronous ZIP read.
  const zipHash=crypto.createHash('sha256'), zipFD=fs.openSync(zipPath,'r'), zipChunk=Buffer.alloc(256*1024);
  let zipRead;while((zipRead=fs.readSync(zipFD,zipChunk,0,zipChunk.length,null))>0)zipHash.update(zipChunk.subarray(0,zipRead));
  fs.closeSync(zipFD);zipSha=zipHash.digest('hex');
  const bd = path.join(OUT, 'build');
  fs.rmSync(bd, { recursive: true, force: true }); fs.mkdirSync(bd, { recursive: true });
  const py = `import zipfile,sys;z=zipfile.ZipFile(sys.argv[1]);print("\\n".join(z.namelist()));z.extractall(sys.argv[2])`;
  const u = cp.spawnSync('python3', ['-c', py, zipPath, bd], { encoding: 'utf8', maxBuffer: 1 << 26 });
  zipList = (u.stdout || '').split('\n').filter(Boolean);
  const cands = zipList.filter(n => /(^|\/)index\.html$/.test(n) && n.split('/').length <= 3).sort((a, b) => a.length - b.length);
  if (cands[0]) buildDir = path.join(bd, path.dirname(cands[0]));
  const nameOk = version && path.basename(zipPath).includes(version);
  set('G-ZIP', u.status === 0 && buildDir && nameOk, u.status !== 0 ? 'ZIP を解けない' : !buildDir ? 'ZIP の中に index.html が無い' : !nameOk ? `ZIP の名前 ${path.basename(zipPath)} に版名 ${version} が無い` : `${path.basename(zipPath)}（${zipList.length} ファイル）`);
} else set('G-ZIP', false, `ZIP が無い：${path.relative(ROOT, zipPath)}（KANI_ZIP で場所を渡す）`);
// G-LINK
set('G-LINK', lib.isDownloadLink(links.zip), links.zip ? (lib.isDownloadLink(links.zip) ? links.zip : `GitHub raw／Release か Google Drive のファイルのリンクでない：${links.zip}`) : 'harness/evidence/links.json の "zip" が空');

// Recompute from raw intervals of this exact archive; old-version evidence cannot pass.
const perf10 = require('./perf10').inspectPerf10(readJson(path.join(EVD, 'perf10.json')), version, zipSha);
set('G-PERF10', perf10.ok, perf10.ok ? '10投の生値から再集計、超過0' : perf10.problems.join('；'));

// ---- 既存テスト（npx playwright test）----
const needSuite = scope.some(id => SUITE.has(id));
let throws = null;
if (needSuite) {
  const key = `${zipSha}:${gitHead}`;
  const cacheF = path.join(OUT, 'suite_cache.json');
  let c = readJson(cacheF);
  const throwsSrc = path.resolve(ROOT, process.env.KANI_THROWS_JSON || path.join('qa-out', 'throws.json'));
  if (!c || c.key !== key || !buildDir) {
    if (!buildDir) c = { key, status: null, log: 'ZIP が無いので走らせていない' };
    else {
      const t0 = Date.now();
      try { fs.rmSync(throwsSrc, { force: true }); } catch {}
      const cmd = process.env.KANI_SUITE_CMD || 'npx playwright test';
      console.error(`… 既存テストを実行中：${cmd}（40 投。時間がかかる）`);
      const r = sh(cmd, { env: { ...process.env, CRAB_BUILD_DIR: buildDir, KANI_BUILD_DIR: buildDir }, timeout: 3 * 3600 * 1000 });
      const log = (r.stdout || '') + (r.stderr || '');
      fs.writeFileSync(path.join(OUT, 'suite.log'), log);
      const st = fs.existsSync(throwsSrc) ? fs.statSync(throwsSrc) : null;
      const tj = st && st.mtimeMs >= t0 ? readJson(throwsSrc) : null;
      c = { key, status: r.status, passed: Number((log.match(/(\d+) passed/) || [])[1] || 0), failed: Number((log.match(/(\d+) failed/) || [])[1] || 0), cmd, throws: tj, ranAt: runAt };
    }
    fs.writeFileSync(cacheF, JSON.stringify(c, null, 1));
  }
  throws = Array.isArray(c.throws) ? c.throws : null;
  if (throws) fs.writeFileSync(path.join(OUT, 'throws.json'), JSON.stringify(throws, null, 1));
  const strong = throws ? throws.filter(r => r.kind === 'strong').length : 0, weak = throws ? throws.filter(r => r.kind === 'weak').length : 0;
  set('G-SUITE-RAN', c.status !== null && throws && strong >= 20 && weak >= 20, c.status === null ? (c.log || '走らなかった') : !throws ? `この実行で ${path.relative(ROOT, throwsSrc)} が出なかった` : `強 ${strong}・弱 ${weak} 投の記録。${c.passed} passed／${c.failed} failed`);
  set('G-SUITE-PASS', c.status === 0 && c.passed > 0 && c.failed === 0, `終了コード ${c.status}、${c.passed} passed／${c.failed} failed（${c.cmd || ''}）`);
  const rows = throws || [];
  const nolaunch = rows.filter(r => ['charge-timeout', 'no-result-300s'].includes(r.fault) || r.phase !== 'result');
  set('G-40-LAUNCH', rows.length >= 40 && nolaunch.length === 0, rows.length < 40 ? `記録が ${rows.length} 投しかない` : nolaunch.length ? `未発射・未決着 ${nolaunch.length} 件：${nolaunch.slice(0, 5).map(r => `${r.kind}#${r.i} ${r.fault}`).join(', ')}` : '0 件');
  const stops = rows.filter(r => r.f || r.fault === 'short-stop' || (typeof r.d === 'number' && r.d < 5));
  set('G-40-STOP', rows.length >= 40 && stops.length === 0, rows.length < 40 ? `記録が ${rows.length} 投しかない` : stops.length ? `床停止・5 m 未満 ${stops.length} 件：${stops.slice(0, 5).map(r => `${r.kind}#${r.i} ${r.f || ''} ${r.d}m`).join(', ')}` : '0 件');
  const noPw = rows.filter(r => typeof r.pw !== 'number');
  const off = rows.filter(r => typeof r.pw === 'number' && Math.abs(r.pw - r.power) > 5);
  set('G-POWER', rows.length >= 40 && noPw.length === 0 && off.length === 0, rows.length < 40 ? `記録が ${rows.length} 投しかない` : noPw.length ? `${noPw.length} 投に pw（実際の勢い）の記録が無い` : off.length ? `差が 5 を超えた ${off.length} 件：${off.slice(0, 5).map(r => `${r.kind}#${r.i} 設定 ${r.power}→${r.pw}`).join(', ')}` : '全投 ±5 以内');
}

// ---- ハーネスの Playwright テスト ----
const pwIds = scope.filter(id => PW.has(id));
if (pwIds.length) {
  if (!buildDir) pwIds.forEach(id => set(id, false, 'ZIP が無いのでテストできない'));
  else {
    const jf = path.join(OUT, 'pw.json'); fs.rmSync(jf, { force: true });
    const grep = '\\[(' + pwIds.join('|') + ')\\]';
    console.error(`… ハーネスのテストを実行中（${pwIds.length} 個）`);
    const r = cp.spawnSync('npx', ['playwright', 'test', '-c', path.join(H, 'playwright.harness.config.js'), '--grep', grep], { cwd: ROOT, encoding: 'utf8', maxBuffer: 1 << 28, env: { ...process.env, KANI_BUILD_DIR: buildDir, KANI_VERSION: version }, timeout: 3 * 3600 * 1000 });
    fs.writeFileSync(path.join(OUT, 'pw.log'), (r.stdout || '') + (r.stderr || ''));
    const rep = readJson(jf);
    const agg = {};
    const walk = s => { for (const sp of s.specs || []) { const m = sp.title.match(/^\[(G-[A-Z0-9-]+)\]/); if (!m) continue; const a = agg[m[1]] || (agg[m[1]] = { ok: true, n: 0, msg: [] }); for (const t of sp.tests || []) { a.n++; if (t.status !== 'expected') { a.ok = false; const res = (t.results || []).slice(-1)[0] || {}; a.msg.push((t.status === 'skipped' ? '走らなかった' : '') + ((res.error && res.error.message) || res.status || t.status)); } } } for (const c of s.suites || []) walk(c); };
    if (rep) (rep.suites || []).forEach(walk);
    for (const id of pwIds) {
      const a = agg[id];
      if (!rep) set(id, false, 'テストの結果ファイルが出なかった（harness/out/pw.log を見る）');
      else if (!a || a.n === 0) set(id, false, 'テストが見つからない／走らなかった');
      else set(id, a.ok, a.ok ? `${a.n} 個 pass` : a.msg.join(' ／ ').replace(/\u001b\[[0-9;]*m/g, ''));
    }
    const boot = readJson(path.join(OUT, 'boot.json'));
    if (boot && results['G-T-BOOT15']) results['G-T-BOOT15'].detail = `${boot.ms} ms。` + results['G-T-BOOT15'].detail;
  }
}

// ---- ファイルのチェック ----
const repoFiles = (sh('git ls-files').stdout || '').split('\n').filter(Boolean);
const VOICES = ['glassbreak_kani_tsuntsun', 'chain7_matomete_tsuntsun', 'newrecord_shinkiroku_tsuntsun', 'perfect_nice_tsuntsun'];
const AUD = /\.(wav|ogg|mp3|m4a)$/i;
if (scope.includes('G-VOICE')) {
  const probs = [];
  for (const v of VOICES) if (!repoFiles.some(f => f.startsWith('voice/') && path.basename(f).startsWith(v) && AUD.test(f))) probs.push(`voice/ に ${v} が無い`);
  const outside = [...repoFiles, ...zipList].filter(f => AUD.test(f) && VOICES.some(v => path.basename(f).startsWith(v)) && !/(^|\/)voice\//.test(f) && !f.startsWith('harness/'));
  if (outside.length) probs.push(`voice/ の外に声 ${outside.length} 本：${outside.slice(0, 3).join(', ')}`);
  const normal = [...repoFiles, ...zipList].filter(f => /_normal\.(wav|ogg|mp3|m4a)$/i.test(f));
  if (normal.length) probs.push(`ノーマルの声が ${normal.length} 本：${normal.slice(0, 3).join(', ')}`);
  set('G-VOICE', probs.length === 0, probs.join('；') || 'ツンツン 4 本、voice/ だけ、ノーマル 0');
}
if (scope.includes('G-NOTICE')) {
  const t = read(path.join(ROOT, 'voice', 'NOTICE.md')) || '';
  const need = ['https://zunko.jp/con_ongen_kiyaku.html', 'https://voicevox.hiroshiba.jp/term/'].filter(u => !t.includes(u));
  set('G-NOTICE', t && need.length === 0, !t ? 'voice/NOTICE.md が無い' : need.length ? `URL が無い：${need.join(', ')}` : 'ok');
}
const PLACEHOLDER = /【(リリーの一覧待ち|確認待ち)/;
if (scope.includes('G-LICENSE')) {
  const lic = ['LICENSE', 'LICENSE.md', 'LICENSE.txt'].map(f => read(path.join(ROOT, f))).find(x => x !== null);
  if (lic !== undefined && lic !== null) set('G-LICENSE', /voice\//.test(lic) && /(対象外|exclud|does not apply|not covered)/i.test(lic), 'LICENSE あり：' + (/voice\//.test(lic) ? 'voice/ の記載あり' : '「voice/ は対象外」が無い'));
  else {
    const rd = read(path.join(ROOT, 'README.md')) || '', cr = read(path.join(ROOT, 'CREDITS.md')) || '';
    const miss = [];
    for (const [n, t] of [['README.md', rd], ['CREDITS.md', cr]]) { if (!t.includes('著作権はさきや')) miss.push(`${n} に「著作権はさきや」が無い`); if (!t.includes('voice/')) miss.push(`${n} に「voice/」が無い`); }
    set('G-LICENSE', miss.length === 0, 'LICENSE なし：' + (miss.join('；') || 'README.md と CREDITS.md に書き分けあり'));
  }
}
if (scope.includes('G-CREDITS')) {
  const probs = [];
  const zc = zipList.find(n => /^([^/]+\/)?CREDITS\.md$/.test(n));
  for (const [n, t] of [['リポジトリの CREDITS.md', read(path.join(ROOT, 'CREDITS.md'))], ['ZIP の CREDITS.md', zc ? read(path.join(OUT, 'build', zc)) : null]]) {
    if (t === null) { probs.push(`${n} が無い`); continue; }
    if (!t.includes('VOICEVOX:ずんだもん')) probs.push(`${n} に「VOICEVOX:ずんだもん」が無い`);
    if (!/^#+.*効果音/m.test(t)) probs.push(`${n} に「効果音」の見出しが無い`);
    if (!/^#+.*3D/m.test(t)) probs.push(`${n} に「3D」の見出しが無い`);
    if (PLACEHOLDER.test(t)) probs.push(`${n} に「【リリーの一覧待ち】／【確認待ち」が残っている`);
  }
  set('G-CREDITS', probs.length === 0, probs.join('；') || 'ok');
}
if (scope.includes('G-README')) {
  const t = read(path.join(ROOT, 'README.md'));
  const probs = [];
  if (t === null) probs.push('README.md が無い');
  else {
    if (!t.includes('VOICEVOX:ずんだもん')) probs.push('「VOICEVOX:ずんだもん」が無い');
    if (!t.includes('配信・動画では『VOICEVOX:ずんだもん』の表記をお願いします')) probs.push('「配信・動画では『VOICEVOX:ずんだもん』の表記をお願いします」が無い');
    if (PLACEHOLDER.test(t)) probs.push('「【確認待ち】／【リリーの一覧待ち】」が残っている（通治さんの答え (a)(c) 待ちなら、それを【進捗】に書く）');
  }
  set('G-README', probs.length === 0, probs.join('；') || 'ok');
}
const feelPath = buildDir ? path.join(buildDir, 'StreamingAssets', 'feel.json') : '';
const feel = feelPath ? readJson(feelPath) : null;
if (scope.includes('G-FEELJSON')) {
  const probs = [];
  if (!feel) probs.push('ZIP の StreamingAssets/feel.json が無いか JSON として読めない');
  else {
    if (feel.defaultPreset !== 'mashimashi') probs.push(`defaultPreset が "${feel.defaultPreset}"（mashimashi は固定）`);
    for (const p of ['mashimashi', 'futsuu', 'off']) if (!feel.presets || typeof feel.presets[p] !== 'object') probs.push(`presets.${p} が無い`);
    if (typeof feel.version !== 'string' || !feel.version) probs.push('version が無い');
    if (typeof feel.targetFpsMobile !== 'number') probs.push('targetFpsMobile（リリーが決めた fps の目標）が無い');
  }
  set('G-FEELJSON', probs.length === 0, probs.join('；') || `feel.json 版 ${feel.version}`);
}
if (scope.includes('G-FEEL-RECORD')) {
  const sp = read(path.join(ROOT, 'spec.md')) || '', fd = read(path.join(H, 'FEEL_DECISIONS.md')) || '';
  const v = feel && feel.version;
  const probs = [];
  if (!v) probs.push('feel.json の version が読めない');
  else {
    if (!sp.includes(`feel.json 版：${v}`)) probs.push(`spec.md に「feel.json 版：${v}」が無い`);
    const i = fd.indexOf(`## feel.json 版：${v}`);
    if (i < 0) probs.push(`FEEL_DECISIONS.md に「## feel.json 版：${v}」の見出しが無い`);
    else { const sec = fd.slice(i).split(/\n## /)[0]; if ((sec.match(/^\|[^-].*\|\s*$/mg) || []).length < 2) probs.push('FEEL_DECISIONS.md のその版の表に行が無い'); }
  }
  set('G-FEEL-RECORD', probs.length === 0, probs.join('；') || `feel.json 版 ${v}`);
}
if (scope.includes('G-VIDEO-PLAY')) {
  const f = ['playthrough.webm', 'playthrough.mp4'].map(n => path.join(EVD, n)).find(p => fs.existsSync(p));
  const meta = readJson(path.join(EVD, 'playthrough_meta.json')) || {};
  const probs = [];
  if (!lib.isDownloadLink(links.playthrough)) probs.push('links.json の "playthrough" が GitHub／Drive のリンクでない');
  if (!f || fs.statSync(f).size < 100 * 1024) probs.push('harness/evidence/playthrough.webm が無い（record_playthrough.js で作る）');
  if (meta.version !== version) probs.push(`録画の版 ${meta.version || '不明'} が今の版 ${version} と違う（録り直す）`);
  if (meta.steps && !['title', 'aim', 'flight', 'result', 'retry-aim'].every(s => meta.steps.includes(s))) probs.push('録画にタイトル→照準→飛行→結果→もう一投 がそろっていない');
  set('G-VIDEO-PLAY', probs.length === 0, probs.join('；') || links.playthrough);
}
const cmpDir = path.join(EVD, 'compare');
const cmp = Object.fromEntries(['mashimashi', 'futsuu', 'off'].map(p => [p, readJson(path.join(cmpDir, `${p}_fx.json`))]));
if (scope.includes('G-VIDEO-CMP')) {
  const probs = [];
  const inputs = ['mashimashi', 'futsuu', 'off'].map(p => read(path.join(cmpDir, `inputs_${p}.json`)));
  for (const p of ['mashimashi', 'futsuu', 'off']) {
    if (!links.compare || !lib.isDownloadLink(links.compare[p])) probs.push(`links.json の compare.${p} が GitHub／Drive のリンクでない`);
    const vf = path.join(cmpDir, `${p}.webm`);
    if (!fs.existsSync(vf) || fs.statSync(vf).size < 100 * 1024) probs.push(`compare/${p}.webm が無い`);
    if (!cmp[p]) probs.push(`compare/${p}_fx.json が無い`); else if (cmp[p].version !== version) probs.push(`${p} の動画の版 ${cmp[p].version} が今の版 ${version} と違う`);
  }
  if (inputs.some(x => x === null) || new Set(inputs).size !== 1) probs.push('3 本の入力（inputs_*.json）が同じでない');
  const m = (cmp.mashimashi && cmp.mashimashi.events) || [];
  for (const [need, test] of [['ガラス突破', e => e.event === 'glass_break'], ['物を壊す', e => e.event === 'break' || e.event === 'chain'], ['着地プッシュ', e => /^push_/.test(e.event)]]) if (!m.some(test)) probs.push(`マシマシの動画に ${need} が映っていない（記録に無い）`);
  const offFx = ((cmp.off && cmp.off.events) || []).filter(e => !['voice', 'flash_skipped'].includes(e.fx));
  if (offFx.length) probs.push(`オフの動画で演出が ${offFx.length} 個出ている`);
  if (cmp.mashimashi && cmp.futsuu && cmp.mashimashi.settings && cmp.mashimashi.settings.preset !== 'mashimashi') probs.push('マシマシの動画の設定が mashimashi でない');
  set('G-VIDEO-CMP', probs.length === 0, probs.join('；') || '3 本そろい、同じ入力');
}
if (scope.includes('G-M14-LOG')) {
  const mx = ['mashimashi', 'futsuu'].map(p => cmp[p] ? lib.maxFlashesPerSecond(cmp[p].events) : null);
  set('G-M14-LOG', mx.every(x => x !== null && x <= lib.FLASH_MAX_PER_SEC), mx.some(x => x === null) ? '比較動画の記録が無い' : `1 秒の最大：マシマシ ${mx[0]} 回、ふつう ${mx[1]} 回（上限 ${lib.FLASH_MAX_PER_SEC}）`);
}
if (scope.includes('G-FPS')) {
  const f = readJson(path.join(EVD, 'fps_mobile.json'));
  const probs = [];
  if (!f) probs.push('harness/evidence/fps_mobile.json が無い（実機スマホで ?debug=1 の「fps記録コピー」から作る）');
  else {
    if (!feel || typeof feel.targetFpsMobile !== 'number') probs.push('feel.json に targetFpsMobile が無い');
    else if (f.targetFps !== feel.targetFpsMobile) probs.push(`記録の targetFps ${f.targetFps} が feel.json の ${feel.targetFpsMobile} と違う`);
    if (!(typeof f.minFps === 'number' && f.minFps >= f.targetFps)) probs.push(`最低 ${f.minFps} fps が目標 ${f.targetFps} fps より低い`);
    if (f.preset !== 'mashimashi') probs.push('マシマシで測っていない');
    if (!Array.isArray(f.scenes) || !['glass_break', 'chain_7'].every(s => f.scenes.includes(s))) probs.push('ガラス突破と連鎖 7 の場面が含まれていない');
    if (!f.device || /headless|swiftshader|desktop/i.test(f.device)) probs.push('実機スマホの名前（device）が無い');
    if (feel && typeof feel.mobileParticleMinRatio === 'number' && !(f.particleRatio >= feel.mobileParticleMinRatio)) probs.push(`粒子を ${f.particleRatio} まで減らしている（下限 ${feel.mobileParticleMinRatio}）`);
    if (f.version !== version) probs.push(`記録の版 ${f.version} が今の版 ${version} と違う`);
  }
  set('G-FPS', probs.length === 0, probs.join('；') || `${f.device}：最低 ${f.minFps}／目標 ${f.targetFps} fps`);
}
if (scope.includes('G-CHECKLIST')) {
  const t = read(path.join(H, 'CHECKLIST.md')) || '';
  const lines = t.split('\n'); let hdr = null; const bad = []; let n = 0;
  for (const l of lines) {
    const c = l.trim().startsWith('|') ? l.trim().replace(/^\||\|$/g, '').split('|').map(s => s.trim()) : null;
    if (!c) continue;
    if (c[0] === 'ID' && c.includes('リリー')) { hdr = c; continue; }
    if (!hdr || !/^V-/.test(c[0])) continue;
    n++;
    const ri = hdr.indexOf('リリー'), ti = hdr.findIndex(x => x.startsWith('秒'));
    if (c[ri] !== '◯' || !c[ti] || c[ti] === '—' || c[ti] === '') bad.push(c[0]);
  }
  set('G-CHECKLIST', n > 0 && bad.length === 0, n === 0 ? 'CHECKLIST.md の表が読めない' : bad.length ? `リリーの ◯ か秒数が無い：${bad.join(', ')}` : `${n} 行すべて ◯`);
}

// ---- 出力 ----
const prev = readJson(path.join(OUT, 'attempts.json')) || {};
const rows = scope.map(id => ({ id, ...(results[id] || { ok: false, detail: 'チェックが走らなかった' }) }));
const att = { ...prev };
for (const r of rows) att[r.id] = r.ok ? 0 : (prev[r.id] || 0) + 1;
fs.writeFileSync(path.join(OUT, 'attempts.json'), JSON.stringify(att, null, 1));
const fails = rows.filter(r => !r.ok);
const lines = [
  '=== KANI GATE ===',
  `TARGET: ${target}`,
  `SCOPE-ITEMS: ${scopeItems.join(', ')}`,
  `RUN-AT: ${runAt}`,
  `VERSION: ${version || '(なし)'}`,
  `ZIP: ${path.relative(ROOT, zipPath)}`,
  `ZIP-SHA256: ${zipSha || '(なし)'}`,
  `ZIP-URL: ${links.zip || '(なし)'}`,
  `GIT-COMMIT: ${gitHead || '(なし)'}`,
  `GIT-DIRTY: ${dirtyLines.length ? 'yes' : 'no'}`,
  '| チェック | 中身 | 対応 | 結果 | くわしく |',
  '|---|---|---|---|---|',
  ...rows.map(r => `| ${r.id} | ${(LABEL[r.id] || [''])[0]} | ${(LABEL[r.id] || ['', ''])[1]} | ${r.ok ? '◯' : '✕'} | ${r.detail.replace(/\|/g, '／')} |`),
  `CHECKS: ${rows.length} ran, ${rows.length - fails.length} ◯, ${fails.length} ✕`,
  `NOT-IN-SCOPE: ${allIds.filter(i => !scope.includes(i)).join(', ') || 'なし'}`,
];
const third = fails.filter(r => att[r.id] >= 3);
if (third.length) lines.push(`STOP-AND-REPORT: ${third.map(r => `${r.id}（${att[r.id]} 回続けて ✕）`).join(', ')} → 同じ直しを 3 回試して直らない。原因と必要なものを #cloe-dot でマツリに 1 行で報告してから続ける`);
lines.push(fails.length ? `GATE-RESULT: FAIL ${fails.length}` : 'GATE-RESULT: ALL-PASS');
const text = lines.join('\n');
fs.writeFileSync(path.join(OUT, `gate_${target}.txt`), text + '\n');
console.log(text);
console.log(fails.length ? `\n✕ があるので【提出】できない。直して、もう一度 bash harness/scripts/gate.sh ${target}` : `\n全部 ◯。次：node harness/scripts/plan.js submit ${target}`);
process.exit(fails.length ? 1 : 0);
