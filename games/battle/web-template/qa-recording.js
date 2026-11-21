'use strict';
(function(){
 if(new URLSearchParams(location.search).get('qa')!=='record')return;
 const container=document.createElement('div');container.id='action-local-qa';document.querySelector('aside').appendChild(container);
 let latest=null;const meta=window.ActionQAMeta||{revision:'crab-action-r1-polish2'};
 const recorder=window.LocalGameRecorder.mount({container,canvas:document.getElementById('unity-canvas'),revision:meta.revision,source:meta,maxSeconds:60,
  beforeStart:async()=>{const button=document.getElementById('sound');if(button.getAttribute('aria-checked')!=='true')await button.onclick();},captureAudio:()=>window.FlightSound.captureOutput(),readState:()=>latest});
 const original=window.receiveActionState;window.receiveActionState=state=>{latest=state;original(state);recorder.observe(state);};
})();
