const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),cp=require('node:child_process'),crypto=require('node:crypto'),vm=require('node:vm');
const root=path.resolve(__dirname,'../..'),web=path.join(root,'games/flight/web-template'),evidence=path.join(root,'docs/juice-layer');
const base='342cd9f5ec6a97817f4a50b21810266d30f909f2';
const read=p=>fs.readFileSync(path.join(root,p),'utf8');
const git=(...args)=>cp.execFileSync('git',args,{cwd:root});
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');

test('frozen placement, physics, camera, models and 26 original edits are preserved',()=>{
 const allowed=new Set(['games/flight/crab-flight/Assets/Scripts/FeelDirectorR27.cs','games/flight/crab-flight/Assets/Scripts/MashimashiPresentationR27.cs',
  ...['feel.js','host.js','index.html','sound-pack.js','sound.js'].map(f=>'games/flight/web-template/'+f),'tasks.md']);
 const changed=git('diff',base,'--name-only','--diff-filter=MDR').toString().trim().split('\n').filter(Boolean);
 for(const p of changed)expect(allowed.has(p),p).toBe(true);
 for(const [p,hash] of Object.entries(JSON.parse(read('docs/juice-layer/protected-before.json')))){
  expect(sha(fs.readFileSync(path.join(root,'../source',p))),p).toBe(hash);
  expect(git('rev-parse','HEAD:'+p).toString()).toBe(git('rev-parse',base+':'+p).toString());
 }
 const html=read('games/flight/web-template/index.html').replace('<script src="juice-hud.js"></script>','');
 expect(html).toBe(git('show',base+':games/flight/web-template/index.html').toString());
 expect(read('games/flight/crab-flight/Assets/Scripts/FlightEventStream.cs')).toBe(git('show',base+':games/flight/crab-flight/Assets/Scripts/FlightEventStream.cs').toString());
});

test('actual C# policies and transplanted burst survive caps, windows, reset and safety',()=>{
 const output=cp.execFileSync('python3',['tools/juice-layer/contracts.py'],{cwd:root,encoding:'utf8'});
 expect(output).toContain('33 passed / 0 failed');
});

test('frozen FBX colours reproduce exactly without touching importer settings',()=>{
 const output=cp.execFileSync('python3',['tools/juice-layer/bake_colors.py','--check'],{cwd:root,encoding:'utf8'});
 expect(output).toContain('19 frozen FBX models / 40 panel colours: MATCH');
 const data=JSON.parse(read('games/flight/crab-flight/Assets/JuiceLayer/Resources/JuiceLayer/PanelColors.json'));
 expect(new Set(data.models.map(x=>JSON.stringify(x.color))).size).toBeGreaterThan(4);
 expect(data.entries.find(e=>e.id==='wood-chair-1').color).not.toEqual([1,1,1]);
});

