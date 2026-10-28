'use strict';
(function(root){
 const kits=[
  {id:0,level:0,name:'すっぴんカニ',shape:{claw:45,shell:45,legs:45},description:'まずはそのまま。軽さと当たり方の基準。'},
  {id:1,level:1,name:'解体ハサミ',shape:{claw:68,shell:42,legs:34},description:'ハサミが大きく、打ち出しも強め。広く当たるぶん空気抵抗も増える。'},
  {id:2,level:2,name:'鉄鍋甲羅',shape:{claw:36,shell:60,legs:38},description:'甲羅が重い。打ち出しは遅め、重さと転がる当たり方で抜ける。'},
  {id:3,level:3,name:'カニ脚グリップ',shape:{claw:38,shell:40,legs:66},description:'大きな脚で接触を拾う。床の摩擦と空気抵抗も増える。'}
 ];
 const api=Object.freeze({kits:kits.map(k=>Object.freeze({...k,shape:Object.freeze(k.shape)})),key:'crab-flight-loadout-r29'});
 if(typeof module==='object'&&module.exports)module.exports=api;else root.CrabLoadout=api;
})(typeof window!=='undefined'?window:globalThis);
