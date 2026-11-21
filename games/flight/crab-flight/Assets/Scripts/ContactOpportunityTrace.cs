#if UNITY_EDITOR
using System;using System.Collections.Generic;using System.Linq;using UnityEngine;
public sealed partial class RigidCrabFlight {
 [Serializable]public sealed class BreakResponseTrace {public string id;public float time;public Vector3 position,incoming,solver,beforeRedirect,recovered,afterBurst;public bool ceiling,normalAbsorptionEnabled,normalAbsorptionApplied,ceilingRedirectApplied;}
 [Serializable]public sealed class ChainContactTrace {public string root,target;public int depth,mask,notifications,used;public float firstTime,lastTime,maxImpulse,distance,targetMass,threshold;public Vector3 sourceOrigin,targetPosition,sourcePosition,preVelocity;}
 [Serializable]public sealed class ContactTraceBundle {public BreakResponseTrace[] responses;public ChainContactTrace[] chains;public DebrisSample[] samples;public int overflow;}
 [Serializable]public sealed class DebrisSample{public string root,name;public int depth;public float time;public Vector3 p,v;}
 readonly List<DebrisSample> reviewSamples=new List<DebrisSample>();
 public void ReviewSampleDebris(){if(reviewSamples.Count>4000)return;foreach(var d in cityDebris)if(d.body&&d.depth==0&&(d.root=="street3-+1-11-market_crate"||d.root=="street3-+1-01-street_awning"))reviewSamples.Add(new DebrisSample{root=d.root,name=d.obj.name,depth=d.depth,time=Time,p=d.body.position,v=d.body.linearVelocity});}
 public readonly List<BreakResponseTrace> ReviewBreakResponses=new List<BreakResponseTrace>();
 readonly Dictionary<string,ChainContactTrace> reviewChainRows=new Dictionary<string,ChainContactTrace>();int reviewChainOverflow;
 void ReviewResetContactTrace(){reviewSamples.Clear();ReviewBreakResponses.Clear();reviewChainRows.Clear();reviewChainOverflow=0;}
 void ReviewObserveChainContact(BreakPanel p,CityDebris d,ContactPair pair){
  int used;chainUsed.TryGetValue(d.root,out used);float distance=Vector3.Distance(p.position,d.origin);
  int mask=(p.broken?1:0)|(d.depth>0?2:0)|(p.id==d.root?4:0)|(p.cityProposal!=null&&p.cityProposal.massKgSuggestion>100?8:0)|(distance>10?16:0)|(primaryPending.Contains(p)?32:0)|(used>=2?64:0)|(!p.city2?128:0);
  string key=d.root+"|"+p.id+"|"+d.depth+"|"+mask;
  ChainContactTrace row;if(!reviewChainRows.TryGetValue(key,out row)){
   if(reviewChainRows.Count>=2048){reviewChainOverflow++;return;}
   row=new ChainContactTrace{root=d.root,target=p.id,depth=d.depth,mask=mask,firstTime=Time,sourceOrigin=d.origin,targetPosition=p.position,distance=distance,targetMass=p.cityProposal!=null?p.cityProposal.massKgSuggestion:0,threshold=p.threshold*SecondaryThresholdFraction,preVelocity=d.preVelocity,sourcePosition=d.obj.transform.position,used=used};reviewChainRows.Add(key,row);
  }
  row.lastTime=Time;row.notifications++;for(int k=0;k<pair.contactCount;k++)row.maxImpulse=Mathf.Max(row.maxImpulse,pair.GetContactPoint(k).impulse.magnitude);
 }
 public string ReviewContactTraceJson(){return JsonUtility.ToJson(new ContactTraceBundle{responses=ReviewBreakResponses.ToArray(),samples=reviewSamples.ToArray(),chains=reviewChainRows.Values.OrderBy(x=>x.firstTime).ToArray(),overflow=reviewChainOverflow});}
}
#endif
