using UnityEngine;
using System;
using System.Collections.Generic;
public static class PulseSculpted {
 [Serializable] class Model { public Piece[] parts; }
 [Serializable] class Piece { public string name,material;public float[] vertices,normals,uv;public int[] triangles; }
 static readonly Dictionary<string,Material> materials=new();
 static readonly Dictionary<string,Mesh[]> meshes=new();
 static readonly Dictionary<string,Model> models=new();
 static Texture2D grain,normal;
 static Material MaterialFor(string name){
  if(materials.TryGetValue(name,out var found))return found;
  Color c;float emit=0,metal=.2f,rough=.4f;
  switch(name){
   case "Slate":c=new Color(.4f,.39f,.43f);metal=.18f;break;
   case "Basalt":c=new Color(.13f,.15f,.2f);break;
   case "Titanium":c=new Color(.43f,.46f,.52f);metal=.65f;rough=.65f;break;
   case "Cyan":c=new Color(.015f,.55f,1);emit=2.4f;break;
   case "Skin":c=new Color(.015f,.38f,.51f);emit=.25f;rough=.7f;break;
   case "Eye":c=new Color(.35f,.95f,1);emit=3;break;
   case "Gold":c=new Color(1,.5f,.065f);emit=.6f;metal=.45f;rough=.7f;break;
   case "Lamp":c=new Color(1,.68f,.25f);emit=2;break;
   case "Purple":c=new Color(.12f,.018f,.24f);metal=.4f;rough=.7f;break;
   case "Pink":c=new Color(.7f,.07f,1);emit=3;break;
   default:c=new Color(.006f,.015f,.035f);metal=.05f;rough=.5f;break;
  }
  var m=new Material(Shader.Find("Standard")){name=name};m.color=c;m.SetFloat("_Metallic",metal);m.SetFloat("_Glossiness",rough);if(emit>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*emit);}
  if(name=="Slate"||name=="Basalt"||name=="Titanium"){
   if(!grain){int size=512;grain=new Texture2D(size,size,TextureFormat.RGB24,true);grain.name="Fine stone surface";normal=new Texture2D(size,size,TextureFormat.RGBA32,true,true);normal.name="Stone micro normals";var a=new Color[size*size];var b=new Color[a.Length];for(int y=0;y<size;y++)for(int x=0;x<size;x++){float n=Mathf.PerlinNoise(x*.023f,y*.023f)*.12f+Mathf.PerlinNoise(x*.24f,y*.24f)*.08f+.78f;float dx=Mathf.PerlinNoise((x+1)*.24f,y*.24f)-Mathf.PerlinNoise(x*.24f,y*.24f),dy=Mathf.PerlinNoise(x*.24f,(y+1)*.24f)-Mathf.PerlinNoise(x*.24f,y*.24f);a[y*size+x]=new Color(n,n,n);b[y*size+x]=new Color(1,.5f+dy*.6f,1,.5f+dx*.6f);}grain.SetPixels(a);grain.Apply();normal.SetPixels(b);normal.Apply();}
   m.mainTexture=grain;m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.4f);
  } materials[name]=m;return m;
 }
 public static GameObject Make(string name,Vector3 position){
  if(!models.TryGetValue(name,out var model)){var text=Resources.Load<TextAsset>("Sculpted/"+name);if(!text)return null;model=JsonUtility.FromJson<Model>(text.text);models[name]=model;var built=new Mesh[model.parts.Length];for(int i=0;i<built.Length;i++){var p=model.parts[i];var v=new Vector3[p.vertices.Length/3];var n=new Vector3[v.Length];var uv=new Vector2[v.Length];for(int j=0;j<v.Length;j++){v[j]=new Vector3(p.vertices[j*3],p.vertices[j*3+1],p.vertices[j*3+2]);n[j]=new Vector3(p.normals[j*3],p.normals[j*3+1],p.normals[j*3+2]);uv[j]=new Vector2(p.uv[j*2],p.uv[j*2+1]);}var mesh=new Mesh(){name=name+" / "+p.name};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.vertices=v;mesh.normals=n;mesh.uv=uv;mesh.triangles=p.triangles;mesh.RecalculateBounds();mesh.RecalculateTangents();built[i]=mesh;}meshes[name]=built;}
  var root=new GameObject(name);root.transform.position=position;Transform deck=null;if(name=="Platform"){deck=new GameObject("Deck • moving surface").transform;deck.SetParent(root.transform,false);}
  for(int i=0;i<model.parts.Length;i++){var p=model.parts[i];var o=new GameObject(p.name);o.transform.SetParent(deck&&p.name.StartsWith("Deck_")?deck:root.transform,false);o.AddComponent<MeshFilter>().sharedMesh=meshes[name][i];o.AddComponent<MeshRenderer>().sharedMaterial=MaterialFor(p.material);}
  return root;
 }
}
