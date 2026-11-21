using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable] public sealed class NeonBackdropLayer {
 public string name,sector,status;
 public int renderers,materials,propertyBlockColors,sharedSourceSlots,skippedOutside;
 public float gateMin,gateMax;
}
[Serializable] public sealed class NeonBackdropSector {
 public string id;public int renderers,materials;public string[] layers;
}
[Serializable] public sealed class NeonBackdropChange {
 public string layer,renderer,sourceMaterial,sourceShader,cloneShader;
 public int sourceId,cloneId,sourceUsers;
 public Color beforeColor,afterColor,depthTone;
 public bool propertyBlockColor,scopedAtlas;
 public float gateMin,gateMax;
}
[Serializable] public sealed class NeonBackdropSample {
 public string renderer,material;public Color before,after;public bool sameInstance,sameProperties;
}
[Serializable] public sealed class NeonBackdropAudit {
 public string stage2="night-backdrop-v1",stage3="spin-vortex-world-v1",hookOrder="AttachNeon after MakeField / BuildBackground / AddBay / AddSurroundings";
 public int darkenedMaterials,darkenedRenderers,protectedMaterials,protectedRenderers,nearSharedSourceSlots;
 public bool allLayersPresent,allSectorsPresent,otherRegionsUnchanged,protectedMaterialsUnchanged,lateWorldEnabled,lateWorldActive;
 public NeonBackdropLayer[] layers;public NeonBackdropSector[] sectors;
 public NeonBackdropChange[] changes;public NeonBackdropSample[] otherRegionSamples;
}

