using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YandexMobileAds;
using YandexMobileAds.Base;

// One SDK owner across scenes. Editor never requests or simulates paid impressions.
public sealed partial class PulseAds : MonoBehaviour {
 public static PulseAds Instance {get;private set;}
 public static bool Fullscreen=>Instance&&Instance.showing;
 public static float BottomInsetPixels=>Instance&&Instance.dockVisible?Instance.bannerPixels+12*Instance.density:0;
 public bool RewardReady=>rewarded!=null&&!showing&&Time.unscaledTime-loadedAt<1800;
 public string Status {get;private set;}="Реклама загружается…";
 Banner banner;RewardedAdLoader loader;RewardedAd rewarded;
 Action grant;Action<bool> finished;
 bool showing,loading,earned,foreground=true,bannerLoaded,bannerVisible,mutedBefore;
 float nextLoad,bannerPixels,density=1,loadedAt;int failures,screenWidth;
 PulseLobby menu;CampaignGame game;
 RectTransform bannerDock;
 bool dockVisible;int measuredWidth,measuredHeight;Rect measuredSafe;
 const int BannerWidthDp=350,BannerHeightDp=50;
 static bool Device=>!Application.isEditor&&(Application.platform==RuntimePlatform.Android||Application.platform==RuntimePlatform.IPhonePlayer);
 string RewardId=>Debug.isDebugBuild?"demo-rewarded-yandex":Application.platform==RuntimePlatform.IPhonePlayer?"R-M-20061555-2":"R-M-20061525-2";
 string BannerId=>Debug.isDebugBuild?"demo-banner-yandex":Application.platform==RuntimePlatform.IPhonePlayer?"R-M-20061555-1":"R-M-20061525-1";
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Boot(){if(!Instance)new GameObject("Yandex Ads • persistent owner").AddComponent<PulseAds>();}
 void Awake(){if(Instance&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);SceneManager.sceneLoaded+=SceneLoaded;if(Device){YandexAds.SetLocationTracking(false);loader=new RewardedAdLoader();}else Status="Реклама доступна в Android / iOS сборке";}
 void SceneLoaded(Scene scene,LoadSceneMode mode){menu=FindFirstObjectByType<PulseLobby>();game=FindFirstObjectByType<CampaignGame>();SetBannerVisible(false);}
 void Update(){
  MeasureBannerSlot();
  bool home=menu&&menu.IsOpen,play=game&&!game.LobbyOpen&&!home;
  bool visible=foreground&&!showing&&!PulseSceneFlow.Loading&&((home&&menu.Page=="")||(play&&!game.Paused&&!game.MenuOpen&&!game.LessonOpen&&!game.Victorious&&!game.CurrentState.dead));
  dockVisible=visible;
  if(Device&&visible&&banner==null&&screenWidth!=Screen.width){try{CreateBanner();}catch(Exception error){Debug.LogError("Yandex banner initialization failed: "+error);}}
  if(banner!=null&&screenWidth!=Screen.width){banner.Destroy();banner=null;bannerLoaded=false;SetBannerVisible(false);}
  SetBannerVisible(visible&&bannerLoaded);
  LayoutBannerDock();
  // Preload only when a reward screen is likely to be used. Never loop on no-fill.
  if(home&&menu.Page=="Награда"||play&&game.VictoryRewardAvailable)PrepareReward();
  if(play&&game.InterstitialEligible)PrepareInterstitial();
 }
 void MeasureBannerSlot(){
  if(measuredWidth==Screen.width&&measuredHeight==Screen.height&&measuredSafe==Screen.safeArea)return;
  measuredWidth=Screen.width;measuredHeight=Screen.height;measuredSafe=Screen.safeArea;
  int dp=Device?Math.Max(1,ScreenUtils.ConvertPixelsToDp((int)Screen.safeArea.width)):390;
  density=Screen.safeArea.width/dp;bannerPixels=BannerHeightDp*density;
 }
 void CreateBanner(){
  screenWidth=Screen.width;int dp=Math.Max(1,ScreenUtils.ConvertPixelsToDp((int)Screen.safeArea.width));density=Screen.safeArea.width/dp;
  // Never crop an ad on a device narrower than the requested fixed placement.
  if(dp<BannerWidthDp){Debug.LogWarning($"Yandex banner skipped: available {dp} dp, required {BannerWidthDp} dp. Empty panel remains visible.");return;}
  // SDK 8.4 Android has no fixedSize native method. Inline bounds the slot to 350x50 dp.
  var size=BannerAdSize.Inline(BannerWidthDp,BannerHeightDp);
  // Android getHeightInPixels already includes density; iOS returns UIKit points.
  bannerPixels=size.Height*(Application.platform==RuntimePlatform.IPhonePlayer?density:1);
  var owned=new Banner(size,AdPosition.BottomCenter);banner=owned;owned.Hide();
  owned.OnAdLoaded+=(s,e)=>{if(banner==owned){bannerLoaded=true;Debug.Log("Yandex banner loaded: 350x50 dp");}};
  owned.OnAdFailedToLoad+=(s,e)=>{Debug.LogWarning("Yandex banner load failed (350x50 dp, "+BannerId+"): "+e.Message);};
  Debug.Log($"Yandex banner request: {BannerId}, 350x50 dp, available {dp} dp, density {density:F2}");
  owned.LoadAd(new AdRequest(BannerId));
  // SDK owns automatic refresh (60 seconds). Custom 50 seconds requires Yandex approval.
 }
 void SetBannerVisible(bool visible){if(bannerVisible==visible)return;bannerVisible=visible;if(banner==null)return;if(visible)banner.Show();else banner.Hide();}
 void LayoutBannerDock(){
  if(!bannerDock&&dockVisible){
   var canvasObject=new GameObject("Ads • bottom dock",typeof(RectTransform),typeof(Canvas));canvasObject.transform.SetParent(transform,false);
   var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=80;
   var panel=new GameObject("Non-interactive neon surround",typeof(RectTransform),typeof(AdPanelFrame));panel.transform.SetParent(canvasObject.transform,false);
   bannerDock=panel.GetComponent<RectTransform>();bannerDock.anchorMin=bannerDock.anchorMax=new Vector2(.5f,0);bannerDock.pivot=new Vector2(.5f,0);
   panel.GetComponent<AdPanelFrame>().raycastTarget=false;
  }
  if(!bannerDock)return;bannerDock.gameObject.SetActive(dockVisible);
  var safe=Screen.safeArea;bannerDock.anchoredPosition=new Vector2(safe.center.x-Screen.width*.5f,safe.yMin);
  bannerDock.sizeDelta=new Vector2(Mathf.Min(safe.width,(BannerWidthDp+16)*density),bannerPixels+12*density);
  bannerDock.GetComponent<AdPanelFrame>().SetDensity(density);
 }
 public void PrepareReward(){
  if(!Device||loading||showing||!foreground||failures>=3||Time.unscaledTime<nextLoad)return;
  if(rewarded!=null){if(Time.unscaledTime-loadedAt<1800)return;rewarded.Destroy();rewarded=null;}
  loading=true;Status="Реклама загружается…";
  loader.LoadAd(new AdRequest(RewardId),ad=>{if(!this){ad.Destroy();return;}loading=false;rewarded=ad;loadedAt=Time.unscaledTime;failures=0;Status="Смотреть рекламу";},error=>{if(!this)return;loading=false;failures++;nextLoad=Time.unscaledTime+(float)Math.Min(300,30*Math.Pow(2,Math.Min(failures-1,4)));Status="Реклама пока недоступна. Попробуй позже";Debug.LogWarning("Yandex rewarded: "+error.Message);});
 }
 public bool ShowReward(Action onReward,Action<bool> onFinished){
  if(!RewardReady){if(failures>=3&&Time.unscaledTime>=nextLoad)failures=0;PrepareReward();return false;}
  showing=true;earned=false;grant=onReward;finished=onFinished;mutedBefore=AudioListener.pause;AudioListener.pause=true;SetBannerVisible(false);
  var ad=rewarded;ad.OnRewarded+=(s,e)=>{if(!showing||earned||rewarded!=ad)return;earned=true;var action=grant;grant=null;action?.Invoke();};
  ad.OnAdDismissed+=(s,e)=>Finish(ad);ad.OnAdFailedToShow+=(s,e)=>{Status="Не удалось показать рекламу";Finish(ad);};
  try{ad.Show();}catch(Exception e){Debug.LogWarning(e.Message);Finish(ad);}return true;
 }
 void Finish(RewardedAd ad){if(!showing||rewarded!=ad)return;var callback=finished;bool success=earned;showing=false;grant=null;finished=null;rewarded=null;ad.Destroy();AudioListener.pause=mutedBefore;nextLoad=Time.unscaledTime+2;callback?.Invoke(success);}
 void OnApplicationPause(bool paused){foreground=!paused;if(paused)SetBannerVisible(false);}
 void OnDestroy(){if(Instance!=this)return;SceneManager.sceneLoaded-=SceneLoaded;banner?.Destroy();loader?.CancelLoading();rewarded?.Destroy();DisposeInterstitial();if(showing)AudioListener.pause=mutedBefore;Instance=null;}
}

