const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'../../..'),web=path.join(root,'games/flight/web-template');
const evidence=process.env.KANI_STORY_EVIDENCE_DIR||__dirname;
const captions={boost:'扇風機、強すぎる！',bounce:'フライパンで跳ね上げられた！',spin:'回転寿司のレーンに乗った！',brake:'もちにくっついた…',blast:'花火、点火済みだった！',death:'かに漁師だ！'};
async function fixture(page,size={width:390,height:844}){
 await page.setViewportSize(size);await page.clock.install({time:new Date('2026-11-27T00:00:00Z')});
 let html=fs.readFileSync(path.join(web,'index.html'),'utf8').replace(/<script\b[^>]*>[\s\S]*?<\/script>/g,'').replace('<meta charset=','<base href="https://story-fixture.invalid/"><meta charset=');
 await page.route('https://story-fixture.invalid/**',route=>{
  const name=new URL(route.request().url()).pathname.slice(1);
  if(name==='index.html')return route.fulfill({body:html,contentType:'text/html'});
  const file=name==='StreamingAssets/feel.json'?path.join(root,'games/flight/crab-flight/Assets/StreamingAssets/feel.json'):path.resolve(web,name);
  if(!file.startsWith(root+path.sep)||!fs.existsSync(file))return route.fulfill({status:404,body:''});
  return route.fulfill({body:fs.readFileSync(file),contentType:name.endsWith('.css')?'text/css':name.endsWith('.json')?'application/json':'application/octet-stream'});
 });
 await page.goto('https://story-fixture.invalid/index.html');await page.clock.pauseAt(new Date('2026-11-27T00:01:00Z'));
 await page.evaluate(()=>{
  window.send=()=>{};
  document.querySelectorAll('.overlay,#aimHUD,#aimActions,#gesture,#launchZone,#kickFeedback').forEach(e=>e.classList.add('hidden'));
  document.body.dataset.view='flight';document.body.dataset.phase='flight';document.getElementById('flightHUD').classList.remove('hidden');
  window.state={generation:1,phase:'flight',objectMode:'redesign',destructionEvent:0,paused:false,distance:12,eventId:0,events:[],objectCaption:null};
  const badge=document.createElement('p');badge.textContent='ストーリーHUD部品 / 合成状態・Unity未起動';badge.style.cssText='position:fixed;bottom:4px;color:white;z-index:50;font-size:11px';document.body.append(badge);
 });
 for(const p of ['flight-cues.js','feel.js','juice-hud.js','object-controls.js'])await page.addScriptTag({path:path.join(web,p)});
 await page.evaluate(async()=>{await FlightFeel.load();ObjectControls.mount(send);window.paint=()=>{ObjectControls.update(state,state.phase);FlightFeel.update(state);FlightCues.update(state);JuiceHud.update(state);};paint();window.hit=(kind,visible=true)=>{state.objectCaption={kind,visible,sequence:(state.objectCaption?.sequence||0)+1};paint();};});
}
for(const [kind,text] of Object.entries(captions))test('exact caption and specified expiry: '+kind,async({page})=>{
 await fixture(page);await page.evaluate(kind=>hit(kind),kind);
 await expect(page.locator('#objectStoryCaption')).toHaveText(text);await expect(page.locator('#objectStoryCaption')).toBeVisible();
 await page.clock.runFor(900);await expect(page.locator('#objectStoryCaption')).toBeVisible();
 await page.clock.runFor(kind==='death'?301:101);await expect(page.locator('#objectStoryCaption')).toBeHidden();
 await page.evaluate(()=>paint());await expect(page.locator('#objectStoryCaption')).toBeHidden();
});
test('latest only, dense event ring, no queue and old timers cannot clear a new caption',async({page})=>{
 await fixture(page);await page.evaluate(()=>hit('boost'));await page.clock.runFor(800);
 await page.evaluate(()=>{state.events=Array.from({length:32},(_,i)=>({generation:1,id:i+40,type:'impact'}));hit('blast');hit('bounce');hit('death');});
 await expect(page.locator('#objectStoryCaption')).toHaveText(captions.death);await expect(page.locator('#objectStoryCaption')).toHaveCount(1);
 await page.clock.runFor(201);await expect(page.locator('#objectStoryCaption')).toBeVisible();
 await page.clock.runFor(1000);await expect(page.locator('#objectStoryCaption')).toBeHidden();
 await page.clock.runFor(5000);await expect(page.locator('#objectStoryCaption')).toBeHidden();
});
test('effects off and OS reduced motion suppress immediately and never replay missed captions',async({page})=>{
 await fixture(page);await page.evaluate(()=>hit('boost'));
 await page.evaluate(()=>FlightFeel.setSettings({preset:'off'}));await expect(page.locator('#objectStoryCaption')).toBeHidden();
 await page.evaluate(()=>{hit('blast',false);FlightFeel.setSettings({preset:'mashimashi'});paint();});await expect(page.locator('#objectStoryCaption')).toBeHidden();
 await page.evaluate(()=>hit('spin'));await expect(page.locator('#objectStoryCaption')).toBeVisible();
 await page.emulateMedia({reducedMotion:'reduce'});await expect(page.locator('#objectStoryCaption')).toBeHidden();
 await page.evaluate(()=>hit('brake'));await page.emulateMedia({reducedMotion:'no-preference'});await page.evaluate(()=>paint());await expect(page.locator('#objectStoryCaption')).toBeHidden();
 await page.evaluate(()=>{FlightFeel.setSettings({preset:'futsuu'});hit('bounce');});await expect(page.locator('#objectStoryCaption')).toBeVisible();
});
test('native flags, activation flag, pause, legacy and retry suppress without delayed replay',async({page})=>{
 await fixture(page);
 for(const field of ['reducedMotion','paused','feelReduced','feelOff','legacy','activation']){
  await page.evaluate(field=>{
   state.reducedMotion=field==='reducedMotion';state.paused=field==='paused';state.feel={reduced:field==='feelReduced',preset:field==='feelOff'?'off':'mashimashi'};state.objectMode=field==='legacy'?'legacy':'redesign';hit('blast',field!=='activation');
  },field);
  await expect(page.locator('#objectStoryCaption')).toBeHidden();
  await page.evaluate(()=>{state.reducedMotion=state.paused=false;state.feel={};state.objectMode='redesign';paint();});await expect(page.locator('#objectStoryCaption')).toBeHidden();
 }
 await page.evaluate(()=>hit('brake'));await expect(page.locator('#objectStoryCaption')).toBeVisible();
 await page.evaluate(()=>{state.generation++;state.phase='ready';state.objectCaption=null;paint();});await expect(page.locator('#objectStoryCaption')).toBeHidden();
 await page.evaluate(()=>{state.phase='flight';hit('boost');});await expect(page.locator('#objectStoryCaption')).toHaveText(captions.boost);
});
test('unknown kind and stale sequence cannot replace current text',async({page})=>{
 await fixture(page);await page.evaluate(()=>{hit('spin');state.objectCaption={sequence:0,kind:'death',visible:true};paint();});await expect(page.locator('#objectStoryCaption')).toHaveText(captions.spin);
 await page.evaluate(()=>{state.objectCaption={sequence:2,kind:'toString',visible:true};paint();});await expect(page.locator('#objectStoryCaption')).toBeHidden();
});
for(const [name,size] of Object.entries({portrait:{width:390,height:844},small:{width:320,height:568},landscape:{width:844,height:390}})){
 test('caption fits and avoids real destruction pop / HUD: '+name,async({page})=>{
  await fixture(page,size);await page.evaluate(()=>{
   hit('bounce');state.destructionEvent=24;paint();
   FlightCues.fire('break',{occ:100,material:'wood',position:{x:0,y:0,z:0}});
  });await page.clock.runFor(80);
  const caption=await page.locator('#objectStoryCaption').boundingBox(),hud=await page.locator('#flightHUD').boundingBox();
  expect(caption.y).toBeGreaterThan(hud.y+hud.height);expect(caption.x).toBeGreaterThanOrEqual(0);expect(caption.x+caption.width).toBeLessThanOrEqual(size.width);expect(caption.y+caption.height).toBeLessThan(size.height-80);
  const popup=page.locator('[data-feel-popup]');
  await expect(popup).toBeVisible();const box=await popup.boundingBox();expect(box.y).toBeGreaterThanOrEqual(caption.y+caption.height+12);
  await page.screenshot({path:path.join(evidence,'caption-'+name+'.png')});
 });
 test('death stays visible through immediate result and retry clears it: '+name,async({page})=>{
  await fixture(page,size);await page.evaluate(()=>{
   state.phase='result';document.body.dataset.view='result';document.getElementById('flightHUD').classList.add('hidden');document.getElementById('result').classList.remove('hidden');hit('death');
  });await expect(page.locator('#result #objectStoryCaption')).toHaveText(captions.death);await expect(page.locator('#objectStoryCaption')).toBeVisible();
  const b=await page.locator('#objectStoryCaption').boundingBox();expect(b.x).toBeGreaterThanOrEqual(0);expect(b.x+b.width).toBeLessThanOrEqual(size.width);expect(b.y+b.height).toBeLessThan(size.height);
  await page.screenshot({path:path.join(evidence,'death-'+name+'.png')});
  await page.evaluate(()=>{state.generation++;state.phase='ready';state.objectCaption=null;paint();});await expect(page.locator('#objectStoryCaption')).toBeHidden();
 });
}
