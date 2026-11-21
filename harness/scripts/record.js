#!/usr/bin/env node
'use strict';
// 動画を「台本どおりの同じ入力」で撮る。人の手で撮らない（同じ入力にするため）。
//   node harness/scripts/record.js playthrough   … 通し動画（タイトル→照準→飛行→結果→もう一投）＝ T02（M4）
//   node harness/scripts/record.js compare       … 比較動画 3 本（マシマシ／ふつう／オフ、同じ入力）＝ F10（M13・M14）
// 先に bash harness/scripts/gate.sh <ID> を 1 回走らせておく（ZIP が harness/out/build に解かれる）。
// 別の場所のビルドを撮るなら KANI_BUILD_DIR=<index.html のあるフォルダ>。
// 撮れたら：動画を GitHub Release か Google Drive に上げ、リンクを harness/evidence/links.json に書く。
const fs = require('fs');
const path = require('path');
const cp = require('child_process');
const { chromium } = require('@playwright/test');
const K = require('../tests/kani-helpers');

const H = path.resolve(__dirname, '..');
const ROOT = path.resolve(H, '..');
const EVD = path.join(H, 'evidence');
const mode = process.argv[2];
if (!['playthrough', 'compare'].includes(mode)) { console.error('使い方：node harness/scripts/record.js playthrough|compare'); process.exit(2); }
const version = (fs.readFileSync(path.join(ROOT, 'VERSION'), 'utf8').split('\n')[0] || '').trim();
let buildDir = process.env.KANI_BUILD_DIR;
if (!buildDir) {
  const b = path.join(H, 'out', 'build');
  const find = d => fs.existsSync(path.join(d, 'index.html')) ? d : (fs.existsSync(d) ? fs.readdirSync(d).map(x => path.join(d, x)).filter(x => fs.statSync(x).isDirectory()).map(find).find(Boolean) : null);
  buildDir = find(b);
}
if (!buildDir) { console.error('✕ ビルドが見つからない。先に gate.sh を走らせるか KANI_BUILD_DIR を渡す'); process.exit(2); }
const PORT = Number(process.env.KANI_REC_PORT || 8011);
const BASE = `http://127.0.0.1:${PORT}`;
// 同じ入力（3 本とも、毎回この値）。変えるときは 3 本とも撮り直す。
const INPUT = { power: 100, angle: 40, direction: 1, viewport: { width: 1280, height: 720 }, url: '/?test=1' };
const ARGS = ['--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--ignore-gpu-blocklist'];

(async () => {
  const srv = cp.spawn('python3', ['-m', 'http.server', String(PORT), '--bind', '127.0.0.1'], { cwd: buildDir, stdio: 'ignore' });
  await new Promise(r => setTimeout(r, 1500));
  const browser = await chromium.launch({ headless: process.env.KANI_HEADED !== '1', args: ARGS });
  try {
    if (mode === 'playthrough') {
      const dir = path.join(EVD, 'tmp_play'); fs.rmSync(dir, { recursive: true, force: true });
      const ctx = await browser.newContext({ viewport: INPUT.viewport, baseURL: BASE, recordVideo: { dir, size: INPUT.viewport } });
      const page = await ctx.newPage();
      const steps = [];
      await K.boot(page, { url: '/', hook: false }); steps.push('title');
      await K.toAim(page); steps.push('aim');
      await page.evaluate(([p, a, d]) => { config.power = p; config.angle = a; config.direction = d; configure(); }, [INPUT.power, INPUT.angle, INPUT.direction]); // eslint-disable-line no-undef
      const r = await K.launchAndWait(page, { onFlight: () => steps.push('flight') });
      if (r.phase === 'result') steps.push('result');
      await page.waitForTimeout(2500);
      await page.click('#retry');
      await page.waitForFunction(() => document.body.dataset.view === 'aim', null, { timeout: 30000 }); steps.push('retry-aim');
      await page.waitForTimeout(1500);
      const v = page.video(); await ctx.close();
      fs.renameSync(await v.path(), path.join(EVD, 'playthrough.webm'));
      fs.rmSync(dir, { recursive: true, force: true });
      fs.writeFileSync(path.join(EVD, 'playthrough_meta.json'), JSON.stringify({ version, steps, result: r, input: INPUT, recordedAt: K.lib.jstNow().full }, null, 1));
      console.log(`◯ harness/evidence/playthrough.webm（${steps.join('→')}）。上げてリンクを links.json の "playthrough" に書く`);
    } else {
      const cd = path.join(EVD, 'compare'); fs.mkdirSync(cd, { recursive: true });
      for (const preset of ['mashimashi', 'futsuu', 'off']) {
        const dir = path.join(cd, 'tmp_' + preset); fs.rmSync(dir, { recursive: true, force: true });
        const ctx = await browser.newContext({ viewport: INPUT.viewport, baseURL: BASE, recordVideo: { dir, size: INPUT.viewport } });
        const page = await ctx.newPage();
        await K.boot(page, { url: INPUT.url });
        await K.setSettings(page, { preset, shake: true });
        await K.toAim(page);
        await page.evaluate(() => window.__kaniTest.clear());
        await page.evaluate(([p, a, d]) => { config.power = p; config.angle = a; config.direction = d; configure(); }, [INPUT.power, INPUT.angle, INPUT.direction]); // eslint-disable-line no-undef
        const r = await K.launchAndWait(page);
        await page.waitForTimeout(3000);
        const log = await page.evaluate(() => ({ events: JSON.parse(JSON.stringify(window.__kaniTest.events)), settings: window.__kaniTest.settings(), version: window.__kaniTest.version }));
        const v = page.video(); await ctx.close();
        fs.renameSync(await v.path(), path.join(cd, `${preset}.webm`));
        fs.rmSync(dir, { recursive: true, force: true });
        fs.writeFileSync(path.join(cd, `${preset}_fx.json`), JSON.stringify({ version, hookVersion: log.version, settings: log.settings, result: r, events: log.events, recordedAt: K.lib.jstNow().full }, null, 1));
        fs.writeFileSync(path.join(cd, `inputs_${preset}.json`), JSON.stringify(INPUT, null, 1));
        const kinds = [...new Set(log.events.map(e => e.event))];
        console.log(`◯ compare/${preset}.webm：距離 ${r.d} m、出来事 ${kinds.join(', ')}`);
      }
      console.log('3 本を上げて、links.json の compare.mashimashi / futsuu / off に書く。CHECKLIST.md に秒数を書く');
    }
  } finally { await browser.close(); srv.kill(); }
})().catch(e => { console.error('✕ ' + e.message); process.exit(1); });
