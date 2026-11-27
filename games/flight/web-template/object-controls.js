'use strict';
// State comes from Unity; no browser-side flight/score simulation.
window.ObjectControls=(()=>{
 let send=()=>{},state=null,view='',pointer=null,keys=new Set(),diving=false,jetLocked=false;
 const $=id=>document.getElementById(id);
 const active=()=>view==='flight'&&state?.phase==='flight'&&state?.objectMode==='redesign'&&!state.paused;
 function dive(){const next=active()&&(pointer!==null||keys.size>0);if(next!==diving){diving=next;send(next?'DiveDown':'DiveUp');} $('objectDive').setAttribute('aria-pressed',String(diving));}
 function release(){pointer=null;keys.clear();dive();}
 function paint(){
  const on=active();$('objectControls').classList.toggle('hidden',!on);
  if(state?.objectMode==='redesign')$('kick').classList.add('hidden');
  const remaining=Math.max(0,Math.min(2,Number(state?.jetsRemaining)||0));
  $('objectJet').disabled=!on||remaining===0||jetLocked;
  $('jetIcons').textContent=remaining===2?'● ●':remaining===1?'● ○':'○ ○';
  $('objectJet').setAttribute('aria-label','噴射、残り'+remaining+'回');
  $('maxChain').textContent=String(Math.max(0,Number(state?.maxChain)||0));
  if(!on)release();
 }
 function jet(){if(!active()||jetLocked||Number(state.jetsRemaining)<=0)return;jetLocked=true;send('Jet');paint();}
 function update(next,nextView){
  // Unity acknowledges remaining budget on every Jet command. A new report
  // releases the local debounce, but the authoritative two-shot cap is C#.
  state=next;view=nextView;jetLocked=false;paint();
 }
 function setView(next){view=next;paint();}
 function mount(sender){
  send=sender;$('objectJet').addEventListener('click',jet);
  $('objectDive').addEventListener('pointerdown',e=>{
   if(!active()||e.button!==0||pointer!==null)return;e.preventDefault();pointer=e.pointerId;
   try{e.currentTarget.setPointerCapture(e.pointerId);}catch{pointer=null;}dive();
  });
  const up=e=>{if(e.pointerId===pointer){pointer=null;dive();}};
  for(const type of ['pointerup','pointercancel','lostpointercapture'])$('objectDive').addEventListener(type,up);
  window.addEventListener('keydown',e=>{
   if(!active()||e.target.closest?.('input,textarea,select,[contenteditable="true"]'))return;
   const key=e.key.toLowerCase();
   if(['j','arrowup'].includes(key)){e.preventDefault();if(!e.repeat)jet();}
   if(['k','arrowdown'].includes(key)){e.preventDefault();keys.add(key);dive();}
   // Space/Enter on the focused dive button are an accessible hold action.
   if(e.target===$('objectDive')&&[' ','enter'].includes(key)){e.preventDefault();keys.add(key);dive();}
  });
  window.addEventListener('keyup',e=>{const key=e.key.toLowerCase();if(keys.delete(key)){e.preventDefault();dive();}});
  window.addEventListener('blur',release);
  document.addEventListener('visibilitychange',()=>{if(document.hidden)release();});
  paint();
 }
 return {mount,update,setView,release};
})();
