using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public sealed class ScreenBackdropColor:MonoBehaviour {
 Material owned;
 void OnEnable(){
  var shader=Resources.Load<Shader>("Lobby/ScreenBackdrop");if(!shader)return;
  owned=new Material(shader){name="Skin screen / shared linear color"};
  GetComponent<RawImage>().material=owned;
 }
 void OnDisable(){if(!owned)return;GetComponent<RawImage>().material=null;if(Application.isPlaying)Destroy(owned);else DestroyImmediate(owned);owned=null;}
}
