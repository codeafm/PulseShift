using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public static class MobileBuild {
 public static void Configure(){
  var texture=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Lobby/ArchipelagoBackdrop.png");
  texture.npotScale=TextureImporterNPOTScale.None;texture.mipmapEnabled=false;texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Bilinear;
  texture.maxTextureSize=2048;texture.textureCompression=TextureImporterCompression.Uncompressed;texture.ignoreMipmapLimit=true;
  var android=texture.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=2048;android.format=TextureImporterFormat.ETC2_RGB4;android.compressionQuality=100;android.crunchedCompression=false;texture.SetPlatformTextureSettings(android);texture.SaveAndReimport();
  PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
  PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=false;PlayerSettings.allowedAutorotateToLandscapeRight=false;
  PlayerSettings.Android.renderOutsideSafeArea=true;
  var scene=EditorSceneManager.OpenScene(IntegratedBuild.ScenePath);var game=Object.FindFirstObjectByType<CampaignGame>();
  PulseAtmosphere.Ensure(game.gameCamera);game.lobby=PulseLobby.Ensure(game);PulseAtmosphere.Ensure(game.lobby.stage.view);
  Directory.CreateDirectory("Assets/Art/Mobile");
  const string backgroundPath="Assets/Art/Mobile/GameplayBackdrop.mat";
  var background=AssetDatabase.LoadAssetAtPath<Material>(backgroundPath);
  if(!background){background=new Material(Resources.Load<Shader>("PulseClouds")){name="Mobile / full-bleed archipelago"};AssetDatabase.CreateAsset(background,backgroundPath);}
  background.SetTexture("_MainTex",Resources.Load<Texture2D>("Lobby/ArchipelagoBackdrop"));background.SetFloat("_UseBackdrop",1);background.SetFloat("_ScreenAspect",9f/16);EditorUtility.SetDirty(background);
  game.gameCamera.transform.Find("Cloud backdrop").GetComponent<Renderer>().sharedMaterial=background;
  game.editableBoard.Find("Hero").localScale=Vector3.one*game.actorReadabilityScale;
  EditorSceneManager.SaveScene(scene,IntegratedBuild.ScenePath);AssetDatabase.SaveAssets();
 }
 public static void Desktop(){Configure();IntegratedBuild.BuildCleanLobby();}
 [MenuItem("PULSESHIFT/Build Android mobile APK")]
 public static void Android(){
  if(File.Exists(SeparateMenuBuild.MenuPath)){SeparateMenuBuild.Android();return;}
  Configure();
  PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
  PlayerSettings.Android.optimizedFramePacing=true;
  Directory.CreateDirectory("../BuildAndroid");EditorUserBuildSettings.buildAppBundle=false;
  var result=BuildPipeline.BuildPlayer(new[]{IntegratedBuild.ScenePath},"../BuildAndroid/PULSESHIFT-Mobile.apk",BuildTarget.Android,BuildOptions.None);
  if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Android build: "+result.summary.result);
  File.WriteAllText("../BuildAndroid/build-verification.json","{\"success\":true,\"package\":\""+PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)+"\",\"physicalDeviceTested\":false,\"orientation\":\"portrait\",\"backend\":\"IL2CPP ARM64\"}");
  Debug.Log("MOBILE_ANDROID_BUILD_VERIFIED");
 }
}
