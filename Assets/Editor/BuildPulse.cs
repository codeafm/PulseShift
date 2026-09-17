using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class BuildPulse {
 public static void Setup(){
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var game=new GameObject("PULSESHIFT").AddComponent<PulseGame>();game.CreateEditableScene();
  System.IO.Directory.CreateDirectory("Assets/Art/Generated");
  foreach(var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){var mesh=filter.sharedMesh;if(mesh&&!AssetDatabase.Contains(mesh))AssetDatabase.CreateAsset(mesh,AssetDatabase.GenerateUniqueAssetPath("Assets/Art/Generated/Mesh.asset"));}
  foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))foreach(var mat in renderer.sharedMaterials){if(mat&&!AssetDatabase.Contains(mat)){foreach(var property in mat.GetTexturePropertyNames()){var texture=mat.GetTexture(property);if(texture&&!AssetDatabase.Contains(texture))AssetDatabase.CreateAsset(texture,AssetDatabase.GenerateUniqueAssetPath("Assets/Art/Generated/Texture.asset"));}AssetDatabase.CreateAsset(mat,AssetDatabase.GenerateUniqueAssetPath("Assets/Art/Generated/Material.mat"));}}
  System.IO.Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(scene,"Assets/Scenes/PULSESHIFT.unity");
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/PULSESHIFT.unity",true)};
  PlayerSettings.productName="PULSESHIFT";PlayerSettings.companyName="Pulse Studio";PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;PlayerSettings.colorSpace=ColorSpace.Linear;
  AssetDatabase.SaveAssets();Debug.Log("PULSE_SETUP_OK");
 }
 public static void Build(){Setup();var report=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"../Build/PULSESHIFT.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Build failed: "+report.summary.result);}
}
