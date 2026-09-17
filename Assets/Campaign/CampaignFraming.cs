using UnityEngine;
using System.Linq;

// Move/scale the LEVEL, never the manually authored camera. This also supports FOV 22.
public static class CampaignFraming {
 public static void Fit(Transform board,Camera camera){
  if(!Application.isPlaying)camera.aspect=9f/16;
  var target=Application.isPlaying?MobileViewport.BoardPixels:new Rect(camera.pixelWidth*.04f,camera.pixelHeight*.24f,camera.pixelWidth*.92f,camera.pixelHeight*.575f);
  ScreenComposition.Fit(board,camera,ScreenComposition.Geometry(board),target);
 }
}
