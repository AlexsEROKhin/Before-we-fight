using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D),typeof(CircleCollider2D))]
public sealed class WarcryBruteEnemy:MonoBehaviour,IEnemySupportTarget
{
 [SerializeField] float detectionRadius=5.8f,attackDistance=.9f,moveSpeed=.9f,enragedSpeed=1.35f,roarDuration=.55f,enrageSeconds=4f,windupSeconds=.65f,cooldownSeconds=1.9f,impactRadius=1.05f;
 [SerializeField] int damage=40,hitsToKill=5;[SerializeField] Sprite[] frames,effectFrames;
 TopDownPlayer player;Rigidbody2D body;SpriteRenderer visual;float nextAttack,animTime,enragedUntil;bool acting,hasRoared;int hits;Coroutine action;GameObject activeEffect;EnemyTelegraph activeWarning;
 void Awake(){player=FindFirstObjectByType<TopDownPlayer>();body=GetComponent<Rigidbody2D>();visual=GetComponent<SpriteRenderer>();hits=hitsToKill;body.gravityScale=0;body.freezeRotation=true;GetComponent<CircleCollider2D>().isTrigger=true;}
 void Update(){if(player==null||!player.IsAlive){body.linearVelocity=Vector2.zero;Anim(0,4,7);return;}Vector2 d=player.transform.position-transform.position;visual.flipX=d.x<0;if(acting)return;if(d.magnitude>detectionRadius){body.linearVelocity=Vector2.zero;Anim(0,4,7);hasRoared=false;return;}if(!hasRoared){action=StartCoroutine(Roar());return;}if(d.magnitude>attackDistance){float speed=Time.time<enragedUntil?enragedSpeed:moveSpeed;body.linearVelocity=d.normalized*speed;Anim(4,4,8);}else{body.linearVelocity=Vector2.zero;Anim(0,4,7);if(Time.time>=nextAttack)action=StartCoroutine(HeavyAttack());}}
 IEnumerator Roar(){acting=true;hasRoared=true;body.linearVelocity=Vector2.zero;yield return Play(8,3,.25f);activeEffect=CreateEffect("War Cry",transform.position);var sr=activeEffect.GetComponent<SpriteRenderer>();float e=0;while(e<roarDuration){e+=Time.deltaTime;if(effectFrames.Length>=4)sr.sprite=effectFrames[Mathf.Min(3,Mathf.FloorToInt(e/roarDuration*4))];if(frames.Length>=15)visual.sprite=frames[11+Mathf.FloorToInt(e*8)%4];yield return null;}Destroy(activeEffect);activeEffect=null;enragedUntil=Time.time+enrageSeconds;acting=false;action=null;}
 IEnumerator HeavyAttack()
 {
  Vector2 target=player.transform.position;
  activeWarning=EnemyTelegraph.TryBegin(this,target,impactRadius,windupSeconds+1.2f,new Color(1f,.3f,.18f,.85f));
  if(activeWarning==null){nextAttack=Time.time+.3f;yield break;}
  acting=true;
  body.linearVelocity=Vector2.zero;
  yield return Play(15,4,windupSeconds);
  yield return Play(19,4,.24f);
  activeEffect=CreateEffect("Heavy Impact",target);
  var sr=activeEffect.GetComponent<SpriteRenderer>();
  for(int i=4;i<effectFrames.Length;i++)
  {
   sr.sprite=effectFrames[i];
   yield return new WaitForSeconds(.07f);
  }
  if(player!=null&&player.IsAlive&&Vector2.Distance(player.transform.position,target)<=impactRadius)
   player.ReceiveDamage(damage);
  if(activeWarning!=null)activeWarning.Finish();
  activeWarning=null;
  yield return Play(23,4,.5f);
  Destroy(activeEffect);
  activeEffect=null;
  nextAttack=Time.time+cooldownSeconds;
  acting=false;
  action=null;
 }
 GameObject CreateEffect(string name,Vector2 position){var go=new GameObject(name);go.transform.position=position;var sr=go.AddComponent<SpriteRenderer>();sr.sortingOrder=32;if(effectFrames.Length>0)sr.sprite=effectFrames[0];return go;}
 IEnumerator Play(int s,int c,float d){float e=0;while(e<d){e+=Time.deltaTime;if(frames.Length>=s+c)visual.sprite=frames[s+Mathf.Min(c-1,Mathf.FloorToInt(e/d*c))];yield return null;}}
 void Anim(int s,int c,float fps){if(frames==null||frames.Length<s+c)return;animTime+=Time.deltaTime;visual.sprite=frames[s+Mathf.FloorToInt(animTime*fps)%c];}
 public void TakeHit(){hits--;if(acting&&action!=null){StopCoroutine(action);if(activeWarning!=null)activeWarning.Finish();activeWarning=null;acting=false;action=null;body.linearVelocity=Vector2.zero;if(activeEffect!=null)Destroy(activeEffect);activeEffect=null;nextAttack=Time.time+1f;}if(hits<=0)EnemyDefeat.PlayDeath(this,visual,body,frames,32,5,2);}
 public int CurrentHits=>hits;
 public int MaximumHits=>hitsToKill;
 public void RestoreOneHit(){if(enabled&&hits>0)hits=Mathf.Min(hitsToKill,hits+1);}
 void OnDestroy(){if(activeWarning!=null)activeWarning.Finish();if(activeEffect!=null)Destroy(activeEffect);}
}
