using UnityEngine;
using System.Collections.Generic;

public static class ScreenComposition {
 // Cache the neutral LOCAL mesh envelope. World AABBs and animated skin bounds used to
 // inflate at every reframe and progressively shrink the hero after opening the menu.
 public static Vector3[] Geometry(Transform root){
  var points=new List<Vector3>();
  foreach(var renderer in root.GetComponentsInChildren<Renderer>(true)){
   Mesh mesh=null;
   if(renderer is SkinnedMeshRenderer skin)mesh=skin.sharedMesh;
   else if(renderer is MeshRenderer){var filter=renderer.GetComponent<MeshFilter>();if(filter)mesh=filter.sharedMesh;}
   if(!mesh)continue;
   var b=mesh.bounds;
   for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
    points.Add(root.InverseTransformPoint(renderer.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z)))));
  }
  return points.ToArray();
 }
 public static Rect Bounds(Transform root,Camera camera,Vector3[] points){
  var min=new Vector2(float.MaxValue,float.MaxValue);var max=-min;
  foreach(var point in points){Vector2 p=camera.WorldToScreenPoint(root.TransformPoint(point));min=Vector2.Min(min,p);max=Vector2.Max(max,p);}
  return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
 }
 public static void Fit(Transform root,Camera camera,Vector3[] points,Rect target){
  if(points.Length==0||target.width<=0||target.height<=0)return;
  for(int i=0;i<12;i++){
   var b=Bounds(root,camera,points);float factor=Mathf.Min(target.width/Mathf.Max(1,b.width),target.height/Mathf.Max(1,b.height));
   root.localScale*=Mathf.Clamp(factor,.6f,1.7f);
   b=Bounds(root,camera,points);var delta=target.center-b.center;
   float distance=Vector3.Dot(root.position-camera.transform.position,camera.transform.forward);
   float units=(camera.orthographic?camera.orthographicSize*2:2*Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f)*distance)/Mathf.Max(1,camera.pixelHeight);
   root.position+=camera.transform.right*delta.x*units+camera.transform.up*delta.y*units;
  }
 }
}
