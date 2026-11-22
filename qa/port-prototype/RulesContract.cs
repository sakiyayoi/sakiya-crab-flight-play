using System;
public static class RulesContract {
 static int passed;
 static void Check(bool value,string name){if(!value)throw new Exception(name);passed++;Console.WriteLine("PASS "+name);}
 static bool Near(float a,float b)=>Math.Abs(a-b)<.0001;
 static float Speed(PortRules.Velocity v)=>(float)Math.Sqrt(v.x*v.x+v.y*v.y+v.z*v.z);
 public static int Main(){
  bool air=false,reverse;var input=new PortRules.Velocity(24,7,3);
  var normal=PortRules.Role("hazard",input,1,false,ref air,out reverse);Check(!reverse&&Near(Speed(normal),Speed(input)*.35f),"ordinary hazard slows");
  foreach(var role in new[]{"boost30","boost45","boost60","jackpot"}){
   air=false;var boosted=PortRules.Role(role,input,1,false,ref air,out reverse);
   Check(air&&!reverse&&Speed(boosted)>Speed(input),role+" arms airborne condition");
   var reward=PortRules.Role("hazard",boosted,1,false,ref air,out reverse);
   Check(reverse&&!air&&Speed(reward)>Speed(boosted)+19,"airborne hazard reverses once "+role);
   PortRules.Role("hazard",input,1,false,ref air,out reverse);Check(!reverse,"reversal consumed "+role);
  }
  air=true;PortRules.Role("hazard",input,1,true,ref air,out reverse);Check(!reverse&&!air,"ground invalidates boost before hazard");
  air=false;PortRules.Role("boost45",input,1,true,ref air,out reverse);Check(!air,"same-step ground cannot arm");
  air=false;var redirect=PortRules.Role("redirect30",input,1,false,ref air,out reverse);Check(Near(Speed(redirect),Speed(input))&&!air,"angle-only redirect preserves 3D energy and cannot arm");
  air=true;PortRules.Role("redirect30",input,1,false,ref air,out reverse);Check(air,"redirect preserves already valid airborne state");
  air=false;var left=PortRules.Role("boost60",input,-1,false,ref air,out reverse);Check(left.x<0&&left.y>0,"role direction preserved");
  foreach(var role in new[]{"landmark","resource","preview",""}){air=false;var v=PortRules.Role(role,input,1,false,ref air,out reverse);Check(Near(Speed(v),Speed(input)),role+" no extra velocity effect");}
  Check(Near(PortRules.Zoom(0,0,.5f),12),"portrait floor");Check(Near(PortRules.Zoom(0,0,1.7f),10),"landscape floor");
  Check(Near(PortRules.Zoom(50,20,1.7f),19),"speed/height formula");Check(Near(PortRules.Zoom(300,400,1),30),"zoom maximum");
  Check(Near(PortRules.Zoom(-50,20,1.7f),19),"direction independent zoom");
  Check(Near(PortRules.Lead(80,.5f,20),4.5f),"portrait half-width limit");
  Check(Near(PortRules.Lead(0,2,20),3),"minimum lookahead");Check(Near(PortRules.Lead(20,2,20),4.4f),"lookahead speed");
  Console.WriteLine("TOTAL "+passed+" passed / 0 failed");return 0;
 }
}
