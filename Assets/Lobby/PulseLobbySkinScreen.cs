using UnityEngine;
using UnityEngine.UI;

public partial class PulseLobby {
 Transform skinContent;SkinCarousel skinCarousel;
 LobbyButton SkinButton(Transform root,string name,LobbyAction action,float w,float h,float x,float y,string label,int size,Color accent,int value=0){
  var b=Button(root,name,action,w,h,x,y,label,size,accent,value);var plate=b.GetComponent<LobbyPlate>();plate.clean=true;plate.top=new Color(.025f,.065f,.13f,.88f);plate.bottom=new Color(.004f,.014f,.035f,.93f);plate.SetVerticesDirty();return b;
 }
 void SkinLock(Transform root){
  void Bar(string name,float w,float h,float x,float y,Color c){var g=Rect(root,name,w,h,x,y).gameObject.AddComponent<Image>();g.color=c;g.raycastTarget=false;}
  Bar("Shackle top",12,3,0,9,White);Bar("Shackle left",3,10,-6,5,White);Bar("Shackle right",3,10,6,5,White);
  Bar("Lock body",21,16,0,-5,White);Bar("Keyhole",3,6,0,-5,new Color(.01f,.03f,.08f));
 }
 void BuildSkinScreen(Transform page){
  var background=Rect(page,"Skin screen • full bleed background",540,960,0,0).gameObject.AddComponent<RawImage>();
  var backdrop=stage&&stage.backdrop?stage.backdrop.GetComponent<Renderer>().sharedMaterial.mainTexture:null;
  background.texture=backdrop?backdrop:Resources.Load<Texture2D>("Lobby/ArchipelagoBackdrop");background.color=new Color(.8f,.86f,1,1);background.raycastTarget=true;
  background.gameObject.AddComponent<ScreenBackdropColor>();
  var content=Rect(page,"Skin screen • 540 x 960 composition",540,960,0,0);
  var screen=page.gameObject.AddComponent<SkinScreenLayout>();screen.Initialize(canvas,content,background);
  var root=(Transform)content;skinContent=root;
  var close=SkinButton(root,"Back to menu",LobbyAction.Close,40,40,34,42,"‹",36,Cyan);
  close.GetComponentInChildren<Text>().verticalOverflow=VerticalWrapMode.Overflow;
  var heroTab=SkinButton(root,"Hero category",LobbyAction.SkinHeroTab,130,64,135,56,"НАШ МОДЕЛЬ",13,skinPortalTab?new Color(.2f,.36f,.55f):Gold);heroTab.GetComponent<LobbyPlate>().primary=!skinPortalTab;
  var heroIcon=Icon(heroTab.transform,LobbyIcon.Hero,28,0,13,skinPortalTab?Cyan:Gold);heroTab.GetComponentInChildren<Text>().rectTransform.anchoredPosition=new Vector2(0,-19);
  heroTab.GetComponentInChildren<Text>().rectTransform.sizeDelta=new Vector2(124,24);
  var portalTab=SkinButton(root,"Portal category",LobbyAction.SkinPortalTab,125,64,277,56,"ПОРТАЛ",13,skinPortalTab?Gold:new Color(.2f,.36f,.55f));portalTab.GetComponent<LobbyPlate>().primary=skinPortalTab;
  Neon(portalTab.transform,NeonSymbol.Pulse,28,0,13,Cyan);portalTab.GetComponentInChildren<Text>().rectTransform.anchoredPosition=new Vector2(0,-19);
  portalTab.GetComponentInChildren<Text>().rectTransform.sizeDelta=new Vector2(119,24);
  var wallet=SkinButton(root,"Skin crystals wallet",LobbyAction.Crystals,162,37,446,30,Profile.crystals.ToString("N0"),17,new Color(.22f,.39f,.57f));Neon(wallet.transform,NeonSymbol.Crystal,25,-57,0,Gold);Icon(wallet.transform,LobbyIcon.Plus,23,62,0,Cyan);
  wallet=SkinButton(root,"Skin resonance wallet",LobbyAction.Resonance,162,37,446,74,Profile.resonance.ToString("N0"),17,new Color(.22f,.39f,.57f));Neon(wallet.transform,NeonSymbol.Pulse,25,-57,0,Purple);Icon(wallet.transform,LobbyIcon.Plus,23,62,0,Cyan);
  var colors=skinPortalTab?PortalColors:SkinColors;var names=skinPortalTab?PortalNames:SkinNames;
  var descriptions=skinPortalTab?PortalDescriptions:SkinDescriptions;var prices=skinPortalTab?PulseProfile.PortalPrices:PulseProfile.SkinPrices;
  int owned=skinPortalTab?Profile.ownedPortals:Profile.ownedSkins,selected=skinPortalTab?Profile.selectedPortal:Profile.selectedSkin;
  skinPreviewIndex=Mathf.Clamp(skinPreviewIndex,0,4);bool unlocked=(owned&(1<<skinPreviewIndex))!=0;Color accent=colors[skinPreviewIndex];
  // Transparent render sits over the same full-screen castle vista as the reference.
  var preview=At(root,"Live model • swipe to browse",516,414,270,483);preview.gameObject.AddComponent<RectMask2D>();
  skinCarousel=preview.gameObject.AddComponent<SkinCarousel>();skinCarousel.Initialize(this,skinPortalTab,skinPreviewIndex);
  skinCarousel.Changed=(category,id)=>{PlaySkinSwipe();bool changedCategory=skinPortalTab!=category;skinPortalTab=category;skinPreviewIndex=id;RefreshSkinScreen(changedCategory);};
  skinCarousel.Settled=()=>RefreshSkinScreen(false);
  var title=Label(root,names[skinPreviewIndex],25,270,176,490,40,White,true);title.name="Skin title";title.alignment=TextAnchor.MiddleLeft;
  var rarity=Label(root,new[]{"ОБЫЧНЫЙ","РЕДКИЙ","ЭПИЧЕСКИЙ","РЕДКИЙ","ЛЕГЕНДАРНЫЙ"}[skinPreviewIndex],16,270,210,490,28,new Color(.48f,.66f,.87f));rarity.name="Skin rarity";rarity.alignment=TextAnchor.MiddleLeft;
  var desc=Label(root,descriptions[skinPreviewIndex]+(skinPreviewIndex==0?"\nНачало большого пути.":""),15,270,251,490,53,new Color(.56f,.72f,.89f));desc.name="Skin description";desc.alignment=TextAnchor.UpperLeft;
  var previous=SkinButton(root,"Previous skin",LobbyAction.SkinPrevious,48,90,39,474,"‹",66,Cyan);previous.GetComponent<LobbyPlate>().hitAreaOnly=true;
  var next=SkinButton(root,"Next skin",LobbyAction.SkinNext,48,90,501,474,"›",66,Cyan);next.GetComponent<LobbyPlate>().hitAreaOnly=true;
  BuildSkinCards();
  bool usesResonance=skinPortalTab?skinPreviewIndex==3:skinPreviewIndex==4;
  string offer=unlocked?(selected==skinPreviewIndex?"✓   ВЫБРАНО":"ВЫБРАТЬ"):"ОТКРЫТЬ  ·  "+prices[skinPreviewIndex]+(usesResonance?" РЕЗОНАНСА":" КРИСТАЛЛОВ");
  var buy=SkinButton(root,"Skin primary action",skinPortalTab?LobbyAction.BuyPortal:LobbyAction.BuySkin,278,54,270,881,offer,unlocked?20:14,Cyan,skinPreviewIndex);
  // Keep the selected state bright, as in the reference, while the profile method is idempotent.
  Label(root,"Свайпните, чтобы сменить облик",11,270,930,450,22,new Color(.42f,.61f,.79f));
  RefreshSkinScreen();
 }

