'use strict';
// Original procedural effects; no external recordings or downloads.
(function(root){
 const durations={adjust:.045,charge:.24,launch:.40,land:.24,record:.42,kick:.10,perfect:.18,good:.14,miss:.12,spin:.10,shell:.22,claw:.14,legs:.12,glass:.22,dish:.18,wall:.34,wood:.21,metal:.36,food:.12};
 function sample(kind,t){const d=durations[kind],u=t/d;if(u<0||u>1)return 0;const envelope=Math.min(1,t/.008)*Math.pow(1-u,2);
  if(kind==='wood')return .24*envelope*(Math.sin(2*Math.PI*119*t)+.3*Math.sin(2*Math.PI*401*t));
  if(kind==='metal')return .16*envelope*(Math.sin(2*Math.PI*317*t)+.6*Math.sin(2*Math.PI*701*t)+.25*Math.sin(2*Math.PI*1201*t));
  if(kind==='food')return .18*envelope*Math.sin(2*Math.PI*(110*t-50*t*t));
  if(kind==='glass')return .13*envelope*(Math.sin(2*Math.PI*1763*t)+.7*Math.sin(2*Math.PI*2617*t)+.3*Math.sin(2*Math.PI*3671*t));
  if(kind==='dish')return .17*envelope*(Math.sin(2*Math.PI*893*t)+.6*Math.sin(2*Math.PI*1439*t));
  if(kind==='wall')return .25*envelope*(Math.sin(2*Math.PI*(63*t-35*t*t))+.25*Math.sin(2*Math.PI*317*t));
  if(kind==='spin')return .065*envelope*(Math.sin(2*Math.PI*327*t)+.5*Math.sin(2*Math.PI*753*t)+.3*Math.sin(2*Math.PI*1163*t));
  if(kind==='kick')return .12*envelope*Math.sin(2*Math.PI*(220*t+1800*t*t));
  if(kind==='perfect')return .14*envelope*(Math.sin(2*Math.PI*740*t)+.4*Math.sin(2*Math.PI*1110*t));
  if(kind==='good')return .14*envelope*Math.sin(2*Math.PI*(340*t+700*t*t));
  if(kind==='miss')return .08*envelope*Math.sin(2*Math.PI*(170*t-200*t*t));
  if(kind==='charge')return .12*Math.sin(2*Math.PI*(170*t+600*t*t))*Math.sin(Math.PI*u);
  if(kind==='launch')return .19*envelope*(Math.sin(2*Math.PI*(650*t-700*t*t))+.3*Math.sin(2*Math.PI*1213*t));
  if(kind==='shell')return .24*envelope*(Math.sin(2*Math.PI*(75*t-75*t*t))+.12*Math.sin(2*Math.PI*603*t));
  if(kind==='claw')return .18*envelope*(Math.sin(2*Math.PI*235*t)+.3*Math.sin(2*Math.PI*1453*t));
  if(kind==='legs')return .14*envelope*(Math.sin(2*Math.PI*145*t)+.25*Math.sin(2*Math.PI*887*t));
  if(kind==='land')return .24*envelope*(Math.sin(2*Math.PI*(100*t-90*t*t))+.2*Math.sin(2*Math.PI*843*t));
  if(kind==='record')return .10*envelope*(Math.sin(2*Math.PI*660*t)+Math.sin(2*Math.PI*880*t));
  return .07*envelope*Math.sin(2*Math.PI*580*t);
 }
 let context=null,master=null,output=null,enabled=false,paused=false,lastAdjust=0,volume=.65,spinIntensity=0,lastSpin=-1;const active=new Set();
 function setVolume(value){if(!Number.isFinite(value))return;volume=Math.min(1,Math.max(0,value));if(context&&master)master.gain.setValueAtTime(enabled&&!paused?volume:0,context.currentTime);}
 async function toggle(){try{if(!context){const Context=root.AudioContext||root.webkitAudioContext;if(!Context)return false;context=new Context();master=context.createGain();output=context.createDynamicsCompressor();output.threshold.value=-12;output.knee.value=12;output.ratio.value=4;output.attack.value=.003;output.release.value=.12;master.connect(output);output.connect(context.destination);}if(enabled){enabled=false;master.gain.setValueAtTime(0,context.currentTime);}else{await context.resume();enabled=context.state==='running';master.gain.setValueAtTime(enabled&&!paused?volume:0,context.currentTime);if(enabled)play('adjust');}return enabled;}catch{return false;}}
 function play(kind,intensity=1){if(!durations[kind]||paused||!enabled||!context||context.state!=='running')return;const now=context.currentTime;if(kind==='adjust'&&now-lastAdjust<.05)return;if(kind==='adjust')lastAdjust=now;const buffer=context.createBuffer(1,Math.ceil(context.sampleRate*durations[kind]),context.sampleRate),data=buffer.getChannelData(0);for(let i=0;i<data.length;i++)data[i]=sample(kind,i/context.sampleRate)*Math.min(1,Math.max(0,Number.isFinite(intensity)?intensity:1));const source=context.createBufferSource();source.buffer=buffer;source.connect(master);source.kind=kind;if(kind==='spin'&&source.playbackRate)source.playbackRate.value=.5+spinIntensity*1.8;active.add(source);source.onended=()=>{active.delete(source);source.disconnect();};source.start();}
 function setSpin(value){spinIntensity=Number.isFinite(value)?Math.min(1,Math.max(0,value)):0;if(spinIntensity<.03){for(const source of active)if(source.kind==='spin'){try{source.stop();}catch{}}return;}if(!paused&&enabled&&context&&context.state==='running'&&context.currentTime-lastSpin>.075){lastSpin=context.currentTime;play('spin',.25+.75*spinIntensity);}}
 function setPaused(value){paused=!!value;if(paused){for(const source of active){try{source.stop();}catch{}}active.clear();}if(context&&master)master.gain.setValueAtTime(enabled&&!paused?volume:0,context.currentTime);}
 function captureOutput(){
  if(!context||context.state!=='running'||!enabled)throw new Error('音をオンにしてから録画してください');
  if(volume<=0)throw new Error('音量が0です。録画する音量を設定してください');
  const destination=context.createMediaStreamDestination();output.connect(destination);const captureClock=context.createConstantSource();captureClock.offset.value=0;captureClock.connect(destination);captureClock.start();let released=false;
  return {stream:destination.stream,clock:()=>context.currentTime,state:()=>({enabled,volume}),decode:bytes=>context.decodeAudioData(bytes),release(){if(released)return;released=true;captureClock.stop();captureClock.disconnect();output.disconnect(destination);for(const track of destination.stream.getTracks())track.stop();}};
 }
 const api={graph:()=>({context,master,output,enabled,paused,volume}),activeVoiceCount:()=>active.size,sample,durations,toggle,play,setVolume,setPaused,setSpin,captureOutput};if(typeof module!=='undefined')module.exports=api;else root.FlightSound=api;
})(typeof window!=='undefined'?window:{});
