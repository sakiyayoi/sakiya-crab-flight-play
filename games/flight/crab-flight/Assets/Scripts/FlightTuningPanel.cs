using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Scripting;
using RuntimeInspectorNamespace;

[Serializable,Preserve] public sealed class FlightTuningProfile {
 public float Angle=12,Power=100,Claw=45,Shell=45,Legs=45,SecondaryFraction=.25f;
}
// QA-only adapter. No initialization, world/physics writes or visible UI unless explicitly opened.
public sealed class FlightTuningPanel:MonoBehaviour {
 public GameObject inspectorPrefab;public FlightTuningProfile Profile=new FlightTuningProfile();
 public bool Visible{get;private set;}public int ApplyCount{get;private set;}public string Error{get;private set;}="";
 CrabFlightGame game;GameObject canvas;Text status;float reportAt;
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void FlightTuningReport(string json);
#endif
 [Serializable] public class Reply {public bool visible;public int applied;public string error,phase;public FlightTuningProfile edit;public float appliedAngle,appliedPower,secondaryFraction;}
 void Ensure(){if(game==null)game=GetComponent<CrabFlightGame>();game.Initialize();}
 public void SetDeveloperMode(string value){Ensure();Visible=value=="1";if(Visible){if(canvas==null)MakeUI();canvas.SetActive(true);game.SetPaused("1");}else{if(canvas)canvas.SetActive(false);game.SetPaused("0");}Publish();}
 public void ApplyFlightTuning(string json){if(!string.IsNullOrEmpty(json))Profile=JsonUtility.FromJson<FlightTuningProfile>(json);Apply();}
 public void Apply(){Ensure();var s=game.State();Error="";if(s.phase=="flight"||s.aiming){Error="Finish or cancel the throw before applying";Publish();return;}
  Profile.Angle=FlightSettings.Safe(Profile.Angle,10,80);Profile.Power=FlightSettings.Safe(Profile.Power,0,100);Profile.Claw=FlightSettings.Safe(Profile.Claw,0,100);Profile.Shell=FlightSettings.Safe(Profile.Shell,0,100);Profile.Legs=FlightSettings.Safe(Profile.Legs,0,100);Profile.SecondaryFraction=FlightSettings.Safe(Profile.SecondaryFraction,.1f,1);
  game.secondaryThresholdFraction=Profile.SecondaryFraction;RigidCrabFlight.SecondaryThresholdFraction=Profile.SecondaryFraction;
  if(s.phase=="result")game.Prepare("");game.Configure(JsonUtility.ToJson(new FlightSettings{angle=Profile.Angle,power=Profile.Power,claw=Profile.Claw,shell=Profile.Shell,legs=Profile.Legs}));
  ApplyCount++;if(Visible)game.SetPaused("1");Publish();
 }
 public Reply Snapshot(){Ensure();var s=game.State();return new Reply{visible=Visible,applied=ApplyCount,error=Error,phase=s.phase,edit=Profile,appliedAngle=s.angle,appliedPower=s.power,secondaryFraction=game.secondaryThresholdFraction};}
 void Update(){if(!Visible)return;if(Time.unscaledTime>=reportAt){reportAt=Time.unscaledTime+.1f;Publish();}}
 void Publish(){var r=Snapshot();if(status)status.text=$"Applied #{r.applied}: {r.appliedAngle:0.##} deg / {r.appliedPower:0.##} power\nChain fraction {r.secondaryFraction:0.###}  {r.error}";
#if UNITY_WEBGL && !UNITY_EDITOR
  FlightTuningReport(JsonUtility.ToJson(r));
#endif
 }
 void MakeUI(){canvas=new GameObject("QA Runtime Inspector",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.transform.SetParent(transform,false);var c=canvas.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=1000;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(390,844);scaler.matchWidthOrHeight=.5f;
  var panel=Instantiate(inspectorPrefab,canvas.transform).GetComponent<RuntimeInspector>();var rect=panel.GetComponent<RectTransform>();rect.anchorMin=new Vector2(0,0);rect.anchorMax=new Vector2(1,1);rect.offsetMin=new Vector2(12,160);rect.offsetMax=new Vector2(-12,-100);panel.Inspect(Profile);panel.ExposeProperties=RuntimeInspectorNamespace.RuntimeInspector.VariableVisibility.None;
  var buttonObject=new GameObject("Apply on next throw",typeof(RectTransform),typeof(Image),typeof(Button));buttonObject.transform.SetParent(canvas.transform,false);var br=buttonObject.GetComponent<RectTransform>();br.anchorMin=new Vector2(0,0);br.anchorMax=new Vector2(1,0);br.offsetMin=new Vector2(12,100);br.offsetMax=new Vector2(-12,152);buttonObject.GetComponent<Image>().color=new Color(.98f,.69f,.22f);buttonObject.GetComponent<Button>().onClick.AddListener(Apply);var bt=Label(buttonObject.transform,"APPLY FOR NEXT THROW",Color.black,18);bt.alignment=TextAnchor.MiddleCenter;
  var so=new GameObject("QA readback",typeof(RectTransform));so.transform.SetParent(canvas.transform,false);var sr=so.GetComponent<RectTransform>();sr.anchorMin=new Vector2(0,0);sr.anchorMax=new Vector2(1,0);sr.offsetMin=new Vector2(12,12);sr.offsetMax=new Vector2(-12,94);status=Label(so.transform,"",Color.white,15);status.alignment=TextAnchor.MiddleCenter;
  if(EventSystem.current==null){var es=new GameObject("QA EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));es.transform.SetParent(canvas.transform,false);}
 }
 static Text Label(Transform parent,string text,Color color,int size){var o=new GameObject("Label",typeof(RectTransform),typeof(Text));o.transform.SetParent(parent,false);var r=o.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;var t=o.GetComponent<Text>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.color=color;return t;}
 void OnDestroy(){if(canvas)Destroy(canvas);}
}