 void SelectSkinPreview(bool category,int id,int direction=1){
  if(!skinCarousel||!skinContent||!skinContent.gameObject.activeInHierarchy){skinPortalTab=category;skinPreviewIndex=id;ShowPage("Скины");return;}
  skinCarousel.Select(category,id,direction);
 }
 void BuildSkinCards(){
  var old=skinContent.Find("Skin cards");if(old){old.gameObject.SetActive(false);if(Application.isPlaying)Destroy(old.gameObject);else DestroyImmediate(old.gameObject);}
  var row=Rect(skinContent,"Skin cards",540,960,0,0);var colors=skinPortalTab?PortalColors:SkinColors;
  for(int i=0;i<5;i++){
   var card=SkinButton(row,"Skin card "+i,LobbyAction.PreviewSkin,99,145,57+i*106,754,null,14,colors[i],i);
   var thumb=Rect(card.transform,"Model thumbnail",91,106,0,16).gameObject.AddComponent<RawImage>();thumb.raycastTarget=false;
   thumb.gameObject.AddComponent<SkinShowcase>().Initialize(this,thumb,skinPortalTab,i,colors[i],true);
   var lockRect=Rect(card.transform,"Lock",22,27,0,-21);SkinLock(lockRect);
   var strip=Rect(card.transform,"Price background",91,30,0,-53).gameObject.AddComponent<Image>();strip.color=new Color(0,.01f,.035f,.86f);strip.raycastTarget=false;
   var icon=Neon(card.transform,NeonSymbol.Crystal,17,-31,-53,Gold);icon.name="Price icon";
   Text(card.transform,"Card status","",14,60,28,10,-54,White);
  }
 }
 void RefreshSkinScreen(bool categoryChanged=false){
  if(!skinContent)return;if(categoryChanged)BuildSkinCards();
  var colors=skinPortalTab?PortalColors:SkinColors;var names=skinPortalTab?PortalNames:SkinNames;var descriptions=skinPortalTab?PortalDescriptions:SkinDescriptions;
  int owned=skinPortalTab?Profile.ownedPortals:Profile.ownedSkins,selected=skinPortalTab?Profile.selectedPortal:Profile.selectedSkin;
  var prices=skinPortalTab?PulseProfile.PortalPrices:PulseProfile.SkinPrices;
  skinContent.Find("Skin title").GetComponent<Text>().text=names[skinPreviewIndex];
  skinContent.Find("Skin rarity").GetComponent<Text>().text=new[]{"ОБЫЧНЫЙ","РЕДКИЙ","ЭПИЧЕСКИЙ","РЕДКИЙ","ЛЕГЕНДАРНЫЙ"}[skinPreviewIndex];
  skinContent.Find("Skin description").GetComponent<Text>().text=descriptions[skinPreviewIndex]+(skinPreviewIndex==0?"\nНачало большого пути.":"");
  foreach(var pair in new[]{("Hero category",!skinPortalTab),("Portal category",skinPortalTab)}){
   var plate=skinContent.Find(pair.Item1).GetComponent<LobbyPlate>();plate.primary=pair.Item2;plate.accent=pair.Item2?Gold:new Color(.2f,.36f,.55f);plate.SetVerticesDirty();
  }
  skinLockVisual=null;
  for(int i=0;i<5;i++){
   var card=skinContent.Find("Skin cards/Skin card "+i);var plate=card.GetComponent<LobbyPlate>();plate.primary=i==skinPreviewIndex;plate.accent=plate.primary?colors[i]:new Color(.17f,.29f,.43f);plate.SetVerticesDirty();
   bool available=(owned&(1<<i))!=0;var lockRect=(RectTransform)card.Find("Lock");lockRect.gameObject.SetActive(!available);if(!available&&i==skinPreviewIndex)skinLockVisual=lockRect;
   var status=card.Find("Card status").GetComponent<Text>();status.text=selected==i?"✓":available?"ОТКРЫТ":prices[i].ToString("N0");status.fontSize=selected==i?26:available?11:14;status.color=selected==i?Cyan:White;
   status.rectTransform.anchoredPosition=new Vector2(available?0:10,-54);
   var icon=card.Find("Price icon").GetComponent<NeonGraphic>();icon.gameObject.SetActive(!available);bool resonance=skinPortalTab?i==3:i==4;icon.symbol=resonance?NeonSymbol.Pulse:NeonSymbol.Crystal;icon.accent=resonance?Purple:Gold;icon.SetVerticesDirty();
  }
  bool unlocked=(owned&(1<<skinPreviewIndex))!=0,usesResonance=skinPortalTab?skinPreviewIndex==3:skinPreviewIndex==4;
  var buy=skinContent.Find("Skin primary action").GetComponent<LobbyButton>();buy.action=skinPortalTab?LobbyAction.BuyPortal:LobbyAction.BuySkin;buy.value=skinPreviewIndex;
  buy.GetComponent<Button>().interactable=!skinCarousel.IsMoving;
  var label=buy.GetComponentInChildren<Text>();label.fontSize=unlocked?20:14;
  label.text=unlocked?(selected==skinPreviewIndex?"✓   ВЫБРАНО":"ВЫБРАТЬ"):"ОТКРЫТЬ  ·  "+prices[skinPreviewIndex]+(usesResonance?" РЕЗОНАНСА":" КРИСТАЛЛОВ");
  skinContent.Find("Skin crystals wallet").GetComponentInChildren<Text>().text=Profile.crystals.ToString("N0");
  skinContent.Find("Skin resonance wallet").GetComponentInChildren<Text>().text=Profile.resonance.ToString("N0");
 }
}
