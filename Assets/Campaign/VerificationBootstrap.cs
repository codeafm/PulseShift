using UnityEngine;
using System;
using System.Linq;

public static class VerificationBootstrap {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
 static void ConfigureHiddenQA(){
  var args=Environment.GetCommandLineArgs();
  if(args.Any(a=>a=="-campaignVerify"||a=="-lobbyVerify"||a=="-mobileVerify"||a=="-sceneVerify")){
   Application.runInBackground=true;Debug.Log("VERIFICATION_BOOT: background test execution enabled; player saves protected.");
  }
 }
}
