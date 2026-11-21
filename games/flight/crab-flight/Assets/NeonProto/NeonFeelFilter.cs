using UnityEngine;

// The existing cue renderer keeps its event list and clock. Only drawing is filtered.
public sealed partial class MashimashiPresentationR27 {
 bool neonSpinGhostSuppressed;
 public bool NeonSpinGhostSuppressed=>neonSpinGhostSuppressed;
 public bool NeonSpinBubbleSuppressed=>neonSpinGhostSuppressed;
 public int NeonBubbleMarks {get {int n=0;foreach(var m in marks)if(m.kind==0&&m.material==5)n++;return n;}}
 public int NeonVisibleBubbles=>NeonSkipBubbles?0:NeonBubbleMarks;
 public int NeonGhostMarks {get {int n=0;foreach(var m in marks)if(m.kind==2)n++;return n;}}
 public int NeonVisibleGhosts=>NeonSkipGhosts?0:NeonGhostMarks;
 public void DrawNeon(Camera camera,float now,bool spinGate){neonSpinGhostSuppressed=spinGate;Draw(camera,now);}
 bool NeonSkipImpactFans {
  get {
#if UNITY_EDITOR
   return reviewNoImpactFans;
#else
   return false;
#endif
  }
 }
 bool NeonSkipGhosts {
  get {
#if UNITY_EDITOR
   return neonSpinGhostSuppressed||reviewNoGhosts;
#else
   return neonSpinGhostSuppressed;
#endif
  }
 }
 bool NeonSkipBubbles {
  get {
#if UNITY_EDITOR
   return neonSpinGhostSuppressed||reviewNoBubbles;
#else
   return neonSpinGhostSuppressed;
#endif
  }
 }
#if UNITY_EDITOR
 bool reviewNoImpactFans,reviewNoGhosts,reviewNoBubbles;
 public void ReviewSpinGate(bool value){neonSpinGhostSuppressed=value;}
 public void ReviewFrozenDraw(Camera camera,float now,bool noFans,bool noGhosts,bool noBubbles){
  reviewNoImpactFans=noFans;reviewNoGhosts=noGhosts;reviewNoBubbles=noBubbles;
  bool pendingFlash=flash;Draw(camera,now);flash=pendingFlash;
 }
 public string ReviewCueInventory()=>JsonUtility.ToJson(ReadResources(0))+"; marks="+marks.Count+"; flash="+flash;
#endif
}

public sealed partial class CrabFlightGame {
 void DrawNeonFeel(float now){
  bool reduced=ReducedMotion||feelSettingsR27.reducedMotion;
  bool gate=NeonProto.SpinGate(phase,currentSpinRps,reduced,FeelEffectsR27,aiming||spinCharge.Started);
  feelDrawingR27.DrawNeon(cam,now,gate);
 }
}
