using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using C2 = CrabCityDensityR2;

// Original renderer-only regional art. Never edits placements, contact descriptors or break rules.
// Call BuildBackground AFTER the host's DepthTone; ApplyBreakables AFTER the R9 mesh binding.
public static class SectionWorldArtR16 {
 public const string Revision="section-art-r16-3-indoor-mall";
 public static bool Enabled;
 public static int ChangedVisuals {get;private set;}
 public static int BackgroundTriangles {get;private set;}
 public static float MaximumExpansion {get;private set;}
 static readonly Color[] Palette={
  new Color(.23f,.31f,.32f),new Color(.55f,.57f,.51f),new Color(.12f,.23f,.28f),
  new Color(.48f,.32f,.19f),new Color(.67f,.47f,.23f),new Color(.22f,.36f,.27f),
  new Color(.35f,.43f,.43f),new Color(.09f,.14f,.24f),new Color(.50f,.64f,.65f),new Color(.22f,.30f,.33f),new Color(.72f,.70f,.62f),new Color(.48f,.62f,.64f),new Color(.24f,.28f,.29f),new Color(.86f,.79f,.62f)};
 // PROTO3 cute-background trial: pastel override for mall brushes only.
 static readonly Color[] MallCutePalette={ new Color(.36f,.68f,.60f),new Color(.95f,.76f,.82f),new Color(.55f,.82f,.73f), new Color(.94f,.66f,.74f),new Color(.96f,.80f,.62f),new Color(.64f,.86f,.72f), new Color(.93f,.68f,.76f),new Color(.58f,.64f,.88f),new Color(.82f,.91f,.87f),new Color(.62f,.74f,.72f),new Color(.96f,.92f,.83f),new Color(.62f,.88f,.79f),new Color(.89f,.85f,.79f),new Color(.99f,.85f,.81f)};

