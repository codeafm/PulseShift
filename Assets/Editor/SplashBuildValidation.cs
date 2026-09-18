using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

// Validate the actual scenes processed by Unity, including profile overrides.
public sealed class SplashBuildValidation : IProcessSceneWithReport {
 public int callbackOrder => -1000;
 public void OnProcessScene(Scene scene, BuildReport report) {
  if(report == null) return;
  if(scene.buildIndex == 0 && scene.path != SplashSceneBuild.SplashPath)
   throw new BuildFailedException("PULSESHIFT: first build scene must be Assets/Scenes/PULSESHIFT_Splash.unity. Use PULSESHIFT > Build saved Menu and Gameplay (Android).");
  if(scene.path == SplashSceneBuild.SplashPath && !scene.GetRootGameObjects().Any(r => r.GetComponentInChildren<PulseSplashScreen>(true)))
   throw new BuildFailedException("PULSESHIFT: splash controller is missing from startup scene.");
 }
}
