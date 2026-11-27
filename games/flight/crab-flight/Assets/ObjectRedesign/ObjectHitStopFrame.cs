using UnityEngine;

// Built-in rendering pipeline: hold the final camera image, not any Transform,
// Rigidbody, physics dt, input, VFX parameters or the global time scale.
// UI/audio and the fixed-step simulation continue behind this presentation frame.
[RequireComponent(typeof(Camera))]
public sealed class ObjectHitStopFrame : MonoBehaviour {
 readonly ObjectHitStopRules.Window window=new ObjectHitStopRules.Window();
 RenderTexture held;
 CrabFlightGame owner;
 public void Bind(CrabFlightGame game){owner=game;}
 public bool Holding=>window.Active(Time.realtimeSinceStartupAsDouble);
 public void Request(ObjectRules.Kind kind){window.Request(ObjectHitStopRules.Seconds(kind));}
 void Update(){if(!owner||!owner.ObjectHitStopEnabled)Clear();}
 void OnRenderImage(RenderTexture source,RenderTexture destination){
  if(!owner||!owner.ObjectHitStopEnabled){Clear();Graphics.Blit(source,destination);return;}
  double now=Time.realtimeSinceStartupAsDouble;
  if(!window.Active(now)){Graphics.Blit(source,destination);return;}
  if(!held||!held.IsCreated()||held.width!=source.width||held.height!=source.height||held.graphicsFormat!=source.graphicsFormat){
   // Preserve the source colour format/space. No tone, light or saturation edit.
   var descriptor=source.descriptor;descriptor.depthBufferBits=0;descriptor.msaaSamples=1;descriptor.useMipMap=false;descriptor.autoGenerateMips=false;
   ReleaseFrame();held=new RenderTexture(descriptor){name="Object impact held frame",filterMode=FilterMode.Bilinear};
   held.Create();Graphics.Blit(source,held);
  }
  if(window.NeedsCapture){Graphics.Blit(source,held);window.Captured(now);}
  Graphics.Blit(held,destination);
 }
 public void Clear(){window.Clear();ReleaseFrame();}
 void ReleaseFrame(){if(!held)return;held.Release();Destroy(held);held=null;}
 void OnDisable(){Clear();}
 void OnDestroy(){Clear();}
}
