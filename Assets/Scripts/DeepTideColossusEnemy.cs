using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D),typeof(CircleCollider2D))]
public sealed class DeepTideColossusEnemy:MonoBehaviour,IEnemySupportTarget
{
 [SerializeField] float detectionRadius=6.5f,slamDistance=2.2f,moveSpeed=.65f,chargeSeconds=1.05f,shockwaveRadius=2.15f,cooldownSeconds=4f,exhaustedSeconds=1.15f;
 [SerializeField] int damage=40,hitsToKill=7;[SerializeField] Sprite[] frames,effectFrames;
 TopDownPlayer player;Rigidbody2D body;SpriteRenderer visual;float nextAttack,animTime;bool acting;int hits;Coroutine slam;GameObject activeEffect;EnemyTelegraph activeWarning;
 void Awake(){player=FindFirstObjectByType<TopDownPlayer>();body=GetComponent<Rigidbody2D>();visual=GetComponent<SpriteRenderer>();hits=hitsToKill;body.gravityScale=0;body.freezeRotation=true;GetComponent<CircleCollider2D>().isTrigger=true;}
 void Update(){if(player==null||!player.IsAlive){body.linearVelocity=Vector2.zero;Anim(0,4,6);return;}Vector2 d=player.transform.position-transform.position;visual.flipX=d.x<0;if(acting)return;if(d.magnitude>detectionRadius){body.linearVelocity=Vector2.zero;Anim(0,4,6);}else if(d.magnitude>slamDistance){body.linearVelocity=d.normalized*moveSpeed;Anim(4,4,7);}else{body.linearVelocity=Vector2.zero;Anim(0,4,6);if(Time.time>=nextAttack)slam=StartCoroutine(Slam());}}
 IEnumerator Slam()
 {
  activeWarning=EnemyTelegraph.TryBegin(this,body.position,shockwaveRadius,chargeSeconds+2f,new Color(.2f,.65f,1f,.8f));
  if(activeWarning==null){nextAttack=Time.time+.3f;yield break;}
  acting=true;
  body.linearVelocity=Vector2.zero;
  yield return Play(8,4,.4f);
  float e=0;
  while(e<chargeSeconds)
  {
   e+=Time.deltaTime;
   if(frames.Length>=16)visual.sprite=frames[12+Mathf.FloorToInt(e*7)%4];
   yield return null;
  }
  yield return Play(16,4,.3f);
  activeEffect=new GameObject("DeepTide Shockwave");
  activeEffect.transform.position=transform.position;
  var sr=activeEffect.AddComponent<SpriteRenderer>();
  sr.sortingOrder=31;
  yield return Play(20,3,.2f);
  for(int i=0;i<effectFrames.Length;i++)
  {
   sr.sprite=effectFrames[i];
   yield return new WaitForSeconds(.07f);
  }
  if(player!=null&&player.IsAlive&&Vector2.Distance(player.transform.position,transform.position)<=shockwaveRadius)
  {player.ReceiveDamage(damage);player.ApplySlow(.55f,.65f);}
  if(activeWarning!=null)activeWarning.Finish();
  activeWarning=null;
  Destroy(activeEffect);
  activeEffect=null;
  yield return Play(23,5,exhaustedSeconds);
  nextAttack=Time.time+cooldownSeconds;
  acting=false;
  slam=null;
 }
 IEnumerator Play(int s,int c,float d){float e=0;while(e<d){e+=Time.deltaTime;if(frames.Length>=s+c)visual.sprite=frames[s+Mathf.Min(c-1,Mathf.FloorToInt(e/d*c))];yield return null;}}
 void Anim(int s,int c,float fps){if(frames==null||frames.Length<s+c)return;animTime+=Time.deltaTime;visual.sprite=frames[s+Mathf.FloorToInt(animTime*fps)%c];}
 public void TakeHit(){hits--;if(acting&&slam!=null){StopCoroutine(slam);if(activeWarning!=null)activeWarning.Finish();activeWarning=null;acting=false;slam=null;body.linearVelocity=Vector2.zero;if(activeEffect!=null)Destroy(activeEffect);activeEffect=null;nextAttack=Time.time+1.25f;}if(hits<=0)EnemyDefeat.PlayDeath(this,visual,body,frames,33,6,3);}
 public int CurrentHits=>hits;
 public int MaximumHits=>hitsToKill;
 public void RestoreOneHit(){if(enabled&&hits>0)hits=Mathf.Min(hitsToKill,hits+1);}
 void OnDestroy(){if(activeWarning!=null)activeWarning.Finish();if(activeEffect!=null)Destroy(activeEffect);}
}
