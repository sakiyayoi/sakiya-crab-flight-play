using UnityEngine;

// Rounded story silhouettes. One shared scale grows art and the runtime trigger equally.
// Faces live on the camera-facing (-Z) surface, independent of spinning detail meshes.
public static class ObjectVisual {
 public sealed class Motion {
  public Transform rotor,net,soft;public bool plate;public ObjectRules.Kind kind;
  public Transform[] marks;Vector3 netRest;
  public void SetNet(Transform value){net=value;netRest=value.localPosition;}
  public void Pose(float time,bool reduced){
   if(rotor!=null){
    float angle=reduced?0:time*(plate?180:720);
    rotor.localRotation=plate?Quaternion.Euler(0,angle,0):Quaternion.Euler(0,0,angle);
   }
   if(soft!=null){
    float wave=reduced?0:Mathf.Sin(time*(kind==ObjectRules.Kind.Bounce?8:3));
    float amount=kind==ObjectRules.Kind.Bounce?.08f:.035f;
    soft.localScale=new Vector3(1-wave*amount,1+wave*amount,1);
   }
   if(net!=null){
    float wave=reduced?0:Mathf.Sin(time*1.4f);
    net.localPosition=new Vector3(netRest.x,netRest.y+wave*.055f,netRest.z);
    net.localRotation=Quaternion.Euler(0,0,wave*8);
   }
   if(marks==null)return;
   for(int i=0;i<marks.Length;i++){
    marks[i].gameObject.SetActive(!reduced);
    float t=(time*(kind==ObjectRules.Kind.Boost?2.4f:2)+i/(float)marks.Length)%1;
    if(kind==ObjectRules.Kind.Boost)
     marks[i].localPosition=new Vector3(.65f+t*1.15f,.22f+(i-1)*.25f,-.08f);
    else {
     marks[i].localPosition=new Vector3(.22f+(i-1)*t*.30f,1.02f+t*.42f,0);
     marks[i].localScale=Vector3.one*(.06f*(1-t)+.015f);
    }
   }
  }
 }
 public static GameObject Build(ObjectRules.Kind kind,out Material material,out Motion motion){
  var root=new GameObject("Action "+kind);var def=ObjectRules.Get(kind);motion=new Motion{kind=kind};
  ColorUtility.TryParseHtmlString(def.color,out var color);
  material=new Material(Shader.Find("Standard"));material.color=color;
  material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*ObjectRules.Emission);
  material.SetFloat("_Glossiness",.35f);
  var story=Group(root,def.shape,Vector3.zero);
  story.transform.localScale=Vector3.one*ObjectRules.ObjectScale;
  switch(kind){
   case ObjectRules.Kind.Boost:{
    Part(story,material,"fan-base",PrimitiveType.Cylinder,new Vector3(0,-1,0),new Vector3(.85f,.10f,.6f));
    Part(story,material,"fan-stand",PrimitiveType.Cylinder,new Vector3(0,-.62f,0),new Vector3(.14f,.32f,.14f));
    var head=Group(story,"fan-head",new Vector3(0,.22f,0));
    Ring(head,material,"fan-guard",.76f,.05f,-.17f);
    Grid(head,material,"fan-guard-grid",.71f,.027f,-.18f);
    var blades=Group(head,"fan-rotor",Vector3.zero);motion.rotor=blades.transform;
    for(int i=0;i<4;i++){
     float a=i*Mathf.PI/2;
     Part(blades,material,"fan-blade-"+i,PrimitiveType.Sphere,new Vector3(Mathf.Cos(a)*.34f,Mathf.Sin(a)*.34f,0),new Vector3(.6f,.18f,.1f),new Vector3(0,0,i*90+20));
    }
    Part(head,material,"fan-hub",PrimitiveType.Sphere,Vector3.zero,new Vector3(.56f,.50f,.36f));
    Face(head,material,kind,new Vector3(0,0,-.245f),.48f);
    // Local +X is outward on +X; the existing mirrored root also mirrors wind.
    motion.marks=new Transform[3];
    for(int i=0;i<3;i++)motion.marks[i]=Part(story,material,"fan-wind-"+i,PrimitiveType.Sphere,new Vector3(.8f,.22f+(i-1)*.25f,-.08f),new Vector3(.32f,.045f,.045f)).transform;
    break;
   }
   case ObjectRules.Kind.Bounce:{
    var pan=Group(story,"tilted-frying-pan",Vector3.zero);pan.transform.localRotation=Quaternion.Euler(0,0,-12);motion.soft=pan.transform;
    Part(pan,material,"pan-disc",PrimitiveType.Cylinder,new Vector3(-.3f,0,0),new Vector3(1.4f,.09f,1.2f),new Vector3(90,0,0));
    Part(pan,material,"pan-handle",PrimitiveType.Sphere,new Vector3(.67f,0,0),new Vector3(.9f,.18f,.22f));
    Face(pan,material,kind,new Vector3(-.3f,0,-.12f),.9f);break;
   }
   case ObjectRules.Kind.Spin:{
    Part(story,material,"sushi-belt",PrimitiveType.Sphere,new Vector3(0,-.35f,0),new Vector3(2,.25f,1.25f));
    for(int i=0;i<5;i++)Part(story,material,"belt-slat-"+i,PrimitiveType.Sphere,new Vector3((i-2)*.37f,-.19f,0),new Vector3(.08f,.06f,1.2f));
    var plate=Group(story,"sushi-plate-rotor",new Vector3(0,-.08f,0));motion.rotor=plate.transform;motion.plate=true;
    Part(plate,material,"sushi-plate",PrimitiveType.Cylinder,Vector3.zero,new Vector3(1.45f,.07f,1.05f));
    Part(plate,material,"sushi-rice",PrimitiveType.Sphere,new Vector3(.16f,.19f,.10f),new Vector3(.7f,.2f,.4f),default(Vector3),Color.white);
    // Topping reuses the existing orange action token; crab red stays reserved.
    ColorUtility.TryParseHtmlString(ObjectRules.Get(ObjectRules.Kind.Blast).color,out var sushi);
    Part(plate,material,"sushi-topping",PrimitiveType.Sphere,new Vector3(.16f,.33f,.10f),new Vector3(.78f,.10f,.45f),default(Vector3),sushi);
    // Fixed front rim stays readable while the plate/topping continue to turn.
    Part(story,material,"sushi-face-rim",PrimitiveType.Sphere,new Vector3(0,-.08f,-.50f),new Vector3(.85f,.34f,.10f));
    Face(story,material,kind,new Vector3(0,-.08f,-.565f),.60f);break;
   }
   case ObjectRules.Kind.Brake:{
    var mochi=Group(story,"mochi-soft",Vector3.zero);motion.soft=mochi.transform;
    Part(mochi,material,"mochi-white-body",PrimitiveType.Sphere,Vector3.zero,new Vector3(1.75f,1.15f,1.45f),default(Vector3),Color.white);
    var rim=Group(mochi,"mochi-purple-rim",Vector3.zero);rim.transform.localScale=new Vector3(1,.66f,1);
    Ring(rim,material,"mochi-glow-edge",.88f,.065f,-.48f);
    Face(mochi,material,kind,new Vector3(0,-.02f,-.74f),1.1f);
    for(int i=0;i<3;i++)Part(mochi,material,"mochi-sticky-strand-"+i,PrimitiveType.Sphere,new Vector3((i-1)*.43f,-.68f,-.2f),new Vector3(.065f,.46f+(i==1?.18f:0),.065f),default(Vector3),Color.white);
    break;
   }
   case ObjectRules.Kind.Blast:
    Part(story,material,"firework-shell",PrimitiveType.Sphere,new Vector3(0,-.15f,0),Vector3.one*1.6f);
    Part(story,material,"firework-fuse",PrimitiveType.Cylinder,new Vector3(.12f,.80f,0),new Vector3(.10f,.24f,.10f),new Vector3(0,0,-25));
    Part(story,material,"firework-lit-tip",PrimitiveType.Sphere,new Vector3(.22f,1.02f,0),Vector3.one*.14f);
    Face(story,material,kind,new Vector3(0,-.15f,-.82f),1.0f);
    motion.marks=new Transform[3];
    for(int i=0;i<3;i++)motion.marks[i]=Part(story,material,"fuse-spark-"+i,PrimitiveType.Sphere,new Vector3(.22f,1.02f,0),Vector3.one*.06f).transform;
    break;
   case ObjectRules.Kind.Death:{
    // A human and a separate open net: hat, body, two boots and grid silhouette.
    var fisher=Group(story,"fisher-person",new Vector3(-.5f,0,0));
    Part(fisher,material,"fisher-body",PrimitiveType.Capsule,new Vector3(0,-.12f,0),new Vector3(.5f,.35f,.42f));
    Part(fisher,material,"fisher-head",PrimitiveType.Sphere,new Vector3(0,.47f,0),Vector3.one*.38f,default(Vector3),Color.black);
    Part(fisher,material,"fisher-face-surface",PrimitiveType.Sphere,new Vector3(0,.47f,-.14f),new Vector3(.35f,.32f,.14f));
    Face(fisher,material,kind,new Vector3(0,.47f,-.225f),.40f);
    Part(fisher,material,"fisher-hat-crown",PrimitiveType.Sphere,new Vector3(0,.73f,0),new Vector3(.48f,.24f,.46f));
    Part(fisher,material,"fisher-hat-brim",PrimitiveType.Cylinder,new Vector3(0,.64f,0),new Vector3(.77f,.045f,.65f),default(Vector3),Color.black);
    for(int i=0;i<2;i++){
     float x=i==0?-.16f:.16f;
     Part(fisher,material,"fisher-boot-leg-"+i,PrimitiveType.Capsule,new Vector3(x,-.73f,0),new Vector3(.20f,.30f,.23f),default(Vector3),Color.black);
     Part(fisher,material,"fisher-boot-toe-"+i,PrimitiveType.Sphere,new Vector3(x+.055f,-.96f,-.08f),new Vector3(.30f,.18f,.40f),default(Vector3),Color.black);
    }
    Part(fisher,material,"fisher-net-arm",PrimitiveType.Sphere,new Vector3(.3f,.12f,0),new Vector3(.45f,.16f,.2f),new Vector3(0,0,25));
    Part(fisher,material,"fisher-free-arm",PrimitiveType.Capsule,new Vector3(-.31f,-.15f,0),new Vector3(.16f,.22f,.17f),new Vector3(0,0,-18));
    Part(story,material,"net-handle",PrimitiveType.Cylinder,new Vector3(.32f,.02f,0),new Vector3(.07f,.78f,.07f),new Vector3(0,0,-20),Color.black);
    var net=Group(story,"fisher-net",new Vector3(.51f,1.05f,0));motion.SetNet(net.transform);
    Ring(net,material,"net-hoop",.55f,.065f,0);Grid(net,material,"net-grid",.51f,.035f,0);break;
   }
  }
  return root;
 }
 public const string FaceInk="#241E2E";
 static void Face(GameObject parent,Material material,ObjectRules.Kind kind,Vector3 position,float scale){
  var face=Group(parent,"face-"+kind.ToString().ToLowerInvariant(),position);
  face.transform.localScale=Vector3.one*scale;
  ColorUtility.TryParseHtmlString(FaceInk,out var ink);
  // Two separate soft eyes; expressions differ by proportions and tilt only.
  float eyeY=kind==ObjectRules.Kind.Spin?.055f:kind==ObjectRules.Kind.Death?.10f:.16f;
  float eyeH=kind==ObjectRules.Kind.Spin?.06f:kind==ObjectRules.Kind.Bounce?.10f:.18f;
  float eyeW=kind==ObjectRules.Kind.Spin?.20f:kind==ObjectRules.Kind.Brake?.12f:.14f;
  float tilt=kind==ObjectRules.Kind.Bounce?22:kind==ObjectRules.Kind.Death?12:0;
  Ink(face,material,"face-eye-left",new Vector3(-.23f,eyeY,0),new Vector3(eyeW,eyeH,.065f),tilt,ink);
  Ink(face,material,"face-eye-right",new Vector3(.23f,eyeY,0),new Vector3(eyeW,eyeH,.065f),-tilt,ink);
  var mouth=Group(face,"face-mouth",new Vector3(0,-.14f,0));
  if(kind==ObjectRules.Kind.Brake){
   // A tiny soft w-mouth, the broad white mochi's only facial detail.
   Ink(mouth,material,"face-mouth-left",new Vector3(-.055f,0,0),new Vector3(.11f,.055f,.055f),-28,ink);
   Ink(mouth,material,"face-mouth-right",new Vector3(.055f,0,0),new Vector3(.11f,.055f,.055f),28,ink);
  }else if(kind==ObjectRules.Kind.Blast){
   Ink(mouth,material,"face-mouth-o",Vector3.zero,new Vector3(.15f,.20f,.06f),0,ink);
  }else{
   Ink(mouth,material,"face-mouth-smile",Vector3.zero,new Vector3(.20f,.055f,.055f),0,ink);
   if(kind==ObjectRules.Kind.Boost||kind==ObjectRules.Kind.Bounce){
    Ink(mouth,material,"face-mouth-corner-left",new Vector3(-.09f,.025f,0),new Vector3(.06f,.08f,.055f),-25,ink);
    Ink(mouth,material,"face-mouth-corner-right",new Vector3(.09f,.025f,0),new Vector3(.06f,.08f,.055f),25,ink);
   }
  }
 }
 static void Ink(GameObject root,Material material,string name,Vector3 p,Vector3 size,float tilt,Color ink){
  Part(root,material,name,PrimitiveType.Sphere,p,size,new Vector3(0,0,tilt),ink,true);
 }
 static GameObject Group(GameObject parent,string name,Vector3 position){
  var o=new GameObject(name);o.transform.SetParent(parent.transform,false);o.transform.localPosition=position;return o;
 }
 static void Ring(GameObject root,Material m,string name,float radius,float thickness,float z){
  const int sides=16;
  for(int i=0;i<sides;i++){
   float a=i*Mathf.PI*2/sides;
   Part(root,m,name+"-"+i,PrimitiveType.Sphere,new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,z),new Vector3(2*radius*Mathf.Sin(Mathf.PI/sides)+thickness,thickness,thickness),new Vector3(0,0,i*360f/sides+90));
  }
 }
 static void Grid(GameObject root,Material m,string name,float radius,float thickness,float z){
  for(int i=-2;i<=2;i++){
   float offset=i*radius/3,span=2*Mathf.Sqrt(radius*radius-offset*offset);
   Part(root,m,name+"-horizontal-"+i,PrimitiveType.Sphere,new Vector3(0,offset,z),new Vector3(span,thickness,thickness));
   Part(root,m,name+"-vertical-"+i,PrimitiveType.Sphere,new Vector3(offset,0,z),new Vector3(thickness,span,thickness));
  }
 }
 static GameObject Part(GameObject root,Material m,string name,PrimitiveType shape,Vector3 p,Vector3 s,Vector3 rotation=default(Vector3),Color? accent=null,bool ink=false){
  var o=GameObject.CreatePrimitive(shape);o.name=name;o.transform.SetParent(root.transform,false);o.transform.localPosition=p;o.transform.localScale=s;o.transform.localRotation=Quaternion.Euler(rotation.x,rotation.y,rotation.z);
  var c=o.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);var renderer=o.GetComponent<Renderer>();renderer.sharedMaterial=m;
  // All parts share the owned material; accents need no extra material lifetime.
  if(accent.HasValue){var block=new MaterialPropertyBlock();block.SetColor("_Color",accent.Value);block.SetColor("_EmissionColor",ink||accent.Value==Color.black?Color.black:accent.Value*ObjectRules.Emission);renderer.SetPropertyBlock(block);}
  return o;
 }
}
