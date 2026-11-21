'use strict';
// 手触り（5-1・5-2・5-2b）と安全（M14・M15）のテスト。テスト名の先頭 [G-…] がゲートのチェック名。
// 値そのもの（何フレーム・何 m）はリリーが決めるので、ここでは決め打ちしない。見るのは：
//   ・固定の 4 つ（①〜⑦ が出る／点滅 1 秒 3 回まで・揺れオフと視差効果を減らすで揺れ 0／初期マシマシ）
//   ・spec の決まりの関係（ガラス突破が一番派手、マシマシ＞ふつう、ヒットストップの後にスロー、timeScale は 0 にしない）
//   ・実際に出た値が feel.json（resolvedFeel）と同じ（直書きしていない）
// 前提：TEST_HOOKS.md の window.__kaniTest（?test=1 のとき記録する）。
const { test, expect } = require('@playwright/test');
const h = require('./kani-helpers');
const lib = h.lib;
const WHITE = '#FFFFFF';
const isWhite = c => String(c).toUpperCase() === WHITE;
const sum = (a, f) => a.reduce((s, x) => s + (Number(f(x)) || 0), 0);

test('[G-F-HOOK] テスト用フック window.__kaniTest がそろっている', async ({ page }) => {
  const errs = []; h.watchErrors(page, errs);
  await h.boot(page);
  const t = await page.evaluate(() => {
    const k = window.__kaniTest; const f = n => typeof k[n] === 'function';
    return { ready: k.ready, version: k.version, eventsIsArray: Array.isArray(k.events), fns: ['clear', 'settings', 'setSettings', 'resolvedFeel', 'fire', 'cameraMaxOffset', 'fps'].filter(n => !f(n)) };
  });
  expect(t.fns, `関数が無い：${t.fns.join(', ')}`).toEqual([]);
  expect(t.eventsIsArray, 'events が配列でない').toBeTruthy();
  expect(t.version, 'version が VERSION ファイルと違う').toBe(process.env.KANI_VERSION);
  await page.evaluate(() => { window.__kaniTest.clear(); window.__kaniTest.fire('launch', {}); });
  await page.waitForTimeout(1500);
  const ev = await page.evaluate(() => JSON.parse(JSON.stringify(window.__kaniTest.events)));
  expect(ev.length, 'fire("launch") のあと events が空').toBeGreaterThan(0);
  for (const e of ev) {
    expect(typeof e.t === 'number' && typeof e.event === 'string' && typeof e.fx === 'string' && Number.isInteger(e.occ), `形が違う記録：${JSON.stringify(e)}`).toBeTruthy();
  }
  const fps = await page.evaluate(() => window.__kaniTest.fps());
  expect(typeof fps.now === 'number' && typeof fps.min === 'number', 'fps() が {now, min, …} を返さない').toBeTruthy();
  expect(errs).toEqual([]);
});

test('[G-F-DEF] 何も保存していないブラウザでは 演出：マシマシ・画面揺れ：オン（固定）', async ({ browser }) => {
  const ctx = await browser.newContext();
  const page = await ctx.newPage();
  await h.boot(page);
  const s = await page.evaluate(() => window.__kaniTest.settings());
  expect(s.preset, '初期の演出').toBe('mashimashi');
  expect(s.shake, '初期の画面揺れ').toBe(true);
  await page.click('#titleOptions');
  await expect(page.locator('#fxPreset [data-value="mashimashi"]')).toHaveAttribute('aria-checked', 'true');
  await expect(page.locator('#shakeToggle')).toHaveAttribute('aria-checked', 'true');
  await ctx.close();
});

