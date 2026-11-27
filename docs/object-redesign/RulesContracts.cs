using System;
using System.Linq;
public static class RulesContracts {
 static int pass;
 static void Check(bool value,string name){if(!value)throw new Exception("FAIL "+name);pass++;Console.WriteLine("PASS "+name);}
 static void Main(){
  var expected=new[]{"#0000FF","#00FF00","#FFFF00","#8000FF","#FF8000","#FF00FF"};
  var amount=new[]{24f,26f,240f,.2f,30f,0f};
  var shapes=new[]{"arrow","spring","propeller","cloud","barrel","spikes"};
  var motion=new[]{"slide","bounce","rotate","float","blink-bob","fast-slide"};
  Check(ObjectRules.Definitions.Length==6,"six unique kinds");
  for(int i=0;i<6;i++){var d=ObjectRules.Get((ObjectRules.Kind)i);
   Check(d.color==expected[i]&&d.amount==amount[i]&&d.shape==shapes[i]&&d.motion==motion[i],"definition/color/constant "+d.kind);
  }
  Check(ObjectRules.Emission==4,"strong fixed emission");
  Check(ObjectRules.Placements.Length>=14&&ObjectRules.Placements.Length<=20,"14 to 20 authored +x objects");
  Check(ObjectRules.Placements.Select(x=>x.id).Distinct().Count()==16,"unique placements");
  Check(ObjectRules.Placements.Select(x=>x.kind).Distinct().Count()==6,"all six kinds placed");
  foreach(var p in ObjectRules.Placements){
   var d=ObjectRules.Get(p.kind);float extent=ObjectRules.HalfSize+d.amplitude;
   Check(p.x-extent>4.5f&&p.x+extent<135,"entire movement stays in opening "+p.id);
   float moveX=(d.motion=="slide"||d.motion=="fast-slide")?d.amplitude:0;
   float moveY=(d.motion=="slide"||d.motion=="fast-slide"||d.motion=="rotate")?0:d.amplitude;
   bool clear=true;
   foreach(float bx in ObjectRules.BandX)foreach(float by in ObjectRules.BandY)
    if(bx<p.x&&p.x-moveX-ObjectRules.HalfSize-(bx+.25f)<.5f&&
       by+1+.5f>p.y-moveY-ObjectRules.HalfSize&&by-1-.5f<p.y+moveY+ObjectRules.HalfSize)clear=false;
   Check(clear,"at least 0.5m clear before action swept envelope "+p.id);
   if(p.kind==ObjectRules.Kind.Death)Check(p.y-ObjectRules.HalfSize>ObjectRules.StandardBandMaxY+ObjectRules.CrabMargin,"death outside full standard band + crab margin "+p.id);
  }
  Check(!ObjectRules.Enabled(null)&&!ObjectRules.Enabled("")&&!ObjectRules.Enabled("legacy")&&!ObjectRules.Enabled("Redesign"),"default and invalid mode stay legacy");
  Check(ObjectRules.Enabled("redesign"),"explicit redesign");
  foreach(float x in new[]{-500f,-135f,-134f,-28f,-4.5f,0,4.5f,8,28,134,135,600}){
   Check(ObjectRules.KeepLegacyPanel("legacy",x),"legacy retains position "+x);
   Check(ObjectRules.KeepLegacyPanel("redesign",x)==!ObjectRules.InOpening(x),"redesign only opening "+x);
  }
  Check(ObjectRules.BounceDelta(-40,false)==66&&ObjectRules.BounceDelta(10,false)==16&&ObjectRules.BounceDelta(30,false)==0,"upward rebound independent of incidence and never slows a higher rise");
  Check(ObjectRules.BounceDelta(-40,true)==92&&ObjectRules.SpinMultiplier==2,"spin doubles rebound and crushing");
  var r=new ObjectRules.Run();r.Reset();
  Check(!r.RequestJet(false)&&r.Jets==2,"no input outside flight");
  Check(r.RequestJet(true)&&r.Jets==1,"first jet");
  Check(!r.RequestJet(true)&&r.Jets==1,"pending duplicate rejected");r.JetPending=false;
  Check(r.RequestJet(true)&&r.Jets==0,"second jet");r.JetPending=false;
  Check(!r.RequestJet(true)&&r.Jets==0,"third jet rejected");
  r.Dive=true;r.Release();Check(!r.Dive&&!r.JetPending,"pause/blur release");
  r.Spin(10);Check(r.Spinning(10)&&r.Spinning(12.999f)&&!r.Spinning(13),"exact three simulation seconds");
  r.Spin(12);Check(r.Spinning(14)&&!r.Spinning(15),"new spin refresh is bounded");
  r.Break(1);r.Break(2);r.Break(3.5f);Check(r.MaxChain==3,"1.5 second chain including boundary");
  r.Break(5.01f);Check(r.Chain==1&&r.MaxChain==3,"gap restarts current but retains max");
  r.Kill();Check(r.Dead&&!r.Dive&&!r.RequestJet(true)&&!r.Spinning(12),"death cancels actions");
  r.Reset();Check(r.Jets==2&&!r.Dead&&!r.Dive&&r.MaxChain==0&&!r.Spinning(1),"retry resets all run state");
  Console.WriteLine(pass+" passed / 0 failed");
 }
}
