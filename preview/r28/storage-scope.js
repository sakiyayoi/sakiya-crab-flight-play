'use strict';
// Deployment shell only. The tested game payload is unchanged; its origin storage is scoped here.
(()=>{const prefix='kani-preview:kani-v0.2-feel-r27c:';
 const get=Storage.prototype.getItem,set=Storage.prototype.setItem,remove=Storage.prototype.removeItem;
 Storage.prototype.getItem=function(k){return get.call(this,prefix+String(k));};
 Storage.prototype.setItem=function(k,v){return set.call(this,prefix+String(k),v);};
 Storage.prototype.removeItem=function(k){return remove.call(this,prefix+String(k));};
 const open=indexedDB.open.bind(indexedDB);indexedDB.open=(name,...args)=>open(prefix+String(name),...args);
 if(navigator.locks){const request=navigator.locks.request.bind(navigator.locks);navigator.locks.request=(name,...args)=>request(prefix+String(name),...args);}
 window.__kaniPreviewStorage={version:'kani-v0.3-impact-r28',prefix,scope:'separate preview storage; stable saves untouched'};
})();
