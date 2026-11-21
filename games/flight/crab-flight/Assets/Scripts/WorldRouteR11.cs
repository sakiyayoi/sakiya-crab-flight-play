using System;using UnityEngine;using UnityEngine.Rendering;
// Authorial whole-route layout, still a graybox. Existing real contacts/destruction/finite rotor are reused.
public static class WorldRouteR11 {
 public const float WorldLimit=3300;
 [Serializable]public class Sector{public string id,label;public float start,end,viewX,viewY,viewSize;public bool placeholder;}
 [Serializable]public class Recipe{public string revision;public float worldLimit;public Sector[] sectors;}
 static Recipe data;public static Recipe Data=>data??(data=JsonUtility.FromJson<Recipe>(Resources.Load<TextAsset>("WorldRouteR11").text));
 public static string RegionAt(float x){foreach(var s in Data.sectors)if(Mathf.Abs(x)<s.end)return s.id;return "orbit";}
 public static GameObject BuildBackdrop(Transform parent,Shader shader,bool extendedBackdrop=false){
  var root=new GameObject("R11 all-section graybox");root.transform.SetParent(parent,false);var owned=root.AddComponent<FarWorldR10Owned>();
  Color[] colors={new Color(.24f,.34f,.27f),new Color(.43f,.49f,.48f),new Color(.62f,.66f,.64f),new Color(.12f,.19f,.30f)};var mats=new Material[4];for(int i=0;i<4;i++){mats[i]=new Material(shader){color=colors[i]};owned.assets.Add(mats[i]);}
  Action<string,Vector3,Vector3,int> box=(name,p,s,m)=>{var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(root.transform,false);o.transform.localPosition=p;o.transform.localScale=s;var c=o.GetComponent<Collider>();c.enabled=false;UnityEngine.Object.Destroy(c);var r=o.GetComponent<Renderer>();r.sharedMaterial=mats[m];r.shadowCastingMode=ShadowCastingMode.Off;};
  foreach(float sign in new[]{-1f,1f}){
   box("route ground",new Vector3(sign*(extendedBackdrop?2300:2450),-.2f,extendedBackdrop?110:8),new Vector3(extendedBackdrop?2000:1700,.3f,extendedBackdrop?230:26),1);
   for(int i=0;i<6;i++){float x=1360+i*116;var hill=GameObject.CreatePrimitive(PrimitiveType.Sphere);hill.name="country ridge graybox";hill.transform.SetParent(root.transform,false);hill.transform.localPosition=new Vector3(sign*x,-4,65);hill.transform.localScale=new Vector3(155,35+i%3*12,45);var hc=hill.GetComponent<Collider>();hc.enabled=false;UnityEngine.Object.Destroy(hc);hill.GetComponent<Renderer>().sharedMaterial=mats[0];box("country pylon",new Vector3(sign*x,11,15),new Vector3(1,22,1.5f),2);box("pylon arm",new Vector3(sign*x,22,15),new Vector3(12,.55f,1),2);}
   for(int i=0;i<4;i++){float x=2120+i*150;box("range gantry",new Vector3(sign*x,20,22),new Vector3(3,40,3),1);box("range arm",new Vector3(sign*(x+16),39,22),new Vector3(35,2,3),2);box("range silo",new Vector3(sign*(x+65),13,38),new Vector3(18,26,18),2);}
   box("orbital horizon",new Vector3(sign*2980,105,150),new Vector3(650,260,5),3);
   for(int i=0;i<3;i++){float x=2760+i*170;box("orbital hub",new Vector3(sign*x,22,18),new Vector3(5,5,4),2);box("orbital wings",new Vector3(sign*x,22,18),new Vector3(34,.8f,10),1);}
  }return root;
 }
}
public sealed partial class CrabFlightGame{public bool useRouteBlockoutR11=true;}
public sealed partial class RigidCrabFlight{
 public bool UseRouteBlockoutR11;bool routeFloorReadyR11;
 public void EnableRouteFloorR11(){if(!UseRouteBlockoutR11||routeFloorReadyR11)return;routeFloorReadyR11=true;var mat=Material("R11 extended sand",.56f,.42f,.06f);foreach(float side in new[]{-1f,1f})Floor("sand",new Vector3(side*2450,-.25f,0),new Vector3(1700,.5f,20),mat);}
#if UNITY_EDITOR
 // Isolated contact fixture, explicitly not an ordinary player launch or reach proof.
 public string ReviewRouteSegmentR11(string id){var p=panels.Find(x=>x.id==id);Body.rotation=Quaternion.Euler(0,90,0);root.rotation=Body.rotation;var point=p.shapes[0].bounds.center;Body.position=point-new Vector3(direction*8,0,0);Body.position=new Vector3(Body.position.x,Mathf.Max(3,Body.position.y),0);root.position=Body.position;UnityEngine.Physics.SyncTransforms();lockedComZ=Body.worldCenterOfMass.z;Body.linearVelocity=new Vector3(direction*45,0,0);Body.angularVelocity=new Vector3(0,.28f,-.96f).normalized*(direction*8*Mathf.PI*2);Body.WakeUp();return id;}
#endif
}
