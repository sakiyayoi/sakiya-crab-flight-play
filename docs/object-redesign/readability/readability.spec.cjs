const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),cp=require('node:child_process');
const root=path.resolve(__dirname,'../../..'),web=path.join(root,'games/flight/web-template'),evidence=__dirname;
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

test('readability production timeline, geometry and immutable physics contracts',()=>{
 const out=cp.execFileSync('python3',['docs/object-redesign/readability/contracts.py'],{cwd:root,encoding:'utf8',timeout:160000});
 expect(out).toContain('70 passed / 0 failed');expect(out).toContain('static passed / 0 failed');
});
for(const setting of ['normal','off','os-reduced','native-reduced'])test('readability death subtitle survives '+setting,async({page})=>{
 await fixture(page);
 if(setting==='off')await page.evaluate(()=>FlightFeel.setSettings({preset:'off'}));
 if(setting==='os-reduced')await page.emulateMedia({reducedMotion:'reduce'});
 if(setting==='native-reduced')await page.evaluate(()=>{state.reducedMotion=true;document.body.dataset.reduced='true';});
 await page.evaluate(()=>{state.objectDeath=true;state.objectCapture={stage:'hold',caption:false};hit('death',false);});
 await expect(page.locator('#objectStoryCaption')).toBeHidden();
 await page.evaluate(()=>{state.objectCapture={stage:'capture',caption:false};paint();});await expect(page.locator('#objectStoryCaption')).toBeHidden();
 await page.evaluate(()=>{state.objectCapture={stage:'caption',caption:true};paint();});
 await expect(page.locator('#objectStoryCaption')).toHaveText('かに漁師だ！');await expect(page.locator('#objectStoryCaption')).toBeVisible();
 const size=await page.locator('#objectStoryCaption').evaluate(e=>parseFloat(getComputedStyle(e).fontSize));expect(size).toBeGreaterThanOrEqual(30);
 await page.clock.runFor(1199);await expect(page.locator('#objectStoryCaption')).toBeVisible();
 await page.screenshot({path:path.join(evidence,'capture-caption-'+setting+'.png')});
 await page.evaluate(()=>{state.objectCapture={stage:'done',caption:false};state.phase='result';paint();});await expect(page.locator('#objectStoryCaption')).toBeHidden();
 // Execute the production result-cause assignment, not a duplicate test string.
 const line=fs.readFileSync(path.join(web,'host.js'),'utf8').split('\n').find(l=>l.includes("$('resultCause').textContent=s.objectDeath"));
 await page.evaluate(line=>{window.s=state;window.$=id=>document.getElementById(id);(0,eval)(line);document.getElementById('result').classList.remove('hidden');},line);
 await expect(page.locator('#resultCause')).toHaveText('かに漁師につかまった！');await expect(page.locator('#resultCause')).toBeVisible();
 await page.screenshot({path:path.join(evidence,'capture-result-'+setting+'.png')});
 await page.evaluate(line=>{state.generation++;state.phase='ready';state.objectDeath=false;state.objectCaption=null;state.objectCapture=null;paint();(0,eval)(line);},line);
 await expect(page.locator('#resultCause')).toBeEmpty();await expect(page.locator('#resultCause')).toBeHidden();
});
test('readability badge layer projects all six kinds upright and mirrors boost',async({page})=>{
 await fixture(page);
 await page.evaluate(()=>{state.objectBadges=['boost','bounce','spin','brake','blast','death'].map((kind,i)=>({kind,side:1,visible:true,x:i%2===0?.28:.72,y:.65-Math.floor(i/2)*.18}));paint();});
 await expect(page.locator('.objectFunctionBadge:visible')).toHaveCount(6);
 expect(await page.locator('.objectFunctionBadge').allTextContents()).toEqual(['→','↑','↻','↓','✹','✕']);
 for(const badge of await page.locator('.objectFunctionBadge').all()){
  const b=await badge.boundingBox();expect(b.width).toBe(34);expect(b.height).toBe(34);
  const matrix=await badge.evaluate(e=>getComputedStyle(e).transform);expect(matrix).toMatch(/^matrix\(1, 0, 0, 1,/);
 }
 await page.evaluate(()=>{state.objectBadges[0].side=-1;state.objectBadges[2].x=.6;paint();});await expect(page.locator('.objectFunctionBadge').first()).toHaveText('←');
 await page.emulateMedia({reducedMotion:'reduce'});await page.evaluate(()=>FlightFeel.setSettings({preset:'off'}));await expect(page.locator('.objectFunctionBadge:visible')).toHaveCount(6);
 await page.screenshot({path:path.join(evidence,'badges-390.png')});
 await page.evaluate(()=>{state.objectBadges[0].visible=false;paint();});await expect(page.locator('.objectFunctionBadge:visible')).toHaveCount(5);
 await page.evaluate(()=>{state.phase='result';paint();});await expect(page.locator('.objectFunctionBadge:visible')).toHaveCount(0);
});
test('readability death pause/retry and large caption fit at 320 and landscape',async({page})=>{
 for(const size of [{width:320,height:568},{width:844,height:390}]){
  await fixture(page,size);await page.evaluate(()=>{state.objectCapture={stage:'caption',caption:true};hit('death');});
  const b=await page.locator('#objectStoryCaption').boundingBox();expect(b.x).toBeGreaterThanOrEqual(0);expect(b.x+b.width).toBeLessThanOrEqual(size.width);expect(b.y+b.height).toBeLessThan(size.height);
  await page.evaluate(()=>{state.paused=true;paint();});await expect(page.locator('#objectStoryCaption')).toBeHidden();
  await page.evaluate(()=>{state.paused=false;paint();});await expect(page.locator('#objectStoryCaption')).toBeVisible();
  await page.evaluate(()=>{state.generation++;state.phase='ready';state.objectCaption=null;state.objectCapture=null;paint();});await expect(page.locator('#objectStoryCaption')).toBeHidden();
 }
});
