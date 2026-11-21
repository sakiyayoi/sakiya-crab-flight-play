using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build.Reporting;
public static class InspectActionBuildReport {
 [Serializable]public class Message{public string step,type,text;}
 [Serializable]public class Result{public string status,result,detail;public int summaryErrors,summaryWarnings;public Message[] messages;}
 public static void Run(){var list=new List<Message>();var data=new Result();var method=typeof(BuildReport).GetMethod("GetLatestReport",BindingFlags.Public|BindingFlags.Static);var report=method==null?null:(BuildReport)method.Invoke(null,null);if(report==null){data.status="LATEST_REPORT_UNAVAILABLE";}else{data.status="READ_ONLY_REPORT_INSPECTED";data.result=report.summary.result.ToString();data.summaryErrors=report.summary.totalErrors;data.summaryWarnings=report.summary.totalWarnings;foreach(var step in report.steps)foreach(var message in step.messages)if(message.type==LogType.Error||message.type==LogType.Exception||message.type==LogType.Assert||message.type==LogType.Warning)list.Add(new Message{step=step.name,type=message.type.ToString(),text=message.content});data.detail="Saved actual latest BuildReport, not a new build or substituted PASS";}data.messages=list.ToArray();File.WriteAllText("../evidence/polish1-build-report-inspection.json",JsonUtility.ToJson(data,true));}
}
