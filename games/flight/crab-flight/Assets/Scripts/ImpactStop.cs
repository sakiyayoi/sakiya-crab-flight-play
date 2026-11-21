using System;
using System.Collections.Generic;
// A local presentation clock. Never changes global timeScale or rigid-body forces.
public sealed class ImpactStop {
 public bool CoreFeedback,HeavyFeedback=true;
 readonly HashSet<int> seen=new HashSet<int>();int generation=-1;float clock,windowStart,budget,heavyDebt,last=-10;
 public const float HeavyHoldLimit=.105f,HeavyBudget=.19f,HeavyRefill=.16f,HeavyCooldown=.24f;
 public float Remaining{get;private set;}public float TotalFrozen{get;private set;}public int Requests{get;private set;}public int Suppressed{get;private set;}
 public void Reset(){seen.Clear();generation=-1;clock=windowStart=budget=heavyDebt=Remaining=TotalFrozen=0;last=-10;Requests=Suppressed=0;}
 public void SetReducedMotion(bool reduced){if(reduced)Remaining=Math.Min(Remaining,HeavyHoldLimit*.5f);}
 public void Request(int gen,int id,string type,string material,float strength,bool reduced,float weight=.5f){
  if(!Finite(strength)||!Finite(weight))return;
  if(gen!=generation){seen.Clear();generation=gen;}if(!seen.Add(id))return;
  float duration=type=="destruction"?(material=="wall"?.055f:.009f+.043f*Math.Clamp(weight,0,1))*(.8f+.2f*Math.Clamp(strength,0,1)):type=="landing"&&strength>=.6f?.022f:0;
#if UNITY_EDITOR
  if(Street3Presentation.ReviewBaseline)duration=type=="destruction"?(material=="wall"?.055f:material=="metal"?.04f:material=="glass"?.024f:.018f+.012f*Math.Clamp(strength,0,1)):type=="landing"&&strength>=.6f?.022f:0;
#endif
  if(CoreFeedback&&type=="destruction"){float w=Math.Clamp(weight,0,1);duration=material=="food"||material=="tableware"?.012f:material=="glass"?.022f+.012f*w:material=="wall"?.060f:material=="metal"?.030f+.032f*w:.020f+.030f*w;duration*=.82f+.18f*Math.Clamp(strength,0,1);}
  bool heavy=CoreFeedback&&HeavyFeedback;
#if UNITY_EDITOR
  heavy&=!Street3Presentation.ReviewBaseline;
#endif
  if(heavy){
   float w=Math.Clamp(weight,0,1),s=Math.Clamp(strength,0,1);
   if(type=="destruction")duration=(material=="food"?.012f:material=="tableware"?.020f:material=="glass"?.038f+.012f*w:material=="wall"?HeavyHoldLimit:material=="metal"?.062f+.033f*w:.040f+.035f*w)*(.84f+.16f*s);
   else if(type=="landing"&&s>=.6f)duration=.035f+.010f*s;
   duration=Math.Min(duration,HeavyHoldLimit)*(reduced?.5f:1);
   if(duration<=0)return;
   // A burst may replace a light hold with its strongest hit. Once visual time
   // advances, no further receipt can extend that hold. Debt drains only with
   // visual time: at most 190ms initially, then 160ms per second, without a
   // fixed-window boundary that could admit two back-to-back full budgets.
   if(clock-last<.00001f&&Remaining>0){
    float extra=Math.Max(0,duration-Remaining);
    if(heavyDebt+extra<=HeavyBudget){Remaining+=extra;heavyDebt+=extra;}else Suppressed++;
    return;
   }
   if(Remaining>0||clock-last<HeavyCooldown||heavyDebt+duration>HeavyBudget){Suppressed++;return;}
   Remaining=duration;heavyDebt+=duration;last=clock;Requests++;return;
  }
  if(duration<=0)return;if(reduced)duration*=.5f;
  if(clock-windowStart>=1){windowStart=clock;budget=0;}
  // Events drained in one update form one burst. A heavy hit can promote a
  // light stop, but simultaneous panels never add their stops together.
  bool promote=true;
#if UNITY_EDITOR
  promote=!Street3Presentation.ReviewBaseline;
#endif
  if(promote&&clock-last<.00001f&&Remaining>0){float extra=Math.Max(0,duration-Remaining);if(budget+extra<=.12f){Remaining+=extra;budget+=extra;}else Suppressed++;return;}
  if(clock-last<.18f||budget+duration>.12f){Suppressed++;return;}
  duration=Math.Min(duration,.06f);Remaining=Math.Max(Remaining,duration);budget+=duration;last=clock;Requests++;
 }
 public float Filter(float dt){if(!Finite(dt)||dt<=0)return 0;clock+=dt;heavyDebt=Math.Max(0,heavyDebt-dt*HeavyRefill);float held=Math.Min(dt,Remaining);Remaining-=held;TotalFrozen+=held;return dt-held;}
 static bool Finite(float value){return !float.IsNaN(value)&&!float.IsInfinity(value);}
}
