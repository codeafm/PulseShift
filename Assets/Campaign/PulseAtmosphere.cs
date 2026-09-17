using UnityEngine;

[ExecuteAlways,RequireComponent(typeof(Camera))]
public class PulseAtmosphere:MonoBehaviour {
 [Range(0,1)] public float density=.7f;
 public bool reducedMotion;
 public float Clock{get;private set;}
 Camera view;Transform[] layers;Material[] materials;Mesh mesh;
 public static PulseAtmosphere Ensure(Camera camera){var effect=camera.GetComponent<PulseAtmosphere>();if(!effect)effect=camera.gameObject.AddComponent<PulseAtmosphere>();return effect;}
 void OnEnable(){view=GetComponent<Camera>();}
 void Prepare(){
  if(layers!=null&&layers[0])return;
  var shader=Resources.Load<Shader>("MobileMist");if(!shader||!shader.isSupported)return;
  mesh=new Mesh{name="Mist / screen quad",hideFlags=HideFlags.HideAndDontSave};
  mesh.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};
  mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateBounds();
  layers=new Transform[2];materials=new Material[2];
  for(int i=0;i<2;i++){
   var g=new GameObject(i==0?"Living mist • behind islands":"Living mist • foreground wisps"){hideFlags=HideFlags.HideAndDontSave};
   g.transform.SetParent(transform,false);g.layer=gameObject.layer;g.AddComponent<MeshFilter>().sharedMesh=mesh;
   var renderer=g.AddComponent<MeshRenderer>();renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
   materials[i]=new Material(shader){hideFlags=HideFlags.HideAndDontSave};materials[i].SetFloat("_Near",i);renderer.sharedMaterial=materials[i];layers[i]=g.transform;
  }
 }
 void LateUpdate(){if(Application.isPlaying&&!reducedMotion)Clock+=Time.unscaledDeltaTime;}
 void OnPreCull(){
  if(!view)view=GetComponent<Camera>();Prepare();if(layers==null)return;
  for(int i=0;i<2;i++){
   float distance=i==0?Mathf.Min(view.farClipPlane*.72f,58):view.nearClipPlane+1;
   float h=view.orthographic?view.orthographicSize*2:2*Mathf.Tan(view.fieldOfView*Mathf.Deg2Rad*.5f)*distance;
   layers[i].localPosition=new Vector3(0,0,distance);layers[i].localScale=new Vector3(h*view.aspect*1.015f,h*1.015f,1);
   materials[i].SetFloat("_Clock",Clock);materials[i].SetFloat("_Aspect",view.aspect);materials[i].SetFloat("_Density",density);
  }
 }
 void OnDisable(){if(layers!=null)foreach(var layer in layers)if(layer)Dispose(layer.gameObject);if(materials!=null)foreach(var m in materials)if(m)Dispose(m);if(mesh)Dispose(mesh);layers=null;materials=null;mesh=null;}
 static void Dispose(Object value){if(Application.isPlaying)Destroy(value);else DestroyImmediate(value);}
}
