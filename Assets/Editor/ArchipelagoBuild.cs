using UnityEngine;
using UnityEditor;

public static class ArchipelagoBuild {
 public static void Desktop(){
  ReferenceModelBaker.BakeAll();
  PlayerSettings.bundleVersion="1.2";PlayerSettings.Android.bundleVersionCode=3;
  CinematicBuild.Desktop();
 }
}
