using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CrabLifeActors;

[Serializable]public sealed class LifeContactReceiptR6 {public string objectId,kind;public int eventId,generation;public float time,budget;public bool accepted;}
[Serializable]public sealed class LifeScatterAuditR6 {public int peopleTargets,birdTargets,peopleHit,birdsHit,liveThrown,fragmentCount,cityFragments;public double maxEnergyExcess;public LifeContactReceiptR6[] contacts;public ScatterReceiptR6[] scatter;}
public sealed partial class RigidCrabFlight {
 public static bool LifeContactsEnabledR6=true;public LifeGeometryCacheR17 LifeGeometryCacheR17;
 sealed class LifeTargetRecordR6 {public string id;public ActorKind kind;public LifeActorRig rig;public bool bird;}
 sealed class LifeThrownR6 {public LifeActorRig rig;public float born;}
 readonly List<LifeTargetRecordR6> lifeTargetsR6=new List<LifeTargetRecordR6>();
 readonly List<LifeThrownR6> lifeThrownR6=new List<LifeThrownR6>();
 readonly List<LifeContactReceiptR6> lifeReceiptsR6=new List<LifeContactReceiptR6>();
 int lifeGenerationR6;float lifeAgeR6;
 public int PeopleHitR6{get;private set;}public int BirdsHitR6{get;private set;}
 public int LifePeopleTargetsR6=>lifeTargetsR6.Count(x=>!x.bird);public int LifeBirdTargetsR6=>lifeTargetsR6.Count(x=>x.bird);
 static readonly float[] PeopleAnchorsR6={15,60,145,280,628,705,827,906,1008,1150,1270};
 static readonly float[] BirdAnchorsR6={45,165,650,800,970,1150};
 public static bool ReplacesBirdR6(float x,int k)=>LifeContactsEnabledR6&&k==0&&Array.IndexOf(BirdAnchorsR6,x)>=0;
 void EnableLifeContactsR6(){
  if(!LifeContactsEnabledR6||lifeTargetsR6.Count>0)return;
  for(int side=-1;side<=1;side+=2){
   foreach(float x in PeopleAnchorsR6){var kind=x<28?ActorKind.Chef:x<135?ActorKind.Shopper:x<600?ActorKind.OfficeWalker:x<900?ActorKind.Courier:ActorKind.OfficeWalker;AddLifeTargetR6("life-person-"+side+"-"+x,kind,new Vector3(side*x,0,0),2.1f,side);}
   foreach(float x in BirdAnchorsR6){bool gull=x>=600&&x<900;AddLifeTargetR6("life-bird-"+side+"-"+x,gull?ActorKind.HarborGull:ActorKind.CityPigeon,new Vector3(side*x,gull?7.5f:x<135?11f:8.5f,0),3f,side);}
  }
 }
 void AddLifeTargetR6(string id,ActorKind kind,Vector3 position,float scale,int side){
  var root=new GameObject(id);SceneManager.MoveGameObjectToScene(root,Scene);root.transform.position=position;
  var rig=LifeActorRig.Create(kind,root.transform,Vector3.zero,0,scale,(int)Mathf.Abs(position.x),LifeGeometryCacheR17);bool bird=rig.IsBird;if(bird)rig.SetBirdMode(BirdMode.Flying);
  var item=new LifeTargetRecordR6{id=id,kind=kind,rig=rig,bird=bird};lifeTargetsR6.Add(item);
  var p=new BreakPanel{id=id,type=bird?"bird":"person",position=position,size=Vector3.one,threshold=bird?.13f:.4f,visual=root,retention=bird?.998f:.995f,life=item};
  if(bird)AddPanelShape(p,new Vector3(0,.40f*scale,0),new Vector3(.42f,.64f,.70f)*scale);
  else{AddPanelShape(p,new Vector3(0,.91f*scale,0),new Vector3(.62f,1.48f,.45f)*scale);AddPanelShape(p,new Vector3(0,1.75f*scale,-.03f*scale),new Vector3(.46f,.55f,.45f)*scale);}
  panels.Add(p);CityContactTargets++;
 }
 void SpawnLifeReactionR6(BreakPanel p,Vector3 point,Vector3 normal,Vector3 incomingVelocity,float budget){
  var obj=UnityEngine.Object.Instantiate(p.visual,p.visual.transform.position,p.visual.transform.rotation);obj.name="Comic flight for "+p.id;SceneManager.MoveGameObjectToScene(obj,FragmentSceneCalmR4);obj.SetActive(true);
  RecycleCityFragment();if(fragments.Count>=48||CityFragments>=20){Release(obj);return;}
  var record=new CityDebris{obj=obj,root=p.id,depth=0,origin=p.position};
  foreach(var c in obj.GetComponents<BoxCollider>()){c.enabled=true;c.sharedMaterial=debrisMaterial;c.providesContacts=false;c.contactOffset=.005f;record.colliderIds.Add(c.GetInstanceID());debrisByCollider[c.GetInstanceID()]=record;if(!SeparateDebrisCalmR4)foreach(var crabCollider in colliders)UnityEngine.Physics.IgnoreCollision(c,crabCollider,true);}
  var rb=obj.AddComponent<Rigidbody>();rb.mass=p.life.bird?.16f:1.5f;rb.linearDamping=.3f;rb.angularDamping=.6f;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;record.body=rb;
  ScatterShardsR6(new List<Rigidbody>{rb},point,normal,incomingVelocity,budget,p.id);
  var rig=obj.GetComponentInChildren<LifeActorRig>();rig.ResetVisuals(lifeGenerationR6);rig.PhysicalFlight=true;bool accepted=rig.React(ReactionCue.Contact,lifeAgeR6,rig.transform.InverseTransformDirection(incomingVelocity),1,DestructionEvent,lifeGenerationR6);
  lifeReceiptsR6.Add(new LifeContactReceiptR6{objectId=p.id,kind=p.life.kind.ToString(),eventId=DestructionEvent,generation=lifeGenerationR6,time=Time,budget=budget,accepted=accepted});
  lifeThrownR6.Add(new LifeThrownR6{rig=rig,born=lifeAgeR6});cityDebris.Add(record);fragments.Add(obj);CityFragments++;
 }
 // Sample bones once per visual frame, not each 1/240s physics step; root contacts stay fixed.
 public void TickLifeContactsR6(float dt,float viewX,bool reduced,bool animate){
  lifeAgeR6+=dt;
  foreach(var a in lifeTargetsR6)if(a.rig&&a.rig.gameObject.activeInHierarchy&&Mathf.Abs(a.rig.transform.position.x-viewX)<90)a.rig.Sample(lifeAgeR6,reduced,animate);
  for(int i=lifeThrownR6.Count-1;i>=0;i--){var a=lifeThrownR6[i];if(!a.rig){lifeThrownR6.RemoveAt(i);continue;}if(Mathf.Abs(a.rig.transform.position.x-viewX)<90)a.rig.Sample(lifeAgeR6,reduced,animate);}
 }
 void ResetLifeContactsR6(){lifeGenerationR6++;lifeAgeR6=0;PeopleHitR6=BirdsHitR6=0;lifeThrownR6.Clear();lifeReceiptsR6.Clear();foreach(var a in lifeTargetsR6){a.rig.ResetVisuals(lifeGenerationR6);if(a.bird)a.rig.SetBirdMode(BirdMode.Flying);}}
 public SkinnedMeshRenderer[] LifeRenderersFeelR6(string id)=>fragments.Where(x=>x&&x.name=="Comic flight for "+id).SelectMany(x=>x.GetComponentsInChildren<SkinnedMeshRenderer>()).ToArray();
 public string ReadLifeScatterR6()=>JsonUtility.ToJson(new LifeScatterAuditR6{peopleTargets=LifePeopleTargetsR6,birdTargets=LifeBirdTargetsR6,peopleHit=PeopleHitR6,birdsHit=BirdsHitR6,liveThrown=lifeThrownR6.Count(x=>x.rig),fragmentCount=fragments.Count,cityFragments=CityFragments,maxEnergyExcess=ScatterMaxEnergyExcessR6,contacts=lifeReceiptsR6.ToArray(),scatter=scatterReceiptsR6.ToArray()});
}
