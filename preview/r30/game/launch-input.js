'use strict';
// Recent real-time velocity, normalized by the short viewport side. No accumulated meter.
(function(root){
 const WINDOW_MS=100,MAX_SPEED=5,MAX_SPIN=12,MIN_SPEED=.7,FRESH_MS=80;
 const clamp=(v,a,b)=>Math.min(b,Math.max(a,v));
 function velocityAim(vx,vy,scale=390){
  const speed=Math.hypot(vx,vy),direction=vx<0?-1:1,rawAngle=Math.atan2(-vy,Math.abs(vx))*180/Math.PI;
  return {angle:Math.round(clamp(rawAngle,10,80)),power:Math.round(clamp(speed/MAX_SPEED,0,1)*100),direction,spinRps:direction*clamp(speed/MAX_SPEED,0,1)*MAX_SPIN,speed,normalizedVx:vx,normalizedVy:vy,upward:vy<-.15&&rawAngle>=10,scale};
 }
 class PointerSwipe {
  constructor(){this.pressed=new Set();this.active=null;this.blocked=false;}
  down(id,x,y,eligible,scale,time){
   if(this.pressed.has(id))return {type:'ignore'};this.pressed.add(id);
   if(this.pressed.size>1){const had=!!this.active;this.active=null;this.blocked=true;return {type:had?'cancel':'ignore',reason:'multitouch'};}
   if(this.blocked||!eligible||![x,y,time,scale].every(Number.isFinite)||scale<=0)return {type:'ignore'};
   this.active={id,x,y,time,scale,path:0,spinPhase:0,lastMotion:time,samples:[{x,y,time}]};return {type:'begin',id,x,y,sampleTime:time};
  }
  append(x,y,time){
   const a=this.active,last=a.samples.at(-1);if(![x,y,time].every(Number.isFinite)||time<last.time)return false;
   if(time===last.time)return true; // Coalesced/rounded timestamps never divide by zero or add energy.
   const moved=Math.hypot(x-last.x,y-last.y);a.path+=moved;const angularStep=Math.min(MAX_SPIN*360*(time-last.time)/1000,moved/a.scale*(MAX_SPIN/MAX_SPEED)*360);a.spinPhase=(a.spinPhase+(x<last.x?-1:1)*angularStep)%360;if(moved>=a.scale*.002)a.lastMotion=time;
   a.samples.push({x,y,time});while(a.samples.length>2&&a.samples[1].time<time-WINDOW_MS)a.samples.shift();return true;
  }
  aim(time){
   const a=this.active,last=a.samples.at(-1),start=Math.max(a.time,time-WINDOW_MS);let before=a.samples[0],after=last;
   for(let i=1;i<a.samples.length;i++){if(a.samples[i].time>=start){before=a.samples[i-1];after=a.samples[i];break;}}
   const t=after.time===before.time?0:clamp((start-before.time)/(after.time-before.time),0,1),x=before.x+(after.x-before.x)*t,y=before.y+(after.y-before.y)*t,dt=(last.time-start)/1000;
   const vx=dt>=.016?(last.x-x)/dt/a.scale:0,vy=dt>=.016?(last.y-y)/dt/a.scale:0,r=velocityAim(vx,vy,a.scale),displacement=Math.hypot(last.x-x,last.y-y);
   r.valid=dt>=.016&&a.path>=a.scale*.04&&displacement>=a.scale*.02&&time-a.lastMotion<=FRESH_MS&&r.upward&&r.speed>=MIN_SPEED;
   r.reason=time-a.lastMotion>FRESH_MS?'stale':!r.upward?'upward-only':r.speed<MIN_SPEED?'slow':dt<.016||a.path<a.scale*.04||displacement<a.scale*.02?'micro':'fresh';
   return {...r,spinPhase:a.spinPhase,x:last.x,y:last.y,startX:a.x,startY:a.y,path:a.path,sampleTime:time,windowMs:dt*1000};
  }
  move(id,x,y,buttons,time){
   if(!this.active||id!==this.active.id)return {type:'ignore'};
   if(buttons===0||!this.append(x,y,time)){this.active=null;this.blocked=true;return {type:'cancel',reason:'lost-button-or-time'};}
   return {type:'preview',...this.aim(time)};
  }
  up(id,x,y,insideViewport=true,time){
   this.pressed.delete(id);let result={type:'ignore'};
   if(this.active&&id===this.active.id){if(!this.blocked&&insideViewport&&this.append(x,y,time)){const aim=this.aim(time);result={type:aim.valid?'launch':'cancel',...aim};}else result={type:'cancel',reason:'outside-or-time'};this.active=null;}
   if(this.pressed.size===0)this.blocked=false;return result;
  }
  cancel(reason='cancel',clear=false){const had=!!this.active;this.active=null;if(clear){this.pressed.clear();this.blocked=false;}else this.blocked=this.pressed.size>0;return {type:had?'cancel':'ignore',reason};}
 }
 const api={PointerSwipe,velocityAim,WINDOW_MS,MAX_SPEED,MAX_SPIN,MIN_SPEED,FRESH_MS};if(typeof module!=='undefined')module.exports=api;else root.FlightLaunch=api;
})(typeof window!=='undefined'?window:{});
