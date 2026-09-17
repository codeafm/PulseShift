using UnityEngine;
using UnityEngine.UI;

public sealed class CampaignPausePanel:MonoBehaviour {
 CampaignHUD hud;GameObject overlay,menu,settings;RectTransform panel;CanvasGroup fade;
 readonly Color cyan=new Color(.02f,.8f,1);bool wasVisible;
 public static CampaignPausePanel Ensure(CampaignHUD hud){var view=hud.GetComponent<CampaignPausePanel>();if(!view)view=hud.gameObject.AddComponent<CampaignPausePanel>();view.hud=hud;if(!view.overlay)view.Build();return view;}
 RectTransform Box(Transform parent,string name,float w,float h,float x=0,float y=0){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.sizeDelta=new Vector2(w,h);r.anchoredPosition=new Vector2(x,y);return r;}
 Text Text(Transform parent,string name,string value,int size,float w,float h,float y){var r=Box(parent,name,w,h,0,y);var t=r.gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;t.text=value;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.7f,.86f,1);t.raycastTarget=false;return t;}
 Button Action(Transform parent,string name,string title,float y,LobbyIcon icon,System.Action click,bool primary=false){
  var r=Box(parent,name,346,58,0,y);var p=r.gameObject.AddComponent<LobbyPlate>();p.clean=true;p.primary=primary;p.accent=cyan;p.top=new Color(.012f,.075f,.15f,.97f);p.bottom=new Color(.003f,.017f,.04f,1);
  var b=r.gameObject.AddComponent<Button>();b.targetGraphic=p;b.onClick.AddListener(()=>click());
  var symbol=Box(r,"Icon",32,32,-126,0).gameObject.AddComponent<LobbyGlyph>();symbol.icon=icon;symbol.color=primary?cyan:new Color(.64f,.83f,1);symbol.raycastTarget=false;
  var text=Text(r,"Label",title,18,268,48,0);text.rectTransform.anchoredPosition=new Vector2(26,0);text.fontStyle=FontStyle.Bold;return b;
 }
 void Build(){
  var root=Box(hud.safeRoot,"Pause • cinematic overlay",0,0);root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.sizeDelta=Vector2.zero;overlay=root.gameObject;
  var shade=overlay.AddComponent<Image>();shade.color=new Color(.002f,.008f,.025f,.68f);fade=overlay.AddComponent<CanvasGroup>();
  panel=Box(root,"Pause frame",410,510);var plate=panel.gameObject.AddComponent<LobbyPlate>();plate.clean=true;plate.primary=true;plate.accent=cyan;plate.top=new Color(.006f,.025f,.065f,.98f);plate.bottom=new Color(.001f,.008f,.025f,.99f);
  var logo=Box(panel,"Logo plate",272,55,0,224).gameObject.AddComponent<LobbyPlate>();logo.clean=true;logo.accent=cyan;
  var brand=Text(logo.transform,"PULSESHIFT","PULSESHIFT",26,268,48,0);brand.fontStyle=FontStyle.BoldAndItalic;brand.color=new Color(.65f,.95f,1);
  foreach(float y in new[]{264f,-253f}){var emblem=Box(panel,"Portal emblem",53,53,0,y).gameObject.AddComponent<NeonGraphic>();emblem.symbol=NeonSymbol.Pulse;emblem.accent=cyan;emblem.raycastTarget=false;}
  menu=Box(panel,"Pause actions",410,510).gameObject;
  var title=Text(menu.transform,"Title","ПАУЗА",31,360,47,156);title.fontStyle=FontStyle.Bold;title.color=Color.white;
  Text(menu.transform,"Subtitle","Мир замер. Продолжи путешествие\nили вернись в главное меню.",16,355,52,107);
  Action(menu.transform,"Resume","ПРОДОЛЖИТЬ",28,LobbyIcon.Play,()=>hud.game.RequestUI(CampaignUICommand.Pause),true);
  var restart=Action(menu.transform,"Restart","НАЧАТЬ ЗАНОВО",-44,LobbyIcon.Clock,()=>hud.game.RequestUI(CampaignUICommand.Restart));
  restart.transform.Find("Icon").gameObject.SetActive(false);var arrow=Box(restart.transform,"Restart arrow",32,32,-126,0).gameObject.AddComponent<NeonGraphic>();arrow.symbol=NeonSymbol.Undo;arrow.raycastTarget=false;
  Action(menu.transform,"Settings","НАСТРОЙКИ",-116,LobbyIcon.Gear,()=>ShowSettings(true));
  Action(menu.transform,"Home","В ГЛАВНОЕ МЕНЮ",-188,LobbyIcon.Home,()=>hud.game.RequestUI(CampaignUICommand.Home));
  settings=Box(panel,"Pause audio settings",410,510).gameObject;Text(settings.transform,"Title","НАСТРОЙКИ",28,360,45,153).fontStyle=FontStyle.Bold;
  AddSlider("МУЗЫКА",68,true);AddSlider("ЭФФЕКТЫ",-34,false);
  Action(settings.transform,"Back to pause","НАЗАД К ПАУЗЕ",-166,LobbyIcon.Arrow,()=>ShowSettings(false));settings.SetActive(false);overlay.SetActive(false);
 }
 PulseProfile Profile=>hud.game.useSeparateMenuScene?PulseSceneFlow.Profile:hud.game.lobby?hud.game.lobby.Profile:PulseSceneFlow.Profile;
 void AddSlider(string title,float y,bool music){
  Text(settings.transform,title,title,18,310,32,y+30);
  var r=Box(settings.transform,title+" volume",306,35,0,y-12);var bg=r.gameObject.AddComponent<Image>();bg.color=new Color(.025f,.08f,.14f);
  var fill=Box(r,"Fill",0,9).gameObject.AddComponent<Image>();fill.color=cyan;fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;fill.rectTransform.sizeDelta=Vector2.zero;
  var area=Box(r,"Handle area",286,35);var handle=Box(area,"Handle",18,29).gameObject.AddComponent<Image>();handle.color=new Color(.65f,.95f,1);
  var slider=r.gameObject.AddComponent<Slider>();slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=0;slider.maxValue=1;slider.SetValueWithoutNotify(music?Profile.music:Profile.sound);
  slider.onValueChanged.AddListener(v=>{if(music)Profile.music=v;else Profile.sound=v;Profile.Save(PulseSceneFlow.Testing);hud.game.ApplyAudioSettings();});
 }
 public void ShowSettings(bool value){menu.SetActive(!value);settings.SetActive(value);}
 public void Refresh(){
  bool visible=hud.game.Paused&&!hud.game.LessonOpen&&!hud.game.MenuOpen;
  if(visible&&!wasVisible){ShowSettings(false);fade.alpha=0;overlay.SetActive(true);}wasVisible=visible;
  fade.alpha=Application.isPlaying?Mathf.MoveTowards(fade.alpha,visible?1:0,Time.unscaledDeltaTime*8):(visible?1:0);
  fade.blocksRaycasts=fade.interactable=visible;if(!visible&&fade.alpha==0)overlay.SetActive(false);
  float fit=Mathf.Min(1,Mathf.Min((hud.safeRoot.rect.width-36)/410,(hud.safeRoot.rect.height-100)/560));panel.localScale=Vector3.one*fit*Mathf.Lerp(.97f,1,fade.alpha);
  if(visible)overlay.transform.SetAsLastSibling();
 }
}
