using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using PulseCampaign;
using System.IO;
using System.Linq;

[CustomEditor(typeof(CampaignGame))]
public class CampaignInspector:Editor {
 public override void OnInspectorGUI(){DrawDefaultInspector();EditorGUILayout.HelpBox("Preview Level selects a campaign level. Generate Preview creates its islands in Scene. Saving the scene preserves your artwork edits. Logic and connections remain in Resources/Campaign/levels.json.",MessageType.Info);
  GUI.enabled=!EditorApplication.isPlaying;
  if(GUILayout.Button("Generate selected level in Scene")){var game=(CampaignGame)target;if(EditorUtility.DisplayDialog("Rebuild generated level","This replaces the generated island group for this scene. Save a separate scene first if you want to keep your current artwork edits.","Generate","Cancel")){Undo.RegisterFullObjectHierarchyUndo(game.gameObject,"Generate campaign preview");game.BakePreview();CampaignBuild.PersistArt();EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);Selection.activeGameObject=game.editableBoard.gameObject;SceneView.lastActiveSceneView?.FrameSelected();}}
  if(GUILayout.Button("Verify campaign logic"))CampaignBuild.Verify();GUI.enabled=true;
 }
}
public static class CampaignBuild {
 public const string ScenePath="Assets/Scenes/PULSESHIFT_Campaign.unity";
 [MenuItem("PULSESHIFT/Open 100-level campaign")]
 public static void Open(){if(EditorApplication.isPlaying){Debug.LogWarning("Stop Play before opening the campaign scene.");return;}if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()){PlayerSettings.colorSpace=ColorSpace.Linear;EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};EditorSceneManager.OpenScene(ScenePath);var game=Object.FindFirstObjectByType<CampaignGame>();if(game){Selection.activeGameObject=game.editableBoard.gameObject;SceneView.lastActiveSceneView?.FrameSelected();}}}
 public static void PersistArt(){
  Directory.CreateDirectory("Assets/Art/Campaign");
  foreach(var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var mesh=filter.sharedMesh;if(mesh&&!AssetDatabase.Contains(mesh))AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath("Assets/Art/Campaign/Model.asset"));}
  foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))foreach(var material in renderer.sharedMaterials){if(material&&!AssetDatabase.Contains(material)){foreach(var property in material.GetTexturePropertyNames()){var texture=material.GetTexture(property);if(texture&&!AssetDatabase.Contains(texture))AssetDatabase.CreateAsset(texture,AssetDatabase.GenerateUniqueAssetPath("Assets/Art/Campaign/Surface.asset"));}AssetDatabase.CreateAsset(material,AssetDatabase.GenerateUniqueAssetPath("Assets/Art/Campaign/Material.mat"));}}
  AssetDatabase.SaveAssets();
 }
 public static void Verify(){
  var file=Resources.Load<TextAsset>("Campaign/levels");var data=JsonUtility.FromJson<Campaign>(file.text);
  if(data.levels.Length==0)throw new System.Exception("Campaign is empty");
  if(data.levels.Select(l=>l.id).Distinct().Count()!=data.levels.Length)throw new System.Exception("Duplicate level identifiers");
  foreach(var level in data.levels){CampaignGenerator.ValidateDefinition(level);Solver.Replay(level,level.solution);var search=Solver.Solve(level,Rules.Initial(level));if(search.path==null)throw new System.Exception("Unity solver found no solution on level "+level.id);}
  Debug.Log($"UNITY_CAMPAIGN_LOGIC_VERIFIED {data.levels.Length}/{data.levels.Length}");
 }
 public static void Setup(){
  Verify();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var game=new GameObject("PULSESHIFT • 100-level campaign").AddComponent<CampaignGame>();game.BakePreview();PersistArt();
  Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,ScenePath);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
  PlayerSettings.productName="PULSESHIFT Campaign";PlayerSettings.companyName="Pulse Studio";PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;PlayerSettings.colorSpace=ColorSpace.Linear;AssetDatabase.SaveAssets();
 }
 public static void Build(){Setup();var result=BuildPipeline.BuildPlayer(new[]{ScenePath},"../BuildCampaign/PULSESHIFT.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Campaign build failed: "+result.summary.result);Debug.Log("CAMPAIGN_BUILD_OK");}
}
