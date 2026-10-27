using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class WhirlraiderEnemy : MonoBehaviour, IEnemySupportTarget
{
    [SerializeField] private float detectionRadius=6f, moveSpeed=2.1f, attackDistance=1.35f;
    [SerializeField] private float telegraphSeconds=.45f, spinSeconds=1.8f, spinSpeed=3.1f, cooldownSeconds=3f, recoverySeconds=.9f;
    [SerializeField] private int damagePerHit=30, maxSpinHits=3, hitsToKill=4;
    [SerializeField] private Sprite[] frames;
    private TopDownPlayer player; private Rigidbody2D body; private SpriteRenderer visual; private EnemyMovement movement;
    private float nextAttackAt, animationTime, nextDamageAt; private bool acting; private int hits;
    private void Awake(){player=FindFirstObjectByType<TopDownPlayer>();body=GetComponent<Rigidbody2D>();visual=GetComponent<SpriteRenderer>();movement=GetComponent<EnemyMovement>() ?? gameObject.AddComponent<EnemyMovement>();hits=hitsToKill;body.gravityScale=0;body.freezeRotation=true;GetComponent<CircleCollider2D>().isTrigger=true;}
    private void Update(){if(player==null||!player.IsAlive){body.linearVelocity=Vector2.zero;Animate(0,4,8);return;}Vector2 d=player.transform.position-transform.position;visual.flipX=d.x<0;if(acting)return;if(d.magnitude>detectionRadius){body.linearVelocity=Vector2.zero;Animate(0,4,8);return;}if(d.magnitude>attackDistance){movement.Steer(d.normalized*moveSpeed);Animate(4,4,11);}else{body.linearVelocity=Vector2.zero;Animate(0,4,8);if(Time.time>=nextAttackAt)StartCoroutine(Spin(d.normalized));}}
    private IEnumerator Spin(Vector2 direction){EnemyTelegraph warning=EnemyTelegraph.TryBegin(this,body.position,.8f,telegraphSeconds+.25f,new Color(1f,.3f,.2f,.85f));if(warning==null){nextAttackAt=Time.time+.25f;yield break;}acting=true;body.linearVelocity=Vector2.zero;yield return Play(8,3,telegraphSeconds);warning.Finish();yield return Play(11,3,.2f);int dealt=0;float e=0;while(e<spinSeconds){e+=Time.deltaTime;if(player!=null&&player.IsAlive){Vector2 toward=((Vector2)player.transform.position-body.position).normalized;direction=Vector2.Lerp(direction,toward,Time.deltaTime*.5f).normalized;}movement.Steer(direction*spinSpeed);Animate(14,6,14);if(player!=null&&player.IsAlive&&dealt<maxSpinHits&&Time.time>=nextDamageAt&&Vector2.Distance(player.transform.position,transform.position)<.7f){player.ReceiveDamage(damagePerHit);dealt++;nextDamageAt=Time.time+.42f;}yield return null;}body.linearVelocity=Vector2.zero;yield return Play(20,3,.25f);yield return Play(23,4,recoverySeconds);nextAttackAt=Time.time+cooldownSeconds;acting=false;}
    private IEnumerator Play(int start,int count,float duration){float e=0;while(e<duration){e+=Time.deltaTime;if(frames.Length>=start+count)visual.sprite=frames[start+Mathf.Min(count-1,Mathf.FloorToInt(e/duration*count))];yield return null;}}
    private void Animate(int start,int count,float fps){if(frames==null||frames.Length<start+count)return;animationTime+=Time.deltaTime;visual.sprite=frames[start+Mathf.FloorToInt(animationTime*fps)%count];}
    public void TakeHit(){hits--;if(hits<=0)EnemyDefeat.PlayDeath(this,visual,body,frames,31,4);}
    public int CurrentHits => hits;
    public int MaximumHits => hitsToKill;
    public void RestoreOneHit(){if(enabled&&hits>0)hits=Mathf.Min(hitsToKill,hits+1);}
}
