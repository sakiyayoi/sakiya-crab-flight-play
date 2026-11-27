using System;using UnityEngine;
[Serializable]public class FeelEventR27{public int particles,stopFrames,maxMashimashiStopFrames,flashPhases,flashPriority,confetti;public float squash,shakeM,shakeMs,kickM,residueMs,slowScale,slowMs,visualKnockbackM,textPeak,textRest,speedThreshold,ghostIntervalMs,lineIntervalMs;}
[Serializable]public class FeelEventsR27{public FeelEventR27 charge,ready,launch,tankGlass,destruction,chain2,chain4,chain7,landing,perfect,good,miss,newRecord,flightFast;}
[Serializable]public class FeelMultiplierR27{public float shakeMultiplier,particleMultiplier,squashMultiplier,kickMultiplier,hitStopMultiplier;}
[Serializable]public class FeelPresetsR27{public FeelMultiplierR27 mashimashi,futsuu,off;}
[Serializable]public class FeelCameraR27{public float fovBase,fovNormal,fovMashimashi,zoomMs;}
[Serializable]public class FeelUiR27{public float resultCountMs,popInMs,popHoldMs,popOutMs,popRisePx,chainSettleMs,fontPx,chainFontPx;}
[Serializable]public class FeelFlashR27{public string white,orange,yellow;public float phaseMs;}
[Serializable]public class FeelDrawingR27{public float particleLifeMs,ringLifeMs,ringRadiusM,ghostLifeMs,particleSpeedM,particleSizeM,flashAlpha,squashMs,residueSizeM,popupKnockbackM,knockbackMs;}
[Serializable]public class FeelBudgetR27{public int ghostCopies,ringSegments;public float mobileTargetFps,mobileParticleScale;}
[Serializable]public class FeelConfig{public string objectMode="legacy";public bool UsesObjectRedesign=>string.Equals(objectMode,"redesign",StringComparison.Ordinal);public string burstMode="marks";public bool UsesEffekseer=>string.Equals(burstMode,"effekseer",StringComparison.Ordinal);public string version,defaultPreset;public float targetFpsMobile,mobileParticleMinRatio;public FeelEventsR27 events;public FeelPresetsR27 presets;public FeelCameraR27 camera;public FeelUiR27 ui;public FeelFlashR27 flash;public FeelDrawingR27 drawing;public FeelBudgetR27 budget;
 public FeelMultiplierR27 Multiplier(string p)=>p=="off"?presets.off:p=="futsuu"?presets.futsuu:presets.mashimashi;
 public FeelEventR27 Event(string e,int chain)=>e=="charge_stroke"?events.charge:e=="charge_ready"?events.ready:e=="launch"?events.launch:e=="glass_break"?events.tankGlass:e=="break"?events.destruction:e=="chain"?(chain>=7?events.chain7:chain>=4?events.chain4:events.chain2):e=="ground_hit"?events.landing:e=="push_perfect"?events.perfect:e=="push_good"?events.good:e=="new_record"?events.newRecord:events.miss;
}
