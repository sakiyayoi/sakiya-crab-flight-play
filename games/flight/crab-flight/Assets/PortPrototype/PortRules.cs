using System;
// Orthographic half-height (world metres), not a perspective FOV in degrees.
// Redesign framing only; legacy PortRules and all following rates stay intact.
public static class PortCameraSettings {
 public const float BaseMultiplier=1.3f, MaximumMultiplier=1.7f;
 public const float LegacyMaximum=30f, Maximum=LegacyMaximum*MaximumMultiplier;
 public const float ZoomFollowRate=2f, PositionFollowRate=3f, VerticalFollowRate=1.2f;
 public const float SubjectMargin=3.5f, NextObjectMargin=2f;
 public static float Minimum(float aspect)=>(aspect<1?12f:10f)*BaseMultiplier;
 public static float Zoom(float speed,float height,float aspect){
  float oldMinimum=aspect<1?12f:10f;
  float t=(PortRules.Zoom(speed,height,aspect)-oldMinimum)/(LegacyMaximum-oldMinimum);
  return Minimum(aspect)+(Maximum-Minimum(aspect))*t;
 }
 public static float FitNext(float desired,float distance,float verticalSpan,float aspect){
  // Keep both silhouettes inside the view (45% maximum forward lead). Never
  // shrink beyond the new baseline or pull back past the readable hard limit.
  float horizontal=Math.Max((distance+NextObjectMargin)/(aspect*1.45f),SubjectMargin/(aspect*.55f));
  float vertical=(Math.Abs(verticalSpan)+SubjectMargin+NextObjectMargin)*.5f;
  return PortRules.Clamp(Math.Max(desired,Math.Max(horizontal,vertical)),Minimum(aspect),Maximum);
 }
}
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
