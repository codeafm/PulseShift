using UnityEngine;
using System.Collections.Generic;

// These meshes and texture atlases come from the user's actual Blender model library.
public static class ReferenceArt {
 static readonly Dictionary<string,ReferenceModelAsset> cache=new();
 public static GameObject Make(string id,Vector3 position){
  if(!cache.TryGetValue(id,out var asset)||!asset){asset=Resources.Load<ReferenceModelAsset>("ReferenceModels/"+id);if(!asset)throw new System.InvalidOperationException("Missing baked Blender model: "+id);cache[id]=asset;}
  var root=new GameObject(id+" • Blender reference model");root.transform.position=position;
  var origin=root.AddComponent<ReferenceIdentity>();origin.modelId=id;
  var visual=new GameObject("Reference visual").transform;visual.SetParent(root.transform,false);
  foreach(var part in asset.parts){
   string name=part.name=="Deck"?"Deck • moving surface":part.name;
   var o=new GameObject(name);o.transform.SetParent(visual,false);o.transform.localPosition=part.pivot;
   Renderer renderer;
   if(part.mesh.blendShapeCount>0){var skin=o.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=part.mesh;skin.localBounds=new Bounds(part.mesh.bounds.center,part.mesh.bounds.size+Vector3.one*.3f);skin.updateWhenOffscreen=true;renderer=skin;}
   else{o.AddComponent<MeshFilter>().sharedMesh=part.mesh;renderer=o.AddComponent<MeshRenderer>();}
   renderer.sharedMaterial=asset.material;
  }
  CinematicArt.Dress(root,id);
  if(id=="Spirit"||id=="Shadow"||id=="Crystal"||id=="Portal")root.AddComponent<ReferenceMotion>().modelId=id;
  return root;
 }
 public static Transform Part(Transform root,string part)=>root.Find("Reference visual/"+part);
}