 public static GameObject BuildBackground(Transform parent,GameObject city,Shader shader,bool far,bool route){
  if(!Enabled)return null;
  var root=new GameObject(Revision);root.transform.SetParent(parent,false);
  // These named renderer-only layers are replaced as a unit. Life, ground and contacts stay active.
  if(city)foreach(var t in city.GetComponentsInChildren<Transform>(true))
   if(t.name=="R9 regional depth and atrium"||t.name==FarWorldArtR10.Revision||t.name=="R11 all-section graybox")t.gameObject.SetActive(false);
  if(city){var farBand=city.transform.Find("DecorativeFar");if(farBand)farBand.gameObject.SetActive(false);}
  BackgroundTriangles=0;
  if(city)ReplaceMallNearDecorations(city,root.transform);
  Near(root.transform,shader);
  foreach(float side in new[]{-1f,1f}){
   Mall(root.transform,shader,side);Office(root.transform,shader,side);
   if(far){Port(root.transform,shader,side);Viaduct(root.transform,shader,side);}
   if(route&&far&&!LateWorldArtR9.Enabled){Country(root.transform,shader,side);Range(root.transform,shader,side);Orbit(root.transform,shader,side);}
  }
  if(route&&far&&LateWorldArtR9.Enabled)LateWorldArtR9.Build(root.transform,shader);
  return root;
 }
 static void Near(Transform p,Shader shader){
  var b=new Brush("tank / wet tile and pipes",p,shader);
  b.Box(2,new Vector3(0,3.5f,8.7f),new Vector3(11,7,.18f));
  for(int y=0;y<7;y++)for(int x=0;x<8;x++)b.Box((x+y)%4==0?8:6,new Vector3(-4.8f+x*1.36f,.5f+y*.95f,8.55f),new Vector3(1.30f,.89f,.07f));
  foreach(float x in new[]{-4.8f,4.8f}){b.Rod(1,new Vector3(x,.3f,7.9f),new Vector3(x,7.2f,7.9f),.11f);b.Rod(1,new Vector3(x,7.2f,7.9f),new Vector3(x*.6f,7.2f,7.9f),.11f);}
  b.Finish(false);
  b=new Brush("restaurant / timber ribs and service alcoves",p,shader);
  foreach(float side in new[]{-1f,1f}){
   for(int i=0;i<5;i++)b.Box(3,new Vector3(side*(7.3f+i*4.0f),11.8f,7.9f),new Vector3(.22f,2.8f,2.6f));
   b.Box(3,new Vector3(side*16,10.1f,7.8f),new Vector3(16,.28f,.35f));
   b.Box(0,new Vector3(side*21.7f,5.3f,8.2f),new Vector3(4.1f,6.2f,.4f));
   for(int k=0;k<6;k++)b.Box(1,new Vector3(side*21.7f,3.1f+k*.8f,7.94f),new Vector3(3.6f,.12f,.13f));
   b.Disc(4,new Vector3(side*15.7f,7.8f,8.3f),1.1f,.12f,20);
   b.Disc(3,new Vector3(side*15.7f,7.8f,8.20f),.82f,.06f,20);
  }b.Finish(false);
 }
 static void Mall(Transform p,Shader shader,float side){
  var b=new Brush("mall / enclosed shopping atrium "+side,p,shader);
  // A real-depth indoor floor covers the former road markings only inside this district.
  b.Box(10,new Vector3(side*81.5f,.017f,-102),new Vector3(107,.05f,256));
  for(float x=30;x<=134;x+=4)b.Box(6,new Vector3(side*x,.044f,-19),new Vector3(.035f,.002f,90));
  for(float z=-228;z<=24;z+=4)b.Box(6,new Vector3(side*81.5f,.044f,z),new Vector3(107,.002f,.035f));
  b.Box(10,new Vector3(side*81.5f,12.5f,25),new Vector3(107,25,.4f));
  // Three levels of glazed shop windows, display plinths and illuminated fascia.
  foreach(float floor in new[]{0f,8,16})foreach(float x in new[]{41f,62,105,125}){
   b.Box(2,new Vector3(side*x,floor+3.25f,17.8f),new Vector3(17,5.7f,.15f));
   foreach(float dx in new[]{-8.5f,0,8.5f})b.Box(1,new Vector3(side*(x+dx),floor+3.25f,17.55f),new Vector3(.12f,6,.20f));
   b.Box(10,new Vector3(side*x,floor+6.35f,17.4f),new Vector3(18,.8f,.55f));
   b.Box(13,new Vector3(side*x,floor+5.93f,17.10f),new Vector3(17.5f,.12f,.15f));
   b.Box(1,new Vector3(side*x,floor+.4f,16.8f),new Vector3(16.5f,.8f,2));
   foreach(float dx in new[]{-5f,0,5}){
    b.Box(10,new Vector3(side*(x+dx),floor+1.0f,16.4f),new Vector3(2.2f,1.2f,1.5f));
    b.Box(dx==0?11:3,new Vector3(side*(x+dx),floor+2.0f,16.5f),new Vector3(1.15f,.8f,.7f));
   }
   b.Rod(11,new Vector3(side*(x-6),floor+1.2f,17.35f),new Vector3(side*(x-3.5f),floor+4.8f,17.35f),.045f);
  }
  // The front galleries leave a central atrium open around two escalators.
  foreach(float floor in new[]{8f,16}){
   foreach(var span in new[]{new Vector2(50,44),new Vector2(116.5f,37)}){
    b.Box(10,new Vector3(side*span.x,floor,14.4f),new Vector3(span.y,.45f,12));
    b.Box(11,new Vector3(side*span.x,floor+1.0f,8.3f),new Vector3(span.y,.95f,.10f));
    b.Box(1,new Vector3(side*span.x,floor+1.55f,8.25f),new Vector3(span.y,.08f,.14f));
    for(float dx=-span.y*.5f;dx<=span.y*.5f;dx+=4)b.Box(1,new Vector3(side*(span.x+dx),floor+.85f,8.22f),new Vector3(.07f,1.45f,.15f));
   }
   b.Box(10,new Vector3(side*84,floor,20),new Vector3(25,.45f,3));
  }
  foreach(float x in new[]{29f,72,98,134})b.Box(10,new Vector3(side*x,12.3f,12),new Vector3(.7f,24.6f,.9f));
  // Paired stair treads and continuous dark handrails make these escalators, not ramps.
  for(int flight=0;flight<2;flight++){
   float start=flight==0?73:97,end=flight==0?97:73,baseY=flight*8;
   b.Rod(1,new Vector3(side*start,baseY+.25f,10.2f),new Vector3(side*end,baseY+8.25f,10.2f),.8f);
   for(int i=0;i<24;i++){float t=(i+.5f)/24;b.Box(12,new Vector3(side*Mathf.Lerp(start,end,t),baseY+8*t+.35f,10.2f),new Vector3(1.08f,.16f,3.8f));}
   foreach(float z in new[]{8.15f,12.25f}){
    b.Rod(11,new Vector3(side*start,baseY+.75f,z),new Vector3(side*end,baseY+8.75f,z),.32f);
    b.Rod(0,new Vector3(side*start,baseY+1.4f,z),new Vector3(side*end,baseY+9.4f,z),.13f);
   }
  }
  // Visible ceiling coffers and warm linear fixtures close the room above the galleries.
  b.Box(10,new Vector3(side*101,24.6f,15),new Vector3(68,.45f,21));
  for(float x=69;x<=134;x+=8){b.Box(1,new Vector3(side*x,24.25f,15),new Vector3(.20f,.35f,21));b.Box(13,new Vector3(side*x,23.99f,14),new Vector3(.18f,.10f,15));}
  foreach(float z in new[]{5f,14,25})b.Box(1,new Vector3(side*101,24.88f,z),new Vector3(68,.12f,.22f));
  for(float x=69;x<=134;x+=8){b.Box(1,new Vector3(side*x,24.88f,15),new Vector3(.18f,.12f,21));b.Box(13,new Vector3(side*x,24.96f,14),new Vector3(.13f,.04f,15));}
  // The accepted high contact route belongs to the tall entrance atrium, with indoor gallery walls.
  b.Box(10,new Vector3(side*37,30,25),new Vector3(58,60,.3f));
  b.Box(10,new Vector3(side*37,60,15),new Vector3(59,.5f,20));
  foreach(float x in new[]{10f,25,42,60})b.Box(1,new Vector3(side*x,30,12),new Vector3(.5f,60,.65f));
  foreach(var high in new[]{new Vector2(18,27),new Vector2(34,39),new Vector2(51,52)}){
   b.Box(10,new Vector3(side*high.x,high.y,14),new Vector3(20,.35f,8));
   b.Box(2,new Vector3(side*high.x,high.y+3.5f,22),new Vector3(18,6,.12f));
   b.Box(11,new Vector3(side*high.x,high.y+1,9.7f),new Vector3(18,1,.08f));
   b.Box(13,new Vector3(side*high.x,high.y+6.6f,21.8f),new Vector3(18,.14f,.15f));
  }
  b.Finish(false);
 }
 static void Office(Transform p,Shader shader,float side){
  var b=new Brush("office / vertical fins and setbacks "+side,p,shader);
  float[] xs={158,207,269,347,432,532};float[] heights={24,39,30,50,35,55};
  for(int i=0;i<xs.Length;i++){
   float x=side*xs[i],h=heights[i],w=24+(i%3)*5;
   b.Box(0,new Vector3(x,h*.5f,23),new Vector3(w,h,8));
   b.Box(2,new Vector3(x,h*.48f,18.9f),new Vector3(w-1,h*.94f,.22f));
   for(int col=-2;col<=2;col++)b.Box(6,new Vector3(x+col*w*.19f,h*.5f,18.6f),new Vector3(.28f,h,.65f));
   for(float y=4;y<h;y+=5.5f)b.Box(0,new Vector3(x,y,18.5f),new Vector3(w,.18f,.5f));
   b.Box(6,new Vector3(x+side*w*.13f,h+2.2f,23.6f),new Vector3(w*.65f,4.4f,6.4f));
   b.Box(1,new Vector3(x,3.5f,9.6f),new Vector3(w*.75f,.3f,4));
   foreach(float dx in new[]{-.32f,.32f})b.Box(0,new Vector3(x+w*dx,1.6f,9.3f),new Vector3(.3f,3.2f,.4f));
   if(i%2==0){b.Box(0,new Vector3(x+w*.6f,1,11),new Vector3(3,2,2.4f));b.Crown(5,new Vector3(x+w*.6f,3.1f,11),new Vector3(2.5f,3,2.1f));}
  }b.Finish();
 }
 static void Port(Transform p,Shader shader,float side){
  var b=new Brush("port / sawtooth sheds and dock cranes "+side,p,shader);
  b.Box(2,new Vector3(side*751,-.07f,35),new Vector3(294,.06f,45));
  foreach(float x in new[]{641f,711f,797f,869f}){
   b.Box(6,new Vector3(side*x,3.4f,19),new Vector3(42,6.8f,12));
   for(int j=0;j<3;j++)b.Wedge(1,new Vector3(side*(x-14+j*14),8.2f,19),new Vector3(14,3,13));
   for(int j=-2;j<=2;j++)b.Box(0,new Vector3(side*(x+j*8),3.4f,12.6f),new Vector3(.25f,6.8f,.5f));
   b.Box(3,new Vector3(side*x,.6f,9.6f),new Vector3(45,1.2f,2.2f));
   for(int j=-2;j<=2;j++)b.Box(0,new Vector3(side*(x+j*9),1.7f,9.1f),new Vector3(.6f,1.2f,.8f));
  }
  foreach(float x in new[]{675f,830f}){
   b.Rod(4,new Vector3(side*x,0,30),new Vector3(side*x,34,30),.85f);
   b.Rod(4,new Vector3(side*(x-18),34,30),new Vector3(side*(x+31),36,30),.72f);
   b.Rod(0,new Vector3(side*x,24,30),new Vector3(side*(x+31),36,30),.25f);
   b.Rod(0,new Vector3(side*(x+23),35.7f,30),new Vector3(side*(x+23),11,30),.08f);
  }b.Finish();
 }
 static void Viaduct(Transform p,Shader shader,float side){
  var b=new Brush("viaduct / deep spans and bracing "+side,p,shader);
  for(int i=0;i<5;i++){
   float x=936+i*78,h=i%2==0?24:34;
   b.Box(6,new Vector3(side*x,h*.5f,18),new Vector3(3.0f,h,4.5f));
   b.Box(1,new Vector3(side*x,h,18),new Vector3(69,1.6f,8.5f));
   b.Box(0,new Vector3(side*x,h-1.2f,13.6f),new Vector3(69,1.1f,.35f));
   b.Rod(0,new Vector3(side*(x-31),h-.5f,16),new Vector3(side*x,h-12,16),.40f);
   b.Rod(0,new Vector3(side*x,h-12,16),new Vector3(side*(x+31),h-.5f,16),.40f);
   for(int j=-3;j<=3;j++)b.Box(4,new Vector3(side*(x+j*9),h+1.1f,13.7f),new Vector3(.18f,1.7f,.20f));
   b.Box(4,new Vector3(side*x,h+1.85f,13.7f),new Vector3(68,.12f,.20f));
  }b.Finish();
 }
 static void Country(Transform p,Shader shader,float side){
  var b=new Brush("country / low terraces and windbreaks "+side,p,shader);
  b.Box(3,new Vector3(side*1675,-.08f,0),new Vector3(750,.10f,24));
  for(int i=0;i<9;i++){
   float x=1370+i*82;
   b.Crown(i%2==0?5:6,new Vector3(side*x,4,53+i%3*11),new Vector3(120,14+i%3*4,26));
   if(i%3!=1){b.Box(5,new Vector3(side*x,.16f,13),new Vector3(45,.16f,8));for(int j=0;j<5;j++)b.Box(3,new Vector3(side*x,.26f,10+j*1.4f),new Vector3(44,.07f,.10f));}
  }
  foreach(float x in new[]{1430f,1710f,1990f}){
   b.Box(0,new Vector3(side*x,7.6f,19),new Vector3(.32f,15.2f,.4f));
   b.Rod(0,new Vector3(side*(x-5),15,19),new Vector3(side*(x+5),15,19),.16f);
   for(int j=0;j<3;j++)b.Disc(1,new Vector3(side*(x-3+j*3),15.4f,18.8f),.25f,.16f,8);
  }
  b.Finish();
 }
 static void Range(Transform p,Shader shader,float side){
  var b=new Brush("range / test rings and launch structures "+side,p,shader);
  b.Box(0,new Vector3(side*2365,-.06f,0),new Vector3(630,.08f,24));
  foreach(float x in new[]{2115f,2350f,2585f}){
   b.Arch(1,new Vector3(side*x,8,22),20,20,1.4f,20);
   foreach(float dx in new[]{-20f,20f}){b.Box(6,new Vector3(side*(x+dx),4,22),new Vector3(3.4f,8,5));b.Box(4,new Vector3(side*(x+dx),1,18.7f),new Vector3(3.6f,2,.3f));}
   b.Rod(4,new Vector3(side*(x+27),0,27),new Vector3(side*(x+39),41,27),1.5f);
   b.Rod(0,new Vector3(side*(x+12),33,27),new Vector3(side*(x+39),41,27),.5f);
   b.Box(6,new Vector3(side*(x-34),10,35),new Vector3(10,20,9));
   b.Wedge(1,new Vector3(side*(x-34),22,35),new Vector3(10,4,9));
   for(int j=0;j<5;j++)b.Box(4,new Vector3(side*(x-12+j*6),.01f,-3),new Vector3(2.5f,.012f,.5f));
  }b.Finish();
 }
 static void Orbit(Transform p,Shader shader,float side){
  var b=new Brush("orbit / segmented rings and array wings "+side,p,shader);
  b.Box(7,new Vector3(side*2990,-.06f,0),new Vector3(620,.08f,24));
  b.Box(7,new Vector3(side*2990,70,155),new Vector3(640,340,3));
  for(int i=0;i<28;i++){
   float x=2700+(i*73%560),y=10+(i*31%170);
   b.Disc(i%5==0?8:6,new Vector3(side*x,y,150),i%5==0?.50f:.20f,.05f,6);
  }
  foreach(float x in new[]{2755f,2950f,3150f}){
   b.Arch(6,new Vector3(side*x,2,30),35,38,1.1f,22);
   b.Box(9,new Vector3(side*x,20,24),new Vector3(5,6,5));
   foreach(float wing in new[]{-1f,1f}){
    b.Box(2,new Vector3(side*(x+wing*16),20,24),new Vector3(24,7,.4f));
    for(int col=0;col<6;col++)b.Box(9,new Vector3(side*(x+wing*(5+col*4)),20,23.7f),new Vector3(.09f,6.8f,.08f));
    b.Box(9,new Vector3(side*(x+wing*16),20,23.7f),new Vector3(24,.10f,.08f));
   }
   b.Box(6,new Vector3(side*x,7,23),new Vector3(.7f,14,.7f));
  }b.Finish();
 }

