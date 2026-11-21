using UnityEngine;

public static class NeonProto {
 public const string Revision="kani-neon-proto-20261118-stage6";
 public const string SpinStyle="spin-tidy-sparks-sectorart-families-v5";
 public static bool Enabled=true;
 public const float Start=28f,End=135f;
 public const string BackgroundHex="#05061A",BackgroundVioletHex="#2A1650",CrabRedHex="#FF3131";
 public static readonly Color Background=Hex(BackgroundHex),BackgroundViolet=Hex(BackgroundVioletHex);
 public static readonly Color CrabRed=Hex(CrabRedHex),CrabCore=Hex("#280B22"),CrabTip=Hex("#FF6872");
 public static readonly Color[] Palette={Hex("#FF3131"),Hex("#FF3CC7"),Hex("#8B5CFF"),Hex("#25DFFF"),Hex("#45FFB1"),Hex("#FFE45E")};
 public static bool SpinGate(string phase,float spinRps,bool reduced,bool effects,bool aimingOrCharging)=>Enabled&&effects&&!reduced&&(phase=="flight"||aimingOrCharging)&&Mathf.Abs(spinRps)>1;
 static Color Hex(string value){ColorUtility.TryParseHtmlString(value,out var color);return color;}
 public static void Release(Object value){if(!value)return;if(Application.isPlaying)Object.Destroy(value);else Object.DestroyImmediate(value);}
}
