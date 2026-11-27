using UnityEngine;

// Story silhouettes only: the caller retains the Stage 1 root, collider and motion.
public static class ObjectVisual {
 public sealed class Motion {
  public Transform rotor;public bool plate;
  public void Pose(float time,bool reduced){
   if(rotor==null)return;
   float angle=reduced?0:time*(plate?180:720);
   rotor.localRotation=plate?Quaternion.Euler(0,angle,0):Quaternion.Euler(0,0,angle);
  }
 }
 public static GameObject Build(ObjectRules.Kind kind,out Material material,out Motion motion){
  var root=new GameObject("Action "+kind);var def=ObjectRules.Get(kind);motion=new Motion();
  ColorUtility.TryParseHtmlString(def.color,out var color);
  material=new Material(Shader.Find("Standard"));material.color=color;
  material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*ObjectRules.Emission);
  material.SetFloat("_Glossiness",.35f);
  var story=Group(root,def.shape,Vector3.zero);
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
     Part(blades,material,"fan-blade-"+i,PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.34f,Mathf.Sin(a)*.34f,0),new Vector3(.6f,.18f,.1f),new Vector3(0,0,i*90+20));
    }
    Part(head,material,"fan-hub",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.23f);break;
   }
   case ObjectRules.Kind.Bounce:{
    var pan=Group(story,"tilted-frying-pan",Vector3.zero);pan.transform.localRotation=Quaternion.Euler(0,0,-12);
    Part(pan,material,"pan-disc",PrimitiveType.Cylinder,new Vector3(-.3f,0,0),new Vector3(1.4f,.09f,1.2f),new Vector3(55,0,0));
    Part(pan,material,"pan-handle",PrimitiveType.Cube,new Vector3(.67f,0,0),new Vector3(.9f,.18f,.22f));break;
   }
   case ObjectRules.Kind.Spin:{
    Part(story,material,"sushi-belt",PrimitiveType.Cube,new Vector3(0,-.35f,0),new Vector3(2,.25f,1.25f));
    for(int i=0;i<5;i++)Part(story,material,"belt-slat-"+i,PrimitiveType.Cube,new Vector3((i-2)*.37f,-.19f,0),new Vector3(.08f,.06f,1.2f));
    var plate=Group(story,"sushi-plate-rotor",new Vector3(0,-.08f,0));motion.rotor=plate.transform;motion.plate=true;
    Part(plate,material,"sushi-plate",PrimitiveType.Cylinder,Vector3.zero,new Vector3(1.45f,.07f,1.05f));
    Part(plate,material,"sushi-rice",PrimitiveType.Cube,new Vector3(0,.19f,0),new Vector3(.7f,.2f,.4f),default(Vector3),Color.white);
    // Topping reuses the existing orange action token; crab red stays reserved.
    ColorUtility.TryParseHtmlString(ObjectRules.Get(ObjectRules.Kind.Blast).color,out var sushi);
    Part(plate,material,"sushi-topping",PrimitiveType.Cube,new Vector3(0,.33f,0),new Vector3(.78f,.10f,.45f),default(Vector3),sushi);break;
   }
   case ObjectRules.Kind.Brake:{
    Part(story,material,"mochi-white-body",PrimitiveType.Sphere,Vector3.zero,new Vector3(1.75f,1.15f,1.45f),default(Vector3),Color.white);
    var rim=Group(story,"mochi-purple-rim",Vector3.zero);rim.transform.localScale=new Vector3(1,.66f,1);
    Ring(rim,material,"mochi-glow-edge",.88f,.065f,-.48f);break;
   }
   case ObjectRules.Kind.Blast:
    Part(story,material,"firework-shell",PrimitiveType.Sphere,new Vector3(0,-.15f,0),Vector3.one*1.6f);
    Part(story,material,"firework-fuse",PrimitiveType.Cylinder,new Vector3(.12f,.80f,0),new Vector3(.10f,.24f,.10f),new Vector3(0,0,-25));
    Part(story,material,"firework-lit-tip",PrimitiveType.Sphere,new Vector3(.22f,1.02f,0),Vector3.one*.14f);break;
   case ObjectRules.Kind.Death:{
    // A human and a separate open net: hat, body, two boots and grid silhouette.
    var fisher=Group(story,"fisher-person",new Vector3(-.5f,0,0));
    Part(fisher,material,"fisher-body",PrimitiveType.Capsule,new Vector3(0,-.12f,0),new Vector3(.5f,.35f,.42f));
    Part(fisher,material,"fisher-head",PrimitiveType.Sphere,new Vector3(0,.47f,0),Vector3.one*.38f,default(Vector3),Color.black);
    Part(fisher,material,"fisher-hat-crown",PrimitiveType.Cylinder,new Vector3(0,.73f,0),new Vector3(.48f,.12f,.46f));
    Part(fisher,material,"fisher-hat-brim",PrimitiveType.Cylinder,new Vector3(0,.64f,0),new Vector3(.77f,.045f,.65f),default(Vector3),Color.black);
    for(int i=0;i<2;i++){
     float x=i==0?-.16f:.16f;
     Part(fisher,material,"fisher-boot-leg-"+i,PrimitiveType.Cylinder,new Vector3(x,-.73f,0),new Vector3(.20f,.25f,.23f),default(Vector3),Color.black);
     Part(fisher,material,"fisher-boot-toe-"+i,PrimitiveType.Cube,new Vector3(x+.055f,-.96f,-.08f),new Vector3(.30f,.18f,.40f),default(Vector3),Color.black);
    }
    Part(fisher,material,"fisher-net-arm",PrimitiveType.Cube,new Vector3(.3f,.12f,0),new Vector3(.45f,.16f,.2f),new Vector3(0,0,25));
    Part(story,material,"net-handle",PrimitiveType.Cylinder,new Vector3(.32f,-.30f,0),new Vector3(.07f,.55f,.07f),new Vector3(0,0,-20),Color.black);
    var net=Group(story,"fisher-net",new Vector3(.51f,.50f,0));
    Ring(net,material,"net-hoop",.55f,.065f,0);Grid(net,material,"net-grid",.51f,.035f,0);break;
   }
  }
  return root;
 }
 static GameObject Group(GameObject parent,string name,Vector3 position){
  var o=new GameObject(name);o.transform.SetParent(parent.transform,false);o.transform.localPosition=position;return o;
 }
 static void Ring(GameObject root,Material m,string name,float radius,float thickness,float z){
  const int sides=16;
  for(int i=0;i<sides;i++){
   float a=i*Mathf.PI*2/sides;
   Part(root,m,name+"-"+i,PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,z),new Vector3(2*radius*Mathf.Sin(Mathf.PI/sides)+thickness,thickness,thickness),new Vector3(0,0,i*360f/sides+90));
  }
 }
 static void Grid(GameObject root,Material m,string name,float radius,float thickness,float z){
  for(int i=-2;i<=2;i++){
   float offset=i*radius/3,span=2*Mathf.Sqrt(radius*radius-offset*offset);
   Part(root,m,name+"-horizontal-"+i,PrimitiveType.Cube,new Vector3(0,offset,z),new Vector3(span,thickness,thickness));
   Part(root,m,name+"-vertical-"+i,PrimitiveType.Cube,new Vector3(offset,0,z),new Vector3(thickness,span,thickness));
  }
 }
 static void Part(GameObject root,Material m,string name,PrimitiveType shape,Vector3 p,Vector3 s,Vector3 rotation=default(Vector3),Color? accent=null){
  var o=GameObject.CreatePrimitive(shape);o.name=name;o.transform.SetParent(root.transform,false);o.transform.localPosition=p;o.transform.localScale=s;o.transform.localRotation=Quaternion.Euler(rotation.x,rotation.y,rotation.z);
  var c=o.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);var renderer=o.GetComponent<Renderer>();renderer.sharedMaterial=m;
  // All parts share the owned material; accents need no extra material lifetime.
  if(accent.HasValue){var block=new MaterialPropertyBlock();block.SetColor("_Color",accent.Value);block.SetColor("_EmissionColor",accent.Value==Color.black?Color.black:accent.Value*ObjectRules.Emission);renderer.SetPropertyBlock(block);}
 }
}
