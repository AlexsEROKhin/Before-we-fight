using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class HookwardenEnemy : MonoBehaviour, IEnemySupportTarget
{
    [SerializeField] private float detectionRadius = 7f, preferredDistance = 4.2f, moveSpeed = 0.85f;
    [SerializeField] private float telegraphSeconds = 0.65f, hookRange = 5.5f, hookSpeed = 8f, cooldownSeconds = 3.5f, missRecoverySeconds = 1.2f;
    [SerializeField] private int hookDamage = 30, heavyDamage = 40, hitsToKill = 5;
    [SerializeField] private Sprite[] frames, hookFrames;
    private TopDownPlayer player; private Rigidbody2D body; private SpriteRenderer visual; private EnemyMovement movement;
    private float nextAttackAt, animationTime; private bool acting; private int hits;

    private void Awake()
    {
        player = FindFirstObjectByType<TopDownPlayer>(); body = GetComponent<Rigidbody2D>(); visual = GetComponent<SpriteRenderer>(); movement = GetComponent<EnemyMovement>() ?? gameObject.AddComponent<EnemyMovement>(); hits = hitsToKill;
        body.gravityScale = 0f; body.freezeRotation = true; GetComponent<CircleCollider2D>().isTrigger = true;
    }
    private void Update()
    {
        if (player == null || !player.IsAlive) { body.linearVelocity = Vector2.zero; Animate(0,4,8f); return; }
        Vector2 delta = player.transform.position-transform.position; float distance=delta.magnitude; visual.flipX=delta.x<0f;
        if (acting) { body.linearVelocity=Vector2.zero; return; }
        if (distance>detectionRadius) { body.linearVelocity=Vector2.zero; Animate(0,4,8f); return; }
        if (distance>preferredDistance) { movement.Steer(delta.normalized*moveSpeed); Animate(4,4,8f); }
        else
        {
            bool clear = EnemyCombatCoordinator.HasClearShot(body.position, player.transform.position);
            if (!clear)
            {
                Vector2 tangent = new Vector2(-delta.y, delta.x).normalized;
                if ((GetInstanceID() & 1) == 0) tangent = -tangent;
                movement.Steer(tangent * moveSpeed);
                Animate(4,4,8f);
            }
            else
            {
                body.linearVelocity=Vector2.zero;
                Animate(0,4,8f);
                if(Time.time>=nextAttackAt&&distance<=hookRange)
                    StartCoroutine(HookAttack(delta.normalized));
            }
        }
    }
    private IEnumerator HookAttack(Vector2 direction)
    {
        EnemyTelegraph warning=EnemyTelegraph.TryBegin(this,player.transform.position,.5f,telegraphSeconds+.2f,new Color(1f,.4f,.2f,.8f));
        if(warning==null){nextAttackAt=Time.time+.25f;yield break;}
        acting=true; body.linearVelocity=Vector2.zero; yield return PlayRange(8,3,telegraphSeconds);
        warning.Finish();
        Vector2 start=body.position,end=start+direction*hookRange; GameObject obj=new GameObject("Hookwarden Hook");
        SpriteRenderer hook=obj.AddComponent<SpriteRenderer>(); hook.sprite=hookFrames.Length>0?hookFrames[0]:null; hook.sortingOrder=30;
        obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
        bool hit=false; float travel=hookRange/hookSpeed,elapsed=0f;
        while(elapsed<travel){elapsed+=Time.deltaTime;obj.transform.position=Vector2.Lerp(start,end,elapsed/travel);if(Vector2.Distance(obj.transform.position,player.transform.position)<0.38f){hit=true;break;}yield return null;}
        if(hit){if(hookFrames.Length>2)hook.sprite=hookFrames[2];player.ReceiveDamage(hookDamage);player.PullToward(body.position+direction*0.8f,0.35f);yield return PlayRange(16,3,0.35f);yield return PlayRange(19,4,0.38f);if(Vector2.Distance(player.transform.position,body.position)<1.25f)player.ReceiveDamage(heavyDamage);}
        else{elapsed=0f;while(elapsed<travel){elapsed+=Time.deltaTime;obj.transform.position=Vector2.Lerp(end,start,elapsed/travel);yield return null;}yield return PlayRange(23,3,missRecoverySeconds);}
        Destroy(obj);nextAttackAt=Time.time+cooldownSeconds;acting=false;
    }
    private IEnumerator PlayRange(int start,int count,float duration){float e=0f;while(e<duration){e+=Time.deltaTime;if(frames.Length>=start+count)visual.sprite=frames[start+Mathf.Min(count-1,Mathf.FloorToInt(e/duration*count))];yield return null;}}
    private void Animate(int start,int count,float fps){if(frames==null||frames.Length<start+count)return;animationTime+=Time.deltaTime;visual.sprite=frames[start+Mathf.FloorToInt(animationTime*fps)%count];}
    public void TakeHit(){hits--;if(hits<=0)EnemyDefeat.PlayDeath(this,visual,body,frames,30,4);}
    public int CurrentHits => hits;
    public int MaximumHits => hitsToKill;
    public void RestoreOneHit(){if(enabled&&hits>0)hits=Mathf.Min(hitsToKill,hits+1);}
}
