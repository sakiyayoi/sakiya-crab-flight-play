// Offline source/policy receipts and the actual host adapter with a stubbed Unity bridge.
// These do NOT play Unity, evaluate PhysX, or certify visual/gameplay completion.
const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),cp=require('node:child_process'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),project='games/flight/crab-flight/',web='games/flight/web-template/';
const read=f=>fs.readFileSync(path.join(root,f),'utf8'),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const base='2d0d849120b0bc0002173f49b2db0fd7f6f05987';
const source=f=>read(project+'Assets/'+f);
test('current managed compilation and executed C# policy contracts',()=>{
 const r=JSON.parse(read('docs/new-loop/fix/compile.json'));expect(r.exitCode).toBe(0);expect(r.errors).toBe(0);
 for(const [name,digest]of Object.entries(r.sources))expect(sha(fs.readFileSync(path.join(root,project,name))),name).toBe(digest);
 expect(read('docs/new-loop/fix/contracts.log')).toMatch(/TOTAL \d+ passed \/ 0 failed/);
});
test('26 inherited dirty files and water fix remain byte identical',()=>{
 const r=JSON.parse(read('docs/new-loop/preexisting-worktree.json'));expect(Object.keys(r)).toHaveLength(26);
 for(const [f,h]of Object.entries(r))expect(sha(fs.readFileSync(path.join(root,f))),f).toBe(h);
 for(const f of ['PortPrototype/PortHeroVisual.cs','NeonProto/NeonCrabVisual.cs','PortPrototype/PortRules.cs','PortPrototype/Resources/PortPrototype/Layout.json']){
  const p=project+'Assets/'+f;expect(sha(fs.readFileSync(path.join(root,p)))).toBe(sha(cp.execFileSync('git',['show',base+':'+p],{cwd:root})));
 }
 expect(source('Scripts/CrabFlightGame.cs')).toContain('void LateUpdate(){SyncStampR8(5);neonCrab?.SyncTankVisibility(phase);}');
});
test('segment membership preserves whole authored chain sets',()=>{
 const entries=JSON.parse(source('PortPrototype/Resources/PortPrototype/Layout.json')).instances;
 for(const [beat,start,end,total]of [['S5',697,757,54],['S7',781,848,41]]){
  const es=entries.filter(e=>e.beat===beat);expect(es).toHaveLength(total);expect(end-start).toBeGreaterThanOrEqual(40);expect(end-start).toBeLessThanOrEqual(80);
  for(const e of es){expect(e.p[0]).toBeGreaterThanOrEqual(start);expect(e.p[0]).toBeLessThanOrEqual(end);if(e.chain)expect(entries.filter(v=>v.chain===e.chain).every(v=>v.beat===beat)).toBe(true);}
 }
 expect(entries.filter(e=>e.chain==='yard-main')).toHaveLength(34);
 // Graph coverage is a topology check, NOT an observed destruction count.
 const yard=entries.filter(e=>e.chain==='yard-main'),ignition=yard.find(e=>e.role==='jackpot');
 const seen=new Set([ignition.id]),queue=[ignition];while(queue.length){const a=queue.shift();for(const b of yard)if(!seen.has(b.id)&&Math.hypot(...a.p.map((v,i)=>v-b.p[i]))<=8){seen.add(b.id);queue.push(b);}}
 expect(seen.size).toBe(34);
});
test('direct wave and beam enter one accounting pipeline, with explicit causes and real-time stagger',()=>{
 const physics=source('PortPrototype/NewLoopPhysics.cs'),roles=source('PortPrototype/PortRoles.cs'),breaks=source('Scripts/RestaurantPhysics.cs');
 const wave=physics.slice(physics.indexOf('void QueueNewLoopWave'),physics.indexOf('void ExplodeNewLoopShards'));
 expect(wave).toContain('pendingBreaks[p]=');expect(wave).not.toMatch(/broken\s*=|BreakEvents|threshold\s*=/);
 expect(roles).toContain('if(NewLoopActive){QueueNewLoopWave(w);continue;}');
 expect(roles).toContain('(NewLoopActive?newLoopWaveClock:Time)<w.due');
 expect(physics).toContain('NewLoopRules.HopDue(newLoopWaveClock,step.hop)');
 expect(physics).toContain('cause=step.hop<=1?"wave-direct":"chain"');
 expect(physics).toContain('newLoopBeamPending.Contains(p)?"beam"');
 expect(breaks).toContain('!(NewLoopActive&&(newLoopBeamPending.Contains(p)||newLoopDirectPending.ContainsKey(p)))&&breakImpulses[p]/Body.mass<effectiveThreshold');
 expect((breaks.match(/DestructionEvent\+\+/g)||[])).toHaveLength(1);
 expect(physics).toContain('if(!NewLoopActive||NewLoopReleased||NewLoopParked||!NewLoopTarget(p))return;');
 const reset=physics.slice(physics.indexOf('void RestoreNewLoopRest'),physics.indexOf('public void ReleaseNewLoopActor'));
 expect(reset).not.toMatch(/Physics\.Simulate\(/);expect(reset).toContain('timer.Elapsed.TotalMilliseconds');
 expect(physics).not.toContain('p.x=Mathf.Clamp');
 expect(source('Scripts/RigidCrabFlight.cs')).toContain('if(NewLoopActive){pressed=braced=false;return false;}');
 expect(source('Scripts/CrabFlightGame.cs')).toContain('spinAngle=useNewLoop?0:displaySpinAngle');
});
async function fixture(page,width=390,height=844){
 await page.setViewportSize({width,height});
 const html=read(web+'index.html').replace(/<script\b[^>]*>[\s\S]*?<\/script>/gi,'').replace(/<link[^>]+href="(?:game-ui|port-presentation|new-loop)\.css"[^>]*>/g,'');
 await page.setContent(html);
 for(const f of ['game-ui.css','port-presentation.css','new-loop.css'])await page.addStyleTag({content:read(web+f)});
 await page.evaluate(()=>{
  window.view='aim';window.pendingPrepareR23=0;window.runtimeReady=true;window.config={angle:18};window.sent=[];
  window.send=(method,value='')=>sent.push({method,value});window.configure=()=>send('Configure',JSON.stringify(config));
  window.setView=v=>{window.view=v;document.body.dataset.view=v;for(const id of ['loading','title','aimHUD','launchZone','aimActions','flightHUD','result'])document.getElementById(id).classList.add('hidden');for(const id of ({aim:['aimHUD','launchZone','aimActions'],flight:['flightHUD'],result:['result']}[v]||[]))document.getElementById(id).classList.remove('hidden');};
  setView('aim');document.getElementById('retry').onclick=()=>send('Prepare');
  window.fixtureState={phase:'ready',paused:false,attempts:1,generation:1,feel:{preset:'mashimashi',shake:true,reduced:false},newLoop:{segment:0,segmentName:'S5',tier:2,energy:.7,aimAngle:18,held:false,spinCount:8,maxCue:0,beamArmed:false,beamUsed:false,beamActive:false,total:54,broken:0,destructionRate:0,hasPrevious:false,cleared:false,elapsed:0,releaseAge:0}};
 });
 await page.addScriptTag({content:read(web+'new-loop.js')});
 await page.evaluate(()=>NewLoopUI.update(fixtureState));
}
test('actual host adapter: pointer/keyboard charge and release; beam hold only',async({page})=>{
 const errors=[];page.on('pageerror',e=>errors.push(String(e)));await fixture(page);
 const z=await page.locator('#launchZone').boundingBox();await page.mouse.move(z.x+z.width/2,z.y+z.height/2);await page.mouse.down();await page.mouse.move(z.x+z.width/2,z.y+z.height/2-35);await page.mouse.up();
 let sent=await page.evaluate(()=>sent);expect(sent.map(s=>s.method)).toEqual(['BeginSpinCharge','BeginAim','PreviewLaunch','PreviewLaunch','ReleaseLaunch']);expect(JSON.parse(sent.at(-1).value).angle).toBeGreaterThan(18);
 await page.evaluate(()=>sent.length=0);await page.locator('#launchZone').focus();await page.keyboard.down('Enter');await page.keyboard.press('ArrowUp');await page.keyboard.up('Enter');
 sent=await page.evaluate(()=>sent);expect(sent.filter(s=>s.method==='ReleaseLaunch')).toHaveLength(1);expect(sent.some(s=>s.method==='KickDown')).toBe(false);
 await page.evaluate(()=>{sent.length=0;setView('flight');fixtureState.phase='flight';fixtureState.newLoop.beamArmed=true;NewLoopUI.update(fixtureState);});
 await page.keyboard.down('Space');await page.keyboard.up('Space');expect(await page.evaluate(()=>sent)).toEqual([{method:'SetNewLoopBeamHeld',value:'1'},{method:'SetNewLoopBeamHeld',value:'0'}]);
 await expect(page.locator('#kick')).toBeHidden();expect(errors).toEqual([]);
});
for(const [width,height]of [[390,844],[844,390],[320,568]])test(`result adapter ${width}x${height}: score, rate, comparison and retry together`,async({page})=>{
 await fixture(page,width,height);
 await page.evaluate(()=>{setView('result');fixtureState.phase='result';Object.assign(fixtureState.newLoop,{broken:38,destructionRate:38/54,cleared:true,hasPrevious:true,previousBroken:10,delta:28,nextUnlocked:true});NewLoopUI.update(fixtureState);});
 await expect(page.locator('#destructionResultValue')).toHaveText('38');await expect(page.locator('#newLoopResult')).toContainText('70.4%');await expect(page.locator('#newLoopCompare')).toContainText('+28');
 for(const id of ['destructionResultValue','newLoopResult','newLoopCompare','retry']){await expect(page.locator('#'+id)).toBeVisible();const r=await page.locator('#'+id).boundingBox();expect(r.y).toBeGreaterThanOrEqual(0);expect(r.y+r.height).toBeLessThanOrEqual(height);}
 expect(await page.evaluate(()=>document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
 await page.locator('#retry').click();expect(await page.evaluate(()=>sent.at(-1).method)).toBe('Prepare');
 await page.screenshot({path:path.join(root,`docs/new-loop/fix/ui-fixture-${width}x${height}.png`)});
});
test('adapter OFF leaves old inputs unhandled and new controls hidden',async({page})=>{
 await fixture(page);await page.evaluate(()=>{fixtureState.newLoop=null;NewLoopUI.update(fixtureState);sent.length=0;});
 await page.locator('#launchZone').click();await page.locator('#launchZone').press('Enter');expect(await page.evaluate(()=>sent)).toEqual([]);
 await expect(page.locator('#newLoopBeam')).toBeHidden();await expect(page.locator('#newLoopResult')).toBeHidden();
});
test('shipping template includes adapter, styles and gated progression; JS parses',()=>{
 const html=read(web+'index.html');expect(html).toContain('href="new-loop.css"');expect(html).toContain('src="new-loop.js"');
 expect(html.indexOf('src="new-loop.js"')).toBeLessThan(html.indexOf('src="host.js"'));
 const host=read(web+'host.js');expect(host).toContain('async function settleProgress(s){if(s.newLoop)return;');expect(host).toContain('if(!invalid&&!s.newLoop){savedBest=');
 for(const f of ['new-loop.js','host.js','sound-pack.js','flight-cues.js'])cp.execFileSync(process.execPath,['--check',path.join(root,web,f)]);
 expect(source('Scripts/DestructionProgressionR12.cs')).toContain('if(NewLoopActive&&(!NewLoopReleased||NewLoopParked))return;');
});
test('charge spin counter and MAX are present, and charge HUD fits mobile/landscape',async({page})=>{
 for(const [width,height]of [[390,844],[320,568],[844,390]]){
  await fixture(page,width,height);
  await page.evaluate(()=>{Object.assign(fixtureState.newLoop,{tier:3,energy:1,spinCount:17,maxCue:1,held:true,beamArmed:true});NewLoopUI.update(fixtureState);});
  await expect(page.locator('#newLoopSpin')).toContainText('回転数');await expect(page.locator('#newLoopSpinValue')).toHaveText('17');await expect(page.locator('#newLoopMax')).toBeVisible();
  for(const id of ['chargeGauge','newLoopSpin','newLoopStatus','newLoopMax']){
   const r=await page.locator('#'+id).boundingBox();expect(r.x).toBeGreaterThanOrEqual(0);expect(r.x+r.width).toBeLessThanOrEqual(width);expect(r.y+r.height).toBeLessThan(height);
  }
  await page.screenshot({path:path.join(root,`docs/new-loop/fix/ui-charge-fixture-${width}x${height}.png`)});
 }
});
test('break count ticks per accepted count, deduplicates snapshots, respects reduced/effects-off and resets',async({page})=>{
 await fixture(page);
 await page.evaluate(()=>{window.tickValues=[];new MutationObserver(()=>tickValues.push(Number(document.getElementById('destructionValue').textContent))).observe(document.getElementById('destructionValue'),{childList:true});setView('flight');fixtureState.phase='flight';fixtureState.newLoop.broken=5;NewLoopUI.update(fixtureState);});
 await expect(page.locator('#destructionValue')).toHaveText('5');
 expect(await page.evaluate(()=>tickValues.filter((v,i,a)=>i===0||v!==a[i-1]))).toEqual([0,1,2,3,4,5]);
 await page.evaluate(()=>NewLoopUI.update(fixtureState));await expect(page.locator('#destructionValue')).toHaveText('5');
 await page.emulateMedia({reducedMotion:'reduce'});
 await page.evaluate(()=>{fixtureState.newLoop.broken=10;NewLoopUI.update(fixtureState);});await expect(page.locator('#destructionValue')).toHaveText('10');
 expect(await page.locator('#destructionValue').evaluate(el=>el.getAnimations().filter(a=>a.playState==='running').length)).toBe(0);
 await page.emulateMedia({reducedMotion:'no-preference'});
 await page.evaluate(()=>{fixtureState.feel.preset='off';fixtureState.newLoop.broken=15;NewLoopUI.update(fixtureState);});await expect(page.locator('#destructionValue')).toHaveText('15');
 expect(await page.locator('#destructionValue').evaluate(el=>el.getAnimations().length)).toBe(0);
 await page.evaluate(()=>{fixtureState.phase='ready';fixtureState.generation++;fixtureState.newLoop.broken=0;fixtureState.newLoop.spinCount=0;NewLoopUI.update(fixtureState);});
 await expect(page.locator('#destructionValue')).toHaveText('0');await expect(page.locator('#newLoopSpinValue')).toHaveText('0');
});
test('ON presentation is render-only, all added motion gated, and protected authoring inputs unchanged',()=>{
 const show=source('PortPrototype/NewLoopPresentation.cs'),game=source('PortPrototype/NewLoopGame.cs');
 expect(show).toContain('neonCrab.Root.rotation=');expect(show).toContain('neonCrab.Root.localScale*=');
 expect(show).not.toMatch(/crab.Root\.(?:localScale|rotation)\s*[*=]|UnityEngine.Random|Random.Range/);
 expect(show).toContain('feelGateR27.TryDisplay');expect(show).toContain('newLoopDimCommands.DrawRenderer');expect(show).toContain('SetNewLoopDim(max?.15f');
 expect(game).toContain('NewLoopMotion&&feelSettingsR27.shake?1-(1-NewLoopRules.CameraPunchScale)');
 expect(game).toContain('if(NewLoopMotion&&feelSettingsR27.shake&&phase=="ready"&&spinCharge.Held)');
 expect(source('Scripts/City2Contacts.cs')).toContain('if(NewLoopActive&&NewLoopReleased&&NewLoopTarget(p))ExplodeNewLoopShards');
 // Both wave and beam use the same destruction event => port presentation path.
 expect(source('Scripts/FeelDirectorR27.cs')).toContain('portEntry.asset,cam,now,c.velocity,useNewLoop&&NewLoopMotion)');
 const dirty=JSON.parse(read('docs/new-loop/preexisting-worktree.json'));
 const allow=new Set(['PortPrototype/NewLoopGame.cs','PortPrototype/NewLoopPhysics.cs','PortPrototype/NewLoopRules.cs','PortPrototype/PortRoles.cs','PortPrototype/PortParticles.cs','Scripts/City2Contacts.cs','Scripts/CrabFlightGame.cs','Scripts/FeelDirectorR27.cs','Scripts/RestaurantPhysics.cs']);
 const entries=cp.execFileSync('git',['ls-tree','-r','dad6137','--',project+'Assets'],{cwd:root,encoding:'utf8'}).trim().split('\n');let count=0;
 for(const row of entries){const [meta,p]=row.split('\t');if(!/\.(?:cs|shader|json|fbx)$/.test(p)||dirty[p]||allow.has(p.replace(project+'Assets/','')))continue;
  const blob=meta.split(' ')[2];expect(cp.execFileSync('git',['hash-object',p],{cwd:root,encoding:'utf8'}).trim(),p).toBe(blob);count++;
 }
 expect(count).toBeGreaterThan(450);
});
test('existing charge assets get exponential gain/pitch and immediate tier-up cue; OFF blend unchanged',async({page})=>{
 const metadata=JSON.parse(read(web+'SoundPack/event-assets.json'));
 await page.setContent('<div>Offline audio-graph contract (no listening)</div>');
 await page.evaluate(meta=>{
  window.audioCalls=[];let assetIndex=0;
  const param=(kind,node)=>({value:0,setValueAtTime(v,t){audioCalls.push({kind,node,v,t,method:'set'});},setTargetAtTime(v,t,tau){audioCalls.push({kind,node,v,t,tau,method:'target'});},exponentialRampToValueAtTime(v,t){audioCalls.push({kind,node,v,t,method:'ramp'});},cancelScheduledValues(){}});
  const ctx={currentTime:1,state:'running',createBufferSource(){const node={id:null};return {set buffer(v){node.id=v.id;},playbackRate:param('pitch',node),connect(){},disconnect(){},start(){audioCalls.push({kind:'start',node});},stop(){this.onended?.();}};},createGain(){const node={id:audioCalls.length};return {gain:param('gain',node),connect(){},disconnect(){}};},createAnalyser(){return {getFloatTimeDomainData(){}};},async decodeAudioData(){return {id:meta.assets[assetIndex++].id};}};
  window.audioContext=ctx;window.FlightSound={graph:()=>({context:ctx,master:{connect(){}},enabled:true,paused:false}),play(){},toggle:async()=>true,setPaused(){},setSpin(){},activeVoiceCount:()=>0};
  window.fetch=async url=>({ok:true,json:async()=>meta,arrayBuffer:async()=>new ArrayBuffer(1)});
 },metadata);
 await page.addScriptTag({content:read(web+'sound-pack.js')});
 await page.evaluate(()=>FlightSound.toggle());await expect.poll(()=>page.evaluate(()=>FlightSound.packReady())).toBe(true);
 const values=await page.evaluate(()=>{
  const s={phase:'ready',generation:1,chargeStarted:true,events:[],newLoop:{held:true,tier:1,energy:0,beamArmed:false}};const gains=[];
  for(const e of [0,.25,.5,.75,1]){s.newLoop.energy=e;s.newLoop.tier=e===1?3:e>=.5?2:1;audioCalls.length=0;audioContext.currentTime+=.3;FlightSound.update(s);gains.push(Math.max(...audioCalls.filter(x=>x.kind==='gain'&&x.method==='target').map(x=>x.v)));}
  audioCalls.length=0;s.events=[{id:1,generation:1,type:'charge-tier',objectId:'3',strength:1}];FlightSound.update(s);
  const cue=audioCalls.filter(x=>x.kind==='pitch'&&x.method==='ramp');
  audioCalls.length=0;s.newLoop=null;s.chargeEnergy=.5;s.chargeReady=false;s.events=[];FlightSound.update(s);
  return {gains,cue,off:audioCalls.filter(x=>x.kind==='gain'&&x.method==='target').map(x=>x.v)};
 });
 const gains=values.gains;expect(gains.every((v,i)=>i===0||v>gains[i-1])).toBe(true);expect(gains[4]-gains[3]).toBeGreaterThan(gains[1]-gains[0]);
 expect(values.cue).toHaveLength(1);expect(values.cue[0].node.id).toBe('charge_ready_clack');expect(values.cue[0].v).toBe(1.65);
 const mid=metadata.assets.find(a=>a.id==='charge_mid_loop');expect(Math.max(...values.off)).toBeCloseTo(mid.gain_linear*.49,7);
});
