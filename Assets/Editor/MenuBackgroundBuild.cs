using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;

// Explicit one-time integration of the user-supplied menu background.
// This is intentionally not invoked by scene loading or an ordinary build.
public static class MenuBackgroundBuild {
 const string TexturePath="Assets/Art/UI/MenuCosmosBackdrop.png";
 const string MaterialPath="Assets/Art/UI/MenuCosmosBackdrop.mat";
 public static void ApplyOnce(){
  if(File.Exists(MaterialPath))throw new System.Exception("Menu background is already integrated; edit the saved scene/material instead.");
  var importer=(TextureImporter)AssetImporter.GetAtPath(TexturePath);
  importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.alphaIsTransparency=false;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
  foreach(string platform in new[]{"Standalone","Android"}){var p=importer.GetPlatformTextureSettings(platform);p.overridden=true;p.maxTextureSize=2048;p.format=TextureImporterFormat.RGB24;p.textureCompression=TextureImporterCompression.Uncompressed;importer.SetPlatformTextureSettings(p);}importer.SaveAndReimport();
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);if(!texture||texture.width!=874||texture.height!=1800)throw new System.Exception("Unexpected menu background dimensions.");
  var material=new Material(Shader.Find("Unlit/Texture")){name="Menu / supplied cosmic archipelago"};material.mainTexture=texture;AssetDatabase.CreateAsset(material,MaterialPath);
  var scene=EditorSceneManager.OpenScene(SeparateMenuBuild.MenuPath);var lobby=Object.FindFirstObjectByType<PulseLobby>();var stage=lobby.stage;
  stage.backdrop.GetComponent<Renderer>().sharedMaterial=material;stage.backdrop.name="MENU BACKGROUND • supplied art • editable material";
  // The image already contains the portal and islands. Keep the animated 3D hero, remove visual duplicates.
  stage.portalGroup.gameObject.SetActive(false);stage.bridgeA.gameObject.SetActive(false);stage.bridgeB.gameObject.SetActive(false);
  HideHeroPlatform(stage);
  var authored=stage.GetComponent<AuthoredMenuArt>();authored.artwork=stage.spirit;authored.viewport=new Rect(.36f,.34f,.28f,.20f);authored.adaptToScreen=true;
  var greet=lobby.safeRoot.Find("Tap the living hero") as RectTransform;greet.anchorMin=greet.anchorMax=new Vector2(.5f,.43f);greet.anchoredPosition=Vector2.zero;greet.sizeDelta=new Vector2(190,250);
  EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("MENU_BACKGROUND_APPLIED_874x1800");SeparateMenuBuild.BuildExisting();
 }
 static void HideHeroPlatform(LobbyStage stage){var heroPlatform=stage.heroGroup.Cast<Transform>().FirstOrDefault(t=>t!=stage.spirit&&t.name.StartsWith("Normal"));if(!heroPlatform)throw new System.Exception("Hero platform was not found.");heroPlatform.gameObject.SetActive(false);}
}
