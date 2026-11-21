using System.Collections.Generic;
using UnityEngine;
namespace CrabCityDensityR2 {
 // Owns only this builder's transient meshes/materials. Host objects and imported assets are untouched.
 public sealed class CityDensityOwnedAssets : MonoBehaviour {
  [System.NonSerialized]public List<Mesh> meshes=new List<Mesh>();
  [System.NonSerialized]public List<Material> materials=new List<Material>();
  void OnDestroy(){foreach(var m in meshes)if(m)Release(m);foreach(var m in materials)if(m)Release(m);}
  static void Release(Object o){if(Application.isPlaying)Object.Destroy(o);else Object.DestroyImmediate(o);}
 }
}
