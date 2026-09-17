using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PulseCampaign;
using System.Linq;

public class CampaignHUD:MonoBehaviour {
 public CampaignGame game;
 public RectTransform safeRoot;
 public Text levelText,chapterText,gemText,pulseText,turnText,messageText,dialogTitle,dialogBody,primaryText,secondaryText,pageText;
 public GameObject dialog,levelPicker,starsRoot,freezeRoot,dashRoot;
 public CanvasGroup dialogFade,pickerFade;
 public NeonGraphic[] stars,gemIcons,clockDots;
 public NeonActionButton[] levelButtons;
 public NeonActionButton primary,secondary;
 Button victoryRewardButton;Text victoryRewardLabel;
 void RefreshRewardOffer(){
  if(!victoryRewardButton){
   var r=Rect(dialogPanel.transform,"Victory • optional ad bonus",new Vector2(378,43),new Vector2(0,-101));
   var plate=r.gameObject.AddComponent<LobbyPlate>();plate.clean=true;plate.accent=new Color(1,.72f,.2f);plate.raycastTarget=true;
   victoryRewardButton=r.gameObject.AddComponent<Button>();victoryRewardButton.targetGraphic=plate;
   victoryRewardLabel=Label(r,"Bonus","",13,new Vector2(356,39),Vector2.zero);victoryRewardLabel.fontStyle=FontStyle.Bold;
   victoryRewardButton.onClick.AddListener(()=>game.WatchVictoryReward());
  }
  bool offer=game.VictoryRewardAvailable&&!game.MenuOpen&&!game.Paused;
  victoryRewardButton.gameObject.SetActive(offer);
  if(offer){var ads=PulseAds.Instance;victoryRewardButton.interactable=ads&&!PulseAds.Fullscreen;victoryRewardLabel.text=ads&&ads.RewardReady?"▶ РЕКЛАМА · +10 КРИСТАЛЛОВ +1 ИМПУЛЬС":ads?ads.Status:"Реклама недоступна";
   dialogBody.rectTransform.sizeDelta=new Vector2(410,65);dialogBody.rectTransform.anchoredPosition=new Vector2(0,-38);dialogBody.text=$"УРОВЕНЬ {game.CurrentLevel.id} ПРОЙДЕН\nСвет снова наполняет этот мир!";
  }
 }
 NeonActionButton pauseHome;NeonGraphic dialogPanel;RawImage victoryArt,defeatArt;VictoryCelebrationFX victoryFx;DeathVignetteFX deathVignette;GameObject victoryNextIcon,victoryListIcon;
 NeonActionButton[] allButtons;float oldGems=-1,oldPulses=-1,counterPulse;Font font;
 public static CampaignHUD Ensure(CampaignGame game){var existing=game.gameplayHUD?game.gameplayHUD:FindObjectsByType<CampaignHUD>(FindObjectsInactive.Include,FindObjectsSortMode.InstanceID).FirstOrDefault(h=>h.gameObject.scene==game.gameObject.scene);if(existing){game.gameplayHUD=existing;existing.game=game;existing.EnsureHome();existing.EnsureVictoryPresentation();game.RefreshPresentationHuds();return existing;}var root=new GameObject("UI • animated PULSESHIFT canvas",typeof(RectTransform));var canvas=root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=10;var scaler=root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(540,960);scaler.matchWidthOrHeight=0;root.AddComponent<GraphicRaycaster>();var hud=root.AddComponent<CampaignHUD>();game.gameplayHUD=hud;hud.game=game;hud.Build();game.RefreshPresentationHuds();return hud;}
 RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 position,Vector2? anchor=null){var g=new GameObject(name,typeof(RectTransform));var r=g.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor??new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=position;return r;}
 Text Label(Transform parent,string name,string value,int size,Vector2 dimensions,Vector2 pos,Vector2? anchor=null){var r=Rect(parent,name,dimensions,pos,anchor);var t=r.gameObject.AddComponent<Text>();t.font=font?font:Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.text=value;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.78f,.9f,1);t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.supportRichText=false;return t;}
 NeonGraphic Graphic(Transform parent,string name,NeonSymbol symbol,Vector2 size,Vector2 pos,Color? color=null){var r=Rect(parent,name,size,pos);var g=r.gameObject.AddComponent<NeonGraphic>();g.symbol=symbol;if(color.HasValue)g.accent=color.Value;g.raycastTarget=false;return g;}
 NeonActionButton ButtonAt(Transform parent,string name,CampaignUICommand command,NeonSymbol? icon,Vector2 size,Vector2 pos,Vector2? anchor=null,string label=null){
  var r=Rect(parent,name,size,pos,anchor);var f=r.gameObject.AddComponent<NeonGraphic>();f.symbol=command==CampaignUICommand.Pulse?NeonSymbol.Disc:NeonSymbol.Panel;f.raycastTarget=true;
  var b=r.gameObject.AddComponent<Button>();b.transition=Selectable.Transition.None;b.targetGraphic=f;var a=r.gameObject.AddComponent<NeonActionButton>();a.game=game;a.command=command;
  if(icon.HasValue){var g=Graphic(r,"Icon",icon.Value,size*.53f,new Vector2(0,label==null?0:5));if(icon==NeonSymbol.Pulse)g.spin=true;}
  if(label!=null)Label(r,"Label",label,icon.HasValue?10:16,new Vector2(size.x-10,icon.HasValue?18:size.y-8),new Vector2(0,icon.HasValue?-size.y*.29f:0));return a;
 }
 GameObject Overlay(string name,out CanvasGroup fade){var r=Rect(safeRoot,name,Vector2.zero,Vector2.zero);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.sizeDelta=Vector2.zero;var shade=r.gameObject.AddComponent<Image>();shade.color=new Color(.003f,.009f,.027f,.9f);fade=r.gameObject.AddComponent<CanvasGroup>();fade.alpha=0;fade.blocksRaycasts=false;return r.gameObject;}
 void Build(){
  font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");safeRoot=Rect(transform,"Safe area",Vector2.zero,Vector2.zero);safeRoot.anchorMin=Vector2.zero;safeRoot.anchorMax=Vector2.one;safeRoot.sizeDelta=Vector2.zero;
  var top=new Vector2(.5f,1);var bottom=new Vector2(.5f,0);
  ButtonAt(safeRoot,"Pause",CampaignUICommand.Pause,NeonSymbol.Pause,new Vector2(52,52),new Vector2(-225,-39),top);
  ButtonAt(safeRoot,"Levels",CampaignUICommand.Levels,NeonSymbol.Grid,new Vector2(52,52),new Vector2(225,-39),top);
  levelText=Label(safeRoot,"Level title","LEVEL 01",23,new Vector2(300,32),new Vector2(0,-28),top);levelText.fontStyle=FontStyle.Bold;
  chapterText=Label(safeRoot,"Chapter","Первые импульсы",12,new Vector2(330,23),new Vector2(0,-56),top);
  var counters=Rect(safeRoot,"Counters",new Vector2(400,50),new Vector2(0,-95),top);
  Graphic(counters,"Gem glass",NeonSymbol.Panel,new Vector2(134,46),new Vector2(-105,0),new Color(.18f,.45f,.7f));
  gemIcons=new NeonGraphic[3];for(int i=0;i<3;i++)gemIcons[i]=Graphic(counters,"Memory "+i,NeonSymbol.Crystal,new Vector2(17,27),new Vector2(-151+i*19,0));
  gemText=Label(counters,"Gem counter","0/3",19,new Vector2(55,30),new Vector2(-72,0));
  Graphic(counters,"Pulse glass",NeonSymbol.Panel,new Vector2(126,46),new Vector2(111,0));Graphic(counters,"Pulse counter icon",NeonSymbol.Pulse,new Vector2(29,29),new Vector2(78,0)).spin=true;
  pulseText=Label(counters,"Pulse counter","3",23,new Vector2(52,30),new Vector2(121,0));
  clockDots=new NeonGraphic[4];for(int i=0;i<4;i++){var r=Rect(safeRoot,"Rhythm "+i,new Vector2(12,12),new Vector2(-27+i*18,-133),top);clockDots[i]=r.gameObject.AddComponent<NeonGraphic>();clockDots[i].symbol=NeonSymbol.Disc;clockDots[i].raycastTarget=false;}
  turnText=Label(safeRoot,"Turn and phase","Голубая фаза · ход 0",11,new Vector2(270,22),new Vector2(0,-151),top);
  ButtonAt(safeRoot,"Pulse",CampaignUICommand.Pulse,NeonSymbol.Pulse,new Vector2(124,124),new Vector2(0,105),bottom,"ИМПУЛЬС");
  ButtonAt(safeRoot,"Wait",CampaignUICommand.Wait,NeonSymbol.Wait,new Vector2(86,94),new Vector2(-116,105),bottom,"ЖДАТЬ");
  ButtonAt(safeRoot,"Undo",CampaignUICommand.Undo,NeonSymbol.Undo,new Vector2(86,94),new Vector2(116,105),bottom,"ОТМЕНА");
  freezeRoot=ButtonAt(safeRoot,"Freeze",CampaignUICommand.Freeze,NeonSymbol.Freeze,new Vector2(77,69),new Vector2(-184,189),bottom,"СТОП · 1").gameObject;
  dashRoot=ButtonAt(safeRoot,"Dash",CampaignUICommand.Dash,NeonSymbol.Chevron,new Vector2(77,69),new Vector2(184,189),bottom,"РЫВОК · 1").gameObject;
  ButtonAt(safeRoot,"Hint",CampaignUICommand.Hint,null,new Vector2(134,34),new Vector2(0,201),bottom,"ПОДСКАЗКА");
  messageText=Label(safeRoot,"Status message","Выбери соседний остров",13,new Vector2(468,41),new Vector2(0,30),bottom);
  dialog=Overlay("Dialog overlay",out dialogFade);
  var victoryArtRect=Rect(dialog.transform,"Victory • illustrated frame",new Vector2(520,930),new Vector2(0,-4));victoryArt=victoryArtRect.gameObject.AddComponent<RawImage>();victoryArt.texture=Resources.Load<Texture2D>("Campaign/VictoryDialogPanel");victoryArt.raycastTarget=false;victoryArt.gameObject.SetActive(false);
  var defeatArtRect=Rect(dialog.transform,"Defeat • illustrated frame",new Vector2(520,930),new Vector2(0,-4));defeatArt=defeatArtRect.gameObject.AddComponent<RawImage>();defeatArt.texture=Resources.Load<Texture2D>("Campaign/DefeatDialogPanel");defeatArt.raycastTarget=false;defeatArt.gameObject.SetActive(false);
  victoryFx=Rect(dialog.transform,"Victory • fireworks and sparks",new Vector2(540,960),Vector2.zero).gameObject.AddComponent<VictoryCelebrationFX>();victoryFx.raycastTarget=false;victoryFx.gameObject.SetActive(false);
  var panel=Graphic(dialog.transform,"Glass dialog",NeonSymbol.Panel,new Vector2(440,376),Vector2.zero);dialogPanel=panel;panel.raycastTarget=true;
  dialogTitle=Label(panel.transform,"Title","ГОТОВ К ИГРЕ?",24,new Vector2(380,58),new Vector2(0,126));dialogTitle.fontStyle=FontStyle.Bold;
  dialogBody=Label(panel.transform,"Body","",17,new Vector2(375,124),new Vector2(0,19));
  starsRoot=Rect(panel.transform,"Victory stars",new Vector2(205,53),new Vector2(0,65)).gameObject;stars=new NeonGraphic[3];for(int i=0;i<3;i++)stars[i]=Graphic(starsRoot.transform,"Star "+i,NeonSymbol.Star,new Vector2(44,44),new Vector2((i-1)*62,0),new Color(1,.6f,.055f));
  primary=ButtonAt(panel.transform,"Primary",CampaignUICommand.Primary,null,new Vector2(340,51),new Vector2(0,-81),label:"ПРОДОЛЖИТЬ");primaryText=primary.GetComponentInChildren<Text>();
  victoryListIcon=Graphic(primary.transform,"Victory list icon",NeonSymbol.Grid,new Vector2(29,29),new Vector2(-142,0),new Color(.03f,.9f,1)).gameObject;victoryNextIcon=Graphic(primary.transform,"Victory next icon",NeonSymbol.Next,new Vector2(29,29),new Vector2(142,0),new Color(.03f,.9f,1)).gameObject;victoryListIcon.SetActive(false);victoryNextIcon.SetActive(false);
  secondary=ButtonAt(panel.transform,"Secondary",CampaignUICommand.Restart,null,new Vector2(340,43),new Vector2(0,-140),label:"НАЧАТЬ ЗАНОВО");secondaryText=secondary.GetComponentInChildren<Text>();
  ButtonAt(panel.transform,"Dialog level select",CampaignUICommand.Levels,NeonSymbol.Grid,new Vector2(35,35),new Vector2(186,151));
  levelPicker=Overlay("Level selection overlay",out pickerFade);var gridPanel=Graphic(levelPicker.transform,"Archipelago glass",NeonSymbol.Panel,new Vector2(444,590),Vector2.zero);gridPanel.raycastTarget=true;
  Label(gridPanel.transform,"Heading","АРХИПЕЛАГ",25,new Vector2(300,40),new Vector2(0,245));Label(gridPanel.transform,"Subtitle","Сохрани свет каждого острова",13,new Vector2(350,24),new Vector2(0,211));
  levelButtons=new NeonActionButton[20];for(int i=0;i<20;i++)levelButtons[i]=ButtonAt(gridPanel.transform,"Level slot "+i,CampaignUICommand.SelectLevel,null,new Vector2(81,62),new Vector2((i%4-1.5f)*96,146-i/4*72),label:(i+1).ToString());
  var previous=ButtonAt(gridPanel.transform,"Previous page",CampaignUICommand.PreviousPage,NeonSymbol.Next,new Vector2(49,43),new Vector2(-154,-230));previous.transform.Find("Icon").localRotation=Quaternion.Euler(0,0,180);
  ButtonAt(gridPanel.transform,"Next page",CampaignUICommand.NextPage,NeonSymbol.Next,new Vector2(49,43),new Vector2(154,-230));
  ButtonAt(gridPanel.transform,"Close",CampaignUICommand.CloseLevels,null,new Vector2(191,43),new Vector2(0,-230),label:"ВЕРНУТЬСЯ");pageText=Label(gridPanel.transform,"Page","1 / 5",12,new Vector2(120,22),new Vector2(0,-272));
  var deathRect=Rect(transform,"Death • full-screen red edge vignette",Vector2.zero,Vector2.zero);deathRect.anchorMin=Vector2.zero;deathRect.anchorMax=Vector2.one;deathRect.offsetMin=deathRect.offsetMax=Vector2.zero;deathVignette=deathRect.gameObject.AddComponent<DeathVignetteFX>();deathVignette.raycastTarget=false;deathVignette.intensity=0;deathVignette.transform.SetAsLastSibling();
  if(!FindFirstObjectByType<EventSystem>()){var es=new GameObject("EventSystem • UI input");es.AddComponent<EventSystem>();es.AddComponent<StandaloneInputModule>();}
  EnsureHome();Cache();Refresh();
 }
 void EnsureHome(){
  if(!safeRoot||!dialog)return;
  // Migrate the saved scene as well as fresh HUDs. Gameplay has a single navigation entry: Pause.
  foreach(string name in new[]{"Home","Levels"}){var old=safeRoot.Find(name);if(old){old.gameObject.SetActive(false);if(Application.isPlaying)Destroy(old.gameObject);else DestroyImmediate(old.gameObject);}}
  var panel=dialog.transform.Find("Glass dialog");var home=panel.Find("Pause • main menu");
  pauseHome=home?home.GetComponent<NeonActionButton>():ButtonAt(panel,"Pause • main menu",CampaignUICommand.Home,null,new Vector2(340,45),new Vector2(0,-188),label:"В ГЛАВНОЕ МЕНЮ");
  pauseHome.gameObject.SetActive(false);Cache();
  foreach(string name in new[]{"Wait","Undo"}){var control=safeRoot.Find(name) as RectTransform;if(control){control.sizeDelta=new Vector2(86,94);var label=control.GetComponentInChildren<Text>();label.rectTransform.anchoredPosition=new Vector2(0,-24);label.fontSize=11;}}
  foreach(string name in new[]{"Pause","Pulse","Wait","Undo","Hint"}){var control=safeRoot.Find(name) as RectTransform;if(control)PulseControlHalo.Attach(control,name=="Pulse");}
  var pulse=safeRoot.Find("Pulse") as RectTransform;if(pulse){pulse.sizeDelta=new Vector2(144,144);var icon=pulse.Find("Icon") as RectTransform;icon.sizeDelta=new Vector2(88,88);icon.anchoredPosition=new Vector2(0,12);var label=pulse.GetComponentInChildren<Text>();label.fontSize=13;label.rectTransform.anchoredPosition=new Vector2(0,-41);}
  foreach(string name in new[]{"Wait","Undo"}){var control=safeRoot.Find(name) as RectTransform;if(control){control.anchoredPosition=new Vector2(name=="Wait"?-132:132,105);control.sizeDelta=new Vector2(94,100);}}
 }
 void EnsureVictoryPresentation(){
  if(!dialog)return;dialogPanel=dialog.transform.Find("Glass dialog")?.GetComponent<NeonGraphic>();
  var art=dialog.transform.Find("Victory • illustrated frame") as RectTransform;if(!art){art=Rect(dialog.transform,"Victory • illustrated frame",new Vector2(520,930),new Vector2(0,-4));victoryArt=art.gameObject.AddComponent<RawImage>();victoryArt.raycastTarget=false;art.SetSiblingIndex(0);}else victoryArt=art.GetComponent<RawImage>();if(victoryArt){victoryArt.texture=Resources.Load<Texture2D>("Campaign/VictoryDialogPanel");victoryArt.gameObject.SetActive(false);}
  var defeat=dialog.transform.Find("Defeat • illustrated frame") as RectTransform;if(!defeat){defeat=Rect(dialog.transform,"Defeat • illustrated frame",new Vector2(520,930),new Vector2(0,-4));defeatArt=defeat.gameObject.AddComponent<RawImage>();defeatArt.raycastTarget=false;defeat.SetSiblingIndex(Mathf.Min(1,dialog.transform.childCount-1));}else defeatArt=defeat.GetComponent<RawImage>();if(defeatArt){defeatArt.texture=Resources.Load<Texture2D>("Campaign/DefeatDialogPanel");defeatArt.gameObject.SetActive(false);}
  var fx=dialog.transform.Find("Victory • fireworks and sparks") as RectTransform;if(!fx){fx=Rect(dialog.transform,"Victory • fireworks and sparks",new Vector2(540,960),Vector2.zero);victoryFx=fx.gameObject.AddComponent<VictoryCelebrationFX>();victoryFx.raycastTarget=false;fx.SetSiblingIndex(Mathf.Min(2,dialog.transform.childCount-1));}else victoryFx=fx.GetComponent<VictoryCelebrationFX>();if(victoryFx)victoryFx.gameObject.SetActive(false);
  var vignette=transform.Find("Death • full-screen red edge vignette") as RectTransform;var legacy=safeRoot? safeRoot.Find("Death • red edge vignette") as RectTransform:null;if(!vignette&&legacy){vignette=legacy;vignette.name="Death • full-screen red edge vignette";vignette.SetParent(transform,false);}if(!vignette){vignette=Rect(transform,"Death • full-screen red edge vignette",Vector2.zero,Vector2.zero);deathVignette=vignette.gameObject.AddComponent<DeathVignetteFX>();deathVignette.raycastTarget=false;}else deathVignette=vignette.GetComponent<DeathVignetteFX>();if(deathVignette){vignette.anchorMin=Vector2.zero;vignette.anchorMax=Vector2.one;vignette.offsetMin=vignette.offsetMax=Vector2.zero;deathVignette.intensity=0;deathVignette.transform.SetAsLastSibling();}
  if(primary){var list=primary.transform.Find("Victory list icon");victoryListIcon=list?list.gameObject:Graphic(primary.transform,"Victory list icon",NeonSymbol.Grid,new Vector2(29,29),new Vector2(-142,0),new Color(.03f,.9f,1)).gameObject;var next=primary.transform.Find("Victory next icon");victoryNextIcon=next?next.gameObject:Graphic(primary.transform,"Victory next icon",NeonSymbol.Next,new Vector2(29,29),new Vector2(142,0),new Color(.03f,.9f,1)).gameObject;victoryListIcon.SetActive(false);victoryNextIcon.SetActive(false);}
 }
 void Awake(){Cache();EnsureVictoryPresentation();if(dialogBody){dialogBody.resizeTextForBestFit=true;dialogBody.resizeTextMinSize=12;dialogBody.resizeTextMaxSize=17;}}
 void Cache(){allButtons=GetComponentsInChildren<NeonActionButton>(true).Where(b=>b.name!="Home"&&b.name!="Levels").ToArray();}
 public Button CommandButton(CampaignUICommand command)=>allButtons.First(b=>b&&b.command==command&&b.gameObject.activeInHierarchy).GetComponent<Button>();
 void Update(){if(!game)return;MobileViewport.Layout(safeRoot,GetComponent<Canvas>());Refresh();}
 void Fade(CanvasGroup group,bool visible){float target=visible?1:0;group.alpha=Mathf.MoveTowards(group.alpha,target,Time.unscaledDeltaTime*7);group.blocksRaycasts=group.interactable=visible;float scale=Mathf.Lerp(.94f,1,group.alpha);foreach(RectTransform child in group.transform)child.localScale=Vector3.one*scale;}
 public void Refresh(){
  if(!game||!levelText)return;var l=game.CurrentLevel;var s=game.CurrentState;
  levelText.text=$"LEVEL {l.id:00}  /  {game.Data.levels.Length}";chapterText.text=(l.layout??CampaignGenerator.Chapters[l.chapter]).ToUpperInvariant()+"   ·   ГЛАВА "+(l.chapter+1);gemText.text=game.GemCount+"/3";pulseText.text=(l.pulseBudget-s.pulses).ToString();turnText.text=$"{(s.phase==0?"ГОЛУБАЯ":"ЯНТАРНАЯ")} ФАЗА   ·   ХОД {game.Turns}";messageText.text=game.Message;
  if(oldGems!=game.GemCount||oldPulses!=l.pulseBudget-s.pulses){counterPulse=1;oldGems=game.GemCount;oldPulses=l.pulseBudget-s.pulses;}counterPulse=Mathf.Max(0,counterPulse-Time.unscaledDeltaTime*3);gemText.transform.localScale=pulseText.transform.localScale=Vector3.one*(1+.13f*Mathf.Sin(counterPulse*Mathf.PI));
  for(int i=0;i<3;i++)gemIcons[i].color=(s.gems&(1<<i))!=0?Color.white:new Color(.25f,.29f,.35f,.7f);
  for(int i=0;i<4;i++)clockDots[i].Tint(i==s.tick?new Color(.03f,.8f,1):new Color(.1f,.23f,.37f));
  freezeRoot.SetActive(l.freezeCharges>0);dashRoot.SetActive(l.dashCharges>0);freezeRoot.GetComponentInChildren<Text>().text="СТОП · "+s.freezes;dashRoot.GetComponentInChildren<Text>().text=game.DashSelected?"ВЫБЕРИ ЦЕЛЬ":"РЫВОК · "+s.dashes;
  if(allButtons==null)Cache();foreach(var a in allButtons){bool active=true;switch(a.command){case CampaignUICommand.Pulse:active=game.CanPlay&&s.pulses<l.pulseBudget;break;case CampaignUICommand.Wait:case CampaignUICommand.Hint:active=game.CanPlay;break;case CampaignUICommand.Freeze:active=game.CanPlay&&s.freezes>0;break;case CampaignUICommand.Dash:active=game.CanPlay&&s.dashes>0;break;case CampaignUICommand.Undo:active=game.CanUndo&&!game.MenuOpen&&!game.LessonOpen;break;case CampaignUICommand.Pause:active=!game.MenuOpen&&!game.LessonOpen;break;case CampaignUICommand.Home:active=!game.Busy||game.Paused;break;case CampaignUICommand.Levels:active=!game.Busy;break;}a.GetComponent<Button>().interactable=active;}
  bool modal=game.Paused||game.LessonOpen||game.ResultVisible&&(s.dead||game.Victorious);Fade(dialogFade,modal&&!game.MenuOpen&&!game.Paused);Fade(pickerFade,game.MenuOpen);
  CampaignPausePanel.Ensure(this).Refresh();
  if(!pauseHome)pauseHome=dialog.GetComponentsInChildren<NeonActionButton>(true).FirstOrDefault(b=>b.command==CampaignUICommand.Home);
  if(pauseHome)pauseHome.gameObject.SetActive(game.Paused&&!game.LessonOpen);
  bool victory=game.Victorious&&!game.LessonOpen,defeat=s.dead&&!game.LessonOpen,resultArt=victory||defeat;
  if(victoryArt)victoryArt.gameObject.SetActive(victory);if(defeatArt)defeatArt.gameObject.SetActive(defeat);if(victoryFx)victoryFx.gameObject.SetActive(victory);if(deathVignette)deathVignette.intensity=Mathf.MoveTowards(deathVignette.intensity,defeat?1:0,Time.unscaledDeltaTime*(defeat?4.5f:2.5f));if(dialogPanel)dialogPanel.color=resultArt?Color.clear:Color.white;
  // Invisible result windows need no text/layout/mesh work during ordinary play.
  // Keep transitions, input state and the death vignette updated above.
  if(!modal&&!game.MenuOpen&&dialogFade.alpha<=0&&pickerFade.alpha<=0){if(victoryRewardButton)victoryRewardButton.gameObject.SetActive(false);return;}
  var panelRect=(RectTransform)dialogPanel.transform;panelRect.sizeDelta=new Vector2(resultArt?500:440,game.Paused?480:resultArt?720:376);
  dialogTitle.fontSize=resultArt?34:24;dialogTitle.rectTransform.sizeDelta=resultArt?new Vector2(440,64):new Vector2(380,58);dialogTitle.rectTransform.anchoredPosition=resultArt?new Vector2(0,125):new Vector2(0,126);
  starsRoot.SetActive(victory);var starsRect=(RectTransform)starsRoot.transform;starsRect.anchoredPosition=victory?new Vector2(0,38):new Vector2(0,65);starsRect.sizeDelta=victory?new Vector2(280,76):new Vector2(205,53);
  for(int i=0;i<3;i++){stars[i].Tint(i<game.EarnedStars?new Color(1,.62f,.04f):new Color(.1f,.16f,.23f));stars[i].rectTransform.sizeDelta=victory?new Vector2(68,68):new Vector2(44,44);stars[i].rectTransform.anchoredPosition=victory?new Vector2((i-1)*88,0):new Vector2((i-1)*62,0);}
  dialogBody.rectTransform.sizeDelta=resultArt?new Vector2(410,112):new Vector2(375,124);dialogBody.rectTransform.anchoredPosition=resultArt?new Vector2(0,-45):new Vector2(0,game.Victorious?-1:19);dialogBody.fontSize=17;
  var primaryRect=(RectTransform)primary.transform;var secondaryRect=(RectTransform)secondary.transform;primaryRect.sizeDelta=resultArt?new Vector2(382,64):new Vector2(340,51);primaryRect.anchoredPosition=resultArt?new Vector2(0,-151):new Vector2(0,-81);secondaryRect.sizeDelta=resultArt?new Vector2(382,54):new Vector2(340,43);secondaryRect.anchoredPosition=resultArt?new Vector2(0,-224):new Vector2(0,-140);
  primary.GetComponent<NeonGraphic>().color=resultArt?Color.clear:Color.white;secondary.GetComponent<NeonGraphic>().color=resultArt?Color.clear:Color.white;
  if(victoryListIcon)victoryListIcon.SetActive(victory);if(victoryNextIcon)victoryNextIcon.SetActive(victory);
  dialogTitle.text=game.LessonOpen?l.title:game.Victorious?(l.id==game.Data.levels.Length?"КАМПАНИЯ ПРОЙДЕНА":"ОСТРОВ СПАСЁН"):s.dead?"ИСКРА ПОГАСЛА":"ПАУЗА";
  dialogBody.text=game.LessonOpen?l.lesson.Replace("Призрачный остров появляется на два такта и исчезает на два.","Печать острова открыта два такта и закрыта два. Крест означает опасность.")+"\n\nНажми на соседний остров. Мир движется только после твоего действия.":game.Victorious?$"УРОВЕНЬ {l.id} ПРОЙДЕН  ·  {game.Turns} ХОДОВ\n\nТЫ КОМПЛЕКТИРОВАЛ ОСТРОВ!\nСвет снова наполняет этот мир!":s.dead?game.Message+"\n\nСвет погас, но путь ещё можно продолжить.\nОтмени последний ход или начни заново.":"Мир замер. Продолжи путешествие\nили вернись в главное меню.\n\nПройденные уровни сохраняются.\nТекущий остров начнётся заново.";
  primaryText.text=game.LessonOpen?"НАЧАТЬ ПУТЕШЕСТВИЕ":game.Victorious?(l.id<game.Data.levels.Length?"СЛЕДУЮЩИЙ ОСТРОВ":"ВЫБОР УРОВНЯ"):s.dead?"ОТМЕНИТЬ ХОД":"ПРОДОЛЖИТЬ";
  if(game.Victorious&&game.ActiveMode!=LobbyMode.Campaign){primaryText.text=game.ActiveMode==LobbyMode.Endless?"СЛЕДУЮЩАЯ КАРТА":"В ГЛАВНОЕ МЕНЮ";dialogBody.text=game.ActiveMode==LobbyMode.Endless?"СЕРИЯ: "+game.ActiveEndlessCount+" ОСТРОВОВ":"ЦЕЛЬ: "+game.ActiveModeTarget+" ХОДОВ · ТВОЙ РЕЗУЛЬТАТ: "+game.Turns;}
  primary.GetComponent<Button>().interactable=(game.Paused||!game.Busy)&&(!s.dead||game.CanUndo);secondary.GetComponent<Button>().interactable=!game.Busy;secondary.gameObject.SetActive(!game.LessonOpen);secondary.command=game.Victorious?CampaignUICommand.Home:CampaignUICommand.Restart;secondaryText.text=game.Victorious?"ГЛАВНОЕ МЕНЮ":"НАЧАТЬ ЗАНОВО";
  RefreshRewardOffer();
  if(game.MenuOpen){int page=game.MenuPage;pageText.text=$"{page+1} / {(game.Data.levels.Length+19)/20}";for(int i=0;i<20;i++){int n=page*20+i;var b=levelButtons[i];b.gameObject.SetActive(n<game.Data.levels.Length);if(n>=game.Data.levels.Length)continue;b.value=n;b.GetComponent<Button>().interactable=n<game.Unlocked;b.GetComponentInChildren<Text>().text=$"{n+1:00}"+(game.SavedStars(n+1)>0?"  ★":"");}}
 }
}
