#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.Linq;using UnityEngine;using UnityEngine.SceneManagement;using Object=UnityEngine.Object;
public sealed partial class FlightMeasurementPlayMode {
 sealed class RenderIsolation:IDisposable {readonly Renderer[] renderers;readonly Light[] lights;readonly Camera[] cameras;
 public RenderIsolation(Scene game,Scene physical){Func<Scene,bool> outside=s=>s!=game&&s!=physical;renderers=Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(x=>x.enabled&&outside(x.gameObject.scene)).ToArray();lights=Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(x=>x.enabled&&outside(x.gameObject.scene)).ToArray();cameras=Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(x=>x.enabled&&outside(x.gameObject.scene)).ToArray();foreach(var x in renderers)x.enabled=false;foreach(var x in lights)x.enabled=false;foreach(var x in cameras)x.enabled=false;}
 public void Dispose(){foreach(var x in renderers)if(x)x.enabled=true;foreach(var x in lights)if(x)x.enabled=true;foreach(var x in cameras)if(x)x.enabled=true;}}
}
#endif
