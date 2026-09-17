using UnityEngine;
using UnityEngine.SceneManagement;

// One session-wide policy. Never changes saved lighting, materials or the display resolution.
[DefaultExecutionOrder(2000)]
public sealed class MobilePerformance:MonoBehaviour {
 static MobilePerformance instance;
 static bool hardwareLite,sessionLite;static int quality=2;static bool wants60=true;
 static bool hasOverrides;static int originalAA,originalCascades,originalLights;static float originalShadowDistance;static ShadowResolution originalShadowResolution;static bool originalSoftParticles,originalProbes;
 CampaignGame game;PulseLobby menu;float warmUntil,elapsed;int frames,slowWindows;bool focused=true;
 public static bool IsMobile=>Application.isMobilePlatform&&!Application.isEditor;
 public static bool Lite=>IsMobile&&(hardwareLite||sessionLite||quality==0);
 public static int TargetFps=>MobilePerformancePolicy.TargetFps(IsMobile,hardwareLite,sessionLite,quality,wants60);
 public static string ModeCaption=>Lite?"АВТО · 30 FPS":TargetFps+" FPS";
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
 static void Reset(){instance=null;hardwareLite=sessionLite=hasOverrides=false;quality=2;wants60=true;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 static void Boot(){
  if(!IsMobile)return;
  hardwareLite=MobilePerformancePolicy.NeedsLite(SystemInfo.systemMemorySize,SystemInfo.processorCount,SystemInfo.graphicsDeviceName);
  var root=new GameObject("Mobile performance • automatic frame budget");DontDestroyOnLoad(root);instance=root.AddComponent<MobilePerformance>();
  SceneManager.sceneLoaded+=instance.SceneLoaded;Application.targetFrameRate=TargetFps;
  Debug.Log($"Mobile performance: {(hardwareLite?"lite":"standard")}, RAM {SystemInfo.systemMemorySize} MB, GPU {SystemInfo.graphicsDeviceName}");
 }
 public static void Apply(PulseProfile profile,bool gameplay){
  RestoreOverrides();
  quality=profile!=null?Mathf.Clamp(profile.quality,0,2):2;wants60=profile==null||profile.fps60;
  if(QualitySettings.names.Length>0)QualitySettings.SetQualityLevel(Mathf.Clamp(Mathf.RoundToInt(quality*(QualitySettings.names.Length-1)/2f),0,QualitySettings.names.Length-1),false);
  Application.targetFrameRate=TargetFps;
  if(!IsMobile)return;
  QualitySettings.vSyncCount=0;
  if(Lite){
   hasOverrides=true;originalAA=QualitySettings.antiAliasing;originalCascades=QualitySettings.shadowCascades;originalLights=QualitySettings.pixelLightCount;originalShadowDistance=QualitySettings.shadowDistance;originalShadowResolution=QualitySettings.shadowResolution;originalSoftParticles=QualitySettings.softParticles;originalProbes=QualitySettings.realtimeReflectionProbes;
   QualitySettings.antiAliasing=2;
   if(gameplay){QualitySettings.shadowCascades=1;QualitySettings.shadowResolution=ShadowResolution.Medium;QualitySettings.shadowDistance=Mathf.Min(QualitySettings.shadowDistance,30);QualitySettings.pixelLightCount=Mathf.Min(QualitySettings.pixelLightCount,2);QualitySettings.softParticles=false;QualitySettings.realtimeReflectionProbes=false;}
  }
 }
 static void RestoreOverrides(){
  if(!hasOverrides)return;
  QualitySettings.antiAliasing=originalAA;QualitySettings.shadowCascades=originalCascades;QualitySettings.pixelLightCount=originalLights;QualitySettings.shadowDistance=originalShadowDistance;QualitySettings.shadowResolution=originalShadowResolution;QualitySettings.softParticles=originalSoftParticles;QualitySettings.realtimeReflectionProbes=originalProbes;hasOverrides=false;
 }
 void SceneLoaded(Scene scene,LoadSceneMode mode){
  game=FindFirstObjectByType<CampaignGame>();menu=FindFirstObjectByType<PulseLobby>();ResetWindow();warmUntil=Time.unscaledTime+10;
 }
 void ResetWindow(){elapsed=0;frames=0;slowWindows=0;}
 void Update(){
  if(!focused||Time.unscaledTime<warmUntil||PulseSceneFlow.Loading||PulseAds.Fullscreen||TargetFps!=60){ResetWindow();return;}
  bool play=game&&!game.LobbyOpen&&!game.MotionPaused&&!game.Victorious&&!game.CurrentState.dead;
  bool home=menu&&menu.IsOpen&&menu.Page=="";
  if(!play&&!home){ResetWindow();return;}
  float dt=Time.unscaledDeltaTime;if(dt<=0||dt>.25f){ResetWindow();warmUntil=Time.unscaledTime+2;return;}
  elapsed+=dt;frames++;if(elapsed<6)return;
  slowWindows=MobilePerformancePolicy.SlowWindow(elapsed,frames,60)?slowWindows+1:0;
  elapsed=0;frames=0;
  // Three sustained windows; loading, ads and one-off stalls never select the tier.
  if(slowWindows<3)return;
  sessionLite=true;Apply(menu?menu.Profile:PulseSceneFlow.Profile,play);
  Debug.Log("Mobile performance: sustained slow frames; using lite / 30 FPS for this session.");
 }
 void OnApplicationPause(bool paused){focused=!paused;ResetWindow();warmUntil=Time.unscaledTime+5;}
 void OnApplicationFocus(bool focus){focused=focus;ResetWindow();warmUntil=Time.unscaledTime+5;}
 void OnDestroy(){SceneManager.sceneLoaded-=SceneLoaded;if(instance==this)instance=null;}
}
