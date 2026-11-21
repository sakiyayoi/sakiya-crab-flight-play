// カニとばし 受入テスト（launch / main loop / 床停止 20強+20弱 / console error）
// 前提: host.js のトップレベル変数 lastState・config・configure() と、#start #launchZone #retry、
//       launchZone の Enter（未チャージ時=チャージ、チャージ後=発射）が resultspace1-r1 と同じであること。
const { test, expect } = require('@playwright/test');
const fs = require('fs');
const OUT = 'qa-out';
fs.mkdirSync(OUT, { recursive: true });
const N = Number(process.env.CRAB_THROWS || 20);           // 強・弱それぞれの回数
const MIN_DIST = Number(process.env.CRAB_MIN_DIST || 5);  // これ未満で止まったら床停止扱い (m)
const ANGLES = [20, 40, 55, 70];
const VERSION='crab-flight-r23-input-ack3';
if(N!==20||MIN_DIST!==5)throw new Error('Acceptance conditions are fixed at20 per strength and5m');

function watchConsole(page, bag) {
  page.on('console', m => { if (m.type() === 'error') bag.push('console.error: ' + m.text()); });
  page.on('pageerror', e => bag.push('pageerror: ' + e.message));
  page.on('requestfailed', r => bag.push('requestfailed: ' + r.url() + ' ' + (r.failure()?.errorText || '')));
  page.on('response', r => { if (r.status() >= 400) bag.push(`http ${r.status()}: ${r.url()}`); });
}
async function boot(page) {
  await page.goto('/');
  await page.waitForFunction(() => document.body.dataset.view === 'title', null, { timeout: 180000 });
  await page.click('#start');
  await page.waitForFunction(() => document.body.dataset.view === 'aim' && document.body.dataset.inputPending !== 'true' && lastState?.phase==='ready' && !lastState.chargeStarted, null, { timeout: 30000 });
  expect(await page.evaluate(()=>[document.body.dataset.wrapperRevision,lastState.revision,lastState.productVersion])).toEqual([VERSION,VERSION,VERSION]);
}
async function snapshot(page){return page.evaluate(()=>({phase:lastState?.phase,d:lastState?.distance,f:lastState?.physicsFault,t:lastState?.time,surf:lastState?.surface,pw:lastState?.power,ang:lastState?.angle,direction:lastState?.direction,floorSampled:lastState?.floorSampled,minHeight:lastState?.minHeight,groundStuckSeconds:lastState?.groundStuckSeconds,energy:lastState?.chargeEnergy,spinPhase:lastState?.visualSpinAngle,revision:lastState?.revision,productVersion:lastState?.productVersion,sourceIdentity:lastState?.sourceIdentity,generation:lastState?.generation,upgradeLevel:lastState?.upgradeLevel,reducedMotion:lastState?.reducedMotion,cameraSize:lastState?.cameraSize,attempt:lastState?.attempts,launch:lastState?.launchReceipt,throwAudit:lastState?.throwAudit,reject:lastState?.launchRejectReason}));}
async function freshAim(page){
  if(await page.evaluate(()=>document.body.dataset.view)==='result')await page.click('#retry');
  await page.waitForFunction(()=>document.body.dataset.view==='aim'&&document.body.dataset.inputPending!=='true'&&lastState?.phase==='ready'&&!lastState.chargeStarted,null,{timeout:30000});
}
async function throwOnce(page,power,angle,direction){
  await freshAim(page);
  const first=await page.evaluate(()=>({offset:window.readInputReceiptsR23().length,generation:lastState.generation}));
  const id=await page.evaluate(([p,a,d])=>{config.power=p;config.angle=a;config.direction=d;return configure();},[power,angle,direction]);
  await page.waitForFunction(id=>window.readInputReceiptsR23().some(e=>e.id===id&&e.ack),id,{timeout:30000});
  const configured=await page.evaluate(id=>window.readInputReceiptsR23().find(e=>e.id===id),id);
  if(!configured.ack.accepted||configured.ack.appliedPower!==power)return {...await snapshot(page),configured,fault:'configure-rejected-or-mismatch'};
  await page.focus('#launchZone');const t0=Date.now();
  while(!(await page.evaluate(()=>!!lastState?.chargeReady))){
    if(Date.now()-t0>120000)return {...await snapshot(page),fault:'charge-timeout'};
    await page.keyboard.press('Enter');await page.waitForTimeout(150);
  }
  await page.keyboard.press('Enter');const t1=Date.now();let s;
  do {await page.waitForTimeout(200);s=await snapshot(page);if(Date.now()-t1>300000){s.fault='no-result-300s';break;}}while(s.phase!=='result');
  const commands=await page.evaluate(offset=>window.readInputReceiptsR23().slice(offset),first.offset);
  const release=commands.filter(e=>e.method==='ReleaseLaunch').at(-1);
  const launch=s.launch;
  const expectedPower=launch?Math.max(power*(.18+.82*launch.charge),(45+20*Math.min(1,power/65))*launch.charge):null;
  const mismatch=!release?.ack?.accepted||release.ack.requestedPower!==power||!launch||Math.abs(launch.power-expectedPower)>.001||s.generation!==first.generation;
  return {...s,configured,commands,chargeWallMs:t1-t0,resultWallMs:Date.now()-t1,expectedPower,fault:s.fault||s.f||(s.d<MIN_DIST?'short-stop':mismatch?'input-ack-mismatch':'')};
}
async function recoverFailed(page){
  if(await page.evaluate(()=>view)==='result')return;
  await page.keyboard.press('Escape');await page.click('#home');await page.click('#start');
  await page.waitForFunction(()=>view==='aim'&&lastState?.phase==='ready'&&!lastState.chargeStarted&&document.body.dataset.inputPending!=='true');
}

