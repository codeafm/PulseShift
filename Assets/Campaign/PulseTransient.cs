using UnityEngine;
public class PulseTransient:MonoBehaviour {
 public LineRenderer ring;public ParticleSystem particles;public float radius=3;public Color baseColor=Color.white;
 CampaignGame game;float age;
 void Start(){game=FindFirstObjectByType<CampaignGame>();}
 void Update(){bool pause=game&&game.MotionPaused;if(particles){if(pause&&particles.isPlaying)particles.Pause();else if(!pause&&particles.isPaused)particles.Play();}if(pause)return;age+=Time.deltaTime;if(ring){float t=Mathf.Clamp01(age/.7f);transform.localScale=Vector3.one*Mathf.Lerp(.15f,radius,1-(1-t)*(1-t));var c=baseColor;c.a*=1-t;ring.startColor=ring.endColor=c;ring.widthMultiplier=.05f*(1-t);}if(age>1.4f)Destroy(gameObject);}
}
