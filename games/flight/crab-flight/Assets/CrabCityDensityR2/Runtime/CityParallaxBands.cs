using UnityEngine;
namespace CrabCityDensityR2 {
 // Opt-in visual compensation for the host's orthographic camera.
 // Only the named decorative renderer bands move. Breakable roots remain world anchored.
 public sealed class CityParallaxBands : MonoBehaviour {
  public Camera target;
  public float midOffsetFraction=.08f,farOffsetFraction=.28f;
  Transform mid,far;Vector3 midRest,farRest;
  public static CityParallaxBands Attach(GameObject cityRoot,Camera camera){
   if(!cityRoot||!camera)throw new System.ArgumentNullException("cityRoot/camera");
   var component=cityRoot.GetComponent<CityParallaxBands>();if(!component)component=cityRoot.AddComponent<CityParallaxBands>();
   component.target=camera;component.mid=cityRoot.transform.Find("DecorativeMid");component.far=cityRoot.transform.Find("DecorativeFar");
   component.midRest=Vector3.zero;component.farRest=Vector3.zero;component.Apply();return component;
  }
  public void Apply(){
   if(!target)return;
   // Derived from the host's X20..35 reveal. Absolute camera X, no integration/drift.
   float x=Mathf.Clamp(target.transform.position.x,-400,400);
   float reveal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(20,35,Mathf.Abs(x)));
   if(mid)mid.localPosition=midRest+Vector3.right*(x*midOffsetFraction*reveal);
   if(far)far.localPosition=farRest+Vector3.right*(x*farOffsetFraction*reveal);
  }
  void LateUpdate(){Apply();}
  void OnDisable(){if(mid)mid.localPosition=midRest;if(far)far.localPosition=farRest;}
 }
}
