'use strict';
// Only the displayed HUD value is interpolated. Authoritative state/results stay untouched.
(function(root){
 const element=document.getElementById('destructionValue');
 element.style.display='inline-block';element.style.alignSelf='flex-start';element.style.transformOrigin='left center';
 const media=matchMedia('(prefers-reduced-motion: reduce)');
 const CATCHUP_MS=240,POP_MS=150,PEAK_SCALE=1.16;
 let generation=null,shown=0,target=0,from=0,start=0,duration=CATCHUP_MS,remaining=CATCHUP_MS;
 let frameId=0,pop=null,paused=false,state=null;
 const motion=()=>!media.matches&&root.FlightFeel?.settings().preset!=='off'&&document.body.dataset.reduced!=='true';
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
  state=s;const value=Number.isFinite(s.destructionEvent)?Math.max(0,Math.floor(s.destructionEvent)):0;
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
 function refreshSettings(){if(!motion())flush();}
 media.addEventListener('change',refreshSettings);
 root.JuiceHud={update,refreshSettings};
})(window);
