'use strict';
// Provided VOICEVOX assets only. Uses the existing user-gesture unlocked graph and master gain.
(function(root){
 const sound=root.FlightSound,ids={glass_break:'glassbreak_kani_tsuntsun',chain:'chain7_matomete_tsuntsun',new_record:'newrecord_shinkiroku_tsuntsun',push_perfect:'perfect_nice_tsuntsun'};
 const buffers=new Map(),active=new Set(),events=[];let loadPromise=null,generation=-1,count=0,lastId='',lastAt=-Infinity,pending=null,stopEpoch=0;
 const stats={ready:false,decoded:0,errors:[],played:0,skipped:0};
 const graph=()=>sound.graph();
 function stop(){stopEpoch++;if(pending){clearTimeout(pending.timer);pending=null;}for(const source of active){try{source.stop();}catch{}}active.clear();}
 function log(e){events.push(e);if(events.length>5000)events.shift();}
 async function load(){if(loadPromise)return loadPromise;const ctx=graph().context;if(!ctx)return false;loadPromise=(async()=>{
  for(const id of Object.values(ids)){const r=await fetch('voice/'+id+'.wav');if(!r.ok)throw Error('voice '+id+' HTTP '+r.status);buffers.set(id,await ctx.decodeAudioData(await r.arrayBuffer()));stats.decoded++;}
  stats.ready=true;return true;
 })().catch(e=>{stats.errors.push(String(e));return false;});return loadPromise;}
 // Hash selects optional lines without consuming the simulation RNG or changing physics.
 function chance(cue){let h=2166136261;for(const c of cue.event+':'+cue.sourceId+':'+(cue.objectId||''))h=Math.imul(h^c.charCodeAt(0),16777619);return(h>>>0)/4294967296;}
 function play(cue){
  const id=ids[cue.event],g=graph(),priority=cue.event==='glass_break'||cue.event==='new_record';
  let reason='';if(!id)return false;
  if(!g.enabled||g.paused||!g.context||g.context.state!=='running')reason='audio-disabled';
  else if(!stats.ready)reason='not-decoded';
  else if(count>=3)reason='run-limit';else if(count>=2&&!priority)reason='record-reserve';else if(lastId===id)reason='repeat';
  else if(!priority&&performance.now()-lastAt<8000)reason='interval';
  else if(cue.event==='chain'&&(cue.chain<7||chance(cue)>=.5))reason='chance';
  else if(cue.event==='push_perfect'&&chance(cue)>=.3)reason='chance';
  if(reason){stats.skipped++;log({t:performance.now(),generation,occ:cue.occ,event:cue.event,id,played:false,reason});return false;}
  if(active.size){if(priority)stop();else return false;}
  const source=g.context.createBufferSource(),gain=g.context.createGain();source.buffer=buffers.get(id);source.playbackRate.value=1;gain.gain.value=1;source.connect(gain);gain.connect(g.master);
  source.onended=()=>{active.delete(source);try{source.disconnect();gain.disconnect();}catch{}};active.add(source);source.start();count++;lastAt=performance.now();lastId=id;stats.played++;
  const e={t:lastAt,generation,occ:cue.occ,event:cue.event,fx:'voice',id,playbackRate:1,contextTime:g.context.currentTime,duration:source.buffer.duration,played:true};log(e);if(root.__kaniTestPush)root.__kaniTestPush(e);return true;
 }
 function consume(cue){if(cue.event==='reset'){stop();generation=cue.generation;count=0;lastId='';lastAt=-Infinity;return;}
  if(!ids[cue.event])return;
  if(!stats.ready&&graph().enabled){const g=generation,epoch=stopEpoch;load().then(ok=>{if(ok&&epoch===stopEpoch&&g===generation&&performance.now()-cue.t<1500)play(cue);});}else play(cue);
 }
 const toggle=sound.toggle,paused=sound.setPaused;
 sound.toggle=async()=>{const on=await toggle();if(on)await load();else stop();return on;};
 sound.setPaused=value=>{if(value)stop();paused(value);};
 root.FlightCues.subscribe(consume);
 root.FlightVoice={debug:()=>({...stats,generation,count,active:active.size,events:events.map(x=>({...x}))}),stop};
})(window);
