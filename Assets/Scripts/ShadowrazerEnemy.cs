using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D),typeof(CircleCollider2D))]
public sealed class ShadowrazerEnemy:MonoBehaviour,IEnemySupportTarget
{
 [SerializeField] float detectionRadius=7f,preferredDistance=3.4f,moveSpeed=1.25f,warningSeconds=.65f,cooldownSeconds=2.8f,recoverySeconds=.55f,impactRadius=.72f;
 [SerializeField] int damage=34,hitsToKill=3;[SerializeField] Sprite[] frames,effectFrames;
 TopDownPlayer player;Rigidbody2D body;SpriteRenderer visual;float nextAttack,animTime;bool acting;int hits;Coroutine cast;GameObject activeEffect;
 void Awake(){player=FindFirstObjectByType<TopDownPlayer>();body=GetComponent<Rigidbody2D>();visual=GetComponent<SpriteRenderer>();hits=hitsToKill;body.gravityScale=0;body.freezeRotation=true;GetComponent<CircleCollider2D>().isTrigger=true;}
 void Update(){if(player==null||!player.IsAlive){body.linearVelocity=Vector2.zero;Anim(0,4,8);return;}Vector2 d=player.transform.position-transform.position;visual.flipX=d.x<0;if(acting)return;if(d.magnitude>detectionRadius){body.linearVelocity=Vector2.zero;Anim(0,4,8);return;}if(Mathf.Abs(d.magnitude-preferredDistance)>.65f){body.linearVelocity=(d.magnitude>preferredDistance?d.normalized:-d.normalized)*moveSpeed;Anim(4,4,9);return;}body.linearVelocity=Vector2.zero;Anim(0,4,8);if(Time.time>=nextAttack)cast=StartCoroutine(Cast(d.magnitude));}
 IEnumerator Cast(float distance){Vector2 target=player.transform.position;if(!EnemyCombatCoordinator.TryReserve(this,target,impactRadius+.2f,warningSeconds+.5f)){nextAttack=Time.time+.3f;yield break;}acting=true;body.linearVelocity=Vector2.zero;int variant=distance<2.5f?0:distance<4.5f?1:2;int castStart=8+variant*4;activeEffect=new GameObject("Shadowrazer Warning");activeEffect.transform.position=target;var sr=activeEffect.AddComponent<SpriteRenderer>();sr.sortingOrder=32;yield return Play(castStart,4,.32f);float e=0;while(e<warningSeconds){e+=Time.deltaTime;if(effectFrames.Length>=variant*3+2)sr.sprite=effectFrames[variant*3+Mathf.FloorToInt(e*8)%2];yield return null;}if(effectFrames.Length>=variant*3+3)sr.sprite=effectFrames[variant*3+2];if(player!=null&&player.IsAlive&&Vector2.Distance(player.transform.position,target)<=impactRadius)player.ReceiveDamage(damage);EnemyCombatCoordinator.Release(this);yield return new WaitForSeconds(.14f);yield return Play(20,3,recoverySeconds);Destroy(activeEffect);activeEffect=null;nextAttack=Time.time+cooldownSeconds;acting=false;cast=null;}
 IEnumerator Play(int s,int c,float d){float e=0;while(e<d){e+=Time.deltaTime;if(frames.Length>=s+c)visual.sprite=frames[s+Mathf.Min(c-1,Mathf.FloorToInt(e/d*c))];yield return null;}}
 void Anim(int s,int c,float fps){if(frames==null||frames.Length<s+c)return;animTime+=Time.deltaTime;visual.sprite=frames[s+Mathf.FloorToInt(animTime*fps)%c];}
 public void TakeHit(){hits--;if(acting&&cast!=null){StopCoroutine(cast);EnemyCombatCoordinator.Release(this);acting=false;cast=null;body.linearVelocity=Vector2.zero;if(activeEffect!=null)Destroy(activeEffect);activeEffect=null;nextAttack=Time.time+1f;}if(hits<=0)EnemyDefeat.PlayDeath(this,visual,body,frames,27,4);}
 public int CurrentHits=>hits;
 public int MaximumHits=>hitsToKill;
 public void RestoreOneHit(){if(enabled&&hits>0)hits=Mathf.Min(hitsToKill,hits+1);}
 void OnDestroy(){EnemyCombatCoordinator.Release(this);if(activeEffect!=null)Destroy(activeEffect);}
}
