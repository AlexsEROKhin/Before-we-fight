using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D),typeof(CircleCollider2D))]
public sealed class FrostbinderEnemy:MonoBehaviour,IEnemySupportTarget
{
 [SerializeField] float detectionRadius=6f,retreatDistance=2.4f,retreatSpeed=1.1f,telegraphSeconds=.7f,cooldownSeconds=3f,recoverySeconds=.6f,impactRadius=.65f,slowMultiplier=.35f,slowSeconds=1.2f;
 [SerializeField] int damage=32,hitsToKill=3;[SerializeField] Sprite[] frames,effectFrames;
 TopDownPlayer player;Rigidbody2D body;SpriteRenderer visual;float nextAttack,animTime;bool acting;int hits;Coroutine cast;GameObject activeEffect;
 void Awake(){player=FindFirstObjectByType<TopDownPlayer>();body=GetComponent<Rigidbody2D>();visual=GetComponent<SpriteRenderer>();hits=hitsToKill;body.gravityScale=0;body.freezeRotation=true;GetComponent<CircleCollider2D>().isTrigger=true;}
 void Update(){if(player==null||!player.IsAlive){body.linearVelocity=Vector2.zero;Anim(0,4,8);return;}Vector2 d=player.transform.position-transform.position;visual.flipX=d.x<0;if(acting)return;if(d.magnitude>detectionRadius){body.linearVelocity=Vector2.zero;Anim(0,4,8);}else if(d.magnitude<retreatDistance){body.linearVelocity=-d.normalized*retreatSpeed;Anim(4,4,8);}else{body.linearVelocity=Vector2.zero;Anim(0,4,8);if(Time.time>=nextAttack)cast=StartCoroutine(Cast());}}
 IEnumerator Cast(){Vector2 target=player.transform.position;if(!EnemyCombatCoordinator.TryReserve(this,target,impactRadius+.25f,telegraphSeconds+1f)){nextAttack=Time.time+.3f;yield break;}acting=true;body.linearVelocity=Vector2.zero;activeEffect=new GameObject("Frostbinder Sigil");var sr=activeEffect.AddComponent<SpriteRenderer>();sr.sortingOrder=32;activeEffect.transform.position=target;yield return Play(8,3,.25f);float e=0;while(e<telegraphSeconds){e+=Time.deltaTime;if(effectFrames.Length>0)sr.sprite=effectFrames[Mathf.Min(2,Mathf.FloorToInt(e*8)%3)];if(frames.Length>=15)visual.sprite=frames[11+Mathf.FloorToInt(e*8)%4];yield return null;}yield return Play(15,3,.2f);for(int i=3;i<effectFrames.Length;i++){sr.sprite=effectFrames[i];yield return new WaitForSeconds(.07f);}if(player!=null&&player.IsAlive&&Vector2.Distance(player.transform.position,target)<=impactRadius){player.ReceiveDamage(damage);player.ApplySlow(slowMultiplier,slowSeconds);}EnemyCombatCoordinator.Release(this);yield return Play(18,3,recoverySeconds);Destroy(activeEffect);activeEffect=null;nextAttack=Time.time+cooldownSeconds;acting=false;cast=null;}
 IEnumerator Play(int s,int c,float d){float e=0;while(e<d){e+=Time.deltaTime;if(frames.Length>=s+c)visual.sprite=frames[s+Mathf.Min(c-1,Mathf.FloorToInt(e/d*c))];yield return null;}}
 void Anim(int s,int c,float fps){if(frames==null||frames.Length<s+c)return;animTime+=Time.deltaTime;visual.sprite=frames[s+Mathf.FloorToInt(animTime*fps)%c];}
 public void TakeHit(){hits--;if(acting&&cast!=null){StopCoroutine(cast);EnemyCombatCoordinator.Release(this);acting=false;cast=null;body.linearVelocity=Vector2.zero;if(activeEffect!=null)Destroy(activeEffect);activeEffect=null;nextAttack=Time.time+1f;}if(hits<=0)EnemyDefeat.PlayDeath(this,visual,body,frames,25,4);}
 public int CurrentHits=>hits;
 public int MaximumHits=>hitsToKill;
 public void RestoreOneHit(){if(enabled&&hits>0)hits=Mathf.Min(hitsToKill,hits+1);}
 void OnDestroy(){EnemyCombatCoordinator.Release(this);if(activeEffect!=null)Destroy(activeEffect);}
}
