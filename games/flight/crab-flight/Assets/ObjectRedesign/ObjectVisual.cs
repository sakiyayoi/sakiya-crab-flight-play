using UnityEngine;

// Runtime-only shapes. Red is reserved for the crab; death adds black spikes.
public static class ObjectVisual {
 public static GameObject Build(ObjectRules.Kind kind,out Material material){
  var root=new GameObject("Action "+kind);var def=ObjectRules.Get(kind);
  ColorUtility.TryParseHtmlString(def.color,out var color);
  material=new Material(Shader.Find("Standard"));material.color=color;
  material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*ObjectRules.Emission);
  material.SetFloat("_Glossiness",.35f);
  switch(kind){
   case ObjectRules.Kind.Boost:
    Part(root,material,PrimitiveType.Cube,new Vector3(-.25f,0,0),new Vector3(1.2f,.32f,.6f),0);
    Part(root,material,PrimitiveType.Cube,new Vector3(.45f,.3f,0),new Vector3(.85f,.3f,.6f),-45);
    Part(root,material,PrimitiveType.Cube,new Vector3(.45f,-.3f,0),new Vector3(.85f,.3f,.6f),45);break;
   case ObjectRules.Kind.Bounce:
    Part(root,material,PrimitiveType.Cube,new Vector3(0,.55f,0),new Vector3(1.9f,.22f,1.4f),0);
    for(int i=0;i<4;i++)Part(root,material,PrimitiveType.Cube,new Vector3(0,-.55f+i*.26f,0),new Vector3(.9f,.12f,.65f),i%2==0?22:-22);break;
   case ObjectRules.Kind.Spin:
    Part(root,material,PrimitiveType.Sphere,Vector3.zero,Vector3.one*.5f,0);
    for(int i=0;i<3;i++){float a=i*120*Mathf.Deg2Rad;Part(root,material,PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.48f,Mathf.Sin(a)*.48f,0),new Vector3(1,.28f,.5f),i*120+20);}break;
   case ObjectRules.Kind.Brake:
    for(int i=0;i<3;i++)Part(root,material,PrimitiveType.Sphere,new Vector3((i-1)*.52f,i==1?.15f:0,0),Vector3.one*(i==1?1.2f:.9f),0);break;
   case ObjectRules.Kind.Blast:
    Part(root,material,PrimitiveType.Cylinder,Vector3.zero,new Vector3(1.25f,.75f,1.25f),0);
    Part(root,material,PrimitiveType.Cube,new Vector3(0,.88f,0),new Vector3(.18f,.4f,.18f),25);break;
   case ObjectRules.Kind.Death:
    Part(root,material,PrimitiveType.Sphere,Vector3.zero,Vector3.one*1.1f,0);
    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Part(root,material,PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.7f,Mathf.Sin(a)*.7f,0),new Vector3(.72f,.23f,.5f),i*45,i%2==0);}break;
  }
  return root;
 }
 static void Part(GameObject root,Material m,PrimitiveType shape,Vector3 p,Vector3 s,float angle,bool black=false){
  var o=GameObject.CreatePrimitive(shape);o.transform.SetParent(root.transform,false);o.transform.localPosition=p;o.transform.localScale=s;o.transform.localRotation=Quaternion.Euler(0,0,angle);
  var c=o.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);var renderer=o.GetComponent<Renderer>();renderer.sharedMaterial=m;
  if(black){var block=new MaterialPropertyBlock();block.SetColor("_Color",Color.black);block.SetColor("_EmissionColor",Color.black);renderer.SetPropertyBlock(block);}
 }
}
