using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using PulseCampaign;

// Only small session data survives a scene change. No camera, board or menu is persistent.
public static class PulseSceneFlow {
 public const string MenuScene="PULSESHIFT_Menu",GameScene="PULSESHIFT_Game";
 static Campaign data;static PulseProfile profile;static int pendingLevel=-1;static bool loading;
 public static bool Testing=>Environment.GetCommandLineArgs().Contains("-sceneVerify");
 public static Campaign Data=>data??(data=JsonUtility.FromJson<Campaign>(Resources.Load<TextAsset>("Campaign/levels").text));
 public static PulseProfile Profile=>profile??(profile=PulseProfile.Load(Testing));
 public static int Unlocked=>Testing?Data.levels.Length:Mathf.Clamp(PlayerPrefs.GetInt("PulseCampaign.Variety2.Unlocked",1),1,Data.levels.Length);
 public static int Stars(int id)=>Testing?0:PlayerPrefs.GetInt("PulseCampaign.Variety2.Stars."+id,0);
 public static LobbyMode Mode{get;private set;}
 public static int EndlessCount{get;private set;}
 public static int ModeTarget{get;private set;}
 public static bool Loading=>loading;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
 static void Reset(){data=null;profile=null;pendingLevel=-1;loading=false;Mode=LobbyMode.Campaign;EndlessCount=ModeTarget=0;}
 public static int TakeLevel(int fallback){int selected=pendingLevel<0?fallback:pendingLevel;pendingLevel=-1;return Mathf.Clamp(selected,0,Data.levels.Length-1);}
 public static void Play(int index,LobbyMode mode){
  if(loading)return;pendingLevel=Mathf.Clamp(index,0,Data.levels.Length-1);Mode=mode;EndlessCount=0;ModeTarget=Data.levels[pendingLevel].optimalTurns+2;Load(GameScene);
 }
 public static void Home(){if(loading)return;pendingLevel=-1;Mode=LobbyMode.Campaign;EndlessCount=0;Profile.Save(Testing);Load(MenuScene);}
 public static void Advance(CampaignGame game){if(Mode==LobbyMode.Endless){int next=10+(EndlessCount*17+DateTime.UtcNow.DayOfYear)%Mathf.Max(1,Data.levels.Length-10);game.BeginLobbyLevel(next);}else Home();}
 public static void RecordVictory(int level,int turns,int stars){
  var p=Profile;p.wins++;p.RecordFirstWin(level);string today=DateTime.UtcNow.ToString("yyyy-MM-dd");
  if(Mode==LobbyMode.Challenge&&turns<=ModeTarget&&p.challengeDay!=today){p.challengeDay=today;p.resonance+=2;p.crystals+=100;}
  if(Mode==LobbyMode.Endless){EndlessCount++;p.bestEndless=Mathf.Max(p.bestEndless,EndlessCount);if(EndlessCount%3==0)p.resonance++;}
  p.Save(Testing);
 }
 static void Load(string scene){
  if(!Application.CanStreamedLevelBeLoaded(scene)){Debug.LogError("Scene is missing from Build Settings: "+scene);return;}
  loading=true;var host=new GameObject("Scene transition • temporary");UnityEngine.Object.DontDestroyOnLoad(host);host.AddComponent<PulseSceneTransition>().Begin(scene);
 }
 public static void TransitionFinished(){loading=false;}
}

public class PulseSceneTransition:MonoBehaviour {
 CanvasGroup shade;
 public void Begin(string scene){StartCoroutine(Change(scene));}
 IEnumerator Change(string scene){
  var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;gameObject.AddComponent<GraphicRaycaster>();
  var panel=new GameObject("Fade",typeof(RectTransform));var r=(RectTransform)panel.transform;r.SetParent(transform,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;panel.AddComponent<Image>().color=new Color(.005f,.012f,.038f);shade=panel.AddComponent<CanvasGroup>();shade.alpha=0;
  for(float t=0;t<.16f;t+=Time.unscaledDeltaTime){shade.alpha=t/.16f;yield return null;}shade.alpha=1;
  var op=SceneManager.LoadSceneAsync(scene,LoadSceneMode.Single);yield return op;yield return null;
  for(float t=0;t<.2f;t+=Time.unscaledDeltaTime){shade.alpha=1-t/.2f;yield return null;}
  PulseSceneFlow.TransitionFinished();Destroy(gameObject);
 }
}
