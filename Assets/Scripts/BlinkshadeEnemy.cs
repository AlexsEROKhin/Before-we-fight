using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Rigidbody2D),typeof(CircleCollider2D))]
public sealed class BlinkshadeEnemy:MonoBehaviour,IEnemySupportTarget
{
 [SerializeField] float detectionRadius=6f,stalkDistance=3.2f,moveSpeed=1.6f,warningSeconds=.5f,cooldownSeconds=3.2f,recoverySeconds=.75f,retreatSeconds=.55f;
 [SerializeField] int damage=32,hitsToKill=3;[SerializeField] Sprite[] frames;
 TopDownPlayer player;Rigidbody2D body;SpriteRenderer visual;Collider2D hitbox;EnemyTelegraph activeWarning;float nextAttack,animTime;bool acting;int hits;
 void Awake(){player=FindFirstObjectByType<TopDownPlayer>();body=GetComponent<Rigidbody2D>();visual=GetComponent<SpriteRenderer>();hitbox=GetComponent<Collider2D>();hits=hitsToKill;body.gravityScale=0;body.freezeRotation=true;hitbox.isTrigger=true;}
 void Update(){if(player==null||!player.IsAlive){body.linearVelocity=Vector2.zero;Anim(0,4,8);return;}Vector2 d=player.transform.position-transform.position;visual.flipX=d.x<0;if(acting)return;if(d.magnitude>detectionRadius){body.linearVelocity=Vector2.zero;Anim(0,4,8);}else if(d.magnitude>stalkDistance){body.linearVelocity=d.normalized*moveSpeed;Anim(4,4,9);}else{body.linearVelocity=Vector2.zero;Anim(0,4,8);if(Time.time>=nextAttack)StartCoroutine(Blink());}}
 IEnumerator Blink(){Vector2 facing=((Vector2)player.transform.position-body.position).normalized;Vector2 landing=(Vector2)player.transform.position-facing*.72f;activeWarning=EnemyTelegraph.TryBegin(this,landing,1f,warningSeconds+.7f,new Color(1f,.25f,.45f,.8f));if(activeWarning==null){nextAttack=Time.time+.3f;yield break;}acting=true;body.linearVelocity=Vector2.zero;yield return Play(12,3,warningSeconds);yield return Play(15,3,.18f);visual.enabled=false;hitbox.enabled=false;body.position=landing;yield return new WaitForSeconds(.08f);visual.enabled=true;hitbox.enabled=true;yield return Play(18,3,.18f);yield return Play(21,4,.26f);if(player!=null&&player.IsAlive&&Vector2.Distance(player.transform.position,transform.position)<1f)player.ReceiveDamage(damage);activeWarning.Finish();activeWarning=null;yield return Play(28,3,recoverySeconds);Vector2 away=((Vector2)transform.position-(Vector2)player.transform.position).normalized;float e=0;while(e<retreatSeconds){e+=Time.deltaTime;body.linearVelocity=away*moveSpeed;Anim(25,3,10);yield return null;}body.linearVelocity=Vector2.zero;nextAttack=Time.time+cooldownSeconds;acting=false;}
 IEnumerator Play(int s,int c,float d){float e=0;while(e<d){e+=Time.deltaTime;if(frames.Length>=s+c)visual.sprite=frames[s+Mathf.Min(c-1,Mathf.FloorToInt(e/d*c))];yield return null;}}
 void Anim(int s,int c,float fps){if(frames==null||frames.Length<s+c)return;animTime+=Time.deltaTime;visual.sprite=frames[s+Mathf.FloorToInt(animTime*fps)%c];}
 public void TakeHit(){hits--;if(hits<=0)EnemyDefeat.PlayDeath(this,visual,body,frames,35,4);}
 public int CurrentHits=>hits;
 public int MaximumHits=>hitsToKill;
 public void RestoreOneHit(){if(enabled&&hits>0)hits=Mathf.Min(hitsToKill,hits+1);}
 void OnDisable(){if(activeWarning!=null)activeWarning.Finish();activeWarning=null;visual.enabled=true;hitbox.enabled=true;}
}
