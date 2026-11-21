using System;

[Serializable] public class CrabBuild {
 public float claw=45, shell=45, legs=45;
}
[Serializable] public class FlightSettings : CrabBuild {
 public float angle=40, power=70;public int upgradeLevel,gear;
 public void Sanitize(){upgradeLevel=Math.Clamp(upgradeLevel,0,3);gear=Math.Clamp(gear,0,3);claw=Safe(claw,0,100);shell=Safe(shell,0,100);legs=Safe(legs,0,100);angle=Safe(angle,10,80);power=Safe(power,0,100);}
 public static float Safe(float x,float lo,float hi){return float.IsNaN(x)||float.IsInfinity(x)?lo:Math.Max(lo,Math.Min(hi,x));}
 public FlightSettings Copy(){return new FlightSettings{claw=claw,shell=shell,legs=legs,angle=angle,power=power,upgradeLevel=upgradeLevel,gear=gear};}
 // Prototype hypotheses: a larger claw adds launch impulse and drag; shell
 // mass lowers initial speed but resists aerodynamic deceleration.
 public double Mass => .8+shell*.012+claw*.004;
 public double LaunchSpeed => (1+upgradeLevel*.04)*power*.49*(.86+claw*.0048)/Math.Pow(Mass,.32);
 public double Drag => (.006+claw*.00012+legs*.000055)/Mass;
}
public sealed class FlightPhysics {
 public const double StepSeconds=1.0/120.0;
 public double X,Y,Vx,Vy,Time,Peak,Distance;
 public bool Landed;
 readonly double drag;
 public FlightPhysics(FlightSettings original){var s=original.Copy();s.Sanitize();double a=s.angle*Math.PI/180;Vx=s.LaunchSpeed*Math.Cos(a);Vy=s.LaunchSpeed*Math.Sin(a);drag=s.Drag;}
 public void Step(){
  if(Landed)return;
  double beforeX=X,beforeY=Y,speed=Math.Sqrt(Vx*Vx+Vy*Vy),dt=StepSeconds;
  Vx-=drag*speed*Vx*dt;Vy+=(-9.81-drag*speed*Vy)*dt;X+=Vx*dt;Y+=Vy*dt;Time+=dt;Peak=Math.Max(Peak,Y);
  if(Y<=0){double f=beforeY>0?beforeY/(beforeY-Y):0;Distance=beforeX+(X-beforeX)*f;X=Distance;Y=0;Landed=true;}
 }
 public static FlightPhysics Complete(FlightSettings s){var sim=new FlightPhysics(s);for(int i=0;i<3600&&!sim.Landed;i++)sim.Step();if(!sim.Landed)throw new InvalidOperationException("Flight did not land within30seconds");return sim;}
}
