using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public partial class PulseLobby {
 int pendingVideoReward;bool videoRewardBusy;Text videoCountdownLabel;
 public bool PendingPulseReward=>pendingVideoReward==1;
 public int VideoRewardsUsed=>Profile.RewardCount(PendingPulseReward,Today);
 public int VideoRewardsLeft=>Mathf.Max(0,5-VideoRewardsUsed);
 public string GiftCountdown { get {var left=DateTime.UtcNow.Date.AddDays(1)-DateTime.UtcNow;return string.Format("{0:00}:{1:00}:{2:00}",(int)left.TotalHours,left.Minutes,left.Seconds);} }
 public string QualityCaption=>new[]{"Низкое","Среднее","Высокое"}[Mathf.Clamp(Profile.quality,0,2)];
 public void OpenVideoReward(bool pulse){pendingVideoReward=pulse?1:0;Profile.PrepareRewardDay(Today);ShowPage("Награда");}
 public void BeginVideoReward(){
  if(videoRewardBusy||VideoRewardsLeft<=0)return;
  if(TestMode){StartCoroutine(VideoRewardPreview(PendingPulseReward));return;}
  var ads=PulseAds.Instance;if(!ads){Toast("Реклама пока недоступна");return;}
  bool pulse=PendingPulseReward;var profile=Profile;videoRewardBusy=true;
  if(!ads.ShowReward(()=>{if(profile.ClaimVideoReward(pulse,DateTime.UtcNow.ToString("yyyy-MM-dd")))profile.Save(false);},success=>{if(!this)return;videoRewardBusy=false;Save();if(success){PlayBonus();stage.Celebrate();}OpenVideoReward(pulse);Toast(success?(pulse?"+1 импульс":"+10 кристаллов"):"Просмотр не завершён — награда не начислена");})){videoRewardBusy=false;Toast(ads.Status);}
 }
 IEnumerator VideoRewardPreview(bool pulse){
  videoRewardBusy=true;ShowPage("Просмотр");
  for(int seconds=3;seconds>0;seconds--){if(videoCountdownLabel)videoCountdownLabel.text="REWARD VIDEO\nНаграда через "+seconds;yield return new WaitForSecondsRealtime(1);}
  bool granted=Profile.ClaimVideoReward(pulse,Today);Save();videoRewardBusy=false;if(granted){PlayBonus();stage.Celebrate();Toast(pulse?"+1 импульс":"+10 кристаллов");}OpenVideoReward(pulse);
 }
}
