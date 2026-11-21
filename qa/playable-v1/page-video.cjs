'use strict';
// Local game page only, using the already installed FFmpeg explicitly.
// No desktop, microphone, audio input, browser-registry edits, or downloads.
const {spawn}=require('child_process');
const {once}=require('events');
const fs=require('fs');
const path=require('path');
const {performance}=require('perf_hooks');
async function startPageVideo(page,{output,ffmpeg,width=1280,height=800,fps=12}){
 if(!fs.existsSync(ffmpeg))throw Error('Existing FFmpeg executable is missing: '+ffmpeg);
 fs.mkdirSync(path.dirname(output),{recursive:true});
 const args=['-hide_banner','-loglevel','error','-f','image2pipe','-framerate',String(fps),'-c:v','mjpeg','-i','pipe:0','-an','-c:v','libvpx','-deadline','realtime','-cpu-used','8','-threads','1','-b:v','900k','-y',output];
 const child=spawn(ffmpeg,args,{stdio:['pipe','ignore','pipe']});let stderr='',spawnError=null;
 child.stderr.on('data',b=>{stderr+=b.toString();});child.on('error',e=>{spawnError=e;});
 const exited=new Promise(resolve=>child.on('exit',(code,signal)=>resolve({code,signal})));
 child.stdin.on('error',e=>{spawnError=e;});
 const cdp=await page.context().newCDPSession(page),start=performance.now();
 let history=[],last=null,written=0,framesReceived=0,stopping=false,endFrames=null,pumpError=null;
 const onFrame=event=>{
  const entry={at:performance.now()-start,jpeg:Buffer.from(event.data,'base64')};history.push(entry);framesReceived++;
  cdp.send('Page.screencastFrameAck',{sessionId:event.sessionId}).catch(()=>{});
 };
 cdp.on('Page.screencastFrame',onFrame);
 try {await cdp.send('Page.startScreencast',{format:'jpeg',quality:80,maxWidth:width,maxHeight:height,everyNthFrame:1});}
 catch(error){cdp.off('Page.screencastFrame',onFrame);await cdp.detach().catch(()=>{});child.stdin.end();await exited;throw error;}
 const pump=(async()=>{
  try{
   while(!stopping||written<endFrames){
    const target=endFrames??Math.floor((performance.now()-start)/1000*fps);
    if(!last&&!history.length&&stopping)throw Error('No screencast frames received');
    if(written>=target||(!last&&!history.length)){await new Promise(r=>setTimeout(r,10));continue;}
    const at=written/fps*1000;
    while(history.length&&history[0].at<=at)last=history.shift();
    const frame=last||history[0];
    if(!child.stdin.write(frame.jpeg))await once(child.stdin,'drain');
    written++;
   }
  }catch(e){pumpError=e;}
 })();
 return {async stop(){
  if(stopping)throw Error('Recorder already stopped');
  const endMs=performance.now()-start;endFrames=Math.floor(endMs/1000*fps);stopping=true;
  await cdp.send('Page.stopScreencast').catch(()=>{});await pump;
  cdp.off('Page.screencastFrame',onFrame);await cdp.detach();child.stdin.end();
  const result=await exited;
  const receipt={output,ffmpeg,args,framesReceived,encodedFrames:written,fps,wallMs:endMs,encodedMs:written/fps*1000,bytes:fs.existsSync(output)?fs.statSync(output).size:0,exitCode:result.code,signal:result.signal,stderr,scope:'continuous Chrome game-page viewport including DOM HUD; silent; no desktop/microphone/audio capture'};
  if(spawnError||pumpError||result.code!==0||!receipt.bytes)throw Error(JSON.stringify({...receipt,error:String(spawnError||pumpError||'encoding failed')}));
  return receipt;
 }};
}
module.exports={startPageVideo};
