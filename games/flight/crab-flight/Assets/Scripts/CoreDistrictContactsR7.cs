using UnityEngine;
public sealed partial class RigidCrabFlight {
 #if UNITY_EDITOR
 public static bool ReviewDisableCorePanels;
#endif
 public bool UseCoreDestruction;bool coreDistrictReady;
 public void EnableCoreDistrictContacts(){
#if UNITY_EDITOR
 if(ReviewDisableCorePanels)return;
#endif
 if(!UseCoreDestruction||coreDistrictReady)return;coreDistrictReady=true;
  for(int side=-1;side<=1;side+=2){
   AddPanel("core-mall-glass-"+side,"glass",new Vector3(side*112,7.2f,0),new Vector3(.14f,4.8f,3.2f),new Color(.58f,.84f,.88f),.55f);
   AddPanel("core-office-glass-"+side,"glass",new Vector3(side*205,3.5f,0),new Vector3(.16f,5.0f,3.4f),new Color(.68f,.82f,.88f),.65f);
  }
 }
}
