using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D),typeof(CircleCollider2D))]
public sealed class StonecleaverEnemy:MonoBehaviour,IEnemySupportTarget
{
 [SerializeField] float detectionRadius=6.5f,moveSpeed=.65f,attackDistance=3f,telegraphSeconds=.8f,fissureSeconds=1.5f,fissureRange=5f,cooldownSeconds=3.8f,recoverySeconds=1f;
 [SerializeField] int damage=40,hitsToKill=6; [SerializeField] Sprite[] frames,fissureFrames;
 TopDownPlayer player;Rigidbody2D body;SpriteRenderer visual;float nextAttack,animTime;bool acting;int hits;EnemyTelegraph activeWarning;GameObject activeFissure;
 void Awake(){player=FindFirstObjectByType<TopDownPlayer>();body=GetComponent<Rigidbody2D>();visual=GetComponent<SpriteRenderer>();hits=hitsToKill;body.gravityScale=0;body.freezeRotation=true;GetComponent<CircleCollider2D>().isTrigger=true;}
 void Update(){if(player==null||!player.IsAlive){body.linearVelocity=Vector2.zero;Anim(0,4,7);return;}Vector2 d=player.transform.position-transform.position;visual.flipX=d.x<0;if(acting)return;if(d.magnitude>detectionRadius){body.linearVelocity=Vector2.zero;Anim(0,4,7);}else if(d.magnitude>attackDistance){body.linearVelocity=d.normalized*moveSpeed;Anim(4,4,6);}else{body.linearVelocity=Vector2.zero;Anim(0,4,7);if(Time.time>=nextAttack)StartCoroutine(Slam(d.normalized));}}
 IEnumerator Slam(Vector2 dir){Vector2 target=player.transform.position;activeWarning=EnemyTelegraph.TryBegin(this,target,.7f,telegraphSeconds+fissureSeconds+.6f,new Color(1f,.45f,.2f,.8f));if(activeWarning==null){nextAttack=Time.time+.3f;yield break;}acting=true;body.linearVelocity=Vector2.zero;yield return Play(8,4,telegraphSeconds);yield return Play(12,3,.3f);GameObject go=activeFissure=new GameObject("Stonecleaver Fissure");var sr=go.AddComponent<SpriteRenderer>();sr.sortingOrder=28;Vector2 start=body.position+dir*.7f,end=start+dir*fissureRange;go.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg);float e=0;bool dealt=false;while(e<fissureSeconds){e+=Time.deltaTime;go.transform.position=Vector2.Lerp(start,end,e/fissureSeconds);if(fissureFrames.Length>0)sr.sprite=fissureFrames[Mathf.Min(fissureFrames.Length-1,Mathf.FloorToInt(e/fissureSeconds*fissureFrames.Length))];if(!dealt&&player!=null&&player.IsAlive&&Vector2.Distance(player.transform.position,go.transform.position)<.65f){player.ReceiveDamage(damage);dealt=true;}yield return null;}if(activeWarning!=null)activeWarning.Finish();activeWarning=null;Destroy(go);activeFissure=null;yield return Play(15,4,recoverySeconds);nextAttack=Time.time+cooldownSeconds;acting=false;}
 IEnumerator Play(int s,int c,float d){float e=0;while(e<d){e+=Time.deltaTime;if(frames.Length>=s+c)visual.sprite=frames[s+Mathf.Min(c-1,Mathf.FloorToInt(e/d*c))];yield return null;}}
 void Anim(int s,int c,float fps){if(frames==null||frames.Length<s+c)return;animTime+=Time.deltaTime;visual.sprite=frames[s+Mathf.FloorToInt(animTime*fps)%c];}
 public void TakeHit(){hits--;if(hits<=0)EnemyDefeat.PlayDeath(this,visual,body,frames,24,5,2);}
 public int CurrentHits=>hits;
 public int MaximumHits=>hitsToKill;
 public void RestoreOneHit(){if(enabled&&hits>0)hits=Mathf.Min(hitsToKill,hits+1);}
 void OnDestroy(){if(activeWarning!=null)activeWarning.Finish();if(activeFissure!=null)Destroy(activeFissure);}
}