test('[G-F-UI] 設定は 2 項目（演出 3 つ／画面揺れ オン・オフ）、再読み込み後も残る、旧「演出の動き」が無い', async ({ page }) => {
  await h.boot(page);
  await page.click('#titleOptions');
  for (const [v, label] of [['mashimashi', 'マシマシ'], ['futsuu', 'ふつう'], ['off', 'オフ']]) {
    const b = page.locator(`#fxPreset [data-value="${v}"]`);
    await expect(b, `#fxPreset に ${v} が無い`).toBeVisible();
    expect((await b.innerText()).includes(label), `${v} の文字が「${label}」でない`).toBeTruthy();
  }
  await expect(page.locator('#shakeToggle')).toHaveAttribute('role', 'switch');
  expect((await page.locator('#shakeToggle').innerText()).includes('画面揺れ'), '#shakeToggle に「画面揺れ」の文字が無い').toBeTruthy();
  await page.click('#fxPreset [data-value="futsuu"]');
  if (await page.locator('#shakeToggle').getAttribute('aria-checked') === 'true') await page.click('#shakeToggle');
  let s = await page.evaluate(() => window.__kaniTest.settings());
  expect([s.preset, s.shake]).toEqual(['futsuu', false]);
  await page.reload();
  await h.boot(page);
  s = await page.evaluate(() => window.__kaniTest.settings());
  expect([s.preset, s.shake], '再読み込みで設定が消えた').toEqual(['futsuu', false]);
  const body = await page.evaluate(() => document.body.innerText);
  expect(body.includes('演出の動き'), '旧項目「演出の動き」が残っている（K1）').toBeFalsy();
  await h.setSettings(page, { preset: 'mashimashi', shake: true });
});

