'use strict';
// Original procedural effects; no external recordings or downloads.
(function(root){
 const durations={adjust:.045,charge:.17,launch:.28,land:.24,record:.42,shot:.24,creature:.25,miss:.14,hurt:.22,guard:.12};
 function sample(kind,t){const d=durations[kind],u=t/d;if(u<0||u>1)return 0;const envelope=Math.min(1,t/.008)*Math.pow(1-u,2);
  if(kind==='guard')return .10*envelope*(Math.sin(2*Math.PI*1120*t)+.3*Math.sin(2*Math.PI*1730*t));
  if(kind==='hurt')return .20*envelope*(Math.sin(2*Math.PI*(65*t-40*t*t))+.18*Math.sin(2*Math.PI*510*t));
  if(kind==='shot')return .22*envelope*(Math.sin(2*Math.PI*(160*t-170*t*t))+.35*Math.sin(2*Math.PI*1237*t));
  if(kind==='creature')return .17*envelope*Math.sin(2*Math.PI*(220*t+70*t*t)+Math.sin(t*70));
  if(kind==='miss')return .06*envelope*Math.sin(2*Math.PI*(500*t-600*t*t));
  if(kind==='charge')return .12*Math.sin(2*Math.PI*(170*t+600*t*t))*Math.sin(Math.PI*u);
  if(kind==='launch')return .19*envelope*(Math.sin(2*Math.PI*(650*t-700*t*t))+.3*Math.sin(2*Math.PI*1213*t));
  if(kind==='land')return .24*envelope*(Math.sin(2*Math.PI*(100*t-90*t*t))+.2*Math.sin(2*Math.PI*843*t));
  if(kind==='record')return .10*envelope*(Math.sin(2*Math.PI*660*t)+Math.sin(2*Math.PI*880*t));
  return .07*envelope*Math.sin(2*Math.PI*580*t);
 }
 let context=null,master=null,enabled=false,suspended=false,lastAdjust=0,volume=.45;
 async function toggle(){try{if(!context){const Context=root.AudioContext||root.webkitAudioContext;if(!Context)return false;context=new Context();master=context.createGain();master.connect(context.destination);}if(enabled){enabled=false;master.gain.setValueAtTime(0,context.currentTime);}else{await context.resume();enabled=context.state==='running';master.gain.setValueAtTime(enabled&&!suspended?volume:0,context.currentTime);if(enabled)play('adjust');}return enabled;}catch{return false;}}
 function play(kind){if(!enabled||suspended||!context||context.state!=='running')return;const now=context.currentTime;if(kind==='adjust'&&now-lastAdjust<.05)return;if(kind==='adjust')lastAdjust=now;const buffer=context.createBuffer(1,Math.ceil(context.sampleRate*durations[kind]),context.sampleRate),data=buffer.getChannelData(0);for(let i=0;i<data.length;i++)data[i]=sample(kind,i/context.sampleRate);const source=context.createBufferSource();source.buffer=buffer;source.connect(master);source.onended=()=>source.disconnect();source.start();}
 function setVolume(value){const n=Number(value);volume=Number.isFinite(n)?Math.min(1,Math.max(0,n)):.45;if(master)master.gain.setValueAtTime(enabled&&!suspended?volume:0,context.currentTime);}
 function captureOutput(){
  if(!context||context.state!=='running'||!enabled)throw new Error('音をオンにしてから録画してください');
  if(volume<=0)throw new Error('音量が0です。録画する音量を設定してください');
  const destination=context.createMediaStreamDestination();master.connect(destination);let released=false;
  return {stream:destination.stream,clock:()=>context.currentTime,state:()=>({enabled,volume}),decode:bytes=>context.decodeAudioData(bytes),release(){if(released)return;released=true;master.disconnect(destination);for(const track of destination.stream.getTracks())track.stop();}};
 }
 function setSuspended(value){suspended=!!value;if(master)master.gain.setValueAtTime(enabled&&!suspended?volume:0,context.currentTime);}
 const api={setSuspended,sample,durations,toggle,play,setVolume,captureOutput};if(typeof module!=='undefined')module.exports=api;else root.FlightSound=api;
})(typeof window!=='undefined'?window:{});
