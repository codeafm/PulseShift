using UnityEngine;

public partial class CampaignGame {
 bool adWinRecorded,adOffer,adClaimed;
 public bool VictoryRewardAvailable=>adOffer&&!adClaimed&&Victorious&&ResultVisible;
 void ResetAdVictory(){adWinRecorded=adOffer=adClaimed=false;}
 void RecordAdVictory(){
  if(adWinRecorded||testing||ActiveMode!=LobbyMode.Campaign||level.id<=5)return;
  adWinRecorded=true;var p=UserProfile;if(p==null)return;adOffer=p.RecordAdEligibleWin(level.id);p.Save(false);
 }
 public void WatchVictoryReward(){
  if(!VictoryRewardAvailable)return;var ads=PulseAds.Instance;if(!ads)return;var p=UserProfile;if(p==null)return;
  // Explicit opt-in bonus: both currencies, separate from menu daily quotas.
  if(!ads.ShowReward(()=>{if(adClaimed)return;adClaimed=true;p.crystals+=10;p.resonance++;p.Save(false);},success=>{if(this)message=success?"Награда: +10 кристаллов и +1 импульс":"Просмотр не завершён";}))message=ads.Status;
 }
}
