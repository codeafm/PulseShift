using UnityEngine;
using PulseCampaign;
using System.Collections.Generic;

public static class CampaignVisuals {
 static readonly Dictionary<string,Material> mats=new();
 public static Material Energy(string name,Color color){if(mats.TryGetValue(name,out var m)&&m)return m;m=new Material(Resources.Load<Shader>("PulseEnergy")){name=name};m.SetColor("_Color",color);mats[name]=m;return m;}
 public static LineRenderer Line(Transform parent,string name,Vector3[] points,Material m,float width){var go=new GameObject(name);go.transform.SetParent(parent,false);var l=go.AddComponent<LineRenderer>();l.useWorldSpace=false;l.positionCount=points.Length;l.SetPositions(points);l.widthMultiplier=width;l.sharedMaterial=m;l.numCornerVertices=3;l.numCapVertices=3;return l;}
 public static LineRenderer Ring(Transform root,string name,float radius,Color color,float width=.018f,float y=.16f){var p=new Vector3[65];for(int i=0;i<p.Length;i++){float a=i*Mathf.PI/32;p[i]=new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius);}return Line(root,name,p,Energy(name,color),width);}
 public static Transform Deck(Transform p)=>ReferenceArt.Part(p,"Deck • moving surface");
 public static Transform Spikes(Transform p)=>ReferenceArt.Part(p,"Spikes");
 public static GameObject Platform(Island node,int index){
  string model=node.kind==TileKind.Spikes?"Trap":node.kind==TileKind.Fragile?"Breakable":node.kind==TileKind.Conveyor?"Move":node.kind==TileKind.Phase||node.kind==TileKind.Teleport||node.kind==TileKind.Relay||node.kind==TileKind.Rift?"Rotate":"Normal";
  var p=ReferenceArt.Make(model,new Vector3(node.x*.96f,node.y,node.z*1.42f));p.name=$"Island {index:00} · {node.kind}";var marker=p.AddComponent<CampaignIsland>();marker.index=index;marker.kind=node.kind;
  var box=p.AddComponent<BoxCollider>();box.center=new Vector3(0,-.08f,0);box.size=new Vector3(1.66f,.38f,1.55f);
  var signs=new GameObject("Trap and route markers").transform;signs.SetParent(p.transform,false);
  if(node.kind==TileKind.Phase){Ring(Deck(p.transform),"Phase circuit",.84f,node.phase==0?new Color(.01f,.5f,1,.8f):new Color(1,.5f,.02f,.8f),.025f);}
  if(node.kind==TileKind.Teleport){Ring(signs,"Linked rift",.57f,new Color(.48f,.04f,1,.8f),.028f);Ring(signs,"Linked rift inner",.43f,new Color(.48f,.04f,1,.5f),.018f);}
  Ring(signs,"Move highlight",.94f,new Color(.025f,.55f,1,.65f),.022f).gameObject.SetActive(false);
  Ring(signs,"Guard next step",.75f,new Color(1,.22f,.025f,.65f),.018f).gameObject.SetActive(false);
  CampaignVarietyArt.Decorate(p.transform,node);
  return p;
 }
 public static void CreateScenery(){
  var root=new GameObject("ATMOSPHERE • Blender basalt islands");var random=new System.Random(42);
  for(int i=0;i<22;i++){
   float side=i%2==0?-1:1;var pos=new Vector3(side*(5+(float)random.NextDouble()*6),-3-(float)random.NextDouble()*4,-9+(float)random.NextDouble()*28);
   var island=ReferenceArt.Make("Normal",pos);island.name="Distant island "+i;island.transform.SetParent(root.transform,true);island.transform.localScale=Vector3.one*(.6f+(float)random.NextDouble()*.8f);
   var cliff=ReferenceArt.Part(island.transform,"Base");cliff.localScale=new Vector3(1,2.6f,1);
   Line(island.transform,"Falling blue energy",new[]{new Vector3(.2f,-.6f,-.72f),new Vector3(.2f,-2.5f,-.72f),new Vector3(.2f,-4.3f,-.72f)},Energy("Distant light falls",new Color(.025f,.25f,1,.45f)),.025f);
  }
 }
 public static GameObject MakeBoard(Level level){
  var root=new GameObject("LEVEL • editable Blender campaign");var nodes=new List<GameObject>();
  for(int i=0;i<level.islands.Length;i++){var p=Platform(level.islands[i],i);p.transform.SetParent(root.transform,true);var edges=new List<int>();foreach(var e in level.links){if(e.a==i)edges.Add(e.b);if(e.b==i)edges.Add(e.a);}p.GetComponent<CampaignIsland>().connectedIslands=edges.ToArray();nodes.Add(p);}
  CompactRewardSpurs(nodes,level);
  for(int i=0;i<level.islands.Length;i++){var n=level.islands[i];if(n.kind==TileKind.Conveyor){var d=nodes[n.destination].transform.position-nodes[i].transform.position;d.y=0;Deck(nodes[i].transform).rotation=Quaternion.LookRotation(d);}if(n.gem>=0){var crystal=ReferenceArt.Make("Crystal",nodes[i].transform.position+Vector3.up*.18f);crystal.name="Crystal "+n.gem;crystal.transform.SetParent(root.transform,true);}}
  var hero=ReferenceArt.Make("Spirit",nodes[level.start].transform.position+Vector3.up*.18f);hero.name="Hero";hero.transform.SetParent(root.transform,true);
  Ring(hero.transform,"Spirit contact aura",.22f,new Color(.015f,.75f,1,.65f),.014f,.005f);
  var light=new GameObject("Spirit light").AddComponent<Light>();light.transform.SetParent(hero.transform,false);light.transform.localPosition=Vector3.up*.5f;light.type=LightType.Point;light.color=new Color(.025f,.65f,1);light.intensity=1.4f;light.range=2.4f;
  var portal=ReferenceArt.Make("Portal",nodes[level.goal].transform.position+Vector3.up*.18f);portal.name="Exit portal";portal.transform.localScale=Vector3.one*1.12f;portal.transform.SetParent(root.transform,true);
  var portalLight=new GameObject("Portal light").AddComponent<Light>();portalLight.transform.SetParent(portal.transform,false);portalLight.transform.localPosition=new Vector3(0,1,-.4f);portalLight.type=LightType.Point;portalLight.color=new Color(.025f,.5f,1);portalLight.intensity=2;portalLight.range=3.5f;
  for(int i=0;i<level.patrols.Length;i++){var g=ReferenceArt.Make("Shadow",nodes[Rules.GuardAt(level.patrols[i],0)].transform.position+Vector3.up*.18f);g.name="Guard "+i;g.transform.SetParent(root.transform,true);CampaignVarietyArt.DecorateEnemy(g,level.patrols[i]);}
  var routes=new GameObject("Route connections").transform;routes.SetParent(root.transform,false);foreach(var e in level.links){Vector3 a=nodes[e.a].transform.position,b=nodes[e.b].transform.position;var direction=(b-a).normalized;a+=direction*1.04f+Vector3.up*.08f;b-=direction*1.04f-Vector3.up*.08f;Line(routes,$"Route {e.a}-{e.b}",new[]{a,(a+b)/2-Vector3.up*.025f,b},Energy("Visible route chain",new Color(.035f,.65f,1,.88f)),.031f);}
  if(level.layout!=null)CampaignVarietyArt.Theme(root,level.biome);
  return root;
 }
 // Fold isolated reward wings into unused space, not into a new logical route.
 // Only degree-one leaf positions move. Indices, links, guards and solver data stay intact.
 static void CompactRewardSpurs(List<GameObject> nodes,Level level){
  for(int i=0;i<nodes.Count;i++){
   var links=nodes[i].GetComponent<CampaignIsland>().connectedIslands;
   if(links.Length!=1||i==level.start||i==level.goal)continue;
   var original=nodes[i].transform.position;var parent=nodes[links[0]].transform.position;
   if(Mathf.Abs(original.x)<Mathf.Abs(parent.x)+.5f)continue;
   var best=original;float score=Mathf.Abs(original.x)*3;
   for(int j=0;j<48;j++){
    float angle=j*Mathf.PI/24;var candidate=parent+new Vector3(Mathf.Cos(angle)*2.35f,original.y-parent.y,Mathf.Sin(angle)*2.85f);
    bool clear=true;for(int k=0;k<nodes.Count;k++){if(k==i)continue;var d=nodes[k].transform.position-candidate;d.y=0;if(d.sqrMagnitude<2.2f*2.2f){clear=false;break;}}
    if(!clear)continue;
    // Keep the new spur close to its old row and make it clearly shorter than long teleports.
    float cost=Mathf.Abs(candidate.x)*3+Mathf.Abs(candidate.z-original.z)*.4f;
    if(cost<score){score=cost;best=candidate;}
   }
   nodes[i].transform.position=best;
  }
 }
}