test.describe.serial('マシマシ・ふつうで全部の出来事を出した記録', () => {
  const L = {};
  test.beforeAll(async ({ browser }) => {
    const ctx = await browser.newContext({ viewport: { width: 1280, height: 800 } });
    const page = await ctx.newPage();
    await h.boot(page);
    for (const preset of ['mashimashi', 'futsuu']) {
      await h.setSettings(page, { preset, shake: true });
      const r = await h.fireAll(page);
      r.resolved = await page.evaluate(p => JSON.parse(JSON.stringify(window.__kaniTest.resolvedFeel(p))), preset);
      L[preset] = r;
      h.save(`feel_${preset}.json`, r);
    }
    await ctx.close();
  });
  const M = () => L.mashimashi.events;
  const F = () => L.futsuu.events;

  test('[G-F-M1] ① 衝撃波リング：破壊・ガラス突破・PERFECT で出る', async () => {
    for (const ev of ['glass_break', 'break', 'push_perfect']) expect(h.of(M(), ev, 'shockwave').length, `${ev} で shockwave が無い`).toBeGreaterThan(0);
  });
  test('[G-F-M2] ② スピードライン：発射〜高速飛行で出る', async () => {
    expect([...h.of(M(), 'launch', 'speedlines'), ...h.of(M(), 'flight_fast', 'speedlines')].filter(e => e.on === true).length, 'speedlines on が無い').toBeGreaterThan(0);
  });
  test('[G-F-M3] ③ 壊した数のポップ：破壊で +1、連鎖 n で +n', async () => {
    expect(h.of(M(), 'break', 'popup_number').some(e => e.value >= 1), 'break で popup_number が無い').toBeTruthy();
    for (const n of [2, 4, 7]) expect(h.of(M(), 'chain', 'popup_number', e => e.chain === n).some(e => e.value === n), `連鎖 ${n} で +${n} が無い`).toBeTruthy();
  });
  test('[G-F-M4] ④ 色つきフラッシュ：破壊・ガラス突破で白以外の色が出る', async () => {
    const fl = [...h.of(M(), 'break', 'flash'), ...h.of(M(), 'glass_break', 'flash')];
    expect(fl.some(e => Array.isArray(e.colors) && e.colors.some(c => !isWhite(c))), '色つき（白以外）のフラッシュが無い').toBeTruthy();
  });
  test('[G-F-M5] ⑤ 高速時のカニの残像', async () => {
    expect(h.of(M(), 'flight_fast', 'afterimage').filter(e => e.on === true).length, 'afterimage on が無い').toBeGreaterThan(0);
  });
  test('[G-F-M6] ⑥ 連鎖ごとに大きくなる吹き出し（2→4→7 で大きくなる）', async () => {
    const ct = n => h.of(M(), 'chain', 'chain_text', e => e.chain === n)[0];
    const [a, b, c] = [ct(2), ct(4), ct(7)];
    expect(a && b && c, '連鎖 2／4／7 のどれかで chain_text が無い').toBeTruthy();
    expect(a.peak < b.peak && b.peak < c.peak, `ピークが大きくならない：${a.peak}, ${b.peak}, ${c.peak}`).toBeTruthy();
    expect(a.settle < b.settle && b.settle < c.settle, `落ち着きが大きくならない：${a.settle}, ${b.settle}, ${c.settle}`).toBeTruthy();
  });
  test('[G-F-M7] ⑦ ガラス突破：ヒットストップの後にスロー（重ねない）＋ずんだもんの声', async () => {
    const hs = h.of(M(), 'glass_break', 'hitstop')[0], sl = h.of(M(), 'glass_break', 'slowmo')[0];
    expect(hs && sl, 'glass_break で hitstop か slowmo が無い').toBeTruthy();
    expect(sl.seconds > 0 && sl.timeScale > 0 && sl.timeScale < 1, `slowmo の値がおかしい：${JSON.stringify(sl)}`).toBeTruthy();
    expect(sl.t >= hs.t + hs.frames * 16.7 * 0.5, 'スローがヒットストップと重なっている（スローは後）').toBeTruthy();
    expect(h.of(M(), 'glass_break', 'voice').some(e => /glassbreak_kani/.test(e.id)), 'ガラス突破で声 glassbreak_kani が鳴らない').toBeTruthy();
  });
  test('[G-CR-VOICEPLAY] 声：ガラス突破と新記録でツンツンが鳴る、ノーマルは鳴らない', async () => {
    expect(h.of(M(), 'glass_break', 'voice').some(e => e.id === 'glassbreak_kani_tsuntsun'), 'glassbreak_kani_tsuntsun が無い').toBeTruthy();
    expect(h.of(M(), 'new_record', 'voice').some(e => e.id === 'newrecord_shinkiroku_tsuntsun'), 'newrecord_shinkiroku_tsuntsun が無い').toBeTruthy();
    expect([...M(), ...F()].filter(e => e.fx === 'voice' && /_normal/.test(e.id)), 'ノーマルの声が鳴った').toEqual([]);
  });
  test('[G-F-MATCH] 実際に出た値が feel.json（resolvedFeel）と同じ（直書きしていない）', async () => {
    const bad = [];
    let compared = 0;
    for (const preset of ['mashimashi', 'futsuu']) {
      const R = L[preset].resolved || {};
      for (const k of ['launch', 'glass_break', 'break', 'chain_7', 'push_perfect']) if (!R[k]) bad.push(`${preset}: resolvedFeel に ${k} が無い`);
      for (const [, list] of h.byOcc(L[preset].events)) {
        const key = h.eventKey(list[0]); const r = R[key]; if (!r) continue;
        const hs = list.find(e => e.fx === 'hitstop');
        if (hs && typeof r.hitstopFrames === 'number') { compared++; if (hs.frames !== r.hitstopFrames) bad.push(`${preset} ${key} hitstop ${hs.frames}≠${r.hitstopFrames}`); }
        const sh = list.find(e => e.fx === 'shake');
        if (sh && typeof r.shakeAmp === 'number') { compared++; if (Math.abs(sh.amp - r.shakeAmp) > 1e-6) bad.push(`${preset} ${key} shake ${sh.amp}≠${r.shakeAmp}`); }
        const pc = sum(list.filter(e => e.fx === 'particles'), e => e.count);
        if (r.particles !== undefined && list.some(e => e.fx === 'particles')) {
          compared++;
          const ok = Array.isArray(r.particles) ? pc >= r.particles[0] && pc <= r.particles[1] : pc === r.particles;
          if (!ok) bad.push(`${preset} ${key} particles ${pc}≠${JSON.stringify(r.particles)}`);
        }
      }
    }
    expect(compared, '比べられる値が 1 つも無い').toBeGreaterThan(0);
    expect(bad).toEqual([]);
  });
  test('[G-F-STRONGER] マシマシはふつうより強い（揺れ・粒子の合計が大きい、ヒットストップは同じか長い）', async () => {
    const shake = ev => sum(ev.filter(e => e.fx === 'shake'), e => e.amp);
    const parts = ev => sum(ev.filter(e => e.fx === 'particles'), e => e.count);
    const hsMax = ev => Math.max(0, ...ev.filter(e => e.fx === 'hitstop').map(e => e.frames));
    expect(shake(M()), '揺れの合計').toBeGreaterThan(shake(F()));
    expect(parts(M()), '粒子の合計').toBeGreaterThan(parts(F()));
    expect(hsMax(M()), 'ヒットストップの最長').toBeGreaterThanOrEqual(hsMax(F()));
  });
  test('[G-F-GLASS] ガラス突破が一番派手（ヒットストップ最長、揺れ・粒子が一番大きい）（4-4）', async () => {
    for (const preset of ['mashimashi', 'futsuu']) {
      const occ = [...h.byOcc(L[preset].events).values()];
      const g = occ.find(l => l[0].event === 'glass_break'); expect(g, `${preset}: glass_break の記録が無い`).toBeTruthy();
      const others = occ.filter(l => l[0].event !== 'glass_break');
      const hf = l => Math.max(0, ...l.filter(e => e.fx === 'hitstop').map(e => e.frames));
      const sa = l => Math.max(0, ...l.filter(e => e.fx === 'shake').map(e => e.amp));
      const pc = l => sum(l.filter(e => e.fx === 'particles'), e => e.count);
      expect(hf(g), `${preset}: ガラス突破のヒットストップが一番長くない`).toBeGreaterThan(Math.max(0, ...others.map(hf)));
      expect(sa(g), `${preset}: ガラス突破の揺れが一番大きくない`).toBeGreaterThanOrEqual(Math.max(0, ...others.map(sa)));
      expect(pc(g), `${preset}: ガラス突破の粒子が一番多くない`).toBeGreaterThanOrEqual(Math.max(0, ...others.map(pc)));
    }
  });
  test('[G-F-FUTSUU] ふつうで C1〜C6・C8 が出る', async () => {
    const f = F(), miss = [];
    if (!h.of(f, 'break', 'flash').some(e => e.target === 'object' && (e.colors || []).every(isWhite))) miss.push('C1 壊した物が白く光る（flash target=object, 白）');
    if (!h.of(f, 'break', 'hitstop').some(e => e.frames > 0)) miss.push('C2 破壊のヒットストップ');
    if (!h.of(f, 'break', 'shake').some(e => e.amp > 0 && e.decayMs > 0)) miss.push('C3 破壊の揺れ（減衰つき）');
    if (!f.some(e => e.fx === 'squash' && e.pct > 0)) miss.push('C4 伸び縮み');
    if (!h.of(f, 'break', 'particles').some(e => e.count > 0)) miss.push('C5 破片');
    if (!f.some(e => e.fx === 'debris_keep' && e.seconds > 0)) miss.push('C5 残骸が残る（debris_keep）');
    if (!h.of(f, 'break', 'knockback').some(e => e.dist > 0)) miss.push('C6 ノックバック');
    if (!f.some(e => e.fx === 'countup' && e.ms > 0)) miss.push('C8 結果のカウントアップ');
    expect(miss, '足りない').toEqual([]);
  });
  test('[G-F-TS] ヒットストップ・スローの timeScale は 0 にしない', async () => {
    const z = [...M(), ...F()].filter(e => (e.fx === 'hitstop' || e.fx === 'slowmo') && !(e.timeScale > 0));
    expect(z, 'timeScale が 0 以下').toEqual([]);
  });
});

