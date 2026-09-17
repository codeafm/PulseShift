using UnityEngine;
public static class PulseScenery {
 public static void Create(){
  var root=new GameObject("ATMOSPHERE • distant islands");var random=new System.Random(42);
  for(int i=0;i<34;i++){
   float sign=i%2==0?-1:1;float x=sign*(3.8f+(float)random.NextDouble()*4),z=-9+(float)random.NextDouble()*24,y=-2.2f-(float)random.NextDouble()*3;
   var island=PulseSculpted.Make("Platform",new Vector3(x,y,z));island.name="Distant basalt island "+i;island.transform.SetParent(root.transform,true);float scale=.4f+(float)random.NextDouble()*.6f;island.transform.localScale=Vector3.one*scale;
   foreach(var t in island.GetComponentsInChildren<Transform>()){if(t.name.StartsWith("Foundation")){t.localScale=new Vector3(1,6+(float)random.NextDouble()*7,1);t.localPosition+=Vector3.down*.4f;}if(t.name.StartsWith("Deck_"))t.gameObject.SetActive(false);}
  }
 }
}
