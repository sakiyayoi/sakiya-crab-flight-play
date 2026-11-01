'use strict';
(function(root){
 const text=[
  '企画・制作：さきや／リリー（Codex）',
  '本ゲームの自作部分（コード・絵・効果音）の著作権はさきや。第三者コード・3Dデータは各作者の権利とライセンスに従います。',
  'voice/ の音声は VOICEVOX:ずんだもん の利用規約に従います（さきやの著作権の範囲外）。',
  '音声：VOICEVOX:ずんだもん',
  '配信・動画では『VOICEVOX:ずんだもん』の表記をお願いします。',
  '声ファイルはゲームコード・自作素材のライセンスの対象外です。ずんだもん音源利用ガイドラインとVOICEVOXソフトウェア利用規約に従います。詳しくは同梱のvoice/NOTICE.mdをご覧ください。',
  '効果音：本案件のCrab Kitchen sound pack r1（数式・固定seedによる合成、Python/NumPy・Web Audio・FFmpeg）。',
  '3D・エフェクト：本案件で制作したモデルとコード（Unity・Blender・C#・Python）。全素材の作者と道具の一覧はCREDITS.mdをご覧ください。',
  '第三者3D：Kenney「Food Kit 2.0」（CC0、料理・皿）／methodical pixel「Crab low poly animated 3d model」（CC0、比較用データ）。',
  '第三者コード：RuntimeInspector（Süleyman Yasir KULA / MIT）、ColorWheel（Felix Kate / BSD 3-Clause）。原文のNOTICEをlicenses/へ同梱。'
 ];
 function mount({parent=document.querySelector('#title .title-actions')||document.getElementById('title'),onOpen=()=>{},onClose=()=>{}}={}){
  if(!parent)throw Error('Credit entry parent is missing');
  if(document.getElementById('credits'))throw Error('Credits already mounted');
  const entry=document.createElement('button');entry.id='creditsButton';entry.type='button';entry.textContent='クレジット';entry.style.cssText='min-height:48px;align-self:flex-start;padding:8px 16px;font-size:16px';
  const dialog=document.createElement('dialog');dialog.id='credits';dialog.setAttribute('aria-labelledby','crabCreditsTitle');
  dialog.style.cssText='box-sizing:border-box;max-width:min(640px,calc(100vw - 32px));max-height:calc(100dvh - 32px);padding:24px;background:var(--ink);color:var(--paper);font:inherit;line-height:1.7;overflow:auto;border:1px solid currentColor';
  const heading=document.createElement('h2');heading.id='crabCreditsTitle';heading.textContent='クレジット';heading.tabIndex=-1;heading.setAttribute('autofocus','');heading.style.marginTop='0';dialog.append(heading);
  for(const line of text){const p=document.createElement('p');p.textContent=line;p.style.fontSize='14px';dialog.append(p);}
  const back=document.createElement('button');back.type='button';back.id='creditsBack';back.textContent='戻る';back.style.cssText='min-height:48px;min-width:100px;font-size:16px';back.onclick=()=>dialog.close();dialog.append(back);
  dialog.addEventListener('close',()=>{onClose();entry.focus({preventScroll:true});});
  entry.onclick=()=>{onOpen();dialog.showModal();heading.focus({preventScroll:true});dialog.scrollTop=0;};
  parent.insertBefore(entry,parent.querySelector('.muted'));document.body.append(dialog);
  return {entry,dialog,destroy(){if(dialog.open)dialog.close();entry.remove();dialog.remove();}};
 }
 const api={mount,text:()=>text.slice()};if(typeof module!=='undefined')module.exports=api;else root.CrabCredits=api;
})(typeof window!=='undefined'?window:{});
