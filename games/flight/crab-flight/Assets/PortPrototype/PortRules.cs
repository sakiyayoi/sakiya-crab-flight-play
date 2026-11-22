using System;
// Pure calculations shared by the runtime and the license-independent contract test.
public static class PortRules {
 public struct Velocity { public float x,y,z;public Velocity(float a,float b,float c){x=a;y=b;z=c;} }
 public static float Clamp(float v,float a,float b)=>Math.Max(a,Math.Min(b,v));
 public static float Zoom(float vx,float height,float aspect)=>Clamp(9+Math.Abs(vx)*.16f+Math.Max(0,height)*.10f,aspect<1?12:10,30);
 public static float Lead(float vx,float aspect,float size)=>Math.Min(Clamp(Math.Abs(vx)*.22f*(aspect<1?1.25f:1),3,18),size*aspect*.45f);
 public static Velocity Role(string role,Velocity v,int direction,bool grounded,ref bool airBoost,out bool reversed){
  reversed=false;if(grounded)airBoost=false;
  bool boost=role.StartsWith("boost",StringComparison.Ordinal)||role=="jackpot";
  if(boost||role=="redirect30"){
   float angle=role=="boost60"?60:role=="boost30"||role=="redirect30"?30:45;
   // Preserve the Z component and XY speed for angle-only redirects.
   float speed=(float)Math.Sqrt(v.x*v.x+v.y*v.y)+(boost?(role=="jackpot"?9:5):0);
   double a=angle*Math.PI/180;v=new Velocity(direction*speed*(float)Math.Cos(a),speed*(float)Math.Sin(a),v.z);
   if(boost)airBoost=!grounded;
  }else if(role=="hazard"){
   reversed=airBoost&&!grounded;
   if(reversed){float speed=(float)Math.Sqrt(v.x*v.x+v.y*v.y)+20;v=new Velocity(direction*speed*.70710678f,speed*.70710678f,v.z);}
   else v=new Velocity(v.x*.35f,v.y*.35f,v.z*.35f);
   airBoost=false;
  }
  return v;
 }
}