for (const vp of [{ name: 'desktop', viewport: { width: 1280, height: 800 } },
                  { name: 'mobile390', viewport: { width: 390, height: 844 }, isMobile: true, hasTouch: true }]) {
  test(`launch + start + console (${vp.name})`, async ({ browser }) => {
    const ctx = await browser.newContext({ viewport: vp.viewport, isMobile: !!vp.isMobile, hasTouch: !!vp.hasTouch });
    const page = await ctx.newPage(); const errs = []; watchConsole(page, errs);
    await boot(page);
    await page.screenshot({ path: `${OUT}/${vp.name}_aim.png` });
    fs.writeFileSync(`${OUT}/${vp.name}_display.json`,JSON.stringify(await page.evaluate(()=>({viewport:[innerWidth,innerHeight],dpr:devicePixelRatio,canvas:[$('unity-canvas').width,$('unity-canvas').height],camera:lastState.cameraSize,version:lastState.revision,sourceIdentity:lastState.sourceIdentity})),null,2));
    const box = await page.locator('#launchZone').boundingBox();
    expect(box && box.y + box.height <= vp.viewport.height, 'launchZone が画面内').toBeTruthy();
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), '横はみ出しなし').toBeTruthy();
    fs.writeFileSync(`${OUT}/${vp.name}_console.json`, JSON.stringify(errs, null, 1));
    expect(errs, 'console error / pageerror / 4xx-5xx なし').toEqual([]);
    await ctx.close();
  });
}

