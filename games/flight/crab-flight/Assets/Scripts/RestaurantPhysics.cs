using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Authored, bounded destruction fixture. Selected CC0 food meshes are data only.
// The engine's real crab contact impulse opens a panel, after the solver step.
public sealed partial class RigidCrabFlight {
 sealed class BreakPanel {
  public string id,type;public Vector3 position,size;public Color color;public float threshold;
  public GameObject visual;public BoxCollider collider;public readonly List<BoxCollider> shapes=new List<BoxCollider>();public bool broken;public CrabCityDensity.CityVisualBreakable city;public CrabCityDensityR2.CityVisualBreakable city2;public City2Data.Proposal cityProposal;public CrabCityDensityR2.CityDensityPack.Asset cityAsset;public float retention;public LifeTargetRecordR6 life;
 }
 readonly List<BreakPanel> panels=new List<BreakPanel>();
 readonly List<GameObject> fragments=new List<GameObject>();
 readonly Dictionary<int,BreakPanel> breakByCollider=new Dictionary<int,BreakPanel>();
 readonly Dictionary<BreakPanel,Vector3> pendingBreaks=new Dictionary<BreakPanel,Vector3>();
 readonly Dictionary<BreakPanel,Vector3> breakNormals=new Dictionary<BreakPanel,Vector3>();
 public readonly List<WorldBreakEvent> BreakEvents=new List<WorldBreakEvent>();
 public int CityFragments {get;private set;}
 readonly Dictionary<BreakPanel,float> breakImpulses=new Dictionary<BreakPanel,float>();
 bool restaurant;PhysicsMaterial debrisMaterial;
 public int GlassBroken {get;private set;}public int WallsBroken {get;private set;}
 public int DishesBroken {get;private set;}public int FurnitureBroken {get;private set;}public int EquipmentBroken {get;private set;}public int FoodBroken {get;private set;}public int DestructionEvent {get;private set;}
 public int ActiveFragments=>fragments.Count;public bool Restaurant=>restaurant;
 public bool UseCC0FoodFragments=true;
 public int FoodFragmentsSpawned {get;private set;}
 public readonly List<string> FoodFragmentModels=new List<string>();
 public GameObject[] ReviewFragments=>fragments.ToArray();
 public int ReviewUnexpectedCrabDebrisContacts(){int failures=0;foreach(var obj in fragments){var c=obj.GetComponent<Collider>();if(obj.scene==Scene)foreach(var crabCollider in colliders)if(!UnityEngine.Physics.GetIgnoreCollision(c,crabCollider))failures++;}return failures;}
 public string LastBroken="";
 public readonly List<string> BrokenObjects=new List<string>();
 public void EnableRestaurant(){foreach(var unit in RestaurantWorkR18()){} }
 public IEnumerable<object> RestaurantWorkR18(){
  if(restaurant)yield break;restaurant=true;Floor("tank",new Vector3(0,.2f,0),new Vector3(8.9f,.4f,5.2f),Material("Tank stone",.55f,.65f,.05f));debrisMaterial=Material("Restaurant fragments",.48f,.55f,.13f);
  for(int side=-1;side<=1;side+=2){
   AddPanel("tank-glass-"+side,"glass",new Vector3(side*4.5f,3.25f,0),new Vector3(.13f,6.5f,5.2f),new Color(.44f,.77f,.81f),1.1f);
   AddPanel("dish-stack-"+side,"tableware",new Vector3(side*13.5f,1.6f,0),new Vector3(1.7f,3.2f,3.6f),new Color(.88f,.86f,.72f),.65f);
   // A grid of independent breakable wall cells; a high arc can also break the roof.
   for(int row=0;row<3;row++)AddPanel("outer-wall-"+side+"-"+row,"wall",new Vector3(side*25,(row+.5f)*4.5f,0),new Vector3(.55f,4.5f,7),new Color(.68f,.39f,.25f),3.2f);
   AddPanel("ceiling-"+side,"wall",new Vector3(side*12.5f,13.5f,0),new Vector3(24,.45f,7),new Color(.80f,.72f,.56f),2.6f);
   AddPanel("food-tray-"+side,"food",new Vector3(side*7.5f,1.9f,0),new Vector3(1.4f,1.2f,3),new Color(.81f,.54f,.28f),.25f);
   AddPanel("wood-chair-"+side,"wood",new Vector3(side*10,1.7f,0),new Vector3(1.6f,3.4f,3),new Color(.34f,.20f,.10f),.45f);
   AddPanel("wood-table-"+side,"wood",new Vector3(side*18,2.1f,0),new Vector3(3.3f,4.2f,4),new Color(.48f,.29f,.12f),.7f);
   AddPanel("metal-kitchen-"+side,"metal",new Vector3(side*21.5f,2.3f,0),new Vector3(1.6f,4.6f,3),new Color(.40f,.46f,.47f),1.6f);
   if(CrabFlightGame.ActiveCityLayout==0){AddPanel("wood-street-crate-"+side,"wood",new Vector3(side*39,1.7f,0),new Vector3(2.4f,3.4f,3),new Color(.40f,.29f,.16f),.65f);
   AddPanel("metal-street-bin-"+side,"metal",new Vector3(side*53,1.8f,0),new Vector3(1.5f,3.6f,3),new Color(.26f,.34f,.32f),1.15f);}
  }
  PrepareCostProbeR15.Mark("restaurant.nonCity");yield return null;if(CrabFlightGame.CityEnabled){if(CrabFlightGame.ActiveCityLayout>0){foreach(var unit in City2ContactWorkR18())yield return unit;}else EnableCityContacts();}PrepareCostProbeR15.Mark("restaurant.city");yield return null;EnableLifeContactsR6();PrepareCostProbeR15.Mark("restaurant.life");yield return null;
 }

