using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Explicit one-time scene authoring. Ordinary launches/builds never rewrite the saved position.
public static class MenuHeroAnchorBuild {
 public static void Apply(){
  var scene=EditorSceneManager.OpenScene(SeparateMenuBuild.MenuPath);var lobby=Object.FindFirstObjectByType<PulseLobby>();var authored=lobby.stage.GetComponent<AuthoredMenuArt>();
  if(authored.artwork!=lobby.stage.spirit)throw new System.Exception("The menu artwork is not the live hero.");
  authored.lockArtworkPosition=true;EditorUtility.SetDirty(authored);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  Debug.Log("MENU_HERO_ANCHOR_ENABLED");SeparateMenuBuild.BuildExisting();
 }
}
