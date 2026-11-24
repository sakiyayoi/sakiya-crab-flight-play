using System;
using System.IO;
using System.Linq;
using System.Text.Json;
public static class RulesContract {
 static int passed;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);passed++;Console.WriteLine("PASS "+name);}
 static bool Eq(float a,float b)=>BitConverter.SingleToInt32Bits(a)==BitConverter.SingleToInt32Bits(b);
 static bool Near(float a,float b)=>Math.Abs(a-b)<.0001;
 public static int Main(string[] args){
  Check(NewLoopRules.BeamSeconds>=2&&NewLoopRules.BeamSeconds<=3,"beam duration 2-3 seconds");
  Check(NewLoopRules.ChargeSeconds>=1.5&&NewLoopRules.ChargeSeconds<=2,"full charge 1.5-2 seconds");
  Check(Near(NewLoopRules.Reach(0),8)&&Near(NewLoopRules.Reach(1),16),"wave reach 8 to 16m");
  Check(NewLoopRules.Capacity(7,0)==7&&NewLoopRules.Capacity(7,1)==21,"wave capacity x1 to x3");
  Check(NewLoopRules.OrdinaryCapacity(0)==0&&NewLoopRules.OrdinaryCapacity(1)==3,"ordinary contact bounded; no automatic chain enrolment");
  Check(NewLoopRules.Cleared(38,54)&&!NewLoopRules.Cleared(37,54)&&!NewLoopRules.Cleared(0,0),"S5 clear boundary 38/54");
  Check(NewLoopRules.Cleared(29,41)&&!NewLoopRules.Cleared(28,41),"S7 clear boundary 29/41");
  Check(!NewLoopRules.ResultDue(4.9f,3.99f,true,false,false)&&NewLoopRules.ResultDue(5.91f,4,true,false,false),"release cap exactly 4s");
  Check(NewLoopRules.ResultDue(.1f,0,false,true,false)&&NewLoopRules.ResultDue(.1f,0,false,false,true),"rest and exit immediately before release");
  Check(NewLoopRules.ResultDue(2,0,true,true,false)&&NewLoopRules.ResultDue(2,0,true,false,true),"rest and exit immediately after release");
  Check(NewLoopRules.ResultDue(5.2f,0,false,false,false),"miss cannot hang forever");
  Check(!NewLoopRules.ResultDue(5.2f,2.8f,true,false,false),"4s destruction allowance not replaced by global cutoff");
  foreach(var r in new[]{"boost30","boost45","boost60","jackpot"})Check(NewLoopRules.Ignition(r),"ignition "+r);
  foreach(var r in new[]{"","redirect30","resource","hazard","landmark","preview"})Check(!NewLoopRules.Ignition(r),"not ignition "+(r.Length==0?"(ordinary)":r));
  Check(Near(NewLoopRules.SlowSeconds,.3f)&&Near(NewLoopRules.SlowScale,.25f),"release slow is 0.25x for 0.30 real seconds");
  Check(Near(NewLoopRules.CameraPunchScale,.72f)&&Near(NewLoopRules.CameraPunchSeconds,.3f),"camera punch 0.72x / 0.30s");
  Check(Near(NewLoopRules.VisualSpin(0),2)&&Near(NewLoopRules.VisualSpin(1),32)&&NewLoopRules.VisualSpin(.5f)<NewLoopRules.VisualSpin(.9f)/2,"steep visual spin 2 to 32 rps");
  Check(NewLoopRules.VisualSpin(.9f)-NewLoopRules.VisualSpin(.8f)>NewLoopRules.VisualSpin(.2f)-NewLoopRules.VisualSpin(.1f),"exponentially increasing charge intensity");
  Check(NewLoopRules.ShardSpeedMin>18&&NewLoopRules.ShardSpeedMax==28,"explosion debris faster than old 18m/s cap");
  var line=Enumerable.Range(0,5).ToArray();
  var linePlan=NewLoopRules.WavePlan(0,line,0,true,(a,b)=>Math.Abs(a-b)*7.9f,a=>a.ToString());
  Check(linePlan.Count==5&&linePlan.Select(p=>p.hop).SequenceEqual(line),"graph chain hop order, including bridges");
  Check(linePlan.All(p=>Near(NewLoopRules.HopDue(1,p.hop),1+.04f*p.target)),"each hop staggered by 0.04s");
  using(var doc=JsonDocument.Parse(File.ReadAllText(args[0]))){
   var instances=doc.RootElement.GetProperty("instances").EnumerateArray().ToArray();
   Func<JsonElement,JsonElement,float> dist=(a,b)=>{
    var x=a.GetProperty("p").EnumerateArray().Select(v=>v.GetSingle()).ToArray();var y=b.GetProperty("p").EnumerateArray().Select(v=>v.GetSingle()).ToArray();
    return (float)Math.Sqrt(x.Zip(y,(v,w)=>(v-w)*(v-w)).Sum());
   };
   Func<JsonElement,string> id=a=>a.GetProperty("id").GetString();
   foreach(var beat in new[]{"S5","S7"})foreach(var energy in new[]{0f,.5f,1f}){
    var members=instances.Where(p=>p.GetProperty("beat").GetString()==beat).ToArray();
    foreach(var ignition in members.Where(p=>NewLoopRules.Ignition(p.GetProperty("role").GetString()))){
     string chain=ignition.GetProperty("chain").GetString();var set=members.Where(p=>p.GetProperty("chain").GetString()==chain).ToArray();
     var plan=NewLoopRules.WavePlan(ignition,set,energy,true,dist,id);
     Check(plan.Count==set.Length&&plan.Select(p=>id(p.target)).Distinct().Count()==set.Length,beat+" ignition "+id(ignition)+" full authored "+chain+" at "+energy);
     Check(plan.Zip(plan.Skip(1),(a,b)=>a.hop<=b.hop).All(v=>v),"authored ignition breadth-first ordering");
    }
    foreach(var ordinary in members.Where(p=>!NewLoopRules.Ignition(p.GetProperty("role").GetString()))){
     string chain=ordinary.GetProperty("chain").GetString();var set=members.Where(p=>id(p)==id(ordinary)||chain.Length>0&&p.GetProperty("chain").GetString()==chain).ToArray();
     var plan=NewLoopRules.WavePlan(ordinary,set,energy,false,dist,id);
     if(plan.Count>1+NewLoopRules.OrdinaryCapacity(energy)||plan.Skip(1).Any(p=>p.hop!=1||dist(ordinary,p.target)>NewLoopRules.Reach(energy)))throw new Exception("ordinary wave escaped radius/cap");
    }
    Check(true,beat+" every ordinary target bounded at "+energy);
   }
  }
  var c=new SpinCharge{NewLoop=true};c.Begin();
  Check(c.Ready&&c.Energy==0,"zero-energy throw allowed");
  c.Add(20,.1f);Check(c.Energy==0,"swipes cannot create parallel energy stock");
  for(int i=0;i<9;i++)c.Tick(.1f);Check(Near(c.Energy,.5f)&&c.Tier==2,"middle tier at half energy");
  int frames=0;while(!c.BeamArmed&&frames++<12)c.Tick(.1f);
  Check(c.BeamArmed&&c.Tier==3&&c.Elapsed<=2.01f,"100 percent arms beam and third tier");
  c.Tick(.05f);Check(c.OverSpun&&!c.BeamArmed&&c.Energy<1&&c.Dizzy==1,"over-spin decays and disarms with wobble");
  float e=c.Energy;c.Held=false;c.Tick(.1f);Check(c.Energy==e&&c.Dizzy==0&&c.OverSpun,"release escape stops sway; penalty stays latched");
  c.Begin();for(int i=0;i<12;i++)c.Tick(.1f);Check(c.Energy<e&&c.Dizzy==0&&c.OverSpun&&!c.BeamArmed,"no mid-throw heal or rearm; wobble ends after about 1s");
  c.Reset();Check(c.Energy==0&&!c.OverSpun&&!c.Started&&!c.Held,"retry starts empty and clears throw-local penalty");
  c.Begin();c.Tick(float.NaN);Check(c.Energy==0,"non-finite input does not poison stock");
  Check(NewLoopRules.Wobble(true,false,.2f)==0&&NewLoopRules.Wobble(true,true,1.01f)==0,"sway escape and bounded duration");
  // Compare actual current OFF implementation against actual base source, under the same scalar shim.
  for(int mask=0;mask<8;mask++){
   var a=new SpinCharge{NewLoop=false,ContinuousWork=(mask&1)!=0,EnhancedSpinR16=(mask&2)!=0,PowerScale=(mask&4)!=0?28f/22:1,BodyLimit=42};
   var b=new LegacySpinCharge{ContinuousWork=a.ContinuousWork,EnhancedSpinR16=a.EnhancedSpinR16,PowerScale=a.PowerScale,BodyLimit=a.BodyLimit};a.Begin();b.Begin();
   for(int i=0;i<1200;i++){float distance=(i%17)*.07f,dt=(i%11+1)*.001f;a.Add(distance,dt);b.Add(distance,dt);a.Tick(.013f);b.Tick(.013f);
    if(!Eq(a.Work,b.Work)||!Eq(a.Energy,b.Energy)||!Eq(a.Elapsed,b.Elapsed)||!Eq(a.SpinRps,b.SpinRps)||!Eq(a.BodySpinRps,b.BodySpinRps)||a.Ready!=b.Ready)throw new Exception("legacy charge drift "+mask+"/"+i);
   }a.Reset();b.Reset();Check(Eq(a.Work,b.Work)&&a.Started==b.Started,"OFF SpinCharge bit comparison configuration "+mask+" (1200 steps)");
  }
  Console.WriteLine("TOTAL "+passed+" passed / 0 failed");return 0;
 }
}
