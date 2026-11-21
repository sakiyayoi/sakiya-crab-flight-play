const fs = require('node:fs');
const path = require('node:path');
const {spawn} = require('node:child_process');
const net = require('node:net');

const dir = __dirname;
const readbackOnly = process.argv.includes('--saved-state-readback');
const evidence = path.join(dir, readbackOnly ? 'life-r6-evidence/save-readback01' : 'life-r6-evidence/functional01');
fs.mkdirSync(evidence, {recursive: true});
const report = {startedUTC: new Date().toISOString(), expectedRevision: 'kani-life-r6', scope: 'New bold king-crab R2b on a partial reconstructed baseline browser loop, not hand operation, performance/M14/40-throw validation or source restoration', events: [], errors: [], milestones: [], throws: []};
const save = () => fs.writeFileSync(path.join(evidence, 'result.json'), JSON.stringify(report, null, 2));
const pause = ms => new Promise(r => setTimeout(r, ms));
let server, chrome, socket;
let sequence = 0, sessionId;
const pending = new Map();

async function command(method, params = {}, page = true) {
  const id = ++sequence;
  const response = new Promise((resolve, reject) => pending.set(id, {resolve, reject}));
  socket.send(JSON.stringify({id, method, params, ...(page ? {sessionId} : {})}));
  return response;
}
async function evaluate(expression) {
  const r = await command('Runtime.evaluate', {expression, returnByValue: true, awaitPromise: true});
  if (r.exceptionDetails) throw new Error(JSON.stringify(r.exceptionDetails));
  return r.result.value;
}
async function waitFor(expression, timeout, label) {
  const start = Date.now();
  while (Date.now() - start < timeout) {
    await command('Page.bringToFront');if(await evaluate("typeof view!=='undefined'&&view==='pause'&&!!lastState?.paused")){report.events.push({type:'ordinary-resume-owned-r5',atUTC:new Date().toISOString()});await click('resume');}if (await evaluate(expression)) return;
    await pause(200);
  }
  throw new Error(label + ' timed out after ' + timeout + 'ms');
}
async function snapshot(name, screenshot = true) {
  const state = await evaluate(`(async()=>({focusTrace:window.__focusTrace||[],view, phase, runtimeReady, pendingPrepareR23, config: {...config}, state: lastState, inputReceipts: typeof readInputReceiptsR23 === 'function' ? await readInputReceiptsR23() : [], throwReceipts: typeof readThrowReceiptsR21 === 'function' ? await readThrowReceiptsR21() : []}))()`);
  fs.writeFileSync(path.join(evidence, name + '.json'), JSON.stringify(state, null, 2));
  if (screenshot) {
    const image = await command('Page.captureScreenshot', {format: 'png', captureBeyondViewport: false});
    fs.writeFileSync(path.join(evidence, name + '.png'), Buffer.from(image.data, 'base64'));
  }
  report.milestones.push({name, atUTC: new Date().toISOString(), view: state.view, phase: state.phase, revision: state.state?.revision, distance: state.state?.distance});
  save();
  console.log('MILESTONE', name, state.view, state.phase, state.state?.distance);
  return state;
}
async function click(id) {
  const p = await evaluate(`(()=>{const e=document.getElementById(${JSON.stringify(id)}),r=e.getBoundingClientRect();if(e.classList.contains('hidden')||!r.width||!r.height)throw Error('Button not visible');return {x:r.x+r.width/2,y:r.y+r.height/2}})()`);
  await command('Input.dispatchMouseEvent', {type: 'mousePressed', x: p.x, y: p.y, button: 'left', clickCount: 1});
  await command('Input.dispatchMouseEvent', {type: 'mouseReleased', x: p.x, y: p.y, button: 'left', clickCount: 1});
}
async function enter() {
  if(await evaluate("view==='pause'")){report.events.push({type:'ordinary-resume-after-focus-pause',atUTC:new Date().toISOString()});save();await click('resume');await waitFor("view!=='pause'",5000,'Ordinary resume');}
  await command('Page.bringToFront');
  await evaluate("document.getElementById('launchZone').focus()");
  await command('Input.dispatchKeyEvent', {type: 'keyDown', key: 'Enter', code: 'Enter', windowsVirtualKeyCode: 13, nativeVirtualKeyCode: 13});
  await command('Input.dispatchKeyEvent', {type: 'keyUp', key: 'Enter', code: 'Enter', windowsVirtualKeyCode: 13, nativeVirtualKeyCode: 13});
}
async function throwOnce(number) {
  await evaluate(`document.getElementById('launchZone').focus()`);
  const start = Date.now(); let presses = 0;
  while (!await evaluate('!!lastState?.chargeReady')) {
    if (Date.now() - start > 120000) throw new Error('Charge did not become ready');
    await enter(); presses++; await pause(150);
  }
  await snapshot('charged-' + number);
  if(number===1){
   await command('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:false});
   await pause(500);await snapshot('mobile-charged-1');
   await command('Emulation.setDeviceMetricsOverride',{width:1280,height:800,deviceScaleFactor:1,mobile:false});
   await pause(500);
  }
  if(number===1)await evaluate("(()=>{const stream=document.getElementById('unity-canvas').captureStream(30);window.__calmVideoChunks=[];window.__calmVideoRecorder=new MediaRecorder(stream,{mimeType:'video/webm;codecs=vp8',videoBitsPerSecond:1800000});window.__calmVideoRecorder.ondataavailable=e=>{if(e.data.size)window.__calmVideoChunks.push(e.data)};window.__calmVideoRecorder.onstop=()=>{stream.getTracks().forEach(t=>t.stop());window.__calmVideoDone=true};window.__calmVideoRecorder.start(250);})()");
  await enter();
  await waitFor(`lastState?.phase==='flight'`, 15000, 'Flight ' + number);
  await pause(600);
  const launched = await snapshot('flight-' + number);
  if(!JSON.parse(launched.state.launchReceipt.bodyJson).shapeIDs.includes('carapace'))throw new Error('King model did not reach actual Player');
  if (!launched.state.launchReceipt?.serial) throw new Error('Missing actual launch receipt');
  await waitFor(`lastState?.phase==='result'`, 300000, 'Result ' + number);
  const result = await snapshot('result-' + number);
  if(number===1){await evaluate("window.__calmVideoRecorder.stop()");await waitFor('window.__calmVideoDone',10000,'Video completion');const clip=await evaluate("new Promise(resolve=>{const r=new FileReader();r.onload=()=>resolve(r.result.split(',')[1]);r.readAsDataURL(new Blob(window.__calmVideoChunks,{type:'video/webm'}));})");fs.writeFileSync(path.join(evidence,'LifeR6-normal-play-VFR.webm'),Buffer.from(clip,'base64'));}
  if(result.state.feel.shake||result.state.feel.cameraMaxOffset!==0||result.state.feel.zoom!==1)throw Error('Default camera motion leaked');
  let audit = null;
  if (result.state.throwAudit) audit = JSON.parse(result.state.throwAudit);
  const item = {number, chargeKeyPresses: presses, launch: launched.state.launchReceipt, final: result.state, audit};
  report.throws.push(item); save();
  if (result.state.revision !== report.expectedRevision) throw new Error('Result version mismatch');
  if (!(Number(result.state.distance) > 0)) throw new Error('No positive travelled distance');
  if (audit?.status !== 'result') throw new Error('Missing completed throw audit');
  return item;
}

(async () => {
  await new Promise((resolve, reject) => {const probe=net.createServer();probe.once('error',reject);probe.listen(8765,'127.0.0.1',()=>probe.close(resolve));});
  const serverLog = fs.openSync(path.join(evidence, 'server.log'), 'a');
  server = spawn('python3', ['-m','http.server','8765','--bind','127.0.0.1','--directory',process.env.CRAB_TARGET_DIRECTORY], {stdio:['ignore',serverLog,serverLog]});
  const profile = path.join(dir, 'life-r6-evidence/functional01', 'isolated-profile');
  if (readbackOnly ? !fs.existsSync(profile) : fs.existsSync(profile)) throw new Error('Unexpected owned profile state; no blind restart');
  const activePort = path.join(profile, 'DevToolsActivePort');
  const previousEndpoint = fs.existsSync(activePort) ? fs.readFileSync(activePort, 'utf8') : '';
  const chromeLog = fs.openSync(path.join(evidence, 'chrome.log'), 'a');
  const args = ['--headless=new','--use-angle=metal','--enable-gpu','--no-first-run','--no-default-browser-check','--disable-sync','--disable-background-networking','--remote-debugging-address=127.0.0.1','--remote-debugging-port=0','--user-data-dir='+profile,'--window-size=1280,800','about:blank'];
  chrome = spawn('/Applications/Google Chrome.app/Contents/MacOS/Google Chrome', args, {stdio:['ignore',chromeLog,chromeLog]});
  report.processes = {node:process.pid, server:server.pid, chrome:chrome.pid, args};save();
  const deadline = Date.now()+30000;
  while (!fs.existsSync(activePort) || fs.readFileSync(activePort, 'utf8') === previousEndpoint) {if(Date.now()>deadline)throw new Error('New Chrome debugging endpoint not ready');await pause(200);}
  const port=fs.readFileSync(activePort,'utf8').split('\n')[0];
  const version=await (await fetch('http://127.0.0.1:'+port+'/json/version')).json();
  report.browser=version.Browser;
  socket = new WebSocket(version.webSocketDebuggerUrl);
  await new Promise((resolve,reject)=>{socket.addEventListener('open',resolve,{once:true});socket.addEventListener('error',reject,{once:true});});
  socket.addEventListener('message', event => {
    const m=JSON.parse(event.data);
    if(m.id){const p=pending.get(m.id);if(p){pending.delete(m.id);m.error?p.reject(new Error(JSON.stringify(m.error))):p.resolve(m.result);}return;}
    if(m.method==='Runtime.exceptionThrown')report.errors.push({type:'pageerror',data:m.params.exceptionDetails});
    if(m.method==='Runtime.consoleAPICalled'&&m.params.type==='error')report.errors.push({type:'console',data:m.params.args.map(x=>x.value??x.description)});
    if(m.method==='Network.loadingFailed')report.errors.push({type:'requestfailed',data:m.params});
    if(m.method==='Network.responseReceived'&&m.params.response.status>=400)report.errors.push({type:'httperror',status:m.params.response.status,url:m.params.response.url});
  });
  const targets=await command('Target.getTargets',{},false);
  const target=targets.targetInfos.find(t=>t.type==='page'&&t.url==='about:blank');
  sessionId=(await command('Target.attachToTarget',{targetId:target.targetId,flatten:true},false)).sessionId;
  await command('Page.enable');await command('Runtime.enable');await command('Network.enable');
  await command('Emulation.setDeviceMetricsOverride',{width:1280,height:800,deviceScaleFactor:1,mobile:false});
  await command('Page.addScriptToEvaluateOnNewDocument',{source:"if(location.hostname==='127.0.0.1')localStorage.setItem('crab-flight-feel-v1',JSON.stringify({preset:'mashimashi',shake:true}));"});
  await command('Page.navigate',{url:'http://127.0.0.1:8765/'});
  await waitFor(`typeof lastState!=='undefined'&&lastState?.runtimeReady&&view==='title'`,180000,'Runtime title');
  await evaluate("window.__focusTrace=[];for(const n of ['blur','focus','visibilitychange'])window.addEventListener(n,()=>window.__focusTrace.push({event:n,t:performance.now(),hidden:document.hidden}));");
  const title=await snapshot('title');
  report.legacySavedShake=await evaluate("JSON.parse(localStorage.getItem('crab-flight-feel-v1')).shake");report.calmSettings=await evaluate('FlightFeel.settings()');if(report.legacySavedShake!==true||report.calmSettings.shake!==false)throw Error('Legacy shake ON was not migrated safely to default OFF');
  if(title.state.revision!==report.expectedRevision)throw new Error('Loaded executable version mismatch');
  report.initialSound = await evaluate(`FlightSound.graph().enabled`);
  if(report.initialSound!==false)throw new Error('Sound unexpectedly enabled');
  if (readbackOnly) {
    report.scope='Saved-state readback only, no additional throw or game change; original loop snapshots did not await nested archive Promises and do not prove archive emptiness';
    const original=JSON.parse(fs.readFileSync(path.join(dir,'life-r6-evidence/functional01','result.json'),'utf8'));
    const saved=await evaluate(`(async()=>({savedBest, bestText:document.getElementById('best').textContent, archive:await readThrowReceiptsR21()}))()`);
    if(saved.savedBest!==Math.max(...original.throws.map(t=>t.final.best)))throw new Error('Best did not persist across browser restart');
    for(const item of original.throws){const row=saved.archive.find(x=>x.id===item.audit.id);if(!row||row.status!=='result'||row.distance!==item.final.distance)throw new Error('Completed throw archive did not persist');}
    report.savedProgress={best:saved.savedBest,bestText:saved.bestText,archive:saved.archive.map(x=>({id:x.id,status:x.status,distance:x.distance,revision:x.revision}))};
    if(report.errors.length)throw new Error('Readback browser errors captured');
    report.status='PASS_RECON_SAVED_STATE_READBACK';return;
  }
  await click('start');
  await waitFor(`view==='aim'&&!pendingPrepareR23&&lastState?.phase==='ready'&&!lastState.chargeStarted`,20000,'Ready');
  await snapshot('ready');
  await command('Emulation.setDeviceMetricsOverride',{width:390,height:844,deviceScaleFactor:1,mobile:false});
  await pause(500);await snapshot('mobile-ready');
  await command('Emulation.setDeviceMetricsOverride',{width:1280,height:800,deviceScaleFactor:1,mobile:false});
  await pause(500);
  await throwOnce(1);
  await click('retry');
  await waitFor(`view==='aim'&&!pendingPrepareR23&&lastState?.phase==='ready'&&!lastState.chargeStarted`,20000,'Retry ready');
  const reset=await snapshot('retry-ready');
  if(reset.state.launchReceipt?.serial!==0||reset.state.throwAudit!=='')throw new Error('Previous launch state survived retry');
  await command('Emulation.setEmulatedMedia',{features:[{name:'prefers-reduced-motion',value:'reduce'}]});
  await evaluate("FlightFeel.setSettings({preset:'off',shake:false})");
  await waitFor("lastState?.feel?.preset==='off'&&lastState?.reducedMotion",10000,'OFF and reduced acknowledged');
  report.offFlashStart=await evaluate('lastState.feel.flashCount');
  await throwOnce(2);
  report.offFlashEnd=await evaluate('lastState.feel.flashCount');if(report.offFlashStart!==report.offFlashEnd)throw Error('Flash admitted while OFF/reduced');
  if(report.errors.length)throw new Error('Browser errors captured: '+report.errors.length);
  report.focusTrace=await evaluate('window.__focusTrace');
  report.status='PASS_LIFE_R6_NORMAL_LOOPS_OFF_REDUCED_AND_SHAKE_OFF';
})().catch(error=>{report.status='FAIL';report.failure=String(error.stack??error);console.log('SMOKE_FAIL',report.failure);process.exitCode=1;}).finally(async()=>{
  report.endedUTC=new Date().toISOString();save();
  if(socket?.readyState===1){await Promise.race([command('Browser.close',{},false).catch(()=>{}),pause(1500)]);socket.close();}
  if(chrome?.exitCode===null)chrome.kill('SIGTERM');
  if(server?.exitCode===null)server.kill('SIGTERM');
  console.log('SMOKE_TERMINAL',report.status,'throws',report.throws.length,'errors',report.errors.length);
});
