using UnityEngine;

public partial class CampaignGame {
 bool adWinRecorded,adOffer,adClaimed;
 bool interstitialDue,interstitialWinRecorded;
 public bool InterstitialEligible=>!testing&&ActiveMode==LobbyMode.Campaign&&level!=null&&level.id>5;
 public bool VictoryRewardAvailable=>adOffer&&!adClaimed&&Victorious&&ResultVisible;
 void ResetAdVictory(){adWinRecorded=adOffer=adClaimed=interstitialDue=interstitialWinRecorded=false;}
 void RecordAdVictory(){
  if(adWinRecorded||testing||ActiveMode!=LobbyMode.Campaign||level.id<=5)return;
  adWinRecorded=true;var p=UserProfile;if(p==null)return;adOffer=p.RecordAdEligibleWin(level.id);p.Save(false);
 }
 void RecordInterstitialResult(bool won){
  if(!InterstitialEligible||won&&interstitialWinRecorded)return;
  var p=UserProfile;if(p==null)return;
  if(won)interstitialWinRecorded=true;
  interstitialDue=p.RecordInterstitialResult(level.id,won);p.Save(false);
  PulseAds.Instance?.PrepareInterstitial(true);
 }
 bool TryResultInterstitial(CampaignUICommand command,int value){
  if(!interstitialDue||!InterstitialEligible||LobbyOpen||busy||!(state.dead||Victorious))return false;
  bool leaving=false;
  switch(command){
   case CampaignUICommand.Home:case CampaignUICommand.Restart:case CampaignUICommand.Levels:leaving=true;break;
   case CampaignUICommand.SelectLevel:leaving=value>=0&&value<Data.levels.Length&&value<unlocked;break;
   case CampaignUICommand.Primary:leaving=!showLesson&&(state.dead?CanUndo:Victorious);break;
   case CampaignUICommand.Undo:leaving=CanUndo&&!levelMenu&&!showLesson;break;
  }
  if(!leaving)return false;
  // Consume BEFORE calling the SDK: even a synchronous failure/duplicate callback
  // must not show again or stall the requested transition. No late surprise ads.
  interstitialDue=false;
  var ads=PulseAds.Instance;
  return ads&&ads.ShowInterstitial(()=>{if(this)RequestUI(command,value);});
 }
 public void WatchVictoryReward(){
  if(!VictoryRewardAvailable)return;var ads=PulseAds.Instance;if(!ads)return;var p=UserProfile;if(p==null)return;
  // Explicit opt-in bonus: both currencies, separate from menu daily quotas.
  if(ads.ShowReward(()=>{if(adClaimed)return;adClaimed=true;p.crystals+=10;p.resonance++;p.Save(false);},success=>{if(this)message=success?"Награда: +10 кристаллов и +1 импульс":"Просмотр не завершён";}))interstitialDue=false;
  else message=ads.Status;
 }
}
