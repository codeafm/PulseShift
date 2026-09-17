using UnityEngine;
public class PulseAura:MonoBehaviour {
 public static void Create(GameObject hero){var root=new GameObject("Pulse field • animated rings");root.transform.SetParent(hero.transform,false);var m=new Material(Resources.Load<Shader>("PulseEnergy"));m.SetColor("_Color",new Color(.05f,.65f,1,1));for(int j=0;j<3;j++){var go=new GameObject("Pulse ring "+j);go.transform.SetParent(root.transform,false);var l=go.AddComponent<LineRenderer>();l.sharedMaterial=m;l.useWorldSpace=false;l.positionCount=97;l.widthMultiplier=.012f;l.startColor=l.endColor=new Color(.1f,.8f,1,.25f);for(int i=0;i<97;i++){float a=i*Mathf.PI/48;l.SetPosition(i,new Vector3(Mathf.Cos(a)*(1.1f+j*.45f),.05f,Mathf.Sin(a)*(1.1f+j*.45f)));}}root.AddComponent<PulseAura>();}
 void Update(){int j=0;foreach(var l in GetComponentsInChildren<LineRenderer>()){float t=Mathf.Repeat(Time.time*.2f+j*.22f,1);l.transform.localScale=Vector3.one*(.65f+t*.7f);l.startColor=l.endColor=new Color(.1f,.8f,1,.28f*(1-t));j++;}}
}
