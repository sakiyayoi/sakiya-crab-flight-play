using UnityEngine;

public sealed partial class RigidCrabFlight {
 bool overdriveRestaurantRouteEnabled;

 // Opt-in additions only. The r4 restaurant remains the baseline fixture.
 // Call after EnableRestaurant so every prop uses the same contact/break path.
 public void EnableOverdriveRestaurantRoute(){
  if(!restaurant||overdriveRestaurantRouteEnabled)return;
  overdriveRestaurantRouteEnabled=true;
  for(int side=-1;side<=1;side+=2){
   // Independent floor piles occupy gaps between the existing fixtures.
   // Their support does not disappear when another breakable object is hit.
   AddOverdriveGroundPile("r5-restaurant-entry-dishes-"+side,"tableware",new Vector3(side*6f,.4f,0),new Vector3(.9f,.8f,2.4f),new Color(.88f,.86f,.72f),.65f);
   AddOverdriveGroundPile("r5-restaurant-chair-food-"+side,"food",new Vector3(side*8.7f,.275f,0),new Vector3(.65f,.55f,2.4f),new Color(.81f,.54f,.28f),.25f);
   AddOverdriveGroundPile("r5-restaurant-dish-tower-"+side,"tableware",new Vector3(side*11.7f,.55f,0),new Vector3(1.3f,1.1f,2.8f),new Color(.88f,.86f,.72f),.65f);
   // Both new timber props stand on their own legs, directly on the floor.
   AddPanel("r5-restaurant-service-table-"+side,"wood",new Vector3(side*15.7f,1.45f,0),new Vector3(1.1f,2.9f,3f),new Color(.48f,.29f,.12f),.7f);
   AddOverdriveGroundPile("r5-restaurant-service-food-"+side,"food",new Vector3(side*14.8f,.275f,0),new Vector3(.6f,.55f,2.6f),new Color(.81f,.54f,.28f),.25f);
   AddOverdriveGroundPile("r5-restaurant-table-dishes-"+side,"tableware",new Vector3(side*20.15f,.45f,0),new Vector3(.65f,.9f,2.8f),new Color(.88f,.86f,.72f),.65f);
   AddOverdriveGroundPile("r5-restaurant-kitchen-food-"+side,"food",new Vector3(side*22.625f,.25f,0),new Vector3(.5f,.5f,2.5f),new Color(.81f,.54f,.28f),.25f);
   // One last low timber contact leads into the existing breakable exit wall.
   AddPanel("r5-restaurant-exit-table-"+side,"wood",new Vector3(side*23.5f,1.6f,0),new Vector3(1.2f,3.2f,3f),new Color(.40f,.29f,.16f),.65f);
  }
 }

 void AddOverdriveGroundPile(string id,string type,Vector3 position,Vector3 size,Color color,float threshold){
  AddPanel(id,type,position,size,color,threshold);
  // Dishes starts its first plate 0.065625 * height above the panel bottom.
  // Translate only these authored meshes down so geometry and collider rest at
  // floor y=0. No collider, threshold, support dependency or break rule changes.
  var visual=panels[panels.Count-1].visual.transform;
  foreach(Transform piece in visual)piece.localPosition+=Vector3.down*(size.y*.065625f);
 }
}
