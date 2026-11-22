'use strict';
// Presentation consumes actual Unity state and the existing deduplicated cue stream.
// It never increments the score or infers a destruction from a role preview.
(function(root){
 const stage=document.getElementById('stage'),media=matchMedia('(prefers-reduced-motion: reduce)');
 const labels={boost30:'↗ 低く加速',boost45:'↗ 斜めに加速',boost60:'↑ 高く加速',jackpot:'✦ 大連鎖',hazard:'! 減速ドラム',redirect30:'↗ 低空へ',resource:'↻ 回転補給',landmark:'クレーン',preview:'次の街へ'};
 const preview=document.createElement('aside');preview.id='portPreview';preview.hidden=true;preview.setAttribute('aria-label','次の目印');
 const distance=document.createElement('div'),roles=document.createElement('div'),special=document.createElement('div');roles.className='port-roles';special.className='port-special';preview.append(distance,roles,special);stage.append(preview);
 const particles=document.createElement('div');particles.id='portNumbers';particles.setAttribute('aria-hidden','true');stage.append(particles);
 const subtitle=document.createElement('p');subtitle.id='portNextLandmark';document.getElementById('retry').before(subtitle);
 let state=null,generation=-1,active=false;
 function reduced(){return media.matches||document.body.dataset.reduced==='true';}
 function enabled(){return root.FlightFeel?.settings().preset!=='off';}
 function update(s){
  state=s;const x=s.worldX??s.distance*(s.direction<0?-1:1);
  if(generation!==s.generation||s.phase!=='flight'){particles.replaceChildren();generation=s.generation;}
  const p=s.portPreview;active=!!p&&s.direction>=0&&x>=570&&x<900;
  document.body.dataset.port=String(active);
  preview.hidden=!active||s.phase!=='flight';
  if(p){
   const text=p.landmark?'次のランドマークまで あと'+Math.ceil(p.remaining)+'m':'港のランドマークを通過';
   distance.textContent=p.landmark?p.landmark+'　あと'+Math.ceil(p.remaining)+'m':'次の街へ';
   subtitle.textContent=text;subtitle.hidden=s.direction<0;
   roles.replaceChildren(...(p.nextRoles||[]).slice(0,3).map(role=>{const tag=document.createElement('span');tag.textContent=labels[role]||role;return tag;}));
   special.textContent=p.reversalReady?'✦ 地面に触れずに危険ドラムへ：強加速':'危険ドラム：加速後、地面に触れず当てると反転';
   special.dataset.ready=String(p.reversalReady);
  }else subtitle.hidden=true;
 }
 function handles(c){return (c.event==='break'||c.event==='chain')&&c.position?.x>=600&&c.position.x<900;}
 function project(p){
  const s=state;if(!s?.cameraPosition||!s.cameraSize||!s.cameraRight||!s.cameraUp)return null;
  const d={x:p.x-s.cameraPosition.x,y:p.y-s.cameraPosition.y,z:p.z-s.cameraPosition.z};
  const dot=v=>d.x*v.x+d.y*v.y+d.z*v.z,rect=document.getElementById('unity-canvas').getBoundingClientRect();
  return{x:rect.left+(.5+dot(s.cameraRight)/(2*s.cameraSize*s.cameraAspect))*rect.width,y:rect.top+(.5-dot(s.cameraUp)/(2*s.cameraSize))*rect.height};
 }
 function cue(c){
  if(c.event==='reset'||c.event==='result'){particles.replaceChildren();return;}
  if(!handles(c)||c.event!=='break'||state?.phase!=='flight'||!enabled())return;
  const p=project(c.position);if(!p||p.x<0||p.x>innerWidth||p.y<0||p.y>innerHeight)return;
  if(particles.childElementCount>=12)particles.firstElementChild.remove();
  const el=document.createElement('strong');el.className='port-number';el.textContent='+1';el.style.left=p.x+'px';el.style.top=p.y+'px';particles.append(el);
  const target=document.getElementById('destructionValue').getBoundingClientRect(),dx=target.x+target.width/2-p.x,dy=target.y+target.height/2-p.y;
  const frames=reduced()?[{opacity:1},{opacity:0}]:[
   {transform:'translate(-50%,-50%) scale(.65) rotate(-4deg)',opacity:0,offset:0},
   {transform:'translate(-50%,calc(-50% - 18px)) scale(1.16) rotate(-4deg)',opacity:1,offset:.15/.65},
   {transform:'translate(-50%,calc(-50% - 14px)) scale(1) rotate(-4deg)',opacity:1,offset:.38/.65},
   {transform:'translate(calc(-50% + '+dx+'px),calc(-50% + '+dy+'px)) scale(.45)',opacity:0,offset:1}];
  const animation=el.animate(frames,{duration:reduced()?250:650,easing:'ease-out',fill:'forwards'});animation.onfinish=()=>el.remove();
 }
 root.FlightCues.subscribe(cue);
 root.PortPresentation={update,handles};
})(window);
