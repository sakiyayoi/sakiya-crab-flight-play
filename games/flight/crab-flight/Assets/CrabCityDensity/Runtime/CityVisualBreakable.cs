using UnityEngine;

namespace CrabCityDensity {
 // Visual state only. Contact, thresholds, momentum, debris physics and pooling belong to the host.
 // This component has no Update, automatic animation, Rigidbody, Collider, force or input handling.
 public sealed class CityVisualBreakable : MonoBehaviour {
  public string placementId, assetId;
  public float suggestedTotalMassKg;
  public Vector3 suggestedEnvelopeCenter, suggestedEnvelopeSize;
  public GameObject intact, fractured;
  public Transform[] shardPivots;
  public Vector3[] shardRestPositions;
  public bool IsBroken {get; private set;}
  public void SetBroken(bool broken) {
   IsBroken=broken;
   if(intact)intact.SetActive(!broken);
   if(fractured)fractured.SetActive(broken);
  }
  // Reset restored fragment local pivots even if the host has moved/rotated them for debris.
  public void ResetVisual() {
   for(int i=0;i<shardPivots.Length;i++){var p=shardPivots[i];if(p){p.localPosition=shardRestPositions[i];p.localRotation=Quaternion.identity;p.localScale=Vector3.one;}}
   SetBroken(false);
  }
 }
}
