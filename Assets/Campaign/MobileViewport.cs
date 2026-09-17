using UnityEngine;

// Shared pixels -> logical portrait layout. Insets affect controls, never the full-bleed artwork.
public static class MobileViewport {
 public static Rect? TestSafeArea;
 public static Vector2Int? TestScreenSize;
 public static int Width=>TestScreenSize?.x??Screen.width;
 public static int Height=>TestScreenSize?.y??Screen.height;
 public static Rect DeviceSafeArea {
  get {
   var area=TestSafeArea??Screen.safeArea;
   if(area.width<1||area.height<1)area=new Rect(0,0,Width,Height);
   return Rect.MinMaxRect(Mathf.Clamp(area.xMin,0,Width-1),Mathf.Clamp(area.yMin,0,Height-1),Mathf.Clamp(area.xMax,1,Width),Mathf.Clamp(area.yMax,1,Height));
  }
 }
 public static Rect SafeArea {get {var area=DeviceSafeArea;float inset=Application.isPlaying?PulseAds.BottomInsetPixels:0;area.yMin=Mathf.Min(area.yMax-1,area.yMin+inset);return area;}}
 // Advertising changes available height, not the size of every button and label.
 public static float Scale=>Mathf.Max(.01f,Mathf.Min(DeviceSafeArea.width/540f,DeviceSafeArea.height/840f));
 public static Vector2 LogicalSize=>SafeArea.size/Scale;
 public static Rect BoardPixels {
  get {var a=SafeArea;float s=Scale;return Rect.MinMaxRect(a.xMin+10*s,a.yMin+230*s,a.xMax-10*s,a.yMax-168*s);}
 }
 public static void Layout(RectTransform root,Canvas canvas){
  var area=SafeArea;float units=Mathf.Max(.01f,canvas.scaleFactor);
  root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.pivot=new Vector2(.5f,.5f);
  root.sizeDelta=LogicalSize;root.localScale=Vector3.one*(Scale/units);
  root.anchoredPosition=(area.center-new Vector2(Width,Height)*.5f)/units;
 }
 public static void LayoutAuthored(RectTransform root,Canvas canvas){
  var area=SafeArea;float units=Mathf.Max(.01f,canvas.scaleFactor);
  var layout=root.GetComponent<BannerAuthoredLayout>();if(!layout)layout=root.gameObject.AddComponent<BannerAuthoredLayout>();
  var authored=layout.OriginalSize;
  float scale=Mathf.Max(.01f,Mathf.Min(DeviceSafeArea.width/authored.x,DeviceSafeArea.height/authored.y));
  root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.pivot=new Vector2(.5f,.5f);
  float height=Mathf.Min(authored.y,area.height/scale);
  root.sizeDelta=new Vector2(authored.x,height);layout.Reflow(height/authored.y);
  root.localScale=Vector3.one*(scale/units);
  root.anchoredPosition=(area.center-new Vector2(Width,Height)*.5f)/units;
 }
}

// Cache authored offsets once; reflow vertically without shrinking the artwork/buttons.
public sealed class BannerAuthoredLayout:MonoBehaviour {
 Vector2 originalSize;readonly System.Collections.Generic.Dictionary<RectTransform,Vector2> offsets=new System.Collections.Generic.Dictionary<RectTransform,Vector2>();
 public Vector2 OriginalSize {get {if(originalSize.x<1){originalSize=((RectTransform)transform).sizeDelta;if(originalSize.x<1||originalSize.y<1)originalSize=new Vector2(540,1170);}return originalSize;}}
 public void Reflow(float ratio){foreach(Transform child in transform){var rect=child as RectTransform;if(!rect)continue;if(!offsets.TryGetValue(rect,out var offset)){offset=rect.anchoredPosition;offsets.Add(rect,offset);}rect.anchoredPosition=new Vector2(offset.x,offset.y*ratio);}}
}
