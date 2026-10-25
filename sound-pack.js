'use strict';
// Original local WAV pack. Shares the existing user-gesture audio graph and output capture.
(function(root){
 const api=root.FlightSound,original={play:api.play,toggle:api.toggle,setPaused:api.setPaused,setSpin:api.setSpin};
 const deferred=new Set();const buffers=new Map(),assets=new Map(),groups=new Map(),voices=new Set(),loops=new Map(),lastVariant=new Map(),cooldown=new Map(),pendingWalls=new Map(),playedWalls=new Set();
 let loading=null,ready=false,latest=null,generation=-1,cursor=0,wasReady=false,chain=0,chainTime=-10,chainMarks=new Set(),lastLand=-10,duckUntil=0;
 const stats={ready:false,decoded:0,errors:0,processed:0,duplicates:0,gaps:0,peakVoices:0,peakMaterialVoices:0,voiceDrops:0,rawPeak:0,events:[]};let meter;
 const graph=()=>api.graph(),now=()=>graph().context?.currentTime||0;
 function stopVoice(v,fade=.03){if(v.stopping)return;v.stopping=true;try{v.gain.gain.cancelScheduledValues(now());v.gain.gain.setTargetAtTime(0,now(),Math.max(.002,fade/3));v.source.stop(now()+fade);}catch{}for(const[k,x]of loops)if(x===v)loops.delete(k);}
 function stopAll(){for(const timer of deferred)clearTimeout(timer);deferred.clear();for(const v of voices)stopVoice(v);for(const p of pendingWalls.values())clearTimeout(p.timer);pendingWalls.clear();loops.clear();chain=0;chainMarks.clear();}
 function group(event){const list=groups.get(event)||[];if(!list.length)return null;let i=((lastVariant.get(event)??-1)+1)%list.length;lastVariant.set(event,i);return list[i];}
 function makeVoice(asset,intensity=1,loop=false,pan=0,retried=false){
  const g=graph(),ctx=g.context;if(!ready||!g.enabled||g.paused||!ctx||ctx.state!=='running'||!asset)return null;
  const material=asset.event.startsWith('break.'),active=[...voices].filter(v=>!v.stopping),same=active.filter(v=>v.asset.event===asset.event);
  if(!loop&&(now()-(cooldown.get(asset.event)??-100)<asset.cooldown_seconds||same.length>=asset.max_voices_per_event)){stats.voiceDrops++;return null;}
  if(material&&active.filter(v=>v.material).length>=4){stats.voiceDrops++;return null;}
  if(voices.size+api.activeVoiceCount()>=12){const victim=active.filter(v=>!v.loop&&v.asset.priority>asset.priority).sort((a,b)=>b.asset.priority-a.asset.priority||a.started-b.started)[0];if(loop||retried||deferred.size>=12||(!victim&&!Array.from(voices).some(v=>v.stopping))){stats.voiceDrops++;return null;}if(victim)stopVoice(victim,.01);const gen=generation,timer=setTimeout(()=>{deferred.delete(timer);if(gen===generation)makeVoice(asset,intensity,false,pan,true);},35);deferred.add(timer);return null;}
  const source=ctx.createBufferSource(),gain=ctx.createGain();source.buffer=buffers.get(asset.id);source.loop=loop;source.playbackRate.value=(asset.pitch_min+asset.pitch_max)*.5;gain.gain.value=loop?0:asset.gain_linear*Math.min(1,Math.max(0,intensity));source.connect(gain);
  let panner=null;if(ctx.createStereoPanner){panner=ctx.createStereoPanner();panner.pan.value=Math.max(-.24,Math.min(.24,pan));gain.connect(panner);panner.connect(g.master);}else gain.connect(g.master);
  const v={asset,source,gain,panner,material,loop,started:now(),stopping:false};voices.add(v);source.onended=()=>{voices.delete(v);try{source.disconnect();gain.disconnect();panner?.disconnect();}catch{}};source.start();if(!loop)cooldown.set(asset.event,now());
  stats.peakVoices=Math.max(stats.peakVoices,active.length+1+api.activeVoiceCount());stats.peakMaterialVoices=Math.max(stats.peakMaterialVoices,active.filter(x=>x.material).length+(material?1:0));return v;
 }
 function shot(event,strength=1,pan=0,weight=.5){const a=group(event),v=makeVoice(a,strength,false,pan);if(v&&!v.loop)v.source.playbackRate.setValueAtTime(a.pitch_max+(a.pitch_min-a.pitch_max)*Math.min(1,Math.max(0,weight)),now());return v;}
 function loop(id,weight){let v=loops.get(id);if(weight<.003){if(v)stopVoice(v,.08);return;}if(!v){v=makeVoice(assets.get(id),0,true);if(!v)return;loops.set(id,v);}v.gain.gain.setTargetAtTime(assets.get(id).gain_linear*weight,now(),.0267);}
 async function load(){if(loading)return loading;loading=(async()=>{const ctx=graph().context;if(!ctx)return;const response=await fetch('SoundPack/event-assets.json');if(!response.ok)throw Error('sound metadata load');const m=await response.json();for(const a of m.assets){assets.set(a.id,a);if(!groups.has(a.event))groups.set(a.event,[]);groups.get(a.event).push(a);}await Promise.all(m.assets.map(async a=>{const r=await fetch('SoundPack/'+a.path);if(!r.ok)throw Error('sound asset load');buffers.set(a.id,await ctx.decodeAudioData(await r.arrayBuffer()));stats.decoded++;}));ready=stats.ready=true;original.setSpin(0);meter=ctx.createAnalyser();meter.fftSize=1024;graph().master.connect(meter);})().catch(()=>{stats.errors++;ready=false;});return loading;}
 function materialEvent(e){const kind=e.material==='tableware'?'dish':e.material;const pan=latest?Math.max(-.24,Math.min(.24,(e.position.x-latest.worldX)/30)):0;if(!ready){original.play(kind,e.strength);return;}shot('break.'+kind,Math.max(.3,e.strength),pan,e.weight??.5);if((e.weight||0)>.78&&now()-(cooldown.get('heavy.body')??-10)>.24){cooldown.set('heavy.body',now());api.play('shell',.30);}}
 function consume(e){stats.processed++;stats.events.push({generation:e.generation,id:e.id,type:e.type,objectId:e.objectId,material:e.material,weight:e.weight??.5,physicsTime:e.physicsTime,atAudio:now()});if(stats.events.length>256)stats.events.shift();
  if(e.type==='reset'){stopAll();playedWalls.clear();wasReady=false;return;}
  if(e.type==='charge'){if(ready)shot('charge.work',.7+e.strength*.3);return;}
  if(e.type==='launch'){stopAll();duckUntil=now()+.1;if(ready)shot('flight.launch');else original.play('launch');return;}
  if(e.type==='destruction'){
   if(now()-chainTime>.8){chain=0;chainMarks.clear();}chain++;chainTime=now();if([2,4,7].includes(chain)&&!chainMarks.has(chain)){chainMarks.add(chain);if(ready)shot('chain.threshold.'+chain);}
   if(e.material==='wall')playedWalls.add(e.objectId);materialEvent(e);return;
  }
  if(e.type==='exit'){const p=pendingWalls.get(e.objectId);if(p){clearTimeout(p.timer);pendingWalls.delete(e.objectId);}if(ready)shot('landmark.exterior',.8);else original.play('wall',.65);playedWalls.add(e.objectId);duckUntil=now()+.1;return;}
  if(e.type==='landing'&&now()-lastLand>=.09){lastLand=now();duckUntil=now()+.1;const part=e.objectId==='shell'?'shell':e.objectId.startsWith('claw')?'claw':'legs';if(ready)shot('contact.'+part,Math.max(.2,e.strength));else original.play(part,Math.max(.2,e.strength));}
 }
 function update(s){latest=s;const g=graph(),events=Array.isArray(s.events)?s.events:[];
  if(generation!==s.generation){stopAll();generation=s.generation;cursor=0;playedWalls.clear();wasReady=false;lastLand=-10;}
  const max=events.length?events.at(-1).id:cursor;if(!g.enabled||g.paused||s.paused){stopAll();cursor=max;wasReady=!!s.chargeReady;return;}
  if(events.length&&cursor&&events[0].id>cursor+1)stats.gaps++;
  for(const e of events){if(e.id<=cursor)continue;if(e.generation===generation)consume(e);cursor=e.id;}
  if(s.phase==='result')stopAll();
  if(!ready)return;
  if(s.phase==='ready'&&s.chargeStarted){const E=Math.min(1,Math.max(0,s.chargeEnergy));const w=[Math.max(0,1-2*E),Math.max(0,1-2*Math.abs(E-.5)),Math.max(0,2*E-1)],norm=Math.hypot(...w)||1;for(let i=0;i<3;i++)loop(['charge_low_loop','charge_mid_loop','charge_high_loop'][i],w[i]/norm*(.28+.42*E));if(s.chargeReady&&!wasReady)shot('charge.ready');wasReady=!!s.chargeReady;}else for(const id of ['charge_low_loop','charge_mid_loop','charge_high_loop'])loop(id,0);
  if(s.phase==='flight'){let t=Math.min(1,Math.max(0,(s.speed-8)/27));t=t*t*(3-2*t);const gain=s.speed<8?0:(.15+.55*t)*(now()<duckUntil?.4:1);loop('wind_slow_loop',Math.cos(t*Math.PI*.5)*gain);loop('wind_fast_loop',Math.sin(t*Math.PI*.5)*gain);}else{loop('wind_slow_loop',0);loop('wind_fast_loop',0);}
  if(meter){const data=new Float32Array(meter.fftSize);meter.getFloatTimeDomainData(data);for(const x of data)stats.rawPeak=Math.max(stats.rawPeak,Math.abs(x));}
 }
 api.update=update;api.packReady=()=>ready;api.debug=()=>({...stats,activeVoices:voices.size,loops:loops.size,pendingWalls:pendingWalls.size,pendingVoices:deferred.size,generation,cursor});
 api.toggle=async()=>{const result=await original.toggle();if(result)load();else stopAll();return result;};
 api.setPaused=value=>{if(value)stopAll();original.setPaused(value);if(latest)update(latest);};
 api.setSpin=value=>{if(!ready)original.setSpin(value);};
 api.play=(kind,intensity=1)=>{if(voices.size+api.activeVoiceCount()>=12)return;original.play(kind,intensity);};
})(window);