test('[G-S-FLASH] 点滅はどの 1 秒も 3 回まで（白・色つきを数える。重なっても）', async ({ page }) => {
  await h.boot(page);
  await h.setSettings(page, { preset: 'mashimashi', shake: true });
  await page.evaluate(() => window.__kaniTest.clear());
  const burst = [['glass_break', {}], ['chain', { chain: 7 }], ['break', {}], ['break', {}], ['break', {}], ['break', {}], ['break', {}], ['push_perfect', {}], ['new_record', {}]];
  for (const [e, o] of burst) { await page.evaluate(([a, b]) => window.__kaniTest.fire(a, b), [e, o]); await page.waitForTimeout(80); }
  await page.waitForTimeout(3000);
  const ev = await page.evaluate(() => JSON.parse(JSON.stringify(window.__kaniTest.events)));
  h.save('flash_burst.json', ev);
  expect(lib.flashEntries(ev).length, 'フラッシュが 1 回も出ていない（数え役が全部消している？）').toBeGreaterThan(0);
  expect(lib.maxFlashesPerSecond(ev), '1 秒の中のフラッシュの数').toBeLessThanOrEqual(lib.FLASH_MAX_PER_SEC);
});

test('[G-S-SHAKEOFF] 画面揺れ：オフ（マシマシのまま）で揺れ・カメラの蹴り 0、再読み込み後もオフ', async ({ page }) => {
  await h.boot(page);
  await h.setSettings(page, { preset: 'mashimashi', shake: false });
  await page.reload();
  await h.boot(page);
  expect((await page.evaluate(() => window.__kaniTest.settings())).shake, '再読み込みで揺れオフが消えた').toBe(false);
  const r = await h.fireAll(page);
  h.save('feel_shakeoff.json', r);
  expect(r.events.filter(e => e.fx === 'shake' && (e.amp !== 0 || (e.kick || 0) !== 0)), '揺れ・蹴りが 0 でない').toEqual([]);
  expect(r.camMax, 'カメラが動いた（cameraMaxOffset）').toBe(0);
  await h.setSettings(page, { preset: 'mashimashi', shake: true });
});

