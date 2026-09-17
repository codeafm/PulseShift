using UnityEngine;

public static class PulseHaptics {
 public static void SoftDeath(bool enabled){
  if(!enabled)return;
#if UNITY_ANDROID && !UNITY_EDITOR
  try{using var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer");using var activity=player.GetStatic<AndroidJavaObject>("currentActivity");using var window=activity.Call<AndroidJavaObject>("getWindow");using var view=window.Call<AndroidJavaObject>("getDecorView");view.Call<bool>("performHapticFeedback",3);}catch(System.Exception e){Debug.LogWarning("Soft haptic unavailable: "+e.Message);}
#elif UNITY_IOS && !UNITY_EDITOR
  Handheld.Vibrate();
#endif
 }
}
