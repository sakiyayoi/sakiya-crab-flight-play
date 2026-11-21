using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
[Serializable]public class CrabBuild{public float claw=45,shell=45,legs=45;}
[Serializable]public class ActionConfig{public string left="sword",right="cannon";public void Sanitize(){left=Valid(left);right=Valid(right);}static string Valid(string s){return s=="sword"||s=="cannon"||s=="creature"?s:"sword";}}
[Serializable]public class ActionState{public string revision="crab-action-r1-telegraph1",phase,left,right,message,eventName,enemyIntent;public bool runtimeReady,enemyOpen,paused;public int punishHits,guardHits,enemyAttacks;public float playerStagger,enemyStagger,hp,enemyHp,leftCooldown,rightCooldown,x,z,enemyX,enemyZ,time;public int eventId,hits,misses,swaps;}
public sealed class CrabActionGame:MonoBehaviour{
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void ActionReport(string json);
#endif
#if UNITY_EDITOR
 public Action<string,Vector3,Vector3,float> ReviewObserver;
 public Action<string,Vector3,Vector3,float> ReviewForecastObserver;
 public int ReviewVisibleForecastCount(){int n=0;foreach(var unit in new[]{player,enemy})if(unit!=null)foreach(var weapon in unit.weapons)if(weapon!=null&&weapon.forecast!=null&&weapon.forecast.Visible)n++;return n;}
#endif
 [System.Diagnostics.Conditional("UNITY_EDITOR")]
 void Observe(string stage,Vector3 a,Vector3 b,float radius){
#if UNITY_EDITOR
  ReviewObserver?.Invoke(stage,a,b,radius);
#endif
 }
 [System.Diagnostics.Conditional("UNITY_EDITOR")]
 void ObserveForecast(string stage,Vector3 a,Vector3 b,float radius){
#if UNITY_EDITOR
  ReviewForecastObserver?.Invoke(stage,a,b,radius);
#endif
 }
 public Shader crabShader,groundShader;public bool aiEnabled=true;
 public ActionConfig config=new ActionConfig();
 sealed class Unit{public CrabModel model;public CapsuleCollider collider;public Weapon[] weapons=new Weapon[2];public float hp=100,hitClock,stagger,staggerMax;public bool player;public Vector3 facing=Vector3.forward;}
 sealed class Weapon{public string kind;public Transform root,tip;public float clock=-1,cooldown,chargeCue;public bool hit,fired,hostile;public Vector3 previous,rest,restScale;public Quaternion restRotation;public LineRenderer trail;public Renderer chargeRenderer;public MaterialPropertyBlock chargeBlock;public readonly List<Vector3> trace=new List<Vector3>();public readonly AttackGeometry.Volume[] forecastSamples=new AttackGeometry.Volume[13];public AttackForecast forecast;}
 sealed class Shot{public Unit owner;public Weapon weapon;public Vector3 direction;public Transform root;public float life=AttackGeometry.ShotLifetime;}
 sealed class Spark{public Transform root;public Vector3 velocity;public float life=.24f;}
 Unit player,enemy;Camera cameraView;readonly Dictionary<Collider,Unit> bodies=new Dictionary<Collider,Unit>();readonly List<Shot> shots=new List<Shot>();readonly List<Spark> sparks=new List<Spark>();Vector2 movement;float combatMinimumSize=5.2f;float accumulator,elapsed,reportClock,enemyClock,freezeClock,shakeClock;string phase="customize",message="左右のパーツを付け替えよう",eventName="ready";int eventId,hits,misses,swaps;bool initialized,reducedMotion,paused,showForecast=true;bool enemyOpen,enemyGuard;int enemyAttacks,punishHits,guardHits;Vector3 committedFacing;string enemyIntent="構え";
 public void Comfort(string value){reducedMotion=value=="1";if(reducedMotion)shakeClock=0;}
 public void ForecastVisibility(string value){showForecast=value!="0";if(!showForecast)HideForecasts();}
 readonly Dictionary<Color,Material> materialPool=new Dictionary<Color,Material>();Material playerForecastMaterial,enemyForecastMaterial;
 public Camera ReviewCamera=>cameraView;
 public struct ProbeSample{public int phase,lk,rk,lf,rf,shots,sparks,eventId;public float lc,rc,lp,rp,lcue,rcue;}
 public ProbeSample ReadProbe(){var a=player.weapons[0];var b=player.weapons[1];return new ProbeSample{phase=phase=="battle"?1:phase=="win"?2:phase=="lose"?3:0,lk=ProbeKind(a.kind),rk=ProbeKind(b.kind),lc=a.clock,rc=b.clock,lp=ProbePose(a),rp=ProbePose(b),lf=a.fired?1:0,rf=b.fired?1:0,lcue=a.chargeCue,rcue=b.chargeCue,shots=shots.Count,sparks=sparks.Count,eventId=eventId};}
 static int ProbeKind(string k)=>k=="cannon"?1:k=="creature"?2:0;
 static float ProbePose(Weapon w)=>w.kind=="sword"?Quaternion.Angle(w.root.localRotation,w.restRotation):w.kind=="creature"?Mathf.Abs(w.root.localScale.z/w.restScale.z-1):Vector3.Distance(w.root.localPosition,w.rest);

