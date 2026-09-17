using UnityEngine;
public static class PulseEffects {
 static Material sparkle;
 public static void Burst(Transform parent,Vector3 position,Color color,int count=22,float size=1){
  if(MobilePerformance.Lite)count=Mathf.Max(4,Mathf.RoundToInt(count*.45f));
  if(!sparkle)sparkle=new Material(Resources.Load<Shader>("PulseSpark"));
  var o=new GameObject("FX • energy motes");o.transform.SetParent(parent,true);o.transform.position=position;
  var p=o.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=p.main;main.loop=false;main.duration=.8f;main.startLifetime=new ParticleSystem.MinMaxCurve(.35f,.85f);main.startSpeed=new ParticleSystem.MinMaxCurve(.35f*size,1.8f*size);main.startSize=new ParticleSystem.MinMaxCurve(.045f,.12f);main.startColor=color;main.maxParticles=96;main.simulationSpace=ParticleSystemSimulationSpace.World;
  var emission=p.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)count)});var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.08f;
  var alpha=new Gradient();alpha.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)});var life=p.colorOverLifetime;life.enabled=true;life.color=alpha;
  p.GetComponent<ParticleSystemRenderer>().sharedMaterial=sparkle;o.AddComponent<PulseTransient>().particles=p;p.Play();
 }
 public static void Wave(Transform parent,Vector3 position,Color color,float radius=3){var ring=CampaignVisuals.Ring(parent,"FX • expanding wave",1,Color.white,.045f,0);ring.transform.position=position;ring.startColor=ring.endColor=color;var t=ring.gameObject.AddComponent<PulseTransient>();t.ring=ring;t.radius=radius;t.baseColor=color;}
}
