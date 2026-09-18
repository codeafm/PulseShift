using System;
using UnityEngine;
using UnityEngine.UI;

public partial class PulseLobby {
 static readonly Color QuietBlue=new Color(.49f,.69f,.82f);
 // Native vector panels reuse the menu's existing art; no new large textures,
 // extra cameras or particle systems are needed on older phones.
 LobbyPlate UtilityCard(Transform root,string name,float x,float y,float width,float height,Color accent,bool strong=false){
  var plate=At(root,name,width,height,x,y).gameObject.AddComponent<LobbyPlate>();
  plate.clean=true;plate.primary=strong;plate.accent=accent;plate.raycastTarget=false;
  plate.top=new Color(.018f,.065f,.12f,.98f);plate.bottom=new Color(.005f,.019f,.05f,.99f);return plate;
 }
 void UtilityRule(Transform root,float y,float width=392){
  var line=At(root,"Fine cyan divider",width,1,270,y).gameObject.AddComponent<Image>();line.color=new Color(.1f,.52f,.75f,.4f);line.raycastTarget=false;
 }
 void UtilityFrame(Transform root,string oldTitle,string title,string subtitle,LobbyIcon icon){
  var sheet=root.Find("Glass sheet");if(sheet)sheet.gameObject.SetActive(false);
  var label=root.Find(oldTitle);if(label)label.gameObject.SetActive(false);
  // Keep the input shield, but don't draw a dark rectangle over the menu artwork.
  var shield=root.GetComponent<Image>();if(shield)shield.color=Color.clear;
  var frame=UtilityCard(root,"Refined modal frame",270,491,496,794,new Color(.055f,.53f,.8f),true);frame.raycastTarget=true;frame.transform.SetSiblingIndex(1);
  UtilityCard(root,"Inset header",250,151,412,76,new Color(.08f,.63f,.9f));
  Icon(At(root,"Header emblem",42,42,83,151),icon,38,0,0,Cyan);
  Label(root,title,27,267,151,312,43,White,true);
  Label(root,subtitle,16,270,207,408,28,QuietBlue);
  var close=root.Find("Close page") as RectTransform;
  if(close){close.anchoredPosition=new Vector2(477-270,480-151);close.sizeDelta=new Vector2(48,48);var plate=close.GetComponent<LobbyPlate>();plate.clean=true;plate.accent=Cyan;var glyph=close.GetComponentInChildren<LobbyGlyph>();if(glyph){glyph.rectTransform.sizeDelta=Vector2.one*32;glyph.color=Cyan;}close.SetAsLastSibling();}
 }
 void UtilitySection(Transform root,string text,float y){
  var label=Label(root,text,14,270,y,414,24,QuietBlue,true);label.alignment=TextAnchor.MiddleLeft;
 }
 void BuildProfilePage(Transform root){
  UtilityFrame(root,"ПРОФИЛЬ","ПРОФИЛЬ","Твой герой. Твоё путешествие.",LobbyIcon.Hero);
  var tint=SkinColors[Mathf.Clamp(Profile.selectedSkin,0,SkinColors.Length-1)];
  var avatar=At(root,"Profile avatar",150,150,270,322);
  var halo=Neon(avatar,NeonSymbol.Disc,136,0,0,new Color(.05f,.65f,.9f,.35f));halo.filled=false;
  Icon(avatar,LobbyIcon.Hero,82,0,0,tint);
  Label(root,"ИМЯ ИГРОКА",15,270,430,398,26,QuietBlue,true).alignment=TextAnchor.MiddleLeft;
  var input=At(root,"Player name input",414,68,270,485);
  var plate=input.gameObject.AddComponent<LobbyPlate>();plate.clean=true;plate.accent=new Color(.08f,.5f,.7f);plate.top=new Color(.012f,.04f,.075f);plate.bottom=new Color(.005f,.017f,.036f);
  nameInput=input.gameObject.AddComponent<InputField>();nameInput.targetGraphic=plate;
  var entry=Text(input,"Name",Profile.playerName,24,366,48,0,0,White);entry.alignment=TextAnchor.MiddleLeft;entry.supportRichText=false;
  var placeholder=Text(input,"Placeholder","Введи имя",24,366,48,0,0,QuietBlue);placeholder.alignment=TextAnchor.MiddleLeft;
  nameInput.textComponent=entry;nameInput.placeholder=placeholder;nameInput.characterLimit=16;nameInput.contentType=InputField.ContentType.Standard;nameInput.lineType=InputField.LineType.SingleLine;nameInput.text=Profile.playerName;
  nameInput.caretColor=Cyan;nameInput.customCaretColor=true;nameInput.selectionColor=new Color(.08f,.55f,.8f,.4f);
  Label(root,"До 16 символов · можно изменить позже",14,270,540,398,27,QuietBlue).alignment=TextAnchor.MiddleLeft;
  var save=Button(root,"Save profile name",LobbyAction.SaveName,414,66,270,634,"СОХРАНИТЬ ИМЯ",21,Cyan,primary:true);save.GetComponent<LobbyPlate>().clean=true;
  UtilityRule(root,708,380);
  Label(root,"Прогресс хранится\nна этом устройстве",17,270,760,380,65,QuietBlue);
 }
 void BuildGiftPage(Transform root){
  bool ready=Profile.GiftReady(Today);
  UtilityFrame(root,"ПОДАРОК","ПОДАРОК ДНЯ","Немного света для нового путешествия",LobbyIcon.Gift);
  var halo=At(root,"Gift halo",170,142,270,300);
  var ring=Neon(halo,NeonSymbol.Disc,143,0,0,new Color(.04f,.58f,1,.33f));ring.filled=false;
  Icon(halo,LobbyIcon.Gift,108,0,0,Cyan);
  Label(root,ready?"ТВОЯ ЕЖЕДНЕВНАЯ НАГРАДА":"СПАСИБО, ЧТО ТЫ С НАМИ",15,270,382,410,26,ready?Gold:Cyan,true);
  GiftResource(root,174,"100","КРИСТАЛЛОВ",Gold,NeonSymbol.Crystal);
  GiftResource(root,366,"1","ИМПУЛЬС",Cyan,NeonSymbol.Pulse);
  UtilitySection(root,"СЕМЬ ДНЕЙ СВЕТА",587);
  DateTime previous;bool continuing=DateTime.TryParse(Profile.giftDay,out previous)&&previous.Date==DateTime.UtcNow.Date.AddDays(-1);
  int current=ready?(continuing?Mathf.Min(7,Profile.giftStreak+1):1):Mathf.Max(1,Profile.giftStreak);
  for(int i=1;i<=7;i++){
   bool done=i<current||!ready&&i==current;bool active=ready&&i==current;
   var color=active?Cyan:done?new Color(.27f,.8f,.61f):new Color(.13f,.26f,.39f);
   var day=UtilityCard(root,"Gift day "+i,78+(i-1)*64,644,56,70,color,active);
   Text(day.transform,"Day label","ДЕНЬ",10,52,17,0,21,QuietBlue);
   Text(day.transform,"Day number",i.ToString(),24,44,32,0,-2,active?White:done?new Color(.35f,.95f,.7f):QuietBlue,true);
   if(done){var mark=Rect(day.transform,"Claimed",18,2,0,-25).gameObject.AddComponent<Image>();mark.color=new Color(.35f,.95f,.7f);mark.raycastTarget=false;}
  }
  Label(root,ready?"Заходи каждый день и продолжай серию":"Сегодня получено · серия "+current+" дн.",17,270,711,412,28,QuietBlue);
  var claim=Button(root,"Claim daily gift",LobbyAction.ClaimGift,416,66,270,778,ready?"ЗАБРАТЬ ПОДАРОК":"ПОДАРОК ПОЛУЧЕН",22,Cyan,primary:true);
  claim.GetComponent<LobbyPlate>().clean=true;
  claim.GetComponent<Button>().interactable=ready;
  Label(root,ready?"БЕЗ РЕКЛАМЫ · КАЖДЫЙ ДЕНЬ":"Новый подарок после 00:00 UTC",14,270,842,412,26,ready?QuietBlue:Cyan);
 }
 void GiftResource(Transform root,float x,string amount,string caption,Color tint,NeonSymbol symbol){
  // The reward artwork sits directly on the modal: no rectangular image backing.
  var reward=At(root,"Gift resource "+caption,174,150,x,480);
  Neon(reward,symbol,72,0,34,tint);
  Text(reward,"Amount",amount,30,166,39,0,-19,White,true);
  Text(reward,"Resource",caption,15,170,25,0,-56,tint,true);
 }
 void BuildSettingsPage(Transform root){
  UtilityFrame(root,"НАСТРОЙКИ","НАСТРОЙКИ","Твой ритм. Твой комфорт.",LobbyIcon.Gear);
  UtilityCard(root,"Audio controls",270,345,430,246,new Color(.065f,.31f,.47f));
  UtilityAudioSlider(root,"Музыка",259,Profile.music,true);
  UtilityAudioSlider(root,"Эффекты",337,Profile.sound,false);
  UtilityRule(root,401,386);
  UtilityToggle(root,"Звук прыжка",LobbyAction.ToggleJumpSound,433,Profile.jumpSound,true);
  UtilitySection(root,"ИГРОВОЙ КОМФОРТ",502);
  UtilityToggle(root,"Вибрация",LobbyAction.ToggleVibration,549,Profile.vibration);
  UtilityValue(root,"Плавность",MobilePerformance.Lite?"АВТО · 30 FPS":Profile.fps60?"60 FPS":"30 FPS",LobbyAction.ToggleFps,614);
  UtilityValue(root,"Графика",QualityCaption,LobbyAction.CycleQuality,679);
  UtilityToggle(root,"Меньше движения",LobbyAction.ToggleMotion,744,Profile.reducedMotion);
  var support=Button(root,"Settings support",LobbyAction.Support,204,43,160,818,"ПОДДЕРЖКА",14,new Color(.12f,.35f,.5f));support.GetComponent<LobbyPlate>().clean=true;
  var privacy=Button(root,"Settings privacy",LobbyAction.Privacy,204,43,380,818,"КОНФИДЕНЦИАЛЬНОСТЬ",12,new Color(.12f,.35f,.5f));privacy.GetComponent<LobbyPlate>().clean=true;
  Label(root,"Настройки сохраняются автоматически",14,270,859,420,24,QuietBlue);
 }
 LobbyButton UtilitySettingRow(Transform root,string title,LobbyAction action,float y,bool inset=false){
  var button=Button(root,title,action,inset?394:430,inset?50:56,270,y);
  var plate=button.GetComponent<LobbyPlate>();plate.clean=true;plate.hitAreaOnly=inset;plate.accent=new Color(.075f,.32f,.47f);plate.top=new Color(.017f,.061f,.105f);plate.bottom=new Color(.007f,.025f,.052f);
  var label=Text(button.transform,"Setting title",title,20,258,35,inset?-56:-61,0,White);label.alignment=TextAnchor.MiddleLeft;return button;
 }
 void UtilityToggle(Transform root,string title,LobbyAction action,float y,bool active,bool inset=false){
  var button=UtilitySettingRow(root,title,action,y,inset);
  var toggle=Rect(button.transform,"Switch",83,32,147,0).gameObject.AddComponent<LobbyPlate>();toggle.clean=true;toggle.raycastTarget=false;toggle.accent=active?Cyan:new Color(.25f,.36f,.46f);
  toggle.top=active?new Color(.015f,.25f,.34f):new Color(.045f,.075f,.1f);toggle.bottom=new Color(.009f,.03f,.054f);
  Text(toggle.transform,"Switch state",active?"ВКЛ":"ВЫКЛ",12,49,26,active?-11:12,0,active?Cyan:QuietBlue,true);
  var knob=Rect(toggle.transform,"Switch light",15,20,active?26:-27,0).gameObject.AddComponent<Image>();knob.color=active?Cyan:QuietBlue;knob.raycastTarget=false;
 }
 void UtilityValue(Transform root,string title,string value,LobbyAction action,float y){
  var button=UtilitySettingRow(root,title,action,y);
  Text(button.transform,"Setting value",value,17,172,34,90,0,Cyan,true);
  Icon(button.transform,LobbyIcon.Arrow,16,190,0,QuietBlue);
 }
 void UtilityAudioSlider(Transform root,string title,float y,float value,bool music){
  var label=Label(root,title,20,250,y,332,31,White);label.alignment=TextAnchor.MiddleLeft;
  var percent=Label(root,Mathf.RoundToInt(value*100)+"%",17,434,y,64,29,Cyan,true);
  var host=At(root,title+" slider",376,40,270,y+32);var hit=host.gameObject.AddComponent<Image>();hit.color=Color.clear;
  var track=Rect(host,"Track",360,6,0,0).gameObject.AddComponent<Image>();track.color=new Color(.08f,.18f,.25f);track.raycastTarget=false;
  var fillArea=Rect(host,"Fill area",360,6,0,0);var fill=Rect(fillArea,"Fill",0,0,0,0).gameObject.AddComponent<Image>();fill.color=Cyan;fill.raycastTarget=false;fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;
  var handleArea=Rect(host,"Handle area",360,24,0,0);var handle=Rect(handleArea,"Handle",18,0,0,0).gameObject.AddComponent<Image>();handle.color=White;handle.rectTransform.anchorMin=new Vector2(.5f,0);handle.rectTransform.anchorMax=new Vector2(.5f,1);
  var slider=host.gameObject.AddComponent<Slider>();slider.direction=Slider.Direction.LeftToRight;slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.minValue=0;slider.maxValue=1;slider.SetValueWithoutNotify(value);
  slider.onValueChanged.AddListener(v=>{if(music)Profile.music=v;else Profile.sound=v;percent.text=Mathf.RoundToInt(v*100)+"%";Save();});
 }
}
