using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
public static class PolishDiagnosticReview {
 [Serializable]public class Entry{public string type,message,stack;}
 [Serializable]public class Result{public string status;public Entry[] diagnostics;public string method="PolishActionReview.Run3fixedtrials+20conditions,ActionReview.Run";}
 public static void Run(){var entries=new List<Entry>();Application.LogCallback receive=(message,stack,type)=>{if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)entries.Add(new Entry{type=type.ToString(),message=message,stack=stack});};Application.logMessageReceived+=receive;try{PolishActionReview.Run();ActionReview.Run();}finally{Application.logMessageReceived-=receive;File.WriteAllText("../evidence/polish1-native-diagnostics.json",JsonUtility.ToJson(new Result{status=entries.Count==0?"PASS_NO_NATIVE_DIAGNOSTICS":"DIAGNOSTICS_REMAIN",diagnostics=entries.ToArray()},true));}}
}
