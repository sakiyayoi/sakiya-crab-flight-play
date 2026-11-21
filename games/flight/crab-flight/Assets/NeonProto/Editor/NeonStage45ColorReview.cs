using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class NeonStage45ColorReview {
 [Serializable]class SampleCheck {public string name,phase;public float spin,firstStep,settled;public bool eligible,reduced,effects,neonEnabled,expected;public Color material;}
 [Serializable]class Receipt {public bool pass;public string rate="Exponential lerp 10/s; snap only within 0.0001 of endpoint";public List<SampleCheck> checks=new List<SampleCheck>();}
 public static void Check(CrabFlightGame game,string dir){
  var result=new Receipt();
  try{
   game.ReviewStage45Isolation();
   foreach(string name in new[]{"flight-positive","flight-negative","aim-negative","charge-positive","ready-no-spin","ready-unarmed","result","reduced","effects-off","threshold","neon-disabled"}){
    bool expected=name=="flight-positive"||name=="flight-negative"||name=="aim-negative"||name=="charge-positive";
    string phase=name=="result"?"result":name.StartsWith("ready")||name.StartsWith("aim")||name.StartsWith("charge")?"ready":"flight";
    bool eligible=name.StartsWith("aim")||name.StartsWith("charge"),reduced=name=="reduced",effects=name!="effects-off",enabled=name!="neon-disabled";
    float spin=name=="ready-no-spin"?0:name=="threshold"?1:name.Contains("negative")?-20:20;
    // Settle at the opposite endpoint first, then measure the first real frame.
    NeonProto.Enabled=true;
    for(int i=0;i<90;i++)game.ReviewSampleVortex(1f/60,expected?"ready":"flight",expected?0:20,90,false,true);
    float before=game.State().neon.spinCoreBlend;
    if(before!=(expected?0:1))throw new Exception(name+" opposite endpoint failed");
    NeonProto.Enabled=enabled;game.ReviewSampleVortex(0,phase,spin,90,reduced,effects,eligible);
    if(game.State().neon.spinCoreBlend!=before)throw new Exception(name+" zero dt changed color");
    game.ReviewSampleVortex(1f/60,phase,spin,90,reduced,effects,eligible);float first=game.State().neon.spinCoreBlend;
    if(!(first>0&&first<1))throw new Exception(name+" color popped at transition");
    for(int i=0;i<89;i++)game.ReviewSampleVortex(1f/60,phase,spin,90,reduced,effects,eligible);
    var state=game.State().neon;Color target=expected?NeonProto.CrabRed:NeonProto.CrabCore;
    result.checks.Add(new SampleCheck{name=name,phase=phase,spin=spin,firstStep=first,settled=state.spinCoreBlend,eligible=eligible,reduced=reduced,effects=effects,neonEnabled=enabled,expected=expected,material=state.coreColor});
    if(state.spinCoreBlend!=(expected?1:0)||state.coreColor!=target||state.spinVortexActive!=expected)throw new Exception(name+" final material/gate mismatch");
    NeonProto.Enabled=true;
   }
   // Renderer-only fade-back at the same final staged pose.
   NeonStage45Identify.Capture(game,dir,"stage45-core-return-dark");result.pass=true;
  }finally{NeonProto.Enabled=true;File.WriteAllText(dir+"/stage45-core-boundaries.json",JsonUtility.ToJson(result,true));}
 }
}
