"""Exact, reviewed blastfix deltas for historical byte-identity contracts.
Do not update the historical baseline: reject any change beyond these replacements.
"""
QUEUE_OLD='  objectBreakPending.Add(p);if(blast)objectBlastPending.Add(p);'
QUEUE_NEW='''  if(!UseObjectRedesign||p==null||p.broken||p.objectSuppressed)return;
  // A later blast may promote an already queued band hit, but never requeue it.
  if(blast)objectBlastPending.Add(p);if(!objectBreakPending.Add(p))return;
  // Object actions own this break; debris callbacks must not replace its source.
  primaryPending.Add(p);secondarySources.Remove(p);'''
CITY_CHANGES=[
 (' void RecycleCityFragment(){',' void RecycleCityFragment(List<Rigidbody> scattering=null){'),
 ('  var record=cityDebris.FirstOrDefault();var old=record!=null?record.obj:fragments.FirstOrDefault();if(!old)return;',
 '''  // Do not retire a body still awaiting ScatterShardsR6 in this spawn batch.
  // null keeps the legacy FIFO selection exactly as before.
  var record=cityDebris.FirstOrDefault(d=>scattering==null||!scattering.Contains(d.body));
  var old=record!=null?record.obj:fragments.FirstOrDefault(o=>scattering==null||!scattering.Any(rb=>rb&&rb.gameObject==o));if(!old)return;'''),
 ('   RecycleCityFragment();if(fragments.Count>=48||CityFragments>=20){Release(obj);continue;}',
  '   RecycleCityFragment(UseObjectRedesign?spawned:null);if(fragments.Count>=48||CityFragments>=20){Release(obj);continue;}')]
def restore(s,changes):
 for old,new in changes:
  assert s.count(new)==1,'blastfix boundary differs: '+new
  s=s.replace(new,old)
 return s
def restore_city2(s):return restore(s,CITY_CHANGES)
def restore_runtime(s):return restore(s,[(QUEUE_OLD,QUEUE_NEW)])
