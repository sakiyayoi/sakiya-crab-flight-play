using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using CrabCityDensity;

public sealed partial class RigidCrabFlight {
 void EnableCityContacts(){
  var geometry=JsonUtility.FromJson<CityDensityPack.Geometry>(Resources.Load<TextAsset>("CityDensity/CityGeometry").text);
  var layout=JsonUtility.FromJson<CityDensityPack.Layout>(Resources.Load<TextAsset>("CityDensity/CityContactProposals").text);
  var used=layout.instances.Select(x=>x.asset).Distinct().ToArray();geometry.assets=geometry.assets.Where(x=>used.Contains(x.id)).ToArray();
  var geometryAsset=new TextAsset(JsonUtility.ToJson(geometry));var layoutAsset=new TextAsset(JsonUtility.ToJson(layout));var root=CityDensityPack.Build(geometryAsset,layoutAsset,Shader.Find("Standard"));Release(geometryAsset);Release(layoutAsset);SceneManager.MoveGameObjectToScene(root,Scene);
  foreach(var visual in root.GetComponentsInChildren<CityVisualBreakable>()){
   string type=visual.assetId=="market_crate"?"wood":"metal";
   var panel=new BreakPanel{id=visual.placementId,type=type,position=visual.transform.position,size=visual.suggestedEnvelopeSize,threshold=type=="wood"?.65f:1.05f,visual=visual.gameObject,city=visual};
   AddPanelShape(panel,visual.suggestedEnvelopeCenter,visual.suggestedEnvelopeSize);panels.Add(panel);
  }
 }
 void SpawnCityShards(BreakPanel p,Vector3 contact){
  foreach(var pivot in p.city.shardPivots){
   if(fragments.Count>=48||CityFragments>=12){pivot.gameObject.SetActive(false);continue;}
   var obj=UnityEngine.Object.Instantiate(pivot.gameObject,pivot.position,pivot.rotation);obj.name="City physical fragment "+p.id;SceneManager.MoveGameObjectToScene(obj,FragmentSceneCalmR4);
   bool first=true;var bounds=new Bounds();foreach(var renderer in obj.GetComponentsInChildren<Renderer>()){if(first){bounds=renderer.bounds;first=false;}else bounds.Encapsulate(renderer.bounds);}
   var c=obj.AddComponent<BoxCollider>();c.center=obj.transform.InverseTransformPoint(bounds.center);c.size=Vector3.Max(Vector3.one*.04f,bounds.size);c.sharedMaterial=debrisMaterial;c.contactOffset=.005f;
   if(!SeparateDebrisCalmR4)foreach(var crabCollider in colliders)UnityEngine.Physics.IgnoreCollision(c,crabCollider,true);
   var rb=obj.AddComponent<Rigidbody>();rb.mass=Mathf.Clamp(p.city.suggestedTotalMassKg/Mathf.Max(1,p.city.shardPivots.Length),.05f,3);rb.linearDamping=.3f;rb.angularDamping=.6f;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
   rb.linearVelocity=Vector3.ClampMagnitude(incoming*.22f+new Vector3(direction*1.3f,1.1f,CityFragments%2==0?-1.4f:1.4f),18);rb.angularVelocity=new Vector3(1,2,CityFragments%2==0?-2:2);fragments.Add(obj);CityFragments++;pivot.gameObject.SetActive(false);
  }
 }
#if UNITY_EDITOR
 public void ReviewAddSimultaneousPanels(){AddPanel("probe-wood","wood",new Vector3(4.5f,3.25f,0),new Vector3(.13f,6.5f,5.2f),Color.gray,.01f);AddPanel("probe-metal","metal",new Vector3(4.5f,3.25f,0),new Vector3(.13f,6.5f,5.2f),Color.gray,.01f);}
#endif
}
