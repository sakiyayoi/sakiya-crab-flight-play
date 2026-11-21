'use strict';
// Strict user rule. No statistical tolerance, trimming, rounding or retry selection.
function inspectPerf10(record, version, zipSHA256) {
  const problems=[];
  if (!record || record.version!==version || record.zipSHA256!==zipSHA256) problems.push('版／ZIP SHAが現在の実物と一致しない');
  if (!record || !Array.isArray(record.rows) || record.rows.length!==10) return {ok:false,problems:[...problems,'同じ実行の10投が必要'],rows:[]};
  if (!record.conditions || record.conditions.turns!==10 || record.conditions.tolerance!==0) problems.push('測定条件／許容差0の記録が無い');
  if (!Array.isArray(record.errors) || record.errors.length) problems.push('エラー記録が無い、またはエラーがある');
  const sourceIDs=new Set(),serials=new Set();
  const rows=record.rows.map((r,i)=>{
    const f=r.perf?.frames;
    if(r.i!==i+1 || !Array.isArray(f) || f.length<2 || !r.perf.seenFlight || r.perf.visibility!=='visible' || !f.some(v=>v[3]==='flight') || f.at(-1)[3]!=='result') problems.push(`投${i+1}:測定区間が不完全`);
    const valid=Array.isArray(f) && f.every((v,j)=>Array.isArray(v)&&Number.isFinite(v[0])&&Number.isFinite(v[1])&&v[1]>0&&(!j||v[0]>f[j-1][0]));
    if(!valid)problems.push(`投${i+1}:不正なフレーム生値`);
    const values=valid?f.map(v=>v[1]):[];
    const mean=values.reduce((s,v)=>s+v,0)/values.length,worst=values.reduce((m,v)=>Math.max(m,v),0);
    if(r.after?.version!==version || !r.after?.sourceIdentity || r.final?.fault)problems.push(`投${i+1}:版／実行識別／物理状態が不適合`);
    sourceIDs.add(r.after?.sourceIdentity);serials.add(r.launch?.serial);
    return {i:i+1,samples:values.length,meanFrameMs:mean,worstFrameMs:worst};
  });
  if(sourceIDs.size!==1 || serials.size!==10 || serials.has(undefined))problems.push('同一版／異なる実投10件の識別が揃っていない');
  for(const r of rows.slice(1))if(r.meanFrameMs>rows[0].meanFrameMs || r.worstFrameMs>rows[0].worstFrameMs)problems.push(`投${r.i}が1投目より重い:平均${r.meanFrameMs}ms／最悪${r.worstFrameMs}ms`);
  return {ok:problems.length===0,problems,rows,metric:record.metric||record.method};
}
module.exports={inspectPerf10};
