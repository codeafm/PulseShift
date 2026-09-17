using UnityEngine;
using PulseCampaign;
using System.Collections.Generic;

public static class LegacyCampaignVisuals {
 static readonly Dictionary<string,Material> mats=new();static Mesh spikeMesh,basaltMesh;
 public static Material Energy(string name,Color color){if(mats.TryGetValue(name,out var m)&&m)return m;m=new Material(Resources.Load<Shader>("PulseEnergy")){name=name};m.SetColor("_Color",color);mats[name]=m;return m;}
 static Material Solid(string name,Color color,float glow=0){if(mats.TryGetValue(name,out var m)&&m)return m;m=new Material(Shader.Find("Standard")){name=name,color=color};m.SetFloat("_Metallic",.35f);m.SetFloat("_Glossiness",.45f);if(glow>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*glow);}mats[name]=m;return m;}
 public static LineRenderer Line(Transform parent,string name,Vector3[] points,Material m,float width){var go=new GameObject(name);go.transform.SetParent(parent,false);var l=go.AddComponent<LineRenderer>();l.useWorldSpace=false;l.positionCount=points.Length;l.SetPositions(points);l.widthMultiplier=width;l.sharedMaterial=m;l.numCornerVertices=3;l.numCapVertices=3;return l;}
 public static LineRenderer Ring(Transform root,string name,float radius,Color color,float width=.018f,float y=.16f){var p=new Vector3[65];for(int i=0;i<p.Length;i++){float a=i*Mathf.PI/32;p[i]=new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius);}return Line(root,name,p,Energy(name,color),width);}
 public static Transform Deck(Transform platform){var deck=platform.Find("Deck • moving surface");return deck?deck:platform;}
 public static GameObject Platform(Island node,int index){
  var p=PulseSculpted.Make("Platform",new Vector3(node.x,node.y,node.z));p.name=$"Island {index:00} · {node.kind}";var marker=p.AddComponent<CampaignIsland>();marker.index=index;marker.kind=node.kind;
  ExtendCliffs(p,2.5f);
  var box=p.AddComponent<BoxCollider>();box.center=new Vector3(0,-.1f,0);box.size=new Vector3(1.65f,.6f,1.5f);
  var signs=new GameObject("Trap and route markers").transform;signs.SetParent(p.transform,false);
  switch(node.kind){
   case TileKind.Spikes:
    var spikes=new GameObject("Spikes").transform;spikes.SetParent(signs,false);spikes.localPosition=Vector3.up*.13f;
    if(!spikeMesh){var v=new[]{new Vector3(-.11f,0,-.1f),new Vector3(.11f,0,-.1f),new Vector3(.11f,0,.1f),new Vector3(-.11f,0,.1f),new Vector3(0,.6f,0)};spikeMesh=new Mesh(){name="Obsidian spike"};spikeMesh.vertices=v;spikeMesh.triangles=new[]{0,4,1,1,4,2,2,4,3,3,4,0,0,1,2,0,2,3};spikeMesh.RecalculateNormals();}
    for(int i=0;i<7;i++){float a=i*Mathf.PI/3;var g=new GameObject("Razor");g.transform.SetParent(spikes,false);g.transform.localPosition=i==6?Vector3.zero:new Vector3(Mathf.Cos(a)*.48f,0,Mathf.Sin(a)*.48f);g.AddComponent<MeshFilter>().sharedMesh=spikeMesh;g.AddComponent<MeshRenderer>().sharedMaterial=Solid("Red crystal spikes",new Color(1,.07f,.025f),1.4f);}
    Ring(signs,"Spike warning",.8f,new Color(1,.05f,.015f,.55f),.027f);break;
   case TileKind.Fragile:
    var black=Solid("Fracture",new Color(.008f,.01f,.017f));Line(signs,"Fracture A",new[]{new Vector3(-.75f,.14f,-.5f),new Vector3(-.2f,.14f,-.12f),new Vector3(.13f,.14f,.27f),new Vector3(.7f,.14f,.6f)},black,.04f);Line(signs,"Fracture B",new[]{new Vector3(-.2f,.14f,-.12f),new Vector3(.2f,.14f,-.4f),new Vector3(.75f,.14f,-.34f)},black,.026f);break;
   case TileKind.Phase:
    var color=node.phase==0?new Color(.01f,.6f,1,.9f):new Color(1,.5f,.035f,.9f);Ring(Deck(p.transform),"Phase circuit",.8f,color,.024f);
    foreach(var tr in p.GetComponentsInChildren<Transform>())if(tr.name.StartsWith("Foundation")||tr.name.StartsWith("Rock")||tr.name.StartsWith("Basalt rock")||tr.name.StartsWith("Blue fissure"))tr.gameObject.SetActive(false);break;
   case TileKind.Conveyor:
    var arrow=new GameObject("Directional arrow").transform;arrow.SetParent(signs,false);
    var blue=Energy("Conveyor arrow",new Color(.025f,.75f,1,1));Line(arrow,"Arrow",new[]{new Vector3(0,.16f,-.55f),new Vector3(0,.16f,.55f),new Vector3(-.22f,.16f,.3f),new Vector3(0,.16f,.55f),new Vector3(.22f,.16f,.3f)},blue,.055f);break;
   case TileKind.Teleport:
    Ring(signs,"Teleport outer",.61f,new Color(.4f,.15f,1,.9f),.04f);Ring(signs,"Teleport inner",.43f,new Color(.7f,.4f,1,.8f),.025f);break;
  }
  Ring(signs,"Move highlight",.91f,new Color(.05f,.65f,1,.55f),.022f).gameObject.SetActive(false);
  Ring(signs,"Guard next step",.71f,new Color(1,.3f,.02f,.5f),.018f).gameObject.SetActive(false);
  return p;
 }
 static void ExtendCliffs(GameObject island,float stretch){
  // Separate hexagonal rock columns produce a broken silhouette instead of one stretched drum.
  if(!basaltMesh){
   var vertices=new List<Vector3>();var triangles=new List<int>();
   for(int side=0;side<6;side++){
    float a=side*Mathf.PI/3,b=(side+1)*Mathf.PI/3;var p=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));var q=new Vector3(Mathf.Cos(b),0,Mathf.Sin(b));int at=vertices.Count;
    vertices.AddRange(new[]{p,q,new Vector3(q.x*.88f,-1,q.z*.88f),new Vector3(p.x*.88f,-1,p.z*.88f)});triangles.AddRange(new[]{at,at+1,at+2,at,at+2,at+3});
   }
   basaltMesh=new Mesh{name="Split basalt column"};basaltMesh.SetVertices(vertices);basaltMesh.SetTriangles(triangles,0);basaltMesh.RecalculateNormals();basaltMesh.RecalculateBounds();
  }
  var random=new System.Random(Mathf.RoundToInt(island.transform.position.x*97+island.transform.position.z*53));
  for(int i=0;i<13;i++){
   float angle=i*Mathf.PI/6;float radius=i==12?0:.62f;float depth=stretch*(.48f+(float)random.NextDouble()*.38f);
   var column=new GameObject("Basalt rock column "+i);column.transform.SetParent(island.transform,false);column.transform.localPosition=new Vector3(Mathf.Cos(angle)*radius,-.24f,Mathf.Sin(angle)*radius);column.transform.localScale=new Vector3(i==12?.61f:.24f,depth,i==12?.61f:.24f);
   column.AddComponent<MeshFilter>().sharedMesh=basaltMesh;column.AddComponent<MeshRenderer>().sharedMaterial=Solid("Basalt column "+i%3,new Color(.075f+i%3*.012f,.1f+i%3*.014f,.17f+i%3*.012f));
   if(i==8||i==10){Vector3 p=column.transform.localPosition+new Vector3(0,-.3f,-.22f);Line(island.transform,"Blue fissure "+i,new[]{p,p+Vector3.down*(depth*.48f)},Energy("Basalt blue fissures",new Color(.01f,.35f,1,.7f)),.013f);}
  }
 }
 public static void CreateScenery(){
  var root=new GameObject("ATMOSPHERE • floating basalt cliffs");var random=new System.Random(42);
  for(int i=0;i<30;i++){
   float side=i%2==0?-1:1;var pos=new Vector3(side*(5+(float)random.NextDouble()*6),-3-(float)random.NextDouble()*4,-9+(float)random.NextDouble()*28);
   var island=PulseSculpted.Make("Platform",pos);island.name="Background island "+i;island.transform.SetParent(root.transform,true);ExtendCliffs(island,5+(float)random.NextDouble()*3);island.transform.localScale=Vector3.one*(.55f+(float)random.NextDouble()*.9f);
   Line(island.transform,"Falling blue energy",new[]{new Vector3(.2f,-.7f,-.7f),new Vector3(.2f,-3,-.7f),new Vector3(.2f,-5.3f,-.7f)},Energy("Distant light falls",new Color(.025f,.35f,1,.55f)),.025f);
  }
 }
 public static GameObject MakeBoard(Level level){
  var root=new GameObject("LEVEL • editable campaign islands");var nodes=new List<GameObject>();
  for(int i=0;i<level.islands.Length;i++){var p=Platform(level.islands[i],i);p.transform.SetParent(root.transform,true);var edges=new List<int>();foreach(var e in level.links){if(e.a==i)edges.Add(e.b);if(e.b==i)edges.Add(e.a);}p.GetComponent<CampaignIsland>().connectedIslands=edges.ToArray();nodes.Add(p);}
  for(int i=0;i<level.islands.Length;i++){var n=level.islands[i];if(n.kind==TileKind.Conveyor){var a=nodes[i].transform.Find("Trap and route markers/Directional arrow");var d=nodes[n.destination].transform.position-nodes[i].transform.position;d.y=0;a.rotation=Quaternion.LookRotation(d);}if(n.gem>=0){var crystal=PulseSculpted.Make("Crystal",nodes[i].transform.position+Vector3.up*.18f);crystal.name="Crystal "+n.gem;crystal.transform.SetParent(root.transform,true);}}
  var hero=PulseSculpted.Make("Spark",nodes[level.start].transform.position+Vector3.up*.18f);hero.name="Hero";hero.transform.localScale=Vector3.one*1.15f;hero.transform.SetParent(root.transform,true);PulseAura.Create(hero);
  var portal=PulseSculpted.Make("Portal",nodes[level.goal].transform.position+Vector3.up*.18f);portal.name="Exit portal";portal.transform.localScale=Vector3.one*1.17f;portal.transform.SetParent(root.transform,true);
  for(int i=0;i<level.patrols.Length;i++){var g=PulseSculpted.Make("Enemy",nodes[Rules.GuardAt(level.patrols[i],0)].transform.position+Vector3.up*.18f);g.name="Guard "+i;g.transform.SetParent(root.transform,true);}
  var routes=new GameObject("Route connections").transform;routes.SetParent(root.transform,false);foreach(var e in level.links){Vector3 a=nodes[e.a].transform.position,b=nodes[e.b].transform.position;var direction=(b-a).normalized;a+=direction*.84f;b-=direction*.84f;Line(routes,$"Route {e.a}-{e.b}",new[]{a,(a+b)/2-Vector3.up*.17f,b},Solid("Distant connecting chain",new Color(.12f,.19f,.27f)),.025f);}
  return root;
 }
}
