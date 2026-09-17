using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;

public static class IntegratedBuild {
 public const string ScenePath="Assets/Scenes/PULSESHIFT_Integrated.unity";
 [MenuItem("PULSESHIFT/Open integrated game")]
 public static void Open(){
  if(File.Exists(SeparateMenuBuild.GamePath)){SeparateMenuBuild.OpenGame();return;}
  if(EditorApplication.isPlaying){EditorUtility.DisplayDialog("PULSESHIFT","Сначала остановите Play верхней кнопкой в Unity, затем откройте новую сцену.","Понятно");return;}
  AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
  if(!File.Exists(ScenePath)){EditorUtility.DisplayDialog("PULSESHIFT","Сцена ещё не перенесена. Дождитесь завершения подготовки файлов.","Понятно");return;}
  if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()){
   PlayerSettings.colorSpace=ColorSpace.Linear;EditorSceneManager.OpenScene(ScenePath);
   var otherScenes=EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath).Select(s=>new EditorBuildSettingsScene(s.path,false)).ToList();otherScenes.Insert(0,new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=otherScenes.ToArray();
   var game=Object.FindFirstObjectByType<CampaignGame>();Selection.activeGameObject=game.editableBoard.gameObject;SceneView.lastActiveSceneView?.FrameSelected();
  }
 }
 public static void Setup(){
  // The existing project stripped all built-in defaults. Explicitly retain native Canvas shaders.
  var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
  var included=graphics.FindProperty("m_AlwaysIncludedShaders");
  foreach(var name in new[]{"UI/Default","UI/Default Font","Hidden/Internal-DepthNormalsTexture"}){var shader=Shader.Find(name);if(!shader)continue;bool found=false;for(int i=0;i<included.arraySize;i++)if(included.GetArrayElementAtIndex(i).objectReferenceValue==shader)found=true;if(!found){int i=included.arraySize;included.InsertArrayElementAtIndex(i);included.GetArrayElementAtIndex(i).objectReferenceValue=shader;}}
  graphics.ApplyModifiedPropertiesWithoutUndo();
  CampaignBuild.Verify();ReferenceModelBaker.BakeAll();
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var game=new GameObject("PULSESHIFT • integrated Blender campaign").AddComponent<CampaignGame>();game.BakePreview();CampaignBuild.PersistArt();
  Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
  PlayerSettings.productName="PULSESHIFT";PlayerSettings.companyName="Pulse Studio";PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.runInBackground=true;
  PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.defaultIsNativeResolution=false;PlayerSettings.resizableWindow=true;
  QualitySettings.antiAliasing=4;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.shadowDistance=60;
  AssetDatabase.SaveAssets();Debug.Log("INTEGRATED_SCENE_READY");
 }
 public static void Build(){Setup();var result=BuildPipeline.BuildPlayer(new[]{ScenePath},"../BuildIntegrated/PULSESHIFT.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Integrated build failed: "+result.summary.result);Debug.Log("INTEGRATED_BUILD_OK");}
 // Build saved artwork without regenerating the user's scene, models or camera.
 public static void BuildSavedScene(){
  VerifyDeliveredAssets();
  var result=BuildPipeline.BuildPlayer(new[]{ScenePath},"../BuildIntegrated/PULSESHIFT.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
  if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Saved-scene build failed: "+result.summary.result);
  Debug.Log("INTEGRATED_SAVED_SCENE_BUILD_OK");
 }
 public static void BuildVariety(){
  CampaignBuild.Verify();var scene=EditorSceneManager.OpenScene(ScenePath);
  var game=Object.FindFirstObjectByType<CampaignGame>();game.previewLevel=1;game.BakePreview();CampaignBuild.PersistArt();
  EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();BuildSavedScene();
 }
 public static void BuildLobby(){
  var scene=EditorSceneManager.OpenScene(ScenePath);var game=Object.FindFirstObjectByType<CampaignGame>();game.startInLobby=true;
  CampaignHUD.Ensure(game);var previous=Object.FindFirstObjectByType<PulseLobby>();if(previous)Object.DestroyImmediate(previous.gameObject);game.lobby=PulseLobby.Ensure(game);
  game.lobby.stage.SetVisible(true);game.lobby.canvas.enabled=true;Canvas.ForceUpdateCanvases();game.lobby.Layout();game.lobby.stage.Frame();game.lobby.canvas.enabled=false;game.lobby.stage.SetVisible(false);CampaignBuild.PersistArt();
  EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();BuildSavedScene();
 }
 public static void BuildCleanLobby(){
  var scene=EditorSceneManager.OpenScene(ScenePath);var game=Object.FindFirstObjectByType<CampaignGame>();
  CampaignHUD.Ensure(game);game.lobby=PulseLobby.Ensure(game);game.lobby.UpgradeLayout();
  game.lobby.stage.SetVisible(true);game.lobby.canvas.gameObject.SetActive(true);game.lobby.canvas.enabled=true;
  Canvas.ForceUpdateCanvases();game.lobby.Layout();game.lobby.stage.Frame();
  game.lobby.Hide();game.editableBoard.gameObject.SetActive(true);
  EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();BuildSavedScene();
 }
 [MenuItem("PULSESHIFT/Preview main menu in Scene")]
 public static void PreviewLobby(){if(EditorApplication.isPlaying)return;if(File.Exists(SeparateMenuBuild.MenuPath)){SeparateMenuBuild.OpenMenu();return;}var game=Object.FindFirstObjectByType<CampaignGame>();if(!game)return;CampaignHUD.Ensure(game);game.lobby=PulseLobby.Ensure(game);game.lobby.Open();Canvas.ForceUpdateCanvases();game.lobby.Layout();game.lobby.stage.Frame();Selection.activeGameObject=game.lobby.canvas.gameObject;EditorSceneManager.MarkSceneDirty(game.gameObject.scene);}
 [MenuItem("PULSESHIFT/Preview gameplay in Scene")]
 public static void PreviewGameplay(){if(EditorApplication.isPlaying)return;if(File.Exists(SeparateMenuBuild.GamePath)){SeparateMenuBuild.OpenGame();return;}var game=Object.FindFirstObjectByType<CampaignGame>();if(!game)return;if(game.lobby)game.lobby.Hide();else game.SetLobbyVisible(false);if(game.editableBoard)game.editableBoard.gameObject.SetActive(true);Selection.activeGameObject=game.editableBoard?game.editableBoard.gameObject:game.gameObject;EditorSceneManager.MarkSceneDirty(game.gameObject.scene);}
 [MenuItem("PULSESHIFT/Verify integrated scene")]
 public static void VerifyDeliveredAssets(){
  CampaignBuild.Verify();var scene=EditorSceneManager.OpenPreviewScene(ScenePath);
  try{
  foreach(var root in scene.GetRootGameObjects())if(root.GetComponentsInChildren<MonoBehaviour>(true).Any(c=>c==null))throw new System.Exception("Missing script in delivered scene: "+root.name);
  var game=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CampaignGame>(true)).FirstOrDefault();var hud=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CampaignHUD>(true)).FirstOrDefault();
  if(!game||!game.editableBoard||!game.gameCamera||!hud||!hud.game||!hud.safeRoot||!hud.dialog||hud.levelButtons.Length!=20)throw new System.Exception("Delivered scene references are incomplete");
  foreach(var id in new[]{"Normal","Move","Rotate","Trap","Breakable","Spirit","Shadow","Crystal","Portal"}){
   var model=Resources.Load<ReferenceModelAsset>("ReferenceModels/"+id);if(!model||!model.material||model.parts.Any(p=>!p.mesh)||!model.material.mainTexture||!model.material.GetTexture("_BumpMap")||!model.material.GetTexture("_EmissionMap")||!model.material.IsKeywordEnabled("_EMISSION"))throw new System.Exception("Delivered model incomplete: "+id);
  }
  foreach(var renderer in game.editableBoard.GetComponentsInChildren<Renderer>(true))if(renderer.sharedMaterials.Any(m=>!m||!m.shader))throw new System.Exception("Delivered material reference missing: "+renderer.name);
  var lobby=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<PulseLobby>(true)).FirstOrDefault();
  if(game.startInLobby){
   if(!lobby||game.lobby!=lobby||!lobby.canvas||!lobby.safeRoot||!lobby.stage||!lobby.stage.spirit||!lobby.stage.view||!lobby.stage.backdrop.GetComponent<Renderer>().sharedMaterial.mainTexture)throw new System.Exception("Delivered lobby references are incomplete");
   foreach(var button in lobby.GetComponentsInChildren<LobbyButton>(true))if(button.lobby!=lobby||button.action==LobbyAction.None)throw new System.Exception("Disconnected lobby button: "+button.name);
   if(lobby.safeRoot.GetComponentsInChildren<LobbyButton>(true).Any(b=>b.transform.parent==lobby.safeRoot&&PulseLobby.RemovedHomeButton(b)))throw new System.Exception("Removed navigation / mode buttons still exist in the delivered scene");
   if(lobby.stage.view.cullingMask!=(1<<28))throw new System.Exception("Menu camera lost its dedicated render layer");
   foreach(var renderer in lobby.GetComponentsInChildren<Renderer>(true))if(renderer.sharedMaterials.Any(m=>!m||!m.shader))throw new System.Exception("Lobby material missing: "+renderer.name);
  }
  File.WriteAllText("../integrated-scene-verification.json","{\"success\":true,\"validatedProject\":\""+Path.GetFullPath(".").Replace('\\','/')+"\",\"scene\":\"Assets/Scenes/PULSESHIFT_Integrated.unity\",\"levels\":100,\"models\":9,\"missingScripts\":0,\"missingModelMaterials\":0}");
  Debug.Log("INTEGRATED_SCENE_VERIFIED");
  }finally{EditorSceneManager.ClosePreviewScene(scene);}
 }
}
