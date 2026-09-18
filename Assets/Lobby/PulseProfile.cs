using System;
using UnityEngine;

[Serializable] public class PulseProfile {
 public string playerName="Искра",giftDay="",challengeDay="",rewardDay="";
 public int crystals,resonance,wins,bestEndless,selectedSkin,selectedPortal,giftStreak,crystalRewardsToday,resonanceRewardsToday;
 public int ownedSkins=1,ownedPortals=1,claimedTasks,claimedAchievements;
 public int adEligibleWins;
 public bool RecordAdEligibleWin(int levelId){if(levelId<=5)return false;adEligibleWins++;return adEligibleWins%3==0;}
 // Independent persisted cadence; tutorials never advance either counter.
 public int interstitialWins,interstitialLosses;
 public bool RecordInterstitialResult(int levelId,bool won){
  if(levelId<=5)return false;
  if(won){interstitialWins=(Mathf.Max(0,interstitialWins)%3+1)%3;return interstitialWins==0;}
  interstitialLosses=(Mathf.Max(0,interstitialLosses)%3+1)%3;return interstitialLosses==0;
 }
 public float sound=.7f,music=.3f;
 public bool reducedMotion,vibration=true,fps60=true;
 public bool jumpSound=true;
 public int quality=2,profileVersion=4;
 public int[] firstWins=new int[0];
 public static PulseProfile Load(bool testing){
  if(testing)return new PulseProfile();
  try{var p=JsonUtility.FromJson<PulseProfile>(PlayerPrefs.GetString("PulseLobby.Profile.v1",""));if(p!=null){p.firstWins=p.firstWins??new int[0];p.playerName=string.IsNullOrWhiteSpace(p.playerName)?"Искра":p.playerName;p.crystals=Mathf.Max(0,p.crystals);p.resonance=Mathf.Max(0,p.resonance);p.ownedSkins|=1;p.ownedPortals|=1;if(p.selectedSkin<0||p.selectedSkin>4||(p.ownedSkins&(1<<p.selectedSkin))==0)p.selectedSkin=0;if(p.selectedPortal<0||p.selectedPortal>4||(p.ownedPortals&(1<<p.selectedPortal))==0)p.selectedPortal=0;p.sound=Mathf.Clamp01(p.sound);p.music=Mathf.Clamp01(p.music);if(p.profileVersion<3){p.vibration=true;p.fps60=true;p.quality=2;p.ownedPortals|=1;p.profileVersion=3;}if(p.profileVersion<4){p.jumpSound=true;p.profileVersion=4;}p.quality=Mathf.Clamp(p.quality,0,2);return p;}}catch(Exception e){Debug.LogWarning("Profile could not be read; existing stored data retained: "+e.Message);}
  return new PulseProfile();
 }
 public void Save(bool testing){if(testing)return;PlayerPrefs.SetString("PulseLobby.Profile.v1",JsonUtility.ToJson(this));PlayerPrefs.Save();}
 public bool GiftReady(string day)=>giftDay!=day;
 public bool ClaimGift(string day){if(!GiftReady(day))return false;DateTime today;if(!DateTime.TryParse(day,out today))today=DateTime.UtcNow.Date;DateTime prior;giftStreak=DateTime.TryParse(giftDay,out prior)&&prior.Date==today.Date.AddDays(-1)?Mathf.Min(7,giftStreak+1):1;giftDay=day;crystals+=100;resonance+=1;return true;}
 public void PrepareRewardDay(string day){if(rewardDay==day)return;rewardDay=day;crystalRewardsToday=0;resonanceRewardsToday=0;}
 public int RewardCount(bool pulse,string day){PrepareRewardDay(day);return pulse?resonanceRewardsToday:crystalRewardsToday;}
 public bool ClaimVideoReward(bool pulse,string day){PrepareRewardDay(day);int used=pulse?resonanceRewardsToday:crystalRewardsToday;if(used>=5)return false;if(pulse){resonance++;resonanceRewardsToday++;}else{crystals+=10;crystalRewardsToday++;}return true;}
 public static readonly int[] SkinPrices={0,200,350,500,3};
 public static readonly int[] PortalPrices={0,250,400,3,650};
 public bool BuySkin(int index){
  if(index<0||index>4)return false;if((ownedSkins&(1<<index))!=0){selectedSkin=index;return true;}
  int price=SkinPrices[index];if(index==4){if(resonance<price)return false;resonance-=price;}else{if(crystals<price)return false;crystals-=price;}
  ownedSkins|=1<<index;selectedSkin=index;return true;
 }
 public bool BuyPortal(int index){
  if(index<0||index>4)return false;if((ownedPortals&(1<<index))!=0){selectedPortal=index;return true;}
  int price=PortalPrices[index];if(index==3){if(resonance<price)return false;resonance-=price;}else{if(crystals<price)return false;crystals-=price;}
  ownedPortals|=1<<index;selectedPortal=index;return true;
 }
 public bool RecordFirstWin(int id){if(Array.IndexOf(firstWins,id)>=0)return false;Array.Resize(ref firstWins,firstWins.Length+1);firstWins[firstWins.Length-1]=id;crystals+=50;return true;}
}
