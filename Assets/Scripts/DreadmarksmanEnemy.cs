using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D),typeof(CircleCollider2D))]
public sealed class DreadmarksmanEnemy:MonoBehaviour,IEnemySupportTarget
{
 [SerializeField] float detectionRadius=8f,minDistance=3.2f,preferredDistance=5f,moveSpeed=1.25f,aimSeconds=.85f,cooldownSeconds=3.4f,shotRadius=.45f;
 [SerializeField] int damage=36,hitsToKill=3;[SerializeField] Sprite[] frames,effectFrames;
 TopDownPlayer player;Rigidbody2D body;SpriteRenderer visual;EnemyMovement movement;float nextAttack,animTime;bool acting;int hits;Coroutine shot;GameObject activeEffect;
 void Awake(){player=FindFirstObjectByType<TopDownPlayer>();body=GetComponent<Rigidbody2D>();visual=GetComponent<SpriteRenderer>();movement=GetComponent<EnemyMovement>() ?? gameObject.AddComponent<EnemyMovement>();hits=hitsToKill;body.gravityScale=0;body.freezeRotation=true;GetComponent<CircleCollider2D>().isTrigger=true;}
 void Update()
 {
  if(player==null||!player.IsAlive){body.linearVelocity=Vector2.zero;Anim(0,4,8);return;}
  Vector2 d=player.transform.position-transform.position;
  float distance=d.magnitude;
  visual.flipX=d.x<0;
  if(acting)return;
  if(distance>detectionRadius){body.linearVelocity=Vector2.zero;Anim(0,4,8);return;}
  if(distance<minDistance){movement.Steer(-d.normalized*moveSpeed);Anim(8,3,10);return;}
  if(distance>preferredDistance+.7f){movement.Steer(d.normalized*moveSpeed);Anim(4,4,9);return;}
  if(!EnemyCombatCoordinator.HasClearShot(body.position,player.transform.position))
  {
   Vector2 tangent=new Vector2(-d.y,d.x).normalized;
   if((GetInstanceID()&1)==0)tangent=-tangent;
   movement.Steer(tangent*moveSpeed);
   Anim(4,4,9);
   return;
  }
  body.linearVelocity=Vector2.zero;
  Anim(0,4,8);
  if(Time.time>=nextAttack)shot=StartCoroutine(Shoot());
 }
 IEnumerator Shoot(){Vector2 target=player.transform.position;if(!EnemyCombatCoordinator.TryReserve(this,target,shotRadius+.2f,aimSeconds+.7f)){nextAttack=Time.time+.3f;yield break;}acting=true;body.linearVelocity=Vector2.zero;yield return Play(11,3,.22f);CreateLine(target);float e=0;while(e<aimSeconds){e+=Time.deltaTime;if(effectFrames.Length>=3)activeEffect.GetComponent<SpriteRenderer>().sprite=effectFrames[Mathf.FloorToInt(e*9)%3];if(frames.Length>=18)visual.sprite=frames[14+Mathf.FloorToInt(e*8)%4];yield return null;}yield return Play(18,3,.15f);var sr=activeEffect.GetComponent<SpriteRenderer>();for(int i=3;i<effectFrames.Length;i++){sr.sprite=effectFrames[i];yield return new WaitForSeconds(.065f);}if(player!=null&&player.IsAlive&&Vector2.Distance(player.transform.position,target)<=shotRadius)player.ReceiveDamage(damage);EnemyCombatCoordinator.Release(this);yield return Play(21,3,.18f);yield return Play(24,4,.42f);yield return Play(28,3,.3f);Destroy(activeEffect);activeEffect=null;nextAttack=Time.time+cooldownSeconds;acting=false;shot=null;}
 void CreateLine(Vector2 target){activeEffect=new GameObject("Dreadmarksman Aim");var sr=activeEffect.AddComponent<SpriteRenderer>();sr.sortingOrder=33;if(effectFrames.Length>0)sr.sprite=effectFrames[0];Vector2 start=transform.position;Vector2 delta=target-start;activeEffect.transform.position=start+delta*.5f;activeEffect.transform.right=delta.normalized;activeEffect.transform.localScale=new Vector3(Mathf.Max(1f,delta.magnitude/2f),1f,1f);}
 IEnumerator Play(int s,int c,float d){float e=0;while(e<d){e+=Time.deltaTime;if(frames.Length>=s+c)visual.sprite=frames[s+Mathf.Min(c-1,Mathf.FloorToInt(e/d*c))];yield return null;}}
 void Anim(int s,int c,float fps){if(frames==null||frames.Length<s+c)return;animTime+=Time.deltaTime;visual.sprite=frames[s+Mathf.FloorToInt(animTime*fps)%c];}
 public void TakeHit(){hits--;if(acting&&shot!=null){StopCoroutine(shot);EnemyCombatCoordinator.Release(this);acting=false;shot=null;body.linearVelocity=Vector2.zero;if(activeEffect!=null)Destroy(activeEffect);activeEffect=null;nextAttack=Time.time+1f;}if(hits<=0)EnemyDefeat.PlayDeath(this,visual,body,frames,35,4);}
 public int CurrentHits=>hits;
 public int MaximumHits=>hitsToKill;
 public void RestoreOneHit(){if(enabled&&hits>0)hits=Mathf.Min(hitsToKill,hits+1);}
 void OnDestroy(){EnemyCombatCoordinator.Release(this);if(activeEffect!=null)Destroy(activeEffect);}
}