 void OnDestroy(){foreach(var m in materialPool.Values)if(m)Release(m);materialPool.Clear();if(playerForecastMaterial)Release(playerForecastMaterial);if(enemyForecastMaterial)Release(enemyForecastMaterial);}
 static void Release(UnityEngine.Object o){
#if UNITY_EDITOR
 if(!Application.isPlaying){UnityEngine.Object.DestroyImmediate(o);return;}
#endif
 UnityEngine.Object.Destroy(o);
 }
 void Start(){Initialize();}
 public void Initialize(){if(initialized)return;initialized=true;Application.targetFrameRate=45;QualitySettings.shadows=ShadowQuality.Disable;CrabModel.SurfaceShader=crabShader;CrabModel.ArenaShader=groundShader;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.55f,.65f,.7f);
  var c=new GameObject("Action camera");cameraView=c.AddComponent<Camera>();cameraView.orthographic=true;cameraView.nearClipPlane=.1f;cameraView.farClipPlane=100;cameraView.backgroundColor=new Color(.14f,.33f,.38f);cameraView.clearFlags=CameraClearFlags.SolidColor;
  var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.25f;sun.color=new Color(1,.91f,.77f);sun.transform.rotation=Quaternion.Euler(50,-25,0);
  CrabModel.Shape("Sand arena",null,PrimitiveType.Cylinder,new Vector3(0,-.22f,0),new Vector3(19,.2f,19),new Color(.64f,.59f,.43f));CrabModel.Shape("Ocean",null,PrimitiveType.Cube,new Vector3(0,-.65f,0),new Vector3(150,.5f,150),new Color(.12f,.34f,.39f));
  var edge=new GameObject("Arena edge").AddComponent<LineRenderer>();edge.useWorldSpace=true;edge.loop=true;edge.positionCount=96;edge.startWidth=edge.endWidth=.08f;edge.sharedMaterial=Material(new Color(.84f,.75f,.53f));for(int i=0;i<96;i++){float t=i*Mathf.PI*2/96;edge.SetPosition(i,new Vector3(Mathf.Cos(t)*9.15f,.008f,Mathf.Sin(t)*9.15f));}
  Rebuild();Customize("");
 }
 Material Material(Color c){if(materialPool.TryGetValue(c,out var cached)&&cached)return cached;var m=new Material(groundShader?groundShader:Shader.Find("Standard"));m.color=c;m.SetFloat("_Glossiness",.32f);materialPool[c]=m;return m;}
 Material ForecastMaterial(bool hostile){
  var cached=hostile?enemyForecastMaterial:playerForecastMaterial;if(cached)return cached;
  // Authored fixed transparent pass: no runtime Standard keyword variant can be stripped.
  var shader=Resources.Load<Shader>("CrabForecast");if(!shader)throw new InvalidOperationException("Original forecast shader missing");
  var material=new Material(shader);material.name=hostile?"Enemy predicted reach":"Player predicted reach";
  material.SetColor("_Color",hostile?new Color(.94f,.10f,.025f,.43f):new Color(.015f,.65f,.94f,.35f));
  if(hostile)enemyForecastMaterial=material;else playerForecastMaterial=material;return material;
 }

 Unit MakeUnit(string name,bool isPlayer){var unit=new Unit{player=isPlayer,model=CrabModel.Make(name,new CrabBuild(),Color.white)};unit.model.Root.rotation=Quaternion.identity;float minY=float.PositiveInfinity;foreach(var r in unit.model.Root.GetComponentsInChildren<MeshRenderer>())minY=Mathf.Min(minY,r.bounds.min.y);unit.model.Root.position=new Vector3(0,.025f-minY,0);unit.collider=unit.model.Root.gameObject.AddComponent<CapsuleCollider>();unit.collider.radius=.76f;unit.collider.height=1.65f;unit.collider.center=new Vector3(0,.4f,0);unit.collider.isTrigger=true;bodies[unit.collider]=unit;return unit;}
 void Rebuild(){ClearShots();bodies.Clear();if(player!=null)player.model.Dispose();if(enemy!=null)enemy.model.Dispose();player=MakeUnit("Your crab",true);enemy=MakeUnit("Opponent crab",false);player.weapons[0]=MakeWeapon(player,0,config.left);player.weapons[1]=MakeWeapon(player,1,config.right);enemy.weapons[0]=MakeWeapon(enemy,0,"sword");enemy.weapons[1]=MakeWeapon(enemy,1,"creature");enemy.model.SetRoleRing(new Color(.9f,.49f,.3f));player.model.SetRoleRing(new Color(.52f,.77f,.75f));}
 public void Configure(string json){Initialize();if(phase=="battle")return;var next=JsonUtility.FromJson<ActionConfig>(json);if(next==null)return;next.Sanitize();if(next.left==config.left&&next.right==config.right)return;config=next;swaps++;Rebuild();Customize("");Event("swap","パーツを付け替えた");}
 public void Begin(string _){Initialize();paused=false;ClearShots();phase="battle";elapsed=0;freezeClock=shakeClock=0;hits=misses=punishHits=guardHits=enemyAttacks=0;enemyOpen=false;enemyGuard=true;enemyIntent="構え";enemyClock=.85f;movement=Vector2.zero;player.hp=enemy.hp=100;player.hitClock=enemy.hitClock=player.stagger=enemy.stagger=0;ResetWeapons(player);ResetWeapons(enemy);Place(player,new Vector3(0,0,-3),Vector3.forward);Place(enemy,new Vector3(0,0,3),Vector3.back);enemy.model.Root.gameObject.SetActive(true);Event("start","移動して間合いを作ろう");UpdateCamera(1,true);}
 public void Customize(string _){paused=false;phase="customize";enemyOpen=enemyGuard=false;HideForecasts();freezeClock=shakeClock=0;movement=Vector2.zero;ClearShots();player.hp=enemy.hp=100;ResetWeapons(player);ResetWeapons(enemy);Place(player,Vector3.zero,Vector3.forward);enemy.model.Root.gameObject.SetActive(false);Event("ready","左右のパーツを付け替えよう");UpdateCamera(1,true);}
 void Place(Unit u,Vector3 p,Vector3 face){p.y=u.model.Root.position.y;u.model.Root.position=p;u.facing=face;u.model.Root.rotation=Quaternion.LookRotation(face);}
 public void Move(string value){if(phase!="battle"||paused){movement=Vector2.zero;return;}var v=value.Split(',');if(v.Length==2&&float.TryParse(v[0],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float x)&&float.TryParse(v[1],System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float z)&&!float.IsNaN(x)&&!float.IsNaN(z)&&!float.IsInfinity(x)&&!float.IsInfinity(z))movement=Vector2.ClampMagnitude(new Vector2(x,z),1);}
 public void AttackLeft(string _){Attack(player,0);}public void AttackRight(string _){Attack(player,1);}
 void Attack(Unit u,int side){if(phase!="battle"||paused||u.hp<=0||u.stagger>0)return;var w=u.weapons[side];if(w.cooldown>0||w.clock>=0)return;w.clock=0;w.cooldown=w.kind=="cannon"?1.25f:w.kind=="creature"?.95f:.78f;w.hit=w.fired=false;w.trace.Clear();w.previous=w.tip.position;if(!u.player)Event("warning","相手が振りかぶった。距離を取ろう");if(u.player)Event("windup",w.kind=="cannon"?"砲口の向きへ発射":w.kind=="creature"?"生物腕を伸ばす":"剣を振りかぶる");}
 void ResetWeapons(Unit u){foreach(var w in u.weapons){w.clock=-1;w.cooldown=0;w.hit=w.fired=false;w.trace.Clear();w.trail.enabled=false;w.forecast?.Hide();Pose(w,0);}u.model.Animate(0,-1,0,false);}
 void Update(){Tick(Mathf.Min(Time.deltaTime,.08f));}
 public void Pause(string value){paused=phase=="battle"&&value=="1";movement=Vector2.zero;if(paused)HideForecasts();Report();}
 public void Tick(float dt){if(!initialized)return;if(paused){Report();return;}accumulator+=dt;while(accumulator>=1f/90){accumulator-=1f/90;Step(1f/90);}UpdateCamera(dt,false);reportClock+=dt;if(reportClock>.06f){reportClock=0;Report();}}
 void Step(float dt){
  if(freezeClock>0&&phase=="battle"){freezeClock=Mathf.Max(0,freezeClock-dt);return;}
  if(phase=="battle"){
   elapsed+=dt;player.stagger=Mathf.Max(0,player.stagger-dt);enemy.stagger=Mathf.Max(0,enemy.stagger-dt);Vector3 forward=Vector3.ProjectOnPlane(cameraView.transform.forward,Vector3.up).normalized,right=Vector3.ProjectOnPlane(cameraView.transform.right,Vector3.up).normalized;Vector3 dir=right*movement.x+forward*movement.y;if(player.stagger<=0&&dir.sqrMagnitude>.01f){player.facing=dir.normalized;Translate(player,dir*4.4f*dt);}
   if(aiEnabled)EnemyExchange(dt);else{enemyOpen=enemyGuard=false;}
   Vector3 between=enemy.model.Root.position-player.model.Root.position;between.y=0;if(between.magnitude<1.45f&&between.magnitude>.001f){Vector3 push=between.normalized*(1.45f-between.magnitude)*.5f;Translate(enemy,push);Translate(player,-push);}
   foreach(var u in new[]{player,enemy}){u.model.Root.rotation=Quaternion.Slerp(u.model.Root.rotation,Quaternion.LookRotation(u.facing),1-Mathf.Exp(-dt*20));u.hitClock=Mathf.Max(0,u.hitClock-dt);u.model.Animate(elapsed,-1,u.hitClock,false);if(u.stagger>0)u.model.Body.localRotation*=Quaternion.Euler(0,0,(u.player?-16:12)*u.stagger/Mathf.Max(.01f,u.staggerMax));if((u.player?movement.sqrMagnitude:1)>.01f)u.model.Walk(elapsed,u.player?movement.magnitude: .6f);}
   Physics.SyncTransforms();foreach(var u in new[]{player,enemy})foreach(var w in u.weapons)UpdateWeapon(u,w,dt);
   for(int i=shots.Count-1;i>=0&&phase=="battle";i--){var shot=shots[i];var start=shot.root.position;float travel=AttackGeometry.ShotSpeed*dt;const float shotRadius=AttackGeometry.ShotRadius;Observe("cannon:travel",start,start+shot.direction*travel,shotRadius);Unit target=null;float nearest=float.PositiveInfinity;foreach(var overlap in Physics.OverlapSphere(start,shotRadius,~0,QueryTriggerInteraction.Collide))if(bodies.TryGetValue(overlap,out var initial)&&initial!=shot.owner&&initial.hp>0){float distance=Vector3.Distance(initial.collider.ClosestPoint(start),start);if(distance<nearest){target=initial;nearest=0;}}if(target==null)foreach(var hit in Physics.SphereCastAll(start,shotRadius,shot.direction,travel,~0,QueryTriggerInteraction.Collide))if(bodies.TryGetValue(hit.collider,out var body)&&body!=shot.owner&&body.hp>0&&hit.distance<nearest){target=body;nearest=hit.distance;}if(target!=null){Damage(shot.owner,target,24,start+shot.direction*nearest);
#if UNITY_EDITOR
Release(shot.root.gameObject);
#else
Destroy(shot.root.gameObject);
#endif
shots.RemoveAt(i);}else{shot.root.position+=shot.direction*travel;shot.life-=dt;if(shot.life<=0||shot.root.position.magnitude>25){if(shot.owner.player){misses++;Event("miss","砲弾は空振り");}
#if UNITY_EDITOR
Release(shot.root.gameObject);
#else
Destroy(shot.root.gameObject);
#endif
shots.RemoveAt(i);}}}
   if(phase!="battle"){ClearShots();HideForecasts();}else UpdateForecasts();
  }else foreach(var u in new[]{player,enemy})if(u.model.Root.gameObject.activeSelf)u.model.Animate(elapsed,-1,0,phase=="lose"&&u.player||phase=="win"&&!u.player);
  for(int i=sparks.Count-1;i>=0;i--){sparks[i].life-=dt;if(sparks[i].life<=0){
#if UNITY_EDITOR
Release(sparks[i].root.gameObject);
#else
Destroy(sparks[i].root.gameObject);
#endif
sparks.RemoveAt(i);}else{sparks[i].root.position+=sparks[i].velocity*dt;sparks[i].root.localScale=Vector3.one*Mathf.Max(.025f,sparks[i].life*.55f);}}
 }

 // One engagement loop teaches a readable committed sweep, tests it again,
 // then changes only reach with the existing creature arm. No HP inflation.
 void EnemyExchange(float dt){
  if(enemy.stagger>0){enemyOpen=true;enemyGuard=false;enemyIntent="よろけ · 攻撃の隙";return;}
  Vector3 delta=player.model.Root.position-enemy.model.Root.position;delta.y=0;float distance=delta.magnitude;
  Weapon active=enemy.weapons[0].clock>=0?enemy.weapons[0]:enemy.weapons[1].clock>=0?enemy.weapons[1]:null;
  enemyOpen=active!=null&&active.clock>=.43f;enemyGuard=!enemyOpen;
  if(active==null){
   enemyIntent="構え · 振り終わりを狙う";enemyClock-=dt;if(distance>.01f)enemy.facing=delta.normalized;
   Vector3 tangent=new Vector3(delta.z,0,-delta.x).normalized;
   Vector3 step=distance>3.15f?delta.normalized*2.5f:distance<2.65f?-delta.normalized*.9f:tangent*.28f;
   Translate(enemy,step*dt);foreach(var w in enemy.weapons)Pose(w,0);
   if(enemyClock<=0&&distance<3.45f){committedFacing=delta.sqrMagnitude>.001f?delta.normalized:enemy.facing;enemy.facing=committedFacing;int side=enemyAttacks%3==2?1:0;enemyAttacks++;Attack(enemy,side);active=enemy.weapons[side];enemyClock=.55f;}
  }
  if(active!=null){
   enemy.facing=committedFacing;
   if(active.clock<.17f)enemyIntent=active.kind=="creature"?"伸びる腕 · 横へ逃げる":"剣の踏み込み · 横へ逃げる";
   else if(active.clock<.43f){enemyIntent="踏み込み！";Translate(enemy,committedFacing*4.2f*dt);}
   else enemyIntent="隙！ 振り終わりに攻撃";
  }
 }
 void HideForecasts(){foreach(var unit in new[]{player,enemy})if(unit!=null)foreach(var weapon in unit.weapons)if(weapon!=null)weapon.forecast?.Hide();}
 static float ForecastSeconds(Weapon w,float targetClock){
  if(targetClock<=w.clock)return 0;if(!w.hostile)return targetClock-w.clock;
  float seconds=0;if(w.clock<.17f)seconds+=(Mathf.Min(targetClock,.17f)-w.clock)/.36f;
  if(targetClock>.17f)seconds+=(targetClock-Mathf.Max(w.clock,.17f))/.90f;
  return seconds;
 }
 Matrix4x4 ForecastMatrix(Unit unit,Weapon weapon,float targetClock){
  float seconds=ForecastSeconds(weapon,targetClock);Vector3 position=unit.model.Root.position;
  if(unit.player){Vector3 forward=Vector3.ProjectOnPlane(cameraView.transform.forward,Vector3.up).normalized,right=Vector3.ProjectOnPlane(cameraView.transform.right,Vector3.up).normalized;Vector3 travel=right*movement.x+forward*movement.y;if(unit.stagger<=0&&travel.sqrMagnitude>.01f)position+=travel*4.4f*seconds;}
  else if(aiEnabled&&unit.stagger<=0&&weapon.clock>=0&&targetClock>.17f)position+=committedFacing.normalized*4.2f*((targetClock-Mathf.Max(weapon.clock,.17f))/.90f);
  Vector2 flat=Vector2.ClampMagnitude(new Vector2(position.x,position.z),8.3f);position=new Vector3(flat.x,position.y,flat.y);
  Vector3 facing=unit.player?unit.facing:committedFacing;if(facing.sqrMagnitude<.001f)facing=unit.model.Root.forward;
  Quaternion rootRotation=Quaternion.Slerp(unit.model.Root.rotation,Quaternion.LookRotation(facing),1-Mathf.Exp(-seconds*20));
  Matrix4x4 relative=unit.model.Root.worldToLocalMatrix*weapon.root.parent.localToWorldMatrix;
  Matrix4x4 body=Matrix4x4.TRS(position,rootRotation,unit.model.Root.localScale)*relative;
  return AttackGeometry.MatrixAt(body,weapon.kind,targetClock,weapon.rest,weapon.restRotation,weapon.restScale,weapon.hostile,enemyGuard);
 }
 static float ArenaRayRange(AttackGeometry.Ray ray){
  float a=ray.direction.x*ray.direction.x+ray.direction.z*ray.direction.z;if(a<.0001f)return ray.range;
  float b=2*(ray.muzzle.x*ray.direction.x+ray.muzzle.z*ray.direction.z),c=ray.muzzle.x*ray.muzzle.x+ray.muzzle.z*ray.muzzle.z-9.15f*9.15f;
  float discriminant=b*b-4*a*c;if(discriminant<0)return ray.range;float edge=(-b+Mathf.Sqrt(discriminant))/(2*a);return Mathf.Min(ray.range,Mathf.Max(.1f,edge));
 }
 void UpdateForecasts(){
  if(!showForecast||paused||phase!="battle"){HideForecasts();return;}
  foreach(var unit in new[]{player,enemy})foreach(var weapon in unit.weapons){
   var visual=weapon.forecast;if(visual==null)continue;
   if(weapon.kind=="cannon"){
    Shot live=null;foreach(var shot in shots)if(shot.weapon==weapon){live=shot;break;}
    if(live!=null){var ray=new AttackGeometry.Ray(live.root.position,live.direction,AttackGeometry.ShotRadius,AttackGeometry.ShotSpeed*live.life);float range=ArenaRayRange(ray);visual.Cannon(ray,range);ObserveForecast((unit.player?"player":"enemy")+":cannon",ray.muzzle,ray.muzzle+ray.direction*range,ray.radius);}
    else if(weapon.clock>=0&&!weapon.fired&&weapon.clock<.18f){var ray=AttackGeometry.Cannon(ForecastMatrix(unit,weapon,.18f),weapon.tip.localPosition);float range=ArenaRayRange(ray);visual.Cannon(ray,range);ObserveForecast((unit.player?"player":"enemy")+":cannon",ray.muzzle,ray.muzzle+ray.direction*range,ray.radius);}
    else visual.Hide();
   }else if(weapon.clock>=0&&weapon.clock<.43f&&!weapon.hit&&unit.stagger<=0){
    var samples=weapon.forecastSamples;float start=Mathf.Max(.17f,weapon.clock);
    for(int i=0;i<samples.Length;i++){float clock=Mathf.Lerp(start,.43f,i/(float)(samples.Length-1));samples[i]=AttackGeometry.Contact(weapon.kind,ForecastMatrix(unit,weapon,clock),weapon.tip.localPosition);}
    float previousClock=Mathf.Max(0,.17f-(weapon.hostile?.36f:1f)/90f);Vector3 leadingTip=weapon.clock>=.17f?weapon.previous:AttackGeometry.Contact(weapon.kind,ForecastMatrix(unit,weapon,previousClock),weapon.tip.localPosition).tip;
    visual.Melee(samples,samples.Length,leadingTip);string label=(unit.player?"player":"enemy")+":"+weapon.kind;
    ObserveForecast(label+":tip",leadingTip,samples[0].tip,samples[0].radius);
    for(int i=0;i<samples.Length;i++){ObserveForecast(label+":shaft",samples[i].shaftStart,samples[i].tip,samples[i].radius);if(i>0)ObserveForecast(label+":tip",samples[i-1].tip,samples[i].tip,samples[i].radius);}
   }else visual.Hide();
  }
 }
 void Translate(Unit u,Vector3 step){var p=u.model.Root.position+step;Vector2 flat=Vector2.ClampMagnitude(new Vector2(p.x,p.z),8.3f);u.model.Root.position=new Vector3(flat.x,p.y,flat.y);}
 void UpdateWeapon(Unit owner,Weapon w,float dt){if(phase!="battle")return;w.cooldown=Mathf.Max(0,w.cooldown-dt);if(w.clock<0)return;w.clock+=dt*(owner.player?1:w.clock<.17f?.36f:w.clock<.43f?.90f:.34f);Pose(w,w.clock);Physics.SyncTransforms();bool active=false;
  if(w.kind=="cannon"){if(!w.fired&&w.clock>=.18f){w.fired=true;var ray=AttackGeometry.Cannon(w.root.localToWorldMatrix,w.tip.localPosition);var shot=CrabModel.Shape("Fictional energy ball",null,PrimitiveType.Sphere,ray.muzzle,Vector3.one*.35f,new Color(1,.72f,.32f)).transform;shots.Add(new Shot{owner=owner,weapon=w,root=shot,direction=ray.direction});Burst(ray.muzzle,new Color(1,.72f,.32f),3);if(owner.player)Event("shot","発射！");}}
  else{active=w.clock>=.17f&&w.clock<=.43f;bool creature=w.kind=="creature";if(!w.fired&&active){w.fired=true;if(owner.player)Event(creature?"creature":"swish",creature?"腕が伸びる！":"剣が通る！");}if(active&&!w.hit){var volume=AttackGeometry.Contact(w.kind,w.root.localToWorldMatrix,w.tip.localPosition);Observe(w.kind+":shaft",volume.shaftStart,volume.tip,volume.radius);foreach(var collider in Physics.OverlapCapsule(volume.shaftStart,volume.tip,volume.radius,~0,QueryTriggerInteraction.Collide))if(bodies.TryGetValue(collider,out var target)&&target!=owner&&target.hp>0){w.hit=true;Damage(owner,target,creature?19:28,target.collider.ClosestPoint(volume.tip));break;}if(!w.hit){Observe(w.kind+":tip-sweep",w.previous,volume.tip,volume.radius);}if(!w.hit)foreach(var collider in Physics.OverlapCapsule(w.previous,volume.tip,volume.radius,~0,QueryTriggerInteraction.Collide))if(bodies.TryGetValue(collider,out var target)&&target!=owner&&target.hp>0){w.hit=true;Damage(owner,target,creature?19:28,target.collider.ClosestPoint(volume.tip));break;}}}
  if(phase!="battle"){w.trail.enabled=false;return;}w.trail.enabled=w.kind!="cannon"&&w.clock>.14f&&w.clock<.48f;if(w.trail.enabled){w.trace.Add(w.tip.position);if(w.trace.Count>8)w.trace.RemoveAt(0);w.trail.positionCount=w.trace.Count;w.trail.SetPositions(w.trace.ToArray());}
  w.previous=w.tip.position;if(w.clock>.65f){if(w.kind!="cannon"&&!w.hit&&owner.player){misses++;Event("miss","空振り。間合いを詰めよう");}w.clock=-1;Pose(w,0);}
 }
 void Damage(Unit attacker,Unit target,float damage,Vector3 contact){if(phase!="battle"||target.hp<=0)return;bool frontGuard=!target.player&&aiEnabled&&enemyGuard&&Vector3.Dot(target.facing,(attacker.model.Root.position-target.model.Root.position).normalized)>.25f;bool punish=!target.player&&aiEnabled&&enemyOpen;if(frontGuard){damage*=.22f;guardHits++;}else if(punish)punishHits++;Observe("hit",contact,contact,damage);target.hp=Mathf.Max(0,target.hp-damage);target.hitClock=frontGuard?.08f:.24f;if(!frontGuard){target.stagger=target.staggerMax=target.player?.55f:.16f;foreach(var w in target.weapons){w.clock=-1;w.fired=true;w.trail.enabled=false;Pose(w,0);}if(!target.player)enemyClock=.35f;}Vector3 push=target.model.Root.position-attacker.model.Root.position;push.y=0;Translate(target,push.normalized*(frontGuard?.10f:.36f));freezeClock=frontGuard?.012f:.04f;shakeClock=reducedMotion?0:.11f;Burst(contact,frontGuard?new Color(.63f,.77f,.75f):attacker.player?new Color(1,.84f,.47f):new Color(1,.48f,.33f),frontGuard?3:6);if(attacker.player)hits++;Event(attacker.player?(frontGuard?"guard":punish?"punish":"hit"):"hurt",attacker.player?(frontGuard?"構えで受けられた。振り終わりが隙":punish?"隙に命中！":"命中！"):"攻撃を受けた");if(target.hp<=0){phase=target.player?"lose":"win";movement=Vector2.zero;foreach(var unit in new[]{player,enemy})foreach(var weapon in unit.weapons)weapon.trail.enabled=false;Event(phase,target.player?"倒された。組み替えて再戦しよう":"勝ち！ この組合せ、どうだった？");}}
 void Burst(Vector3 at,Color color,int count){for(int i=0;i<count;i++){float a=i*2.39996f;var root=CrabModel.Shape("Contact spark",null,PrimitiveType.Cube,at,Vector3.one*.13f,color).transform;sparks.Add(new Spark{root=root,velocity=reducedMotion?Vector3.zero:new Vector3(Mathf.Sin(a),.4f+(i%3)*.2f,Mathf.Cos(a))*2.5f});}}
 LineRenderer MakeTrail(Transform parent,Color color){var l=new GameObject("Visible attack path").AddComponent<LineRenderer>();l.transform.SetParent(parent,false);l.useWorldSpace=true;l.startWidth=.08f;l.endWidth=.015f;l.numCornerVertices=2;l.sharedMaterial=Material(color);l.enabled=false;return l;}
 void ClearShots(){foreach(var shot in shots)if(shot.root)
#if UNITY_EDITOR
Release(shot.root.gameObject);
#else
Destroy(shot.root.gameObject);
#endif
shots.Clear();}
 void Event(string name,string text){eventName=name;message=text;eventId++;Report();}
 void Report(){
#if UNITY_WEBGL && !UNITY_EDITOR
 ActionReport(JsonUtility.ToJson(State()));
#endif
 }
 public ActionState State(){return new ActionState{runtimeReady=UnityEngine.Rendering.SplashScreen.isFinished,phase=phase,paused=paused,left=config.left,right=config.right,enemyIntent=enemyIntent,enemyOpen=enemyOpen,punishHits=punishHits,guardHits=guardHits,enemyAttacks=enemyAttacks,hp=player.hp,enemyHp=enemy.hp,playerStagger=player.stagger,enemyStagger=enemy.stagger,leftCooldown=Mathf.Max(player.stagger,player.weapons[0].cooldown),rightCooldown=Mathf.Max(player.stagger,player.weapons[1].cooldown),x=player.model.Root.position.x,z=player.model.Root.position.z,enemyX=enemy.model.Root.position.x,enemyZ=enemy.model.Root.position.z,time=elapsed,eventId=eventId,eventName=eventName,message=message,hits=hits,misses=misses,swaps=swaps};}
 void UpdateCamera(float dt,bool snap){bool custom=phase=="customize";Vector3 center=custom?player.model.Root.position:(player.model.Root.position+enemy.model.Root.position)*.5f;center.y=.4f;float size=custom?3.5f:Mathf.Max(combatMinimumSize,Vector3.Distance(player.model.Root.position,enemy.model.Root.position)*.52f+2.8f);size*=Mathf.Max(1,.78f/Mathf.Max(cameraView.aspect,.3f));cameraView.orthographicSize=snap?size:Mathf.Lerp(cameraView.orthographicSize,size,1-Mathf.Exp(-dt*6));Vector3 p=center+new Vector3(8,12,-14);cameraView.transform.position=snap?p:Vector3.Lerp(cameraView.transform.position,p,1-Mathf.Exp(-dt*6));cameraView.transform.LookAt(center);if(shakeClock>0&&!reducedMotion&&!snap){shakeClock=Mathf.Max(0,shakeClock-dt);cameraView.transform.position+=cameraView.transform.right*(Mathf.Sin(Time.unscaledTime*95)*shakeClock*.35f);}}
 Weapon MakeWeapon(Unit u,int side,string kind){var root=u.model.ReplaceArm(side);var w=new Weapon{kind=kind,hostile=!u.player,root=root,rest=root.localPosition,restRotation=root.localRotation,restScale=root.localScale};Color metal=new Color(.36f,.43f,.46f),dark=new Color(.14f,.21f,.23f);
  void Shape(string n,PrimitiveType t,Vector3 p,Vector3 scale,Color c){CrabModel.Shape(n,root,t,p,scale,c);}
  Shape("Joint socket",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.36f,dark);
  if(kind=="sword"){Shape("Hilt",PrimitiveType.Cylinder,new Vector3(0,0,.32f),new Vector3(.13f,.32f,.13f),dark);root.GetChild(root.childCount-1).localRotation=Quaternion.Euler(90,0,0);Shape("Crossguard",PrimitiveType.Cube,new Vector3(0,0,.62f),new Vector3(.75f,.14f,.16f),new Color(.53f,.43f,.27f));var blade=new GameObject("Authored sword blade");blade.transform.SetParent(root,false);var mesh=new Mesh();u.model.Own(mesh);mesh.vertices=new[]{new Vector3(-.18f,0,.7f),new Vector3(.18f,0,.7f),new Vector3(0,.10f,.7f),new Vector3(0,-.10f,.7f),new Vector3(-.14f,0,1.85f),new Vector3(.14f,0,1.85f),new Vector3(0,.08f,1.85f),new Vector3(0,-.08f,1.85f),new Vector3(0,0,2.2f)};mesh.triangles=new[]{0,4,2,2,4,6,2,6,1,1,6,5,1,5,3,3,5,7,3,7,0,0,7,4,4,8,6,6,8,5,5,8,7,7,8,4,0,2,3,2,1,3};mesh.RecalculateNormals();blade.AddComponent<MeshFilter>().sharedMesh=mesh;blade.AddComponent<MeshRenderer>().sharedMaterial=Material(new Color(.69f,.73f,.72f));}
  if(kind=="cannon"){Shape("Fictional launcher body",PrimitiveType.Sphere,new Vector3(0,0,.48f),new Vector3(.66f,.52f,1.05f),metal);Shape("Launcher barrel",PrimitiveType.Cylinder,new Vector3(0,0,1.15f),new Vector3(.39f,.61f,.39f),dark);root.GetChild(root.childCount-1).localRotation=Quaternion.Euler(90,0,0);Shape("Muzzle rim",PrimitiveType.Cylinder,new Vector3(0,0,1.78f),new Vector3(.5f,.09f,.5f),metal);root.GetChild(root.childCount-1).localRotation=Quaternion.Euler(90,0,0);Shape("Muzzle opening",PrimitiveType.Sphere,new Vector3(0,0,1.88f),new Vector3(.31f,.31f,.04f),new Color(.08f,.15f,.16f));w.chargeRenderer=root.Find("Muzzle rim").GetComponent<Renderer>();w.chargeBlock=new MaterialPropertyBlock();}
  if(kind=="creature"){var skin=new Color(.30f,.42f,.28f);for(int i=0;i<5;i++){float t=i/4f;Shape("Creature arm segment",PrimitiveType.Sphere,new Vector3(Mathf.Sin(t*2)*.17f,Mathf.Sin(t*3)*.14f,.25f+t*1.65f),new Vector3(.43f-t*.12f,.38f-t*.08f,.65f),skin);}Shape("Creature mouth",PrimitiveType.Sphere,new Vector3(.15f,0,2.05f),new Vector3(.56f,.48f,.64f),new Color(.40f,.45f,.28f));for(int i=0;i<4;i++)Shape("Small fantasy teeth",PrimitiveType.Cube,new Vector3(.15f+(i%2==0?-.16f:.16f),i<2?.17f:-.17f,2.30f),new Vector3(.09f,.12f,.16f),new Color(.87f,.81f,.62f));}
  w.trail=MakeTrail(root,u.player?new Color(.96f,.9f,.63f):new Color(1,.47f,.29f));w.tip=new GameObject("Visible weapon tip").transform;w.tip.SetParent(root,false);w.tip.localPosition=new Vector3(kind=="creature"?.15f:0,0,kind=="cannon"?1.9f:kind=="creature"?2.32f:2.2f);w.forecast=new AttackForecast(u.model.Root,ForecastMaterial(!u.player),!u.player);Pose(w,0);return w;
 }
 void Pose(Weapon w,float t){var pose=AttackGeometry.PoseAt(w.kind,t,w.rest,w.restRotation,w.restScale,w.hostile,enemyGuard);
  w.root.localPosition=pose.position;w.root.localRotation=pose.rotation;w.root.localScale=pose.scale;
  if(w.kind=="cannon"&&w.chargeRenderer){
   // Only the existing visible muzzle rim changes colour; firing, aim and contact clocks stay intact.
   float cue=t<=0?0:t<=.18f?.50f+.50f*Mathf.SmoothStep(0,1,t/.18f):t<.42f?1-Mathf.SmoothStep(0,1,(t-.18f)/.24f):0;
   if(reducedMotion)cue*=.65f;
   if(cue>0){w.chargeBlock.SetColor("_Color",Color.Lerp(new Color(.36f,.43f,.46f),new Color(.92f,.82f,.45f),cue));w.chargeRenderer.SetPropertyBlock(w.chargeBlock);}
   else if(w.chargeCue>0)w.chargeRenderer.SetPropertyBlock(null);
   w.chargeCue=cue;
  }
 }
#if UNITY_EDITOR
 public void ReviewRefreshCamera(){UpdateCamera(1,true);}
 public void ReviewPlace(Vector3 p,Vector3 e){Place(player,p,Vector3.forward);Place(enemy,e,Vector3.back);Physics.SyncTransforms();}
#endif
}
