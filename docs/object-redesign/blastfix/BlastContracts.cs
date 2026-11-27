using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// Minimal recording doubles. No PhysX: destroyed bodies throw on COM reads,
// both immediately and when queued for deferred destruction.
namespace UnityEngine {
 public class MissingReferenceException:Exception {public MissingReferenceException():base("destroyed Rigidbody worldCenterOfMass"){} }
 public class Object {public bool destroyed;public static implicit operator bool(Object o)=>o!=null&&!o.destroyed;
  public static GameObject Instantiate(GameObject p)=>new GameObject();}
 public class Transform {public Vector3 position,localScale;}
 public class GameObject:Object {public string name;public Transform transform=new Transform();public Rigidbody rb;public bool active=true;public readonly Collider collider=new Collider();
  public void SetActive(bool a){active=a;} public static GameObject CreatePrimitive(PrimitiveType p)=>new GameObject();
  public T AddComponent<T>() where T:new(){var t=new T();if(t is Rigidbody r){rb=r;r.gameObject=this;}return t;}
  public T GetComponent<T>() where T:new()=>new T(); public T GetComponentInChildren<T>() where T:new()=>new T();
  public T[] GetComponentsInChildren<T>()=>new T[]{(T)(object)collider};}
 public class Collider:Object {static int next;public readonly int id=++next;public bool enabled=true;public object sharedMaterial;public float contactOffset;public int GetInstanceID()=>id;}
 public class BoxCollider:Collider {}
 public class Renderer {public object sharedMaterial;}
 public class Rigidbody:Object {public GameObject gameObject;public float mass=1,maxAngularVelocity,linearDamping,angularDamping;public Vector3 linearVelocity,angularVelocity,inertiaTensor=Vector3.one,position;public Quaternion rotation,inertiaTensorRotation;public CollisionDetectionMode collisionDetectionMode;
  public Vector3 worldCenterOfMass {get {if(destroyed)throw new MissingReferenceException();return gameObject.transform.position;}}public void ResetInertiaTensor(){} }
 public struct Color {}
 public struct Quaternion {public static Quaternion Inverse(Quaternion q)=>q;public static Quaternion operator*(Quaternion a,Quaternion b)=>a;public static Vector3 operator*(Quaternion q,Vector3 v)=>v;}
 public struct Vector3 {public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;} public static Vector3 zero=>new Vector3();public static Vector3 one=>new Vector3(1,1,1);public static Vector3 up=>new Vector3(0,1,0);public static Vector3 right=>new Vector3(1,0,0);public static Vector3 forward=>new Vector3(0,0,1);
  public float sqrMagnitude=>x*x+y*y+z*z;public float magnitude=>(float)Math.Sqrt(sqrMagnitude);public Vector3 normalized=>magnitude>0?this/magnitude:zero;public void Normalize(){this=normalized;}
  public static Vector3 operator+(Vector3 a,Vector3 b)=>new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);public static Vector3 operator-(Vector3 a,Vector3 b)=>a+(-b);public static Vector3 operator-(Vector3 a)=>a*-1;public static Vector3 operator*(Vector3 a,float b)=>new Vector3(a.x*b,a.y*b,a.z*b);public static Vector3 operator/(Vector3 a,float b)=>a*(1/b);
  public static float Dot(Vector3 a,Vector3 b)=>a.x*b.x+a.y*b.y+a.z*b.z;public static Vector3 Cross(Vector3 a,Vector3 b)=>new Vector3(a.y*b.z-a.z*b.y,a.z*b.x-a.x*b.z,a.x*b.y-a.y*b.x);
  public static Vector3 ProjectOnPlane(Vector3 a,Vector3 n)=>a-n*Dot(a,n);public static Vector3 Scale(Vector3 a,Vector3 b)=>new Vector3(a.x*b.x,a.y*b.y,a.z*b.z);
  public static Vector3 Min(Vector3 a,Vector3 b)=>new Vector3(Math.Min(a.x,b.x),Math.Min(a.y,b.y),Math.Min(a.z,b.z));public static Vector3 Max(Vector3 a,Vector3 b)=>new Vector3(Math.Max(a.x,b.x),Math.Max(a.y,b.y),Math.Max(a.z,b.z));public static Vector3 ClampMagnitude(Vector3 a,float b)=>a.magnitude>b?a.normalized*b:a;}
 public static class Mathf {public static float Min(float a,float b)=>Math.Min(a,b);public static float Max(float a,float b)=>Math.Max(a,b);public static float Clamp01(float a)=>Math.Clamp(a,0,1);public static float Sqrt(float a)=>(float)Math.Sqrt(a);public static float Sign(float a)=>Math.Sign(a);}
 public static class Resources {public static T Load<T>(string p) where T:class=>null;}
 public static class Physics {public static void IgnoreCollision(Collider a,Collider b,bool c){} }
 public enum PrimitiveType {Cube}public enum CollisionDetectionMode {ContinuousSpeculative}
}
static class SceneManager {public static void MoveGameObjectToScene(GameObject o,int scene){} }
class FragmentMaterialRole {public string materialType;}
static class ReadableMaterials {public static void Fragment(GameObject o,string s,bool r){} }
static class BlenderPanelOverride {public static void ApplyFragment(GameObject o,string s,string t){} }
class CityVisual:UnityEngine.Object {public void SetBroken(bool v){} }
class WorldBreakEvent {public string id,material;public Vector3 position,normal,velocity;public bool secondary;public float strength,time,weight;}
public sealed partial class RigidCrabFlight {
 sealed class BreakPanel {public string id,type="glass";public bool broken,objectSuppressed;public float threshold=1;public Vector3 position,size;public GameObject visual=new GameObject();public CityVisual city2=new CityVisual(),city;public object life;}
 sealed class CityDebris {public GameObject obj;public Rigidbody body;public Vector3 preVelocity;public float availableLoss;public double preEnergy;public string root;public readonly List<int> colliderIds=new List<int>();}
 readonly List<GameObject> fragments=new List<GameObject>();readonly List<CityDebris> cityDebris=new List<CityDebris>();readonly Dictionary<int,CityDebris> debrisByCollider=new Dictionary<int,CityDebris>();
 readonly Dictionary<int,BreakPanel> breakByCollider=new Dictionary<int,BreakPanel>();readonly Dictionary<BreakPanel,Vector3> pendingBreaks=new Dictionary<BreakPanel,Vector3>(),breakNormals=new Dictionary<BreakPanel,Vector3>();readonly Dictionary<BreakPanel,float> breakImpulses=new Dictionary<BreakPanel,float>();readonly Dictionary<BreakPanel,CityDebris> secondarySources=new Dictionary<BreakPanel,CityDebris>();
 readonly HashSet<BreakPanel> objectBreakPending=new HashSet<BreakPanel>(),objectBlastPending=new HashSet<BreakPanel>(),primaryPending=new HashSet<BreakPanel>(),portWavePending=new HashSet<BreakPanel>();
 readonly List<WorldBreakEvent> BreakEvents=new List<WorldBreakEvent>();readonly List<string> BrokenObjects=new List<string>(),FoodFragmentModels=new List<string>();
 readonly List<Collider> colliders=new List<Collider>();readonly ObjectRules.Run ObjectRun=new ObjectRules.Run();
 Rigidbody Body=new Rigidbody{mass=3,linearVelocity=new Vector3(10,0,0)};bool restaurant=true,UseObjectRedesign=true,portGroundThisStep,UseCC0FoodFragments,SeparateDebrisCalmR4=true,UseReadableSurfaces,UseRotorBursts,UseCeilingRicochet,CeilingUnlocked,CeilingBreakthrough;
 float Time=1,SecondaryThresholdFraction=.25f,PortWaveImpulsePerMass=3;int direction=1,CityFragments,GlassBroken,WallsBroken,FurnitureBroken,EquipmentBroken,FoodBroken,PeopleHitR6,BirdsHitR6,DishesBroken,DestructionEvent,FoodFragmentsSpawned,FragmentSceneCalmR4,spawnCount;string LastBroken;object debrisMaterial;Vector3 incoming=new Vector3(10,0,0);
 readonly List<GameObject> deferred=new List<GameObject>();bool deferRelease;
 void Release(GameObject o){if(deferRelease)deferred.Add(o);else DestroyNow(o);}
 static void DestroyNow(GameObject o){o.destroyed=true;if(o.rb!=null)o.rb.destroyed=true;}
 void QueuePortWaves(){}void ObservePortContact(int i,Vector3 n){}void ObservePortBreak(BreakPanel p,bool w,bool s){}void RecoverRotorFromBreakR12(string i,bool s){}void SetPanelContactsCalmR4(BreakPanel p,bool b){}
 bool SecondaryAllowed(BreakPanel p,CityDebris d)=>true;float PanelRetention(BreakPanel p)=>.9f;float PanelImpactWeight(BreakPanel p)=>1;
 void SpawnCity2Shards(BreakPanel p,Vector3 v,float m,float e,string r,int d,Vector3 q,Vector3 n){spawnCount++;}
 void SpawnLifeReactionR6(BreakPanel p,Vector3 q,Vector3 n,Vector3 v,float b){}void SpawnCityShards(BreakPanel p,Vector3 q){}void ReleaseBreakRotor(string i,float w){}void ApplyPortRoles(){}
 static int passed;
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL "+name);passed++;Console.WriteLine("PASS "+name);}
 GameObject AddGeneric(string id){var o=new GameObject{name=id};fragments.Add(o);return o;}
 Rigidbody AddCity(string id){var o=new GameObject{name=id};var rb=o.AddComponent<Rigidbody>();rb.mass=1;var d=new CityDebris{obj=o,body=rb};d.colliderIds.Add(o.collider.id);debrisByCollider[o.collider.id]=d;cityDebris.Add(d);fragments.Add(o);CityFragments++;return rb;}
 List<Rigidbody> Batch(int count,bool baseline=false){var bodies=new List<Rigidbody>();for(int i=0;i<count;i++){
  if(baseline)RecycleCityFragmentBaseline();else RecycleCityFragment(UseObjectRedesign?bodies:null);
  if(fragments.Count<48&&CityFragments<20)bodies.Add(AddCity("new-"+i));
 }return bodies;}
 string Inventory()=>string.Join(",",fragments.Select(o=>o.name))+"|"+CityFragments+"|"+debrisByCollider.Count;
 static RigidCrabFlight Pool(int generic,int city,bool defer=false){var w=new RigidCrabFlight{deferRelease=defer};for(int i=0;i<generic;i++)w.AddGeneric("generic-"+i);for(int i=0;i<city;i++)w.AddCity("city-"+i);return w;}
 static void Baseline(){var w=Pool(48,0);var b=w.Batch(6,true);bool threw=false;try{w.ScatterShardsR6(b,Vector3.zero,Vector3.up,Vector3.right*10,100,"baseline");}catch(MissingReferenceException){threw=true;}
  Check(threw&&b.Count==6&&b.Count(rb=>!rb)==5,"75cb1a1 reproduces destroyed COM in actual scatter: 48 ordinary fragments, ONE unique target, six spawn attempts, five recycled before scatter");}
 static void Recycling(){
  foreach(bool defer in new[]{false,true})foreach(int total in new[]{0,19,20,42,43,47,48})foreach(int city in new[]{0,1,6,19,20}){if(city>total)continue;
   var w=Pool(total-city,city,defer);var b=w.Batch(6);Check(b.Count==6&&b.All(rb=>rb&&rb.gameObject.active)&&!b.Any(rb=>w.deferred.Contains(rb.gameObject)),"batch survives recycler total="+total+" city="+city+" deferred="+defer);
   w.ScatterShardsR6(b,Vector3.zero,Vector3.up,Vector3.right*10,100,"fixed");
   Check(w.fragments.Count<=48&&w.CityFragments<=20&&w.scatterReceiptsR6.Single().physical==6&&w.scatterReceiptsR6.Single().total<=100.001,"bounded count and scatter energy/receipt");
   var previous=b;var next=w.Batch(6);Check(next.All(rb=>rb&&rb.gameObject.active),"next unique target batch survives");
   foreach(var o in w.deferred)DestroyNow(o);Check(next.All(rb=>rb),"deferred destroy cannot invalidate current batch");
  }
  for(int total=0;total<=48;total++)for(int city=0;city<=Math.Min(20,total);city++){
   var a=Pool(total-city,city);var b=Pool(total-city,city);a.UseObjectRedesign=false;a.Batch(6);b.Batch(6,true);
   if(a.Inventory()!=b.Inventory())throw new Exception("legacy FIFO changed "+total+"/"+city);
  }Check(true,"legacy recycler matches original for all 819 total/city pool combinations and six spawns");
 }
 BreakPanel Panel(){var p=new BreakPanel{id="same-target"};breakByCollider[1]=p;return p;}
 void Counts(int expected,string label){Check(spawnCount==expected&&DestructionEvent==expected&&GlassBroken==expected&&BreakEvents.Count==expected&&BrokenObjects.Count==expected&&ObjectRun.MaxChain==expected,label);}
 static void Queues(){
  foreach(bool contactFirst in new[]{false,true}){var w=new RigidCrabFlight();var p=w.Panel();
   if(contactFirst)w.QueueBreak(1,12,Vector3.one,Vector3.up);w.QueueObjectBreak(p,true);if(!contactFirst)w.QueueBreak(1,12,Vector3.one,Vector3.up);
   w.QueueObjectBreak(p,true);w.QueueObjectBreak(p,false);
   Check(w.pendingBreaks.Count==1&&w.primaryPending.Contains(p),"blast/contact overlap has one owned queue entry order="+contactFirst);
   w.ApplyBreaks();w.Counts(1,"actual ApplyBreaks: one spawn, material count, total, event, id and maxChain");
   w.QueueObjectBreak(p,true);w.QueueBreak(1,12,Vector3.one,Vector3.up);Check(w.pendingBreaks.Count==0&&w.objectBreakPending.Count==0&&w.objectBlastPending.Count==0,"consumed target rejected at both enqueue paths");w.ApplyBreaks();w.Counts(1,"repeated consume has no double count");
   p.broken=false;w.ObjectRun.Reset();w.spawnCount=w.DestructionEvent=w.GlassBroken=0;w.BreakEvents.Clear();w.BrokenObjects.Clear();w.QueueObjectBreak(p,true);w.ApplyBreaks();w.Counts(1,"next throw reset allows exactly one new break");
  }
  {var w=new RigidCrabFlight();var p=w.Panel();w.QueueObjectBreak(p,false);float impulse=w.breakImpulses[p];w.Body.mass=9;w.QueueObjectBreak(p,true);Check(w.objectBlastPending.Contains(p)&&w.breakImpulses[p]==impulse&&w.pendingBreaks.Count==1,"band then blast promotes metadata without overwriting or requeue");}
  {var w=new RigidCrabFlight();var p=w.Panel();w.secondarySources[p]=new CityDebris{body=new Rigidbody{destroyed=true}};w.QueueObjectBreak(p,true);Check(!w.secondarySources.ContainsKey(p)&&w.primaryPending.Contains(p),"blast replaces stale secondary source and claims primary ownership");w.ApplyBreaks();w.Counts(1,"stale debris cannot suppress owned object break");}
  {var w=new RigidCrabFlight();var p=w.Panel();p.objectSuppressed=true;w.QueueObjectBreak(p,true);w.QueueObjectBreak(null,true);Check(w.pendingBreaks.Count==0&&w.objectBlastPending.Count==0,"suppressed and null targets skipped");p.objectSuppressed=false;w.UseObjectRedesign=false;w.QueueObjectBreak(p,true);Check(w.pendingBreaks.Count==0,"legacy bypasses object enqueue");w.QueueBreak(1,12,Vector3.one,Vector3.up);w.ApplyBreaks();w.Counts(1,"legacy contact still breaks and counts once");}
 }
 public static int Main(){Baseline();Recycling();Queues();Console.WriteLine(passed+" passed / 0 failed");return 0;}
}
