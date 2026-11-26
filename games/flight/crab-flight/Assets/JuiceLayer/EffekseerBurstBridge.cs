using System.Collections.Generic;
using Effekseer;
using UnityEngine;

// Owns only the visual burst handles. The package owns its shared runtime/renderer.
public sealed class EffekseerBurstBridge : MonoBehaviour {
 public const string ResourcePath="Effekseer/ToonHit/ToonHit";
 public const int ActiveLimit=24;
 public const float BurstScale=1.0f;
 struct Playing { public EffekseerHandle handle; public bool priority; }
 readonly List<Playing> playing=new List<Playing>(ActiveLimit);
 EffekseerEffectAsset effectAsset;
 bool warned;
 public int ActiveCount { get { Prune();return playing.Count; } }
 public bool Initialize(){
  Clear();effectAsset=null;warned=false;
  // Feel configuration also runs outside play mode during native review.
  // Load the asset here; defer runtime activation/creation until a real play.
  effectAsset=Resources.Load<EffekseerEffectAsset>(ResourcePath);
  if(!effectAsset){Warn("ToonHit.asset is missing. Import ToonHit.efkefc and Parts before building.");return false;}
  return true;
 }
 bool EnsureRuntime(){
  // 1.80.8 normally creates EffekseerRuntime BeforeSceneLoad, including the
  // Native renderer's Camera.onPreCull hook. Do not add a second system/camera.
  if(!EffekseerSystem.IsValid){
   var runtime=Object.FindFirstObjectByType<EffekseerRuntime>(FindObjectsInactive.Include);
   if(runtime){runtime.gameObject.SetActive(true);runtime.enabled=true;}
   else if(EffekseerSystem.Instance==null){
    EffekseerSettings.Instance.RendererType=EffekseerRendererType.Native;
    var host=new GameObject("Effekseer (juice fallback)");
    host.AddComponent<EffekseerRuntime>();
    Object.DontDestroyOnLoad(host);
   }
  }
  if(!EffekseerSystem.IsValid||EffekseerSystem.Instance.renderer==null||
     EffekseerSettings.Instance.RendererType!=EffekseerRendererType.Native){
   Warn("Native runtime/renderer is unavailable.");return false;
  }
  return true;
 }
 public bool Play(Vector3 position,bool priority){
  if(!Application.isPlaying)return false;
  Prune();
  if(!effectAsset){Warn("Burst is unavailable; check package injection and imported resources.");return false;}
  if(!EnsureRuntime())return false;
  if(playing.Count>=ActiveLimit){
   if(!priority)return false;
   // Always admit a peak, retaining a bounded list: oldest ordinary burst first,
   // otherwise oldest peak. No global StopAllEffects or particle-count proxy.
   int victim=playing.FindIndex(p=>!p.priority);if(victim<0)victim=0;
   playing[victim].handle.Stop();playing.RemoveAt(victim);
  }
  var handle=EffekseerSystem.PlayEffect(effectAsset,position);
  if(!handle.exists){Warn("PlayEffect returned an inactive handle.");return false;}
  handle.SetScale(Vector3.one*BurstScale);
  handle.TimeScale=EffekseerTimeScale.Unscale;
  playing.Add(new Playing{handle=handle,priority=priority});return true;
 }
 void Update(){Prune();}
 void Prune(){
  if(!EffekseerSystem.IsValid){playing.Clear();return;}
  for(int i=playing.Count-1;i>=0;i--)if(!playing[i].handle.exists)playing.RemoveAt(i);
 }
 public void Clear(){
  if(EffekseerSystem.IsValid)for(int i=0;i<playing.Count;i++)if(playing[i].handle.exists)playing[i].handle.Stop();
  playing.Clear();
 }
 void OnDisable(){Clear();}
 void OnDestroy(){Clear();}
 void Warn(string message){if(warned)return;warned=true;Debug.LogWarning("[Juice Effekseer] "+message,this);}
}
