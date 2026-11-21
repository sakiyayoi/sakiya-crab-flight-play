using System;
using System.Runtime.InteropServices;
using UnityEngine;
// Opt-in observer only. Never changes game state, timing, AI, transforms or quality.
public sealed class ActionFrameProbe:MonoBehaviour {
 public CrabActionGame game;bool active;int lastFrame=-1;
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void ActionProbeFrame(int frame,double seconds,float delta,int phase,int lk,float lc,float lp,int lf,float lcue,int rk,float rc,float rp,int rf,float rcue,int shots,int sparks,int eventId);
 [DllImport("__Internal")]static extern void ActionProbeInfo(string json);
#endif
 [Serializable]class Info{public string unity,api,scope="Target-camera postRender CPU callback, not GPU complete/present";public int quality,vSync,targetFrameRate,width,height;}
 void OnEnable(){Camera.onPostRender+=Rendered;}void OnDisable(){Camera.onPostRender-=Rendered;}
 public void EnableProbe(string value){active=value=="1";lastFrame=-1;
#if UNITY_WEBGL && !UNITY_EDITOR
  if(active)ActionProbeInfo(JsonUtility.ToJson(new Info{unity=Application.unityVersion,api=SystemInfo.graphicsDeviceType.ToString(),quality=QualitySettings.GetQualityLevel(),vSync=QualitySettings.vSyncCount,targetFrameRate=Application.targetFrameRate,width=Screen.width,height=Screen.height}));
#endif
 }
 void Rendered(Camera c){if(!active||!game||c!=game.ReviewCamera||lastFrame==Time.frameCount)return;lastFrame=Time.frameCount;var s=game.ReadProbe();
#if UNITY_WEBGL && !UNITY_EDITOR
  ActionProbeFrame(Time.frameCount,Time.realtimeSinceStartupAsDouble,Time.unscaledDeltaTime,s.phase,s.lk,s.lc,s.lp,s.lf,s.lcue,s.rk,s.rc,s.rp,s.rf,s.rcue,s.shots,s.sparks,s.eventId);
#endif
 }
}
