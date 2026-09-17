using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections.Generic;

// Explicit asset integration; never runs on scene load or during ordinary builds.
public static class MenuPanelBuild {
 const string Atlas="Assets/Art/UI/panel.png",Folder="Assets/Art/UI/MenuPanels";
 static readonly Dictionary<string,Sprite> sprites=new();
 static Sprite Slice(string name,float x,float y,float width,float height,Vector4 border){
  const float scale=2172f/2048;var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Atlas);
  var rect=new Rect(Mathf.Round(x*scale),Mathf.Round((683-y-height)*scale),Mathf.Round(width*scale),Mathf.Round(height*scale));
  rect.y=Mathf.Max(0,rect.y);rect.width=Mathf.Min(rect.width,texture.width-rect.x);rect.height=Mathf.Min(rect.height,texture.height-rect.y);
  var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,border*scale,false);sprite.name=name;
  string path=Folder+"/"+name+".asset";if(File.Exists(path))throw new System.Exception("Panel sprite already exists; not overwriting: "+path);
  AssetDatabase.CreateAsset(sprite,path);sprites.Add(name,sprite);return sprite;
 }
 public static void ImportAndApply(){
  if(Directory.Exists(Folder))throw new System.Exception("Menu panel assets already exist. Edit saved sprites and scene instead of reapplying.");
  Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
  var importer=(TextureImporter)AssetImporter.GetAtPath(Atlas);importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;
  foreach(string platform in new[]{"Standalone","Android"}){var p=importer.GetPlatformTextureSettings(platform);p.overridden=true;p.maxTextureSize=4096;p.format=TextureImporterFormat.RGBA32;p.textureCompression=TextureImporterCompression.Uncompressed;importer.SetPlatformTextureSettings(p);}importer.SaveAndReimport();
  Slice("Wallet - crystals",28,8,844,243,new Vector4(207,57,190,65));
  Slice("Wallet - resonance",873,10,712,235,new Vector4(185,50,184,61));
  Slice("Settings",1830,26,197,199,Vector4.one*35);
  Slice("Gift - original with badge",1613,23,215,205,Vector4.one*35);
  Slice("Gift icon",1680,98,81,76,Vector4.zero);
  Slice("Notification",1751,35,67,69,Vector4.zero);
  Slice("Collection",1584,218,437,157,new Vector4(145,42,62,43));
  Slice("Statistics",1584,365,437,153,new Vector4(145,43,62,42));
  Slice("Shop",1584,512,437,163,new Vector4(145,49,62,43));
  Slice("Wide panel",24,244,1545,438,new Vector4(126,94,126,80));
  AssetDatabase.SaveAssets();
  var scene=EditorSceneManager.OpenScene(SeparateMenuBuild.MenuPath);var lobby=Object.FindFirstObjectByType<PulseLobby>();
  LobbyPlate Apply(string name,string sprite,bool sliced=true){var plate=lobby.safeRoot.Find(name).GetComponent<LobbyPlate>();plate.sprite=sprites[sprite];plate.type=sliced?Image.Type.Sliced:Image.Type.Simple;plate.preserveAspect=!sliced;plate.pixelsPerUnitMultiplier=plate.sprite.rect.height/Mathf.Max(1,plate.rectTransform.rect.height);plate.color=Color.white;plate.SetAllDirty();return plate;}
  void HideGlyph(Transform root){foreach(var glyph in root.GetComponentsInChildren<LobbyGlyph>(true))glyph.gameObject.SetActive(false);}
  Apply("Settings","Settings",false);HideGlyph(lobby.safeRoot.Find("Settings"));
  var gift=Apply("Gift","Settings");gift.fillCenter=false;HideGlyph(gift.transform);
  var icon=new GameObject("Gift artwork • atlas",typeof(RectTransform)).AddComponent<Image>();icon.transform.SetParent(gift.transform,false);icon.sprite=sprites["Gift icon"];icon.preserveAspect=true;icon.raycastTarget=false;icon.rectTransform.sizeDelta=new Vector2(37,35);
  var badge=lobby.giftBadge.GetComponent<Image>();badge.sprite=sprites["Notification"];badge.color=Color.white;badge.preserveAspect=true;badge.rectTransform.sizeDelta=new Vector2(23,24);badge.rectTransform.anchoredPosition=new Vector2(23,23);badge.transform.SetAsLastSibling();
  Apply("Profile","Wide panel");Apply("Play • primary action","Wide panel");Apply("Daily reward banner","Wide panel");
  var walletA=Apply("Crystal wallet","Wallet - crystals");var walletB=Apply("Resonance wallet","Wallet - resonance");
  foreach(var wallet in new[]{walletA,walletB}){
   var glyph=wallet.GetComponentInChildren<NeonGraphic>();glyph.rectTransform.anchoredPosition=new Vector2(-wallet.rectTransform.rect.width*.5f+36,0);glyph.rectTransform.sizeDelta=Vector2.one*25;
   foreach(var button in wallet.GetComponentsInChildren<LobbyButton>(true))if(button.transform!=wallet.transform){var p=button.GetComponent<LobbyPlate>();p.hitAreaOnly=true;HideGlyph(button.transform);}
  }
  var left=new[]{"События","Задания","Магазин"};var right=new[]{"Коллекция","Статистика","Скины"};
  for(int i=0;i<3;i++)foreach(var name in new[]{left[i],right[i]}){
   bool isLeft=left.Contains(name);string art=name=="Коллекция"?"Collection":name=="Статистика"?"Statistics":name=="Магазин"?"Shop":"Wide panel";
   var plate=Apply(name,art);var r=plate.rectTransform;r.anchorMin=r.anchorMax=new Vector2(isLeft?0:1,1);r.anchoredPosition=new Vector2(isLeft?80:-80,-352-i*80);r.sizeDelta=new Vector2(150,60);plate.pixelsPerUnitMultiplier=plate.sprite.rect.height/60;
   var label=plate.GetComponentInChildren<Text>();label.fontSize=12;label.rectTransform.sizeDelta=new Vector2(87,34);label.rectTransform.anchoredPosition=new Vector2(13,0);
   if(art!="Wide panel")HideGlyph(plate.transform);else {var glyph=plate.GetComponentInChildren<LobbyGlyph>();glyph.rectTransform.anchoredPosition=new Vector2(-49,0);glyph.rectTransform.sizeDelta=Vector2.one*30;}
  }
  EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("MENU_PANEL_ATLAS_APPLIED");SeparateMenuBuild.BuildExisting();
 }
}
