'use strict';
// 通し版・不具合・クレジット画面のテスト。テスト名の先頭 [G-…] がゲートのチェック名。
const { test, expect } = require('@playwright/test');
const h = require('./kani-helpers');

for (const vp of [{ name: 'PC 1280x800', viewport: { width: 1280, height: 800 } },
                  { name: 'スマホ 390x844', viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true }]) {
  test(`[G-T-START] ${vp.name}：タイトル→照準まで行け、スワイプ枠が画面の中`, async ({ browser }) => {
    const ctx = await browser.newContext({ viewport: vp.viewport, isMobile: !!vp.isMobile, hasTouch: !!vp.hasTouch });
    const page = await ctx.newPage(); const errs = []; h.watchErrors(page, errs);
    await h.boot(page, { url: '/', hook: false });
    await h.toAim(page);
    const box = await page.locator('#launchZone').boundingBox();
    expect(box, '#launchZone が見つからない').toBeTruthy();
    expect(box.x >= 0 && box.y >= 0 && box.x + box.width <= vp.viewport.width && box.y + box.height <= vp.viewport.height, `スワイプ枠が画面の外：${JSON.stringify(box)}`).toBeTruthy();
    expect(errs, 'エラーが出た').toEqual([]);
    await ctx.close();
  });
}

test('[G-T-3RUNS] 3 回続けて結果まで（もう一投・同じまま再投を押す）', async ({ page }) => {
  const errs = []; h.watchErrors(page, errs);
  await h.boot(page, { url: '/', hook: false });
  await h.toAim(page);
  const runs = [];
  runs.push(await h.throwOnce(page, { power: 70, angle: 40, direction: 1 }));
  await page.click('#retry'); // もう一投
  await page.waitForFunction(() => document.body.dataset.view === 'aim', null, { timeout: 30000 });
  runs.push(await h.throwOnce(page, { power: 70, angle: 55, direction: -1 }));
  await page.click('#prepare'); // 同じまま再投
  await page.waitForFunction(() => document.body.dataset.view !== 'result', null, { timeout: 30000 });
  if (await page.evaluate(() => document.body.dataset.view) === 'aim') runs.push(await h.launchAndWait(page));
  else {
    await page.waitForFunction(() => document.body.dataset.view === 'result', null, { timeout: 300000 });
    runs.push(await page.evaluate(() => ({ phase: lastState.phase, d: lastState.distance, f: lastState.physicsFault, fault: lastState.physicsFault || (lastState.distance < 5 ? 'short-stop' : '') }))); // eslint-disable-line no-undef
  }
  h.save('flow_3runs.json', runs);
  expect(runs.length).toBe(3);
  for (const [i, r] of runs.entries()) {
    expect(r.phase, `${i + 1} 回目が結果まで行かない`).toBe('result');
    expect(r.fault || '', `${i + 1} 回目：${r.fault}`).toBe('');
  }
  expect(errs, 'エラーが出た').toEqual([]);
});

test('[G-T-ERR] 起動から 1 投して結果までエラー 0 件', async ({ page }) => {
  const errs = []; h.watchErrors(page, errs);
  await h.boot(page, { url: '/', hook: false });
  await h.toAim(page);
  await h.throwOnce(page, { power: 100, angle: 40, direction: 1 });
  await page.waitForTimeout(1000);
  h.save('flow_errors.json', errs);
  expect(errs, 'console のエラー／pageerror／requestfailed／HTTP 4xx・5xx').toEqual([]);
});

test('[G-T-BOOT15] キャッシュなしで「あそぶ」が押せるまで 15 秒以内', async ({ browser }) => {
  const ctx = await browser.newContext(); // 新しいコンテキスト＝キャッシュなし
  const page = await ctx.newPage();
  const t0 = Date.now();
  await page.goto('/');
  await page.waitForFunction(() => document.body.dataset.view === 'title' && !document.querySelector('#start').disabled, null, { timeout: 180000 });
  const ms = Date.now() - t0;
  h.save('boot.json', { ms });
  await ctx.close();
  expect(ms, `「あそぶ」が押せるまで ${ms} ms`).toBeLessThanOrEqual(15000);
});

test('[G-CR-SCREEN] タイトルからクレジット画面に行けて「VOICEVOX:ずんだもん」がある', async ({ page }) => {
  const errs = []; h.watchErrors(page, errs);
  await h.boot(page, { url: '/', hook: false });
  await expect(page.locator('#creditsButton'), 'タイトルに #creditsButton が無い').toBeVisible();
  await page.click('#creditsButton');
  await expect(page.locator('#credits'), '#credits が出ない').toBeVisible();
  const text = await page.locator('#credits').innerText();
  expect(text.includes('VOICEVOX:ずんだもん'), 'クレジット画面に「VOICEVOX:ずんだもん」がこの文字どおり無い').toBeTruthy();
  expect(errs).toEqual([]);
});
