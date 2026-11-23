using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using C2=CrabCityDensityR2;

public static class BlenderPanelsReview {
 [Serializable] public class Target {public string id,asset,physics,appearance;public Vector3 position,scale;public int blenderParts;}
 [Serializable] public class Targets {public Target[] targets;}
 [Serializable] public class Receipt {public string revision="blender-panels-1",error,inputSha256;public bool enabled,pass;public int seed=42043,ticks,breaks,overridden;public FlightState state;public bool retryReady;}
 [Serializable] public class Imported {public string family;public Vector3 min,max;public int vertices,triangles,parts;public float frontFeatureMeanZ;}
 [Serializable] public class Imports {public Imported[] models;}
 static string Root=>Path.GetFullPath("../../../..");
 static string Hash(byte[] bytes){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
 static string F(Vector3 x)=>JsonUtility.ToJson(x);
 public static void Run(){
  var dir=Environment.GetEnvironmentVariable("BLENDER_PANELS_OUTPUT");if(string.IsNullOrEmpty(dir)||Directory.Exists(dir))throw new Exception("Use a fresh BLENDER_PANELS_OUTPUT directory");
  Directory.CreateDirectory(dir);var receipt=new Receipt{enabled=true};CrabFlightGame game=null;
  try{
   BlenderFamilyOverride.Enabled=receipt.enabled;
   if(receipt.enabled)BlenderPilotImport.Prepare();
   EditorSceneManager.OpenScene("Assets/FlightField.unity",OpenSceneMode.Single);
   game=UnityEngine.Object.FindFirstObjectByType<CrabFlightGame>();
   if(!game)throw new Exception("Saved game scene missing");
   UnityEngine.Random.InitState(42043);game.Initialize();game.enabled=false;
   var targets=NeonDensityReview.Targets(game);
   var rows=targets.Select(v=>new Target{id=v.placementId,asset=v.assetId,position=v.transform.position,scale=v.transform.localScale,
    physics=F(v.transform.position)+F(v.transform.localScale)+JsonUtility.ToJson(v.transform.rotation)+JsonUtility.ToJson(City2Data.Prop(v.assetId))+F(v.suggestedEnvelopeCenter)+F(v.suggestedEnvelopeSize)+"|"+v.suggestedTotalMassKg+"|"+string.Join(";",v.GetComponents<BoxCollider>().Select(c=>F(c.center)+F(c.size)+c.enabled+c.isTrigger)),
    appearance=Appearance(v),blenderParts=v.intact.GetComponentsInChildren<MeshFilter>(true).Count(f=>f.name.StartsWith("Blender "))}).OrderBy(t=>t.id).ToArray();
   receipt.overridden=rows.Count(t=>t.blenderParts>0);
   File.WriteAllText(dir+"/targets.json",JsonUtility.ToJson(new Targets{targets=rows},true));
   if(receipt.overridden!=rows.Length || rows.Any(t=>t.blenderParts!=1))throw new Exception("A City target is not modelled exactly once");
   PanelInventory(game,dir,"panels.json");PanelImports(dir);
   var world=NeonDensityReview.Read(game);
   if(world.targets.Any(t=>!t.registered))throw new Exception("Unregistered City inventory target");
   File.WriteAllText(dir+"/world.json",JsonUtility.ToJson(world,true));
   if(receipt.enabled){
    var imports=new List<Imported>();
    foreach(var family in rows.Where(t=>t.blenderParts>0).Select(t=>t.asset).Distinct().OrderBy(t=>t)){
     var model=Resources.Load<GameObject>("BlenderModels/"+family);var fs=model.GetComponentsInChildren<MeshFilter>(true);var b=fs[0].sharedMesh.bounds;foreach(var f in fs.Skip(1))b.Encapsulate(f.sharedMesh.bounds);
     imports.Add(new Imported{family=family,min=b.min,max=b.max,vertices=fs.Sum(f=>f.sharedMesh.vertexCount),triangles=fs.Sum(f=>f.sharedMesh.triangles.Length/3),parts=fs.Length,frontFeatureMeanZ=FrontFeatureZ(family,fs.Single(f=>f.name=="Intact").sharedMesh)});
    }
    File.WriteAllText(dir+"/imports.json",JsonUtility.ToJson(new Imports{models=imports.ToArray()},true));
   }
   var camera=game.ReviewCamera;game.RefreshReviewCamera();Capture(camera,dir+"/aim.png",1280,800);CapturePortrait(game,dir+"/aim-portrait.png");
   foreach(float x in new[]{43f,79f,118f}){
    camera.orthographicSize=11;camera.transform.position=new Vector3(x,10,-30);camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));Capture(camera,dir+"/mall-overview-"+x+".png",1280,800);
   }
   CaptureSectors(game,dir);
   CapturePanels(game,dir);
   game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));var input=File.ReadAllText(Root+"/accepted-input.json");receipt.inputSha256=Hash(File.ReadAllBytes(Root+"/accepted-input.json"));
   UnityEngine.Random.InitState(42043);game.ReviewReplayAcceptedCalmR3(input);
   var marks=new[]{32f,57f,82f,105f,123f,240f,790f,1100f,1370f,2160f,2800f};int mark=0;var route=new List<string>{"tick,time,x,y,speed,breaks"};
   for(int k=0;k<18000&&!game.Finished;k++){
    game.Tick(1f/60);receipt.ticks++;var s=game.State();
    if(k%30==0)route.Add(k+","+s.time+","+s.worldX+","+s.height+","+s.speed+","+s.destructionEvent);
    if(mark<marks.Length&&s.worldX>=marks[mark]){Capture(camera,dir+"/flight-"+marks[mark]+".png",1280,800);CapturePortrait(game,dir+"/flight-"+marks[mark]+"-portrait.png");File.WriteAllText(dir+"/flight-"+marks[mark]+".json",JsonUtility.ToJson(s,true));mark++;}
   }
   SaveFragments(game,dir);
   receipt.state=game.State();receipt.breaks=game.ReviewSimulation.BreakEvents.Count;
   File.WriteAllLines(dir+"/route.csv",route);File.WriteAllLines(dir+"/break-events.jsonl",game.ReviewSimulation.BreakEvents.Select(e=>JsonUtility.ToJson(e)));
   Capture(camera,dir+"/result.png",1280,800);
   receipt.pass=game.Finished&&receipt.state.phase=="result"&&string.IsNullOrEmpty(receipt.state.physicsFault)&&receipt.state.distance>=2500&&receipt.breaks>=96&&mark==marks.Length;
   game.Prepare("");receipt.retryReady=game.State().phase=="ready";Capture(camera,dir+"/retry.png",1280,800);receipt.pass&=receipt.retryReady;PanelInventory(game,dir,"panels-retry.json");
   if(!receipt.pass)throw new Exception("Replay gate failed: "+receipt.state.distance+"m / "+receipt.breaks);
  }catch(Exception e){receipt.error=e.ToString();throw;}
  finally{File.WriteAllText(dir+"/receipt.json",JsonUtility.ToJson(receipt,true));if(game)game.ShutdownReviewPhysics();}
 }

 [Serializable] public class View {public string sector,direction;public Vector3 camera;public float size;public int visible;public string[] families;}
 static void CaptureSectors(CrabFlightGame game,string dir){
  var camera=game.ReviewCamera;var rows=NeonDensityReview.Targets(game);
  var views=new List<View>();
  foreach(var s in WorldRouteR11.Data.sectors)foreach(int side in new[]{1,-1}){
   var local=rows.Where(v=>WorldRouteR11.RegionAt(v.transform.position.x)==s.id && (v.transform.position.x>=0)==(side>0)).ToArray();
   string family=s.id=="country"?"hay_roll_neon6":s.id=="port"?"cargo_container_neon6":s.id=="viaduct"?"traffic_signal_neon6":s.id=="launchrange"?"dish_antenna_neon6":s.id=="orbit"?"satellite_neon6":s.id=="office"?"delivery_van":"";
   var candidates=local.Where(v=>v.assetId==family).ToArray();
   var chosen=(candidates.Length>0?candidates:local).OrderBy(v=>Mathf.Abs(v.transform.position.x-side*s.viewX)).FirstOrDefault();
   float x=chosen?chosen.transform.position.x:side*s.viewX;
   if(s.id=="mall")x=side*79;
   if(s.id=="restaurant")x=side*16;
   if(s.id=="tank")x=0;
   float y=chosen?Mathf.Max(7,chosen.transform.position.y+4):s.viewY;
   float size=s.id=="tank"?7:s.id=="restaurant"?10:s.id=="mall"?12:14;
   // Frame the district with its floor/context, including the old elevated
   // negative restaurant placements. This changes only the evidence camera.
   if(s.id=="tank")y=9;
   else if(s.id=="restaurant"&&side<0){y=18;size=15;}
   else if(s.id=="mall")y=11;
   else if(s.id=="launchrange"||s.id=="orbit"){y=21;size=22;}
   else if(s.id!="restaurant"){y=11;size=10.5f;}
   camera.orthographicSize=size;camera.transform.position=new Vector3(x,y,-38);camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.14f,1));camera.aspect=1.6f;
   string direction=side>0?"positive":"negative";
   Capture(camera,dir+"/sector-"+s.id+"-"+direction+".png",1280,800);
   var planes=GeometryUtility.CalculateFrustumPlanes(camera);
   var visible=local.Where(v=>v.intact.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&GeometryUtility.TestPlanesAABB(planes,r.bounds))).ToArray();
   views.Add(new View{sector=s.id,direction=direction,camera=camera.transform.position,size=size,visible=visible.Length,families=visible.Select(v=>v.assetId).Distinct().OrderBy(x2=>x2).ToArray()});
   if(local.Length>0&&visible.Length==0)throw new Exception("No City model visible in sector shot "+s.id+direction);
  }
  File.WriteAllLines(dir+"/sector-views.jsonl",views.Select(v=>JsonUtility.ToJson(v)));
 }
 static float FrontFeatureZ(string family,Mesh mesh){
  if(family!="vending_unit")return 0;
  var colors=mesh.colors;var vertices=mesh.vertices;
  var front=Enumerable.Range(0,vertices.Length).Where(i=>colors[i].r<.015f&&colors[i].g>.02f&&colors[i].g<.06f&&colors[i].b>.04f&&colors[i].b<.08f).Select(i=>vertices[i].z).ToArray();
  if(front.Length==0||front.Average()>=0)throw new Exception("FBX front is reversed: vending display must face -Z");
  return front.Average();
 }
 static string Appearance(C2.CityVisualBreakable v){
  var text=string.Join("|",v.GetComponentsInChildren<MeshFilter>(true).Select(f=>{
   var m=f.sharedMesh;var r=f.GetComponent<Renderer>();return f.name+F(f.transform.localPosition)+F(f.transform.localScale)+JsonUtility.ToJson(f.transform.localRotation)+m.name+F(m.bounds.center)+F(m.bounds.size)+m.vertexCount+"/"+m.subMeshCount+"/"+r.enabled+"/"+string.Join(";",r.sharedMaterials.Select(x=>x?x.name+"/"+x.shader.name+"/"+(x.HasProperty("_Color")?x.color.ToString():""):"null"));
  }));return Hash(System.Text.Encoding.UTF8.GetBytes(text));
 }
 static void CapturePortrait(CrabFlightGame game,string path){
  var camera=game.ReviewCamera;var position=camera.transform.position;var rotation=camera.transform.rotation;var size=camera.orthographicSize;var aspect=camera.aspect;
  var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
  var fields=new[]{"cameraOffsetR29","cameraFollowingR29","steadyFlightSpeedR3"}.Select(n=>typeof(CrabFlightGame).GetField(n,flags)).ToArray();
  var values=fields.Select(f=>f.GetValue(game)).ToArray();
  try{camera.aspect=390f/844;game.RefreshReviewCamera();Capture(camera,path,390,844);}
  finally{for(int i=0;i<fields.Length;i++)fields[i].SetValue(game,values[i]);camera.transform.SetPositionAndRotation(position,rotation);camera.orthographicSize=size;camera.aspect=aspect;}
 }
 [Serializable] public class PanelRow {public string id,type,system,physics;public Vector3 position,size,scale;public Color color;public float visualBottom;public int blenderRenderers,legacyRenderers;public string[] renderers;}
 [Serializable] public class PanelRows {public PanelRow[] targets;}
 static object Field(object obj,string name)=>obj.GetType().GetField(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).GetValue(obj);
 static PanelRow[] PanelInventory(CrabFlightGame game,string dir,string filename){
  // Prepare clears simulation after retry; the rebuilt world still owns the panels.
  var sim=game.ReviewSimulation??(RigidCrabFlight)Field(game,"world");
  if(sim==null)throw new InvalidOperationException("PanelInventory ("+filename+"): both CrabFlightGame.ReviewSimulation and world are null; no panel inventory is available.");
  var items=(System.Collections.IEnumerable)Field(sim,"panels");var rows=new List<PanelRow>();
  foreach(var item in items){
   string id=(string)Field(item,"id"),type=(string)Field(item,"type");var go=(GameObject)Field(item,"visual");
   bool city2=(UnityEngine.Object)Field(item,"city2"),city=(UnityEngine.Object)Field(item,"city");bool life=Field(item,"life")!=null;
   var rs=go.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled).ToArray();
   var shape=(IEnumerable<BoxCollider>)Field(item,"shapes");
   var physics=id+"|"+type+"|"+F((Vector3)Field(item,"position"))+F((Vector3)Field(item,"size"))+Field(item,"threshold")+"|"+Field(item,"retention")+"|"+F(go.transform.position)+F(go.transform.localScale)+JsonUtility.ToJson(go.transform.rotation)+"|"+string.Join(";",shape.Select(c=>F(c.center)+F(c.size)+c.enabled+c.isTrigger+c.providesContacts+"/"+c.contactOffset+"/"+(c.sharedMaterial?c.sharedMaterial.name+"/"+c.sharedMaterial.dynamicFriction+"/"+c.sharedMaterial.staticFriction+"/"+c.sharedMaterial.bounciness+"/"+c.sharedMaterial.frictionCombine+"/"+c.sharedMaterial.bounceCombine:"null")));
   rows.Add(new PanelRow{id=id,type=type,system=life?"Life":city2?"City":city?"LegacyCity":id.StartsWith("core-")?"Core":id.StartsWith("r5-")?"Overdrive":"Restaurant",physics=physics,position=go.transform.position,size=(Vector3)Field(item,"size"),scale=go.transform.localScale,color=(Color)Field(item,"color"),visualBottom=rs.Length>0?rs.Min(r=>r.bounds.min.y)-go.transform.position.y:0,blenderRenderers=rs.Count(r=>r.name.StartsWith("Blender ")),legacyRenderers=rs.Count(r=>!r.name.StartsWith("Blender ")),renderers=rs.Select(r=>r.name+"|"+r.sharedMaterial.name+"|"+r.sharedMaterial.shader.name).ToArray()});
  }
  var result=rows.OrderBy(x=>x.id).ToArray();File.WriteAllText(dir+"/"+filename,JsonUtility.ToJson(new PanelRows{targets=result},true));return result;
 }
 static void CapturePanels(CrabFlightGame game,string dir){
  var camera=game.ReviewCamera;
  foreach(int side in new[]{1,-1})foreach(float x in new[]{7f,17f,25f,112f,205f}){
   camera.orthographicSize=x==25?9:x>=112?7:6.5f;camera.transform.position=new Vector3(side*x,x==112?8:x==25?8:5.5f,-25);camera.transform.rotation=Quaternion.LookRotation(new Vector3(.20f*side,-.13f,1));
   Capture(camera,dir+"/panels-"+(side>0?"positive-":"negative-")+x+".png",1280,800);
  }
 }
 [Serializable] public class PanelImport {public string family;public Vector3 min,max;public int vertices,colours,colliders,bodies;}
 [Serializable] public class PanelImportsResult {public PanelImport[] models;}
 static void PanelImports(string dir){
  var rows=new List<PanelImport>();
  foreach(var path in AssetDatabase.FindAssets("t:Model",new[]{"Assets/BlenderModels/Resources/BlenderModels"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>Path.GetFileName(p).StartsWith("panel_"))){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);var f=prefab.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="Intact");var b=f.sharedMesh.bounds;
   rows.Add(new PanelImport{family=Path.GetFileNameWithoutExtension(path),min=b.min,max=b.max,vertices=f.sharedMesh.vertexCount,colours=f.sharedMesh.colors.Length,colliders=prefab.GetComponentsInChildren<Collider>(true).Length,bodies=prefab.GetComponentsInChildren<Rigidbody>(true).Length});
  }
  File.WriteAllText(dir+"/panel-imports.json",JsonUtility.ToJson(new PanelImportsResult{models=rows.OrderBy(r=>r.family).ToArray()},true));
 }
 [Serializable] public class Fragment {public string name;public int blenderRenderers,legacyRenderers;public string physics;}
 [Serializable] public class Fragments {public Fragment[] fragments;}
 static string ColliderText(Collider c){
  string mat=c.sharedMaterial?c.sharedMaterial.name+"/"+c.sharedMaterial.dynamicFriction+"/"+c.sharedMaterial.staticFriction+"/"+c.sharedMaterial.bounciness+"/"+c.sharedMaterial.frictionCombine+"/"+c.sharedMaterial.bounceCombine:"null";
  var b=c as BoxCollider;
  return b?F(b.center)+F(b.size)+b.enabled+b.isTrigger+b.providesContacts+"/"+b.contactOffset+"/"+mat:c.GetType().Name+"/"+F(c.bounds.center)+F(c.bounds.size)+"/"+mat;
 }
 static void SaveFragments(CrabFlightGame game,string dir){
  var rows=game.ReviewSimulation.ReviewFragments.Where(o=>o).Select(o=>{var rb=o.GetComponent<Rigidbody>();var c=o.GetComponent<Collider>();var rs=o.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled).ToArray();return new Fragment{name=o.name,blenderRenderers=rs.Count(r=>r.name.StartsWith("Blender ")),legacyRenderers=rs.Count(r=>!r.name.StartsWith("Blender ")),physics=F(o.transform.localScale)+"|"+c.GetType().Name+"|"+ColliderText(c)+"|"+rb.mass+"|"+rb.linearDamping+"|"+rb.angularDamping};}).ToArray();
  File.WriteAllText(dir+"/fragments.json",JsonUtility.ToJson(new Fragments{fragments=rows},true));
 }
 public static void Capture(Camera camera,string path,int w,int h){
  var prev=camera.targetTexture;var active=RenderTexture.active;var aspect=camera.aspect;
  var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
  try{camera.aspect=(float)w/h;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
  finally{camera.targetTexture=prev;camera.aspect=aspect;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);}
 }
}
