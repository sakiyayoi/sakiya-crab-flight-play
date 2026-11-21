using UnityEngine;
public sealed class FragmentMaterialRole:MonoBehaviour {public string materialType;}
public sealed partial class CrabFlightGame {
#if UNITY_EDITOR
 public void ReviewSetMetalReadability(bool on){foreach(var f in world.ReviewFragments)if(f){var role=f.GetComponent<FragmentMaterialRole>();if(role&&role.materialType=="metal")ReadableMaterials.Metal(f,on);}}
 public void ReviewSetReadability(bool on){useReadableSurfaces=on;if(landingFlag&&landingFlag.gameObject.activeSelf){var marker=landingFlag.position;marker.z=on?2.7f:-1.9f;landingFlag.position=marker;}ReadableMaterials.Actor(crab.Root,on);world.UseReadableSurfaces=on;foreach(var f in world.ReviewFragments)if(f){var role=f.GetComponent<FragmentMaterialRole>();if(role)ReadableMaterials.Fragment(f,role.materialType,on);}}
#endif
}
