using UnityEngine;
using UnityEngine.UI;

public partial class PulseLobby {
 Rect lastLayoutSafe;Vector2 lastLayoutScreen;bool layoutReady;
 public float LayoutWidth=>safeRoot?safeRoot.rect.width:540;
 public float LayoutHeight=>safeRoot?safeRoot.rect.height:960;
 public Vector2 LayoutPoint(float x,float y)=>RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,safeRoot.TransformPoint(new Vector3(x-LayoutWidth*.5f,LayoutHeight*.5f-y,0)));
 RectTransform Place(string name,float x,float y,float width=-1,float height=-1){
  var r=safeRoot.Find(name) as RectTransform;if(!r)return null;
  r.anchoredPosition=new Vector2(x-LayoutWidth*.5f,LayoutHeight*.5f-y);
  if(width>0||height>0)r.sizeDelta=new Vector2(width>0?width:r.sizeDelta.x,height>0?height:r.sizeDelta.y);return r;
 }
 public void Layout(){
  if(!canvas||!safeRoot)return;
  if(standaloneScene){
   var authoredSafe=MobileViewport.SafeArea;var authoredScreen=new Vector2(Screen.width,Screen.height);
   if(layoutReady&&lastLayoutSafe==authoredSafe&&lastLayoutScreen==authoredScreen){stage.ResizeIfNeeded();return;}
   layoutReady=true;lastLayoutSafe=authoredSafe;lastLayoutScreen=authoredScreen;
   // Saved anchors, sizes and offsets are the source of truth for this scene.
   MobileViewport.LayoutAuthored(safeRoot,canvas);
   // The page container is optional in a manually edited menu; its absence must not stop background fitting.
   if(popupRoot){float scale=Mathf.Min(1,Mathf.Min(LayoutWidth/540,LayoutHeight/960));popupRoot.localScale=Vector3.one*scale;
    foreach(Transform page in popupRoot)if(page is RectTransform rect)rect.sizeDelta=new Vector2(LayoutWidth,LayoutHeight)/scale;}
   stage.ResizeIfNeeded();return;
  }
  MobileViewport.Layout(safeRoot,canvas);
  var size=new Vector2(Screen.width,Screen.height);var safe=MobileViewport.SafeArea;
  if(layoutReady&&lastLayoutSafe==safe&&lastLayoutScreen==size){stage.ResizeIfNeeded();return;}
  layoutReady=true;lastLayoutSafe=safe;lastLayoutScreen=size;
  LayoutControls(LayoutWidth,LayoutHeight);
 }
 void LayoutControls(float w,float h){
  Place("Living stardust",w*.5f,h*.5f,w,h);
  Place("Settings",44,44,68,68);Place("Gift",w-44,44,68,68);
  var profile=Place("Profile",w*.5f,44,w-184,68);
  if(profile){var icon=profile.GetComponentInChildren<LobbyGlyph>();if(icon)icon.rectTransform.anchoredPosition=new Vector2(-profile.rect.width*.5f+34,0);
   nameLabel.rectTransform.sizeDelta=levelLabel.rectTransform.sizeDelta=new Vector2(profile.rect.width-82,25);nameLabel.fontSize=20;levelLabel.fontSize=14;
   var track=profile.Find("Progress track") as RectTransform;track.sizeDelta=new Vector2(profile.rect.width-88,4);
  }
  float wallet=(w-38)*.5f;
  var crystal=Place("Crystal wallet",w*.25f+2,124,wallet,68);var resonance=Place("Resonance wallet",w*.75f-2,124,wallet,68);
  Place("Add crystals",crystal.anchoredPosition.x+w*.5f+wallet*.5f-34,124,62,62);
  Place("Add resonance",resonance.anchoredPosition.x+w*.5f+wallet*.5f-34,124,62,62);
  crystal.GetComponentInChildren<NeonGraphic>().rectTransform.anchoredPosition=new Vector2(-wallet*.5f+29,0);
  resonance.GetComponentInChildren<NeonGraphic>().rectTransform.anchoredPosition=new Vector2(-wallet*.5f+29,0);
  crystalLabel.fontSize=resonanceLabel.fontSize=23;crystalLabel.rectTransform.sizeDelta=resonanceLabel.rectTransform.sizeDelta=new Vector2(wallet-114,40);
  crystalLabel.rectTransform.anchoredPosition=resonanceLabel.rectTransform.anchoredPosition=new Vector2(-7,0);
  Place("Orbit • animated logo",w*.5f,239,154,154);
  foreach(Transform child in safeRoot){var label=child.GetComponent<Text>();if(!label)continue;if(label.text.StartsWith("P U L S E")){Place(child.name,w*.5f,219,w-24,50);}else if(label.text.StartsWith("M O V E"))Place(child.name,w*.5f,254,w-70,25);}
  float sideY=Mathf.Lerp(330,352,Mathf.InverseLerp(840,1170,h));
  var left=new[]{"События","Задания","Магазин"};var right=new[]{"Коллекция","Статистика","Скины"};
  for(int i=0;i<3;i++)foreach(var entry in new[]{(left[i],48f),(right[i],w-48f)}){var r=Place(entry.Item1,entry.Item2,sideY+i*101,90,92);if(r){var label=r.GetComponentInChildren<Text>();label.fontSize=13;label.rectTransform.anchoredPosition=new Vector2(0,-24);label.rectTransform.sizeDelta=new Vector2(84,22);var icon=r.GetComponentInChildren<LobbyGlyph>();if(icon){icon.rectTransform.sizeDelta=new Vector2(44,44);icon.rectTransform.anchoredPosition=new Vector2(0,13);}}}
  Place("Play • primary action",w*.5f,h-160,w-54,102);
  Place("Daily reward banner",w*.5f,h-55,w-34,76);
  // Modals retain their readable 540x960 design; only their uniform scale changes on short devices.
  float modalScale=Mathf.Min(1,Mathf.Min(w/540,h/960));popupRoot.localScale=Vector3.one*modalScale;
  foreach(Transform page in popupRoot)if(page is RectTransform rect)rect.sizeDelta=new Vector2(w,h)/modalScale;
  Place("Toast • action feedback",w*.5f,h-228,w-70,42);
  Place("Tap the living hero",w*.4f,h-390,184,240);
  RefreshHeader();Canvas.ForceUpdateCanvases();stage.Frame();
 }
}
