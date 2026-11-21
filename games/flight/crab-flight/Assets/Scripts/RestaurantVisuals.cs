using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Original, renderer-only set dressing. No CreatePrimitive, Collider, Rigidbody, Light or external asset.
// Breakable roots are world-positioned, identity-rotated, scale ONE. Root owns every physics component.
public static class RestaurantVisuals {
 public static bool ExteriorEnabled=true,DensityEnabled=true;
 static Shader shader;static Mesh box,cylinder,bowl;static GameObject environment;
 static readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
 static readonly Color Plaster=new Color(.76f,.71f,.60f),Wood=new Color(.28f,.18f,.12f),Oak=new Color(.48f,.32f,.18f),Stone=new Color(.45f,.46f,.41f),Ivory=new Color(.87f,.84f,.72f),Brass=new Color(.55f,.44f,.26f),Dark=new Color(.13f,.18f,.18f);

 public static void Build(Shader groundShader){
  shader=groundShader?groundShader:Shader.Find("Standard");if(environment)return;
  environment=new GameObject("Restaurant authored environment - visuals only");var p=environment.transform;
  Box("Limestone floor",p,new Vector3(0,-.16f,0),new Vector3(66,.32f,24),new Color(.53f,.51f,.43f));
  // Neutral landing ground continues the real collider beyond this representative room.
  foreach(float side in new[]{-1f,1f})Box("Outside landing ground",p,new Vector3(side*216.5f,-.16f,0),new Vector3(367,.32f,24),new Color(.44f,.47f,.44f));
  for(int x=-380;x<=380;x+=20)if(Mathf.Abs(x)>33)Box("Outside ground marker",p,new Vector3(x,.004f,0),new Vector3(.06f,.006f,22),new Color(.60f,.62f,.54f));
  for(int x=-30;x<=30;x+=5)Box("Floor grout X",p,new Vector3(x,.004f,0),new Vector3(.025f,.006f,22),Stone);
  for(int z=-10;z<=10;z+=4)Box("Floor grout Z",p,new Vector3(0,.005f,z),new Vector3(65,.006f,.025f),Stone);
  Box("Background plaster",p,new Vector3(0,6.75f,10.5f),new Vector3(51,13.5f,.3f),Plaster);
  Box("Background wood dado",p,new Vector3(0,1.25f,10.25f),new Vector3(51,2.5f,.17f),Wood);
  Box("Dado cap",p,new Vector3(0,2.55f,10.10f),new Vector3(51,.13f,.26f),Oak);
  Box("Background ceiling only",p,new Vector3(0,13.75f,6.7f),new Vector3(51,.28f,7.2f),new Color(.63f,.59f,.49f));
  Box("Back cornice",p,new Vector3(0,12.9f,10.0f),new Vector3(51,.30f,.5f),Ivory);
  foreach(float x in new[]{-21f,-8f,8f,21f}){
   Box("Rear timber column",p,new Vector3(x,6.7f,8.4f),new Vector3(.42f,13.4f,.5f),Wood);
   Box("Column foot",p,new Vector3(x,.25f,8.4f),new Vector3(.68f,.5f,.72f),Stone);
  }
  // The windows are painted depth cues on the distant wall, not an opaque foreground pane.
  foreach(float x in new[]{-16f,0f,16f})Window(p,x);
  foreach(float x in new[]{-16.5f,-7f,7f,16.5f})Pendant(p,x,7.5f);
  Tank(p);
  foreach(float x in new[]{-15f,-11.5f,11.5f,15f})Table(p,x,5.5f,1.8f);
  // Low foreground furniture is kept away from the launch lane and first glass break.
  Table(p,-19,8.3f,1.05f);Table(p,19,8.3f,1.05f);
  foreach(float x in new[]{-17.5f,17.5f}){
   Box("Restaurant service cabinet",p,new Vector3(x,.9f,8.1f),new Vector3(3,1.8f,1.35f),Oak);
   Box("Service counter",p,new Vector3(x,1.84f,8.1f),new Vector3(3.2f,.12f,1.5f),Ivory);
   for(int i=-1;i<=1;i++)Box("Cabinet panel",p,new Vector3(x+i*.9f,.95f,7.39f),new Vector3(.76f,1.45f,.035f),Wood);
  }
  // Combine by palette once: few static renderers; breakable visuals are never combined here.
  if(ExteriorEnabled)Exterior(p);
  CombineEnvironment(p);
 }

