using System;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public sealed partial class PulseAds {
 InterstitialAdLoader interstitialLoader;
 Interstitial interstitial;
 Action interstitialFinished;
 bool interstitialLoading;
 int interstitialFailures;
 float interstitialLoadedAt,nextInterstitialLoad;
 string InterstitialId=>Debug.isDebugBuild?"demo-interstitial-yandex":Application.platform==RuntimePlatform.IPhonePlayer?"R-M-20061555-3":"R-M-20061525-3";
 public bool InterstitialReady=>Device&&foreground&&!showing&&interstitial!=null&&Time.unscaledTime-interstitialLoadedAt<1800;

 // One cached ad, one loader, bounded retries. A new level/result can start another
 // retry batch after the cooldown; the frame loop cannot restart exhausted batches.
 public void PrepareInterstitial(bool newOpportunity=false){
  if(!Device||!foreground||showing||interstitialLoading||Time.unscaledTime<nextInterstitialLoad)return;
  if(newOpportunity)interstitialFailures=0;
  if(interstitialFailures>=3)return;
  if(interstitial!=null){
   if(Time.unscaledTime-interstitialLoadedAt<1800)return;
   var expired=interstitial;interstitial=null;DestroyInterstitial(expired);
  }
  interstitialLoading=true;
  try{
   if(interstitialLoader==null)interstitialLoader=new InterstitialAdLoader();
   interstitialLoader.LoadAd(new AdRequest(InterstitialId),ad=>{
    if(!this){DestroyInterstitial(ad);return;}
    interstitialLoading=false;interstitial=ad;interstitialLoadedAt=Time.unscaledTime;interstitialFailures=0;
   },error=>InterstitialLoadFailed(error.Message));
  }catch(Exception error){InterstitialLoadFailed(error.Message);}
 }
 void InterstitialLoadFailed(string reason){
  if(!this)return;
  interstitialLoading=false;interstitialFailures++;
  nextInterstitialLoad=Time.unscaledTime+(float)Math.Min(300,30*Math.Pow(2,Math.Min(interstitialFailures-1,4)));
  Debug.LogWarning("Yandex interstitial load failed: "+reason);
 }
 // False means continue immediately. Never hold a transition waiting for network.
 public bool ShowInterstitial(Action onFinished){
  if(!InterstitialReady){PrepareInterstitial(true);return false;}
  var ad=interstitial;
  showing=true;interstitialFinished=onFinished;mutedBefore=AudioListener.pause;AudioListener.pause=true;
  ad.OnAdDismissed+=(s,e)=>FinishInterstitial(ad);
  ad.OnAdFailedToShow+=(s,e)=>{Debug.LogWarning("Yandex interstitial show failed: "+e.Message);FinishInterstitial(ad);};
  try{SetBannerVisible(false);ad.Show();}
  catch(Exception error){Debug.LogWarning("Yandex interstitial show failed: "+error.Message);FinishInterstitial(ad);}
  return true;
 }
 void FinishInterstitial(Interstitial ad){
  if(!showing||interstitial!=ad)return;
  var callback=interstitialFinished;
  // Clear identity and callback before destroying: duplicate SDK events cannot
  // resume the level twice, including when Show throws synchronously.
  interstitial=null;interstitialFinished=null;showing=false;
  AudioListener.pause=mutedBefore;nextInterstitialLoad=Time.unscaledTime+2;
  DestroyInterstitial(ad);callback?.Invoke();
 }
 static void DestroyInterstitial(Interstitial ad){
  try{ad?.Destroy();}catch(Exception error){Debug.LogWarning("Yandex interstitial cleanup failed: "+error.Message);}
 }
 void DisposeInterstitial(){
  interstitialFinished=null;
  try{interstitialLoader?.CancelLoading();}catch(Exception error){Debug.LogWarning("Yandex interstitial cancel failed: "+error.Message);}
  var ad=interstitial;interstitial=null;DestroyInterstitial(ad);
 }
}
