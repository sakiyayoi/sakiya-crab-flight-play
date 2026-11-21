using System.Collections.Generic;using UnityEngine;
// Called exclusively inside the existing master-gated FlashObjectR27, then restored
// by the existing per-render-frame flash lifecycle. No private timer or extra gate.
public sealed partial class CrabFlightGame {
 readonly Dictionary<SkinnedMeshRenderer,MaterialPropertyBlock> lifeFlashBeforeR6=new Dictionary<SkinnedMeshRenderer,MaterialPropertyBlock>();
 bool FlashLifeObjectR6(FeelCueR27 cue){bool any=false;foreach(var r in world.LifeRenderersFeelR6(cue.objectId)){var before=new MaterialPropertyBlock();r.GetPropertyBlock(before);lifeFlashBeforeR6[r]=before;var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);b.SetFloat("_HitWhite",1);r.SetPropertyBlock(b);any=true;}return any;}
 void RestoreLifeFlashR6(){foreach(var p in lifeFlashBeforeR6)if(p.Key)p.Key.SetPropertyBlock(p.Value);lifeFlashBeforeR6.Clear();}
}
