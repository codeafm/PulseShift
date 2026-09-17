using UnityEngine;

// Runtime art is independent of the game rules; the original Blender assets remain editable.
public static class PulseArt {
 static Material rock,metal,top,cyan,gold,dark,violet;
 static Material Mat(string n,Color c,float emission=0){var m=new Material(Shader.Find("Standard"));m.name=n;m.color=c;m.SetFloat("_Metallic",.35f);m.SetFloat("_Glossiness",.32f);if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*emission);}return m;}
 static void Init(){if(rock)return;rock=Mat("Basalt",new Color(.12f,.15f,.23f));metal=Mat("Titanium",new Color(.34f,.39f,.48f));top=Mat("Weathered slate",new Color(.24f,.28f,.36f));cyan=Mat("Ion light",new Color(.02f,.7f,1),1.5f);gold=Mat("Amber",new Color(1,.46f,.045f),2);dark=Mat("Visor",new Color(.008f,.016f,.045f));violet=Mat("Shadow",new Color(.09f,.015f,.17f));
  var tex=new Texture2D(128,128);var colors=new Color[128*128];for(int y=0;y<128;y++)for(int x=0;x<128;x++){float n=.88f+.12f*Mathf.PerlinNoise(x*.19f,y*.19f);colors[y*128+x]=new Color(n,n,n);}tex.SetPixels(colors);tex.Apply();top.mainTexture=tex;rock.mainTexture=tex;
 }
 static GameObject Part(GameObject root,string n,Mesh mesh,Vector3 pos,Material mat){var o=new GameObject(n);o.transform.SetParent(root.transform,false);o.transform.localPosition=pos;o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mat;return o;}
 static Mesh Prism(float radius,float height,int sides=6,float bottom=1){var v=new Vector3[sides*2+2];for(int i=0;i<sides;i++){float a=(i+.5f)*Mathf.PI*2/sides;v[i]=new Vector3(Mathf.Cos(a)*radius,-height,Mathf.Sin(a)*radius)*new Vector3(bottom,1,bottom).x;v[i].y=-height;v[i+sides]=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);}v[sides*2]=new Vector3(0,-height,0);v[sides*2+1]=Vector3.zero;var t=new int[sides*12];for(int i=0;i<sides;i++){int j=(i+1)%sides,k=i*12;int[] f={i,j,i+sides,j,j+sides,i+sides,sides*2,j,i,sides*2+1,i+sides,j+sides};for(int a=0;a<12;a++)t[k+a]=f[a];}for(int k=0;k<t.Length;k+=3){int q=t[k+1];t[k+1]=t[k+2];t[k+2]=q;}var m=new Mesh();m.vertices=v;m.triangles=t;m.RecalculateNormals();var uv=new Vector2[v.Length];for(int i=0;i<v.Length;i++)uv[i]=new Vector2(v[i].x,v[i].z);m.uv=uv;return m;}
 static GameObject Ball(GameObject r,string n,Vector3 p,Vector3 s,Material m){var o=GameObject.CreatePrimitive(PrimitiveType.Sphere);if(Application.isPlaying)Object.Destroy(o.GetComponent<Collider>());else Object.DestroyImmediate(o.GetComponent<Collider>());o.name=n;o.transform.SetParent(r.transform,false);o.transform.localPosition=p;o.transform.localScale=s;o.GetComponent<Renderer>().sharedMaterial=m;return o;}
 static GameObject Box(GameObject r,Vector3 p,Vector3 s,Material m){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);if(Application.isPlaying)Object.Destroy(o.GetComponent<Collider>());else Object.DestroyImmediate(o.GetComponent<Collider>());o.transform.SetParent(r.transform,false);o.transform.localPosition=p;o.transform.localScale=s;o.GetComponent<Renderer>().sharedMaterial=m;return o;}
 public static void Ring(GameObject root,float radius,float y,Material m,float width=.025f,bool upright=false){var o=new GameObject("Energy circuit");o.transform.SetParent(root.transform,false);var l=o.AddComponent<LineRenderer>();l.useWorldSpace=false;l.sharedMaterial=m;l.widthMultiplier=width;l.positionCount=65;for(int i=0;i<65;i++){float a=i*Mathf.PI/32;l.SetPosition(i,upright?new Vector3(Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,0):new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius));}}
 public static GameObject Make(string name,Vector3 pos){var sculpt=PulseSculpted.Make(name,pos);if(sculpt)return sculpt;Init();var r=new GameObject(name);r.transform.position=pos;
  if(name=="Platform"){
   Part(r,"Floating basalt",Prism(.92f,.9f,6,.78f),Vector3.zero,rock);Part(r,"Chamfer rim",Prism(1,.12f),new Vector3(0,.04f,0),metal);Part(r,"Inset slab",Prism(.89f,.075f),new Vector3(0,.1f,0),top);
   for(int i=0;i<6;i++){float a=(i+.5f)*Mathf.PI/3;var p=new Vector3(Mathf.Cos(a)*.82f,-.25f,Mathf.Sin(a)*.82f);Box(r,p,new Vector3(.11f,.32f,.11f),metal);Box(r,p+Vector3.up*.06f,new Vector3(.06f,.1f,.06f),gold);Part(r,"Hanging rock",Prism(.22f,.65f+(i%3)*.2f,5,.3f),p+Vector3.down*.5f,rock);}
   Ring(r,.38f,.115f,metal,.045f);Ring(r,.32f,.119f,dark,.023f);
  }else if(name=="Crystal"){
   var v=new[]{new Vector3(0,1.1f,0),new Vector3(-.18f,.55f,-.13f),new Vector3(.18f,.55f,-.13f),new Vector3(.18f,.55f,.13f),new Vector3(-.18f,.55f,.13f),new Vector3(0,.15f,0)};var m=new Mesh();m.vertices=v;m.triangles=new[]{0,2,1,0,3,2,0,4,3,0,1,4,5,1,2,5,2,3,5,3,4,5,4,1};m.RecalculateNormals();Part(r,"Memory shard",m,Vector3.zero,gold);
  }else if(name=="Spark"||name=="Enemy"){
   bool spark=name=="Spark";var skin=spark?cyan:violet;Ball(r,"Body",new Vector3(0,.45f,0),new Vector3(.46f,.53f,.38f),skin);Ball(r,"Head",new Vector3(0,.77f,0),new Vector3(.57f,.5f,.48f),skin);Ball(r,"Face",new Vector3(0,.76f,.22f),new Vector3(.43f,.29f,.1f),dark);
   for(int i=-1;i<=1;i+=2){Ball(r,"Eye",new Vector3(i*.1f,.78f,.27f),new Vector3(.09f,.12f,.04f),spark?cyan:gold);Ball(r,"Foot",new Vector3(i*.17f,.16f,.07f),new Vector3(.16f,.17f,.24f),skin);Ball(r,"Hand",new Vector3(i*.3f,.4f,0),new Vector3(.14f,.24f,.14f),skin);if(!spark)Part(r,"Horn",Prism(.09f,.35f,5,0),new Vector3(i*.22f,1.2f,0),skin);}
   if(spark){Part(r,"Flame",Prism(.16f,.55f,8,0),new Vector3(.07f,1.4f,0),cyan);Ring(r,.4f,.12f,cyan,.02f);}
  }else if(name=="Portal"){
   Part(r,"Gate pedestal",Prism(.7f,.22f),new Vector3(0,.15f,0),metal);
   for(int i=-1;i<=1;i+=2){var o=Box(r,new Vector3(i*.62f,.85f,.12f),new Vector3(.23f,1.6f,.35f),metal);o.transform.localRotation=Quaternion.Euler(0,0,i*-12);Box(r,new Vector3(i*.58f,.9f,-.07f),new Vector3(.065f,.68f,.04f),gold);}
   Ring(r,.57f,1,cyan,.065f,true);Ring(r,.48f,1,cyan,.02f,true);Ring(r,.36f,1,cyan,.012f,true);Ring(r,.19f,1,cyan,.02f,true);
  }return r;
 }
 public static void Environment(){Init();var root=new GameObject("Distant floating archipelago");var random=new System.Random(18);for(int i=0;i<55;i++){float x=(float)random.NextDouble()*30-15;float z=(float)random.NextDouble()*32-12;if(Mathf.Abs(x)<4.5f&&z<8)continue;float y=-3-(float)random.NextDouble()*5;var p=new Vector3(x,y,z);Part(root,"Distant column",Prism(.35f+(float)random.NextDouble()*.4f,1.2f+(float)random.NextDouble()*2,6,.65f),p,rock);Box(root,p+new Vector3(0,-1.5f,-.7f),new Vector3(.035f,1.3f,.035f),cyan);}}
}
