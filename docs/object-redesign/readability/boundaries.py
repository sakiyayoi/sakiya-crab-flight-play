"""Exact inverse of the requested presentation delta for historical invariants.
New render code is independently covered by readability/contracts.py. No physics
file, Tick, State, Rules, placement or Collider is excluded from equality checks.
"""
def before_readability(path,s):
 if path.endswith('ObjectHitStopRules.cs'):
  return s.replace('Death=.700','Death=.200')
 if path.endswith('CrabFlightGame.cs'):
  return s.replace(' public ObjectBadgeState[] objectBadges;public ObjectCaptureState objectCapture;\n','').replace('neonCrab?.SyncTankVisibility(phase);LateObjectReadability();','neonCrab?.SyncTankVisibility(phase);')
 if path.endswith('ObjectPresentation.cs'):
  s=s.replace('bool ObjectDeathPresentationHeld=>world!=null&&world.UseObjectRedesign&&simulation!=null&&simulation.ObjectRun.Dead&&objectCapture.Active;','bool ObjectDeathPresentationHeld=>ObjectHitStopEnabled&&simulation!=null&&simulation.ObjectRun.Dead&&objectHitStop&&objectHitStop.Holding;')
  s=s.replace('  AddObjectReadabilityReport(report);\n','').replace('void ClearObjectBursts(){ClearObjectCapture();','void ClearObjectBursts(){')
  return s.replace('   if(hit.kind==ObjectRules.Kind.Death){BeginObjectCapture(hit.position);if(ReducedMotion||feelSettingsR27.reducedMotion)continue;}\n','')
 return s
