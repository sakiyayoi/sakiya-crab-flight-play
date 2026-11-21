using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
public static class ActionReview{
 [Serializable]public class Case{public string name;public ActionState state;}
 [Serializable]public class Result{public string status="PASS_NATIVE_COLLISION_AND_SWAP";public Case[] cases;public int loadouts;public string scope="Real Unity Capsule overlap and SphereCast; browser rendering/touch/audio listening separate";}
 static void Advance(CrabActionGame g,float seconds){for(int i=0;i<Mathf.CeilToInt(seconds*90);i++)g.Tick(1f/90);}
 public static void Run(){Directory.CreateDirectory("../evidence");var cases=new List<Case>();var g=BuildAction.Prepare();g.Configure("{\"left\":\"creature\",\"right\":\"cannon\"}");if(g.State().left!="creature"||g.State().phase!="customize")throw new Exception("Saved loadout must restore before Start");cases.Add(new Case{name="saved-loadout-before-Start",state=g.State()});g.aiEnabled=false;
  g.Configure("{\"left\":\"sword\",\"right\":\"cannon\"}");g.Begin("");g.ReviewPlace(Vector3.zero,new Vector3(0,0,7));g.AttackLeft("");Advance(g,1);if(g.State().enemyHp!=100||g.State().misses!=1)throw new Exception("Far sword should miss");cases.Add(new Case{name="sword-real-air-swing",state=g.State()});
  g.Begin("");g.ReviewPlace(Vector3.zero,new Vector3(0,0,2.15f));g.AttackLeft("");Advance(g,1);if(g.State().enemyHp>=100||g.State().hits!=1)throw new Exception("Near sword should hit once, hp="+g.State().enemyHp);cases.Add(new Case{name="sword-visible-sweep-hit-once",state=g.State()});
  g.Begin("");g.ReviewPlace(Vector3.zero,new Vector3(0,0,6));g.AttackRight("");Advance(g,1);if(g.State().enemyHp>=100)throw new Exception("Cannon should hit along visible path");cases.Add(new Case{name="cannon-projectile-hit",state=g.State()});
  g.Begin("");g.ReviewPlace(Vector3.zero,new Vector3(5,0,6));g.AttackRight("");Advance(g,1.8f);if(g.State().enemyHp!=100||g.State().misses!=1)throw new Exception("Off-axis cannon should miss");cases.Add(new Case{name="cannon-off-axis-miss",state=g.State()});
  g.Customize("");g.Configure("{\"left\":\"creature\",\"right\":\"cannon\"}");g.Begin("");g.ReviewPlace(Vector3.zero,new Vector3(0,0,2.7f));g.AttackLeft("");Advance(g,1);if(g.State().enemyHp>=100)throw new Exception("Creature extension should contact");cases.Add(new Case{name="creature-extending-contact",state=g.State()});
  g.Begin("");g.ReviewPlace(Vector3.zero,new Vector3(0,0,7));Advance(g,4);if(g.State().hp!=100||g.State().enemyHp!=100)throw new Exception("HP must not drain with time");g.Move("1,0");Advance(g,.5f);g.Move("0,0");if(g.State().x<.2f)throw new Exception("Movement input failed");cases.Add(new Case{name="no-time-damage-and-move",state=g.State()});
  g.Customize("");g.Configure("{\"left\":\"sword\",\"right\":\"cannon\"}");g.Begin("");for(int i=0;i<5&&g.State().phase=="battle";i++){g.ReviewPlace(Vector3.zero,new Vector3(0,0,2.15f));g.AttackLeft("");Advance(g,1);}if(g.State().phase!="win")throw new Exception("Actual hits should win");cases.Add(new Case{name="collision-win",state=g.State()});g.Begin("");if(g.State().hp!=100||g.State().enemyHp!=100||g.State().hits!=0)throw new Exception("Rematch reset failed");cases.Add(new Case{name="rematch-reset",state=g.State()});
  g.Begin("");for(int i=0;i<5&&g.State().phase=="battle";i++){g.ReviewPlace(Vector3.zero,new Vector3(0,0,6));g.AttackRight("");Advance(g,1.4f);}if(g.State().phase!="win")throw new Exception("Lethal projectile should finish safely");cases.Add(new Case{name="projectile-lethal-win",state=g.State()});
  g.Begin("");g.aiEnabled=true;Advance(g,6);cases.Add(new Case{name="stationary-player-six-second-pressure",state=g.State()});g.aiEnabled=false;
  int n=0;foreach(string left in new[]{"sword","cannon","creature"})foreach(string right in new[]{"sword","cannon","creature"}){g.Customize("");g.Configure(JsonUtility.ToJson(new ActionConfig{left=left,right=right}));g.Begin("");g.ReviewPlace(Vector3.zero,new Vector3(0,0,7));g.AttackLeft("");g.AttackRight("");Advance(g,.8f);if(g.State().left!=left||g.State().right!=right)throw new Exception("Attachment mismatch");n++;}
  File.WriteAllText("../evidence/native-review.json",JsonUtility.ToJson(new Result{cases=cases.ToArray(),loadouts=n},true));
 }
}