 void AddPanel(string id,string type,Vector3 position,Vector3 size,Color color,float threshold){
  var panel=new BreakPanel{id=id,type=type,position=position,size=size,color=color,threshold=threshold};
  panel.visual=RestaurantVisuals.CreateBreakableVisual(type,position,size,color);panel.visual.name=id;BlenderPanelOverride.Apply(panel.visual,id);
  SceneManager.MoveGameObjectToScene(panel.visual,Scene);if(type=="wood"){AddPanelShape(panel,new Vector3(0,size.y*.36f,0),new Vector3(size.x,size.y*.28f,size.z));for(int sign=-1;sign<=1;sign+=2)AddPanelShape(panel,new Vector3(sign*size.x*.31f,-size.y*.14f,0),new Vector3(size.x*.15f,size.y*.72f,size.z*.75f));}else AddPanelShape(panel,Vector3.zero,size);panels.Add(panel);
 }
 void AddPanelShape(BreakPanel panel,Vector3 center,Vector3 size){bool traced=CityShapeSplitProbeR18.BeforeCreate(center,size);var c=panel.visual.AddComponent<BoxCollider>();c.center=center;c.size=size;c.providesContacts=true;c.sharedMaterial=debrisMaterial;c.contactOffset=.008f;panel.shapes.Add(c);panel.collider=c;surfaces[c.GetInstanceID()]=panel.type;breakByCollider[c.GetInstanceID()]=panel;var proxy=MirrorDebrisStaticCalmR4(c);if(proxy)breakByCollider[proxy.GetInstanceID()]=panel;CityShapeSplitProbeR18.AfterCreate(traced,proxy!=null);PrepareUnitTimelineR18.Created(proxy!=null);}
 void QueueBreak(int collider,float impulse,Vector3 point,Vector3 normal){
  if(!restaurant||!Body)return;ObservePortContact(collider,normal);BreakPanel panel;
  if(breakByCollider.TryGetValue(collider,out panel)&&!panel.broken){if(NewLoopActive){if(!NewLoopTarget(panel))return;ObserveNewLoopContact(panel,point);}if(primaryPending.Add(panel)&&secondarySources.Remove(panel))breakImpulses[panel]=0;pendingBreaks[panel]=point;breakNormals[panel]=normal;float total;breakImpulses.TryGetValue(panel,out total);breakImpulses[panel]=total+impulse;}
 }
 void ApplyBreaks(){
  QueuePortWaves();if(!restaurant||pendingBreaks.Count==0){portGroundThisStep=false;return;}
  float transmission=1;bool broke=false,ceilingBreak=false;int primaryCount=0;string burstId="";float burstWeight=0;Vector3 solverVelocity=Body.linearVelocity;
  foreach(var p in pendingBreaks.Keys)if(!p.broken&&!secondarySources.ContainsKey(p)&&!portWavePending.Contains(p)&&breakImpulses[p]/Body.mass>=p.threshold){transmission=Mathf.Min(transmission,PanelRetention(p));primaryCount++;}
  float mainBudget=.5f*Body.mass*incoming.sqrMagnitude*(1-transmission*transmission)/Mathf.Max(1,primaryCount);
  foreach(var entry in pendingBreaks.OrderBy(x=>x.Key.id,StringComparer.Ordinal)){
   var p=entry.Key;if(NewLoopActive&&!NewLoopTarget(p))continue;bool portWave=portWavePending.Contains(p);CityDebris secondary;bool isSecondary=secondarySources.TryGetValue(p,out secondary);float effectiveThreshold=p.threshold*(isSecondary?SecondaryThresholdFraction:1);if(p.broken||!(NewLoopActive&&newLoopBeamPending.Contains(p))&&breakImpulses[p]/Body.mass<effectiveThreshold)continue;if(isSecondary&&(!secondary.body||!SecondaryAllowed(p,secondary)))continue;if(!isSecondary&&!portWave){broke=true;if(p.id.StartsWith("ceiling-")&&breakNormals[p].y<-.7f&&incoming.y>0)ceilingBreak=true;if(burstId.Length==0||PanelImpactWeight(p)>burstWeight){burstId=p.id;burstWeight=PanelImpactWeight(p);}}p.broken=true;if(NewLoopActive)StopNewLoopPanel(p);ObservePortBreak(p,portWave,isSecondary);RecoverRotorFromBreakR12(p.id,isSecondary||portWave);SetPanelContactsCalmR4(p,false);if(p.city2)p.city2.SetBroken(true);else if(p.city)p.city.SetBroken(true);else p.visual.SetActive(false);
   if(p.type=="glass")GlassBroken++;else if(p.type=="wall")WallsBroken++;else if(p.type=="wood")FurnitureBroken++;else if(p.type=="metal")EquipmentBroken++;else if(p.type=="food")FoodBroken++;else if(p.type=="person")PeopleHitR6++;else if(p.type=="bird")BirdsHitR6++;else DishesBroken++;
   LastBroken=p.id;BrokenObjects.Add(p.id);DestructionEvent++;BreakEvents.Add(new WorldBreakEvent{cause=NewLoopActive?NewLoopBreakCause(p,portWave,isSecondary):null,id=p.id,material=p.type,position=entry.Value,normal=breakNormals[p],velocity=isSecondary?secondary.preVelocity:incoming,secondary=isSecondary||portWave,strength=Mathf.Clamp01(breakImpulses[p]/Body.mass/Mathf.Max(.01f,effectiveThreshold*4)),time=Time,weight=PanelImpactWeight(p)});if(p.city2){float budget=portWave?.5f*Body.mass*PortWaveImpulsePerMass*PortWaveImpulsePerMass:mainBudget;if(isSecondary){if(secondary.availableLoss<0)secondary.availableLoss=Mathf.Max(0,(float)(secondary.preEnergy-KineticEnergyR6(secondary.body)));budget=secondary.availableLoss*.5f;secondary.availableLoss-=budget;}SpawnCity2Shards(p,portWave?Vector3.up*PortWaveImpulsePerMass:isSecondary?secondary.preVelocity:incoming,isSecondary?secondary.body.mass:Body.mass,budget,isSecondary?secondary.root:p.id,isSecondary||portWave?1:0,entry.Value,breakNormals[p]);continue;}if(p.life!=null){SpawnLifeReactionR6(p,entry.Value,breakNormals[p],incoming,mainBudget);continue;}if(p.city){SpawnCityShards(p,entry.Value);continue;}
   // At most 48 physical fragments, with real initial momentum and bounded lifetime.
   int count=p.type=="glass"?5:p.type=="tableware"||p.type=="food"?3:p.type=="metal"?3:4;
   for(int i=0;i<count&&fragments.Count<48;i++){
    Vector3 size=Vector3.Scale(p.size,new Vector3(p.size.x>p.size.y?.24f:.65f,p.size.y>=p.size.x?.24f:.65f,.26f));
    size=Vector3.Max(size,Vector3.one*.09f);size=Vector3.Min(size,Vector3.one*1.5f);
    string model=(p.type=="tableware"||p.type=="food")&&UseCC0FoodFragments?new[]{"FoodFish","FoodSushi","FoodPlateBroken"}[i%3]:null;
    var prefab=model!=null?Resources.Load<GameObject>("RestaurantR2/"+model):null;
    var obj=prefab?UnityEngine.Object.Instantiate(prefab):GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=(prefab?"CC0 "+model+" physical fragment":"Original "+p.type+" fragment")+" for "+p.id;SceneManager.MoveGameObjectToScene(obj,FragmentSceneCalmR4);
    if(prefab){FoodFragmentsSpawned++;FoodFragmentModels.Add(model);}
    obj.transform.position=entry.Value+new Vector3(direction*.45f+(i%2-.5f)*.24f,(i/2-.5f)*.26f,(i%2==0?-1:1)*(1.2f+i*.16f));obj.transform.localScale=size;
    if(!prefab)obj.GetComponent<Renderer>().sharedMaterial=p.visual.GetComponentInChildren<Renderer>().sharedMaterial;obj.AddComponent<FragmentMaterialRole>().materialType=p.type;ReadableMaterials.Fragment(obj,p.type,UseReadableSurfaces);
    var c=obj.GetComponent<Collider>();c.enabled=true;c.sharedMaterial=debrisMaterial;c.contactOffset=.005f;if(!SeparateDebrisCalmR4)foreach(var crabCollider in colliders)UnityEngine.Physics.IgnoreCollision(c,crabCollider,true);
    // Debris reacts to the body but is not a second damage/record or chain trigger.
    var rb=obj.AddComponent<Rigidbody>();rb.mass=p.type=="food"?.06f:p.type=="glass"?.08f:p.type=="metal"?1.1f:p.type=="wood"?.32f:.12f;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;rb.linearDamping=.3f;rb.angularDamping=.6f;
    rb.linearVelocity=Vector3.ClampMagnitude(Body.linearVelocity*.22f+new Vector3(direction*1.3f,1.1f,(i%2==0?-1:1)*1.4f),18);rb.angularVelocity=new Vector3(i+1,2-i,1)*2;
    BlenderPanelOverride.ApplyFragment(obj,p.id,p.type);
    fragments.Add(obj);
   }
  }
  // Frangible static panels absorb part of pre-contact momentum rather than the full
  // rigid-wall stop already computed by the solver. Never exceeds incoming energy.
  if(broke&&Body.linearVelocity.sqrMagnitude<incoming.sqrMagnitude*transmission*transmission)Body.linearVelocity=incoming*transmission;
  Vector3 beforeRedirect=Body.linearVelocity;
  // A broken locked ceiling turns the existing upward motion along the
  // travel direction. Preserve its translational energy instead of deleting
  // all normal motion (the stronger70deg throw lost3669J vs685J).
  if(UseRotorBursts&&UseCeilingRicochet&&ceilingBreak&&!CeilingUnlocked){var v=Body.linearVelocity;float up=Mathf.Max(0,v.y);float travel=incoming.x==0?direction:Mathf.Sign(incoming.x);v.x=travel*Mathf.Sqrt(v.x*v.x+up*up);v.y=Mathf.Min(0,v.y);Body.linearVelocity=v;}
#if UNITY_EDITOR
  Vector3 recoveredVelocity=Body.linearVelocity;
#endif
  if(ceilingBreak&&CeilingUnlocked)CeilingBreakthrough=true;
  if(broke&&burstId.Length>0)ReleaseBreakRotor(burstId,burstWeight);
#if UNITY_EDITOR
  if(broke)ReviewBreakResponses.Add(new BreakResponseTrace{id=burstId,time=Time,position=Body.position,incoming=incoming,solver=solverVelocity,beforeRedirect=beforeRedirect,recovered=recoveredVelocity,afterBurst=Body.linearVelocity,ceiling=ceilingBreak,normalAbsorptionEnabled=false,normalAbsorptionApplied=false,ceilingRedirectApplied=UseRotorBursts&&UseCeilingRicochet&&ceilingBreak&&!CeilingUnlocked});
#endif
  ApplyPortRoles();if(NewLoopActive){newLoopBeamPending.Clear();newLoopPhysicalPending.Clear();}pendingBreaks.Clear();breakImpulses.Clear();breakNormals.Clear();primaryPending.Clear();secondarySources.Clear();
 }
 void ResetRestaurant(){
  ResetPortRoles();ClearFragmentVisibility();if(!restaurant)return;foreach(var item in fragments){if(item)item.SetActive(false);Release(item);}fragments.Clear();pendingBreaks.Clear();breakImpulses.Clear();breakNormals.Clear();
  ResetCity2();BreakEvents.Clear();CityFragments=0;foreach(var panel in panels){if(panel.city2){foreach(Transform child in panel.city2.fractured.transform)child.gameObject.SetActive(true);panel.city2.ResetVisual();}else if(panel.city){foreach(var pivot in panel.city.shardPivots)pivot.gameObject.SetActive(true);panel.city.ResetVisual();}panel.broken=false;panel.visual.SetActive(true);SetPanelContactsCalmR4(panel,true);}
  GlassBroken=WallsBroken=DishesBroken=FurnitureBroken=EquipmentBroken=FoodBroken=DestructionEvent=FoodFragmentsSpawned=0;LastBroken="";BrokenObjects.Clear();FoodFragmentModels.Clear();
 }
}
