using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class CinematicBuild {
 public static void Configure(){
  MobileBuild.Configure();var scene=EditorSceneManager.OpenScene(IntegratedBuild.ScenePath);var game=Object.FindFirstObjectByType<CampaignGame>();
  // V2 stone albedo stays neutral; blue comes from moonlight and recessed energy glass.
  foreach(string id in new[]{"Normal","Move","Rotate","Trap","Breakable","Spirit","Shadow","Crystal","Portal"}){
   var asset=Resources.Load<ReferenceModelAsset>("ReferenceModels/"+id);var m=asset.material;
   m.SetColor("_EmissionColor",Color.white*(id=="Spirit"?1.45f:id=="Portal"?3.1f:id=="Crystal"?3.5f:2.8f));m.SetFloat("_BumpScale",.82f);m.SetFloat("_GlossMapScale",.72f);EditorUtility.SetDirty(m);
  }
  RenderSettings.fog=false;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.078f,.095f,.14f);
  foreach(var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
   if(light.type!=LightType.Directional)continue;
   if(light.name.Contains("Moon key")){light.intensity=1.25f;light.color=new Color(.81f,.87f,1);light.shadows=LightShadows.Soft;light.shadowStrength=.85f;light.shadowBias=.025f;}
   if(light.name.Contains("Amber rim")){light.intensity=.34f;light.color=new Color(1,.68f,.38f);}
   if(light.name.Contains("Lobby •")){light.intensity=1.2f;light.color=new Color(.61f,.78f,1);}
  }
  foreach(var bloom in Object.FindObjectsByType<PulseBloom>(FindObjectsInactive.Include,FindObjectsSortMode.None)){bloom.bloom=.3f;bloom.exposure=1.04f;bloom.contrast=1.06f;}
  foreach(var mist in Object.FindObjectsByType<PulseAtmosphere>(FindObjectsInactive.Include,FindObjectsSortMode.None))mist.density=.58f;
  var lobby=game.lobby;var bg=lobby.stage.backdrop.GetComponent<Renderer>().sharedMaterial;var tex=bg.mainTexture;bg.shader=Resources.Load<Shader>("PulseClouds");bg.mainTexture=tex;bg.SetFloat("_UseBackdrop",1);bg.SetFloat("_ScreenAspect",tex.width/(float)tex.height);EditorUtility.SetDirty(bg);
  foreach(var identity in lobby.stage.GetComponentsInChildren<ReferenceIdentity>(true)){var old=identity.transform.Find("Cinematic • basalt ivy");if(old)Object.DestroyImmediate(old.gameObject);CinematicArt.Dress(identity.gameObject,identity.modelId);}
  var contact=lobby.stage.spirit.Find("Soft contact radiance");if(contact)contact.localScale=Vector3.one*.34f;
  game.BakePreview();CampaignHUD.Ensure(game);lobby.UpgradeLayout();CampaignBuild.PersistArt();
  AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,IntegratedBuild.ScenePath);
 }
 public static void Desktop(){Configure();IntegratedBuild.BuildCleanLobby();}
 public static void Android(){Configure();MobileBuild.Android();}
}
