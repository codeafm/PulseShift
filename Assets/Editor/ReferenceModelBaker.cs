using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

public class ReferenceTextureImporter:AssetPostprocessor {
 void OnPreprocessTexture(){if(!assetPath.Contains("/ReferenceArt/"))return;var t=(TextureImporter)assetImporter;t.textureCompression=TextureImporterCompression.CompressedHQ;t.maxTextureSize=2048;t.mipmapEnabled=true;t.wrapMode=TextureWrapMode.Clamp;
  t.sRGBTexture=assetPath.Contains("_Albedo")||assetPath.Contains("_Emission");if(assetPath.Contains("_Normal")){t.textureType=TextureImporterType.NormalMap;t.convertToNormalmap=false;}
 }
 void OnPostprocessTexture(Texture2D texture){if(!assetPath.Contains("/ReferenceArt/")||!assetPath.Contains("_MetalSmooth"))return;var p=texture.GetPixels32();for(int i=0;i<p.Length;i++){p[i].a=p[i].g;p[i].g=p[i].b=0;}texture.SetPixels32(p);texture.Apply(true,false);}
}
public static class ReferenceModelBaker {
 [Serializable] class Model {public string id;public Group[] groups;}
 [Serializable] class Group {public string name;public float[] vertices,normals,uv,poseA,poseB,pivot;public int[] triangles;}
 static Vector3[] Vectors(float[] data){var v=new Vector3[data.Length/3];for(int i=0;i<v.Length;i++)v[i]=new Vector3(data[i*3],data[i*3+1],data[i*3+2]);return v;}
 [MenuItem("PULSESHIFT/Bake Blender models for Unity")]
 public static void BakeAll(){
  Directory.CreateDirectory("Assets/Resources/ReferenceModels");
  foreach(var id in new[]{"Normal","Move","Rotate","Trap","Breakable","Spirit","Shadow","Crystal","Portal"}){
   string folder="Assets/Resources/ReferenceArt/",path="Assets/Resources/ReferenceModels/"+id+".asset";
   var data=JsonUtility.FromJson<Model>(File.ReadAllText(folder+id+".json"));
   var asset=AssetDatabase.LoadAssetAtPath<ReferenceModelAsset>(path);
   // Existing assets can be rebuilt in place without changing references in saved scenes.
   if(!asset){asset=ScriptableObject.CreateInstance<ReferenceModelAsset>();AssetDatabase.CreateAsset(asset,path);}
   var oldParts=asset.parts??Array.Empty<ReferencePart>();
   asset.referenceId=id;
   var mat=asset.material?asset.material:new Material(Shader.Find("Standard")){name=id+" / baked Blender PBR",color=Color.white};
   mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+id+"_Albedo.png");mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+id+"_Normal.png"));mat.EnableKeyword("_NORMALMAP");mat.SetFloat("_BumpScale",.55f);
   mat.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+id+"_MetalSmooth.png"));mat.EnableKeyword("_METALLICGLOSSMAP");mat.SetFloat("_GlossMapScale",1);
   mat.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+id+"_Emission.png"));mat.SetColor("_EmissionColor",Color.white*4);mat.EnableKeyword("_EMISSION");
   // Prevent StandardShaderGUI from clearing _EMISSION when a saved material is selected.
   mat.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;
   if(!AssetDatabase.Contains(mat))AssetDatabase.AddObjectToAsset(mat,asset);asset.material=mat;
   asset.parts=data.groups.Select(g=>{
    var mesh=oldParts.FirstOrDefault(p=>p.name==g.name)?.mesh;if(!mesh)mesh=new Mesh{name=id+" / "+g.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};else{mesh.Clear();mesh.ClearBlendShapes();}mesh.vertices=Vectors(g.vertices);mesh.normals=Vectors(g.normals);var uv=new Vector2[g.uv.Length/2];for(int i=0;i<uv.Length;i++)uv[i]=new Vector2(g.uv[i*2],g.uv[i*2+1]);mesh.uv=uv;mesh.triangles=g.triangles;mesh.RecalculateBounds();mesh.RecalculateTangents();
    if(g.poseA.Length>0){mesh.AddBlendShapeFrame("Blender idle / A",100,Vectors(g.poseA),null,null);mesh.AddBlendShapeFrame("Blender idle / B",100,Vectors(g.poseB),null,null);}
    if(!AssetDatabase.Contains(mesh))AssetDatabase.AddObjectToAsset(mesh,asset);EditorUtility.SetDirty(mesh);return new ReferencePart{name=g.name,mesh=mesh,pivot=new Vector3(g.pivot[0],g.pivot[1],g.pivot[2])};
   }).ToArray();EditorUtility.SetDirty(asset);Debug.Log("REFERENCE_MODEL_BAKED "+id);
  }
  AssetDatabase.SaveAssets();
 }
}
