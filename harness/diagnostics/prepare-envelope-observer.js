(()=>{
 const d=window.__prepareTimelineCase={control:null,run:null,errors:[],focus:[],recording:false,rafCount:0,syncCount:0,rafDropped:0,syncDropped:0,previous:null,previousCallback:null};
 const raw=new Float64Array(2048*5),sync=new Float64Array(4096*6);
 window.addEventListener('error',e=>d.errors.push(String(e.message)));window.addEventListener('unhandledrejection',e=>d.errors.push(String(e.reason)));
 for(const type of ['blur','focus','visibilitychange'])window.addEventListener(type,()=>d.focus.push({type,t:performance.now(),hidden:document.hidden,focused:document.hasFocus()}));
 window.__crabSyncR8={active:false,record(kind,t,frame,swSeconds,seq,phase){if(!d.recording||!(kind===60||kind===61||kind===70||kind===71||kind===2||kind===3))return;if(d.syncCount>=4096){d.syncDropped++;return;}let k=d.syncCount++*6;sync[k]=kind;sync[k+1]=t;sync[k+2]=frame;sync[k+3]=swSeconds;sync[k+4]=seq;sync[k+5]=phase;}};
 function raf(t){const at=performance.now();if(d.recording){if(d.rafCount>=2048)d.rafDropped++;else{let k=d.rafCount++*5;raw[k]=t;raw[k+1]=d.previous===null?NaN:t-d.previous;raw[k+2]=at;raw[k+3]=document.hidden?1:0;raw[k+4]=document.hasFocus()?1:0;}}d.previous=t;d.previousCallback=at;requestAnimationFrame(raf);}requestAnimationFrame(raf);
 d.start=()=>{if(d.recording)throw Error('Already recording');d.rafCount=d.syncCount=d.rafDropped=d.syncDropped=0;d.recording=true;d.arm=performance.now();d.previousRAFAtArm=d.previous;d.previousCallbackAtArm=d.previousCallback;window.__crabSyncR8.active=true;};
 d.stop=()=>{d.recording=false;window.__crabSyncR8.active=false;return{arm:d.arm,previousRAFAtArm:d.previousRAFAtArm,previousCallbackAtArm:d.previousCallbackAtArm,end:performance.now(),rafCount:d.rafCount,syncCount:d.syncCount,rafDropped:d.rafDropped,syncDropped:d.syncDropped,rawRAF:Array.from(raw.subarray(0,d.rafCount*5)),sync:Array.from(sync.subarray(0,d.syncCount*6))};};
 d.exports={};d.prior=window.receiveFlightState;window.receiveFlightState=s=>{if(s.resourceAudit){const x=JSON.parse(s.resourceAudit);if(x.schema)d.exports[x.schema]=x;}if(d.recording&&s.phase==='ready'&&d.run&&Number.isFinite(d.run.sendPrepareBefore)&&!d.run.firstReadyAt)d.run.firstReadyAt=performance.now();d.prior(s);};
 d.wait=async f=>{let t=performance.now();while(!f()){if(document.hidden||!document.hasFocus()||lastState?.paused)throw Error('Normal focus/visibility/Pause boundary');if(performance.now()-t>15000)throw Error('Bounded Prepare timeout');await new Promise(r=>setTimeout(r,20));}};
 return{installed:true,rafCapacity:2048,syncCapacity:4096,perUnitJSON:false};
})()