// Decoration only: no labels, fake ad controls, masks or input interception.
public sealed class AdPanelFrame:MaskableGraphic {
 float density=1;
 public void SetDensity(float value){if(Mathf.Approximately(density,value))return;density=value;SetVerticesDirty();}
 protected override void OnPopulateMesh(VertexHelper mesh){
  mesh.Clear();var rect=rectTransform.rect;
  // Quiet cyan halo and fine rounded outline, behind the native advertising view.
  Rounded(mesh,rect,10*density,new Color(.02f,.55f,.85f,.08f),new Color(.01f,.24f,.45f,.04f));
  Rounded(mesh,Inset(rect,2*density),9*density,new Color(.04f,.64f,.88f,.22f),new Color(.02f,.35f,.57f,.12f));
  Rounded(mesh,Inset(rect,3*density),8*density,new Color(.15f,.7f,.92f,.75f),new Color(.04f,.35f,.55f,.6f));
  Rounded(mesh,Inset(rect,3.7f*density),7.3f*density,new Color(.015f,.05f,.095f,.98f),new Color(.003f,.014f,.035f,.98f));
 }
 static Rect Inset(Rect r,float amount)=>Rect.MinMaxRect(r.xMin+amount,r.yMin+amount,r.xMax-amount,r.yMax-amount);
 static void Rounded(VertexHelper mesh,Rect r,float radius,Color top,Color bottom){
  radius=Mathf.Min(radius,Mathf.Min(r.width,r.height)*.5f);int start=mesh.currentVertCount;
  mesh.AddVert(r.center,Color.Lerp(bottom,top,.5f),Vector2.zero);
  const int steps=8;int count=4*(steps+1);
  for(int corner=0;corner<4;corner++){
   var center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
   for(int step=0;step<=steps;step++){
    float angle=(corner*90+step*90f/steps)*Mathf.Deg2Rad;
    var point=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
    mesh.AddVert(point,Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,point.y)),Vector2.zero);
   }
  }
  for(int i=0;i<count;i++)mesh.AddTriangle(start,start+1+i,start+1+(i+1)%count);
 }
}
