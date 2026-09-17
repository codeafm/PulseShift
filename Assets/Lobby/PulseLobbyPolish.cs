using UnityEngine;
using UnityEngine.UI;

public partial class PulseLobby {
 // Created once and saved by the editor bake. Subsequent launches preserve authored geometry.
 public void EnsurePolishedHeader(){
  if(!standaloneScene||!safeRoot||safeRoot.Find("Header • refined glass"))return;
  foreach(var name in new[]{"Profile","Crystal wallet","Resonance wallet","Add crystals","Add resonance"}){
   var old=safeRoot.Find(name);if(old)old.gameObject.SetActive(false);
  }
  var header=Rect(safeRoot,"Header • refined glass",496,76,0,-56);header.anchorMin=header.anchorMax=new Vector2(.5f,1);
  var shell=header.gameObject.AddComponent<LobbyPlate>();shell.clean=true;shell.raycastTarget=false;shell.accent=new Color(.13f,.48f,.7f);shell.top=new Color(.025f,.07f,.14f,.95f);shell.bottom=new Color(.008f,.019f,.045f,.96f);
  var profile=HeaderAction(header,"Player profile",LobbyAction.Profile,211,66,-137,0);
  var portrait=Rect(profile,"Avatar medallion",49,49,-75,0).gameObject.AddComponent<LobbyPlate>();portrait.clean=true;portrait.accent=Cyan;portrait.raycastTarget=false;
  Icon(portrait.transform,LobbyIcon.Hero,36,0,1,Cyan);
  nameLabel=Text(profile,"Player name","Искра",20,139,26,28,13,White,true);nameLabel.alignment=TextAnchor.MiddleLeft;nameLabel.resizeTextForBestFit=true;nameLabel.resizeTextMinSize=13;nameLabel.resizeTextMaxSize=20;
  levelLabel=Text(profile,"Profile level","Уровень 1",12,139,20,28,-10,new Color(.5f,.69f,.82f));levelLabel.alignment=TextAnchor.MiddleLeft;
  var track=Rect(profile,"Progress track",124,4,20,-25).gameObject.AddComponent<Image>();track.color=new Color(.05f,.15f,.22f);track.raycastTarget=false;
  progressFill=Rect(track.transform,"Chapter progress",24,4,0,0).gameObject.AddComponent<Image>();progressFill.color=Cyan;progressFill.raycastTarget=false;
  crystalLabel=HeaderWallet(header,"Crystal balance",LobbyAction.Crystals,43,Gold,NeonSymbol.Crystal);
  resonanceLabel=HeaderWallet(header,"Resonance balance",LobbyAction.Resonance,177,Cyan,NeonSymbol.Pulse);
  header.SetSiblingIndex(Mathf.Min(2,safeRoot.childCount-1));
 }
 RectTransform HeaderAction(Transform parent,string name,LobbyAction action,float width,float height,float x,float y){
  var r=Rect(parent,name,width,height,x,y);var hit=r.gameObject.AddComponent<LobbyPlate>();hit.hitAreaOnly=true;
  var button=r.gameObject.AddComponent<Button>();button.targetGraphic=hit;button.transition=Selectable.Transition.None;
  var handler=r.gameObject.AddComponent<LobbyButton>();handler.lobby=this;handler.action=action;return r;
 }
 Text HeaderWallet(Transform parent,string name,LobbyAction action,float x,Color tint,NeonSymbol symbol){
  var r=HeaderAction(parent,name,action,126,54,x,0);var plate=r.GetComponent<LobbyPlate>();plate.hitAreaOnly=false;plate.clean=true;plate.accent=new Color(.1f,.29f,.42f);plate.top=new Color(.018f,.055f,.105f);plate.bottom=new Color(.008f,.024f,.05f);
  Neon(r,symbol,25,-43,0,tint);var label=Text(r,"Balance","0",20,62,30,-1,0,White,true);
  label.resizeTextForBestFit=true;label.resizeTextMinSize=12;label.resizeTextMaxSize=20;
  Icon(r,LobbyIcon.Plus,15,47,0,new Color(.42f,.72f,.85f));
  return label;
 }
}
