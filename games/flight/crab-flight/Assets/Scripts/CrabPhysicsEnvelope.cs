using System;
using UnityEngine;
// Preserve r2's approximate collision envelope during this visual refinement.
// Actual rendered part bounds remain available; this tiny table is not a second actor.
[Serializable] public class CrabEnvelopePart {public string id;public Vector3[] centers,sizes;}
[Serializable] public class CrabEnvelopeData {public float[] anchors;public CrabEnvelopePart[] parts;}
public static class CrabPhysicsEnvelope {
 static CrabEnvelopeData data;
 public static Bounds At(CrabPartProxy part,FlightSettings settings){
  #if CRAB_KING_BOLD_R2
  return part.LocalBounds;
#else
  if(data==null){var asset=Resources.Load<TextAsset>("CrabPhysicsEnvelope");if(!asset)throw new InvalidOperationException("Baseline physics envelope missing");data=JsonUtility.FromJson<CrabEnvelopeData>(asset.text);}
  var row=Array.Find(data.parts,p=>p.id==part.Id);if(row==null)throw new InvalidOperationException("Envelope part missing");float value=part.Kind==1?settings.shell:part.Kind==2?settings.claw:settings.legs;int high=1;while(high<data.anchors.Length-1&&value>data.anchors[high])high++;int low=high-1;float t=Mathf.InverseLerp(data.anchors[low],data.anchors[high],value);return new Bounds(Vector3.Lerp(row.centers[low],row.centers[high],t),Vector3.Lerp(row.sizes[low],row.sizes[high],t));
#endif
 }
}