test('compiler receipt matches current source and event path gates legacy destruction stops',()=>{
 const r=JSON.parse(read('docs/juice-layer/compile.json'));expect(r.errors).toBe(0);expect(r.exitCode).toBe(0);
 for(const [p,hash] of Object.entries(r.sources))expect(sha(fs.readFileSync(path.join(root,'games/flight/crab-flight',p))),p).toBe(hash);
 const director=read('games/flight/crab-flight/Assets/Scripts/FeelDirectorR27.cs');
 expect(director).toContain('PresentJuiceBreak(e,c);FeelCueR27(c);');
 expect(director).toContain('frames=destructionCue?0:');
 expect(director).toContain('count=destructionCue?0:');
 expect(director).not.toContain('=PortShakeOffset(dt)');
 expect(director).toContain('feelCameraDeltaR27+=JuiceShakeOffset(dt)');
 const presentation=read('games/flight/crab-flight/Assets/JuiceLayer/JuicePresentation.cs');
 expect(presentation).toContain('feelClockR27.Request(0,JuiceRules.PeakSeconds,JuiceRules.PeakScale)');
 for(const p of fs.readdirSync(path.join(root,'games/flight/crab-flight/Assets/JuiceLayer')).filter(p=>p.endsWith('.cs'))){
  const text=read('games/flight/crab-flight/Assets/JuiceLayer/'+p);
  expect(text).not.toMatch(/AddForce\(|ApplyBreaks\(|Time\.timeScale\s*=|DestructionEvent\s*\+|\.linearVelocity\s*=|Random\./);
 }
 for(const f of ['juice-hud.js','host.js','sound.js','sound-pack.js','feel.js'])cp.execFileSync(process.execPath,['--check',path.join(web,f)]);
});

async function soundHarness(ready=true){
 const meta=JSON.parse(fs.readFileSync(path.join(web,'SoundPack/event-assets.json'),'utf8'));
 const sources=[],fallback=[],timers=new Map();let serial=0,extra=0,enabled=true,paused=false;
 const param=()=>({value:0,setValueAtTime(v){this.value=v;},setTargetAtTime(v){this.value=v;},cancelScheduledValues(){}});
 const ctx={currentTime:1,state:'running',async decodeAudioData(id){return{id};},
  createBufferSource(){const source={playbackRate:param(),connect(node){this.gain=node;},disconnect(){},start(){sources.push(this);},stop(){this.onended?.();}};return source;},
  createGain(){return{gain:param(),connect(){},disconnect(){}};},createAnalyser(){return{fftSize:1024,getFloatTimeDomainData(){}};}};
 const api={graph:()=>({context:ctx,master:{connect(){}},enabled,paused}),play:(...args)=>fallback.push(args),toggle:async()=>true,setPaused:v=>paused=v,setSpin(){},activeVoiceCount:()=>extra};
 const sandbox={window:{FlightSound:api},Float32Array,console,setTimeout(f){timers.set(++serial,f);return serial;},clearTimeout(id){timers.delete(id);},
  fetch:async url=>({ok:true,json:async()=>meta,arrayBuffer:async()=>meta.assets.find(a=>'SoundPack/'+a.path===url).id})};
 vm.runInNewContext(fs.readFileSync(path.join(web,'sound-pack.js'),'utf8'),sandbox);
 if(ready){await api.toggle();for(let i=0;i<200&&!api.packReady();i++)await Promise.resolve();expect(api.packReady()).toBe(true);}
 const state={generation:1,phase:'flight',paused:false,worldX:0,speed:0,events:[],chargeReady:false};
 let id=0;
 function event(type='destruction',material='wood',time=.2){ctx.currentTime+=time;state.events=[{generation:state.generation,id:++id,type,objectId:'target'+id,material,position:{x:0},strength:.7,weight:.9,physicsTime:id*.01}];api.update(state);}
 return {api,ctx,sources,fallback,state,event,timers,meta,extra(v){extra=v;},enabled(v){enabled=v;},finish(){for(const s of sources)s.onended?.();},
  runTimers(){for(const [id,f] of [...timers]){timers.delete(id);f();}}};
}
const materialSources=h=>h.sources.filter(s=>h.meta.assets.find(a=>a.id===s.buffer.id)?.event.startsWith('break.'));

test('existing WAV path rises in semitones, caps at octave, resets and deduplicates',async()=>{
 const h=await soundHarness();
 for(let i=0;i<18;i++){h.finish();h.event();const current=materialSources(h).at(-1);expect(current.playbackRate.value).toBeCloseTo(2**(Math.min(i,12)/12),8);
  const asset=h.meta.assets.find(a=>a.id===current.buffer.id);expect(current.gain.gain.value).toBeCloseTo(asset.gain_linear*.7,8);}
 const count=h.sources.length;h.api.update(h.state);expect(h.sources).toHaveLength(count);
 h.finish();h.event('destruction','glass',1.50001);expect(materialSources(h).at(-1).playbackRate.value).toBe(1);
 h.finish();h.state.generation++;h.event();expect(materialSources(h).at(-1).playbackRate.value).toBe(1);
 h.api.setPaused(true);h.event();const pausedCount=h.sources.length;h.api.update(h.state);expect(h.sources).toHaveLength(pausedCount);
 h.api.setPaused(false);h.finish();h.event();expect(materialSources(h).at(-1).playbackRate.value).toBe(1);
});

test('fallback receives identical pitch and deferred voice keeps event-time pitch',async()=>{
 const fallback=await soundHarness(false);fallback.event();fallback.event();
 expect(fallback.fallback.filter(x=>x[0]==='wood').map(x=>x[2])).toEqual([1,2**(1/12)]);
 const h=await soundHarness();h.event();h.finish();h.extra(11);h.event('charge');h.event();
 expect(h.timers.size).toBe(1);h.extra(0);h.ctx.currentTime+=.035;h.runTimers();
 expect(materialSources(h).at(-1).playbackRate.value).toBeCloseTo(2**(1/12),8);
 expect(h.api.debug().activeVoices).toBeLessThanOrEqual(12);
});

test('audio generation reset cancels delayed voices and mute does not replay missed events',async()=>{
 const h=await soundHarness();h.event();h.finish();h.extra(11);h.event('charge');h.event();expect(h.timers.size).toBe(1);
 h.state.generation++;h.state.events=[];h.api.update(h.state);expect(h.timers.size).toBe(0);
 h.extra(0);h.enabled(false);h.event();const count=h.sources.length;h.enabled(true);h.api.update(h.state);expect(h.sources).toHaveLength(count);
 h.event();expect(materialSources(h).at(-1).playbackRate.value).toBe(1);
});

async function hudFixture(page,size){
 await page.setViewportSize(size);await page.clock.install({time:new Date('2026-11-25T00:00:00Z')});
 let html=fs.readFileSync(path.join(web,'index.html'),'utf8').replace(/<script\b[^>]*>[\s\S]*?<\/script>/g,'');
 html=html.replace('<meta charset=','<base href="https://juice-fixture.invalid/"><meta charset=');
 await page.route('https://juice-fixture.invalid/**',route=>{
  const name=new URL(route.request().url()).pathname.slice(1),file=path.resolve(web,name);
  if(name==='index.html')return route.fulfill({body:html,contentType:'text/html'});
  if(!file.startsWith(web+path.sep)||!fs.existsSync(file))return route.fulfill({status:404,body:''});
  return route.fulfill({body:fs.readFileSync(file),contentType:name.endsWith('.css')?'text/css':'application/octet-stream'});
 });
 await page.goto('https://juice-fixture.invalid/index.html');await page.clock.pauseAt(new Date('2026-11-25T00:01:00Z'));
 await page.evaluate(()=>{
  document.querySelectorAll('.overlay,#aimHUD,#aimActions,#gesture,#launchZone,#kickFeedback').forEach(e=>e.classList.add('hidden'));
  document.body.dataset.view='flight';document.body.dataset.phase='flight';document.getElementById('flightHUD').classList.remove('hidden');
  window.preset='mashimashi';window.FlightFeel={settings:()=>({preset})};
  window.state={generation:1,phase:'flight',destructionEvent:0,paused:false};
  const badge=document.createElement('p');badge.textContent='HUD部品テスト / 合成状態・Unity未起動';badge.style.cssText='position:fixed;bottom:6px;color:white;z-index:50;font-size:12px';document.body.append(badge);
 });
 await page.addScriptTag({path:path.join(web,'juice-hud.js')});await page.evaluate(()=>JuiceHud.update(state));
}
for(const [name,size] of Object.entries({portrait:{width:390,height:844},small:{width:320,height:568},landscape:{width:844,height:390}})){
 test('HUD '+name+': aggregate count catches up, pops and stays inside viewport',async({page})=>{
  const errors=[];page.on('pageerror',e=>errors.push(String(e)));await hudFixture(page,size);
  await page.evaluate(()=>{state.destructionEvent=80;JuiceHud.update(state);});
  await expect(page.locator('#destructionValue')).toHaveText('0');await page.clock.runFor(80);
  const value=Number(await page.locator('#destructionValue').textContent());expect(value).toBeGreaterThan(0);expect(value).toBeLessThan(80);
  const animation=await page.locator('#destructionValue').evaluate(e=>e.getAnimations().at(-1)?.effect.getTiming().duration);expect(animation).toBe(150);
  const box=await page.locator('#destructionValue').evaluate(e=>{
   const a=e.getAnimations().at(-1);a.pause();a.currentTime=52.5;
   const r=document.createRange();r.selectNodeContents(e);const b=r.getBoundingClientRect();return{x:b.left,width:b.width};
  });expect(box.x).toBeGreaterThanOrEqual(2);expect(box.x+box.width+2).toBeLessThanOrEqual(size.width);
  await page.screenshot({path:path.join(evidence,'hud-fixture-'+name+'.png')});
  await page.clock.runFor(400);await expect(page.locator('#destructionValue')).toHaveText('80');
  expect(await page.evaluate(()=>state.destructionEvent)).toBe(80);expect(errors).toEqual([]);
 });
}

test('HUD pause/resume, dense updates, retry and result never leave stale totals',async({page})=>{
 await hudFixture(page,{width:390,height:844});
 await page.evaluate(()=>{state.destructionEvent=50;JuiceHud.update(state);});await page.clock.runFor(80);
 await page.evaluate(()=>{state.paused=true;JuiceHud.update(state);});const shown=await page.locator('#destructionValue').textContent();
 await page.clock.runFor(1000);await expect(page.locator('#destructionValue')).toHaveText(shown);
 await page.evaluate(()=>{state.paused=false;JuiceHud.update(state);});await page.clock.runFor(300);await expect(page.locator('#destructionValue')).toHaveText('50');
 for(let i=1;i<=10;i++){await page.evaluate(i=>{state.destructionEvent=50+i*100;JuiceHud.update(state);},i);await page.clock.runFor(40);}
 await page.clock.runFor(400);await expect(page.locator('#destructionValue')).toHaveText('1050');
 await page.evaluate(()=>{state.phase='result';state.destructionEvent=1080;JuiceHud.update(state);});await expect(page.locator('#destructionValue')).toHaveText('1080');
 await page.evaluate(()=>{state.generation++;state.phase='ready';state.destructionEvent=0;JuiceHud.update(state);});await page.clock.runFor(400);await expect(page.locator('#destructionValue')).toHaveText('0');
 expect(await page.locator('#destructionValue').evaluate(e=>e.getAnimations().length)).toBe(0);
});

test('HUD reduced motion and effects off cancel the animation immediately',async({page})=>{
 await hudFixture(page,{width:390,height:844});
 await page.evaluate(()=>{state.destructionEvent=20;JuiceHud.update(state);});await page.clock.runFor(40);
 await page.emulateMedia({reducedMotion:'reduce'});await page.clock.runFor(20);await expect(page.locator('#destructionValue')).toHaveText('20');
 expect(await page.locator('#destructionValue').evaluate(e=>e.getAnimations().length)).toBe(0);
 await page.emulateMedia({reducedMotion:'no-preference'});
 await page.evaluate(()=>{state.destructionEvent=60;JuiceHud.update(state);});await page.clock.runFor(40);
 await page.evaluate(()=>{preset='off';JuiceHud.refreshSettings();});await expect(page.locator('#destructionValue')).toHaveText('60');
 expect(await page.locator('#destructionValue').evaluate(e=>e.getAnimations().length)).toBe(0);
});
