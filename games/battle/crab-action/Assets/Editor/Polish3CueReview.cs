using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

// Editor-only regression fixture. It changes positions only inside this test scene;
// input-response performance is measured separately through ordinary browser controls.
public static class Polish3CueReview {
 const BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static object Field(object target,string name)=>target.GetType().GetField(name,Fields).GetValue(target);
 static object Weapon(CrabActionGame game,int side)=>((Array)Field(Field(game,"player"),"weapons")).GetValue(side);
 static Renderer Muzzle(object weapon)=>(Renderer)Field(weapon,"chargeRenderer");
 static void Require(bool condition,string message){if(!condition)throw new Exception("Polish3 cue regression: "+message);}
 static void Step(CrabActionGame game)=>game.Tick(1f/90f);
 static Color PropertyColour(Renderer renderer){var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);Require(!block.isEmpty,"active cue must reach the existing muzzle renderer");return block.GetColor("_Color");}
 static void RequireCleared(Renderer renderer,string stage){var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);Require(block.isEmpty,stage+" must clear the muzzle property block");}

 public static void Run(){
  var game=BuildAction.Prepare();game.Initialize();
  game.Configure("{\"left\":\"cannon\",\"right\":\"cannon\"}");
  game.Begin("");game.ReviewPlace(Vector3.zero,new Vector3(0,0,7));
  var left=Weapon(game,0);var leftRenderer=Muzzle(left);
  RequireCleared(leftRenderer,"initial Begin");
  game.AttackLeft("");
  Step(game);
  var first=game.ReadProbe();
  Require(first.lcue>.02f&&first.lcue<1f,"first rendered step should expose a small cannon cue");
  Require(first.lp<.0001f&&first.lf==0&&first.shots==0,"cue must precede unchanged recoil and projectile");
  var normal=PropertyColour(leftRenderer);
  Require(normal.r>.45f&&normal.g>.45f,"first-step muzzle colour needs clear contrast without replacing its mesh");
  for(int i=0;i<40&&game.ReadProbe().lf==0;i++){
   Step(game);var sample=game.ReadProbe();
   if(sample.lc<.18f)Require(sample.lf==0&&sample.shots==0,"projectile fired before original .18 weapon clock");
  }
  var fired=game.ReadProbe();
  Require(fired.lf==1&&fired.lc>=.18f&&fired.shots==1,"one projectile should fire at original .18 weapon clock");
  var shots=(IList)Field(game,"shots");
  var direction=(Vector3)Field(shots[0],"direction");
  var weaponRoot=(Transform)Field(left,"root");
  Require(Vector3.Angle(direction,weaponRoot.forward)<.001f,"projectile must retain the visible weapon forward");
  game.Begin("");
  Require(game.ReadProbe().lcue==0&&game.ReadProbe().rcue==0,"retry must clear cue values");
  RequireCleared(leftRenderer,"retry");
  game.AttackRight("");Step(game);
  Require(game.ReadProbe().rcue>.02f&&game.ReadProbe().lcue==0,"right cannon cue must be independent");
  game.Customize("");
  Require(game.ReadProbe().rcue==0,"Customize must clear cue values");
  RequireCleared(Muzzle(Weapon(game,1)),"Customize");
  game.Comfort("1");game.Begin("");game.AttackLeft("");Step(game);
  var calm=game.ReadProbe();
  Require(calm.lcue>.02f&&calm.lcue<first.lcue,"reduced motion should keep a quieter continuous cue");
  game.Customize("");game.Configure("{\"left\":\"sword\",\"right\":\"creature\"}");game.Begin("");
  game.AttackLeft("");game.AttackRight("");Step(game);
  Require(game.ReadProbe().lcue==0&&game.ReadProbe().rcue==0,"non-cannon weapons must not report charge cues");
 }
}
