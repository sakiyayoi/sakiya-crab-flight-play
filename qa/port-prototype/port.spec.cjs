const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),cp=require('node:child_process'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),web=path.join(root,'games/flight/web-template'),evidence=path.join(root,'docs/port-prototype');
const read=f=>JSON.parse(fs.readFileSync(path.join(evidence,f),'utf8'));
test('offline regenerated layout and unchanged inputs satisfy strict physical-space audits',()=>{
 cp.execFileSync('python3',['tools/port-prototype/author.py','--check'],{cwd:root});
 cp.execFileSync('python3',['tools/port-prototype/inspect.py'],{cwd:root});
 const a=read('audit.json'),s=read('source-audit.json');expect(a.groundingViolations).toEqual([]);expect(a.after.densityCV).toBeGreaterThanOrEqual(.5);expect(s.aabbOverlapPairs).toEqual([]);expect(s.protectedSourceCount).toBeGreaterThan(30);
});
test('runtime role formulas reject false reversals and preserve angle-only energy',()=>{
 const output=cp.execFileSync('python3',['tools/port-prototype/contracts.py'],{cwd:root}).toString();expect(output).toContain('30 passed / 0 failed');
});
test('managed compiler has zero errors; camera OFF retains original R19 formulas',()=>{
 expect(read('managed-compile/result.json').errors).toBe(0);expect(read('managed-compile/editor-result.json').errors).toBe(0);
 for(const [file,sha] of Object.entries({...read('managed-compile/sources.json'),...read('managed-compile/editor-sources.json')})){expect(crypto.createHash('sha256').update(fs.readFileSync(path.join(root,'games/flight/crab-flight',file))).digest('hex'),file).toBe(sha);}
 const file='games/flight/crab-flight/Assets/Scripts/FlightCameraR19.cs';
 const before=cp.execFileSync('git',['show',read('start.json').base+':'+file],{cwd:root}).toString();
 const after=fs.readFileSync(path.join(root,file),'utf8').replace('  if(!cameraFollowingR29)steadyFlightSpeedR3=Mathf.Abs(simulation.Body.linearVelocity.x);\n  if(FramePortCamera(ref size,ref lead,ref centerY))return;\n','').replace('  if(FollowPortCamera(target,size,dt,snap))return;\n','');
 expect(after).toBe(before);
 const hash=crypto.createHash('sha256').update(fs.readFileSync(path.join(root,'games/flight/crab-flight/Assets/FlightField.unity'))).digest('hex');expect(hash).toBe(read('start.json').sceneSha256);
});
async function fixture(page,size){
 await page.setViewportSize(size);
 let html=fs.readFileSync(path.join(web,'index.html'),'utf8').replace(/<script\b[^>]*>[\s\S]*?<\/script>/g,'');
 html=html.replace('<meta charset=', '<base href="https://port-fixture.invalid/"><meta charset=');
 await page.route('https://port-fixture.invalid/**',route=>{
  if(new URL(route.request().url()).pathname==='/index.html')return route.fulfill({body:html,contentType:'text/html'});
  const name=decodeURIComponent(new URL(route.request().url()).pathname).slice(1),file=path.resolve(web,name);
  if(!file.startsWith(web+path.sep)||!fs.existsSync(file))return route.abort();
  return route.fulfill({body:fs.readFileSync(file),contentType:name.endsWith('.css')?'text/css':name.endsWith('.js')?'application/javascript':name.endsWith('.ttf')?'font/ttf':'text/plain'});
 });
 await page.goto('https://port-fixture.invalid/index.html');
 await page.addScriptTag({path:path.join(web,'flight-cues.js')});
 await page.addScriptTag({path:path.join(web,'port-presentation.js')});
 await page.evaluate(()=>{
  document.querySelectorAll('.overlay,#aimHUD,#aimActions,#gesture,#launchZone,#kickFeedback').forEach(e=>e.classList.add('hidden'));
  document.body.dataset.view='flight';document.body.dataset.phase='flight';document.body.dataset.direction='1';
  document.getElementById('destructionValue').textContent='1481';document.getElementById('flightHUD').classList.remove('hidden');document.getElementById('kick').classList.remove('hidden');
  const badge=document.createElement('p');badge.textContent='UI部品テスト・Unity未起動';badge.style.cssText='position:fixed;bottom:4px;right:8px;font-size:9px;color:#fff3d6;z-index:20';document.body.append(badge);
  window.fixtureState={generation:1,phase:'flight',direction:1,worldX:704,distance:704,portPreview:{landmark:'ヤードのクレーン',remaining:50.13,nextRoles:['boost60','jackpot','hazard'],reversalReady:false},cameraPosition:{x:704,y:10,z:-16},cameraRight:{x:1,y:0,z:0},cameraUp:{x:0,y:1,z:0},cameraSize:20,cameraAspect:innerWidth/innerHeight};
  PortPresentation.update(fixtureState);
 });
 await page.evaluate(()=>document.fonts.ready);
}
for(const [name,size] of Object.entries({portrait:{width:390,height:844},small:{width:320,height:568},landscape:{width:844,height:390}})){
 test('UI fixture '+name+': readable previews, result secondary text, retry retained',async({page})=>{
  const errors=[];page.on('pageerror',e=>errors.push(String(e)));await fixture(page,size);
  await expect(page.locator('#portPreview')).toBeVisible();await expect(page.locator('.port-roles span')).toHaveCount(3);
  await expect(page.locator('#portPreview')).toContainText('あと51m');
  const bounds=await page.locator('#portPreview').boundingBox();expect(bounds.x).toBeGreaterThanOrEqual(0);expect(bounds.x+bounds.width).toBeLessThanOrEqual(size.width);
  const rotor=await page.locator('#rotorStatus').boundingBox();expect(rotor.x+rotor.width).toBeLessThan(bounds.x);
  const score=await page.locator('#destructionValue').evaluate(e=>{const r=document.createRange();r.selectNodeContents(e);return r.getBoundingClientRect().right;});expect(score).toBeLessThan(bounds.x);
  await page.evaluate(()=>{fixtureState.portPreview.reversalReady=true;PortPresentation.update(fixtureState);});
  await expect(page.locator('.port-special')).toHaveAttribute('data-ready','true');
  await page.screenshot({path:path.join(evidence,'ui-fixture-flight-'+name+'.png')});
  await page.evaluate(()=>{fixtureState.phase='result';document.body.dataset.view='result';document.getElementById('result').classList.remove('hidden');document.getElementById('flightHUD').classList.add('hidden');document.getElementById('kick').classList.add('hidden');document.getElementById('destructionResultValue').textContent='48';PortPresentation.update(fixtureState);});
  await expect(page.locator('#portNextLandmark')).toHaveText('次のランドマークまで あと51m');await expect(page.locator('#retry')).toHaveText('もう一投');
  await expect(page.locator('#resultActions')).not.toHaveAttribute('open','');await expect(page.locator('#resultDetails')).not.toHaveAttribute('open','');
  expect(await page.locator('#retry').evaluate(e=>getComputedStyle(e).backgroundColor)).toBe('rgb(255, 49, 49)');
  expect(await page.locator('#result .panel').evaluate(e=>getComputedStyle(e).backgroundImage)).toContain('repeating-linear-gradient');
  await page.locator('#retry').scrollIntoViewIfNeeded();const retry=await page.locator('#retry').boundingBox();expect(retry.y+retry.height).toBeLessThanOrEqual(size.height);
  await page.screenshot({path:path.join(evidence,'ui-fixture-result-'+name+'.png')});expect(errors).toEqual([]);
 });
}
test('UI fixture consumes actual cue once, does not add scores, and clears on replay',async({page})=>{
 await fixture(page,{width:390,height:844});
 await page.evaluate(()=>{fixtureState.events=[{generation:1,id:1,type:'destruction',objectId:'fixture-only',position:{x:704,y:10,z:0},physicsTime:1}];FlightCues.update(fixtureState);FlightCues.update(fixtureState);});
 await expect(page.locator('.port-number')).toHaveCount(1);await expect(page.locator('#destructionValue')).toHaveText('1481');
 await page.evaluate(()=>{fixtureState.generation=2;fixtureState.events=[];PortPresentation.update(fixtureState);FlightCues.update(fixtureState);});
 await expect(page.locator('.port-number')).toHaveCount(0);
});
test('UI fixture reduced motion uses fade only; negative side has no port preview',async({page})=>{
 await page.emulateMedia({reducedMotion:'reduce'});await fixture(page,{width:390,height:844});
 await page.evaluate(()=>FlightCues.fire('break',{position:{x:704,y:10,z:0}}));
 const frames=await page.locator('.port-number').evaluate(e=>e.getAnimations()[0].effect.getKeyframes());expect(frames.every(f=>!f.transform)).toBe(true);
 await page.evaluate(()=>{fixtureState.direction=-1;fixtureState.worldX=-704;PortPresentation.update(fixtureState);});await expect(page.locator('#portPreview')).toBeHidden();await expect(page.locator('#portNextLandmark')).toBeHidden();
});
test('C5 defaults on while explicit saved comfort preference survives',async({page})=>{
 await fixture(page,{width:390,height:844});await page.addScriptTag({path:path.join(web,'feel.js')});
 expect(await page.evaluate(()=>FlightFeel.settings())).toMatchObject({shake:true,shakeExplicit:false});
 await page.evaluate(()=>FlightFeel.setSettings({shake:false}));
 expect(await page.evaluate(()=>JSON.parse(localStorage.getItem('crab-flight-feel-v2-calm')))).toMatchObject({shake:false,shakeExplicit:true});
 await page.reload();await page.addScriptTag({path:path.join(web,'flight-cues.js')});await page.addScriptTag({path:path.join(web,'feel.js')});
 expect(await page.evaluate(()=>FlightFeel.settings())).toMatchObject({shake:false,shakeExplicit:true});
});
for(const title of ['正式入力→発射→破壊→結果→再投・固定入力比較','Native港8ビート・C1–C8・破壊動画と縦横画面','WebGLビルドと実ブラウザ通しプレイ','Game Studio game-playtest対象ゲームの検品'])test.skip('ライセンス待ち: '+title,()=>{});
