using System;
using System.IO;
using UnityEngine;
public static class AnatomyExitReview {
 [Serializable] public class ExitRun {public string variant;public FlightState state;public string[] broken;public int colliders,missingMaterials;public bool before,impact,after,outside;public float exitTime;}
 [Serializable] public class Result {public string input="10deg/power85/spin10.2/phase0, fixed240Hz; same physical r2 envelope";public ExitRun[] runs;}
 public static void Run(){Directory.CreateDirectory("../evidence/exit-comparison");var result=new Result{runs=new ExitRun[2]};
  for(int index=0;index<2;index++){bool baseline=index==0;CrabModel.ReviewUseBaseline(baseline);RestaurantVisuals.ExteriorEnabled=!baseline;var g=BuildFlight.PrepareScene();g.Initialize();g.Prepare("");g.Configure(JsonUtility.ToJson(new FlightSettings{claw=45,shell=45,legs=45,angle=10,power=85}));g.BeginAim("");g.ReleaseLaunch(JsonUtility.ToJson(new FlightAimInput{angle=10,power=85,direction=1,spinRps=10.2f,spinPhase=0,eligible=true}));var row=new ExitRun{variant=baseline?"r2-original":"anatomy1-exit1",exitTime=-1};
   try{for(int tick=0;tick<9000&&!g.Finished;tick++){g.Tick(1f/240);var s=g.State();bool hit=g.ReviewSimulation.BrokenObjects.Exists(x=>x.StartsWith("outer-wall"));if(!row.before&&s.worldX>19&&!hit){Shot(g,row.variant,"before");row.before=true;}if(!row.impact&&hit){row.impact=true;row.exitTime=s.time;Shot(g,row.variant,"impact");}if(row.impact&&!row.after&&s.time>=row.exitTime+.12f){Shot(g,row.variant,"after120ms");row.after=true;}if(row.impact&&!row.outside&&s.time>=row.exitTime+.6f&&s.worldX>29){Shot(g,row.variant,"outside");row.outside=true;}}
    foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){foreach(var material in renderer.sharedMaterials)if(!material||!material.shader||!material.shader.isSupported)row.missingMaterials++;}
    row.state=g.State();row.broken=g.ReviewSimulation.BrokenObjects.ToArray();row.colliders=g.ReviewSimulation.ColliderCount;result.runs[index]=row;File.WriteAllText("../evidence/exit-comparison/result.json",JsonUtility.ToJson(result,true));if(!g.Finished||row.state.physicsFault.Length>0||!row.before||!row.impact||!row.after||!row.outside||row.colliders!=13||row.missingMaterials>0)throw new Exception("Exit sequence incomplete");
   }finally{g.ShutdownReviewPhysics();}
  }CrabModel.ReviewUseBaseline(false);RestaurantVisuals.ExteriorEnabled=true;
 }
 static void Shot(CrabFlightGame g,string variant,string phase){RigidFlightReview.Capture(g,"../evidence/exit-comparison/"+variant+"-"+phase+".png",844,390);}
}
