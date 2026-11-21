'use strict';
// Original local QA helper. Captures only the supplied canvas and the supplied
// live audio graph. It does not request microphone, screen sharing or a network sink.
(function(root){
 function mount(options){
  const doc=root.document,container=options.container;
  if(!container||!options.canvas||typeof options.captureAudio!=='function')throw new Error('Recorder input missing');
  const panel=doc.createElement('section');panel.className='local-record-qa';panel.style.cssText='margin-top:18px;border-top:1px solid #91aaa0;padding:12px 0;font:13px/1.5 system-ui;color:inherit';
  const heading=doc.createElement('strong');heading.textContent='ローカル録画 QA';panel.appendChild(heading);
  const description=doc.createElement('p');description.textContent='この3D画面と、実際に出力する効果音を録画します。HTMLのHUDは状態JSONに残ります。';panel.appendChild(description);
  const start=doc.createElement('button');start.type='button';start.textContent='録画開始（音ON）';start.style.cssText='min-height:44px;margin:4px 0;width:100%';panel.appendChild(start);
  const status=doc.createElement('p');status.textContent='未録画';status.setAttribute('role','status');panel.appendChild(status);
  const result=doc.createElement('div');panel.appendChild(result);container.appendChild(panel);
  const stop=doc.createElement('button');stop.type='button';stop.textContent='録画を停止';stop.style.cssText='position:fixed;top:5px;left:5px;z-index:1000;min-height:44px;background:#fff3c7;color:#132e34;border:2px solid #a64e35';stop.hidden=true;doc.body.appendChild(stop);
  let active=false,recorder=null,videoStream=null,audioLease=null,chunks=[],events=[],timer=null,started=0,frameHint=30,urls=[],session=0;
  const snapshot=()=>options.readState?options.readState():null;
  const elapsed=()=>Math.round((root.performance.now()-started)*1000)/1000;
  const observe=state=>{if(active&&events.length<1800)events.push({wallMs:elapsed(),audioSeconds:audioLease?audioLease.clock():null,state:JSON.parse(JSON.stringify(state))});};
  const cleanup=()=>{if(timer!==null){root.clearTimeout(timer);timer=null;}if(videoStream)for(const t of videoStream.getTracks())t.stop();videoStream=null;if(audioLease)audioLease.release();audioLease=null;};
  const clearResults=()=>{for(const u of urls)root.URL.revokeObjectURL(u);urls=[];result.replaceChildren();};
  const link=(blob,filename,label)=>{const a=doc.createElement('a');a.href=root.URL.createObjectURL(blob);urls.push(a.href);a.download=filename;a.textContent=label;a.style.cssText='display:block;min-height:44px;line-height:44px;color:#ffe19c';result.appendChild(a);return a;};
  const stopRecording=()=>{if(!active||!recorder||recorder.state==='inactive')return;stop.disabled=true;status.textContent='録画を整理中…';recorder.stop();};
  start.onclick=async()=>{
   if(active)return;start.disabled=true;clearResults();events=[];chunks=[];session++;
   try{
    if(typeof options.beforeStart==='function')await options.beforeStart();
    if(!options.canvas.captureStream||!root.MediaRecorder||!root.MediaStream)throw new Error('このブラウザではcanvas録画を利用できません');
    audioLease=options.captureAudio();videoStream=options.canvas.captureStream(frameHint);
    const videoTracks=videoStream.getVideoTracks(),audioTracks=audioLease.stream.getAudioTracks();
    if(videoTracks.length!==1||audioTracks.length!==1)throw new Error('映像1本・実出力音声1本を取得できません');
    const combined=new root.MediaStream([...videoTracks,...audioTracks]);
    const choices=['video/webm;codecs=vp8,opus','video/webm;codecs=vp9,opus','video/webm'];const mime=choices.find(x=>root.MediaRecorder.isTypeSupported(x));
    if(!mime)throw new Error('対応する録画codecがありません');
    recorder=new root.MediaRecorder(combined,{mimeType:mime,videoBitsPerSecond:2500000,audioBitsPerSecond:128000});
    const stamp=new Date().toISOString().replace(/[-:.]/g,'');const base=(options.revision||'local-game')+'-'+stamp+'-'+session;
    const meta={revision:options.revision||'unknown',source:options.source||{},canvasOnly:true,htmlHUDCaptured:false,requestedCanvasCaptureRate:frameHint,actualGameFPS:'UNMEASURED',microphone:false,displayCapture:false,networkSink:false,postAddedSound:false,tracks:combined.getTracks().map(t=>({kind:t.kind,settings:t.getSettings?t.getSettings():{}})),audioAtStart:audioLease.state(),startedUTC:new Date().toISOString(),initialState:snapshot(),states:events};
    recorder.ondataavailable=event=>{if(event.data&&event.data.size)chunks.push(event.data);};
    recorder.onerror=event=>{meta.error=String(event.error&&event.error.name||'MediaRecorder error');status.textContent='録画失敗：'+meta.error;active=false;cleanup();start.disabled=false;stop.hidden=true;};
    recorder.onstop=async()=>{
     const lease=audioLease;meta.wallDurationMs=elapsed();meta.finalState=snapshot();meta.audioAtEnd=lease?lease.state():null;active=false;stop.hidden=true;
     try{
      const blob=new root.Blob(chunks,{type:recorder.mimeType});meta.mimeType=recorder.mimeType;meta.videoBytes=blob.size;if(!blob.size)throw new Error('録画ファイルが空です');
      const bytes=await blob.arrayBuffer();
      if(root.crypto&&root.crypto.subtle){const digest=await root.crypto.subtle.digest('SHA-256',bytes);meta.videoSHA256=Array.from(new Uint8Array(digest)).map(x=>x.toString(16).padStart(2,'0')).join('');}else meta.videoSHA256='UNAVAILABLE';
      try{const decoded=await lease.decode(bytes.slice(0));let sum=0,peak=0,count=0;for(let c=0;c<decoded.numberOfChannels;c++){const data=decoded.getChannelData(c);for(const x of data){peak=Math.max(peak,Math.abs(x));sum+=x*x;count++;}}meta.decodedAudio={channels:decoded.numberOfChannels,sampleRate:decoded.sampleRate,duration:decoded.duration,samples:count,peak,rms:Math.sqrt(sum/Math.max(1,count)),actualSpeakerListening:'UNCONFIRMED'};}catch(error){meta.decodedAudio={state:'DECODE_UNCONFIRMED',reason:String(error.message||error),actualSpeakerListening:'UNCONFIRMED'};}
      link(blob,base+'.webm','WebMを保存');link(new root.Blob([JSON.stringify(meta,null,2)],{type:'application/json'}),base+'.json','状態JSONを保存');
      const player=doc.createElement('video');player.controls=true;player.playsInline=true;player.preload='metadata';player.src=root.URL.createObjectURL(blob);urls.push(player.src);player.style.cssText='width:100%;max-height:240px';result.appendChild(player);
      status.textContent='録画保存の準備完了：映像1本＋実出力音声1本。'+(meta.decodedAudio.peak>0?'録画音声に信号あり。':'音声の実デコードは要確認。');
      panel.dataset.recordingState='ready';
     }catch(error){status.textContent='録画失敗：'+String(error.message||error);panel.dataset.recordingState='failed';}
     finally{cleanup();start.disabled=false;stop.disabled=false;chunks=[];}
    };
    started=root.performance.now();active=true;stop.hidden=false;stop.disabled=false;panel.dataset.recordingState='recording';recorder.start(250);observe(snapshot());status.textContent='録画中：3D画面＋実出力音。停止して保存してください。';timer=root.setTimeout(stopRecording,Math.min(options.maxSeconds||45,60)*1000);
   }catch(error){active=false;cleanup();start.disabled=false;stop.hidden=true;status.textContent='録画を開始できません：'+String(error.message||error);panel.dataset.recordingState='failed';}
  };
  stop.onclick=stopRecording;
  return {observe,stop:stopRecording,panel,isRecording:()=>active};
 }
 const api={mount};if(typeof module!=='undefined')module.exports=api;else root.LocalGameRecorder=api;
})(typeof window!=='undefined'?window:{});