 // Decorative near batches also contain outdoor vans/stalls. Rebuild only the chunks touching
 // the mall, using their original placement transforms and atlas. Outside portions keep original UVs.
 static void ReplaceMallNearDecorations(GameObject city,Transform parent){
  string path=City2Data.UseCoreLayout?(City2Data.UseAtriumRoute?"CityDensityR2/CityPlacementAtrium":"CityDensityR2/CityPlacementCore"):"CityDensityR2/CityPlacementDense";
  var layout=JsonUtility.FromJson<C2.CityDensityPack.Layout>(Resources.Load<TextAsset>(path).text);
  var near=layout.instances.Where(x=>!x.breakable&&x.band=="near").ToArray();var chunks=new HashSet<int>(near.Where(x=>Mathf.Abs(x.p[0])>=28&&Mathf.Abs(x.p[0])<135).Select(x=>x.chunk));
  var root=new GameObject("mall / indoor decorative near replacements");root.transform.SetParent(parent,false);var owner=root.AddComponent<C2.CityDensityOwnedAssets>();
  foreach(var original in city.GetComponentsInChildren<MeshRenderer>(true)){
   if(!original.name.StartsWith("near/chunk-",StringComparison.Ordinal))continue;int chunk=int.Parse(original.name.Substring("near/chunk-".Length).Split('/')[0]);if(!chunks.Contains(chunk))continue;
   var batch=new Brush();foreach(var placement in near.Where(x=>x.chunk==chunk)){
    bool mall=Mathf.Abs(placement.p[0])>=28&&Mathf.Abs(placement.p[0])<135;
    if(mall&&(placement.asset=="road_service_module"||placement.asset=="street_lamp"))continue;
    var asset=City2Data.Asset(placement.asset);var part=new Brush();if(mall)BuildMallGroup(part,asset,-2);else foreach(var piece in asset.pieces)part.AppendOriginal(piece);
    batch.AppendBrush(part,Matrix4x4.TRS(City2Data.V(placement.p),Quaternion.Euler(0,placement.yaw,0),City2Data.V(placement.s)));
   }
   var mesh=batch.Mesh("mall indoor near chunk "+chunk);owner.meshes.Add(mesh);var go=new GameObject(mesh.name);go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=original.sharedMaterial;var depthTone=new MaterialPropertyBlock();original.GetPropertyBlock(depthTone);renderer.SetPropertyBlock(depthTone);renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;original.enabled=false;
  }
 }
 // Dominant visible surface of the indoor replacement, not its old donor asset.
 // City mass, shard shapes, thresholds and retention remain in City2 proposals.
 public static string BreakMaterial(string assetId,Vector3 position,string original){
  if(!Enabled||!(position.y>18&&Mathf.Abs(position.x)<135||WorldRouteR11.RegionAt(position.x)=="mall"))return original;
  if(assetId=="market_stall"||assetId=="facade_break_panel")return "glass";
  if(assetId=="crate_stack"||assetId=="market_crate"||assetId=="street_awning"||assetId=="delivery_van"||assetId=="hvac_unit"||assetId=="junction_cabinet")return "metal";
  return original;
 }
 public static void ApplyBreakables(GameObject root){
  if(!Enabled||!root)return;ChangedVisuals=0;
  var owner=root.GetComponent<C2.CityDensityOwnedAssets>();var cache=new Dictionary<string,Mesh>();
  foreach(var visual in root.GetComponentsInChildren<C2.CityVisualBreakable>()){
   if(visual.assetId.StartsWith("mall_r5_"))continue;string region=visual.transform.position.y>18&&Mathf.Abs(visual.transform.position.x)<135?"mall":WorldRouteR11.RegionAt(visual.transform.position.x);
   if(region!="mall"&&(!Eligible(visual.assetId)||(visual.assetId=="street_bench"&&region!="port"&&region!="country")))continue;
   var asset=City2Data.Asset(visual.assetId);
   foreach(var filter in visual.GetComponentsInChildren<MeshFilter>(true)){
    int group=-2;
    for(var t=filter.transform.parent;t&&t!=visual.transform;t=t.parent){if(t.name.StartsWith("Shard_",StringComparison.Ordinal)){group=int.Parse(t.name.Substring(6));break;}if(t==visual.fractured.transform)group=-1;}
    string key=asset.id+":"+region+":"+group;
    if(!cache.TryGetValue(key,out var mesh)){
     var b=new Brush();
     if(region=="mall")BuildMallGroup(b,asset,group);else foreach(var piece in asset.pieces){if(group!=-2&&piece.shard!=group)continue;BuildPiece(b,asset.id,piece,region,group==-2);}
     mesh=b.Mesh("R16 "+key);var bounds=filter.sharedMesh.bounds;
     var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++){var v=vertices[i];vertices[i]=new Vector3(Mathf.Clamp(v.x,bounds.min.x,bounds.max.x),Mathf.Clamp(v.y,bounds.min.y,bounds.max.y),Mathf.Clamp(v.z,bounds.min.z,bounds.max.z));}
     mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();owner.meshes.Add(mesh);cache[key]=mesh;
    }
    var a=filter.sharedMesh.bounds;var c=mesh.bounds;
    MaximumExpansion=Mathf.Max(MaximumExpansion,Mathf.Max(0,a.min.x-c.min.x,a.min.y-c.min.y,a.min.z-c.min.z,c.max.x-a.max.x,c.max.y-a.max.y,c.max.z-a.max.z));
    filter.sharedMesh=mesh;
   }ChangedVisuals++;
  }
 }
 // Indoor equivalents reuse every existing shard envelope and its host-owned collision descriptor.
 static void BuildMallGroup(Brush b,C2.CityDensityPack.Asset asset,int group,bool intact=false){
  if(group==-2){foreach(int shard in asset.pieces.Select(x=>x.shard).Distinct())BuildMallGroup(b,asset,shard,true);return;}
  var pieces=asset.pieces.Where(x=>x.shard==group).ToArray();if(pieces.Length==0)return;
  var bounds=City2Data.Bounds(pieces[0]);foreach(var piece in pieces)bounds.Encapsulate(City2Data.Bounds(piece));var c=bounds.center;var s=bounds.size;
  if(asset.id=="street_awning"&&group>=0){b.AtlasBox(3,c,s);b.AtlasBox(4,new Vector3(c.x,c.y,bounds.min.z+.006f),new Vector3(s.x*.86f,s.y*.48f,.009f));return;}
  if(asset.id=="market_stall"){
   if(group<2){b.AtlasBox(3,c,s);return;}
   if(group==2||group==3){MallCase(b,bounds,true);return;}
   if(group==4){foreach(float side in new[]{-1f,1f})b.AtlasBox(3,new Vector3(c.x+side*s.x*.47f,c.y,c.z),new Vector3(.12f,s.y,.15f));b.AtlasBox(3,new Vector3(c.x,bounds.max.y-.19f,c.z),new Vector3(s.x,.38f,s.z));b.AtlasBox(4,new Vector3(c.x,bounds.max.y-.19f,bounds.min.z+.009f),new Vector3(s.x*.86f,.20f,.012f));return;}
   MallCase(b,bounds,false);return;
  }
  if(asset.id=="crate_stack"){MallCase(b,bounds,true);return;}
  if(asset.id=="delivery_van"&&group>=0){if(group>=6){b.AtlasBox(3,c,s);return;}MallCase(b,bounds,group==4);return;}
  if(asset.id=="hvac_unit"&&group>=0){MallCase(b,bounds,true);return;}
  foreach(var piece in pieces){var a=City2Data.Bounds(piece);var p=a.center;var z=a.size;string n=piece.name;int m=piece.material;
   if(asset.id=="market_crate"){
    if(n.Contains("nail")||n.Contains("seam"))continue;b.AtlasBox(m==2?7:3,p,z);continue;
   }
   if(asset.id=="facade_break_panel"){
    if(n.Contains("inset"))continue;MallCase(b,a,false);continue;
   }
   if(asset.id=="blade_sign"){
    if(n.Contains("enamel_panel")){if(intact)b.AtlasBox(3,p,z);else b.AtlasBevel(3,p,z,.04f);continue;}b.Append(piece,n.Contains("glyph")?2:m==2?2:3);continue;
   }
   if(asset.id=="street_bench"){b.AtlasBox(m==5?15:3,p,z);continue;}
   if(asset.id=="street_awning"){b.AtlasBox(m==2?2:3,p,z);continue;}
   if(asset.id=="delivery_van"){b.AtlasBox(n.Contains("pillar")?2:3,p,z);continue;}
   if(asset.id=="hvac_unit"){if(n.Contains("pipe")||n.Contains("return")||n.Contains("louver"))continue;b.AtlasBox(m==2?2:3,p,z);continue;}
   if(asset.id=="road_barrier"){b.AtlasBox(n.Contains("panel")?4:n.Contains("reflector")?3:7,p,z);continue;}
   if(asset.id=="steel_drum"){b.Append(piece,n.Contains("curved_wall")?3:n.Contains("hoop")?7:2);continue;}
   if(asset.id=="junction_cabinet"){if(n.Contains("vent"))continue;if(n.Contains("service_panel")){MallCase(b,a,false);continue;}b.AtlasBox(m==2?2:3,p,z);continue;}
   b.Append(piece,m==5?3:m==6||m==7||m==15?4:m);
  }
 }
 static void MallCase(Brush b,Bounds a,bool cabinet){
  var c=a.center;var s=a.size;float frame=Mathf.Min(.07f,Mathf.Min(s.x,s.y)*.08f),front=a.min.z+.005f;
  // Thin front sheets use a tinted pane; deep cases stay open between their glass-edge reflections.
  if(cabinet||s.z<.35f)b.AtlasBox(cabinet?3:4,c,s);
  else{b.AtlasBox(4,new Vector3(c.x,c.y,a.max.z-.012f),new Vector3(s.x,s.y,.018f));for(int i=-1;i<=1;i++)b.AtlasBox(i==0?13:7,new Vector3(c.x+s.x*i*.25f,c.y,c.z),new Vector3(s.x*.14f,s.y*.55f,s.z*.20f));}
  foreach(float side in new[]{-1f,1f}){
   b.AtlasBox(3,new Vector3(c.x+side*(s.x-frame)*.5f,c.y,front),new Vector3(frame,s.y,.008f));
   b.AtlasBox(3,new Vector3(c.x,c.y+side*(s.y-frame)*.5f,front),new Vector3(s.x,frame,.008f));
  }
  if(cabinet)b.AtlasBox(4,new Vector3(c.x,c.y,front),new Vector3(s.x*.70f,s.y*.65f,.006f));
  else b.AtlasStroke(3,new Vector3(c.x-s.x*.28f,c.y-s.y*.30f,front-.001f),new Vector3(c.x+s.x*.12f,c.y+s.y*.28f,front-.001f),Mathf.Min(.025f,frame*.35f));
  float foot=Mathf.Min(.16f,s.y*.16f);b.AtlasBox(2,new Vector3(c.x,a.min.y+foot*.5f,c.z),new Vector3(s.x,foot,s.z));
  b.AtlasBox(2,new Vector3(a.min.x+frame*.5f,c.y,c.z),new Vector3(frame,s.y,s.z));
 }
 static bool Eligible(string id){return id=="street_bench"||id=="market_crate"||id=="blade_sign"||id=="facade_break_panel"||id=="road_barrier"||id=="junction_cabinet"||id=="solar_array_r12"||id=="irrigation_tank_r12";}
 static void BuildPiece(Brush b,string asset,C2.CityDensityPack.Piece piece,string region,bool intact){
  var bounds=City2Data.Bounds(piece);var c=bounds.center;var s=bounds.size;int mat=piece.material;
  // The existing material roles stay wood / metal / glass. Only atlas surface roles change.
  if(mat==6||mat==7||mat==15){if(region=="mall")mat=7;else if(region=="office")mat=6;else if(region=="port")mat=12;else if(region=="country")mat=5;else if(region=="launchrange")mat=8;else if(region=="orbit")mat=4;}
  if(asset=="solar_array_r12"&&mat==6)mat=4;
  bool boards=asset=="market_crate"&&(piece.name.Contains("long_side")||piece.name.Contains("end_side"));
  if(boards){int n=region=="port"?5:3;float h=s.y/n;for(int i=0;i<n;i++)b.AtlasBox(mat,new Vector3(c.x,bounds.min.y+h*(i+.5f),c.z),new Vector3(s.x,h*.84f,s.z));return;}
  bool bevel=(asset=="blade_sign"&&piece.name.Contains("enamel_panel"))||(asset=="facade_break_panel"&&piece.name.Contains("ceramic_cell"));
  // An intact sign has continuous joins; only detached panels retain the broken corner silhouette.
  if(intact&&asset=="blade_sign"&&piece.name.Contains("enamel_panel")){b.AtlasBox(mat,c,s);return;}
  if(bevel&&s.z>.001f){b.AtlasBevel(mat,c,s,region=="mall"?.15f:.055f);return;}
  if(asset=="street_bench"&&piece.name.Contains("back_slat")){
   // Same slat/shard envelope: marine paint with metal end straps versus routed country timber.
   b.AtlasBox(region=="port"?6:5,c+Vector3.forward*.006f,new Vector3(s.x,s.y,s.z-.012f));
   float front=bounds.min.z+.004f;
   if(region=="port")foreach(float side in new[]{-1f,1f}){
    b.AtlasBox(3,new Vector3(c.x+side*s.x*.40f,c.y,front),new Vector3(s.x*.075f,s.y*.96f,.006f));
    b.AtlasBox(2,new Vector3(c.x+side*s.x*.40f,c.y,front-.001f),new Vector3(.035f,.035f,.003f));
   }
   else foreach(float band in new[]{-.26f,.26f})b.AtlasBox(12,new Vector3(c.x,c.y+s.y*band,front),new Vector3(s.x*.92f,.012f,.006f));
   return;
  }
  b.Append(piece,mat);
  // Recesses sit inside each existing panel. They inherit that panel's shard ownership.
  if(asset=="junction_cabinet"&&piece.name.Contains("service_panel")){
   b.AtlasBox(region=="orbit"?4:10,c+new Vector3(0,s.y*.19f,-s.z*.42f),new Vector3(s.x*.62f,s.y*.20f,Mathf.Max(.003f,s.z*.06f)));
  }
  if(asset=="solar_array_r12"&&piece.name.Contains("cell"))for(int i=1;i<4;i++)b.AtlasBox(3,new Vector3(bounds.min.x+s.x*i/4,c.y,bounds.min.z+.002f),new Vector3(.008f,s.y*.90f,.003f));
 }

 // Small local mesh brush, following the existing authored city batching pattern.
 sealed class Brush {
  readonly List<Vector3> v=new List<Vector3>();readonly List<int> t=new List<int>();readonly List<Vector2> uv=new List<Vector2>();
  readonly List<int> roles=new List<int>();readonly Transform root;readonly Shader shader;
  static readonly Vector3[] Corners={new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)};
  static readonly int[] Faces={0,3,2,1,4,5,6,7,0,4,7,3,1,2,6,5,3,7,6,2,0,1,5,4};
  public Brush(){}
  public Brush(string name,Transform parent,Shader shader){root=new GameObject(name).transform;root.SetParent(parent,false);this.shader=shader;}
  void Quad(int role,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector2 tex){int k=v.Count;v.AddRange(new[]{a,b,c,d});uv.AddRange(new[]{tex,tex,tex,tex});t.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});roles.Add(role);roles.Add(role);}
  public void Box(int role,Vector3 c,Vector3 s){BoxTex(role,c,s,Vector2.zero,Quaternion.identity);}
  void BoxTex(int role,Vector3 c,Vector3 s,Vector2 tex,Quaternion rotation){for(int f=0;f<6;f++)Quad(role,c+rotation*Vector3.Scale(Corners[Faces[f*4]],s*.5f),c+rotation*Vector3.Scale(Corners[Faces[f*4+1]],s*.5f),c+rotation*Vector3.Scale(Corners[Faces[f*4+2]],s*.5f),c+rotation*Vector3.Scale(Corners[Faces[f*4+3]],s*.5f),tex);}
  public void Rod(int role,Vector3 a,Vector3 b,float width){BoxTex(role,(a+b)*.5f,new Vector3(width,(b-a).magnitude,width),Vector2.zero,Quaternion.FromToRotation(Vector3.up,b-a));}
  public void Disc(int role,Vector3 c,float radius,float depth,int count){for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count,b=(i+1)*Mathf.PI*2/count;Quad(role,c,c+new Vector3(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius,0),c+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0),c,Vector2.zero);}}
  public void Arch(int role,Vector3 c,float rx,float ry,float width,int count){for(int i=0;i<count;i++){float a=i*Mathf.PI/count,b=(i+1)*Mathf.PI/count;Rod(role,c+new Vector3(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry,0),c+new Vector3(Mathf.Cos(b)*rx,Mathf.Sin(b)*ry,0),width);}}
  public void Wedge(int role,Vector3 c,Vector3 s){Vector3 a=c+new Vector3(-s.x*.5f,-s.y*.5f,-s.z*.5f),b=c+new Vector3(s.x*.5f,-s.y*.5f,-s.z*.5f),d=c+new Vector3(-s.x*.5f,s.y*.5f,-s.z*.5f),z=Vector3.forward*s.z;Quad(role,a,d,b,b,Vector2.zero);Quad(role,a+z,b+z,d+z,d+z,Vector2.zero);Quad(role,a,b,b+z,a+z,Vector2.zero);Quad(role,a,a+z,d+z,d,Vector2.zero);Quad(role,d,d+z,b+z,b,Vector2.zero);}
  public void Crown(int role,Vector3 c,Vector3 s){var a=c+new Vector3(-s.x*.5f,-s.y*.5f,-s.z*.5f);var b=c+new Vector3(s.x*.5f,-s.y*.5f,-s.z*.5f);var d=c+new Vector3(0,s.y*.5f,0);var e=c+new Vector3(-s.x*.5f,-s.y*.5f,s.z*.5f);var f=c+new Vector3(s.x*.5f,-s.y*.5f,s.z*.5f);Quad(role,a,d,b,b,Vector2.zero);Quad(role,b,d,f,f,Vector2.zero);Quad(role,f,d,e,e,Vector2.zero);Quad(role,e,d,a,a,Vector2.zero);}
  static Vector2 Tex(int m){return new Vector2((m%4+.5f)/4,(m/4+.5f)/4);}
  public void AtlasStroke(int mat,Vector3 a,Vector3 b,float width){BoxTex(0,(a+b)*.5f,new Vector3(width,(b-a).magnitude,width),Tex(mat),Quaternion.FromToRotation(Vector3.up,b-a));}
  public void AtlasBox(int mat,Vector3 c,Vector3 s){BoxTex(0,c,s,Tex(mat),Quaternion.identity);}
  public void AtlasBevel(int mat,Vector3 c,Vector3 s,float cut){Vector2[] q={new Vector2(-.5f+cut,-.5f),new Vector2(.5f-cut,-.5f),new Vector2(.5f,-.5f+cut),new Vector2(.5f,.5f-cut),new Vector2(.5f-cut,.5f),new Vector2(-.5f+cut,.5f),new Vector2(-.5f,.5f-cut),new Vector2(-.5f,-.5f+cut)};for(int i=0;i<q.Length;i++){var a=c+new Vector3(q[i].x*s.x,q[i].y*s.y,-s.z*.5f);var b=c+new Vector3(q[(i+1)%8].x*s.x,q[(i+1)%8].y*s.y,-s.z*.5f);var z=Vector3.forward*s.z;Quad(0,c-Vector3.forward*s.z*.5f,b,a,a,Tex(mat));Quad(0,c+Vector3.forward*s.z*.5f,a+z,b+z,b+z,Tex(mat));Quad(0,a,b,b+z,a+z,Tex(mat));}}
  public void AppendOriginal(C2.CityDensityPack.Piece piece){int start=v.Count;for(int i=0;i<piece.v.Length;i+=3)v.Add(new Vector3(piece.v[i],piece.v[i+1],piece.v[i+2]));for(int i=0;i<piece.uv.Length;i+=2)uv.Add(new Vector2(piece.uv[i],piece.uv[i+1]));foreach(int n in piece.t)t.Add(start+n);for(int i=0;i<piece.t.Length/3;i++)roles.Add(0);}
  public void AppendBrush(Brush source,Matrix4x4 matrix){int start=v.Count;foreach(var point in source.v)v.Add(matrix.MultiplyPoint3x4(point));uv.AddRange(source.uv);foreach(int n in source.t)t.Add(start+n);for(int i=0;i<source.t.Count/3;i++)roles.Add(0);}
  public void Append(C2.CityDensityPack.Piece piece,int mat){int k=v.Count;for(int i=0;i<piece.v.Length;i+=3){v.Add(new Vector3(piece.v[i],piece.v[i+1],piece.v[i+2]));uv.Add(Tex(mat));}foreach(int n in piece.t)t.Add(k+n);for(int i=0;i<piece.t.Length/3;i++)roles.Add(0);}
  public Mesh Mesh(string name){var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
  public void Finish(bool groundDepth=true){
   // The host camera always looks down 5.3 / 16. Sink distant foundations along that
   // sight line so a shed or pylon meets the lane horizon instead of appearing airborne.
   if(groundDepth)for(int i=0;i<v.Count;i++){var point=v[i];point.y-=Mathf.Max(0,point.z)*5.3f/16;v[i]=point;}
   var owner=root.gameObject.AddComponent<C2.CityDensityOwnedAssets>();for(int role=0;role<Palette.Length;role++){var indices=new List<int>();for(int i=0;i<roles.Count;i++)if(roles[i]==role)indices.AddRange(new[]{t[i*3],t[i*3+1],t[i*3+2]});if(indices.Count==0)continue;var mesh=new Mesh{name=root.name+" palette "+role,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();owner.meshes.Add(mesh);BackgroundTriangles+=indices.Count/3;var mat=new Material(shader){name="R16 regional material "+role,color=(root.name.StartsWith("mall")?MallCutePalette:Palette)[role]};if(mat.HasProperty("_Glossiness"))mat.SetFloat("_Glossiness",role==11?.42f:role==2?.33f:role==10?.36f:.12f);if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",role==0?.24f:0);owner.materials.Add(mat);var go=new GameObject(mat.name);go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;mesh.UploadMeshData(true);}}
 }
}
public sealed partial class CrabFlightGame {public bool useSectionArtR16=true;}
