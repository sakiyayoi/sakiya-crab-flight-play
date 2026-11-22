using System.Collections.Generic;
using UnityEngine;

// Independent render objects: never attach colliders or change the physical crab.
public sealed class PortHeroVisual {
 readonly Camera camera;readonly Transform body;readonly Transform root;
 readonly List<GameObject> cards=new List<GameObject>();readonly Material hero;
 readonly List<Renderer> shells=new List<Renderer>();readonly List<Material[]> previous=new List<Material[]>();
 readonly Transform[] eyes=new Transform[6];readonly Transform water,shadow;
 bool active;
 public PortHeroVisual(Transform crab,Transform core,Camera cam,Transform owner){
  camera=cam;body=core;root=crab;hero=new Material(Resources.Load<Shader>("PortPrototype/Hero")){color=NeonProto.CrabRed};
  foreach(var r in crab.GetComponentsInChildren<MeshRenderer>()){shells.Add(r);previous.Add(r.sharedMaterials);}
  for(int i=0;i<6;i++){
   var o=PortVisuals.Disc("Port readable eye "+i,owner,Vector3.zero,Vector3.one,i%3==0?PortVisuals.Ink:i%3==1?Color.white:PortVisuals.Ink);cards.Add(o);eyes[i]=o.transform;
  }
  var w=PortVisuals.MeshObject("Port aquarium water and bubbles",owner,Vector3.zero,Vector3.one,new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)},new[]{0,2,1,0,3,2},Color.white);
  NeonProto.Release(w.GetComponent<Renderer>().sharedMaterial);w.GetComponent<Renderer>().sharedMaterial=new Material(Resources.Load<Shader>("PortPrototype/Aquarium"));cards.Add(w);water=w.transform;
  var s=PortVisuals.Disc("Port underwater shadow",owner,Vector3.zero,Vector3.one,new Color(.10f,.22f,.30f));cards.Add(s);shadow=s.transform;
 }
 public bool Sample(string phase,bool aiming){
  bool tank=phase=="ready"||phase=="charge"||aiming,on=PortLayout.Enabled&&(tank||PortLayout.InPort(root.position.x));
  if(active!=on){active=on;for(int i=0;i<shells.Count;i++){if(on){var mats=new Material[previous[i].Length];for(int j=0;j<mats.Length;j++)mats[j]=hero;shells[i].sharedMaterials=mats;}else shells[i].sharedMaterials=previous[i];}}
  foreach(var e in eyes)e.gameObject.SetActive(on);
  water.gameObject.SetActive(on&&tank);shadow.gameObject.SetActive(on&&tank);
  if(!on)return false;
  // A minimum screen-space face survives a wide flight camera, capped at a modest 1.5x.
  float scale=Mathf.Clamp(camera.orthographicSize/12,.8f,1.5f);
  for(int i=0;i<eyes.Length;i++){
   int layer=i%3;float radius=(layer==0?.17f:layer==1?.135f:.066f)*scale;
   eyes[i].position=body.position-camera.transform.forward*(.88f+layer*.008f)+camera.transform.right*((i<3?-.19f:.19f)*scale+(layer==2?.025f:0))+camera.transform.up*(.17f*scale);
   eyes[i].rotation=camera.transform.rotation*Quaternion.Euler(90,0,0);eyes[i].localScale=new Vector3(radius,1,radius*1.15f);
  }
  water.position=body.position+camera.transform.forward*4;water.rotation=camera.transform.rotation;water.localScale=new Vector3(camera.orthographicSize*camera.aspect*2.1f,camera.orthographicSize*2.1f,1);
  shadow.position=body.position+camera.transform.forward*.7f-camera.transform.up*.7f;shadow.rotation=camera.transform.rotation*Quaternion.Euler(90,0,0);shadow.localScale=new Vector3(1.3f,1,.34f);
  return true;
 }
 public void Dispose(){foreach(var o in cards){NeonProto.Release(o.GetComponent<MeshFilter>().sharedMesh);NeonProto.Release(o.GetComponent<Renderer>().sharedMaterial);NeonProto.Release(o);}NeonProto.Release(hero);}
}
