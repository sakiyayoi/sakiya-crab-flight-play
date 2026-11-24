'use strict';
// Host adapter only. Scores, charge tiers and clear judgement always come from Unity.
window.NewLoopUI=(()=>{
 const $=id=>document.getElementById(id),zone=$('launchZone');
 const status=document.createElement('p');status.id='newLoopStatus';status.setAttribute('aria-live','polite');$('aimHUD').append(status);
 const beam=document.createElement('button');beam.id='newLoopBeam';beam.hidden=true;beam.textContent='押している間 ビーム';$('stage').append(beam);
 const result=document.createElement('p');result.id='newLoopResult';result.setAttribute('aria-live','polite');$('destructionResult').after(result);
 const compare=document.createElement('p');compare.id='newLoopCompare';result.after(compare);
 const next=document.createElement('button');next.id='newLoopNext';next.hidden=true;next.textContent='次の区間へ';$('retry').after(next);
 const segment=document.createElement('button');segment.id='newLoopSegment';segment.hidden=true;segment.textContent='S5に戻る';$('aimActions').append(segment);
 let state=null,pointer=null,keyHeld=false,startY=0,startAngle=18,angle=18,beamPointer=null,beamKey=false,lastResultAt=0,lastAttempt=-1;
 const active=()=>!!state?.newLoop;
 const aiming=()=>active()&&view==='aim'&&state.phase==='ready'&&!state.paused&&!pendingPrepareR23&&runtimeReady;
 const stop=e=>{e.preventDefault();e.stopImmediatePropagation();};
 const payload=()=>JSON.stringify({angle,power:50,direction:1,spinRps:0,spinPhase:0,eligible:true});
 function preview(){send('PreviewLaunch',payload());}
 function begin(){send('BeginSpinCharge');send('BeginAim');preview();document.body.dataset.dragging='true';}
 function release(){config.angle=angle;send('ReleaseLaunch',payload());document.body.dataset.dragging='false';}
 function cancel(){if(pointer!==null||keyHeld)send('CancelAim');pointer=null;keyHeld=false;document.body.dataset.dragging='false';setBeam(false);}
 function setBeam(held){if(active())send('SetNewLoopBeamHeld',held?'1':'0');}
 window.addEventListener('pointerdown',e=>{
  if(!aiming()||!(e.target===zone||zone.contains(e.target))||e.button!==0)return;
  stop(e);if(pointer!==null||keyHeld)return;pointer=e.pointerId;startY=e.clientY;startAngle=angle;
  zone.focus({preventScroll:true});try{zone.setPointerCapture(pointer);}catch{pointer=null;return;}begin();
 },true);
 window.addEventListener('pointermove',e=>{
  if(pointer===null||e.pointerId!==pointer)return;stop(e);
  angle=Math.min(45,Math.max(10,startAngle+(startY-e.clientY)*.16));preview();
 },true);
 window.addEventListener('pointerup',e=>{
  if(pointer===null||e.pointerId!==pointer)return;stop(e);pointer=null;release();
 },true);
 window.addEventListener('pointercancel',e=>{if(e.pointerId===pointer){stop(e);cancel();}if(e.pointerId===beamPointer){beamPointer=null;setBeam(false);}},true);
 zone.addEventListener('lostpointercapture',()=>{if(pointer!==null)cancel();});
 window.addEventListener('keydown',e=>{
  if(active()&&view==='flight'&&e.code==='Space'){stop(e);if(!e.repeat){beamKey=true;setBeam(true);}return;}
  if(!aiming()||document.activeElement!==zone)return;
  if(['ArrowUp','ArrowDown','ArrowLeft','ArrowRight'].includes(e.code)){
   stop(e);angle=Math.min(45,Math.max(10,angle+(e.code==='ArrowUp'?2:e.code==='ArrowDown'?-2:0)));
   if(keyHeld||pointer!==null)preview();else{config.angle=angle;configure();}return;
  }
  if(e.code==='Enter'){stop(e);if(!e.repeat&&!keyHeld&&pointer===null){keyHeld=true;begin();}}
 },true);
 window.addEventListener('keyup',e=>{
  if(keyHeld&&e.code==='Enter'){stop(e);keyHeld=false;release();}
  if(beamKey&&e.code==='Space'){stop(e);beamKey=false;setBeam(false);}
 },true);
 beam.addEventListener('pointerdown',e=>{if(!active()||beam.disabled||e.button!==0)return;e.preventDefault();beamPointer=e.pointerId;beam.setPointerCapture(e.pointerId);setBeam(true);});
 beam.addEventListener('pointerup',e=>{if(e.pointerId===beamPointer){beamPointer=null;setBeam(false);}});
 beam.addEventListener('lostpointercapture',()=>{beamPointer=null;setBeam(false);});
 window.addEventListener('blur',cancel);document.addEventListener('visibilitychange',()=>{if(document.hidden)cancel();});
 window.addEventListener('resize',cancel);
 next.onclick=()=>{send('SelectNewLoopSegment','1');setView('aim');};
 segment.onclick=()=>{send('SelectNewLoopSegment','0');setView('aim');};
 function update(s){
  state=s;const n=s.newLoop;document.body.dataset.newLoop=String(!!n);
  beam.hidden=!n||view!=='flight'||!n.beamArmed;segment.hidden=!n||view!=='aim'||n.segment===0;
  next.hidden=!n||view!=='result'||n.segment!==0||!n.nextUnlocked;
  if(!n){status.textContent=result.textContent=compare.textContent='';return;}
  if(s.phase==='ready'&&pointer===null&&!keyHeld)angle=Math.min(45,Math.max(10,n.aimAngle));
  if(s.phase==='result'&&lastAttempt!==s.attempts){lastAttempt=s.attempts;lastResultAt=performance.now();}
  if(s.phase==='ready'&&lastResultAt){document.body.dataset.newLoopResultToReadyMs=String(performance.now()-lastResultAt);lastResultAt=0;}
  document.body.dataset.chargeTier=String(n.tier);zone.setAttribute('aria-label','押して回す。押したまま上下で狙い、離して発射。キーボードは上下で角度、Enterを押してため、離して発射。');
  zone.querySelector('span').textContent='押して回す・上下で狙う・離して発射';
  $('chargeGauge').setAttribute('aria-label','解放エネルギー');document.querySelector('#title .title-copy p').textContent='回してためて、港を壊せ。';
  $('chargeTime').textContent=Math.round(n.energy*100)+'%';$('chargeLabel').textContent=n.beamArmed?'ビーム準備OK':n.overSpun?'回しすぎ！':'解放エネルギー';
  $('chargeFill').style.width=n.energy*100+'%';$('chargeGauge').setAttribute('aria-valuenow',String(Math.round(n.energy*100)));
  $('angleValue').textContent=Math.round(n.aimAngle)+'°';
  status.textContent=n.segmentName+'　'+(n.dizzy?'ふらふら！ 離すと止まる':n.overSpun?'エネルギー減衰中':n.beamArmed?'100%！ 飛んだらビームを長押し':'1.8秒で満タン');
  $('distanceLabel').textContent=n.released?'解放から':'発射から';$('distance').textContent=(n.released?n.releaseAge:n.elapsed).toFixed(1);$('distanceUnit').textContent='秒';
  $('rotorReserve').value=Math.round(n.released?0:n.energy*100);$('rotorPercent').textContent=n.released?'解放！':Math.round(n.energy*100)+'%';
  $('rotorStatus').querySelector('label').textContent='解放エネルギー';
  beam.disabled=n.beamUsed&&!n.beamActive;beam.textContent=n.beamActive?'離すとビーム終了':n.beamUsed?'ビーム終了':'押している間 ビーム';
  result.textContent=n.segmentName+'　破壊率 '+(n.destructionRate*100).toFixed(1)+'%（'+n.broken+'/'+n.total+'）　'+(n.cleared?'クリア！':'70%でクリア');
  compare.textContent=n.hasPrevious?'前回 '+n.previousBroken+'個 ／ 今回 '+(n.delta>=0?'+':'')+n.delta+'個':'この区間の最初の一投';
  $('destructionResultValue').textContent=String(n.broken);$('retry').textContent='もう一投';
  if(s.physicsFault){result.textContent='物理エラー：'+s.physicsFault+'（記録対象外）';}
 }
 return {update,cancel};
})();
