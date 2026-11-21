using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public static class NeonStage6DensityReview {
 [Serializable]public class Family {public string asset;public bool isNew;public int baseline,added,total,flight;}
 [Serializable]public class Sector {public string id;public float start,end,baselineDensity,stage5Density,flightBandPer100m;public int total,ground,mid,high,newFlight;public Family[] families;}
 [Serializable]public class Audit {public bool pass,originalTargetsUnchanged,stage5OriginalSignaturesUnchanged,monotonic;public NeonDensityReview.World before,after;public int additions,addedColliders;public Sector[] sectors;}
 public static Audit Compare(NeonDensityReview.World baseline,CrabFlightGame game){
  var after=NeonDensityReview.Read(game);var added=after.targets.Where(t=>NeonDensity.IsAddition(t.id)).ToArray();
  var prior=JsonUtility.FromJson<NeonDensityReview.Audit>(File.ReadAllText(Path.GetFullPath("../../../../evidence/stage5-native-shots02/stage5-density-audit.json"))).after;
  string[] ids={"tank","restaurant","mall","office","port","viaduct","country","launchrange","orbit"};float[] edges={0,4.5f,28,135,600,900,1300,2050,2680,3220};
  var sectors=ids.Select((id,i)=>{var rows=after.targets.Where(t=>t.position.x>=edges[i]&&t.position.x<edges[i+1]).ToArray();var old=baseline.targets.Where(t=>t.position.x>=edges[i]&&t.position.x<edges[i+1]).ToArray();var p=prior.targets.Where(t=>t.position.x>=edges[i]&&t.position.x<edges[i+1]).ToArray();
   return new Sector{id=id,start=edges[i],end=edges[i+1],total=rows.Length,ground=rows.Count(t=>t.position.y<2),mid=rows.Count(t=>t.position.y>=2&&t.position.y<20),high=rows.Count(t=>t.position.y>=20),newFlight=rows.Count(t=>t.asset.EndsWith("_neon6")&&t.position.y<20),baselineDensity=old.Count(t=>t.position.y<20)*100f/(edges[i+1]-edges[i]),stage5Density=p.Count(t=>t.position.y<20)*100f/(edges[i+1]-edges[i]),flightBandPer100m=rows.Count(t=>t.position.y<20)*100f/(edges[i+1]-edges[i]),families=rows.GroupBy(t=>t.asset).OrderBy(g=>g.Key).Select(g=>new Family{asset=g.Key,isNew=g.Key.EndsWith("_neon6"),baseline=g.Count(t=>!NeonDensity.IsAddition(t.id)),added=g.Count(t=>NeonDensity.IsAddition(t.id)),total=g.Count(),flight=g.Count(t=>t.position.y<20)}).ToArray()};}).ToArray();
  bool unchanged=baseline.targets.All(t=>after.targets.Count(a=>a.id==t.id&&a.signature==t.signature&&a.registered==t.registered&&a.layer==t.layer&&a.tag==t.tag)==1);
  bool priorSame=prior.targets.Where(t=>!NeonDensity.IsAddition(t.id)).All(t=>after.targets.Count(a=>a.id==t.id&&a.signature==t.signature)==1);
  bool monotonic=sectors[1].flightBandPer100m<=sectors[2].flightBandPer100m&&Enumerable.Range(2,6).All(i=>sectors[i].flightBandPer100m>sectors[i+1].flightBandPer100m);
  return new Audit{before=baseline,after=after,sectors=sectors,additions=added.Length,addedColliders=added.Sum(t=>t.colliders),originalTargetsUnchanged=unchanged,stage5OriginalSignaturesUnchanged=priorSame,monotonic=monotonic,pass=unchanged&&priorSame&&monotonic&&after.totalBreakTargets-baseline.totalBreakTargets==NeonDensity.Additions.Length&&added.Length==NeonDensity.Additions.Length&&added.All(t=>t.colliders>0&&t.registered&&t.position.x>0&&t.sector!="mall")&&sectors.Skip(4).All(s=>s.newFlight>=.2f*(s.ground+s.mid)&&s.families.Count(f=>f.isNew)>=2)};
 }
 [Serializable]class Contact {public string asset,id,scope="Existing PhysX contact/break path; new target raised 20m and crab collider aimed at its first shape in an Editor-only isolation fixture. Separate from fixed-input route replay.";public bool registered,broken,eventObserved,pass;public int ticks,collidersAfter,fragments;public string physicsFault;public WorldBreakEvent[] events;}
 [Serializable]class Contacts {public bool pass;public List<Contact> families=new List<Contact>();}
 public static void FamilyContacts(){
  string root=Path.GetFullPath("../../../..");string dir=Environment.GetEnvironmentVariable("NEON_STAGE6_CONTACTS")??root+"/evidence/stage6-contacts02";if(Directory.Exists(dir))throw new Exception("fresh contacts directory required");Directory.CreateDirectory(dir);
  var receipt=new Contacts();CrabFlightGame game=null;
  try{
   NeonProto.Enabled=MallDensityProto.Enabled=NeonDensity.Enabled=true;UnityEngine.Random.InitState(42043);game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
   var families=NeonDensityReview.Targets(game).Where(t=>t.assetId.EndsWith("_neon6")).Select(t=>t.assetId).Distinct().OrderBy(t=>t).ToArray();
   foreach(var family in families){
    game.Prepare("");game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));game.ReviewReplayAcceptedCalmR3(File.ReadAllText(root+"/accepted-input.json"));
    var target=NeonDensityReview.Targets(game).Where(t=>t.assetId==family&&t.transform.position.y<2).OrderBy(t=>Mathf.Abs(t.transform.position.z)).First();
    var row=new Contact{asset=family,id=target.placementId,registered=NeonDensityReview.Read(game).targets.Single(t=>t.id==target.placementId).registered};receipt.families.Add(row);
    target.transform.position+=Vector3.up*20;UnityEngine.Physics.SyncTransforms();
    game.ReviewSimulation.ReviewRouteSegmentR11(target.placementId);
    // Move the existing collision fixture to the authored lateral lane; runtime physics is untouched.
    var body=game.ReviewSimulation.Body;var shell=body.GetComponentsInChildren<Collider>().OrderByDescending(c=>c.bounds.size.x*c.bounds.size.y*c.bounds.size.z).First();var point=target.GetComponents<BoxCollider>().First().bounds.center;body.position+=point-shell.bounds.center-Vector3.right*4;game.ReviewCrab.Root.position=body.position;UnityEngine.Physics.SyncTransforms();
    typeof(RigidCrabFlight).GetField("lockedComZ",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(game.ReviewSimulation,body.worldCenterOfMass.z);
    for(int k=0;k<240&&!target.IsBroken&&!game.Finished;k++){game.Tick(1f/60);row.ticks++;}
    row.broken=target.IsBroken;row.events=game.ReviewSimulation.BreakEvents.Where(e=>e.id==target.placementId).ToArray();row.eventObserved=row.events.Length>0;row.collidersAfter=target.GetComponents<Collider>().Count(c=>c.enabled);row.fragments=game.ReviewSimulation.CityFragments;row.physicsFault=game.State().physicsFault;row.pass=row.registered&&row.broken&&row.eventObserved&&row.collidersAfter==0&&row.fragments>0&&string.IsNullOrEmpty(row.physicsFault);
    NeonProtoReview.Capture(game.ReviewCamera,dir+"/stage6-broken-"+family+".png");
   }
   receipt.pass=receipt.families.Count>=10&&receipt.families.All(x=>x.pass);if(!receipt.pass)throw new Exception("New family physical contact failed; see contacts receipt");
  }finally{File.WriteAllText(dir+"/stage6-family-contacts.json",JsonUtility.ToJson(receipt,true));if(game)game.ShutdownReviewPhysics();}
 }
}