test('[G-S-REDUCED] OS の「視差効果を減らす」オンで揺れ 0・点滅なし', async ({ browser }) => {
  const ctx = await browser.newContext({ reducedMotion: 'reduce' });
  const page = await ctx.newPage();
  await h.boot(page);
  expect((await page.evaluate(() => window.__kaniTest.settings())).reducedMotion, 'reducedMotion を読めていない').toBe(true);
  const r = await h.fireAll(page);
  h.save('feel_reduced.json', r);
  expect(r.events.filter(e => e.fx === 'shake' && (e.amp !== 0 || (e.kick || 0) !== 0)), '揺れ・蹴りが 0 でない').toEqual([]);
  expect(r.camMax, 'カメラが動いた').toBe(0);
  expect(lib.flashEntries(r.events), '点滅が出た').toEqual([]);
  await ctx.close();
});

test('[G-F-OFF] 演出：オフで演出が全部消える（声は残してよい）', async ({ page }) => {
  await h.boot(page);
  await h.setSettings(page, { preset: 'off' });
  const r = await h.fireAll(page);
  h.save('feel_off.json', r);
  expect(r.events.filter(e => !['voice', 'flash_skipped'].includes(e.fx)), 'オフなのに出た演出').toEqual([]);
  expect(r.camMax).toBe(0);
  await h.setSettings(page, { preset: 'mashimashi', shake: true });
});

test('[G-F-REAL] 実際の 1 投（マシマシ・強 100・40°）：ガラス突破・破壊・②・⑤ が出て、点滅は 1 秒 3 回まで', async ({ page }) => {
  const errs = []; h.watchErrors(page, errs);
  await h.boot(page);
  await h.setSettings(page, { preset: 'mashimashi', shake: true });
  await h.toAim(page);
  await page.evaluate(() => window.__kaniTest.clear());
  const r = await h.throwOnce(page, { power: 100, angle: 40, direction: 1 });
  const ev = await page.evaluate(() => JSON.parse(JSON.stringify(window.__kaniTest.events)));
  h.save('feel_real.json', { result: r, events: ev });
  expect(r.phase).toBe('result');
  expect(ev.some(e => e.event === 'glass_break'), 'ガラス突破の記録が無い').toBeTruthy();
  expect(ev.some(e => e.event === 'break' || e.event === 'chain'), '破壊の記録が無い').toBeTruthy();
  expect(ev.some(e => e.fx === 'speedlines' && e.on), '② スピードラインが出ない').toBeTruthy();
  expect(ev.some(e => e.fx === 'afterimage' && e.on), '⑤ 残像が出ない').toBeTruthy();
  expect(lib.maxFlashesPerSecond(ev), '1 秒の中のフラッシュの数').toBeLessThanOrEqual(lib.FLASH_MAX_PER_SEC);
  expect(ev.filter(e => (e.fx === 'hitstop' || e.fx === 'slowmo') && !(e.timeScale > 0))).toEqual([]);
  expect(errs).toEqual([]);
});
