using UnityEngine;
// Display-only material response. No extra pass, emission, x-ray or geometry changes.
public static class ReadableMaterials {
 public static void Actor(Transform root,bool on){foreach(var r in root.GetComponentsInChildren<MeshRenderer>()){var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);b.SetFloat("_ActorReadability",on?1:0);r.SetPropertyBlock(b);}}
 public static void Fragment(GameObject obj,string material,bool on){if(material=="metal"){Metal(obj,on);return;}if(material!="wood")return;foreach(var r in obj.GetComponentsInChildren<MeshRenderer>()){var m=r.sharedMaterial;if(!m)continue;var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);if(m.HasProperty("_FragmentReadability"))b.SetFloat("_FragmentReadability",on?.58f:0);else if(m.HasProperty("_Color")){var c=m.GetColor("_Color");float value=c.grayscale;b.SetColor("_Color",on?new Color(value*.68f,value*.78f,value*.84f,c.a):c);}r.SetPropertyBlock(b);}}
 public static void Metal(GameObject obj,bool on){foreach(var r in obj.GetComponentsInChildren<MeshRenderer>()){var m=r.sharedMaterial;if(!m)continue;var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);if(m.HasProperty("_MetalDebrisReadability"))b.SetFloat("_MetalDebrisReadability",on?.72f:0);else if(m.HasProperty("_Color")){var c=m.GetColor("_Color");b.SetColor("_Color",on?new Color(c.r*.61f,c.g*.71f,c.b*.76f,c.a):c);}r.SetPropertyBlock(b);}}
}
