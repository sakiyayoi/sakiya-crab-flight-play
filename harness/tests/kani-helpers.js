'use strict';
// テストで使う部品。ゲームの名前（DOM の id・変数）はここにまとめてある。
// ここに書いた名前は r1（host.js）で確かめたもの：body[data-view]、#start #launchZone #retry #prepare #titleOptions #kick、
// トップレベルの lastState・config・configure()、launchZone の Enter（未チャージ＝チャージ、チャージ後＝発射）。
// 今の版で名前が変わっていたら、ゲームではなく「このファイルだけ」を直し、【提出】の「変えた値」に書く。
const fs = require('fs');
const path = require('path');
const lib = require('../scripts/lib');

const OUT = path.resolve(__dirname, '..', 'out');
fs.mkdirSync(OUT, { recursive: true });
const TEST_URL = '/?test=1';

function watchErrors(page, bag) {
  page.on('console', m => { if (m.type() === 'error') bag.push('console.error: ' + m.text()); });
  page.on('pageerror', e => bag.push('pageerror: ' + e.message));
  page.on('requestfailed', r => bag.push('requestfailed: ' + r.url() + ' ' + ((r.failure() && r.failure().errorText) || '')));
  page.on('response', r => { if (r.status() >= 400) bag.push(`http ${r.status()}: ${r.url()}`); });
}

async function boot(page, { url = TEST_URL, hook = true } = {}) {
  await page.goto(url);
  await page.waitForFunction(() => document.body.dataset.view === 'title', null, { timeout: 180000 });
  if (hook) await page.waitForFunction(() => !!(window.__kaniTest && window.__kaniTest.ready === true), null, { timeout: 30000 });
}

async function toAim(page) {
  await page.click('#start');
  await page.waitForFunction(() => document.body.dataset.view === 'aim', null, { timeout: 30000 });
}

// 正規のキーボード経路で 1 投（元の harness-for-lily と同じ手順）
async function throwOnce(page, { power = 70, angle = 40, direction = 1 } = {}) {
  if (await page.evaluate(() => document.body.dataset.view) === 'result') {
    await page.click('#retry');
    await page.waitForFunction(() => document.body.dataset.view === 'aim');
  }
  await page.evaluate(([p, a, d]) => { config.power = p; config.angle = a; config.direction = d; configure(); }, [power, angle, direction]); // eslint-disable-line no-undef
  return launchAndWait(page);
}
async function launchAndWait(page, { onFlight } = {}) {
  await page.focus('#launchZone');
  const t0 = Date.now();
  while (!(await page.evaluate(() => !!(lastState && lastState.chargeReady)))) { // eslint-disable-line no-undef
    if (Date.now() - t0 > 120000) return { fault: 'charge-timeout' };
    await page.keyboard.press('Enter');
    await page.waitForTimeout(150);
  }
  await page.keyboard.press('Enter');
  const t1 = Date.now();
  let s;
  if (onFlight) await onFlight(t1);
  do {
    await page.waitForTimeout(200);
    s = await page.evaluate(() => ({ phase: lastState.phase, d: lastState.distance, f: lastState.physicsFault, t: lastState.time, pw: lastState.power })); // eslint-disable-line no-undef
    if (Date.now() - t1 > 300000) return { ...s, fault: 'no-result-300s' };
  } while (s.phase !== 'result');
  return { ...s, fault: s.f || (s.d < 5 ? 'short-stop' : '') };
}

// 演出だけを順に出す（TEST_HOOKS.md の fire）。点滅の数え役に間引かれないよう 1.2 秒あける。
const FIRE_ALL = [
  ['charge_stroke', {}], ['charge_ready', {}], ['launch', {}], ['flight_fast', {}],
  ['glass_break', {}], ['break', { material: 'wood' }], ['chain', { chain: 2 }], ['chain', { chain: 4 }], ['chain', { chain: 7 }],
  ['ground_hit', { part: 'shell' }], ['push_perfect', {}], ['push_good', {}], ['push_miss', {}], ['new_record', {}], ['result', {}]
];
async function fireAll(page, gapMs = 1200) {
  if(await page.locator('#sound').getAttribute('aria-checked')==='false')await page.click('#sound');await page.waitForFunction(()=>window.FlightVoice.debug().ready,null,{timeout:30000});
  await page.evaluate(() => window.__kaniTest.clear());
  for (const [ev, opts] of FIRE_ALL) {
    await page.evaluate(([e, o]) => window.__kaniTest.fire(e, o), [ev, opts]);
    await page.waitForTimeout(gapMs);
  }
  await page.waitForTimeout(1500);
  return page.evaluate(() => ({ events: JSON.parse(JSON.stringify(window.__kaniTest.events)), camMax: window.__kaniTest.cameraMaxOffset(), settings: window.__kaniTest.settings() }));
}
async function setSettings(page, s) {
  await page.evaluate(x => window.__kaniTest.setSettings(x), s);
  await page.waitForFunction(x => { const c = window.__kaniTest.settings(); return Object.keys(x).every(k => c[k] === x[k]); }, s, { timeout: 10000 });
}

// 記録の読み方
const byOcc = events => { const m = new Map(); for (const e of events) { if (!m.has(e.occ)) m.set(e.occ, []); m.get(e.occ).push(e); } return m; };
const of = (events, ev, fx, extra = () => true) => events.filter(e => e.event === ev && e.fx === fx && extra(e));
const eventKey = e => e.event === 'chain' ? `chain_${e.chain}` : e.event;
function save(name, data) { fs.writeFileSync(path.join(OUT, name), JSON.stringify(data, null, 1)); }

module.exports = { OUT, TEST_URL, watchErrors, boot, toAim, throwOnce, launchAndWait, FIRE_ALL, fireAll, setSettings, byOcc, of, eventKey, save, lib };
