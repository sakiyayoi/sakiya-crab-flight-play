using System;

// Stage 1: deterministic gameplay data. Coordinates are authored on +x only.
public static class ObjectRules {
 public enum Kind { Boost, Bounce, Spin, Brake, Blast, Death }
 public sealed class Definition {
  public readonly Kind kind; public readonly string color,shape,motion;
  public readonly float amount,amplitude,hertz,burstScale;
  public Definition(Kind k,string c,string s,string m,float v,float a,float hz,float fx){kind=k;color=c;shape=s;motion=m;amount=v;amplitude=a;hertz=hz;burstScale=fx;}
 }
 public static readonly Definition[] Definitions={
  new Definition(Kind.Boost,"#0000FF","arrow","slide",24,.65f,.8f,2.4f),
  new Definition(Kind.Bounce,"#00FF00","spring","bounce",26,.55f,1.2f,2.8f),
  new Definition(Kind.Spin,"#FFFF00","propeller","rotate",240,0,1.6f,3.0f),
  new Definition(Kind.Brake,"#8000FF","cloud","float",.2f,.35f,.3f,2.2f),
  new Definition(Kind.Blast,"#FF8000","barrel","blink-bob",30,.18f,2,3.6f),
  new Definition(Kind.Death,"#FF00FF","spikes","fast-slide",0,.9f,2,4.0f)
 };
 public sealed class Placement {
  public readonly string id; public readonly Kind kind; public readonly float x,y,z;
  public Placement(string id,Kind k,float x,float y,float z=0){this.id=id;kind=k;this.x=x;this.y=y;this.z=z;}
 }
 public static readonly Placement[] Placements={
  new Placement("restaurant-entry",Kind.Boost,8,3),
  new Placement("restaurant-bounce",Kind.Bounce,14,2),
  new Placement("restaurant-brake",Kind.Brake,18,10),
  new Placement("restaurant-spin",Kind.Spin,20,5),
  new Placement("restaurant-death",Kind.Death,22,26),
  new Placement("restaurant-exit",Kind.Blast,24,3),
  new Placement("mall-entry",Kind.Boost,32,4),
  new Placement("mall-bounce-a",Kind.Bounce,43,2),
  new Placement("mall-spin-a",Kind.Spin,55,7),
  new Placement("mall-brake-a",Kind.Brake,64,9),
  new Placement("mall-boost",Kind.Boost,75,4),
  new Placement("mall-bounce-b",Kind.Bounce,86,2),
  new Placement("mall-spin-b",Kind.Spin,98,7),
  new Placement("mall-brake-b",Kind.Brake,108,9),
  new Placement("mall-death",Kind.Death,114,26),
  new Placement("mall-exit",Kind.Blast,124,4)
 };
 public static readonly float[] BandX={11,17,27,38,49,61,80,92,104,130};
 public static readonly float[] BandY={2,5,8};
 public const float HalfSize=1.15f, Emission=4, SpinSeconds=3, SpinMultiplier=2;
 public const float BlastRadius=9, BandRetention=.995f, BandThreshold=.03f;
 public const float JetDeltaSpeed=14, JetUp=.55f, DiveAcceleration=38;
 public const int JetLimit=2;
 // Static design envelope, not a measured flight assertion: default 40deg/70,
 // no actions, authored y=0..20 band. This is a placement contract,
 // not a guarantee for every launch or after bounce/jet. Add 3.5m crab extent.
 public const float StandardBandMinY=0, StandardBandMaxY=20, CrabMargin=3.5f;
 public const float ChainWindow=1.5f;
 public static bool Enabled(string mode)=>string.Equals(mode,"redesign",StringComparison.Ordinal);
 public static bool InOpening(float x)=>Math.Abs(x)>4.5f&&Math.Abs(x)<135;
 public static bool KeepLegacyPanel(string mode,float x)=>!Enabled(mode)||!InOpening(x);
 public static Definition Get(Kind k)=>Definitions[(int)k];
 public static float BounceDelta(float vy,bool spin)=>Math.Max(0,Get(Kind.Bounce).amount*(spin?SpinMultiplier:1)-vy);
 public sealed class Run {
  public int Jets {get;private set;}=JetLimit;
  public int Chain {get;private set;}
  public int MaxChain {get;private set;}
  public bool Dive,JetPending,Dead;
  public float SpinUntil {get;private set;}
  float lastBreak=float.NegativeInfinity;
  public void Reset(){Jets=JetLimit;Chain=MaxChain=0;Dive=JetPending=Dead=false;SpinUntil=0;lastBreak=float.NegativeInfinity;}
  public bool RequestJet(bool active){if(!active||Dead||Jets<=0||JetPending)return false;Jets--;JetPending=true;return true;}
  public bool Spinning(float time)=>!Dead&&time<SpinUntil;
  public void Spin(float time){SpinUntil=time+SpinSeconds;}
  public void Release(){Dive=JetPending=false;}
  public void Kill(){Dead=true;Release();}
  public void Break(float time){if(time<lastBreak||time-lastBreak>ChainWindow)Chain=0;lastBreak=time;Chain++;MaxChain=Math.Max(MaxChain,Chain);}
 }
}
