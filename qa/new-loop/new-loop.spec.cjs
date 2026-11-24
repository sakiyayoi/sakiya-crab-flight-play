// Offline source/policy receipts and the actual host adapter with a stubbed Unity bridge.
// These do NOT play Unity, evaluate PhysX, or certify visual/gameplay completion.
const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),cp=require('node:child_process'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),project='games/flight/crab-flight/',web='games/flight/web-template/';
const read=f=>fs.readFileSync(path.join(root,f),'utf8'),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const base='2d0d849120b0bc0002173f49b2db0fd7f6f05987';
const source=f=>read(project+'Assets/'+f);
test('current managed compilation and executed C# policy contracts',()=>{
 const r=JSON.parse(read('docs/new-loop/compile.json'));expect(r.exitCode).toBe(0);expect(r.errors).toBe(0);
 for(const [name,digest]of Object.entries(r.sources))expect(sha(fs.readFileSync(path.join(root,project,name))),name).toBe(digest);
 expect(read('docs/new-loop/contracts.log')).toMatch(/TOTAL \d+ passed \/ 0 failed/);
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
test('wave impulses and direct beam share only the real break accounting',()=>{
 const physics=source('PortPrototype/NewLoopPhysics.cs'),roles=source('PortPrototype/PortRoles.cs'),breaks=source('Scripts/RestaurantPhysics.cs');
 const wave=physics.slice(physics.indexOf('void ImpulseNewLoopPanel'),physics.indexOf('void SyncNewLoopProxies'));
 expect(wave).toContain('ForceMode.Impulse');expect(wave).not.toMatch(/pendingBreaks|broken\s*=|BreakEvents|threshold\s*=/);
 expect(roles).toContain('if(NewLoopActive){ImpulseNewLoopPanel(w);continue;}');
 expect(physics).toContain('QueueNewLoopSolverBreak(a,c.impulse.magnitude');expect(physics).toContain('newLoopBeamPending.Contains(p)?"beam"');
 expect(breaks).toContain('!(NewLoopActive&&newLoopBeamPending.Contains(p))&&breakImpulses[p]/Body.mass<effectiveThreshold');
 expect((breaks.match(/DestructionEvent\+\+/g)||[])).toHaveLength(1);
 expect(physics).toContain('if(!NewLoopActive||NewLoopReleased||NewLoopParked||!NewLoopTarget(p))return;');
 const reset=physics.slice(physics.indexOf('void RestoreNewLoopRest'),physics.indexOf('public void ReleaseNewLoopActor'));
 expect(reset).not.toMatch(/Physics\.Simulate\(/);expect(reset).toContain('timer.Elapsed.TotalMilliseconds');
 expect(physics).not.toContain('p.x=Mathf.Clamp'); // no actor teleport at segment edge
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
  window.fixtureState={phase:'ready',paused:false,attempts:1,newLoop:{segment:0,segmentName:'S5',tier:2,energy:.7,aimAngle:18,held:false,beamArmed:false,beamUsed:false,beamActive:false,total:54,broken:0,destructionRate:0,hasPrevious:false,cleared:false,elapsed:0,releaseAge:0}};
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
 await page.screenshot({path:path.join(root,`docs/new-loop/ui-fixture-${width}x${height}.png`)});
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
