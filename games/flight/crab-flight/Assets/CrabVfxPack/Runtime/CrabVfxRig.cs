using UnityEngine;
using UnityEngine.Rendering;

namespace CrabDestructionVfx {
 [DisallowMultipleComponent]
 [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
 public sealed class CrabVfxRig : MonoBehaviour {
  public CrabVfxProfile profile;
  [Tooltip("Assign the included shader directly, so WebGL shader stripping cannot remove it.")]
  public Shader inkShader;
  public bool manualTick;
  public bool reducedMotion;
  CrabVfxCore core;CrabVfxGeometry geometry;Mesh mesh;Material material;
  Vector3[] vertices;Color[] colors;
  public CrabVfxCore Core {get{Ensure();return core;}}
  public void Ensure(){
   if(core!=null)return;
   // Copy settings: a prefab instance never mutates its shared profile.
   VfxTuning settings=profile?JsonUtility.FromJson<VfxTuning>(JsonUtility.ToJson(profile.tuning)):new VfxTuning();
   settings.reducedMotion=reducedMotion||settings.reducedMotion;core=new CrabVfxCore(settings);
   geometry=new CrabVfxGeometry(core.Particles.Length);vertices=new Vector3[geometry.Vertices.Length];colors=new Color[geometry.Vertices.Length];
   mesh=new Mesh{name="Crab VFX pooled mesh"};mesh.MarkDynamic();mesh.indexFormat=IndexFormat.UInt32;
   GetComponent<MeshFilter>().sharedMesh=mesh;
   if(!inkShader)inkShader=Shader.Find("Crab/DestructionInk");
   if(!inkShader){Debug.LogError("Assign Crab DestructionInk shader before enabling VFX.",this);enabled=false;return;}
   material=new Material(inkShader){name="Crab VFX instance ink"};
   var renderer=GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
   renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
  }
  void Awake(){Ensure();}
  void OnEnable(){Ensure();if(core!=null){core.Reset();core.SetReducedMotion(reducedMotion||(profile&&profile.tuning.reducedMotion));}}
  void LateUpdate(){if(!manualTick)Tick(Time.deltaTime);}
  public void Tick(float seconds){if(!isActiveAndEnabled)return;Ensure();core.Step(seconds);Draw();}
  public void ObserveCharge(Vector3 crabPosition,float elapsed,float energy,float visualSpinDegrees,bool active,bool ready){if(isActiveAndEnabled)Core.Charge(V(crabPosition),elapsed,energy,visualSpinDegrees,active,ready);}
  public void EmitLaunch(Vector3 position,Vector3 velocity,float chargeEnergy){if(isActiveAndEnabled)Core.Launch(V(position),V(velocity),chargeEnergy);}
  public void EmitBoost(int token,Vector3 position,Vector3 velocity,float strength){if(isActiveAndEnabled)Core.Boost(token,V(position),V(velocity),strength);}
  public bool EmitBreak(int eventToken,Surface surface,Vector3 contactPoint,Vector3 incomingVelocity,float strength01){return isActiveAndEnabled&&Core.Break(eventToken,surface,V(contactPoint),V(incomingVelocity),strength01);}
  public bool EmitBreak(int eventToken,Surface surface,Vector3 contactPoint,Vector3 incomingVelocity,Vector3 contactNormal,float strength01){return isActiveAndEnabled&&Core.Break(eventToken,surface,V(contactPoint),V(incomingVelocity),V(contactNormal),strength01);}
  public void EmitExit(Vector3 crabPosition,Vector3 velocity){if(isActiveAndEnabled)Core.ExitRestaurant(V(crabPosition),V(velocity));}
  public void SetPaused(bool value){Core.SetPaused(value);}
  public void SetReducedMotion(bool value){reducedMotion=value;Core.SetReducedMotion(value);Draw();}
  public void ResetRun(uint seed=0xC4ABu){Core.Reset(seed);Core.SetReducedMotion(reducedMotion||(profile&&profile.tuning.reducedMotion));if(!isActiveAndEnabled)core.Stop(true);Draw();}
  public void EndRun(bool clear=false){Core.Stop(clear);Draw();}
  public static Surface SurfaceFromId(string id){switch(id){case "glass":return Surface.Glass;case "wood":return Surface.Wood;case "metal":return Surface.Metal;case "wall":return Surface.Plaster;case "tableware":return Surface.Ceramic;case "food":return Surface.Food;default:return Surface.Plaster;}}
  void Draw(){
   if(mesh==null||material==null)return;geometry.Build(core);
   for(int i=0;i<geometry.VertexCount;i++){
    var v=geometry.Vertices[i];vertices[i]=transform.InverseTransformPoint(new Vector3(v.x,v.y,v.z));
    var c=geometry.Colors[i];colors[i]=new Color(c.r,c.g,c.b,c.a);
   }
   mesh.Clear(false);
   mesh.SetVertices(vertices,0,geometry.VertexCount);mesh.SetColors(colors,0,geometry.VertexCount);
   mesh.SetIndices(geometry.Indices,0,geometry.IndexCount,MeshTopology.Triangles,0,false);
   mesh.RecalculateBounds();
  }
  // Disable is a hard lifetime boundary. Re-enable requires fresh Observe/Emit calls.
  void OnDisable(){if(core!=null){core.Stop(true);if(mesh)mesh.Clear(false);}}
  void OnDestroy(){if(mesh)ReleaseOwned(mesh);if(material)ReleaseOwned(material);}
  static void ReleaseOwned(UnityEngine.Object o){
#if UNITY_EDITOR
 if(!Application.isPlaying){UnityEngine.Object.DestroyImmediate(o);return;}
#endif
 UnityEngine.Object.Destroy(o);}
  static V3 V(Vector3 v){return new V3(v.x,v.y,v.z);}
 }
}
