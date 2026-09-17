using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using System.IO;
using System.Linq;

public static class SeparateMenuBuild {
 public const string MenuPath="Assets/Scenes/PULSESHIFT_Menu.unity";
 public const string GamePath="Assets/Scenes/PULSESHIFT_Game.unity";
 public static void ConfigureBuildOrder(){
  var other=EditorBuildSettings.scenes.Where(s=>s.path!=MenuPath&&s.path!=GamePath&&s.path!=IntegratedBuild.ScenePath).ToList();
  other.Insert(0,new EditorBuildSettingsScene(GamePath,true));other.Insert(0,new EditorBuildSettingsScene(MenuPath,true));EditorBuildSettings.scenes=other.ToArray();
 }
 [MenuItem("PULSESHIFT/Open MAIN MENU scene")]
 public static void OpenMenu(){Open(MenuPath,true);}
 [MenuItem("PULSESHIFT/Open GAMEPLAY scene")]
 public static void OpenGame(){Open(GamePath,false);}
 static void Open(string path,bool menu){
  if(EditorApplication.isPlaying){EditorUtility.DisplayDialog("PULSESHIFT","Сначала останови Play.","Понятно");return;}
  if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  EditorSceneManager.OpenScene(path);ConfigureBuildOrder();
  if(menu){var lobby=Object.FindFirstObjectByType<PulseLobby>();Selection.activeGameObject=lobby.stage.heroGroup.gameObject;}else Selection.activeGameObject=Object.FindFirstObjectByType<CampaignGame>().editableBoard.gameObject;
  SceneView.lastActiveSceneView?.FrameSelected();
 }
 public static void Verify(){
  var menu=EditorSceneManager.OpenPreviewScene(MenuPath);
  try{
   var roots=menu.GetRootGameObjects();var lobby=roots.SelectMany(r=>r.GetComponentsInChildren<PulseLobby>(true)).Single();
   if(!lobby.standaloneScene||lobby.game||roots.Any(r=>r.GetComponentInChildren<CampaignGame>(true)||r.GetComponentInChildren<CampaignIsland>(true)||r.GetComponentInChildren<CampaignHUD>(true)))throw new System.Exception("Menu scene contains gameplay ownership.");
   if(!lobby.stage||!lobby.stage.view||!lobby.canvas||!lobby.safeRoot||!lobby.stage.GetComponent<AuthoredMenuArt>().artwork)throw new System.Exception("Menu scene references missing.");
   foreach(var b in lobby.GetComponentsInChildren<LobbyButton>(true))if(b.lobby!=lobby||b.action==LobbyAction.None)throw new System.Exception("Unconnected menu button: "+b.name);
   foreach(var root in roots)foreach(var t in root.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new System.Exception("Missing menu script: "+t.name);
  }finally{EditorSceneManager.ClosePreviewScene(menu);}
  var play=EditorSceneManager.OpenPreviewScene(GamePath);
  try{var roots=play.GetRootGameObjects();var game=roots.SelectMany(r=>r.GetComponentsInChildren<CampaignGame>(true)).Single();if(!game.useSeparateMenuScene||game.lobby||roots.Any(r=>r.GetComponentInChildren<PulseLobby>(true)))throw new System.Exception("Gameplay scene still contains an embedded menu.");}
  finally{EditorSceneManager.ClosePreviewScene(play);}
  Debug.Log("SEPARATE_SCENES_VALIDATED");
 }
 [MenuItem("PULSESHIFT/Build saved Menu and Gameplay (Windows)")]
 public static void BuildExisting(){
  if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  Verify();ConfigureBuildOrder();PlayerSettings.bundleVersion="1.3.7";PlayerSettings.Android.bundleVersionCode=11;
  var scenes=System.Environment.GetCommandLineArgs().Contains("-legacyVerifyScene")?new[]{MenuPath,GamePath,IntegratedBuild.ScenePath}:new[]{MenuPath,GamePath};
  var result=BuildPipeline.BuildPlayer(scenes,"../BuildIntegrated/PULSESHIFT.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
  if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Separate-scene desktop build failed");Debug.Log("SEPARATE_MENU_BUILD_OK");
 }
 [MenuItem("PULSESHIFT/Build saved Menu and Gameplay (Android)")]
 public static void Android(){
  if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  Verify();ConfigureBuildOrder();PlayerSettings.bundleVersion="1.3.7";PlayerSettings.Android.bundleVersionCode=11;
  PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
  PlayerSettings.Android.optimizedFramePacing=true;
  var result=BuildPipeline.BuildPlayer(new[]{MenuPath,GamePath},"../BuildAndroid/PULSESHIFT-Mobile.apk",BuildTarget.Android,BuildOptions.None);
  if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Separate-scene Android build failed");Debug.Log("SEPARATE_MENU_ANDROID_OK");
 }
}
