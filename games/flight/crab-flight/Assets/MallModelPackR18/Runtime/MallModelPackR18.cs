using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using C2=CrabCityDensityR2;
// Explicit renderer adapter. Host contacts, transforms, shards, mass and events remain authoritative.
public static class MallModelPackR18 {
 public const string Revision="mall-object-model-r18-1";
 public static string ModelFor(string id){if(id=="street3-+1-10-market_stall")return "FragranceCase";if(id=="street3-+1-11-market_crate")return "FragranceDisplay";if(id=="street3-+1-12-street_awning")return "BoutiqueEntrance";return null;}
 public static int ApplyToRoot(GameObject root){int count=0;if(!root)return count;foreach(var v in root.GetComponentsInChildren<C2.CityVisualBreakable>(true))if(Apply(v))count++;return count;}
 public static bool Apply(C2.CityVisualBreakable visual){
  if(visual&&visual.assetId.StartsWith("mall_r5_"))return false;string name=visual?ModelFor(visual.placementId):null;if(name==null||visual.GetComponent<MallObjectBindingR18>())return false;
  var prefab=Resources.Load<GameObject>("MallModelsR18/"+name);if(!prefab)throw new System.InvalidOperationException("Mall model prefab missing: "+name);
  var marker=visual.gameObject.AddComponent<MallObjectBindingR18>();
  foreach(var r in visual.GetComponentsInChildren<MeshRenderer>(true)){marker.originalRenderers.Add(r);marker.originalEnabled.Add(r.enabled);r.enabled=false;}
  var intact=Object.Instantiate(prefab,visual.intact.transform,false);intact.name=name+" authored intact";marker.generated.Add(intact);
  foreach(var pivot in visual.shardPivots){int shard=int.Parse(pivot.name.Substring(6));var part=prefab.transform.Find("Part_"+shard);if(!part)continue;var clone=Object.Instantiate(part.gameObject,pivot,false);clone.transform.localPosition=-pivot.localPosition;marker.generated.Add(clone);}
  var fixedSource=visual.fractured.transform.Cast<Transform>().FirstOrDefault(x=>!x.name.StartsWith("Shard_"));var fixedPart=prefab.transform.Find("Part_-1");
  if(fixedSource&&fixedPart){var clone=Object.Instantiate(fixedPart.gameObject,fixedSource,false);clone.transform.localPosition=Vector3.zero;marker.generated.Add(clone);}
  return true;
 }
 public static GameObject AddBay(Transform parent){var prefab=Resources.Load<GameObject>("MallModelsR18/BoutiqueBay");if(!prefab)throw new System.InvalidOperationException("Boutique bay missing");var root=Object.Instantiate(prefab,parent,false);root.name="R18 representative boutique bay";root.transform.localPosition=new Vector3(94,0,0);return root;}
 public static void Restore(C2.CityVisualBreakable visual){var m=visual?visual.GetComponent<MallObjectBindingR18>():null;if(!m)return;for(int i=0;i<m.originalRenderers.Count;i++)if(m.originalRenderers[i])m.originalRenderers[i].enabled=m.originalEnabled[i];foreach(var o in m.generated)if(o){if(Application.isPlaying)Object.Destroy(o);else Object.DestroyImmediate(o);}if(Application.isPlaying)Object.Destroy(m);else Object.DestroyImmediate(m);}
}
public sealed class MallObjectBindingR18:MonoBehaviour {public readonly List<MeshRenderer> originalRenderers=new List<MeshRenderer>();public readonly List<bool> originalEnabled=new List<bool>();public readonly List<GameObject> generated=new List<GameObject>();}
