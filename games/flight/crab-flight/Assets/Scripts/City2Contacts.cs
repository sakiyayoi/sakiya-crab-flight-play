using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using C2=CrabCityDensityR2;

public sealed partial class RigidCrabFlight {
 sealed class CityDebris {public GameObject obj;public Rigidbody body;public string root;public int depth;public Vector3 origin,preVelocity;public float availableLoss;public double preEnergy;public readonly List<int> colliderIds=new List<int>();}
 sealed class CityDecoration {public GameObject obj;public Vector3 velocity,omega;public float age;}
 readonly List<CityDebris> cityDebris=new List<CityDebris>();readonly Dictionary<int,CityDebris> debrisByCollider=new Dictionary<int,CityDebris>();
 readonly List<CityDecoration> cityDecor=new List<CityDecoration>();readonly Dictionary<BreakPanel,CityDebris> secondarySources=new Dictionary<BreakPanel,CityDebris>();readonly Dictionary<string,int> chainUsed=new Dictionary<string,int>();readonly HashSet<BreakPanel> primaryPending=new HashSet<BreakPanel>();
 public int CitySecondaryBreaks{get;private set;}public int CityContactTargets{get;private set;}public int CityDecorativeFragments=>cityDecor.Count;
 public static bool EnableSecondaryChains=true;public static float SecondaryThresholdFraction=.25f;
 public ContactVisualTemplateR15 CityVisualTemplateR15;
 void EnableCity2Contacts(){foreach(var unit in City2ContactWorkR18()){} }
 public IEnumerable<object> City2ContactWorkR18(){
  var root=City2Data.Build(CrabFlightGame.ActiveCityLayout,true,UseOverdrive,CityVisualTemplateR15);SceneManager.MoveGameObjectToScene(root,Scene);PrepareCostProbeR15.Mark("city2.visualsAndMove");
  CityShapeSplitProbeR18.BeginGroup();
  foreach(var visual in root.GetComponentsInChildren<C2.CityVisualBreakable>()){
   PrepareUnitTimelineR18.BeginUnit(visual.placementId,UnityEngine.Time.renderedFrameCount);
   var prop=City2Data.Prop(visual.assetId);var asset=City2Data.Asset(visual.assetId);string type=(visual.assetId.Contains("crate")||visual.assetId=="market_stall"||visual.assetId=="street_bench")?"wood":visual.assetId=="street_awning"?"wood":"metal";
   if(!string.IsNullOrEmpty(prop.material))type=prop.material;type=SectionWorldArtR16.BreakMaterial(visual.assetId,visual.transform.position,type);var p=new BreakPanel{id=visual.placementId,type=type,position=visual.transform.position,size=visual.suggestedEnvelopeSize,threshold=prop.normalizedImpulseThresholdSuggestion,visual=visual.gameObject,city2=visual,cityProposal=prop,cityAsset=asset,retention=prop.maxPostBreakSpeedFractionSuggestion};
   CityShapeSplitProbeR18.BeginVisual(asset,prop,visual.placementId);
   // Individual thin parts and detachable panels; never the whole prop AABB.
   foreach(var shard in prop.shards.Where(x=>x.shard>=0))AddPanelShape(p,City2Data.V(shard.center),Vector3.Max(Vector3.one*.035f,City2Data.V(shard.size)));
   foreach(var piece in asset.pieces.Where(x=>x.shard<0).Select(x=>City2Data.Bounds(x)).OrderByDescending(x=>x.size.x*x.size.y*x.size.z).Take(4))AddPanelShape(p,piece.center,Vector3.Max(Vector3.one*.035f,piece.size));
   if(p.shapes.Count==0)throw new Exception("City2 prop has no visible contact shapes");panels.Add(p);CityContactTargets++;CityShapeSplitProbeR18.EndVisual();PrepareUnitTimelineR18.EndUnit();yield return null;
  }
  CityShapeSplitProbeR18.EndGroup();PrepareCostProbeR15.Mark("city2.contactShapes");
 }
 void PrepareCityDebris(){foreach(var d in cityDebris){if(d.body){d.preVelocity=d.body.linearVelocity;d.preEnergy=KineticEnergyR6(d.body);d.availableLoss=-1;}}}
 void ReadCityDebrisContacts(ContactPairHeader header){
  if(!EnableSecondaryChains||cityDebris.Count==0)return;
  for(int j=0;j<header.pairCount;j++){var pair=header.GetContactPair(j);if(pair.isCollisionExit)continue;CityDebris source;int other;
   if(debrisByCollider.TryGetValue(pair.colliderInstanceID,out source))other=pair.otherColliderInstanceID;else if(debrisByCollider.TryGetValue(pair.otherColliderInstanceID,out source))other=pair.colliderInstanceID;else continue;
   BreakPanel p;if(!breakByCollider.TryGetValue(other,out p))continue;
#if UNITY_EDITOR
   ReviewObserveChainContact(p,source,pair);
#endif
   if(p.broken||!p.city2||source.depth>0||p.id==source.root||p.cityProposal.massKgSuggestion>100||Vector3.Distance(p.position,source.origin)>10||primaryPending.Contains(p))continue;
   int used;chainUsed.TryGetValue(source.root,out used);if(used>=2)continue;
   for(int k=0;k<pair.contactCount;k++){var c=pair.GetContactPoint(k);if(c.separation>.02f&&c.impulse.sqrMagnitude<.000001f)continue;Vector3 n=c.normal;if(Vector3.Dot(n,source.obj.transform.position-c.position)<0)n=-n;CityDebris prior;if(secondarySources.TryGetValue(p,out prior)&&prior!=source){if(c.impulse.magnitude<=breakImpulses[p])continue;breakImpulses[p]=0;}pendingBreaks[p]=c.position;breakNormals[p]=n;secondarySources[p]=source;float v;breakImpulses.TryGetValue(p,out v);breakImpulses[p]=v+c.impulse.magnitude;}
  }
 }
 bool SecondaryAllowed(BreakPanel p,CityDebris source){int used;chainUsed.TryGetValue(source.root,out used);if(used>=2)return false;chainUsed[source.root]=used+1;CitySecondaryBreaks++;return true;}
 float PanelRetention(BreakPanel p){return p.city2||p.life!=null?p.retention:p.type=="glass"?.96f:p.type=="tableware"||p.type=="food"?.985f:p.type=="wood"?.95f:p.type=="metal"?.87f:.82f;}
 void RecycleCityFragment(){
  if(fragments.Count<48&&CityFragments<20)return;
  var record=cityDebris.FirstOrDefault();var old=record!=null?record.obj:fragments.FirstOrDefault();if(!old)return;
  foreach(var c in old.GetComponentsInChildren<Collider>())c.enabled=false;old.SetActive(false);fragments.Remove(old);
  if(record!=null){foreach(var id in record.colliderIds)debrisByCollider.Remove(id);cityDebris.Remove(record);CityFragments--;}
  Release(old);
 }
#if UNITY_EDITOR
 public float ReviewShardVisualBoundsError{get;private set;}
 public float ReviewShardEnergyExcess{get;private set;}
 static Bounds RenderBounds(GameObject o){var rs=o.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(o.transform.position,Vector3.zero);var b=rs[0].bounds;for(int k=1;k<rs.Length;k++)b.Encapsulate(rs[k].bounds);return b;}
#endif
 void SpawnCity2Shards(BreakPanel p,Vector3 sourceVelocity,float sourceMass,float energyBudget,string rootId,int depth,Vector3 hitPoint,Vector3 hitNormal){
  var spawned=new List<Rigidbody>(6);var original=p.city2;var pivots=original.shardPivots;int physicalPieces=0;
  // The fixed scaffold also collapses. Its compound boxes retain the visible rails/poles.
  var fixedNodes=original.fractured.transform.Cast<Transform>().Where(t=>!t.name.StartsWith("Shard_")).ToArray();
  var pieces=pivots.Select(t=>new Tuple<Transform,int>(t,int.Parse(t.name.Substring(6)))).ToList();
  if(fixedNodes.Length>0)pieces.Insert(0,new Tuple<Transform,int>(fixedNodes[0],-1));
  float volume=Mathf.Abs(original.transform.lossyScale.x*original.transform.lossyScale.y*original.transform.lossyScale.z);var masses=pieces.Select(x=>Mathf.Clamp((x.Item2<0?Mathf.Min(3,p.cityProposal.massKgSuggestion*.08f):p.cityProposal.shards.First(s=>s.shard==x.Item2).massKgSuggestion)*volume,.05f,3)).ToArray();
#if UNITY_EDITOR
 float spawnedKinetic=0;
#endif
  float physicalMass=masses.Take(6).Sum();float speedCap=Mathf.Sqrt(Mathf.Max(0,2*energyBudget/Mathf.Max(.05f,physicalMass)));var velocity=Vector3.ClampMagnitude(sourceVelocity,Mathf.Min(18,speedCap));
  for(int i=0;i<pieces.Count;i++){
   var source=pieces[i].Item1;int shard=pieces[i].Item2;
#if UNITY_EDITOR
   var expectedBounds=RenderBounds(source.gameObject);
#endif
   var obj=UnityEngine.Object.Instantiate(source.gameObject,source.position,source.rotation);obj.transform.localScale=source.lossyScale;obj.name="City2 "+p.id+" fragment "+shard+" for "+p.id;SceneManager.MoveGameObjectToScene(obj,FragmentSceneCalmR4);obj.SetActive(true);obj.AddComponent<FragmentMaterialRole>().materialType=p.type;ReadableMaterials.Fragment(obj,p.type,UseReadableSurfaces);
#if UNITY_EDITOR
   var actualBounds=RenderBounds(obj);ReviewShardVisualBoundsError=Mathf.Max(ReviewShardVisualBoundsError,Vector3.Distance(actualBounds.min,expectedBounds.min),Vector3.Distance(actualBounds.max,expectedBounds.max));
#endif
   source.gameObject.SetActive(false);
   if(physicalPieces>=6){while(cityDecor.Count>=12){Release(cityDecor[0].obj);cityDecor.RemoveAt(0);}cityDecor.Add(new CityDecoration{obj=obj,velocity=velocity,omega=new Vector3(.4f,1,.8f)});continue;}
   RecycleCityFragment();if(fragments.Count>=48||CityFragments>=20){Release(obj);continue;}
   var record=new CityDebris{obj=obj,root=rootId,depth=depth,origin=p.position};
   var boxes=shard<0?p.cityAsset.pieces.Where(x=>x.shard<0).Select(x=>City2Data.Bounds(x)).OrderByDescending(x=>x.size.x*x.size.y*x.size.z).Take(4).ToArray():new[]{new Bounds(City2Data.V(p.cityProposal.shards.First(s=>s.shard==shard).center),City2Data.V(p.cityProposal.shards.First(s=>s.shard==shard).size))};
   foreach(var b in boxes){var c=obj.AddComponent<BoxCollider>();var worldCenter=original.transform.TransformPoint(b.center);c.center=obj.transform.InverseTransformPoint(worldCenter);var worldScale=original.transform.lossyScale;var cloneScale=obj.transform.lossyScale;c.size=Vector3.Max(Vector3.one*.035f,Vector3.Scale(b.size,new Vector3(Mathf.Abs(worldScale.x/cloneScale.x),Mathf.Abs(worldScale.y/cloneScale.y),Mathf.Abs(worldScale.z/cloneScale.z))));c.sharedMaterial=debrisMaterial;c.providesContacts=false;c.contactOffset=.005f;record.colliderIds.Add(c.GetInstanceID());debrisByCollider[c.GetInstanceID()]=record;if(!SeparateDebrisCalmR4)foreach(var crabCollider in colliders)UnityEngine.Physics.IgnoreCollision(c,crabCollider,true);}
   var rb=obj.AddComponent<Rigidbody>();record.body=rb;rb.mass=masses[i];rb.linearDamping=.3f;rb.angularDamping=.6f;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;rb.linearVelocity=velocity;spawned.Add(rb);
#if UNITY_EDITOR
   spawnedKinetic+=.5f*rb.mass*rb.linearVelocity.sqrMagnitude;
#endif
   cityDebris.Add(record);fragments.Add(obj);CityFragments++;physicalPieces++;
  }
if(DirectionalShardsR6)ScatterShardsR6(spawned,hitPoint,hitNormal,sourceVelocity,energyBudget,p.id);
#if UNITY_EDITOR
  if(DirectionalShardsR6){spawnedKinetic=0;foreach(var rb in spawned)spawnedKinetic+=(float)KineticEnergyR6(rb);}
  ReviewShardEnergyExcess=Mathf.Max(ReviewShardEnergyExcess,spawnedKinetic-energyBudget);
#endif
 }
 void TickCityDecor(float dt){for(int i=cityDecor.Count-1;i>=0;i--){var d=cityDecor[i];d.age+=dt;if(!d.obj||d.age>3){if(d.obj)Release(d.obj);cityDecor.RemoveAt(i);continue;}d.velocity+=Vector3.down*9.81f*dt;d.obj.transform.position+=d.velocity*dt;d.obj.transform.rotation=Quaternion.AngleAxis(d.omega.magnitude*dt*Mathf.Rad2Deg,d.omega.normalized)*d.obj.transform.rotation;if(d.obj.transform.position.y<0){var p=d.obj.transform.position;p.y=0;d.obj.transform.position=p;d.velocity*=.4f;}}}
 void ResetCity2(){
#if UNITY_EDITOR
 ReviewResetContactTrace();
#endif
 scatterReceiptsR6.Clear();ScatterMaxEnergyExcessR6=0;ResetLifeContactsR6();cityDebris.Clear();debrisByCollider.Clear();secondarySources.Clear();primaryPending.Clear();chainUsed.Clear();foreach(var d in cityDecor)if(d.obj)Release(d.obj);cityDecor.Clear();CitySecondaryBreaks=0;}
// Invoked in a player only through the localhost-only review entrypoint.
 public void ReviewChainProjectile(bool hit){
  // Isolated collision fixture, never used by gameplay or browser input verification.
  Body.useGravity=false;Body.position=new Vector3(-200,10,0);Body.linearVelocity=Vector3.right*.2f;Body.angularVelocity=Vector3.zero;Body.WakeUp();
  var p=panels.First(x=>x.city2&&x.city2.assetId=="market_crate"&&x.position.x>0);var target=p.shapes[0].bounds.center;
  var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name="Editor chain contact fixture";SceneManager.MoveGameObjectToScene(obj,FragmentSceneCalmR4);obj.transform.localScale=Vector3.one*.4f;obj.transform.position=target-Vector3.right*3+(hit?Vector3.zero:Vector3.forward*8);
  var c=obj.GetComponent<BoxCollider>();c.providesContacts=true;var rb=obj.AddComponent<Rigidbody>();rb.mass=2;rb.linearVelocity=Vector3.right*18;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
  var d=new CityDebris{obj=obj,body=rb,root="editor-fixture-primary",origin=target-Vector3.right*3,depth=0};d.colliderIds.Add(c.GetInstanceID());cityDebris.Add(d);debrisByCollider[c.GetInstanceID()]=d;fragments.Add(obj);CityFragments++;
 }
}
