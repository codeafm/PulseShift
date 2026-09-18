using System;

// This directory is outside Assets: these doubles never enter the game build.
namespace UnityEngine {
 public class Object {
  public bool Destroyed;
  public static implicit operator bool(Object value)=>value!=null&&!value.Destroyed;
 }
 public class MonoBehaviour:Object {}
 public enum RuntimePlatform { Android,IPhonePlayer,WindowsEditor }
 public static class Application { public static bool isEditor;public static RuntimePlatform platform; }
 public static class Time { public static float unscaledTime; }
 public static class AudioListener { public static bool pause; }
 public static class Debug {
  public static bool isDebugBuild;
  public static void LogWarning(object value){}
 }
 public static class Mathf {
  public static int Min(int a,int b)=>Math.Min(a,b);
  public static int Max(int a,int b)=>Math.Max(a,b);
  public static int Clamp(int v,int a,int b)=>Math.Min(b,Math.Max(a,v));
  public static float Clamp01(float v)=>Math.Min(1,Math.Max(0,v));
 }
 public static class JsonUtility { public static T FromJson<T>(string s)=>default(T);public static string ToJson(object o)=>"test-profile"; }
 public static class PlayerPrefs {
  public static int Saves;
  public static string GetString(string k,string v)=>v;
  public static void SetString(string k,string v){}
  public static void Save(){Saves++;}
 }
}
namespace YandexMobileAds.Base {
 public class AdRequest { public string Id;public AdRequest(string id){Id=id;} }
 public class AdFailedToLoadEventArgs:EventArgs { public string Message="test no-fill"; }
 public class AdFailureEventArgs:EventArgs { public string Message="test show failure"; }
}
namespace YandexMobileAds {
 using YandexMobileAds.Base;
 public class InterstitialAdLoader {
  public static InterstitialAdLoader Last;
  public static int Constructions,Requests;
  public string RequestedId;
  public bool Cancelled;
  Action<Interstitial> loaded;Action<AdFailedToLoadEventArgs> failed;
  public InterstitialAdLoader(){Last=this;Constructions++;}
  public void LoadAd(AdRequest request,Action<Interstitial> onLoaded,Action<AdFailedToLoadEventArgs> onFailed){
   RequestedId=request.Id;Requests++;loaded=onLoaded;failed=onFailed;
  }
  public void Complete(Interstitial ad){loaded(ad);}
  public void Fail(){failed(new AdFailedToLoadEventArgs());}
  public void CancelLoading(){Cancelled=true;}
 }
 public class Interstitial {
  public event EventHandler<EventArgs> OnAdDismissed;
  public event EventHandler<AdFailureEventArgs> OnAdFailedToShow;
  public bool Destroyed,ThrowOnShow,ThrowOnDestroy;
  public int Shows;
  public void Show(){Shows++;if(ThrowOnShow)throw new Exception("test show exception");}
  // Retain handlers deliberately to simulate stale/duplicate native callbacks.
  public void Destroy(){Destroyed=true;if(ThrowOnDestroy)throw new Exception("test cleanup exception");}
  public void Dismiss(){OnAdDismissed?.Invoke(this,EventArgs.Empty);}
  public void FailShow(){OnAdFailedToShow?.Invoke(this,new AdFailureEventArgs());}
 }
}
public sealed partial class PulseAds:UnityEngine.MonoBehaviour {
 public static PulseAds Instance {get;private set;}
 bool showing,foreground=true,mutedBefore;
 static bool Device=>!UnityEngine.Application.isEditor&&(UnityEngine.Application.platform==UnityEngine.RuntimePlatform.Android||UnityEngine.Application.platform==UnityEngine.RuntimePlatform.IPhonePlayer);
 public bool BannerVisible=true,RewardAvailable;
 public string Status="test status";
 public static bool Fullscreen=>Instance&&Instance.showing;
 Action reward;Action<bool> rewardFinished;
 public PulseAds(){Instance=this;}
 void SetBannerVisible(bool value){BannerVisible=value;}
 public bool ShowReward(Action onReward,Action<bool> onFinished){
  if(!RewardAvailable||showing)return false;
  showing=true;reward=onReward;rewardFinished=onFinished;return true;
 }
 public void CompleteReward(bool success){showing=false;if(success)reward();rewardFinished(success);}
 public void SetForeground(bool value){foreground=value;}
 public void DisposeForTest(){Destroyed=true;DisposeInterstitial();if(showing)UnityEngine.AudioListener.pause=mutedBefore;}
}
public enum LobbyMode { Campaign,Challenge,Endless }
public enum CampaignUICommand { Home,Restart,Levels,SelectLevel,Primary,Undo,Pause,Pulse,Hint }
public class TestLevel {public int id;}
public class TestState {public bool dead;}
public class TestCampaign {public TestLevel[] levels=new TestLevel[100];}
public partial class CampaignGame:UnityEngine.MonoBehaviour {
 public bool testing,busy,showLesson,levelMenu;
 public int unlocked=100;
 public bool LobbyOpen,Victorious,CanUndo=true,ResultVisible=true;
 public LobbyMode ActiveMode=LobbyMode.Campaign;
 public TestLevel level=new TestLevel {id=6};
 public TestState state=new TestState();
 public TestCampaign Data=new TestCampaign();
 public PulseProfile UserProfile=new PulseProfile();
 public int Transitions;public CampaignUICommand LastCommand;public int LastValue;
 string message;
 public void RequestUI(CampaignUICommand command,int value=0){
  if(PulseAds.Fullscreen||TryResultInterstitial(command,value))return;
  Transitions++;LastCommand=command;LastValue=value;
 }
 public void Outcome(bool won){
  state.dead=!won;Victorious=won;
  if(won)RecordAdVictory();
  RecordInterstitialResult(won);
 }
 public void NewLevel(int id){level.id=id;state.dead=Victorious=false;ResetAdVictory();}
}
