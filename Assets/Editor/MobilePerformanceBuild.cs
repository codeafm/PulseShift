using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// Also covers Unity's normal Build / Build And Run, not just our custom menu.
public sealed class MobilePerformanceBuild:IPreprocessBuildWithReport {
 public int callbackOrder=>0;
 public void OnPreprocessBuild(BuildReport report){
  if(report.summary.platform==BuildTarget.Android)PlayerSettings.Android.optimizedFramePacing=true;
 }
}
