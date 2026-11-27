const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),cp=require('node:child_process');
const root=path.resolve(__dirname,'../..'),web=path.join(root,'games/flight/web-template');
const evidence=process.env.KANI_OBJECT_EVIDENCE_DIR||__dirname;
test('real rules, legacy placement identity and physical integration contracts',()=>{
 const out=cp.execFileSync('python3',['docs/object-redesign/contracts.py'],{cwd:root,encoding:'utf8'});
 expect(out).toContain('static passed / 0 failed');
});
test('existing Effekseer and juice behavior contracts remain green',()=>{
 const out=cp.execFileSync('python3',['docs/effekseer-proto/contracts.py'],{cwd:root,encoding:'utf8'});
 expect(out).toContain('45 passed / 0 failed');expect(out).toContain('33 passed / 0 failed');
});
async function fixture(page,size={width:390,height:844}){
 await page.setViewportSize(size);
 let html=fs.readFileSync(path.join(web,'index.html'),'utf8').replace(/<script\b[^>]*>[\s\S]*?<\/script>/g,'');
 html=html.replace('<meta charset=','<base href="https://object-fixture.invalid/"><meta charset=');
 await page.route('https://object-fixture.invalid/**',route=>{
  const name=new URL(route.request().url()).pathname.slice(1),p=path.resolve(web,name);
  if(name==='index.html')return route.fulfill({body:html,contentType:'text/html'});
  if(!p.startsWith(web+path.sep)||!fs.existsSync(p))return route.fulfill({status:404,body:''});
  return route.fulfill({body:fs.readFileSync(p),contentType:name.endsWith('.css')?'text/css':'application/octet-stream'});
 });
 await page.goto('https://object-fixture.invalid/index.html');
 await page.evaluate(()=>{
  document.querySelectorAll('.overlay,#aimHUD,#aimActions,#launchZone,#topbar').forEach(e=>e.classList.add('hidden'));
  document.body.dataset.view='flight';document.body.dataset.phase='flight';
  window.calls=[];window.state={phase:'flight',objectMode:'redesign',jetsRemaining:2,paused:false,maxChain:9};
  const label=document.createElement('p');label.textContent='操作部品テスト：合成状態 / Unity未起動';label.style.cssText='position:fixed;top:45%;color:white;font-size:13px';document.body.append(label);
 });
 await page.addScriptTag({path:path.join(web,'object-controls.js')});
 await page.evaluate(()=>{ObjectControls.mount((method)=>calls.push(method));ObjectControls.update(state,'flight');});
}
for(const [name,size] of Object.entries({portrait:{width:390,height:844},small:{width:320,height:568},landscape:{width:844,height:390}})){
 test('large thumb controls fit '+name,async({page})=>{
  await fixture(page,size);
  for(const id of ['objectJet','objectDive']){
   const b=await page.locator('#'+id).boundingBox();expect(b.width).toBeGreaterThanOrEqual(96);expect(b.height).toBeGreaterThanOrEqual(64);
   expect(b.x).toBeGreaterThanOrEqual(0);expect(b.x+b.width).toBeLessThanOrEqual(size.width);expect(b.y+b.height).toBeLessThanOrEqual(size.height);
  }
  const dive=await page.locator('#objectDive').boundingBox(),jet=await page.locator('#objectJet').boundingBox();expect(dive.x+dive.width).toBeLessThan(jet.x);
  await page.screenshot({path:path.join(evidence,'controls-'+name+'.png')});
 });
}
test('jets by tap and keyboard, budget feedback, repeat suppression',async({page})=>{
 await fixture(page);await page.locator('#objectJet').click();await expect(page.locator('#objectJet')).toBeDisabled();
 expect(await page.evaluate(()=>calls)).toEqual(['Jet']);
 await page.evaluate(()=>{state.jetsRemaining=1;ObjectControls.update(state,'flight');});
 await page.keyboard.press('j');expect(await page.evaluate(()=>calls)).toEqual(['Jet','Jet']);
 await page.evaluate(()=>{state.jetsRemaining=0;ObjectControls.update(state,'flight');});
 await page.keyboard.press('ArrowUp');await expect(page.locator('#jetIcons')).toHaveText('○ ○');
 expect(await page.evaluate(()=>calls)).toEqual(['Jet','Jet']);
 await page.evaluate(()=>{state.jetsRemaining=2;ObjectControls.update(state,'flight');});
 await page.keyboard.down('ArrowUp');await page.keyboard.down('ArrowUp');await page.keyboard.up('ArrowUp');
 expect(await page.evaluate(()=>calls)).toEqual(['Jet','Jet','Jet']);
});
test('dive pointer capture, simultaneous keyboard, release and blur',async({page})=>{
 await fixture(page);const b=await page.locator('#objectDive').boundingBox();await page.mouse.move(b.x+30,b.y+30);await page.mouse.down();
 await expect(page.locator('#objectDive')).toHaveAttribute('aria-pressed','true');
 await page.keyboard.down('k');await page.mouse.move(200,400);await page.mouse.up();
 expect(await page.evaluate(()=>calls)).toEqual(['DiveDown']);await page.keyboard.up('k');
 expect(await page.evaluate(()=>calls)).toEqual(['DiveDown','DiveUp']);
 await page.keyboard.down('ArrowDown');await page.evaluate(()=>window.dispatchEvent(new Event('blur')));
 await expect(page.locator('#objectDive')).toHaveAttribute('aria-pressed','false');await page.keyboard.up('ArrowDown');
 expect(await page.evaluate(()=>calls)).toEqual(['DiveDown','DiveUp','DiveDown','DiveUp']);
});
test('legacy, charge, pause, result and retry gate flight-only input',async({page})=>{
 await fixture(page);await page.keyboard.down('k');
 await page.evaluate(()=>{state.paused=true;ObjectControls.update(state,'pause');});
 await expect(page.locator('#objectControls')).toBeHidden();await page.keyboard.up('k');
 for(const mode of ['legacy','redesign'])for(const phase of ['ready','charge','result']){
  await page.evaluate(({mode,phase})=>{state={...state,paused:false,phase,objectMode:mode};ObjectControls.update(state,phase==='ready'?'aim':phase);},{mode,phase});
  await page.keyboard.press('j');await page.keyboard.press('k');await expect(page.locator('#objectControls')).toBeHidden();
 }
 expect(await page.evaluate(()=>calls)).toEqual(['DiveDown','DiveUp']);
 await page.evaluate(()=>{state={...state,phase:'flight',objectMode:'legacy'};ObjectControls.update(state,'flight');});
 await page.keyboard.press('ArrowUp');expect(await page.evaluate(()=>calls)).toEqual(['DiveDown','DiveUp']);
 await page.evaluate(()=>{state={...state,objectMode:'redesign',jetsRemaining:2,maxChain:0};ObjectControls.update(state,'flight');});
 await expect(page.locator('#objectControls')).toBeVisible();await expect(page.locator('#jetIcons')).toHaveText('● ●');await expect(page.locator('#maxChain')).toHaveText('0');
});
test('result shows authoritative maximum chain beside dominant destruction',async({page})=>{
 await fixture(page);await page.evaluate(()=>{state.phase='result';state.maxChain=17;ObjectControls.update(state,'result');document.getElementById('result').classList.remove('hidden');});
 await expect(page.locator('#maxChain')).toHaveText('17');await expect(page.locator('#objectControls')).toBeHidden();
 const sizes=await page.evaluate(()=>[getComputedStyle(document.getElementById('destructionResultValue')).fontSize,getComputedStyle(document.getElementById('maxChain')).fontSize].map(parseFloat));
 expect(sizes[0]).toBeGreaterThan(sizes[1]);await page.screenshot({path:path.join(evidence,'result-component.png')});
 const host=fs.readFileSync(path.join(web,'host.js'),'utf8');expect(host).toContain('window.ObjectControls.update(s,view)');expect(host).toContain('window.ObjectControls.mount(send)');
});

test('touchscreen jet and held dive use the same real pointer path',async({page})=>{
 await fixture(page);const jet=await page.locator('#objectJet').boundingBox();await page.touchscreen.tap(jet.x+30,jet.y+30);
 expect(await page.evaluate(()=>calls)).toEqual(['Jet']);
 const dive=await page.locator('#objectDive').boundingBox(),cdp=await page.context().newCDPSession(page);
 await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:dive.x+30,y:dive.y+30,id:1}]});
 await expect(page.locator('#objectDive')).toHaveAttribute('aria-pressed','true');
 await cdp.send('Input.dispatchTouchEvent',{type:'touchCancel',touchPoints:[]});
 await expect(page.locator('#objectDive')).toHaveAttribute('aria-pressed','false');
 expect(await page.evaluate(()=>calls)).toEqual(['Jet','DiveDown','DiveUp']);
});
