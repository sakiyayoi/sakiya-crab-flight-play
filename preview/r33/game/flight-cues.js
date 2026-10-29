'use strict';
// Semantic cues are derived once from actual state/events; every presentation consumer shares them.
(function(root){
 const listeners=new Set();let generation=-1,cursor=0,occ=0,chain=0,chainAt=-Infinity,glass=false,result=false,gameEvent=-1;
 const now=()=>performance.now();
 function emit(event,data={}){const cue={...data,event,generation,occ:data.occ??data.sourceId??++occ,t:now()};for(const f of listeners)f(cue);return cue;}
 function reset(g){generation=g;cursor=0;chain=0;chainAt=-Infinity;glass=result=false;gameEvent=-1;update.wasReady=false;emit('reset');}
 function update(s){
  if(generation!==s.generation)reset(s.generation);
  if(s.paused)return;
  for(const e of s.events||[]){
   if(e.generation!==generation||e.id<=cursor)continue;cursor=e.id;
   const data={sourceId:e.id,position:e.position,normal:e.normal,velocity:e.velocity,material:e.material,objectId:e.objectId,secondary:e.secondary,physicsTime:e.physicsTime,points:e.points||0};
   if(e.type==='launch'){chain=0;chainAt=-Infinity;emit('launch',data);}
   else if(e.type==='charge')emit('charge_stroke',data);
   else if(e.type==='destruction'){
    const t=e.physicsTime;if(t-chainAt>.8)chain=0;chainAt=t;chain++;
    if(!glass&&e.material==='glass'&&e.objectId.startsWith('tank-glass-')){glass=true;emit('glass_break',data);}
    else emit('break',data);
    if([2,4,7].includes(chain))emit('chain',{...data,chain});
   }else if(e.type==='landing')emit('ground_hit',{...data,part:e.objectId==='shell'?'shell':e.objectId.startsWith('claw')?'claw':'leg'});
  }
  if(s.chargeReady&&!update.wasReady)emit('charge_ready');update.wasReady=!!s.chargeReady;
  if(s.eventId!==gameEvent){gameEvent=s.eventId;const ev={perfect:'push_perfect',good:'push_good',miss:'push_miss'}[s.eventName];if(ev)emit(ev,{sourceId:s.eventId});}
  if(s.phase==='result'&&!result){result=true;if(s.newRecord&&!s.physicsFault)emit('new_record');emit('result',{distance:s.distance,invalid:!!s.physicsFault});}
 }
 root.FlightCues={update,subscribe(f){listeners.add(f);return()=>listeners.delete(f);},fire:emit};
})(window);