test('main loop: 1投で物理時間が進み結果に到達', async ({ page }) => {
  const errs = []; watchConsole(page, errs);
  await boot(page);
  const r = await throwOnce(page, 70, 40, 1);
  await page.screenshot({ path: `${OUT}/mainloop_result.png` });
  fs.writeFileSync(`${OUT}/mainloop.json`,JSON.stringify(r,null,2));
  fs.writeFileSync(`${OUT}/mainloop_console.json`,JSON.stringify(errs,null,2));
  expect(r.fault).toBe('');
  expect(r.t, 'lastState.time が進む').toBeGreaterThan(0.5);
  expect(r.phase).toBe('result');
  expect(errs).toEqual([]);
  const previousAttempt=r.attempt;await page.click('#prepare');
  await page.waitForFunction(a=>lastState?.attempts===a+1&&lastState?.launchReceipt?.source==='retry',previousAttempt);
  await page.waitForFunction(()=>lastState?.phase==='result',null,{timeout:300000});
  const same=await snapshot(page),ack=await page.evaluate(()=>window.readInputReceiptsR23().filter(e=>e.method==='RetryLaunch').at(-1));
  fs.writeFileSync(`${OUT}/repeat-launch.json`,JSON.stringify({same,ack},null,2));
  expect(ack.ack.accepted).toBe(true);expect(same.launch.phase).toBe(r.launch.phase);expect(same.launch.spin).toBe(r.launch.spin);expect(same.launch.charge).toBe(r.launch.charge);
  fs.writeFileSync(`${OUT}/mainloop_console.json`,JSON.stringify(errs,null,2));expect(errs).toEqual([]);
});

test(`floor-stop: 強${N}回(power100) + 弱${N}回(power55)`, async ({ page }) => {
  test.setTimeout(2*N*(120000+300000)+180000); // Sum of unchanged per-throw budgets plus boot; no trial removed.
  const errs = []; watchConsole(page, errs);
  await boot(page);
  const rows = [];
  for (const [kind, power] of [['strong', 100], ['weak', 55]]) {
    for (let i = 0; i < N; i++) {
      const angle = ANGLES[i % 4], direction = Math.floor(i / 4) % 2 === 0 ? 1 : -1;
      const r = await throwOnce(page, power, angle, direction);
      rows.push({ kind, i: i + 1, power, angle, direction, ...r });
      if(r.fault){await page.screenshot({path:`${OUT}/floorstop_${kind}_${i+1}.png`});await recoverFailed(page);}
      console.log(kind,i+1,r.phase,r.d,r.pw,r.fault);
      if(i===0||i===19)await page.screenshot({path:`${OUT}/${kind}_${i+1}_result.png`});
      fs.writeFileSync(`${OUT}/throws.json`, JSON.stringify(rows, null, 1));
    }
  }
  const stops = rows.filter(r => r.fault);
  console.log(`floor stops: strong ${stops.filter(r => r.kind === 'strong').length}/${N}, weak ${stops.filter(r => r.kind === 'weak').length}/${N}`);
  expect(stops, '床停止・安全停止・未決着 0 件').toEqual([]);
  expect(errs).toEqual([]);
});

test('reset / rejected release / cancel semantics and fresh generation',async({page})=>{
 const errs=[];watchConsole(page,errs);await boot(page);
 await page.evaluate(()=>{send('BeginAim');send('ReleaseLaunch',JSON.stringify({angle:20,power:55,direction:1,eligible:true}));});
 await page.waitForFunction(()=>lastState?.launchRejectReason==='charge-not-ready');
 expect((await snapshot(page)).t).toBe(0);
 await page.evaluate(()=>{send('BeginSpinCharge');send('AddSpinWork',JSON.stringify({distance:.25,seconds:.12,direction:1}));send('BeginAim');send('CancelAim');});
 await page.waitForFunction(()=>lastState?.chargeStarted&&!lastState?.aiming);
 const active=await page.evaluate(()=>({generation:lastState.generation,energy:lastState.chargeEnergy,ready:lastState.chargeReady}));
 expect(active.energy).toBeGreaterThan(0); // CancelAim preserves intentional in-progress charge.
 await page.evaluate(()=>prepare());
 await page.waitForFunction(g=>lastState?.generation>g&&!lastState.chargeStarted&&!lastState.aiming&&lastState.spinRps===0&&lastState.distance===0,active.generation);
 expect(await page.locator('#aimHint').textContent()).not.toContain('発射できませんでした');
 fs.writeFileSync(`${OUT}/reset-boundary.json`,JSON.stringify(await page.evaluate(()=>window.readInputReceiptsR23()),null,2));
 expect(errs).toEqual([]);
});
