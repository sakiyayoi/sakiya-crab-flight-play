using UnityEngine;
using C2=CrabCityDensityR2;

public sealed class PortAtmosphere:MonoBehaviour {
 public static void Build(Transform parent){
  if(!PortLayout.Enabled)return;
  var go=new GameObject("Port authored atmosphere +x only");go.transform.SetParent(parent,false);go.AddComponent<PortAtmosphere>();var owner=go.AddComponent<C2.CityDensityOwnedAssets>();
  var sky=PortVisuals.MeshObject("Port two layer baked harbor cards",go.transform,Vector3.zero,Vector3.one,new[]{new Vector3(600,-35,7.8f),new Vector3(900,-35,7.8f),new Vector3(900,180,7.8f),new Vector3(600,180,7.8f)},new[]{0,2,1,0,3,2},Color.white);
  var r=sky.GetComponent<Renderer>();NeonProto.Release(r.sharedMaterial);var m=new Material(Resources.Load<Shader>("PortPrototype/Atmosphere"));m.SetTexture("_Far",Resources.Load<Texture2D>("PortPrototype/harbor-far"));m.SetTexture("_Mid",Resources.Load<Texture2D>("PortPrototype/harbor-mid"));r.sharedMaterial=m;owner.materials.Add(m);owner.meshes.Add(sky.GetComponent<MeshFilter>().sharedMesh);
  var ground=PortVisuals.Box("Port wet asphalt",go.transform,new Vector3(750,-.025f,0),new Vector3(300,.035f,16),new Color(.14510f,.16863f,.27059f));Own(ground,owner);
  // Waterfront inlets under the authored buoy pockets. Render-only, with the
  // existing ground collision still authoritative; no new water physics.
  foreach(var span in new[]{new Vector2(615,632),new Vector2(668,698),new Vector2(762,782),new Vector2(856,900)}){
   Own(PortVisuals.Box("Port quiet water inlet",go.transform,new Vector3((span.x+span.y)*.5f,-.005f,-5.6f),new Vector3(span.y-span.x,.002f,4.3f),new Color(.18f,.25f,.36f)),owner);
  }
  // Quiet waterfront seams and two market paper notices: scenery, never registrations.
  foreach(float x in new[]{639.3f,802.7f}){
   var notice=PortVisuals.Box("Port market delivery note",go.transform,new Vector3(x,1.1f,3.1f),new Vector3(.42f,.55f,.035f),new Color(1,.953f,.839f));Own(notice,owner);
   var post=PortVisuals.Box("Port notice timber post",go.transform,new Vector3(x,.6f,3.13f),new Vector3(.08f,1.2f,.08f),PortVisuals.Ink);Own(post,owner);
  }
 }
 static void Own(GameObject o,C2.CityDensityOwnedAssets owner){owner.meshes.Add(o.GetComponent<MeshFilter>().sharedMesh);owner.materials.Add(o.GetComponent<Renderer>().sharedMaterial);}
}
