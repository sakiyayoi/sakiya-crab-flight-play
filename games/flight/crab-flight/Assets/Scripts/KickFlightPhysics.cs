using System;

// Original timing experiment. The entire first arc delegates to the preserved
// ui1 integrator so its first-contact measurement is exactly comparable.
public sealed class KickFlightPhysics {
 public const string RuleVersion="crab-kick-r1";
 public const double StepSeconds=FlightPhysics.StepSeconds;
 public const double FloatStart=80,FloatEnd=100;
 public enum RunStage { LAUNCH,AIRBORNE,BRACED,IMPACT,TUMBLE,SETTLED,RESULT }
 public RunStage Stage {get;private set;}
 public double X,Y,Vx,Vy,Time,Peak,Distance,FirstContact=-1,FirstContactTime=-1;
 public int Remaining=3,Used,Perfects,Goods,EarlyMisses,OrdinaryImpacts,ImpactID,EventID,EmptyPresses;
 public string LastGrade="",LastSurface="sand",LastEvent="launch";
 public bool Finished=>Stage==RunStage.RESULT;
 public bool Landed=>Finished;
 public bool Braced=>brace;
 public bool CanKick=>!Finished&&Stage!=RunStage.TUMBLE&&Stage!=RunStage.SETTLED&&Remaining>0&&!brace&&airLatch&&Y>.04;
 public double BraceWindow {get;private set;}
 public double PerfectWindow {get;private set;}
 public double LastPressTime {get;private set;} = -100;
 public double Speed=>Math.Sqrt(Vx*Vx+Vy*Vy);
 public bool Held {get;private set;}
 public double TimeToGround {
  get {if(Finished||Stage==RunStage.TUMBLE||Stage==RunStage.SETTLED)return 0;
   return Math.Max(0,(Vy+Math.Sqrt(Vy*Vy+2*9.81*Math.Max(0,Y)))/9.81);}
 }
 readonly FlightPhysics firstArc;
 readonly double drag,bounceShape,maxSpeed;
 readonly bool floatEnabled;
 bool first=true,brace,airLatch;
 double impactAge,stillAge,settledAge,lastImpactTime=-100;

 public KickFlightPhysics(FlightSettings original,bool includeFloat=true){
  var s=original.Copy();s.Sanitize();firstArc=new FlightPhysics(s);floatEnabled=includeFloat;
  Vx=firstArc.Vx;Vy=firstArc.Vy;drag=s.Drag;
  // Same original claw/shell/legs categories. Long legs allow a little more
  // timing tolerance; big claws rebound more but already add mass and drag.
  double tolerance=1+(s.legs-45)*.002;
  BraceWindow=.25*tolerance;PerfectWindow=.10*tolerance;
  bounceShape=1+(s.claw-45)*.0018;
  maxSpeed=Math.Min(75,Math.Max(1,s.LaunchSpeed*1.05));Stage=RunStage.LAUNCH;
 }
 void Event(string name){LastEvent=name;EventID++;}
 public bool SetKickPressed(bool pressed){
  if(!pressed){Held=false;return false;}
  if(Held)return false;Held=true;
  if(Remaining<=0){EmptyPresses++;return false;}
  if(!CanKick)return false;
  Remaining--;Used++;brace=true;LastPressTime=Time;Stage=RunStage.BRACED;Event("brace");return true;
 }
 public void Step(){
  if(Finished)return;
  Time+=StepSeconds;
  if(Stage==RunStage.SETTLED){settledAge+=StepSeconds;if(settledAge>=.15){Stage=RunStage.RESULT;Distance=X;Event("result");}return;}
  if(Stage==RunStage.TUMBLE){Roll();return;}
  if(Y>.16&&Time-lastImpactTime>.065)airLatch=true;
  if(brace&&Time-LastPressTime>BraceWindow+1e-8){brace=false;EarlyMisses++;Event("miss");}
  if(first){
   firstArc.Step();X=firstArc.X;Y=firstArc.Y;Vx=firstArc.Vx;Vy=firstArc.Vy;Peak=firstArc.Peak;
   // Keep the ui1 contact interpolation and constants, including at power0.
   if(firstArc.Landed){first=false;FirstContact=X;FirstContactTime=Time;Impact();}
  }else{
   double bx=X,by=Y,speed=Speed;
   Vx-=drag*speed*Vx*StepSeconds;Vy+=(-9.81-drag*speed*Vy)*StepSeconds;
   X+=Vx*StepSeconds;Y+=Vy*StepSeconds;Peak=Math.Max(Peak,Y);
   if(Y<=0&&Vy<0){double f=by>0?by/(by-Y):0;X=bx+(X-bx)*f;Y=0;Impact();}
  }
  Distance=X;
  if(Stage==RunStage.IMPACT){impactAge+=StepSeconds;if(impactAge>=.075)Stage=brace?RunStage.BRACED:RunStage.AIRBORNE;}
  else if(Stage!=RunStage.TUMBLE&&Stage!=RunStage.SETTLED)Stage=brace?RunStage.BRACED:RunStage.AIRBORNE;
 }
 void Impact(){
  // This method is reached only after a positive flight arc crosses y=0.
  // airLatch resets here; input cannot manufacture a ground collision.
  ImpactID++;lastImpactTime=Time;airLatch=false;impactAge=0;
  double age=Time-LastPressTime;
  bool perfect=brace&&age<=PerfectWindow+1e-8;
  bool good=brace&&!perfect&&age<=BraceWindow+1e-8;
  LastGrade=perfect?"PERFECT":good?"GOOD":"LAND";
  if(perfect)Perfects++;else if(good)Goods++;else OrdinaryImpacts++;
  LastSurface=floatEnabled&&X>=FloatStart&&X<=FloatEnd?"float":"sand";
  double horizontal=perfect?.98:good?.88:.72;
  double rebound=(perfect?.66:good?.52:.32)*bounceShape*(LastSurface=="float"?1.16:1);
  double incomingEnergy=Vx*Vx+Vy*Vy;
  double nvx=Math.Max(0,Vx*horizontal),nvy=Math.Abs(Vy)*rebound;
  double outgoingEnergy=nvx*nvx+nvy*nvy;
  double limit=Math.Min(maxSpeed*maxSpeed,incomingEnergy*.985);
  if(outgoingEnergy>limit&&outgoingEnergy>0){double scale=Math.Sqrt(limit/outgoingEnergy);nvx*=scale;nvy*=scale;}
  Vx=nvx;Vy=nvy;brace=false;Stage=RunStage.IMPACT;Event(perfect?"perfect":good?"good":"land");
  if(Vy<1.65||Vx<.7){Vy=0;Stage=RunStage.TUMBLE;}
 }
 void Roll(){
  Y=0;Vy=0;double deceleration=floatEnabled&&X>=FloatStart&&X<=FloatEnd?3.5:6;
  double next=Math.Max(0,Vx-deceleration*StepSeconds);X+=(Vx+next)*.5*StepSeconds;Vx=next;Distance=X;
  if(Vx<=.45)stillAge+=StepSeconds;else stillAge=0;
  if(stillAge>=.45){Vx=0;Stage=RunStage.SETTLED;Event("settled");}
 }
 public static KickFlightPhysics Complete(FlightSettings s){
  var sim=new KickFlightPhysics(s);for(int i=0;i<7200&&!sim.Finished;i++)sim.Step();
  if(!sim.Finished)throw new InvalidOperationException("Kick flight failed to settle within60s");return sim;
 }
}
