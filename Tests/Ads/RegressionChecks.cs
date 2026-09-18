using System;
using UnityEngine;
using YandexMobileAds;

public static class AdsRegressionChecks {
 static int checks;
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;}
 static PulseAds Fresh(){
  Time.unscaledTime=0;Application.isEditor=false;Application.platform=RuntimePlatform.Android;
  Debug.isDebugBuild=false;AudioListener.pause=false;InterstitialAdLoader.Last=null;
  InterstitialAdLoader.Constructions=InterstitialAdLoader.Requests=0;
  return new PulseAds();
 }
 static Interstitial Ready(PulseAds ads){ads.PrepareInterstitial();var ad=new Interstitial();InterstitialAdLoader.Last.Complete(ad);return ad;}
 static CampaignGame Due(bool won){
  var game=new CampaignGame();
  for(int i=0;i<3;i++){game.NewLevel(6+i);game.Outcome(won);}
  return game;
 }
 public static int Run(){checks=0;Cadence();Loading();Lifecycle();ResultGate();return checks;}
 static void Cadence(){
  var p=new PulseProfile();
  for(int repeat=0;repeat<6;repeat++)for(int level=1;level<=5;level++){
   Check(!p.RecordInterstitialResult(level,true),"No interstitial for tutorial win");
   Check(!p.RecordInterstitialResult(level,false),"No interstitial for tutorial loss");
  }
  Check(p.interstitialWins==0&&p.interstitialLosses==0,"Tutorials do not advance either counter");
  Check(!p.RecordInterstitialResult(6,true),"First win");
  Check(!p.RecordInterstitialResult(6,false),"First loss independent of win");
  Check(!p.RecordInterstitialResult(7,true),"Second win");
  Check(!p.RecordInterstitialResult(6,false),"Second loss on same level");
  Check(p.RecordInterstitialResult(8,true),"Third win at level eight");
  Check(p.RecordInterstitialResult(6,false),"Third loss on same level");
  for(int i=1;i<=99;i++){
   Check(p.RecordInterstitialResult(6,true)==(i%3==0),"Repeated-level victory cadence");
   Check(p.RecordInterstitialResult(6,false)==(i%3==0),"Repeated-level loss cadence");
  }
  p.interstitialWins=2;p.interstitialLosses=1;
  var restored=new PulseProfile {interstitialWins=p.interstitialWins,interstitialLosses=p.interstitialLosses};
  Check(restored.RecordInterstitialResult(12,true),"Stored win position resumes at third");
  Check(!restored.RecordInterstitialResult(12,false),"Stored loss position stays independent");
  Check(restored.RecordInterstitialResult(12,false),"Stored loss position resumes at third");
  Check(p.adEligibleWins==0&&p.crystals==0&&p.resonance==0,"Interstitial does not change rewards or rewarded counter");
 }
 static void Loading(){
  var ads=Fresh();Application.isEditor=true;ads.PrepareInterstitial();
  Check(InterstitialAdLoader.Requests==0&&!ads.InterstitialReady,"Editor never requests interstitial");
  Application.isEditor=false;ads.SetForeground(false);ads.PrepareInterstitial();
  Check(InterstitialAdLoader.Requests==0,"No background request");ads.SetForeground(true);
  ads.PrepareInterstitial();Check(InterstitialAdLoader.Last.RequestedId=="R-M-20061525-3","Android production ID");
  for(int i=0;i<100;i++)ads.PrepareInterstitial();Check(InterstitialAdLoader.Requests==1,"Single in-flight load");
  InterstitialAdLoader.Last.Fail();Time.unscaledTime=29;ads.PrepareInterstitial(true);
  Check(InterstitialAdLoader.Requests==1,"New opportunity cannot bypass failure cooldown");
  Time.unscaledTime=30;ads.PrepareInterstitial();Check(InterstitialAdLoader.Requests==2,"First retry after 30 seconds");
  InterstitialAdLoader.Last.Fail();Time.unscaledTime=89;ads.PrepareInterstitial();Check(InterstitialAdLoader.Requests==2,"Second retry waits 60 seconds");
  Time.unscaledTime=90;ads.PrepareInterstitial();InterstitialAdLoader.Last.Fail();
  Time.unscaledTime=999;for(int i=0;i<100;i++)ads.PrepareInterstitial();
  Check(InterstitialAdLoader.Requests==3,"No-fill stops after three attempts");
  ads.PrepareInterstitial(true);Check(InterstitialAdLoader.Requests==4&&InterstitialAdLoader.Constructions==1,"New opportunity reuses single loader");
  var old=new Interstitial();InterstitialAdLoader.Last.Complete(old);ads.PrepareInterstitial();Check(InterstitialAdLoader.Requests==4,"Cache reused");
  Time.unscaledTime+=1800;Check(!ads.InterstitialReady,"Thirty-minute cache expires");ads.PrepareInterstitial();
  Check(old.Destroyed&&InterstitialAdLoader.Requests==5,"Expired ad destroyed before reloading");
  ads=Fresh();Application.platform=RuntimePlatform.IPhonePlayer;ads.PrepareInterstitial();
  Check(InterstitialAdLoader.Last.RequestedId=="R-M-20061555-3","iOS production ID");
  foreach(var platform in new[]{RuntimePlatform.Android,RuntimePlatform.IPhonePlayer}){
   ads=Fresh();Application.platform=platform;Debug.isDebugBuild=true;ads.PrepareInterstitial();
   Check(InterstitialAdLoader.Last.RequestedId=="demo-interstitial-yandex","Development build uses demo ID");
  }
 }
 static void Lifecycle(){
  var ads=Fresh();int callbacks=0;
  Check(!ads.ShowInterstitial(()=>callbacks++),"No ready ad must not take transition ownership");
  Check(callbacks==0&&!PulseAds.Fullscreen,"No-fill does not call continuation or freeze gameplay");
  var ad=new Interstitial();InterstitialAdLoader.Last.Complete(ad);
  Check(ads.ShowInterstitial(()=>callbacks++),"Ready ad takes transition ownership");
  Check(PulseAds.Fullscreen&&AudioListener.pause&&!ads.BannerVisible&&ad.Shows==1,"Showing locks input, mutes sound and hides banner");
  Check(!ads.ShowInterstitial(()=>callbacks++),"Second show blocked");
  ad.Dismiss();ad.Dismiss();ad.FailShow();
  Check(callbacks==1&&!PulseAds.Fullscreen&&!AudioListener.pause&&ad.Destroyed,"Duplicate callbacks resume once and restore audio");
  ads.PrepareInterstitial();Check(InterstitialAdLoader.Requests==1,"Reload cooldown after dismiss");
  Time.unscaledTime=2;var next=Ready(ads);ads.ShowInterstitial(()=>callbacks++);ad.Dismiss();
  Check(PulseAds.Fullscreen&&callbacks==1,"Stale previous callback cannot dismiss new ad");
  next.FailShow();Check(callbacks==2&&!PulseAds.Fullscreen,"Failed show resumes transition");
  ads=Fresh();ad=Ready(ads);ad.ThrowOnShow=true;ad.ThrowOnDestroy=true;callbacks=0;AudioListener.pause=true;
  Check(ads.ShowInterstitial(()=>callbacks++),"Synchronous show exception handled by owner");
  Check(callbacks==1&&!PulseAds.Fullscreen&&AudioListener.pause,"Show/cleanup exception resumes once and preserves pre-muted audio");
  ads=Fresh();ad=Ready(ads);ads.SetForeground(false);
  Check(!ads.ShowInterstitial(()=>callbacks++),"No show while app is backgrounded");
  ads=Fresh();ads.PrepareInterstitial();var pendingLoader=InterstitialAdLoader.Last;ads.DisposeForTest();
  ad=new Interstitial();pendingLoader.Complete(ad);
  Check(pendingLoader.Cancelled&&ad.Destroyed,"Owner teardown cancels loading and destroys late delivery");
  ads=Fresh();ad=Ready(ads);callbacks=0;ads.ShowInterstitial(()=>callbacks++);ads.DisposeForTest();ad.Dismiss();
  Check(callbacks==0&&ad.Destroyed&&!AudioListener.pause,"Teardown discards stale continuation and restores audio");
 }
 static void ResultGate(){
  foreach(var won in new[]{true,false})foreach(var command in new[]{CampaignUICommand.Primary,CampaignUICommand.Restart,CampaignUICommand.Home,CampaignUICommand.Undo,CampaignUICommand.Levels,CampaignUICommand.SelectLevel}){
   var ads=Fresh();var ad=Ready(ads);var game=Due(won);
   Check(ad.Shows==0,"No automatic show during result animation");
   game.RequestUI(command,14);game.RequestUI(command,14);
   Check(ad.Shows==1&&game.Transitions==0,"One interstitial before leaving result");
   ad.Dismiss();ad.Dismiss();Check(game.Transitions==1&&game.LastCommand==command&&game.LastValue==14,"Resume exact requested command once");
  }
  var owner=Fresh();var missing=Due(false);missing.RequestUI(CampaignUICommand.Restart);
  Check(missing.Transitions==1&&!PulseAds.Fullscreen,"No-fill does not delay retry");
  var late=new Interstitial();InterstitialAdLoader.Last.Complete(late);missing.RequestUI(CampaignUICommand.Restart);
  Check(late.Shows==0&&missing.Transitions==2,"Late loaded ad does not show for consumed result");
  owner=Fresh();var loaded=Ready(owner);var blocked=Due(false);blocked.busy=true;blocked.RequestUI(CampaignUICommand.Restart);
  Check(loaded.Shows==0,"Never show during movement");blocked.busy=false;blocked.CanUndo=false;blocked.RequestUI(CampaignUICommand.Undo);
  Check(loaded.Shows==0,"Unavailable undo cannot trigger ad");blocked.RequestUI(CampaignUICommand.SelectLevel,-1);
  Check(loaded.Shows==0,"Invalid destination cannot trigger ad");blocked.level.id=5;blocked.RequestUI(CampaignUICommand.Home);
  Check(loaded.Shows==0,"Tutorial destination state rejects even stale pending ad");
  foreach(var mode in new[]{LobbyMode.Challenge,LobbyMode.Endless}){
   var game=new CampaignGame {ActiveMode=mode};for(int i=0;i<6;i++){game.Outcome(false);game.NewLevel(20);game.Outcome(true);}
   Check(game.UserProfile.interstitialWins==0&&game.UserProfile.interstitialLosses==0,"No campaign cadence in other modes");
  }
  var testing=new CampaignGame {testing=true};testing.Outcome(false);testing.Outcome(true);
  Check(testing.UserProfile.interstitialWins==0&&testing.UserProfile.interstitialLosses==0,"Verification mode does not count results");
  var once=new CampaignGame();once.Outcome(true);once.Outcome(true);
  Check(once.UserProfile.interstitialWins==1,"Undo/rewin cannot count same level attempt twice");
  owner=Fresh();loaded=Ready(owner);var voluntary=Due(true);owner.RewardAvailable=true;voluntary.WatchVictoryReward();owner.CompleteReward(true);voluntary.RequestUI(CampaignUICommand.Primary);
  Check(loaded.Shows==0&&voluntary.Transitions==1,"No interstitial immediately after voluntary rewarded");
  Check(voluntary.UserProfile.crystals==10&&voluntary.UserProfile.resonance==1,"Rewarded bonus stays intact");
  owner=Fresh();loaded=Ready(owner);voluntary=Due(true);voluntary.WatchVictoryReward();voluntary.RequestUI(CampaignUICommand.Primary);
  Check(loaded.Shows==1,"Unavailable rewarded does not consume scheduled interstitial");loaded.Dismiss();
  owner=Fresh();loaded=Ready(owner);var deadObject=Due(false);deadObject.RequestUI(CampaignUICommand.Home);deadObject.Destroyed=true;loaded.Dismiss();
  Check(deadObject.Transitions==0,"Destroyed scene receives no continuation");
 }
}