 // Display-only depth separation: background never masquerades as a near contact prop.
 public static void DepthTone(){if(!environment)return;DepthTone(environment);}
 public static void DepthTone(GameObject root){
  if(!root)return;foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()){
   if(!renderer.enabled)continue;var mat=renderer.sharedMaterial;if(!mat||!mat.HasProperty("_Color"))continue;
   var c=mat.color;float luminance=c.r*.2126f+c.g*.7152f+c.b*.0722f;
   var tone=Color.Lerp(c,new Color(luminance*.78f,luminance*.84f,luminance*.87f,c.a),.52f);tone.r*=.83f;tone.g*=.86f;tone.b*=.9f;
   var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);block.SetColor("_Color",tone);block.SetFloat("_Glossiness",.06f);renderer.SetPropertyBlock(block);
  }
 }
 public static GameObject CreateBreakableVisual(string type,Vector3 pos,Vector3 size,Color color){
  if(!shader)shader=Shader.Find("Standard");size=new Vector3(Mathf.Max(.025f,Mathf.Abs(size.x)),Mathf.Max(.025f,Mathf.Abs(size.y)),Mathf.Max(.025f,Mathf.Abs(size.z)));
  var root=new GameObject("Breakable "+(type??"wall")+" authored visual");root.transform.position=pos;root.transform.rotation=Quaternion.identity;root.transform.localScale=Vector3.one;
  string key=(type??"wall").ToLowerInvariant();
  if(key=="glass")Glass(root.transform,size,color);
  else if(key=="tableware"||key=="dish"||key=="dishes")Dishes(root.transform,size,color);
  else if(key=="food")Dishes(root.transform,size,color);
  else if(key=="wood"){Box("Solid timber top",root.transform,new Vector3(0,size.y*.36f,0),new Vector3(size.x,size.y*.28f,size.z),color);for(int sign=-1;sign<=1;sign+=2)Box("Timber leg",root.transform,new Vector3(sign*size.x*.31f,-size.y*.14f,0),new Vector3(size.x*.15f,size.y*.72f,size.z*.75f),Wood);}
  else if(key=="metal"){Box("Steel equipment body",root.transform,Vector3.zero,size,color);Box("Heavy metal rim",root.transform,new Vector3(0,size.y*.45f,0),new Vector3(size.x*1.05f,size.y*.10f,size.z*1.05f),new Color(.67f,.70f,.67f));for(int sign=-1;sign<=1;sign+=2)Box("Metal inset panel",root.transform,new Vector3(0,sign*size.y*.22f,-size.z*.502f),new Vector3(size.x*.7f,size.y*.32f,.025f),Dark);}
  else Wall(root.transform,size,color,pos.y);
  return root;
 }

 // Original 3D street context, renderer-only background outside the physical launch lane.
 static void Exterior(Transform p){
  foreach(float side in new[]{-1f,1f}){
   Box("Exterior paved landing road",p,new Vector3(side*208,.009f,0),new Vector3(364,.014f,11.5f),new Color(.28f,.32f,.33f));
   Box("Pavement edge",p,new Vector3(side*208,.03f,6.4f),new Vector3(364,.06f,1),new Color(.65f,.64f,.58f));
   for(int i=0;i<16;i++)Box("Road dash",p,new Vector3(side*(32+i*20),.025f,0),new Vector3(5,.018f,.08f),new Color(.81f,.72f,.45f));
   // Two renderer-only depth layers. The playable lane and breakables are unchanged.
   if(DensityEnabled)for(int i=0;i<22;i++){
    float x=side*(40+i*15),h=4.5f+(i%4)*1.3f,w=11.4f;
    Box("Streetfront shop block",p,new Vector3(x,h*.5f,16.5f),new Vector3(w,h,5.5f),i%2==0?Plaster:Stone);
    Box("Shop lintel",p,new Vector3(x,2.55f,13.67f),new Vector3(w,.18f,.18f),Ivory);
    Box("Shop recessed glazing",p,new Vector3(x,1.28f,13.70f),new Vector3(w*.79f,2.2f,.035f),Dark);
    for(int k=-1;k<=1;k++)Box("Shop vertical frame",p,new Vector3(x+k*w*.27f,1.3f,13.64f),new Vector3(.075f,2.3f,.10f),Brass);
    Box("Shop canopy",p,new Vector3(x,2.85f,13.3f),new Vector3(w*.84f,.14f,1.25f),i%2==0?Wood:Dark);
    Box("Shop roof lip",p,new Vector3(x,h+.1f,16.5f),new Vector3(w+.15f,.2f,5.7f),Ivory);
    for(int k=-1;k<=1;k++)Box("Upper shop window",p,new Vector3(x+k*w*.26f,h-1.0f,13.72f),new Vector3(1.4f,.9f,.035f),Dark);
   }
   if(DensityEnabled)for(int i=0;i<17;i++){
    float x=side*(44+i*21),h=13+(i%5)*3.5f;
    Box("Far skyline depth",p,new Vector3(x,h*.5f,48+(i%3)*4),new Vector3(16,h,9),new Color(.39f,.47f,.48f));
    Box("Far skyline cap",p,new Vector3(x,h+.15f,48+(i%3)*4),new Vector3(16.3f,.3f,9.3f),new Color(.33f,.41f,.43f));
    for(int row=0;row<4;row++)Box("Far horizontal window row",p,new Vector3(x,3+row*(h-4)/4,43.45f+(i%3)*4),new Vector3(13,.42f,.025f),new Color(.29f,.39f,.41f));
   }
   // Facade thickness and exterior sill make the exit a visible threshold.
   foreach(float z in new[]{-4.2f,4.2f})Box("Outside facade column",p,new Vector3(side*25.5f,6.65f,z),new Vector3(.8f,13.3f,.65f),new Color(.42f,.38f,.31f));
   Box("Exterior cornice",p,new Vector3(side*25.5f,13.6f,0),new Vector3(1.25f,.40f,9),new Color(.69f,.65f,.55f));
   for(int i=0;i<11;i++){
    float x=side*(39+i*27),h=7+(i%4)*3,w=14+(i%3)*3,z=23+(i%2)*8;Color c=i%2==0?new Color(.40f,.49f,.51f):new Color(.52f,.53f,.49f);
    Box("Original distant city block",p,new Vector3(x,h*.5f,z),new Vector3(w,h,10),c);
    Box("Distant roof line",p,new Vector3(x,h+.16f,z),new Vector3(w+.35f,.32f,10.3f),new Color(.28f,.35f,.37f));
    for(int row=0;row<3;row++)for(int col=-1;col<=1;col++)Box("Distant facade window",p,new Vector3(x+col*w*.26f,2+row*(h-3)/3,z-5.025f),new Vector3(w*.16f,1.2f,.04f),new Color(.23f,.34f,.39f));
    float lampX=side*(35+i*24);Rod("Street lamp shaft",p,new Vector3(lampX,0,7),new Vector3(lampX,4.2f,7),.07f,Dark);Box("Street lamp head",p,new Vector3(lampX,4.2f,6.7f),new Vector3(.6f,.16f,.7f),Ivory);
   }
  }
 }
 static void Tank(Transform p){
  Box("Tank stone plinth",p,new Vector3(0,.19f,0),new Vector3(9.5f,.38f,5.65f),new Color(.31f,.35f,.32f));
  Box("Tank bottom",p,new Vector3(0,.41f,0),new Vector3(9.15f,.08f,5.3f),new Color(.42f,.51f,.43f));
  // A shallow authored water surface. No opaque front glass hides the crab.
  Box("Tank water level 0.6",p,new Vector3(0,.594f,0),new Vector3(8.9f,.012f,5.05f),new Color(.20f,.48f,.47f));
  foreach(float z in new[]{-2.72f,2.72f}){
   Box("Tank base long lip",p,new Vector3(0,.47f,z),new Vector3(9.55f,.16f,.14f),Stone);
   Box("Tank upper long rail",p,new Vector3(0,6.5f,z),new Vector3(9.15f,.085f,.085f),new Color(.58f,.66f,.60f));
  }
  // End panes at x +/-4.5 belong solely to root's breakable glass objects.
  for(int i=0;i<3;i++){
   float x=-2.7f+i*2.7f;Arc("Water reflection",p,new Vector3(x,.612f,.45f),.48f+i*.06f,.026f,new Color(.49f,.68f,.60f),.7f,2.5f);
  }
 }
 static void Window(Transform p,float x){
  Box("City window surround",p,new Vector3(x,7.6f,10.20f),new Vector3(9.1f,7.0f,.22f),Wood);
  Box("Window evening sky",p,new Vector3(x,7.6f,10.04f),new Vector3(8.65f,6.55f,.08f),new Color(.38f,.48f,.49f));
  for(int i=0;i<5;i++){
   float h=1.6f+(i%3)*.75f;Box("Distant building silhouette",p,new Vector3(x-3.3f+i*1.65f,4.5f+h*.5f,9.96f),new Vector3(1.35f,h,.06f),new Color(.30f,.37f,.37f));
  }
  Box("Window mullion",p,new Vector3(x,7.6f,9.88f),new Vector3(.11f,6.7f,.13f),Brass);
  Box("Window transom",p,new Vector3(x,7.45f,9.88f),new Vector3(8.8f,.10f,.13f),Brass);
  Box("Window sill",p,new Vector3(x,4.22f,9.9f),new Vector3(9.25f,.18f,.65f),Ivory);
 }
 static void Pendant(Transform p,float x,float z){
  Rod("Pendant cable",p,new Vector3(x,13.3f,z),new Vector3(x,10.2f,z),.025f,Dark);
  Lathe("Pendant shade",p,new Vector3(x,9.95f,z),new Vector3(1.65f,.7f,1.65f),ShadeMesh(),Wood);
  Cylinder("Warm lamp diffuser",p,new Vector3(x,9.65f,z),new Vector3(1.25f,.06f,1.25f),new Color(.87f,.70f,.42f));
 }
 static void Table(Transform p,float x,float z,float height){
  Box("Dining table top",p,new Vector3(x,height,z),new Vector3(2.75f,.16f,1.75f),Oak);
  foreach(float dx in new[]{-1.05f,1.05f})foreach(float dz in new[]{-.58f,.58f})Box("Table leg",p,new Vector3(x+dx,height*.49f,z+dz),new Vector3(.13f,height*.96f,.13f),Wood);
  Box("Linen runner",p,new Vector3(x,height+.09f,z),new Vector3(2.0f,.015f,.55f),new Color(.70f,.66f,.53f));
  // A closed menu and small vase distinguish background dining from root's breakable dish stack.
  Box("Folded menu",p,new Vector3(x-.65f,height+.13f,z),new Vector3(.36f,.07f,.42f),Dark);
  Cylinder("Table vase",p,new Vector3(x+.65f,height+.24f,z),new Vector3(.16f,.30f,.16f),new Color(.54f,.58f,.44f));
 }
 static void Glass(Transform p,Vector3 s,Color tint){
  // Keep the center empty. Thin edges/highlights indicate clear glass without Standard alpha issues.
  int normal=s.x<=s.z?0:2;int u=normal==0?2:0;Vector3 a=Axis(u),n=Axis(normal);float width=s[u],height=s.y,beam=Mathf.Min(.075f,Mathf.Min(width,height)*.035f);
  Color edge=Color.Lerp(new Color(.52f,.69f,.66f),tint,.12f);
  foreach(float side in new[]{-.5f,.5f}){
   Box("Glass clear edge",p,a*(width*side),Dims(normal,s[normal],u,beam,height),edge);
   Box("Glass upper lower rim",p,Vector3.up*(height*side),Dims(normal,s[normal],u,width,beam),edge);
  }
  // Two small reflected light streaks, not an opaque solid pane.
  Rod("Glass highlight long",p,a*(-width*.27f)+Vector3.up*(-height*.22f)-n*.012f,a*(-width*.15f)+Vector3.up*(height*.12f)-n*.012f,.018f,new Color(.71f,.84f,.78f));
  Rod("Glass highlight short",p,a*(width*.24f)+Vector3.up*(height*.10f)-n*.014f,a*(width*.30f)+Vector3.up*(height*.27f)-n*.014f,.014f,new Color(.71f,.84f,.78f));
 }
 static void Wall(Transform p,Vector3 s,Color color,float worldY){
  Box("Plaster breakable panel",p,Vector3.zero,s,color);int normal=s.x<=s.z?0:2,u=normal==0?2:0;Vector3 n=Axis(normal),a=Axis(u);float depth=s[normal]*.5f+.012f;
  foreach(float side in new[]{-1f,1f}){
   if(worldY<5)Box("Lower timber wainscot",p,Vector3.up*(-s.y*.32f)+n*(side*depth),Dims(normal,.035f,u,s[u]*.97f,s.y*.34f),Wood);
   Box("Panel cap trim",p,Vector3.up*(s.y*.46f)+n*(side*depth),Dims(normal,.045f,u,s[u],.08f),Color.Lerp(color,Ivory,.25f));
   for(int i=-1;i<=1;i++)Box("Quiet plaster joint",p,a*(s[u]*i*.30f)+n*(side*depth),Dims(normal,.014f,u,.018f,s.y*.87f),Color.Lerp(color,Stone,.18f));
  }
 }
 static void Dishes(Transform p,Vector3 s,Color color){
  float width=s.x*.92f,depth=s.z*.90f;Color ceramic=Color.Lerp(Ivory,color,.25f);
  // Tall stack uses the full supplied collision envelope and reads as crockery, not a solid crate.
  int count=6;float step=s.y/(count+2);
  for(int i=0;i<count;i++){
   float y=-s.y*.5f+step*(i+1);Lathe("Ceramic rimmed plate stack",p,new Vector3(0,y,0),new Vector3(width,step*.95f,depth),PlateMesh(),ceramic);
  }
  Lathe("Serving bowl",p,new Vector3(0,s.y*.40f,0),new Vector3(width,.45f*Mathf.Min(s.y,1.8f),depth),BowlMesh(),ceramic);
 }
 static Vector3 Axis(int index){return index==0?Vector3.right:index==1?Vector3.up:Vector3.forward;}
 static Vector3 Dims(int normal,float thickness,int u,float width,float height){var s=Vector3.one;s[normal]=thickness;s[u]=width;s.y=height;return s;}
 static Material Material(Color color){if(materials.TryGetValue(color,out var m)&&m&&m.shader==shader)return m;if(!shader)shader=Shader.Find("Standard");m=new Material(shader){name="Restaurant authored palette"};m.color=color;if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",.22f);if(m.HasProperty("_Roughness"))m.SetFloat("_Roughness",.78f);materials[color]=m;return m;}
 static GameObject Part(string name,Transform parent,Mesh mesh,Vector3 pos,Vector3 scale,Color color){var o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=scale;o.AddComponent<MeshFilter>().sharedMesh=mesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterial=Material(color);r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return o;}
 static GameObject Box(string name,Transform p,Vector3 pos,Vector3 size,Color color){return Part(name,p,BoxMesh(),pos,size,color);}
 static GameObject Cylinder(string name,Transform p,Vector3 pos,Vector3 size,Color color){return Lathe(name,p,pos,size,CylinderMesh(),color);}
 static GameObject Lathe(string name,Transform p,Vector3 pos,Vector3 size,Mesh mesh,Color color){return Part(name,p,mesh,pos,size,color);}
 static void Rod(string name,Transform p,Vector3 start,Vector3 end,float width,Color color){var delta=end-start;var o=Box(name,p,(start+end)*.5f,new Vector3(width,delta.magnitude,width),color);o.transform.localRotation=Quaternion.FromToRotation(Vector3.up,delta);}
 static void Arc(string name,Transform p,Vector3 center,float radius,float width,Color color,float start,float end,float aspect=1){const int steps=18;var previous=center+new Vector3(Mathf.Cos(start)*radius,0,Mathf.Sin(start)*radius*aspect);for(int i=1;i<=steps;i++){float a=Mathf.Lerp(start,end,(float)i/steps);var next=center+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius*aspect);Rod(name,p,previous,next,width,color);previous=next;}}
 static Mesh BoxMesh(){if(box)return box;var v=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
  Vector3[] normals={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};foreach(var n in normals){var u=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.up;Vector3 a=Vector3.Cross(u,n),b=Vector3.Cross(n,a);int k=v.Count;v.Add((n-a-b)*.5f);v.Add((n+a-b)*.5f);v.Add((n+a+b)*.5f);v.Add((n-a+b)*.5f);triangles.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up});}box=MeshOf("Restaurant unit box",v,triangles,uv);return box;}
 static Mesh CylinderMesh(){if(!cylinder)cylinder=Profile("Restaurant unit cylinder",new[]{new Vector2(0,-.5f),new Vector2(.5f,-.5f),new Vector2(.5f,.5f),new Vector2(0,.5f)});return cylinder;}
 static Mesh BowlMesh(){if(!bowl)bowl=Profile("Restaurant serving bowl",new[]{new Vector2(0,-.3f),new Vector2(.28f,-.3f),new Vector2(.5f,.12f),new Vector2(.49f,.2f),new Vector2(.42f,.16f),new Vector2(.26f,-.17f),new Vector2(0,-.17f)});return bowl;}
 static Mesh plate;static Mesh PlateMesh(){if(!plate)plate=Profile("Restaurant rimmed plate",new[]{new Vector2(0,-.5f),new Vector2(.30f,-.5f),new Vector2(.47f,-.22f),new Vector2(.5f,.38f),new Vector2(.46f,.5f),new Vector2(.30f,-.12f),new Vector2(0,-.12f)});return plate;}
 static Mesh shade;static Mesh ShadeMesh(){if(!shade)shade=Profile("Restaurant pendant shade",new[]{new Vector2(.5f,-.5f),new Vector2(.48f,-.4f),new Vector2(.10f,.5f),new Vector2(.08f,.35f),new Vector2(.43f,-.5f),new Vector2(.5f,-.5f)});return shade;}
 static Mesh Profile(string name,Vector2[] profile){const int sides=24;var v=new List<Vector3>();var tri=new List<int>();var uv=new List<Vector2>();for(int j=0;j<profile.Length;j++)for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides;v.Add(new Vector3(Mathf.Cos(a)*profile[j].x,profile[j].y,Mathf.Sin(a)*profile[j].x));uv.Add(new Vector2((float)i/sides,(float)j/(profile.Length-1)));}for(int j=0;j<profile.Length-1;j++)for(int i=0;i<sides;i++){int a=j*(sides+1)+i,b=a+sides+1;tri.AddRange(new[]{a,b,a+1,a+1,b,b+1});}return MeshOf(name,v,tri,uv);}
 static Mesh MeshOf(string name,List<Vector3> v,List<int> tri,List<Vector2> uv){var m=new Mesh{name=name};m.SetVertices(v);m.SetTriangles(tri,0);m.SetUVs(0,uv);var colors=new Color[v.Count];for(int i=0;i<colors.Length;i++)colors[i]=Color.white;m.colors=colors;m.RecalculateNormals();m.RecalculateBounds();return m;}
 static void CombineEnvironment(Transform parent){var groups=new Dictionary<Material,List<CombineInstance>>();var renderers=parent.GetComponentsInChildren<MeshRenderer>();foreach(var r in renderers){var f=r.GetComponent<MeshFilter>();if(!f||!f.sharedMesh)continue;if(!groups.TryGetValue(r.sharedMaterial,out var items)){items=new List<CombineInstance>();groups.Add(r.sharedMaterial,items);}items.Add(new CombineInstance{mesh=f.sharedMesh,transform=parent.worldToLocalMatrix*f.transform.localToWorldMatrix});r.enabled=false;}foreach(var pair in groups){var mesh=new Mesh{name="Restaurant static palette mesh"};mesh.CombineMeshes(pair.Value.ToArray(),true,true);var o=new GameObject("Restaurant palette batch");o.transform.SetParent(parent,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterial=pair.Key;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}}
}
