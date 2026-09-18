using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

// Apply through Unity's API as well as saved settings: the open Editor may
// still hold older PlayerSettings in memory when files change externally.
[InitializeOnLoad]
public sealed class PulseBranding : IPreprocessBuildWithReport {
 const string IconPath="Assets/Art/AppIcon/PULSESHIFT-Adventure-Icon.png";
 public int callbackOrder => -2000;
 static PulseBranding(){EditorApplication.delayCall+=ApplyWhenReady;}
 static void ApplyWhenReady(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating){EditorApplication.delayCall+=ApplyWhenReady;return;}
  try{Apply();}catch(System.Exception e){Debug.LogWarning("PULSESHIFT branding setup: "+e.Message);}
 }
 [MenuItem("PULSESHIFT/Apply game icon and disable Unity splash")]
 public static void Apply(){
  var icon=AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
  if(!icon){
   var file=Path.Combine(Application.dataPath,"Art/AppIcon/PULSESHIFT-Adventure-Icon.png");
   if(!File.Exists(file))throw new BuildFailedException("PULSESHIFT icon file is missing: "+file);
   AssetDatabase.ImportAsset(IconPath,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
   icon=AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
  }
  if(!icon)throw new BuildFailedException("PULSESHIFT icon could not be decoded after synchronous import: "+IconPath+". Check texture importer errors in Console.");
  PlayerSettings.SplashScreen.show=false;
  PlayerSettings.SplashScreen.showUnityLogo=false;
  PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});
  foreach(var kind in PlayerSettings.GetSupportedIconKindsForPlatform(BuildTargetGroup.Android)){
   var slots=PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android,kind);
   foreach(var slot in slots){
    // Populate every mandatory layer, including Android adaptive foreground/background.
    var layers=new Texture2D[slot.minLayerCount];
    for(int i=0;i<layers.Length;i++)layers[i]=icon;
    slot.SetTextures(layers);
   }
   PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android,kind,slots);
  }
  AssetDatabase.SaveAssets();
  Debug.Log("PULSESHIFT_BRANDING_APPLIED: adventure icon, Unity splash disabled");
 }
 public void OnPreprocessBuild(BuildReport report){Apply();}
}
