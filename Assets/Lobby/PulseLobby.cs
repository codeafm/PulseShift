using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;

public enum LobbyMode {Campaign,Challenge,Endless}
public partial class PulseLobby:MonoBehaviour {
 public CampaignGame game;public Canvas canvas;public RectTransform safeRoot;public CanvasGroup homeFade;
 public LobbyStage stage;public Transform popupRoot;public Text nameLabel,levelLabel,crystalLabel,resonanceLabel,playLabel,playDetail;public Image progressFill;
 public GameObject giftBadge;public RectTransform logoOrbit;public float entrance;
 [SerializeField,HideInInspector] bool isOpen;
 public bool IsOpen{get=>isOpen;private set=>isOpen=value;}public LobbyMode Mode{get;private set;}
 public PulseProfile Profile{get;private set;}=new PulseProfile();
 public string Page{get;private set;}="";
 public int EndlessCount{get;private set;}public int ModeTarget{get;private set;}
 public bool TestMode{get;private set;}
 public static readonly Color Cyan=new Color(.13f,.8f,1),Purple=new Color(.6f,.32f,1),Gold=new Color(1,.73f,.3f),White=new Color(.89f,.96f,1);
 public static readonly string[] SkinNames={"КЛАССИЧЕСКИЙ","ОГНЕННЫЙ","ГАЛАКТИЧЕСКИЙ","ТЕХНО","ТЕНЬ"};
 public static readonly string[] SkinDescriptions={"Всегда с тобой","Сила внутри","Рождён из звёзд","Будущее рядом","Тишина в движении"};
 public static readonly Color[] SkinColors={new Color(.02f,.8f,1),new Color(1,.32f,.025f),new Color(.55f,.18f,1),new Color(.08f,.75f,1),new Color(.55f,.04f,.9f)};
 public static readonly string[] PortalNames={"СТАНДАРТНЫЙ","СОЛНЕЧНЫЙ","ПУСТОТЫ","ПРИРОДНЫЙ","ЛЕДЯНОЙ"};
 public static readonly string[] PortalDescriptions={"Путь начинается","Свет ведёт","За гранью","Гармония","Холод силы"};
 public static readonly Color[] PortalColors={new Color(.02f,.65f,1),new Color(1,.57f,.04f),new Color(.58f,.12f,1),new Color(.05f,1,.48f),new Color(.32f,.83f,1)};
 bool skinPortalTab;int skinPreviewIndex;RectTransform skinLockVisual;
 AudioSource uiSound,ambience;AudioClip clickClip,ambientClip,bonusClip;string toast="";float toastTime;Text toastLabel;CanvasGroup popupFade;float popupAge;int levelPage,totalStars,completed;InputField nameInput;
 public string Today=>DateTime.UtcNow.ToString("yyyy-MM-dd");
 public int Completed=>completed;public int TotalStars=>totalStars;
 public static PulseLobby Ensure(CampaignGame game){var found=game.lobby?game.lobby:FindObjectsByType<PulseLobby>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(l=>l.gameObject.scene==game.gameObject.scene);if(found){found.game=game;found.gameObject.SetActive(true);found.UpgradeLayout();return found;}var root=new GameObject("MENU • live PULSESHIFT lobby");var lobby=root.AddComponent<PulseLobby>();lobby.game=game;lobby.Build();return lobby;}
 public void Initialize(bool testing){TestMode=testing;Profile=standaloneScene?PulseSceneFlow.Profile:PulseProfile.Load(testing);Mode=LobbyMode.Campaign;UpgradeLayout();RefreshProgress();SetupAudio();stage.ApplySkin(Profile.selectedSkin);stage.ApplyPortalSkin(Profile.selectedPortal);ApplySettings();IsOpen=false;RefreshPresentation();}
 public void RefreshProgress(){completed=totalStars=0;foreach(var l in MenuData.levels){int stars=MenuStars(l.id);totalStars+=stars;if(stars>0)completed++;}}
 public void Open(){if(game&&game.Busy)return;IsOpen=true;if(ambience&&!TestMode&&!ambience.isPlaying)ambience.Play();entrance=0;ClosePopup();UpgradeLayout();RefreshPresentation();RefreshProgress();stage.ApplySkin(Profile.selectedSkin);RefreshHeader();Layout();stage.Frame();}
 public void Hide(){IsOpen=false;if(ambience&&ambience.isPlaying)ambience.Pause();ClosePopup();RefreshPresentation();}
 public void RefreshPresentation(){
  if(!canvas||!stage)return;
  // One owner for both 3D and UI; inactive roots and nested canvases must not leak through.
  if(game)game.SetLobbyVisible(IsOpen);
  if(canvas.gameObject.activeSelf!=IsOpen)canvas.gameObject.SetActive(IsOpen);
  canvas.enabled=IsOpen;stage.SetVisible(IsOpen);
 }
 void LateUpdate(){if(Application.isPlaying)RefreshPresentation();}
 public void Begin(int level,LobbyMode mode){if(standaloneScene){PulseSceneFlow.Play(level,mode);return;}Mode=mode;if(mode==LobbyMode.Endless)EndlessCount=0;ModeTarget=MenuData.levels[level].optimalTurns+2;Hide();game.BeginLobbyLevel(level);}
 public void AdvanceMode(){if(Mode==LobbyMode.Endless){int next=10+(EndlessCount*17+DateTime.UtcNow.DayOfYear)%Mathf.Max(1,MenuData.levels.Length-10);game.BeginLobbyLevel(next);}else Open();}
 public void RecordVictory(int level,int turns,int stars){
  Profile.wins++;Profile.RecordFirstWin(level);
  if(Mode==LobbyMode.Challenge&&turns<=ModeTarget&&Profile.challengeDay!=Today){Profile.challengeDay=Today;Profile.resonance+=2;Profile.crystals+=100;}
  if(Mode==LobbyMode.Endless){EndlessCount++;Profile.bestEndless=Mathf.Max(Profile.bestEndless,EndlessCount);if(EndlessCount%3==0)Profile.resonance++;}
  Save();
 }
 public void Save(){Profile.Save(TestMode);RefreshHeader();ApplySettings();}
 public void Toast(string value){toast=value;toastTime=3.2f;if(toastLabel)toastLabel.text=value;}
 public void Click(LobbyAction action,int value=0){
  if(!IsOpen)return;if(uiSound&&clickClip)uiSound.PlayOneShot(clickClip,Profile.sound*.65f);
  switch(action){
   case LobbyAction.Play:Begin(Mathf.Clamp(MenuUnlocked-1,0,MenuData.levels.Length-1),LobbyMode.Campaign);break;
   case LobbyAction.Home:ClosePopup();break;
   case LobbyAction.Campaign:case LobbyAction.Levels:levelPage=Mathf.Clamp((MenuUnlocked-1)/20,0,4);ShowPage("Уровни");break;
   case LobbyAction.Hero:case LobbyAction.Skins:skinPortalTab=false;skinPreviewIndex=Profile.selectedSkin;ShowPage("Скины");break;
   case LobbyAction.Achievements:ShowPage("Достижения");break;
   case LobbyAction.Settings:ShowPage("Настройки");break;
   case LobbyAction.Profile:ShowPage("Профиль");break;
   case LobbyAction.Crystals:OpenVideoReward(false);break;
   case LobbyAction.Resonance:OpenVideoReward(true);break;
   case LobbyAction.Shop:ShowPage("Магазин");break;
   case LobbyAction.Gift:ShowPage("Подарок");break;
   case LobbyAction.Events:case LobbyAction.Challenge:ShowPage("Испытание дня");break;
   case LobbyAction.Tasks:ShowPage("Задания");break;
   case LobbyAction.Collection:ShowPage("Коллекция");break;
   case LobbyAction.Stats:ShowPage("Статистика");break;
   case LobbyAction.Endless:ShowPage("Бесконечный");break;
   case LobbyAction.Close:ClosePopup();break;
   case LobbyAction.ClaimGift:if(Profile.ClaimGift(Today)){Save();PlayBonus();Toast("+100 кристаллов · +1 резонанс");stage.Celebrate();}ShowPage("Подарок");break;
   case LobbyAction.BuySkin:{bool newlyUnlocked=(Profile.ownedSkins&(1<<value))==0;if(Profile.BuySkin(value)){Save();if(newlyUnlocked)PlayBonus();stage.ApplySkin(Profile.selectedSkin);if(game)game.ApplyPlayerSkin();Toast("Облик выбран: "+SkinNames[value]);if(newlyUnlocked)StartCoroutine(BreakSkinLock());else RefreshSkinScreen();}else{Toast("Недостаточно ресурсов. Забери подарок или выполни задания.");RefreshSkinScreen();}break;}
   case LobbyAction.BuyPortal:{bool newlyUnlocked=(Profile.ownedPortals&(1<<value))==0;if(Profile.BuyPortal(value)){Save();if(newlyUnlocked)PlayBonus();stage.ApplyPortalSkin(Profile.selectedPortal);if(game)game.ApplyPortalSkin();Toast("Портал выбран: "+PortalNames[value]);if(newlyUnlocked)StartCoroutine(BreakSkinLock());else RefreshSkinScreen();}else{Toast("Недостаточно ресурсов для портала");RefreshSkinScreen();}break;}
   case LobbyAction.PreviewSkin:SelectSkinPreview(skinCarousel?skinCarousel.RequestedPortal:skinPortalTab,Mathf.Clamp(value,0,4));break;
   case LobbyAction.SkinHeroTab:SelectSkinPreview(false,Profile.selectedSkin);break;
   case LobbyAction.SkinPortalTab:SelectSkinPreview(true,Profile.selectedPortal);break;
   case LobbyAction.SkinPrevious:CycleSkin(-1);break;
   case LobbyAction.SkinNext:CycleSkin(1);break;
   case LobbyAction.ClaimTask:ClaimMilestone(value,false);break;
   case LobbyAction.ClaimAchievement:ClaimMilestone(value,true);break;
   case LobbyAction.SaveName:var entered=nameInput?nameInput.text.Trim():"";if(entered.Length<2){Toast("Имя должно содержать от 2 до 16 символов");return;}Profile.playerName=entered.Substring(0,Mathf.Min(16,entered.Length));Save();Toast("Имя сохранено");ClosePopup();break;
   case LobbyAction.ToggleMotion:Profile.reducedMotion=!Profile.reducedMotion;Save();ShowPage("Настройки");break;
   case LobbyAction.StartChallenge:Begin(30+DateTime.UtcNow.DayOfYear%Mathf.Min(60,MenuData.levels.Length-30),LobbyMode.Challenge);break;
   case LobbyAction.StartEndless:Begin(10+DateTime.UtcNow.DayOfYear%(MenuData.levels.Length-10),LobbyMode.Endless);break;
   case LobbyAction.SelectLevel:if(value>=0&&value<MenuData.levels.Length&&value<MenuUnlocked)Begin(value,LobbyMode.Campaign);else Toast("Сначала пройди предыдущий уровень");break;
   case LobbyAction.NextPage:levelPage=Mathf.Min((MenuData.levels.Length-1)/20,levelPage+1);ShowPage("Уровни");break;
   case LobbyAction.PreviousPage:levelPage=Mathf.Max(0,levelPage-1);ShowPage("Уровни");break;
   case LobbyAction.Greet:Toast(stage.Greet());break;
   case LobbyAction.WatchReward:BeginVideoReward();break;
   case LobbyAction.ToggleVibration:Profile.vibration=!Profile.vibration;Save();ShowPage("Настройки");break;
   case LobbyAction.ToggleJumpSound:Profile.jumpSound=!Profile.jumpSound;Save();ShowPage("Настройки");break;
   case LobbyAction.ToggleFps:if(MobilePerformance.Lite){Toast("Для плавности на этом устройстве включено 30 FPS");break;}Profile.fps60=!Profile.fps60;Save();ShowPage("Настройки");break;
   case LobbyAction.CycleQuality:Profile.quality=(Profile.quality+1)%3;Save();ShowPage("Настройки");break;
   case LobbyAction.Support:Toast("Поддержка: support@pulseshift.game");break;
   case LobbyAction.Privacy:ShowPage("Политика");break;
  }
 }
 void Update(){if(!Application.isPlaying||!IsOpen)return;Layout();entrance=Mathf.MoveTowards(entrance,1,Time.unscaledDeltaTime*3);homeFade.alpha=Mathf.SmoothStep(0,1,entrance);if(logoOrbit&&!Profile.reducedMotion)logoOrbit.localRotation=Quaternion.Euler(0,0,Time.unscaledTime*9);if(popupFade){popupAge+=Time.unscaledDeltaTime;popupFade.alpha=Mathf.SmoothStep(0,1,popupAge/.18f);popupFade.transform.localScale=Vector3.one*Mathf.Lerp(.97f,1,popupFade.alpha);}toastTime=Mathf.Max(0,toastTime-Time.unscaledDeltaTime);if(toastLabel){toastLabel.gameObject.SetActive(toastTime>0);toastLabel.color=new Color(.78f,.96f,1,Mathf.Clamp01(toastTime*3));}if(Input.GetKeyDown(KeyCode.Escape)){if(Page!="")ClosePopup();else Toast("Нажми «Играть», чтобы продолжить");}}
 public Vector2 Point(float x,float y){var point=safeRoot.TransformPoint(new Vector3(x-270,480-y,0));return RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,point);}
 void RefreshHeader(){if(!nameLabel)return;nameLabel.text=Profile.playerName;levelLabel.text="Уровень "+Mathf.Clamp(MenuUnlocked,1,100);crystalLabel.text=Profile.crystals.ToString("N0");resonanceLabel.text=Profile.resonance.ToString();playLabel.text=playCaption;playDetail.text="УРОВЕНЬ "+Mathf.Clamp(MenuUnlocked,1,100);if(progressFill){float progress=completed>0&&completed%10==0?1:(completed%10)/10f;progressFill.rectTransform.pivot=new Vector2(0,.5f);float width=((RectTransform)progressFill.transform.parent).rect.width;progressFill.rectTransform.anchoredPosition=new Vector2(-width*.5f,0);progressFill.rectTransform.sizeDelta=new Vector2(width*progress,4);}giftBadge.SetActive(Profile.GiftReady(Today));}
 AudioClip skinSwipeClip;
 public void PlaySkinSwipe(){if(uiSound&&skinSwipeClip&&!TestMode&&Profile.sound>0)uiSound.PlayOneShot(skinSwipeClip,Profile.sound);}
 void SetupAudio(){
  if(uiSound)return;uiSound=gameObject.AddComponent<AudioSource>();ambience=gameObject.AddComponent<AudioSource>();ambience.loop=true;ambience.playOnAwake=false;
  skinSwipeClip=Resources.Load<AudioClip>("Audio/SkinSwipe");
  const int rate=22050;float[] click=new float[2205];for(int i=0;i<click.Length;i++){float t=i/(float)rate;click[i]=Mathf.Sin(t*880*2*Mathf.PI)*Mathf.Exp(-t*45)*Mathf.Min(1,t*200)*.2f;}clickClip=AudioClip.Create("Menu • crystal click",click.Length,1,rate,false);clickClip.SetData(click,0);
  ambientClip=Resources.Load<AudioClip>("Audio/MenuMusic");bonusClip=Resources.Load<AudioClip>("Audio/Bonus");ambience.clip=ambientClip;if(!ambientClip)Debug.LogError("Missing menu music: Resources/Audio/MenuMusic");
 }
 public void PlayBonus(){if(uiSound&&bonusClip&&!TestMode)uiSound.PlayOneShot(bonusClip,Profile.sound);}
 void ApplySettings(){MobilePerformance.Apply(Profile,false);if(ambience)ambience.volume=Profile.music;if(game)game.ApplyAudioSettings();if(stage)stage.reducedMotion=Profile.reducedMotion;if(game&&game.gameCamera){var feel=game.gameCamera.GetComponent<PulseCameraFeel>();if(feel)feel.reducedMotion=Profile.reducedMotion;var atmosphere=game.gameCamera.GetComponent<PulseAtmosphere>();if(atmosphere)atmosphere.reducedMotion=Profile.reducedMotion;}if(stage&&stage.view){var mist=stage.view.GetComponent<PulseAtmosphere>();if(mist)mist.reducedMotion=Profile.reducedMotion;}}
 void OnDestroy(){if(clickClip)Destroy(clickClip);}
}
