mergeInto(LibraryManager.library,{ActionReport:function(pointer){var state=JSON.parse(UTF8ToString(pointer));if(typeof window.receiveActionState==="function")window.receiveActionState(state);}});

mergeInto(LibraryManager.library,{
 ActionProbeFrame:function(frame,seconds,delta,phase,lk,lc,lp,lf,lcue,rk,rc,rp,rf,rcue,shots,sparks,eventId){if(typeof window.receiveActionProbeFrame==="function")window.receiveActionProbeFrame(performance.now(),frame,seconds,delta,phase,lk,lc,lp,lf,lcue,rk,rc,rp,rf,rcue,shots,sparks,eventId);},
 ActionProbeInfo:function(pointer){if(typeof window.receiveActionProbeInfo==="function")window.receiveActionProbeInfo(JSON.parse(UTF8ToString(pointer)));}
});