// Renderer-local clones. Inactive legacy layers, contacts, lives and physical floors stay untouched.
public sealed class NeonBackdropDarken {
 sealed class Binding {
  public Renderer renderer;public Material[] before,after;public string layer,sector;
  public MaterialPropertyBlock blockBefore,blockAfter;
 }
 sealed class Selection {
  public string name,sector;public bool global,repeat,surface;
  public Selection(string n,string s,bool g=false,bool r=false,bool u=false){name=n;sector=s;global=g;repeat=r;surface=u;}
 }
 readonly List<Binding> bindings=new List<Binding>();
 readonly List<Material> owned=new List<Material>();
 public readonly NeonBackdropAudit Audit=new NeonBackdropAudit();
 static readonly string[] MallNames={"mall / enclosed shopping atrium -1","mall / enclosed shopping atrium 1","mall / indoor decorative near replacements","R18 representative boutique bay","R7 gift storefront -1","R7 gift storefront 1","DensityProto static mall decor"};
 static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
 static string MaterialState(Material m){
  string s=m.GetInstanceID()+"|"+m.shader.name+"|"+m.renderQueue+"|"+string.Join(",",m.shaderKeywords);
  foreach(string key in new[]{"_Color","_EmissionColor"})if(m.HasProperty(key))s+="|"+JsonUtility.ToJson(m.GetColor(key));
  foreach(string key in new[]{"_Glossiness","_Metallic","_FragmentReadability","_MetalDebrisReadability"})if(m.HasProperty(key))s+="|"+BitConverter.SingleToInt32Bits(m.GetFloat(key));
  foreach(string key in new[]{"_MainTex","_MetallicGlossMap","_BumpMap"})if(m.HasProperty(key))s+="|"+(m.GetTexture(key)?m.GetTexture(key).GetInstanceID():0);
  return s;
 }
 static string RendererState(Renderer r){
  var b=new MaterialPropertyBlock();r.GetPropertyBlock(b);
  return r.enabled+"|"+string.Join(",",r.sharedMaterials.Select(m=>m?m.GetInstanceID():0))+"|"+JsonUtility.ToJson(b.GetColor("_Color"))+"|"+BitConverter.SingleToInt32Bits(b.GetFloat("_Glossiness"));
 }
 public static Color Dark(Color c){
  float l=c.r*.2126f+c.g*.7152f+c.b*.0722f;
  Color navy=Color.Lerp(NeonProto.Background,NeonProto.BackgroundViolet,Mathf.Clamp01(l)*.6f);
  float peak=Mathf.Max(c.r,Mathf.Max(c.g,c.b));
  Color hue=peak>0?c*(Mathf.Max(navy.r,Mathf.Max(navy.g,navy.b))/peak):navy;
  var dark=Color.Lerp(navy,hue,.12f);dark.a=c.a;return dark;
 }
 static bool Intersects(Bounds b,float start,float end)=>b.max.x>=start&&b.min.x<end||b.max.x>-end&&b.min.x<=-start;
 public NeonBackdropDarken(Transform owner){
  var all=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
  var originals=all.SelectMany(r=>r.sharedMaterials).Where(m=>m).Distinct().ToArray();
  var materialBefore=originals.ToDictionary(m=>m,MaterialState);
  var rendererBefore=all.ToDictionary(r=>r,RendererState);
  var users=all.SelectMany(r=>r.sharedMaterials).Where(m=>m).GroupBy(m=>m).ToDictionary(g=>g.Key,g=>g.Count());
  var layers=new List<NeonBackdropLayer>();var changes=new List<NeonBackdropChange>();var selected=new HashSet<Renderer>();
  var scoped=Resources.Load<Shader>("NeonProto/NeonBackdropAtlas");if(!scoped)throw new InvalidOperationException("Neon backdrop atlas shader missing");
  var roots=owner.GetComponentsInChildren<Transform>(true);
  var sceneRoots=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>t.gameObject.scene==owner.gameObject.scene).ToArray();
  var surfaceShader=Resources.Load<Shader>("NeonProto/NeonBackdropSurface");if(!surfaceShader)throw new InvalidOperationException("Neon backdrop surface shader missing");
  var selections=MallNames.Select(n=>new Selection(n,"mall")).ToList();
  selections.Add(new Selection("tank / wet tile and pipes","tank"));
  selections.Add(new Selection("restaurant / timber ribs and service alcoves","restaurant"));
  string[] ids={"office","port","viaduct","country","launchrange","orbit"};
  string[] names={"office / vertical fins and setbacks","port / sawtooth sheds and dock cranes","viaduct / deep spans and bracing","country / low terraces and windbreaks","range / test rings and launch structures","orbit / segmented rings and array wings"};
  for(int i=0;i<ids.Length;i++)foreach(int side in new[]{-1,1})selections.Add(new Selection(names[i]+" "+side,ids[i]));
  foreach(string name in new[]{"R9 regional depth and atrium",FarWorldArtR10.Revision,"CoreR7 mall and office district architecture",LateWorldArtR9.Revision,"R11 all-section graybox"})selections.Add(new Selection(name,"all"));
  selections.Add(new Selection("City2 broad district ground","all",false,true,true));
  selections.Add(new Selection("Street3 roadway, seams and contact cues","all",false,false,true));
  selections.Add(new Selection("Restaurant authored environment - visuals only","all",true,false,true));
  Audit.lateWorldEnabled=LateWorldArtR9.Enabled;
  Audit.lateWorldActive=roots.Any(t=>t.name==LateWorldArtR9.Revision&&t.gameObject.activeInHierarchy);
  foreach(var selection in selections){
   string name=selection.name;var sector=WorldRouteR11.Data.sectors.SingleOrDefault(s=>s.id==selection.sector);
   var layer=new NeonBackdropLayer{name=name,sector=selection.sector,status="absent",gateMin=sector==null?0:sector.start,gateMax=sector==null?(selection.surface?WorldRouteR11.Data.sectors.Last().end:1000000):sector.end};layers.Add(layer);
   var matches=(selection.global?sceneRoots:roots).Where(t=>t.name==name).ToArray();if(matches.Length>1&&!selection.repeat)throw new InvalidOperationException("Duplicate background root: "+name);
   if(matches.Length==0)continue;
   var clones=new Dictionary<Material,Material>();
   foreach(var root in matches){
   if(!root.gameObject.activeInHierarchy){layer.status="inactive - untouched";continue;}
   // A contact anywhere below the selected background root is a hard error, not a silent skip.
   if(root.GetComponentInParent<CrabCityDensityR2.CityVisualBreakable>()||root.GetComponentInParent<Collider>()||root.GetComponentInParent<Rigidbody>()||root.GetComponentsInChildren<CrabCityDensityR2.CityVisualBreakable>(true).Length>0||root.GetComponentsInChildren<Collider>(true).Length>0||root.GetComponentsInChildren<Rigidbody>(true).Length>0)throw new InvalidOperationException("Contact included in backdrop root: "+PathOf(root));
   layer.status="darkened";
   foreach(var r in root.GetComponentsInChildren<MeshRenderer>(false)){
    if(!r.enabled||!r.gameObject.activeInHierarchy)continue;
    if(!selected.Add(r))throw new InvalidOperationException("Overlapping backdrop selection: "+PathOf(r.transform));
    bool near=name==MallNames[2];
    if(near&&!Intersects(r.bounds,NeonProto.Start,NeonProto.End)){selected.Remove(r);layer.skippedOutside++;continue;}
    var b=new Binding{renderer=r,before=r.sharedMaterials,layer=name,sector=selection.sector,blockBefore=new MaterialPropertyBlock(),blockAfter=new MaterialPropertyBlock()};
    r.GetPropertyBlock(b.blockBefore);r.GetPropertyBlock(b.blockAfter);
    bool overrideColor=b.blockBefore.HasColor("_Color");if(overrideColor)layer.propertyBlockColors++;
    b.after=new Material[b.before.Length];
    for(int i=0;i<b.before.Length;i++){
     var source=b.before[i];if(!source)throw new InvalidOperationException("Missing background material");
     bool atlas=source.shader.name=="CrabCityDensity/R2Surface";
     if(!clones.TryGetValue(source,out var clone)){
      clone=new Material(source){name="Neon stage3 / "+name+" / "+source.name};clones.Add(source,clone);owned.Add(clone);
      if(atlas){clone.shader=scoped;clone.SetColor("_NeonLow",NeonProto.Background);clone.SetColor("_NeonHigh",NeonProto.BackgroundViolet);clone.SetFloat("_NeonGateMin",layer.gateMin);clone.SetFloat("_NeonGateMax",layer.gateMax);}
      else if(selection.surface){
       if(source.shader.name!="Standard"||source.GetFloat("_Mode")!=0||source.mainTexture)throw new InvalidOperationException("Unexpected surface shader/texture/mode: "+PathOf(r.transform));
       clone.shader=surfaceShader;clone.SetFloat("_NeonGateMin",0);clone.SetFloat("_NeonGateMax",WorldRouteR11.Data.sectors.Last().end);
       clone.SetColor("_NeonDarkColor",Dark(source.color));
       if(source.HasProperty("_EmissionColor"))clone.SetColor("_NeonDarkEmission",source.GetColor("_EmissionColor")*.06f);
      }else{
       bool fixture=name.StartsWith("mall / enclosed")&&source.name.EndsWith(" 13");
       clone.color=fixture?new Color(.07f,.24f,.30f,source.color.a):Dark(source.color);
       if(clone.HasProperty("_EmissionColor")){var e=source.GetColor("_EmissionColor");clone.SetColor("_EmissionColor",fixture?new Color(.006f,.018f,.024f):new Color(e.r*.06f,e.g*.06f,e.b*.12f,e.a));if(fixture)clone.EnableKeyword("_EMISSION");}
       if(clone.HasProperty("_Glossiness"))clone.SetFloat("_Glossiness",Mathf.Min(source.GetFloat("_Glossiness"),.18f));
      }
     }
     if(!atlas&&overrideColor)b.blockAfter.SetColor(selection.surface?"_NeonDarkColor":"_Color",Dark(b.blockBefore.GetColor("_Color")));
     if(users[source]>1)layer.sharedSourceSlots++;
     changes.Add(new NeonBackdropChange{layer=name,renderer=PathOf(r.transform),sourceMaterial=source.name,sourceShader=source.shader.name,cloneShader=clone.shader.name,sourceId=source.GetInstanceID(),cloneId=clone.GetInstanceID(),sourceUsers=users[source],beforeColor=source.color,afterColor=selection.surface?clone.GetColor("_NeonDarkColor"):clone.color,propertyBlockColor=overrideColor,depthTone=b.blockBefore.GetColor("_Color"),scopedAtlas=atlas,gateMin=layer.gateMin,gateMax=layer.gateMax});
     b.after[i]=clone;
    }
    r.sharedMaterials=b.after;r.SetPropertyBlock(b.blockAfter);bindings.Add(b);layer.renderers++;
   }
   }
   layer.materials=clones.Count;
  }
  Audit.layers=layers.ToArray();Audit.changes=changes.ToArray();Audit.darkenedMaterials=owned.Count;Audit.darkenedRenderers=bindings.Count;
  Audit.nearSharedSourceSlots=layers[2].sharedSourceSlots;Audit.allLayersPresent=layers.Take(MallNames.Length).All(l=>l.renderers>0);
  Audit.sectors=WorldRouteR11.Data.sectors.Select(s=>{
   var bs=bindings.Where(b=>b.sector==s.id||b.sector=="all"&&Intersects(b.renderer.bounds,s.start,s.end)).ToArray();
   return new NeonBackdropSector{id=s.id,renderers=bs.Length,materials=bs.SelectMany(b=>b.after).Distinct().Count(),layers=bs.Select(b=>b.layer).Distinct().ToArray()};
  }).ToArray();
  Audit.allSectorsPresent=Audit.sectors.All(s=>s.renderers>0);
  Audit.protectedMaterials=originals.Length;Audit.protectedRenderers=all.Count(r=>!selected.Contains(r));
  Audit.protectedMaterialsUnchanged=originals.All(m=>MaterialState(m)==materialBefore[m]);
  Audit.otherRegionsUnchanged=all.Where(r=>!selected.Contains(r)).All(r=>RendererState(r)==rendererBefore[r]);
  // Samples now describe protected non-target renderers, not office/port (both are targets).
  Audit.otherRegionSamples=all.Where(r=>!selected.Contains(r)&&r.sharedMaterial&&r.sharedMaterial.HasProperty("_Color")).Take(24).Select(r=>new NeonBackdropSample{renderer=PathOf(r.transform),material=r.sharedMaterial.name,before=r.sharedMaterial.color,after=r.sharedMaterial.color,sameInstance=RendererState(r)==rendererBefore[r],sameProperties=MaterialState(r.sharedMaterial)==materialBefore[r.sharedMaterial]}).ToArray();
  if(!Audit.allLayersPresent||!Audit.allSectorsPresent||!Audit.protectedMaterialsUnchanged||!Audit.otherRegionsUnchanged)throw new InvalidOperationException("Neon backdrop isolation/order contract failed");
 }
// Primitive render surfaces lose their temporary creation Collider at end of frame in players.
 // Do not alter them while that component still exists, even if destruction is queued.
 public static bool Ready(Transform owner)=>!owner.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="City2 broad district ground"&&t.GetComponent<Collider>());
#if UNITY_EDITOR
 public void ReviewMallDark(bool enabled){foreach(var b in bindings.Where(b=>b.sector=="mall")){b.renderer.sharedMaterials=enabled?b.after:b.before;b.renderer.SetPropertyBlock(enabled?b.blockAfter:b.blockBefore);}}
 public void ReviewDark(bool enabled){foreach(var b in bindings){b.renderer.sharedMaterials=enabled?b.after:b.before;b.renderer.SetPropertyBlock(enabled?b.blockAfter:b.blockBefore);}}
#endif
 public void Dispose(){foreach(var b in bindings)if(b.renderer){b.renderer.sharedMaterials=b.before;b.renderer.SetPropertyBlock(b.blockBefore);}foreach(var m in owned)NeonProto.Release(m);}
}
