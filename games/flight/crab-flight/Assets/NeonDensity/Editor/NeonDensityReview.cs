using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;
using C2=CrabCityDensityR2;

public static class NeonDensityReview {
 [Serializable] public class Target {public string id,asset,sector,tag;public Vector3 position,scale;public int layer,colliders;public bool registered;public string signature;}
 [Serializable] public class World {public int totalBreakTargets,cityBreakTargets;public Target[] targets;}
 [Serializable] public class Family {public string asset;public int count;}
 [Serializable] public class Sector {public string id;public int total,ground,mid,high;public float flightBandPer100m;public Family[] families;}
 [Serializable] public class Audit {public bool pass,originalTargetsUnchanged;public World before,after;public int additions,addedColliders;public Sector[] sectors;}
 [Serializable] public class View {public string sector;public Vector3 cameraPosition;public float size;public int addedInFrustum;}
 static RigidCrabFlight PhysicsWorld(CrabFlightGame game)=>(RigidCrabFlight)typeof(CrabFlightGame).GetField("world",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
 public static C2.CityVisualBreakable[] Targets(CrabFlightGame game)=>PhysicsWorld(game).Scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<C2.CityVisualBreakable>(true)).ToArray();
 public static World Read(CrabFlightGame game){
  var world=PhysicsWorld(game);var panels=(IList)typeof(RigidCrabFlight).GetField("panels",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(world);
  var registered=panels.Cast<object>().Select(p=>(string)p.GetType().GetField("id").GetValue(p)).ToHashSet();
  return new World{totalBreakTargets=panels.Count,cityBreakTargets=world.CityContactTargets,targets=Targets(game).Select(v=>new Target{
   id=v.placementId,asset=v.assetId,sector=WorldRouteR11.RegionAt(v.transform.position.x),position=v.transform.position,scale=v.transform.localScale,
   tag=v.tag,layer=v.gameObject.layer,colliders=v.GetComponents<Collider>().Count(c=>c.enabled&&!c.isTrigger),registered=registered.Contains(v.placementId),
   signature=v.assetId+"|"+JsonUtility.ToJson(v.transform.position)+"|"+JsonUtility.ToJson(v.transform.localScale)+"|"+JsonUtility.ToJson(v.transform.rotation)+"|"+string.Join(";",v.GetComponents<BoxCollider>().Select(c=>JsonUtility.ToJson(c.center)+JsonUtility.ToJson(c.size)))
  }).ToArray()};
 }
 public static Audit Compare(World before,CrabFlightGame game){
  var after=Read(game);var added=after.targets.Where(t=>NeonDensity.IsAddition(t.id)).ToArray();
  var sectors=WorldRouteR11.Data.sectors.Where(s=>s.id!="mall"&&s.id!="tank").Select(s=>{
   var rows=added.Where(t=>t.sector==s.id).ToArray();
   return new Sector{id=s.id,total=rows.Length,ground=rows.Count(t=>t.position.y<2),mid=rows.Count(t=>t.position.y>=2&&t.position.y<20),high=rows.Count(t=>t.position.y>=20),
    flightBandPer100m=rows.Count(t=>t.position.y<20)*100f/(s.end-s.start),families=rows.GroupBy(t=>t.asset).OrderBy(g=>g.Key).Select(g=>new Family{asset=g.Key,count=g.Count()}).ToArray()};
  }).ToArray();
  bool original=before.targets.All(t=>after.targets.Count(a=>a.id==t.id&&a.signature==t.signature)==1);
  return new Audit{before=before,after=after,additions=added.Length,addedColliders=added.Sum(t=>t.colliders),originalTargetsUnchanged=original,sectors=sectors,
   pass=original&&after.totalBreakTargets-before.totalBreakTargets==NeonDensity.Additions.Length&&after.cityBreakTargets-before.cityBreakTargets==added.Length&&added.Length==NeonDensity.Additions.Length&&added.All(t=>t.colliders>0&&t.registered&&t.position.x>0&&t.sector!="tank"&&t.sector!="mall")&&sectors.All(s=>s.flightBandPer100m>=6)};
 }
 public static void CaptureSectors(CrabFlightGame game,string dir){
  var camera=game.ReviewCamera;camera.aspect=1.6f;camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));
  var added=Targets(game).Where(v=>NeonDensity.IsAddition(v.placementId)).ToArray();
  var views=new System.Collections.Generic.List<View>();
  foreach(var s in WorldRouteR11.Data.sectors.Where(s=>s.id!="tank"&&s.id!="mall")){
   float x=s.id=="restaurant"?17:s.viewX;
   camera.orthographicSize=s.id=="restaurant"?7:12;camera.transform.position=new Vector3(x,s.id=="restaurant"?10:12,-30);
   game.ReviewCrab.Root.position=new Vector3(x-7,3.3f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);
   var frustum=GeometryUtility.CalculateFrustumPlanes(camera);
   int visible=added.Count(v=>v.intact.GetComponentsInChildren<Renderer>().Any(r=>GeometryUtility.TestPlanesAABB(frustum,r.bounds)));
   if(visible<2)throw new Exception("Density screenshot has fewer than two additional targets: "+s.id);
   NeonProtoReview.Capture(camera,dir+"/stage4-sector-"+s.id+".png");
   foreach(var v in added)v.gameObject.SetActive(false);
   NeonProtoReview.Capture(camera,dir+"/stage4-sector-"+s.id+"-before.png");
   foreach(var v in added)v.gameObject.SetActive(true);
   views.Add(new View{sector=s.id,cameraPosition=camera.transform.position,size=camera.orthographicSize,addedInFrustum=visible});
  }
  File.WriteAllLines(dir+"/stage4-sector-views.jsonl",views.Select(v=>JsonUtility.ToJson(v)));
 }
}
