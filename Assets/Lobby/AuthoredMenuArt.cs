using UnityEngine;

// Resizes the composition as ONE unit. User-authored child transforms are never changed.
[DefaultExecutionOrder(1000)]
public class AuthoredMenuArt:MonoBehaviour {
 public Transform artwork;
 [Tooltip("Area reserved for all 3D menu art, normalized inside the safe screen.")]
 public Rect viewport=new Rect(.04f,.23f,.92f,.47f);
 public bool adaptToScreen=true;
 [Tooltip("Keep the framed hero anchored while its child animation plays.")]
 public bool lockArtworkPosition;
 [SerializeField,HideInInspector] public Vector3[] framingGeometry;
 public Vector3[] FrameGeometry=>framingGeometry!=null&&framingGeometry.Length>0?framingGeometry:geometry??(geometry=ScreenComposition.Geometry(artwork));
 Vector3[] geometry;Vector3 lockedPosition,lockedScale;Quaternion lockedRotation;bool locked;
 public void Frame(Camera camera){
  if(!adaptToScreen||!artwork)return;
  var safe=MobileViewport.SafeArea;var target=new Rect(safe.x+viewport.x*safe.width,safe.y+viewport.y*safe.height,viewport.width*safe.width,viewport.height*safe.height);
  ScreenComposition.Fit(artwork,camera,FrameGeometry,target);
  if(lockArtworkPosition){lockedPosition=artwork.localPosition;lockedScale=artwork.localScale;lockedRotation=artwork.localRotation;locked=true;}
 }
 void OnEnable(){locked=false;}
 void LateUpdate(){if(lockArtworkPosition&&locked&&artwork){artwork.localPosition=lockedPosition;artwork.localScale=lockedScale;artwork.localRotation=lockedRotation;}}
}
