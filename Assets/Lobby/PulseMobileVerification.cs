using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public partial class PulseLobby {
 [Serializable] class MobileProfileResult {public string name;public int width,height,levelsChecked;public Rect safeArea;public float heroHeightFraction,portalHeightFraction;}
 [Serializable] class MobileReport {public bool success;public int checks,buttonRaycasts;public bool physicalDeviceTested=false;public string method="Unity runtime with simulated safe insets; native cameras + Canvas offscreen renders; not physical Android GPU testing";public MobileProfileResult[] profiles;public string[] failures;}
 Rect ButtonPixels(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(null,corners[2]);return UnityEngine.Rect.MinMaxRect(a.x,a.y,b.x,b.y);}
 void VerifySafeButtons(){
  var safe=MobileViewport.SafeArea;
  foreach(var b in canvas.GetComponentsInChildren<LobbyButton>()){
   if(!b.gameObject.activeInHierarchy||Page!=""&&!b.transform.IsChildOf(popupRoot))continue;
   var rect=ButtonPixels((RectTransform)b.transform);
   Verify(rect.xMin>=safe.xMin-2&&rect.xMax<=safe.xMax+2&&rect.yMin>=safe.yMin-2&&rect.yMax<=safe.yMax+2,"Button crosses safe area: "+b.name);
  }
 }
 public IEnumerator VerifyMobile(){
  string profileSave=PlayerPrefs.GetString("PulseLobby.Profile.v1","");string progress=PlayerPrefs.GetInt("PulseCampaign.Variety2.Current",1)+"/"+PlayerPrefs.GetInt("PulseCampaign.Variety2.Unlocked",1);
  var position=game.gameCamera.transform.position;var rotation=game.gameCamera.transform.rotation;float fov=game.gameCamera.fieldOfView;
  var variants=new[]{("Galaxy-S24",432,936,.023f,.016f), ("Phone-20x9",396,880,.038f,.028f), ("Phone-notch",414,896,.058f,.042f), ("Phone-16x9",450,800,.018f,.018f), ("Tablet-4x3",648,864,.026f,.025f), ("Fold-cover-22x9",378,924,.027f,.03f)};
  var reports=new List<MobileProfileResult>();string folder=Path.GetFullPath(Application.dataPath+"/../../MobileQA");Directory.CreateDirectory(folder);
  foreach(var variant in variants){
   ClosePopup();MobileViewport.TestSafeArea=null;Screen.SetResolution(variant.Item2,variant.Item3,FullScreenMode.Windowed);yield return new WaitForSeconds(.4f);
   Verify(Screen.width==variant.Item2&&Screen.height==variant.Item3,"Host clamped requested resolution: "+variant.Item1);
   MobileViewport.TestSafeArea=new Rect(0,Screen.height*variant.Item5,Screen.width,Screen.height*(1-variant.Item4-variant.Item5));
   Open();yield return new WaitForSeconds(.5f);Layout();Canvas.ForceUpdateCanvases();VerifyExclusiveScreen(true);VerifyPageRays();VerifySafeButtons();
   var safe=MobileViewport.SafeArea;var heroBounds=stage.ModelBounds(stage.heroGroup);var portalBounds=stage.ModelBounds(stage.portalGroup);
   float heroSize=heroBounds.height/safe.height,portalSize=portalBounds.height/safe.height;
   Verify(heroSize>.29f&&portalSize>.18f,"Models too small on "+variant.Item1);
   Verify(heroBounds.xMin>=safe.xMin-2&&heroBounds.xMax<=safe.xMax+2&&heroBounds.yMin>=safe.yMin&&heroBounds.yMax<=safe.yMax,"Hero clipped on "+variant.Item1);
   foreach(var root in new[]{stage.heroGroup,stage.portalGroup,stage.bridgeA,stage.bridgeB})if(root.gameObject.activeInHierarchy)Verify(stage.ModelBounds(root).yMax<=LayoutPoint(LayoutWidth*.5f,296).y,"Decorative model overlaps logo: "+variant.Item1+" / "+root.name);
   var play=FindButton(LobbyAction.Play);var gift=FindButton(LobbyAction.Gift);var playBounds=ButtonPixels((RectTransform)play.transform);var giftBounds=ButtonPixels((RectTransform)gift.transform);
   Verify(heroBounds.yMin>=playBounds.yMax-2,"Hero overlaps Play on "+variant.Item1);Verify(giftBounds.yMin-safe.yMin<safe.width*.08f,"Footer floats above bottom on "+variant.Item1);
   var bg=ScreenComposition.Bounds(stage.backdrop,stage.view,ScreenComposition.Geometry(stage.backdrop));Verify(bg.xMin<=0&&bg.yMin<=0&&bg.xMax>=Screen.width&&bg.yMax>=Screen.height,"Background does not cover full display");
   var stable=heroBounds.size;for(int i=0;i<5;i++){stage.Frame();yield return null;}Verify(Vector2.Distance(stable,stage.ModelBounds(stage.heroGroup).size)<1,"Menu models shrink on repeated framing");
   var mist=stage.view.GetComponent<PulseAtmosphere>();float clock=mist.Clock;yield return new WaitForSeconds(.12f);Verify(mist.Clock>clock,"Menu mist is static");Profile.reducedMotion=true;ApplySettings();clock=mist.Clock;yield return new WaitForSeconds(.12f);Verify(Mathf.Approximately(clock,mist.Clock),"Reduced motion does not stop mist");Profile.reducedMotion=false;ApplySettings();
   yield return new WaitForEndOfFrame();var shot=CaptureFrame(stage.view,canvas,variant.Item1=="Galaxy-S24"?1080:0,variant.Item1=="Galaxy-S24"?2340:0);File.WriteAllBytes(Path.Combine(folder,variant.Item1+"-menu.png"),ImageConversion.EncodeToPNG(shot));Destroy(shot);yield return new WaitForSeconds(.2f);
   Press(LobbyAction.Settings);yield return new WaitForSeconds(.2f);VerifyPageRays();VerifySafeButtons();Press(LobbyAction.Close);
   Press(LobbyAction.Skins);yield return new WaitForSeconds(.2f);VerifyPageRays();VerifySafeButtons();Press(LobbyAction.Close);
   Hide();game.VerificationUnlock(100);int levels=0;
   for(int id=0;id<game.Data.levels.Length;id++){
    game.BeginLobbyLevel(id);yield return null;Physics.SyncTransforms();var area=MobileViewport.BoardPixels;
    var board=game.ActiveBoardForVerification;
    foreach(var island in board.GetComponentsInChildren<CampaignIsland>()){
     var screen=game.gameCamera.WorldToScreenPoint(island.transform.position);
     Verify(area.Contains(screen),"Island outside gameplay area: "+variant.Item1+" level "+(id+1));
     Verify(Physics.Raycast(game.gameCamera.ScreenPointToRay(screen),out var hit)&&hit.collider.GetComponentInParent<CampaignIsland>()==island,"Island is not tappable: "+variant.Item1+" level "+(id+1));
    }
    levels++;
    if(id==11||id==27||id==30){yield return new WaitForEndOfFrame();var frame=CaptureFrame(game.gameCamera,game.gameplayHUD.GetComponent<Canvas>(),variant.Item1=="Galaxy-S24"?1080:0,variant.Item1=="Galaxy-S24"?2340:0);File.WriteAllBytes(Path.Combine(folder,variant.Item1+"-level"+(id+1)+".png"),ImageConversion.EncodeToPNG(frame));Destroy(frame);}
   }
   VerifyExclusiveScreen(false);reports.Add(new MobileProfileResult{name=variant.Item1,width=Screen.width,height=Screen.height,safeArea=safe,heroHeightFraction=heroSize,portalHeightFraction=portalSize,levelsChecked=levels});
   game.RequestUI(CampaignUICommand.Home);yield return null;
  }
  Verify(position==game.gameCamera.transform.position&&Quaternion.Angle(rotation,game.gameCamera.transform.rotation)<.001f&&Mathf.Abs(fov-game.gameCamera.fieldOfView)<.001f,"Mobile framing changed the authored camera");
  Verify(PlayerPrefs.GetString("PulseLobby.Profile.v1","")==profileSave&&progress==PlayerPrefs.GetInt("PulseCampaign.Variety2.Current",1)+"/"+PlayerPrefs.GetInt("PulseCampaign.Variety2.Unlocked",1),"Mobile checks modified real player saves");
  MobileViewport.TestSafeArea=null;
  var report=new MobileReport{success=lobbyFailures.Count==0,checks=lobbyChecks,buttonRaycasts=lobbyRays,profiles=reports.ToArray(),failures=lobbyFailures.ToArray()};File.WriteAllText(Path.Combine(folder,"mobile-verification.json"),JsonUtility.ToJson(report,true));Debug.Log("MOBILE_VERIFIED "+report.success+" / "+report.checks);Application.Quit(report.success?0:1);
 }
}
