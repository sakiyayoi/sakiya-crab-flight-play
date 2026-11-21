using System;

// Engine-independent visual simulation. No rigid bodies, input, sound, or game state.
namespace CrabDestructionVfx {
 public enum Surface { Glass, Wood, Metal, Plaster, Ceramic, Food }
 public enum Mark { Shard, Splinter, Chip, Spark, Dust, Ghost, Arc, Wind, Masonry, Ceramic, Food }
 public struct V3 {
  public float x,y,z;
  public V3(float x,float y,float z=0){this.x=x;this.y=y;this.z=z;}
  public static V3 operator +(V3 a,V3 b){return new V3(a.x+b.x,a.y+b.y,a.z+b.z);}
  public static V3 operator -(V3 a,V3 b){return new V3(a.x-b.x,a.y-b.y,a.z-b.z);}
  public static V3 operator *(V3 a,float b){return new V3(a.x*b,a.y*b,a.z*b);}
  public bool Finite {get{return F(x)&&F(y)&&F(z);}}
  public static bool F(float n){return !float.IsNaN(n)&&!float.IsInfinity(n);}
 }
 public struct Ink {
  public float r,g,b,a;
  public Ink(float r,float g,float b,float a=1){this.r=r;this.g=g;this.b=b;this.a=a;}
 }
 [Serializable] public sealed class VfxTuning {
  public int poolLimit=320;
  public float intensity=1, dustOpacity=.16f, ghostOpacity=.14f;
  public float chargeGhostLife=.42f, hitCooldown=.045f, chainWindow=.50f;
  public bool reducedMotion=false;
  public void Sanitize(){
   poolLimit=Math.Max(32,Math.Min(384,poolLimit));
   intensity=Safe(intensity,0,1.25f,1);dustOpacity=Safe(dustOpacity,0,.22f,.16f);
   ghostOpacity=Safe(ghostOpacity,0,.14f,.14f);chargeGhostLife=Safe(chargeGhostLife,.3f,.55f,.42f);
   hitCooldown=Safe(hitCooldown,.04f,.2f,.045f);chainWindow=Safe(chainWindow,.2f,.7f,.5f);
  }
  static float Safe(float f,float lo,float hi,float fallback){return V3.F(f)?Math.Max(lo,Math.Min(hi,f)):fallback;}
 }
 public struct Fleck {
  public bool live; public Mark mark; public V3 origin,position,velocity;
  public Ink ink; public float age,life,size,aspect,angle,spin,gravity,drag;
  public float Alpha {get{
   float t=age/life;float attack=Math.Min(1,t/.14f);float tail=1-t;
   return ink.a*attack*tail*tail;
  }}
 }
 public sealed class CrabVfxCore {
  public const string Revision="crab-vfx-material-impact-r16";
  public readonly VfxTuning Tuning;
  public readonly Fleck[] Particles;
  public int Active {get;private set;} public int Peak {get;private set;}
  public int Dropped {get;private set;} public int AcceptedBreaks {get;private set;}
  public int Chain {get;private set;} public int ChargeStage {get;private set;}
  public bool Paused {get;private set;} public bool Reduced {get;private set;}
  public bool HasLaunched {get;private set;} public bool HasExited {get;private set;}
  public float Clock {get;private set;}
  uint rng;int cursor,lastBreak=-1,burstPriority,burstPromotions;float lastHit=-100,lastReceipt=-100,chargeNext;bool readySeen,acceptEvents=true;
  public CrabVfxCore(VfxTuning tuning=null,uint seed=0xC4ABu){
   Tuning=tuning??new VfxTuning();Tuning.Sanitize();Particles=new Fleck[Tuning.poolLimit];
   Reset(seed);Reduced=Tuning.reducedMotion;
  }
  public void Reset(uint seed=0xC4ABu){
   Array.Clear(Particles,0,Particles.Length);Active=Peak=Dropped=AcceptedBreaks=Chain=ChargeStage=0;
   rng=seed==0?1:seed;cursor=burstPriority=burstPromotions=0;lastBoost=-1;lastBreak=-1;lastHit=lastReceipt=-100;chargeNext=0;Clock=0;
   readySeen=HasLaunched=HasExited=Paused=false;acceptEvents=true;
  }
  public void SetPaused(bool paused){Paused=paused;}
  public void SetReducedMotion(bool reduced){
   if(Reduced==reduced)return;Reduced=reduced;
   if(!reduced)return;
   for(int i=0;i<Particles.Length;i++){
    var p=Particles[i];if(!p.live)continue;
    if(p.mark==Mark.Ghost||p.mark==Mark.Arc||p.mark==Mark.Wind||p.mark==Mark.Spark){Kill(i);continue;}
    p.velocity=p.velocity*.35f;p.spin*=.25f;p.life=Math.Min(p.life,p.age+.45f);Particles[i]=p;
   }
  }
  // Start a fresh charge by Reset first. Elapsed and energy are observations from gameplay.
  public void Charge(V3 at,float elapsed,float energy,float angleDegrees,bool active,bool ready){
   if(!acceptEvents||Paused||HasLaunched||!at.Finite||!V3.F(elapsed)||!V3.F(energy)||!V3.F(angleDegrees))return;
   if(!active){ChargeStage=0;return;}
   float e=Clamp(energy,0,1);ChargeStage=elapsed<3.3f?1:elapsed<7?2:3;
   if(ready&&!readySeen){
    readySeen=true;
    for(int i=0;i<2;i++)Add(Mark.Arc,at,new V3(),new Ink(.98f,.66f,.27f,Reduced?.28f:.42f),.72f,2.45f,1,i*180,0,0,0);
   }
   // Four fades per second maximum. Work intensity, never elapsed alone, powers the wake.
   if(Reduced||ready||e<.03f||Clock<chargeNext)return;
   chargeNext=Clock+1f/(ChargeStage==1?2:ChargeStage==2?3:4);
   Add(Mark.Ghost,at,new V3(),new Ink(.62f,.12f,.035f,Tuning.ghostOpacity*(.35f+.65f*e)),
       Tuning.chargeGhostLife,1.6f+.22f*e,1,angleDegrees,0,0,0);
   if(ChargeStage>=2)Add(Mark.Arc,at,new V3(),new Ink(.78f,.35f,.08f,.32f*e),.40f,2.25f+.2f*e,1,angleDegrees+35,0,0,0);
   if(ChargeStage==3)Add(Mark.Ghost,at,new V3(),new Ink(.62f,.12f,.035f,Tuning.ghostOpacity*.6f*e),
       Tuning.chargeGhostLife*.8f,1.65f+.22f*e,1,angleDegrees-32,0,0,0);
  }
  public void Launch(V3 at,V3 direction,float energy){
   if(!acceptEvents||Paused||HasLaunched||!at.Finite||!direction.Finite||!V3.F(energy))return;
   HasLaunched=true;RemoveCharge();float e=Clamp(energy,0,1);
   float a=Heading(direction);int count=Reduced?5:12;
   for(int i=0;i<count;i++){
    float t=a+180+Range(-48,48);float speed=Range(1,4)*(Reduced?.35f:1)*(.5f+.5f*e);
    Add(i%3==0?Mark.Dust:Mark.Splinter,at+new V3(Range(-.3f,.3f),Range(-.2f,.2f),Range(-.3f,.3f)),Dir(t)*speed,
        i%3==0?new Ink(.75f,.65f,.42f,Tuning.dustOpacity):new Ink(1,.72f,.36f,.60f),
        Range(.30f,.58f),i%3==0?.30f:.17f,i%3==0?1:2,t,Range(-45,45),.2f,1.8f);
   }
   if(!Reduced)for(int i=0;i<2;i++)Add(Mark.Arc,at,new V3(),new Ink(1,.77f,.42f,.5f),.38f,2.2f,1,a+110+i*140,0,0,0);
  }
  // Acceleration cue uses its own bounded token; Launch is intentionally one-shot.
  int lastBoost=-1;
  public void Boost(int token,V3 at,V3 direction,float strength){
   if(!acceptEvents||Paused||token<=lastBoost||!at.Finite||!direction.Finite)return;lastBoost=token;
   float a=Heading(direction),e=Clamp(strength,0,1);int n=Reduced?2:8;
   for(int i=0;i<n;i++)Add(Mark.Wind,at+new V3(0,Range(-.7f,.7f),-.2f),Dir(a+180)*Range(7,12),new Ink(1,.72f,.22f,Reduced?.25f:.55f),Reduced?.12f:.25f,.12f+e*.10f,4,a,0,0,0);
  }
  // One monotonically increasing token per broken panel, once the physics solver commits it.
  // The return value says accepted visually; it never reports gameplay destruction success.
  public bool Break(int eventToken,Surface material,V3 at,V3 incoming,float strength){
   return Break(eventToken,material,at,incoming,new V3(),strength);
  }
  public bool Break(int eventToken,Surface material,V3 at,V3 incoming,V3 normal,float strength){
   if(!acceptEvents||Paused||eventToken<=lastBreak||!at.Finite||!incoming.Finite||!normal.Finite||!V3.F(strength))return false;
   lastBreak=eventToken;AcceptedBreaks++;
   float gap=Clock-lastHit;Chain=Clock-lastReceipt<Tuning.chainWindow?Math.Min(6,Chain+1):1;lastReceipt=Clock;
   int priority=material==Surface.Plaster?4:material==Surface.Metal?3:material==Surface.Food?0:2;
   bool promotion=gap<Tuning.hitCooldown;
   if(promotion){
    // One stronger material can replace the visual emphasis of an incidental
    // light hit. Same-frame chains never spawn an unbounded second shower.
    if(priority<=burstPriority||burstPromotions>=1){Dropped++;return true;}
    burstPromotions++;
   }else {lastHit=Clock;burstPromotions=0;}
   burstPriority=priority;
   float power=Clamp(strength,0,1)*Tuning.intensity;
   float scale=.55f+.65f*power;float incomingAngle=Heading(incoming);
   float a=normal.x*normal.x+normal.y*normal.y>.001f?Heading(normal):incomingAngle;
   float motion=Reduced?.32f:1;
   int n=material==Surface.Glass?18:material==Surface.Wood?15:material==Surface.Metal?10:material==Surface.Food?9:16;
   n=Math.Max(3,(int)Math.Round(n*(.55f+.45f*power)*(Reduced?.45f:1)));
   if(promotion)n=Math.Max(3,n/2);
   for(int i=0;i<n;i++){
    Mark mark=material==Surface.Wood?Mark.Splinter:material==Surface.Metal?Mark.Chip:material==Surface.Plaster?Mark.Masonry:material==Surface.Ceramic?Mark.Ceramic:material==Surface.Food?Mark.Food:Mark.Shard;
    // Glass continues through the impact; masonry kicks back from the recorded
    // normal, wood tumbles in long strips, and metal stays dense and short.
    float spread=material==Surface.Glass?48:material==Surface.Metal?66:material==Surface.Wood?78:100;
    float fan=material==Surface.Glass&&i%3!=0?incomingAngle:a;
    Ink color=ColorFor(material,i);float ang=fan+Range(-spread,spread);
    float speed=Range(1.4f,7.3f)*scale*motion*(material==Surface.Metal?.72f:material==Surface.Food?.62f:1);
    float size=material==Surface.Glass?Range(.07f,.22f):material==Surface.Wood?Range(.10f,.25f):Range(.09f,.25f);
    Add(mark,at+new V3(Range(-.16f,.16f),Range(-.16f,.16f),Range(-.28f,.28f)),
        Dir(ang)*speed+new V3(0,Range(.5f,1.8f)*motion,Range(-.8f,.8f)*motion),color,
        Range(Reduced?.35f:.42f,Reduced?.62f:.95f),size,material==Surface.Wood?3.2f:material==Surface.Metal?1.7f:1,
        Range(0,360),Range(-165,165)*motion,material==Surface.Glass?6:8,material==Surface.Metal?1.6f:.7f);
   }
   int dust=material==Surface.Plaster?7:material==Surface.Wood?4:material==Surface.Ceramic?3:material==Surface.Glass?2:1;
   if(Reduced)dust=Math.Min(dust,2);
   for(int i=0;i<dust;i++)Add(Mark.Dust,at+new V3(Range(-.25f,.25f),Range(-.2f,.2f),Range(-.4f,.4f)),
      new V3(Range(-1,1),Range(.3f,1.4f),0)*motion,
      new Ink(material==Surface.Glass?.62f:.72f,material==Surface.Glass?.83f:.60f,material==Surface.Glass?.83f:.43f,Tuning.dustOpacity),
      Range(.48f,Reduced?.65f:1.05f),Range(.24f,.47f),1,Range(0,360),Range(-12,12),-.2f,1.7f);
   if(material==Surface.Metal&&!Reduced)for(int i=0;i<5;i++){
    float ang=a+(i%2==0?78:-78)+Range(-20,20);
    Add(Mark.Spark,at,Dir(ang)*Range(6,10),new Ink(1,.72f,.30f,.88f),Range(.13f,.24f),.11f,4,ang,0,2,3);
   }
   // A pair of contact-local brush slashes reads as escalating damage, without HUD, flash or camera shake.
   if(Chain>=2&&!Reduced)for(int i=0;i<2;i++)Add(Mark.Wind,at+new V3(0,(i==0?-1:1)*.7f,0),Dir(a)*.6f,
      new Ink(1,.54f,.18f,.38f),.28f,.28f+Chain*.045f,3,a+15*(i==0?-1:1),0,0,0);
   return true;
  }
  public void ExitRestaurant(V3 at,V3 direction){
   if(!acceptEvents||Paused||!HasLaunched||HasExited||!at.Finite||!direction.Finite)return;
   HasExited=true;float a=Heading(direction);V3 side=Dir(a+90);
   // Clear indoor dust immediately: the breakthrough gets air and negative space.
   for(int i=0;i<Particles.Length;i++)if(Particles[i].live&&Particles[i].mark==Mark.Dust)Kill(i);
   if(Reduced)return;
   for(int sign=-1;sign<=1;sign+=2)for(int i=0;i<4;i++){
    V3 pos=at+side*(sign*(1.9f+i*.34f))+Dir(a)*(i*.45f-.8f);
    Add(Mark.Wind,pos,Dir(a)*(-1.1f-i*.3f),new Ink(.80f,.92f,.85f,.34f-i*.045f),
        .45f+i*.09f,.38f,5+i*.5f,a,0,0,0);
   }
  }
  public void Stop(bool clearImmediately=false){acceptEvents=false;RemoveCharge();if(clearImmediately){Array.Clear(Particles,0,Particles.Length);Active=0;}}
  public void Step(float dt){
   if(Paused||!V3.F(dt)||dt<=0)return;
   // Clamp stalls to avoid teleporting visual fragments after focus return. Game clocks are untouched.
   dt=Math.Min(dt,.1f);Clock+=dt;
   for(int i=0;i<Particles.Length;i++){
    var p=Particles[i];if(!p.live)continue;p.age+=dt;
    if(p.age>=p.life){Kill(i);continue;}
    p.velocity.y-=p.gravity*dt;p.velocity=p.velocity*(1f/(1f+p.drag*dt));
    p.position=p.position+p.velocity*dt;p.angle+=p.spin*dt;
    Particles[i]=p;
   }
  }
  void RemoveCharge(){for(int i=0;i<Particles.Length;i++)if(Particles[i].live&&(Particles[i].mark==Mark.Ghost||Particles[i].mark==Mark.Arc))Kill(i);ChargeStage=0;}
  void Kill(int i){if(Particles[i].live){var p=Particles[i];p.live=false;Particles[i]=p;Active--;}}
  void Add(Mark mark,V3 pos,V3 vel,Ink color,float life,float size,float aspect,float angle,float spin,float gravity,float drag){
   if(Tuning.intensity<=0)return;
   int index=-1;
   for(int i=0;i<Particles.Length;i++){int k=(cursor+i)%Particles.Length;if(!Particles[k].live){index=k;break;}}
   if(index<0){Dropped++;return;}cursor=(index+1)%Particles.Length;
   Particles[index]=new Fleck{live=true,mark=mark,origin=pos,position=pos,velocity=vel,ink=color,life=life,size=size,aspect=aspect,angle=angle,spin=spin,gravity=gravity,drag=drag};
   Active++;Peak=Math.Max(Peak,Active);
  }
  public static Ink ColorFor(Surface s,int variant){
   float v=(variant%3)*.08f;
   switch(s){
    case Surface.Glass:return new Ink(.43f+v,.77f+v,.80f+v,.68f);
    case Surface.Wood:return new Ink(.46f+v,.26f+v*.7f,.10f+v*.35f,.9f);
    case Surface.Metal:return new Ink(.39f+v,.46f+v,.47f+v,.92f);
    case Surface.Plaster:return new Ink(.76f+v,.56f+v,.36f+v,.84f);
    case Surface.Ceramic:return new Ink(.87f+v*.3f,.86f+v*.3f,.73f+v*.3f,.9f);
    default:return variant%2==0?new Ink(.83f,.30f,.12f,.9f):new Ink(.54f,.63f,.24f,.9f);
   }
  }
  static float Heading(V3 v){return (float)(Math.Atan2(v.y,v.x)*180/Math.PI);}
  static V3 Dir(float degrees){double r=degrees*Math.PI/180;return new V3((float)Math.Cos(r),(float)Math.Sin(r));}
  static float Clamp(float n,float lo,float hi){return Math.Max(lo,Math.Min(hi,n));}
  float Range(float lo,float hi){rng^=rng<<13;rng^=rng>>17;rng^=rng<<5;return lo+(hi-lo)*(rng&0xffffff)/16777215f;}
 }
}
