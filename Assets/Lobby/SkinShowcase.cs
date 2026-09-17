using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SkinShowcase:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler {
 static int previewSlot;
 PulseLobby lobby;RawImage image;GameObject host,display;RenderTexture target;Vector2 dragStart;bool showingPortal;readonly List<Material> materials=new();
 public void Initialize(PulseLobby owner,RawImage output,bool portal,int index,Color accent,bool thumbnail=false){
  lobby=owner;image=output;showingPortal=portal;target=new RenderTexture(thumbnail?192:768,thumbnail?224:Mathf.RoundToInt(768*output.rectTransform.rect.height/output.rectTransform.rect.width),24,RenderTextureFormat.ARGB32){name="Skin showcase",antiAliasing=4};target.Create();image.texture=target;
  host=new GameObject("3D skin showcase • temporary");var cameraObject=new GameObject("Preview camera");cameraObject.transform.SetParent(host.transform,false);var camera=cameraObject.AddComponent<Camera>();camera.targetTexture=target;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.orthographic=true;camera.fieldOfView=27;camera.cullingMask=1<<27;camera.allowHDR=true;
  var light=new GameObject("Preview key").AddComponent<Light>();light.transform.SetParent(host.transform,false);light.type=LightType.Directional;light.color=new Color(.8f,.9f,1);light.intensity=1.45f;light.cullingMask=1<<27;light.transform.rotation=Quaternion.Euler(38,-32,0);
  var rim=new GameObject("Preview rim").AddComponent<Light>();rim.transform.SetParent(host.transform,false);rim.type=LightType.Directional;rim.color=Color.white;rim.intensity=.65f;rim.cullingMask=1<<27;rim.transform.rotation=Quaternion.Euler(15,155,0);
  display=new GameObject("Rotating collection item");display.transform.SetParent(host.transform,false);var pedestal=ReferenceArt.Make("Normal",Vector3.zero);pedestal.transform.SetParent(display.transform,false);pedestal.transform.localScale=Vector3.one*1.22f;
  var underside=ReferenceArt.Part(pedestal.transform,"Base");if(underside)underside.gameObject.SetActive(false);if(thumbnail)pedestal.SetActive(false);
  var model=ReferenceArt.Make(portal?"Portal":"Spirit",Vector3.zero);model.transform.SetParent(display.transform,false);model.transform.localPosition=Vector3.up*(portal?.2f:.18f);model.transform.localScale=Vector3.one*(portal?1.35f:2.75f);SkinAppearance.Apply(model.transform,portal,index);
  foreach(var motion in model.GetComponentsInChildren<ReferenceMotion>(true))motion.menuActor=true;
  camera.aspect=target.width/(float)target.height;
  var bounds=new Bounds(model.transform.position,Vector3.zero);bool first=true;
  foreach(var r in display.GetComponentsInChildren<Renderer>()){if(!r.enabled||r is LineRenderer)continue;if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
  camera.transform.position=bounds.center+new Vector3(0,1.1f,-8);camera.transform.LookAt(bounds.center);
  float vertical=0,horizontal=0;for(int i=0;i<8;i++){var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));var p=camera.transform.InverseTransformPoint(corner);vertical=Mathf.Max(vertical,Mathf.Abs(p.y));horizontal=Mathf.Max(horizontal,Mathf.Abs(p.x));}
  camera.orthographicSize=Mathf.Max(vertical,horizontal/camera.aspect)*(thumbnail?1.04f:1.08f);SetLayer(host.transform,27);
  host.transform.position=new Vector3(1000+(previewSlot++%1000)*30,1000,1000);
  camera.Render();if(thumbnail)host.SetActive(false);
 }
 static void SetLayer(Transform root,int layer){foreach(var node in root.GetComponentsInChildren<Transform>(true))node.gameObject.layer=layer;}
 void Update(){if(display)display.transform.localRotation=showingPortal?Quaternion.Euler(0,Mathf.Sin(Time.unscaledTime*.65f)*4,0):Quaternion.Euler(0,Mathf.Sin(Time.unscaledTime*.55f)*16,0);}
 public void OnBeginDrag(PointerEventData e){dragStart=e.position;}
 public void OnDrag(PointerEventData e){}
 public void Freeze(){if(host)host.SetActive(false);}
 public void OnEndDrag(PointerEventData e){if(lobby)lobby.SwipeSkin(e.position.x-dragStart.x);}
 void Release(Object value){if(!value)return;if(Application.isPlaying)Destroy(value);else DestroyImmediate(value);}
 void OnDestroy(){if(host)host.SetActive(false);if(target){target.Release();Release(target);}Release(host);foreach(var material in materials)Release(material);}
}
