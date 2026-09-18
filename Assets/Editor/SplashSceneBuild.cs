using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class SplashSceneBuild {
 public const string SplashPath="Assets/Scenes/PULSESHIFT_Splash.unity";
 static SplashSceneBuild(){EditorApplication.delayCall+=UseSplashForPlayMode;}
 static void UseSplashForPlayMode(){
  var splash=AssetDatabase.LoadAssetAtPath<SceneAsset>(SplashPath);
  if(splash&&EditorSceneManager.playModeStartScene!=splash)EditorSceneManager.playModeStartScene=splash;
 }
 [MenuItem("PULSESHIFT/Open SPLASH screen")]
 public static void Open(){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EnsureScene();EditorSceneManager.OpenScene(SplashPath);ConfigureBuildOrder();}
 [MenuItem("PULSESHIFT/Rebuild SPLASH screen")]
 public static void Rebuild(){EnsureScene();ConfigureBuildOrder();Debug.Log("PULSE_SPLASH_READY");}
 public static void EnsureScene(){
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  new GameObject("PULSESHIFT • Splash Screen").AddComponent<PulseSplashScreen>();
  EditorSceneManager.SaveScene(scene,SplashPath);
 }
 public static void ConfigureBuildOrder(){
  var wanted=new[]{SplashPath,SeparateMenuBuild.MenuPath,SeparateMenuBuild.GamePath};
  var remaining=EditorBuildSettings.scenes.Where(s=>!wanted.Contains(s.path)).ToList();
  EditorBuildSettings.scenes=wanted.Select(p=>new EditorBuildSettingsScene(p,true)).Concat(remaining).ToArray();
  UseSplashForPlayMode();
 }
 public static void BuildBatch(){EnsureScene();ConfigureBuildOrder();SeparateMenuBuild.BuildExisting();}
}
