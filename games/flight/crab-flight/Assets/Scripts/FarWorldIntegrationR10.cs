using UnityEngine;
public sealed partial class CrabFlightGame {
 public bool useFarWorldR10=true;
 FarWorldLifeR10 farLifeR10;
 void BuildFarWorldR10(){if(!useFarWorldR10||cityLayout!=2||!cityVisuals)return;
  // Exact world anchor: high supports line up with the physical props, never parallax-shifted.
  var anchor=new GameObject("R10 unshifted world anchor");anchor.transform.SetParent(cityVisuals.transform,false);
  FarWorldArtR10.BuildBackground(anchor.transform,groundShader);if(useRouteBlockoutR11)WorldRouteR11.BuildBackdrop(anchor.transform,groundShader,useProgressionR12);
  farLifeR10=FarWorldLifeR10.Build(anchor.transform,groundShader);
 }
 void TickFarWorldR10(float gameplayDt){if(farLifeR10&&crab!=null)farLifeR10.Tick(gameplayDt,crab.Root.position,ReducedMotion);}
}
