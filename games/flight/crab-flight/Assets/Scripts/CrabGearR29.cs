using UnityEngine;
// Gear uses the existing morphology/mass/contact rules, not an invisible speed reward.
// The visible braces are included by the existing HeroColliderFit supporting hull.
public static class CrabGearR29 {
 static void Cover(string name,Transform parent,PrimitiveType type,Vector3 position,Vector3 scale,Color color){
  var o=CrabModel.Shape(name,parent,type,position,scale,color);var queued=o.GetComponent<Collider>();if(queued){queued.enabled=false;PrimitiveColliderTraceR18.Mutated();}
 }
 public static void Apply(CrabModel crab,int gear){
  if(gear==0)return;
  foreach(var part in crab.CollisionParts){
   if((gear==1&&part.Kind!=2)||(gear==2&&part.Kind!=1)||(gear==3&&part.Kind!=3))continue;
   var b=part.LocalBounds;var color=gear==1?new Color(.96f,.62f,.12f):gear==2?new Color(.18f,.23f,.26f):new Color(.86f,.88f,.76f);
   if(gear==2){
    Cover("R29 iron-pan shell",part.Part,PrimitiveType.Cylinder,b.center+Vector3.up*b.extents.y*.67f,new Vector3(b.size.x*.77f,b.size.y*.07f,b.size.z*.77f),color);
    Cover("R29 pan handle",part.Part,PrimitiveType.Cube,b.center+new Vector3(b.extents.x*.72f,b.extents.y*.67f,0),new Vector3(b.size.x*.23f,b.size.y*.10f,b.size.z*.12f),new Color(.40f,.25f,.14f));
   }else{
    Cover(gear==1?"R29 demolition gauntlet":"R29 crab grip",part.Part,PrimitiveType.Cube,b.center+Vector3.up*b.extents.y*.60f,new Vector3(b.size.x*.66f,b.size.y*.22f,b.size.z*.48f),color);
    for(int i=-1;i<=1;i++)Cover("R29 grip rib",part.Part,PrimitiveType.Cube,b.center+new Vector3(i*b.size.x*.17f,b.extents.y*.77f,0),new Vector3(b.size.x*.07f,b.size.y*.11f,b.size.z*.53f),new Color(.15f,.17f,.16f));
   }
  }
 }
}
