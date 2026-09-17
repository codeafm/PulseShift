using UnityEngine;

public static class CinematicArt {
 static Mesh portalMesh,contactMesh;static Material vortexMaterial,contactMaterial;
 public static void Dress(GameObject root,string id){
  if(id=="Portal")Portal(root.transform);
  if(id=="Spirit"||id=="Crystal")ContactLight(root.transform,id=="Spirit"?.62f:.42f);
  if(id!="Normal"&&id!="Move"&&id!="Rotate"&&id!="Trap"&&id!="Breakable")return;
  var body=ReferenceArt.Part(root.transform,"Base");if(body)body.localScale=Vector3.one;
  // All stone fractures and foliage are now in the editable Blender meshes themselves.
 }
 static void ContactLight(Transform root,float radius){
  if(root.Find("Soft contact radiance"))return;
  if(!contactMesh){contactMesh=new Mesh{name="Light / radial contact plane"};contactMesh.vertices=new[]{new Vector3(-1,0,-1),new Vector3(1,0,-1),new Vector3(1,0,1),new Vector3(-1,0,1)};contactMesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};contactMesh.triangles=new[]{0,2,1,0,3,2};contactMesh.RecalculateBounds();contactMaterial=new Material(Resources.Load<Shader>("CinematicContact")){name="Light / cyan pool"};}
  var g=new GameObject("Soft contact radiance");g.layer=root.gameObject.layer;g.transform.SetParent(root,false);g.transform.localPosition=new Vector3(0,-.065f,0);g.transform.localScale=Vector3.one*radius;g.AddComponent<MeshFilter>().sharedMesh=contactMesh;var renderer=g.AddComponent<MeshRenderer>();renderer.sharedMaterial=contactMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
 }
 static void Portal(Transform root){
  var vortex=ReferenceArt.Part(root,"Vortex");if(!vortex)return;
  var old=vortex.GetComponent<Renderer>();if(old)old.enabled=false;
  
  if(!portalMesh){
   portalMesh=new Mesh{name="Portal / circular liquid surface"};var vertices=new Vector3[66];var uv=new Vector2[66];var triangles=new int[64*3];uv[0]=new Vector2(.5f,.5f);
   for(int i=0;i<=64;i++){float a=i*Mathf.PI*2/64;var p=new Vector2(Mathf.Cos(a),Mathf.Sin(a));vertices[i+1]=new Vector3(p.x*.84f,p.y*.84f,0);uv[i+1]=p*.5f+Vector2.one*.5f;if(i<64){triangles[i*3]=0;triangles[i*3+1]=i+2;triangles[i*3+2]=i+1;}}
   portalMesh.vertices=vertices;portalMesh.uv=uv;portalMesh.triangles=triangles;portalMesh.RecalculateNormals();portalMesh.RecalculateBounds();vortexMaterial=new Material(Resources.Load<Shader>("CinematicPortal")){name="Portal / liquid ion vortex"};
  }
  var existing=vortex.Find("Dimensional water • animated vortex");if(existing){existing.GetComponent<MeshFilter>().sharedMesh=portalMesh;existing.GetComponent<Renderer>().sharedMaterial=vortexMaterial;return;}
  var g=new GameObject("Dimensional water • animated vortex");g.layer=root.gameObject.layer;g.transform.SetParent(vortex,false);g.transform.localPosition=new Vector3(0,0,-.035f);g.AddComponent<MeshFilter>().sharedMesh=portalMesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=vortexMaterial;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
 }
}
