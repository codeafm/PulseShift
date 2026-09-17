using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Explicit one-time integration of the user-provided clean background and live-light rig.
public static class MenuVistaBuild {
 const string TexturePath="Assets/Art/UI/MenuCastleVista.png";
 public static void ApplyOnce(){
  var importer=(TextureImporter)AssetImporter.GetAtPath(TexturePath);importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.alphaIsTransparency=false;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
  foreach(string target in new[]{"Standalone","Android"}){var p=importer.GetPlatformTextureSettings(target);p.overridden=true;p.maxTextureSize=2048;p.format=TextureImporterFormat.RGB24;p.textureCompression=TextureImporterCompression.Uncompressed;importer.SetPlatformTextureSettings(p);}importer.SaveAndReimport();
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);if(!texture||texture.width!=941||texture.height!=1672)throw new System.Exception("Unexpected castle background dimensions.");
  var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/UI/MenuCosmosBackdrop.mat");material.mainTexture=texture;material.SetFloat("_Exposure",.68f);material.SetFloat("_Saturation",.94f);material.SetFloat("_Vignette",.30f);material.SetFloat("_TopShade",.20f);material.SetFloat("_BottomShade",.26f);EditorUtility.SetDirty(material);
  var scene=EditorSceneManager.OpenScene(SeparateMenuBuild.MenuPath);var lobby=Object.FindFirstObjectByType<PulseLobby>();var stage=lobby.stage;stage.backdrop.GetComponent<Renderer>().sharedMaterial=material;stage.backdrop.name="MENU BACKGROUND • castle vista • editable material";
  var authored=stage.GetComponent<AuthoredMenuArt>();authored.artwork=stage.spirit;authored.viewport=new Rect(.33f,.35f,.27f,.20f);authored.adaptToScreen=true;authored.lockArtworkPosition=true;
  if(stage.GetComponent<MenuHeroLighting>())throw new System.Exception("Menu light rig already exists; edit the saved objects instead.");
  Light Add(Transform parent,string name,LightType type,Color color,float intensity){var lightObject=new GameObject(name);lightObject.layer=28;lightObject.transform.SetParent(parent,false);var light=lightObject.AddComponent<Light>();light.type=type;light.color=color;light.intensity=intensity;light.cullingMask=1<<28;light.shadows=LightShadows.None;light.renderMode=LightRenderMode.ForcePixel;return light;}
  var rig=stage.gameObject.AddComponent<MenuHeroLighting>();rig.stage=stage;rig.hero=stage.spirit;rig.view=stage.view;
  rig.overhead=Add(stage.transform,"Hero light • cyan from above",LightType.Spot,new Color(.42f,.84f,1),3.2f);
  rig.rim=Add(stage.transform,"Hero light • warm sunrise rim",LightType.Point,new Color(1,.56f,.31f),.85f);
  rig.contact=Add(stage.transform,"Hero light • animated contact glow",LightType.Point,new Color(.04f,.7f,1),1.35f);
  EditorUtility.SetDirty(authored);EditorUtility.SetDirty(rig);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("MENU_CASTLE_VISTA_AND_LIGHTS_APPLIED");SeparateMenuBuild.BuildExisting();
 }
}
