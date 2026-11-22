using System;
using UnityEngine;

[Serializable] public sealed class ResultRouteSection {
 public string id,label;public float from,to;public int broken;
}
[Serializable] public sealed class FlightResultOverview {
 public string revision="result-overview-1",stage;public float distance,stop,from,to,reached,viewFrom,viewTo;
 public int broken;public float lastBreak;public float[] breaks;public ResultRouteSection[] sections;
}

// Presentation only. Distance remains the solver's final horizontal distance;
// reached is a separate observed maximum, and break positions come from BreakEvents.
public sealed partial class CrabFlightGame {
 public bool useResultOverview=true;
 float resultTravelMin,resultTravelMax,resultAge,resultStartSize;Vector3 resultStartCamera;
 FlightResultOverview resultOverview;

 void ObserveResultTravel(){
  float x=direction*crab.Root.position.x;
  resultTravelMin=Mathf.Min(resultTravelMin,x);resultTravelMax=Mathf.Max(resultTravelMax,x);
 }
 void ResetResultOverview(){resultOverview=null;resultTravelMin=resultTravelMax=resultAge=0;}
 void BeginResultOverview(){
  if(!useResultOverview)return;
  ObserveResultTravel();resultAge=0;resultStartCamera=cam.transform.position;resultStartSize=cam.orthographicSize;
  var breaks=new float[simulation.BreakEvents.Count];float from=resultTravelMin,to=resultTravelMax;
  for(int i=0;i<breaks.Length;i++){breaks[i]=direction*simulation.BreakEvents[i].position.x;from=Mathf.Min(from,breaks[i]);to=Mathf.Max(to,breaks[i]);}
  var sections=new System.Collections.Generic.List<ResultRouteSection>();
  if(useRouteBlockoutR11)foreach(var sector in WorldRouteR11.Data.sectors){
   if(sector.start>=resultTravelMax)break;
   var section=new ResultRouteSection{id=sector.id,label=sector.label,from=sector.start,to=Mathf.Min(sector.end,resultTravelMax)};
   foreach(float x in breaks)if(x>=sector.start&&x<sector.end)section.broken++;
   sections.Add(section);
  }
  resultOverview=new FlightResultOverview{stage="stop",distance=lastDistance,stop=direction*crab.Root.position.x,from=from,to=to,reached=resultTravelMax,broken=breaks.Length,lastBreak=breaks.Length>0?breaks[breaks.Length-1]:0,breaks=breaks,sections=sections.ToArray()};
 }
 bool UpdateResultCamera(float dt,bool snap){
  if(!useResultOverview||phase!="result"||resultOverview==null)return false;
  resultAge+=dt;
  // Keep the existing view in reduced motion and on safety stops. No orbit, shake,
  // world-scale zoom, physics tick, input lock, or automatic replay is introduced.
  bool hold=ReducedMotion||simulation.AbortReason.Length>0;
  if(!hold){
   bool portrait=cam.aspect<1;float size=Mathf.Max(resultStartSize,portrait?10.5f:9.5f);
   if(resultOverview.broken>0){float behind=Mathf.Abs(resultOverview.stop-resultOverview.lastBreak);size=Mathf.Max(size,Mathf.Min(portrait?13:14,(behind+3)/(Mathf.Max(.3f,cam.aspect)*.8f)));}
   float halfWidth=size*cam.aspect;
   // Keep the bold crab left of the desktop result panel; the strip shows the broken route.
   // Portrait keeps the crab centered. Neither composition depends on travel direction.
   float towardBreak=portrait?0:halfWidth*.24f;
   Vector3 center=crab.Root.position+new Vector3(towardBreak,size*(portrait?.22f:.12f),0);
   center.z=0;
   float reveal=snap?1:Mathf.SmoothStep(0,1,Mathf.InverseLerp(.22f,1.22f,resultAge));
   if(usePortCamera){
    float last=direction*resultOverview.lastBreak;
    float left=Mathf.Min(crab.Root.position.x,last),right=Mathf.Max(crab.Root.position.x,last);
    if(resultOverview.broken>0){center.x=(left+right)*.5f;size=Mathf.Max(size,(right-left+8)/(2*Mathf.Max(.3f,cam.aspect)));}
    size=Mathf.Max(size,resultStartSize*1.08f);reveal=snap?1:Mathf.SmoothStep(0,1,resultAge/.8f);
   }
   Vector3 target=center+new Vector3(0,5.3f,-16);
   float bottom=target.y-size*cam.transform.up.y;target-=cam.transform.forward*Mathf.Max(0,(.5f-bottom)/-cam.transform.forward.y);
   cam.transform.position=Vector3.Lerp(resultStartCamera,target,reveal);
   cam.orthographicSize=Mathf.Lerp(resultStartSize,size,reveal);
  }
  resultOverview.stage=hold?"stop":resultAge<(usePortCamera?.8f:1.22f)?"pullback":"overview";
  // The inset is the current rendered X interval, in the same metres as the strip.
  float a=direction*(cam.transform.position.x-cam.orthographicSize*cam.aspect);
  float b=direction*(cam.transform.position.x+cam.orthographicSize*cam.aspect);
  resultOverview.viewFrom=Mathf.Min(a,b);resultOverview.viewTo=Mathf.Max(a,b);
  return true;
 }
}
