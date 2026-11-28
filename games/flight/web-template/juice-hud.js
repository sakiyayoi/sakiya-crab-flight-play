'use strict';
// Only the displayed HUD value is interpolated. Authoritative state/results stay untouched.
(function(root){
 const element=document.getElementById('destructionValue');
 element.style.display='inline-block';element.style.alignSelf='flex-start';element.style.transformOrigin='left center';
 const media=matchMedia('(prefers-reduced-motion: reduce)');
 const CATCHUP_MS=240,POP_MS=150,PEAK_SCALE=1.16;
 const CAPTION_MS=1000,DEATH_CAPTION_MS=1200,CAPTIONS=Object.freeze({
  boost:'扇風機、強すぎる！',bounce:'フライパンで跳ね上げられた！',
  spin:'回転寿司のレーンに乗った！',brake:'もちにくっついた…',
  blast:'花火、点火済みだった！',death:'かに漁師だ！'
 });
 const caption=document.createElement('strong');caption.id='objectStoryCaption';caption.hidden=true;
 // Decorative, short-lived commentary: do not queue six announcements in a screen reader.
 caption.setAttribute('aria-hidden','true');document.getElementById('stage').append(caption);
 const badgeLayer=document.createElement('div');badgeLayer.id='objectFunctionBadges';badgeLayer.setAttribute('aria-hidden','true');document.getElementById('stage').append(badgeLayer);
 const BADGES=Object.freeze({boost:'→',bounce:'↑',spin:'↻',brake:'↓',blast:'✹',death:'✕'});
 const badges=[];
 function updateBadges(s){
  const rows=s.objectMode==='redesign'&&['ready','flight'].includes(s.phase)?s.objectBadges||[]:[];
  // Fixed upper bound: 16 objects on each mirrored side, reuse DOM nodes.
  for(let i=0;i<Math.min(32,rows.length);i++){
   let badge=badges[i];if(!badge){badge=document.createElement('span');badge.className='objectFunctionBadge';badges.push(badge);badgeLayer.append(badge);}
   const b=rows[i],glyph=Object.hasOwn(BADGES,b.kind)?BADGES[b.kind]:null;
   badge.hidden=!b.visible||!glyph||!Number.isFinite(b.x)||!Number.isFinite(b.y);
   if(badge.hidden)continue;
   badge.textContent=b.kind==='boost'&&b.side<0?'←':glyph;badge.dataset.kind=b.kind;
   badge.style.left=(b.x*100)+'%';badge.style.top=((1-b.y)*100)+'%';
  }
  for(let i=rows.length;i<badges.length;i++)badges[i].hidden=true;
 }
 let captionGeneration=null,captionSequence=0,captionTimer=0;
 let generation=null,shown=0,target=0,from=0,start=0,duration=CATCHUP_MS,remaining=CATCHUP_MS;
 let frameId=0,pop=null,paused=false,state=null;
 const motion=()=>!media.matches&&root.FlightFeel?.settings().preset!=='off'&&document.body.dataset.reduced!=='true';
 const captionEnabled=()=>motion()&&!root.FlightFeel?.settings().reducedMotion&&!state?.reducedMotion&&!state?.feel?.reduced&&state?.feel?.preset!=='off'&&!state?.paused&&!document.hidden;
 function clearCaption(){if(captionTimer)clearTimeout(captionTimer);captionTimer=0;caption.hidden=true;caption.textContent='';delete caption.dataset.death;}
 function placeCaption(){
  if(caption.hidden)return;
  const result=state?.phase==='result',parent=result?document.querySelector('#result .panel'):document.getElementById('stage');
  if(!parent){clearCaption();return;}
  if(caption.parentElement!==parent)parent.prepend(caption);
  caption.dataset.result=String(result);
  if(!result)caption.style.top=(document.getElementById('flightHUD').getBoundingClientRect().bottom+12)+'px';
 }
 function updateCaption(s){
  if(captionGeneration!==s.generation){clearCaption();captionGeneration=s.generation;captionSequence=0;}
  const latest=s.objectCaption;
  const fresh=latest&&Number.isInteger(latest.sequence)&&latest.sequence>captionSequence;
  // Consume even while suppressed. Restoring effects must not replay an old hit.
  if(fresh)captionSequence=latest.sequence;
  const death=latest?.kind==='death';
  if(s.objectMode!=='redesign'||!['flight','result'].includes(s.phase)||s.paused||document.hidden){clearCaption();return;}
  if(death&&s.objectCapture){
   // The engine's presentation clock owns hold -> capture -> caption -> result.
   // Information survives effects-off and both reduced-motion paths.
   if(s.objectCapture.caption){
    if(captionTimer)clearTimeout(captionTimer);captionTimer=0;
    caption.textContent=CAPTIONS.death;caption.dataset.death='true';caption.hidden=false;
   }else clearCaption();
   placeCaption();return;
  }
  if(!death&&!captionEnabled()){clearCaption();return;}
  if(fresh){
   clearCaption();
   const text=Object.hasOwn(CAPTIONS,latest.kind)?CAPTIONS[latest.kind]:null;
   if(text&&(latest.visible||death)){caption.textContent=text;caption.dataset.death=String(death);caption.hidden=false;captionTimer=setTimeout(clearCaption,death?DEATH_CAPTION_MS:CAPTION_MS);}
  }
  placeCaption();
 }
 function cancelFrame(){if(frameId)cancelAnimationFrame(frameId);frameId=0;}
 function cancelPop(){if(pop)pop.cancel();}
 function write(value,animate){
  if(value===shown&&element.textContent===String(value))return;
  const increased=value>shown;shown=value;element.textContent=String(value);
  if(increased&&animate&&motion()){
   // One reusable Animation, even if the number changes on every catch-up frame.
   if(!pop)pop=element.animate([{transform:'scale(1)'},{transform:'scale('+PEAK_SCALE+')',offset:.35},{transform:'scale(1)'}],{duration:POP_MS,easing:'ease-out'});
   else {pop.currentTime=0;pop.play();}
  }
 }
 function flush(){cancelFrame();cancelPop();write(target,false);from=shown;remaining=CATCHUP_MS;}
 function frame(now){
  frameId=0;if(paused)return;
  if(!motion()||state?.phase!=='flight'){flush();return;}
  const t=Math.min(1,Math.max(0,(now-start)/duration));
  write(t===1?target:Math.floor(from+(target-from)*t),true);
  if(shown<target)frameId=requestAnimationFrame(frame);
 }
 function schedule(ms=CATCHUP_MS){from=shown;start=performance.now();duration=Math.max(1,ms);cancelFrame();frameId=requestAnimationFrame(frame);}
 function update(s){
  state=s;updateCaption(s);updateBadges(s);const value=Number.isFinite(s.destructionEvent)?Math.max(0,Math.floor(s.destructionEvent)):0;
  const reset=generation!==s.generation||value<target;generation=s.generation;
  const changed=value!==target,wasPaused=paused;target=value;paused=!!s.paused;
  if(reset||s.phase!=='flight'||!motion()){flush();return;}
  if(paused){
   if(!wasPaused){remaining=frameId?Math.max(1,duration-(performance.now()-start)):CATCHUP_MS;cancelFrame();pop?.pause();}
   return;
  }
  if(wasPaused){pop?.play();if(shown<target)schedule(remaining);}
  else if(changed&&shown<target)schedule();
 }
 function refreshSettings(){if(!motion())flush();if(!captionEnabled()&&caption.dataset.death!=='true')clearCaption();}
 media.addEventListener('change',refreshSettings);
 document.addEventListener('visibilitychange',()=>{if(document.hidden)clearCaption();});
 root.addEventListener('resize',placeCaption);
 root.JuiceHud={update,refreshSettings,captionBottom:()=>caption.hidden||state?.phase!=='flight'?0:caption.getBoundingClientRect().bottom};
})(window);
