using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public partial class PulseLobby {
 void PageText(Transform root,string value,float y,int size=16,Color? tint=null,float h=70)=>Label(root,value,size,270,y,412,h,tint);
 LobbyButton PageButton(Transform root,string label,LobbyAction action,float y,int value=0,bool enabled=true,Color? accent=null){var b=Button(root,label,action,392,48,270,y,label,16,accent,value);b.GetComponent<Button>().interactable=enabled;return b;}
 void BuildRewardPage(Transform root,bool pulse){
  var accent=pulse?Cyan:Gold;var cool=new Color(.02f,.75f,1,1);
  var oldSheet=root.Find("Glass sheet");if(oldSheet)oldSheet.gameObject.SetActive(false);
  var oldTitle=root.Find("НАГРАДА");if(oldTitle)oldTitle.gameObject.SetActive(false);
  // The two user-authored source files currently contain the opposite reward artwork:
  // RewardCrystalPanel = blue impulse, RewardPulsePanel = golden crystal.
  string artName=pulse?"Lobby/RewardCrystalPanel":"Lobby/RewardPulsePanel";
  float titleY=pulse?184:139,amountY=pulse?599:587,descriptionY=pulse?642:631,buttonY=pulse?706:717,noteY=pulse?789:805;
  var artRect=At(root,"Reward illustrated frame",506,900,270,500);var art=artRect.gameObject.AddComponent<RawImage>();art.texture=Resources.Load<Texture2D>(artName);art.color=Color.white;art.raycastTarget=false;artRect.SetSiblingIndex(1);
  var close=root.Find("Close page") as RectTransform;if(close){close.anchoredPosition=new Vector2(218,480-titleY);close.sizeDelta=new Vector2(48,48);close.SetAsLastSibling();}
  Label(root,"НАГРАДА",27,270,titleY,330,48,White,true);
  PageText(root,pulse?"1 ИМПУЛЬС":"10 КРИСТАЛЛОВ",amountY,30,accent,44);
  PageText(root,"Награда за короткий просмотр\nОсталось сегодня: "+VideoRewardsLeft+" из 5",descriptionY,15,new Color(.9f,.95f,1),54);
  var watch=Button(root,"Watch rewarded video",LobbyAction.WatchReward,350,58,270,buttonY,VideoRewardsLeft>0?"СМОТРЕТЬ REWARD":"ЛИМИТ НА СЕГОДНЯ",17,cool,0,false);var watchPlate=watch.GetComponent<LobbyPlate>();watchPlate.hitAreaOnly=true;watch.GetComponent<Button>().interactable=VideoRewardsLeft>0;
  Icon(watch.transform,LobbyIcon.Play,27,145,0,cool);
  Icon(At(root,"Reward reset clock",24,24,137,noteY),LobbyIcon.Clock,22,0,0,new Color(.38f,.8f,1));
  Label(root,"Лимит обновляется после 00:00 UTC",12,302,noteY,288,27,new Color(.55f,.79f,1));
 }
 void EnsurePopupRoot(){if(popupRoot)return;popupRoot=Rect(safeRoot,"Pages • native Canvas modals",LayoutWidth,LayoutHeight,0,0);popupRoot.SetAsLastSibling();}
 public void ShowPage(string page){
  ClosePopup();EnsurePopupRoot();Page=page;var root=Rect(popupRoot,page+" • page",LayoutWidth/popupRoot.localScale.x,LayoutHeight/popupRoot.localScale.x,0,0);popupFade=root.gameObject.AddComponent<CanvasGroup>();popupFade.alpha=0;popupAge=0;
  var shield=root.gameObject.AddComponent<Image>();shield.color=new Color(.005f,.009f,.03f,.91f);shield.raycastTarget=true;
  if(page=="Скины"||page=="Магазин"){BuildSkinShowcase(root);if(toastLabel)toastLabel.transform.SetAsLastSibling();return;}
  if(page=="Уровни"){BuildArchipelagoPage(root);if(toastLabel)toastLabel.transform.SetAsLastSibling();return;}
  var panel=At(root,"Glass sheet",500,790,270,505).gameObject.AddComponent<LobbyPlate>();panel.accent=Cyan;panel.raycastTarget=true;
  Label(root,page.ToUpperInvariant(),23,270,142,372,40,White,true);var close=Button(root,"Close page",LobbyAction.Close,41,41,488,141);Icon(close.transform,LobbyIcon.Close,22,0,0);
  switch(page){
   case "Профиль":
    BuildProfilePage(root);break;
   case "Настройки":
    BuildSettingsPage(root);break;
   case "Уровни":
    PageText(root,$"КАМПАНИЯ  ·  {completed}/100 ПРОЙДЕНО  ·  {totalStars} ЗВЁЗД",251,13,Cyan,38);
    for(int i=0;i<20;i++){int id=levelPage*20+i;if(id>=MenuData.levels.Length)continue;bool open=id<MenuUnlocked;int stars=MenuStars(id+1);var b=Button(root,"Level "+(id+1),LobbyAction.SelectLevel,91,69,115+(i%4)*103,326+(i/4)*83,null,20,open?Cyan:new Color(.2f,.25f,.38f),id);b.GetComponent<Button>().interactable=open;Text(b.transform,"Number",(id+1).ToString("00"),23,86,31,0,9,null,true);Text(b.transform,"Stars",open?(stars>0?new string('★',stars):"ОТКРЫТ"):"ЗАКРЫТ",10,86,22,0,-20,open?Gold:new Color(.35f,.44f,.6f));}
    PageButton(root,"ПРЕДЫДУЩИЕ",LobbyAction.PreviousPage,738,enabled:levelPage>0);PageButton(root,"СЛЕДУЮЩИЕ",LobbyAction.NextPage,792,enabled:levelPage<(MenuData.levels.Length-1)/20);break;
   case "Подарок":
    BuildGiftPage(root);break;
   case "Награда":
    shield.color=Color.clear;BuildRewardPage(root,PendingPulseReward);break;
   case "Просмотр":
    videoCountdownLabel=Text(root,"Reward countdown","REWARD VIDEO\nПодготовка...",25,390,130,0,80,Cyan,true);
    Icon(At(root,"Reward play",130,130,270,460),LobbyIcon.Play,120,0,0,Cyan);PageText(root,"Не закрывай экран — награда будет начислена автоматически",620,15,new Color(.7f,.84f,1),55);break;
   case "Политика":
    PageText(root,"Прогресс PULSESHIFT хранится на устройстве.\n\nДля показа рекламы используется Yandex Mobile Ads. Рекламный SDK может обрабатывать технические сведения об устройстве и события показа рекламы согласно политике Яндекса: yandex.ru/legal/confidential/\n\nИгра не передаёт рекламной сети имя игрока и сохранения. Отслеживание местоположения отключено.\n\nНаграда выдаётся только за подтверждённый просмотр рекламы.",390,16,new Color(.75f,.88f,1),350);
    PageButton(root,"ПОНЯТНО",LobbyAction.Close,735,accent:Cyan);break;
   case "Скины":case "Магазин":BuildSkinShowcase(root);break;
   case "Задания":case "Достижения":
    bool achievements=page=="Достижения";var names=achievements?new[]{"ПЕРВЫЙ СВЕТ","СОБИРАТЕЛЬ ЗВЁЗД","ХРАНИТЕЛЬ АРХИПЕЛАГА"}:new[]{"ПЕРВОЕ ПУТЕШЕСТВИЕ","ТРИ ОСТРОВА","МАЛЕНЬКОЕ СОЗВЕЗДИЕ"};var goals=achievements?new[]{1,15,30}:new[]{1,3,9};
    PageText(root,"Забери награды за свой прогресс",263,15,new Color(.6f,.75f,1));
    for(int i=0;i<3;i++){float y=355+i*145;int progress=MilestoneProgress(i,achievements),goal=goals[i];bool claimed=((achievements?Profile.claimedAchievements:Profile.claimedTasks)&(1<<i))!=0;Label(root,names[i],17,270,y-28,395,32,Gold,true);Label(root,$"{Mathf.Min(progress,goal)} / {goal}  ·  награда {Reward(i,achievements)} кристаллов",14,270,y+4,395,31);PageButton(root,claimed?"НАГРАДА ПОЛУЧЕНА":progress>=goal?"ЗАБРАТЬ":"В ПРОЦЕССЕ",achievements?LobbyAction.ClaimAchievement:LobbyAction.ClaimTask,y+52,i,progress>=goal&&!claimed);}
    break;
   case "Испытание дня":
    int selected=30+DateTime.UtcNow.DayOfYear%Mathf.Min(60,MenuData.levels.Length-30);var trial=MenuData.levels[selected];
    Icon(At(root,"Trial icon",84,84,270,335),LobbyIcon.Clock,82,0,0,Purple);
    PageText(root,"КАРТА ДНЯ · "+trial.id,430,23,White);PageText(root,trial.layout+"\nЦель: не больше "+(trial.optimalTurns+2)+" ходов",500,19,Cyan);
    PageText(root,"Одна проверенная карта на день.\nНаграда за цель: 100 кристаллов + 2 резонанса.\nПовторное получение в тот же день невозможно.",604,14,null,94);
    PageButton(root,"НАЧАТЬ ИСПЫТАНИЕ",LobbyAction.StartChallenge,716,accent:Purple);break;
   case "Бесконечный":
    Icon(At(root,"Endless icon",108,85,270,345),LobbyIcon.Infinity,100,0,0,Gold);
    PageText(root,"КАК ДАЛЕКО ТЫ ДОЙДЁШЬ?",441,22,White);PageText(root,"Лучший результат: "+Profile.bestEndless+" островов",500,18,Gold);
    PageText(root,"Непрерывная серия проверенных карт кампании.\nПосле победы открывается следующая карта.\nЗа каждые три победы — 1 резонанс.\nОтмена хода доступна. Выход завершает серию.",613,14,null,120);
    PageButton(root,"НАЧАТЬ СЕРИЮ",LobbyAction.StartEndless,728,accent:Gold);break;
   case "Коллекция":
    PageText(root,"Энциклопедия света и теней",258,16,new Color(.6f,.75f,1));
    string[] cards={"ИСКРА\nСобирает три кристалла и находит портал.","ПАТРУЛЬ\nДвигается по известному маршруту.","ДОЗОРНЫЙ\nЖёлтый прицел предупреждает о выстреле.","ОХОТНИК\nПреследует через ход. Импульс задерживает его."};
    for(int i=0;i<4;i++){float y=342+i*102;Icon(At(root,"Collection icon "+i,47,47,98,y),i==0?LobbyIcon.Hero:i==1?LobbyIcon.Levels:i==2?LobbyIcon.Crown:LobbyIcon.Hero,45,0,0,i==0?Cyan:i==1?Purple:i==2?Gold:new Color(1,.25f,.15f));Label(root,cards[i],15,304,y,307,76);}
    PageButton(root,"ПОСМОТРЕТЬ ОБЛИКИ",LobbyAction.Skins,783);break;
   case "Статистика":
    Icon(At(root,"Statistics icon",88,88,270,321),LobbyIcon.Stats,82,0,0,Cyan);
    PageText(root,$"ОСТРОВА КАМПАНИИ\n{completed} / {MenuData.levels.Length}",435,23,Cyan,81);
    PageText(root,$"ЗВЁЗДЫ\n{totalStars} / {MenuData.levels.Length*3}",552,23,Gold,81);
    PageText(root,$"Победы с обновления меню: {Profile.wins}\nЛучшая бесконечная серия: {Profile.bestEndless}\nОткрытые облики: {CountSkins()} / 10",693,17,null,108);break;
  }
  if(toastLabel)toastLabel.transform.SetAsLastSibling();
 }
 void BuildSkinShowcase(Transform root){BuildSkinScreen(root);}
 public void CycleSkin(int direction){int current=skinCarousel?skinCarousel.RequestedIndex:skinPreviewIndex;SelectSkinPreview(skinCarousel?skinCarousel.RequestedPortal:skinPortalTab,(current+direction+5)%5,direction);}
 public void SwipeSkin(float delta){if(Mathf.Abs(delta)<45)return;CycleSkin(delta<0?1:-1);}
 IEnumerator BreakSkinLock(){
  if(!skinLockVisual){RefreshSkinScreen();yield break;}var lockObject=skinLockVisual;var group=lockObject.gameObject.AddComponent<CanvasGroup>();Vector3 start=lockObject.localScale;
  for(float t=0;t<.38f;t+=Time.unscaledDeltaTime){if(!lockObject)yield break;float p=t/.38f;lockObject.localScale=start*(1+p*.55f);lockObject.localRotation=Quaternion.Euler(0,0,Mathf.Sin(p*35)*10*(1-p));group.alpha=1-p;yield return null;}
  RefreshSkinScreen();
 }
 int CountSkins(){int count=0;for(int i=0;i<5;i++){if((Profile.ownedSkins&(1<<i))!=0)count++;if((Profile.ownedPortals&(1<<i))!=0)count++;}return count;}
 int MilestoneProgress(int i,bool achievement)=>achievement?(i==0?completed:totalStars):(i==0?Mathf.Max(Profile.wins,completed):i==1?completed:totalStars);
 int Reward(int i,bool achievement)=>(achievement?100:50)*(i+1);
 void ClaimMilestone(int i,bool achievement){if(i<0||i>2)return;int[] goals=achievement?new[]{1,15,30}:new[]{1,3,9};int claimed=achievement?Profile.claimedAchievements:Profile.claimedTasks;if((claimed&(1<<i))!=0||MilestoneProgress(i,achievement)<goals[i])return;Profile.crystals+=Reward(i,achievement);if(achievement)Profile.claimedAchievements|=1<<i;else Profile.claimedTasks|=1<<i;Save();PlayBonus();Toast("Награда получена!");ShowPage(achievement?"Достижения":"Задания");}
}
