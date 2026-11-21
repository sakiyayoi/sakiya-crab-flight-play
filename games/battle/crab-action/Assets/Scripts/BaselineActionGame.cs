#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
public sealed class BaselineActionGame:MonoBehaviour{
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void ActionReport(string json);
#endif
 public Shader crabShader,groundShader;public bool aiEnabled=true;
 public ActionConfig config=new ActionConfig();
 sealed class Unit{public CrabModel model;public CapsuleCollider collider;public Weapon[] weapons=new Weapon[2];public float hp=100,hitClock;public bool player;public Vector3 facing=Vector3.forward;}
 sealed class Weapon{public string kind;public Transform root,tip;public float clock=-1,cooldown;public bool hit,fired;public Vector3 previous;}
 sealed class Shot{public Unit owner;public Vector3 direction;public Transform root;public float life=1.4f;}
 sealed class Spark{public Transform root;public float life=.24f;}
 Unit player,enemy;Camera cameraView;readonly Dictionary<Collider,Unit> bodies=new Dictionary<Collider,Unit>();readonly List<Shot> shots=new List<Shot>();readonly List<Spark> sparks=new List<Spark>();Vector2 movement;float accumulator,elapsed,reportClock,enemyClock;string phase="customize",message="左右のパーツを付け替えよう",eventName="ready";int eventId,hits,misses,swaps;bool initialized;
 public Camera ReviewCamera=>cameraView;
 void Start(){Initialize();}
 public void Initialize(){if(initialized)return;initialized=true;Application.targetFrameRate=45;QualitySettings.shadows=ShadowQuality.Disable;CrabModel.SurfaceShader=crabShader;CrabModel.ArenaShader=groundShader;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.55f,.65f,.7f);
  var c=new GameObject("Action camera");cameraView=c.AddComponent<Camera>();cameraView.orthographic=true;cameraView.nearClipPlane=.1f;cameraView.farClipPlane=100;cameraView.backgroundColor=new Color(.14f,.33f,.38f);cameraView.clearFlags=CameraClearFlags.SolidColor;
  var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.25f;sun.color=new Color(1,.91f,.77f);sun.transform.rotation=Quaternion.Euler(50,-25,0);
  CrabModel.Shape("Sand arena",null,PrimitiveType.Cylinder,new Vector3(0,-.22f,0),new Vector3(19,.2f,19),new Color(.64f,.59f,.43f));CrabModel.Shape("Ocean",null,PrimitiveType.Cube,new Vector3(0,-.65f,0),new Vector3(150,.5f,150),new Color(.12f,.34f,.39f));
  var edge=new GameObject("Arena edge").AddComponent<LineRenderer>();edge.useWorldSpace=true;edge.loop=true;edge.positionCount=96;edge.startWidth=edge.endWidth=.08f;edge.sharedMaterial=Material(new Color(.84f,.75f,.53f));for(int i=0;i<96;i++){float t=i*Mathf.PI*2/96;edge.SetPosition(i,new Vector3(Mathf.Cos(t)*9.15f,.008f,Mathf.Sin(t)*9.15f));}
  Rebuild();Customize("");
 }
 Material Material(Color c){var m=new Material(groundShader?groundShader:Shader.Find("Standard"));m.color=c;m.SetFloat("_Glossiness",.32f);return m;}
 Unit MakeUnit(string name,bool isPlayer){var unit=new Unit{player=isPlayer,model=CrabModel.Make(name,new CrabBuild(),Color.white)};unit.model.Root.rotation=Quaternion.identity;float minY=float.PositiveInfinity;foreach(var r in unit.model.Root.GetComponentsInChildren<MeshRenderer>())minY=Mathf.Min(minY,r.bounds.min.y);unit.model.Root.position=new Vector3(0,.025f-minY,0);unit.collider=unit.model.Root.gameObject.AddComponent<CapsuleCollider>();unit.collider.radius=.76f;unit.collider.height=1.65f;unit.collider.center=new Vector3(0,.4f,0);unit.collider.isTrigger=true;bodies[unit.collider]=unit;return unit;}
 void Rebuild(){ClearShots();bodies.Clear();if(player!=null)player.model.Dispose();if(enemy!=null)enemy.model.Dispose();player=MakeUnit("Your crab",true);enemy=MakeUnit("Opponent crab",false);player.weapons[0]=MakeWeapon(player,0,config.left);player.weapons[1]=MakeWeapon(player,1,config.right);enemy.weapons[0]=MakeWeapon(enemy,0,"sword");enemy.weapons[1]=MakeWeapon(enemy,1,"creature");enemy.model.SetRoleRing(new Color(.9f,.49f,.3f));player.model.SetRoleRing(new Color(.52f,.77f,.75f));}
 public void Configure(string json){if(phase=="battle")return;var next=JsonUtility.FromJson<ActionConfig>(json);if(next==null)return;next.Sanitize();if(next.left==config.left&&next.right==config.right)return;config=next;swaps++;Rebuild();Customize("");Event("swap","パーツを付け替えた");}
 public void Begin(string _){Initialize();ClearShots();phase="battle";elapsed=0;hits=misses=0;enemyClock=.8f;movement=Vector2.zero;player.hp=enemy.hp=100;player.hitClock=enemy.hitClock=0;ResetWeapons(player);ResetWeapons(enemy);Place(player,new Vector3(0,0,-4),Vector3.forward);Place(enemy,new Vector3(0,0,4),Vector3.back);enemy.model.Root.gameObject.SetActive(true);Event("start","移動して間合いを作ろう");UpdateCamera(1,true);}
 public void Customize(string _){phase="customize";movement=Vector2.zero;ClearShots();player.hp=enemy.hp=100;ResetWeapons(player);ResetWeapons(enemy);Place(player,Vector3.zero,Vector3.forward);enemy.model.Root.gameObject.SetActive(false);Event("ready","左右のパーツを付け替えよう");UpdateCamera(1,true);}
 void Place(Unit u,Vector3 p,Vector3 face){p.y=u.model.Root.position.y;u.model.Root.position=p;u.facing=face;u.model.Root.rotation=Quaternion.LookRotation(face);}
 public void Move(string value){if(phase!="battle"){movement=Vector2.zero;return;}var v=value.Split(',');if(v.Length==2&&float.TryParse(v[0],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float x)&&float.TryParse(v[1],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float z)&&!float.IsNaN(x)&&!float.IsNaN(z)&&!float.IsInfinity(x)&&!float.IsInfinity(z))movement=Vector2.ClampMagnitude(new Vector2(x,z),1);}
 public void AttackLeft(string _){Attack(player,0);}public void AttackRight(string _){Attack(player,1);}
 void Attack(Unit u,int side){if(phase!="battle"||u.hp<=0)return;var w=u.weapons[side];if(w.cooldown>0||w.clock>=0)return;w.clock=0;w.cooldown=w.kind=="cannon"?1.25f:w.kind=="creature"?.95f:.78f;w.hit=w.fired=false;w.previous=w.tip.position;if(u.player)Event("windup",w.kind=="cannon"?"砲口の向きへ発射":w.kind=="creature"?"生物腕を伸ばす":"剣を振りかぶる");}
 void ResetWeapons(Unit u){foreach(var w in u.weapons){w.clock=-1;w.cooldown=0;w.hit=w.fired=false;Pose(w,0);}u.model.Animate(0,-1,0,false);}
 void Update(){Tick(Mathf.Min(Time.deltaTime,.08f));}
 public void Tick(float dt){if(!initialized)return;accumulator+=dt;while(accumulator>=1f/90){accumulator-=1f/90;Step(1f/90);}UpdateCamera(dt,false);reportClock+=dt;if(reportClock>.06f){reportClock=0;Report();}}
 void Step(float dt){
  if(phase=="battle"){
   elapsed+=dt;Vector3 forward=Vector3.ProjectOnPlane(cameraView.transform.forward,Vector3.up).normalized,right=Vector3.ProjectOnPlane(cameraView.transform.right,Vector3.up).normalized;Vector3 dir=right*movement.x+forward*movement.y;if(dir.sqrMagnitude>.01f){player.facing=dir.normalized;Translate(player,dir*4.4f*dt);}
   if(aiEnabled){Vector3 delta=player.model.Root.position-enemy.model.Root.position;delta.y=0;float d=delta.magnitude;if(d>.01f)enemy.facing=delta.normalized;var tangent=new Vector3(delta.z,0,-delta.x).normalized;Vector3 step=d>2.3f?delta.normalized*2.05f:d<1.8f?-delta.normalized*1.1f:tangent*.65f;Translate(enemy,step*dt);enemyClock-=dt;if(enemyClock<=0&&d<3.6f){Attack(enemy,Mathf.Sin(elapsed*1.3f)>0?0:1);enemyClock=.8f;}}
   Vector3 between=enemy.model.Root.position-player.model.Root.position;between.y=0;if(between.magnitude<1.45f&&between.magnitude>.001f){Vector3 push=between.normalized*(1.45f-between.magnitude)*.5f;Translate(enemy,push);Translate(player,-push);}
   foreach(var u in new[]{player,enemy}){u.model.Root.rotation=Quaternion.Slerp(u.model.Root.rotation,Quaternion.LookRotation(u.facing),1-Mathf.Exp(-dt*20));u.hitClock=Mathf.Max(0,u.hitClock-dt);u.model.Animate(elapsed,-1,u.hitClock,false);if((u.player?movement.sqrMagnitude:1)>.01f)u.model.Walk(elapsed,u.player?movement.magnitude: .6f);}
   Physics.SyncTransforms();foreach(var u in new[]{player,enemy})foreach(var w in u.weapons)UpdateWeapon(u,w,dt);
   for(int i=shots.Count-1;i>=0&&phase=="battle";i--){var shot=shots[i];var start=shot.root.position;float travel=16*dt;Unit target=null;float nearest=float.PositiveInfinity;foreach(var hit in Physics.SphereCastAll(start,.20f,shot.direction,travel,~0,QueryTriggerInteraction.Collide))if(bodies.TryGetValue(hit.collider,out var body)&&body!=shot.owner&&body.hp>0&&hit.distance<nearest){target=body;nearest=hit.distance;}if(target!=null){Damage(shot.owner,target,24,start+shot.direction*nearest);UnityEngine.Object.DestroyImmediate(shot.root.gameObject);shots.RemoveAt(i);}else{shot.root.position+=shot.direction*travel;shot.life-=dt;if(shot.life<=0||shot.root.position.magnitude>25){if(shot.owner.player){misses++;Event("miss","砲弾は空振り");}UnityEngine.Object.DestroyImmediate(shot.root.gameObject);shots.RemoveAt(i);}}}
   if(phase!="battle")ClearShots();
  }else foreach(var u in new[]{player,enemy})if(u.model.Root.gameObject.activeSelf)u.model.Animate(elapsed,-1,0,phase=="lose"&&u.player||phase=="win"&&!u.player);
  for(int i=sparks.Count-1;i>=0;i--){sparks[i].life-=dt;if(sparks[i].life<=0){UnityEngine.Object.DestroyImmediate(sparks[i].root.gameObject);sparks.RemoveAt(i);}else sparks[i].root.localScale=Vector3.one*(.3f+( .24f-sparks[i].life)*3);}
 }
 void Translate(Unit u,Vector3 step){var p=u.model.Root.position+step;Vector2 flat=Vector2.ClampMagnitude(new Vector2(p.x,p.z),8.3f);u.model.Root.position=new Vector3(flat.x,p.y,flat.y);}
 void UpdateWeapon(Unit owner,Weapon w,float dt){w.cooldown=Mathf.Max(0,w.cooldown-dt);if(w.clock<0)return;w.clock+=dt;Pose(w,w.clock);Physics.SyncTransforms();bool active=false;
  if(w.kind=="cannon"){if(!w.fired&&w.clock>=.18f){w.fired=true;var shot=CrabModel.Shape("Fictional energy ball",null,PrimitiveType.Sphere,w.tip.position,Vector3.one*.35f,new Color(1,.72f,.32f)).transform;shots.Add(new Shot{owner=owner,root=shot,direction=w.root.forward});if(owner.player)Event("shot","発射！");}}
  else{active=w.clock>=.17f&&w.clock<=.43f;bool creature=w.kind=="creature";if(active&&!w.hit){foreach(var collider in Physics.OverlapCapsule(w.root.position,w.tip.position,creature?.28f:.17f,~0,QueryTriggerInteraction.Collide))if(bodies.TryGetValue(collider,out var target)&&target!=owner&&target.hp>0){w.hit=true;Damage(owner,target,creature?19:28,w.tip.position);break;}if(!w.hit)foreach(var collider in Physics.OverlapCapsule(w.previous,w.tip.position,creature?.28f:.17f,~0,QueryTriggerInteraction.Collide))if(bodies.TryGetValue(collider,out var target)&&target!=owner&&target.hp>0){w.hit=true;Damage(owner,target,creature?19:28,w.tip.position);break;}}if(!w.fired&&active){w.fired=true;if(owner.player)Event(creature?"creature":"swish",creature?"腕が伸びる！":"剣が通る！");}}
  w.previous=w.tip.position;if(w.clock>.65f){if(w.kind!="cannon"&&!w.hit&&owner.player){misses++;Event("miss","空振り。間合いを詰めよう");}w.clock=-1;Pose(w,0);}
 }
 void Damage(Unit attacker,Unit target,float damage,Vector3 contact){if(phase!="battle"||target.hp<=0)return;target.hp=Mathf.Max(0,target.hp-damage);target.hitClock=.24f;Vector3 push=target.model.Root.position-attacker.model.Root.position;push.y=0;Translate(target,push.normalized*.36f);var spark=CrabModel.Shape("Contact reaction",null,PrimitiveType.Sphere,contact,Vector3.one*.3f,new Color(1,.82f,.49f)).transform;sparks.Add(new Spark{root=spark});if(attacker.player)hits++;Event("hit",attacker.player?"命中！":"攻撃を受けた");if(target.hp<=0){phase=target.player?"lose":"win";movement=Vector2.zero;Event(phase,target.player?"倒された。組み替えて再戦しよう":"勝ち！ この組合せ、どうだった？");}}
 void ClearShots(){foreach(var shot in shots)if(shot.root)UnityEngine.Object.DestroyImmediate(shot.root.gameObject);shots.Clear();}
 void Event(string name,string text){eventName=name;message=text;eventId++;Report();}
 void Report(){
#if UNITY_WEBGL && !UNITY_EDITOR
 ActionReport(JsonUtility.ToJson(State()));
#endif
 }
 public ActionState State(){return new ActionState{phase=phase,left=config.left,right=config.right,hp=player.hp,enemyHp=enemy.hp,leftCooldown=player.weapons[0].cooldown,rightCooldown=player.weapons[1].cooldown,x=player.model.Root.position.x,z=player.model.Root.position.z,enemyX=enemy.model.Root.position.x,enemyZ=enemy.model.Root.position.z,time=elapsed,eventId=eventId,eventName=eventName,message=message,hits=hits,misses=misses,swaps=swaps};}
 void UpdateCamera(float dt,bool snap){bool custom=phase=="customize";Vector3 center=custom?player.model.Root.position:(player.model.Root.position+enemy.model.Root.position)*.5f;center.y=.4f;float size=custom?3.5f:Mathf.Max(6.6f,Vector3.Distance(player.model.Root.position,enemy.model.Root.position)*.52f+2.8f);size*=Mathf.Max(1,.78f/Mathf.Max(cameraView.aspect,.3f));cameraView.orthographicSize=snap?size:Mathf.Lerp(cameraView.orthographicSize,size,1-Mathf.Exp(-dt*6));Vector3 p=center+new Vector3(8,12,-14);cameraView.transform.position=snap?p:Vector3.Lerp(cameraView.transform.position,p,1-Mathf.Exp(-dt*6));cameraView.transform.LookAt(center);}
 Weapon MakeWeapon(Unit u,int side,string kind){var root=u.model.ReplaceArm(side);var w=new Weapon{kind=kind,root=root};Color metal=new Color(.36f,.43f,.46f),dark=new Color(.14f,.21f,.23f);
  void Shape(string n,PrimitiveType t,Vector3 p,Vector3 scale,Color c){CrabModel.Shape(n,root,t,p,scale,c);}
  Shape("Joint socket",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.36f,dark);
  if(kind=="sword"){Shape("Hilt",PrimitiveType.Cylinder,new Vector3(0,0,.32f),new Vector3(.13f,.32f,.13f),dark);root.GetChild(root.childCount-1).localRotation=Quaternion.Euler(90,0,0);Shape("Crossguard",PrimitiveType.Cube,new Vector3(0,0,.62f),new Vector3(.75f,.14f,.16f),new Color(.53f,.43f,.27f));var blade=new GameObject("Authored sword blade");blade.transform.SetParent(root,false);var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.18f,0,.7f),new Vector3(.18f,0,.7f),new Vector3(0,.10f,.7f),new Vector3(0,-.10f,.7f),new Vector3(-.14f,0,1.85f),new Vector3(.14f,0,1.85f),new Vector3(0,.08f,1.85f),new Vector3(0,-.08f,1.85f),new Vector3(0,0,2.2f)};mesh.triangles=new[]{0,4,2,2,4,6,2,6,1,1,6,5,1,5,3,3,5,7,3,7,0,0,7,4,4,8,6,6,8,5,5,8,7,7,8,4,0,2,3,2,1,3};mesh.RecalculateNormals();blade.AddComponent<MeshFilter>().sharedMesh=mesh;blade.AddComponent<MeshRenderer>().sharedMaterial=Material(new Color(.69f,.73f,.72f));}
  if(kind=="cannon"){Shape("Fictional launcher body",PrimitiveType.Sphere,new Vector3(0,0,.48f),new Vector3(.66f,.52f,1.05f),metal);Shape("Launcher barrel",PrimitiveType.Cylinder,new Vector3(0,0,1.15f),new Vector3(.39f,.61f,.39f),dark);root.GetChild(root.childCount-1).localRotation=Quaternion.Euler(90,0,0);Shape("Muzzle rim",PrimitiveType.Cylinder,new Vector3(0,0,1.78f),new Vector3(.5f,.09f,.5f),metal);root.GetChild(root.childCount-1).localRotation=Quaternion.Euler(90,0,0);Shape("Muzzle opening",PrimitiveType.Sphere,new Vector3(0,0,1.88f),new Vector3(.31f,.31f,.04f),new Color(.08f,.15f,.16f));}
  if(kind=="creature"){var skin=new Color(.30f,.42f,.28f);for(int i=0;i<5;i++){float t=i/4f;Shape("Creature arm segment",PrimitiveType.Sphere,new Vector3(Mathf.Sin(t*2)*.17f,Mathf.Sin(t*3)*.14f,.25f+t*1.65f),new Vector3(.43f-t*.12f,.38f-t*.08f,.65f),skin);}Shape("Creature mouth",PrimitiveType.Sphere,new Vector3(.15f,0,2.05f),new Vector3(.56f,.48f,.64f),new Color(.40f,.45f,.28f));for(int i=0;i<4;i++)Shape("Small fantasy teeth",PrimitiveType.Cube,new Vector3(.15f+(i%2==0?-.16f:.16f),i<2?.17f:-.17f,2.30f),new Vector3(.09f,.12f,.16f),new Color(.87f,.81f,.62f));}
  w.tip=new GameObject("Visible weapon tip").transform;w.tip.SetParent(root,false);w.tip.localPosition=new Vector3(kind=="creature"?.15f:0,0,kind=="cannon"?1.9f:kind=="creature"?2.32f:2.2f);Pose(w,0);return w;
 }
 void Pose(Weapon w,float t){float swing=0,extension=1;if(t>0){if(w.kind=="sword")swing=t<.17f?Mathf.Lerp(0,-65,t/.17f):t<.43f?Mathf.Lerp(-65,65,(t-.17f)/.26f):Mathf.Lerp(65,0,(t-.43f)/.22f);if(w.kind=="creature")extension=1+Mathf.Sin(Mathf.Clamp01(t/.55f)*Mathf.PI)*.55f;}
  w.root.localRotation=Quaternion.Euler(0,swing,0);w.root.localScale=new Vector3(1,1,extension);
 }
#if UNITY_EDITOR
 public void ReviewPlace(Vector3 p,Vector3 e){Place(player,p,Vector3.forward);Place(enemy,e,Vector3.back);Physics.SyncTransforms();}
#endif
}

#endif
