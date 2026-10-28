'use strict';
// The result strip consumes the complete solver-derived snapshot, never the
// rolling 32-event sound/VFX buffer. It does not estimate unseen destruction.
window.FlightResult=(()=>{
 const root=document.createElement('div');root.id='resultJourney';root.className='hidden';
 root.innerHTML='<div id="resultViewCaption"></div><div class="journey-heading"><b>この一投の跡</b><span id="journeyCount"></span></div><svg id="journeyStrip" viewBox="0 0 300 28" preserveAspectRatio="none" role="img"></svg><div class="journey-scale"><span id="journeyFrom"></span><span class="journey-key">黄:破壊 ●:停止 枠:画面</span><span id="journeyTo"></span></div><details id="journeyDetails"><summary id="journeySummary"></summary><ol id="journeySections"></ol></details>';
 document.querySelector('#result .row').before(root);
 const $=id=>document.getElementById(id),svg=$('journeyStrip'),ns='http://www.w3.org/2000/svg';let key='';
 const metre=x=>Number(x).toFixed(1)+' m';
 function node(tag,attributes,parent=svg){const n=document.createElementNS(ns,tag);for(const [k,v]of Object.entries(attributes))n.setAttribute(k,String(v));parent.append(n);return n;}
 function update(s){
  const o=s.phase==='result'?s.resultOverview:null;root.classList.toggle('hidden',!o);if(!o){key='';delete document.body.dataset.resultSide;return;}
  // The pullback can leave the actor on either side of the screen. Keep the card opposite its actual viewport position.
  document.body.dataset.resultSide=s.actorViewport.x>=.5?'left':'right';
  const from=o.from,to=o.to,span=Math.max(.01,to-from),x=p=>8+284*Math.max(0,Math.min(1,(p-from)/span));
  const currentKey=s.generation+':'+s.attempts;
  if(key!==currentKey){
   key=currentKey;$('journeyDetails').open=false;svg.replaceChildren();
   node('line',{x1:8,x2:292,y1:14,y2:14,stroke:'#fff0d666','stroke-width':2});
   for(const section of o.sections||[])node('rect',{x:x(section.from),y:12,width:Math.max(.5,x(section.to)-x(section.from)),height:4,fill:'#fff0d6'});
   // Each tick is an actual break event at its actual horizontal position.
   for(const point of o.breaks||[])node('line',{x1:x(point),x2:x(point),y1:4,y2:24,stroke:'#ffcb40','stroke-width':1.5});
   node('rect',{id:'journeyWindow',x:8,y:1,width:284,height:26,fill:'none',stroke:'#fff0d6','stroke-width':1,'stroke-dasharray':'3 2'});
   node('circle',{cx:x(o.stop),cy:14,r:4.5,fill:'#ff5c2b',stroke:'#fff0d6','stroke-width':1.5});
   $('journeyFrom').textContent=metre(from);$('journeyTo').textContent=metre(to);
   $('journeyCount').textContent='粉砕 '+o.broken;
   $('journeySummary').textContent=(s.direction<0?'← 左へ':'右へ →')+'　通過 '+(o.sections||[]).length+'区間 · 最遠 '+metre(o.reached);
   $('journeySections').replaceChildren();
   for(const section of o.sections||[]){const li=document.createElement('li');li.textContent=section.label+'　'+metre(section.from)+'–'+metre(section.to)+' · 粉砕 '+section.broken;$('journeySections').append(li);}
   if(!o.sections?.length){const li=document.createElement('li');li.textContent='通過区間なし';$('journeySections').append(li);}
   svg.setAttribute('aria-label','発射地点からの水平位置。'+metre(from)+'から'+metre(to)+'。停止位置 '+metre(o.stop)+'。粉砕 '+o.broken+'件。黄線が実際の破壊位置、赤丸がカニ、白枠がいまの画面。');
  }
  const windowStart=x(Math.max(from,o.viewFrom)),windowEnd=x(Math.min(to,o.viewTo));
  const frame=$('journeyWindow');frame.setAttribute('x',windowStart);frame.setAttribute('width',Math.max(0,windowEnd-windowStart));
  $('resultViewCaption').textContent=(o.stage==='pullback'?'停止地点から、ひと息引いて':'停止地点のまわり')+' · '+(o.broken?'最後の破壊 '+metre(Math.abs(o.stop-o.lastBreak))+(o.stop>=o.lastBreak?'手前':'先'):'この一投の破壊なし');
 }
 return {update};
})();
