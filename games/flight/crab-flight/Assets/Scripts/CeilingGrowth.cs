using UnityEngine;
public sealed partial class RigidCrabFlight {
 public bool UseCeilingGrowth=true;public bool CeilingBreakthrough{get;private set;}public bool CeilingExited{get;private set;}
 public bool CeilingUnlocked=>UseCeilingGrowth&&settings!=null&&settings.upgradeLevel>=2;
 void ResetCeilingRun(){CeilingBreakthrough=CeilingExited=false;}
 void ObserveCeilingExit(){if(CeilingBreakthrough&&Lowest()>13.8f)CeilingExited=true;}
}
public sealed partial class CrabFlightGame {public bool useCeilingGrowth=true,useAtriumRoute=true;}
