using UnityEngine;
using UnityEngine.UI;

public partial class PulseLobby {
 RectTransform Rect(Transform parent,string name,float w,float h,float x,float y){var root=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(parent,false);root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.sizeDelta=new Vector2(w,h);root.anchoredPosition=new Vector2(x,y);return root;}
 RectTransform At(Transform parent,string name,float w,float h,float x,float y)=>Rect(parent,name,w,h,x-270,480-y);
 Text Text(Transform parent,string name,string text,int size,float w,float h,float x,float y,Color? tint=null,bool bold=false){var r=Rect(parent,name,w,h,x,y);var t=r.gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=text;t.fontSize=size;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;t.alignment=TextAnchor.MiddleCenter;t.color=tint??White;t.raycastTarget=false;t.supportRichText=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
 Text Label(Transform parent,string text,int size,float x,float y,float w=420,float h=36,Color? tint=null,bool bold=false)=>Text(parent,text,text,size,w,h,x-270,480-y,tint,bold);
 LobbyGlyph Icon(Transform parent,LobbyIcon icon,float size,float x,float y,Color? tint=null){var r=Rect(parent,icon+" icon",size,size,x,y);var g=r.gameObject.AddComponent<LobbyGlyph>();g.icon=icon;g.color=tint??White;g.raycastTarget=false;return g;}
 NeonGraphic Neon(Transform parent,NeonSymbol icon,float size,float x,float y,Color? tint=null){var r=Rect(parent,icon+" icon",size,size,x,y);var g=r.gameObject.AddComponent<NeonGraphic>();g.symbol=icon;g.accent=tint??Cyan;g.raycastTarget=false;return g;}
 LobbyButton Button(Transform parent,string name,LobbyAction action,float w,float h,float x,float y,string label=null,int size=17,Color? accent=null,int value=0,bool primary=false){
  var r=At(parent,name,w,h,x,y);var plate=r.gameObject.AddComponent<LobbyPlate>();plate.accent=accent??Cyan;plate.primary=primary;plate.raycastTarget=true;if(primary){plate.top=new Color(.02f,.22f,.52f,.97f);plate.bottom=new Color(.005f,.04f,.17f,.98f);}
  var button=r.gameObject.AddComponent<Button>();button.targetGraphic=plate;button.transition=Selectable.Transition.None;var b=r.gameObject.AddComponent<LobbyButton>();b.lobby=this;b.action=action;b.value=value;b.primary=primary;if(label!=null)Text(r,"Label",label,size,w-16,h-10,0,0,null,true);return b;
 }
 LobbyButton Side(string name,LobbyAction action,LobbyIcon icon,float x,float y,Color? color=null){var b=Button(safeRoot,name,action,74,74,x,y);Icon(b.transform,icon,32,0,12,color);Text(b.transform,"Label",name,12,72,24,0,-22,null,true);return b;}
 public void Build(){
  var ui=new GameObject("Lobby Canvas • editable UI",typeof(RectTransform));ui.transform.SetParent(transform,false);canvas=ui.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=30;var scaler=ui.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(540,960);scaler.matchWidthOrHeight=0;ui.AddComponent<GraphicRaycaster>();
  safeRoot=Rect(ui.transform,"Safe portrait layout • 540 x 960",540,960,0,0);homeFade=safeRoot.gameObject.AddComponent<CanvasGroup>();var sparkles=Rect(safeRoot,"Living stardust",540,960,0,0).gameObject.AddComponent<LobbySparkles>();sparkles.lobby=this;sparkles.raycastTarget=false;
  var settings=Button(safeRoot,"Settings",LobbyAction.Settings,49,49,34,39);Icon(settings.transform,LobbyIcon.Gear,30,0,0);
  var profile=Button(safeRoot,"Profile",LobbyAction.Profile,197,56,164,39);Icon(profile.transform,LobbyIcon.Hero,44,-70,0,Cyan);nameLabel=Text(profile.transform,"Player name","Искра",16,127,23,22,12,null,true);levelLabel=Text(profile.transform,"Profile level","Уровень 1",11,127,20,22,-8,new Color(.43f,.74f,1));
  var track=Rect(profile.transform,"Progress track",119,4,22,-21).gameObject.AddComponent<Image>();track.color=new Color(.08f,.13f,.29f);track.raycastTarget=false;progressFill=Rect(track.transform,"Chapter progress",119,4,0,0).gameObject.AddComponent<Image>();progressFill.color=Cyan;progressFill.type=Image.Type.Simple;progressFill.raycastTarget=false;
  var shards=Button(safeRoot,"Crystal wallet",LobbyAction.Crystals,132,49,337,39);Neon(shards.transform,NeonSymbol.Crystal,27,-44,0,Gold);crystalLabel=Text(shards.transform,"Balance","0",16,58,28,-4,0,null,true);var plus=Button(safeRoot,"Add crystals",LobbyAction.Crystals,38,40,382,39);Icon(plus.transform,LobbyIcon.Plus,21,0,0);
  var resonance=Button(safeRoot,"Resonance wallet",LobbyAction.Resonance,118,49,468,39);Neon(resonance.transform,NeonSymbol.Pulse,27,-38,0);resonanceLabel=Text(resonance.transform,"Balance","0",16,37,28,-1,0,null,true);var plusR=Button(safeRoot,"Add resonance",LobbyAction.Resonance,36,40,508,39);Icon(plusR.transform,LobbyIcon.Plus,21,0,0);
  var gift=Button(safeRoot,"Gift",LobbyAction.Gift,48,48,502,104,null,17,Purple);Icon(gift.transform,LobbyIcon.Gift,30,0,0);giftBadge=Rect(gift.transform,"Gift notification",13,13,19,20).gameObject;var badge=giftBadge.AddComponent<Image>();badge.color=new Color(1,.16f,.32f);badge.raycastTarget=false;
  logoOrbit=At(safeRoot,"Orbit • animated logo",176,176,270,146);var orbit=Neon(logoOrbit,NeonSymbol.Disc,176,0,0,new Color(.02f,.5f,1,.55f));orbit.filled=false;Neon(logoOrbit,NeonSymbol.Disc,12,69,39,Cyan);Neon(logoOrbit,NeonSymbol.Disc,9,-69,-39,Cyan);
  Label(safeRoot,"P U L S E S H I F T",30,270,134,402,50,White,true);Label(safeRoot,"M O V E   ·   T H I N K   ·   R E A C H",10,270,170,360,25,new Color(.41f,.7f,1));
  Side("События",LobbyAction.Events,LobbyIcon.Crown,45,242,Gold);Side("Задания",LobbyAction.Tasks,LobbyIcon.Tasks,45,328);Side("Магазин",LobbyAction.Shop,LobbyIcon.Shop,45,414,Gold);
  Side("Коллекция",LobbyAction.Collection,LobbyIcon.Star,495,242,Gold);Side("Статистика",LobbyAction.Stats,LobbyIcon.Stats,495,328);Side("Скины",LobbyAction.Skins,LobbyIcon.Brush,495,414);
  var greet=At(safeRoot,"Tap the living hero",180,220,185,567);var hit=greet.gameObject.AddComponent<Image>();hit.color=Color.clear;var greetButton=greet.gameObject.AddComponent<Button>();greetButton.targetGraphic=hit;greetButton.transition=Selectable.Transition.None;var greetAction=greet.gameObject.AddComponent<LobbyButton>();greetAction.lobby=this;greetAction.action=LobbyAction.Greet;
  var play=Button(safeRoot,"Play • primary action",LobbyAction.Play,431,91,270,640,null,32,Cyan,primary:true);Icon(play.transform,LobbyIcon.Play,47,-136,0,Cyan);playLabel=Text(play.transform,"Play label","ИГРАТЬ",31,240,42,19,11,null,true);playDetail=Text(play.transform,"Continue level","УРОВЕНЬ 1",15,240,27,19,-23,Cyan);Icon(play.transform,LobbyIcon.Arrow,25,178,0,Cyan);
  var reward=Button(safeRoot,"Daily reward banner",LobbyAction.Gift,507,70,270,828,null,18,Purple);Icon(reward.transform,LobbyIcon.Gift,36,-199,0,Gold);Text(reward.transform,"Title","ПОДАРОК ДНЯ",19,350,28,5,11,null,true);Text(reward.transform,"Description","100 кристаллов + 1 резонанс",12,350,25,5,-14,new Color(.65f,.72f,1));Icon(reward.transform,LobbyIcon.Arrow,26,217,0,Purple);
  popupRoot=Rect(safeRoot,"Pages • native Canvas modals",540,960,0,0);toastLabel=Label(safeRoot,"",13,270,585,430,45,Cyan);toastLabel.name="Toast • action feedback";toastLabel.gameObject.SetActive(false);
  var stageObject=new GameObject("Lobby Stage • animated 3D models");stageObject.transform.SetParent(transform,false);stage=stageObject.AddComponent<LobbyStage>();stage.lobby=this;stage.Build();UpgradeLayout();canvas.enabled=false;stage.SetVisible(false);
 }
 [SerializeField,HideInInspector] int layoutVersion;
 public static bool RemovedHomeButton(LobbyButton button)=>button.name.StartsWith("Navigation • ",System.StringComparison.Ordinal)||button.action==LobbyAction.Campaign||button.action==LobbyAction.Challenge||button.action==LobbyAction.Endless;
 public void UpgradeLayout(){
  EnsureLevelEntry();
  EnsurePolishedHeader();
  if(standaloneScene)return;
  if(!safeRoot)return;
  // Migrate existing editable scenes too, not just newly generated menus.
  foreach(var button in safeRoot.GetComponentsInChildren<LobbyButton>(true)){
   if(button.transform.parent!=safeRoot||!RemovedHomeButton(button))continue;
   button.gameObject.SetActive(false);
   if(Application.isPlaying)Destroy(button.gameObject);else DestroyImmediate(button.gameObject);
  }
  if(layoutVersion>=2)return;
  var play=safeRoot.Find("Play • primary action") as RectTransform;if(play)play.anchoredPosition=new Vector2(0,480-775);
  var reward=safeRoot.Find("Daily reward banner") as RectTransform;if(reward)reward.anchoredPosition=new Vector2(0,480-885);
  var greet=safeRoot.Find("Tap the living hero") as RectTransform;if(greet){greet.anchoredPosition=new Vector2(185-270,480-567);greet.sizeDelta=new Vector2(180,220);}
  layoutVersion=2;
 }
 public void ClosePopup(){Page="";if(!popupRoot)return;foreach(Transform child in popupRoot){child.gameObject.SetActive(false);if(Application.isPlaying)Destroy(child.gameObject);else DestroyImmediate(child.gameObject);}popupFade=null;nameInput=null;}
}
