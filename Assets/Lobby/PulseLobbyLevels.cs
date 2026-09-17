using UnityEngine;
using UnityEngine.UI;

public partial class PulseLobby {
 void EnsureLevelEntry(){
  if(!safeRoot)return;var existing=safeRoot.Find("Levels • island card");
  if(existing){existing.Find("Level number").GetComponent<Text>().text=Mathf.Clamp(MenuUnlocked,1,100).ToString();return;}
  var card=Rect(safeRoot,"Levels • island card",70,70,0,0);card.anchorMin=card.anchorMax=new Vector2(1,.5f);card.anchoredPosition=new Vector2(-40,184);
  var art=Rect(card,"Supplied island artwork",0,0,0,0).gameObject.AddComponent<RawImage>();art.rectTransform.anchorMin=Vector2.zero;art.rectTransform.anchorMax=Vector2.one;art.texture=Resources.Load<Texture2D>("Lobby/LevelIslandButton");art.uvRect=new Rect(.07f,.105f,.86f,.81f);art.raycastTarget=true;
  var button=card.gameObject.AddComponent<Button>();button.targetGraphic=art;button.transition=Selectable.Transition.ColorTint;var action=card.gameObject.AddComponent<LobbyButton>();action.lobby=this;action.action=LobbyAction.Levels;
  var shade=Rect(card,"Level number backing",34,22,0,21).gameObject.AddComponent<Image>();shade.color=new Color(.002f,.02f,.065f,.9f);shade.raycastTarget=false;
  Text(card,"LVL","LVL",7,34,10,0,27,Cyan,true);Text(card,"Level number",Mathf.Clamp(MenuUnlocked,1,100).ToString(),12,34,16,0,17,White,true);
  if(popupRoot)popupRoot.SetAsLastSibling();if(toastLabel)toastLabel.transform.SetAsLastSibling();
 }
 void BuildArchipelagoPage(Transform root){
  var panel=At(root,"Archipelago panel",458,584,270,491).gameObject.AddComponent<LobbyPlate>();panel.clean=true;panel.primary=true;panel.accent=Cyan;
  Label(root,"АРХИПЕЛАГ",27,270,245,380,44,White,true);
  Label(root,"Выбери путь до следующего острова",15,270,291,398,32,new Color(.64f,.8f,.94f));
  for(int i=0;i<20;i++){
   int id=levelPage*20+i;if(id>=MenuData.levels.Length)continue;bool open=id<MenuUnlocked;int stars=MenuStars(id+1);bool current=id==MenuUnlocked-1;
   var button=SkinButton(root,"Level "+(id+1),LobbyAction.SelectLevel,86,57,119+(i%4)*101,354+(i/4)*68,(id+1).ToString(),20,current?Cyan:new Color(.12f,.45f,.62f),id);button.GetComponent<Button>().interactable=open;button.GetComponent<LobbyPlate>().primary=current;
   if(stars>0){button.GetComponentInChildren<Text>().rectTransform.anchoredPosition=new Vector2(-12,0);Icon(button.transform,LobbyIcon.Star,20,24,0,Gold);}
   else if(!open){button.GetComponentInChildren<Text>().rectTransform.anchoredPosition=new Vector2(-12,0);SkinLock(Rect(button.transform,"Locked level",15,19,26,0));}
  }
  var prev=SkinButton(root,"Previous levels",LobbyAction.PreviousPage,48,44,99,724,"‹",32,Cyan);prev.GetComponent<Button>().interactable=levelPage>0;
  prev.GetComponentInChildren<Text>().verticalOverflow=VerticalWrapMode.Overflow;
  SkinButton(root,"Return to menu",LobbyAction.Close,239,44,270,724,"ВЕРНУТЬСЯ",18,Cyan);
  var next=SkinButton(root,"Next levels",LobbyAction.NextPage,48,44,441,724,"›",32,Cyan);next.GetComponent<Button>().interactable=levelPage<(MenuData.levels.Length-1)/20;
  next.GetComponentInChildren<Text>().verticalOverflow=VerticalWrapMode.Overflow;
 }
}
