using UnityEngine;

// Sort a whole character in front of or behind a tree using ground contact,
// just like the reward chest. This never changes sprite or collider sizes.
[DisallowMultipleComponent]
public sealed class TreeSideSorting : MonoBehaviour
{
    [SerializeField] private TopDownPlayer player;
    [SerializeField, Min(0f)] private float sideMargin = 0.03f;
    [SerializeField] private float frontFootMargin = 0.05f;
    [SerializeField] private int frontSortingOrder = 1;

    private SpriteRenderer playerVisual;
    private Collider2D playerCollider;
    private MageCreep[] mages;
    private SpriteRenderer[] mageVisuals;
    private Collider2D[] mageColliders;
    private RingCreep[] ringCreeps;
    private SpriteRenderer[] ringVisuals;
    private Collider2D[] ringColliders;
    private IoEnemy[] ios;
    private SpriteRenderer[] ioVisuals;
    private Collider2D[] ioColliders;
    private TrapperEnemy[] trappers;
    private SpriteRenderer[] trapperVisuals;
    private Collider2D[] trapperColliders;
    private SpriteRenderer[] treeVisuals;
    private BoxCollider2D[] trunks;

    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<TopDownPlayer>();
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider2D>();
            Transform visual = player.transform.Find("PlayerVisual");
            if (visual != null) playerVisual = visual.GetComponent<SpriteRenderer>();
        }

        mages = FindObjectsByType<MageCreep>(FindObjectsSortMode.None);
        mageVisuals = new SpriteRenderer[mages.Length];
        mageColliders = new Collider2D[mages.Length];
        for (int i = 0; i < mages.Length; i++)
        {
            mageVisuals[i] = mages[i].GetComponent<SpriteRenderer>();
            mageColliders[i] = mages[i].GetComponent<Collider2D>();
        }

        ringCreeps = FindObjectsByType<RingCreep>(FindObjectsSortMode.None);
        ringVisuals = new SpriteRenderer[ringCreeps.Length];
        ringColliders = new Collider2D[ringCreeps.Length];
        for (int i = 0; i < ringCreeps.Length; i++)
        {
            ringVisuals[i] = ringCreeps[i].GetComponent<SpriteRenderer>();
            ringColliders[i] = ringCreeps[i].GetComponent<Collider2D>();
        }

        ios = FindObjectsByType<IoEnemy>(FindObjectsSortMode.None);
        ioVisuals = new SpriteRenderer[ios.Length];
        ioColliders = new Collider2D[ios.Length];
        for (int i = 0; i < ios.Length; i++)
        {
            Transform ioVisual = ios[i].transform.Find("IoVisual (editable)");
            ioVisuals[i] = ioVisual != null ? ioVisual.GetComponent<SpriteRenderer>() : null;
            ioColliders[i] = ios[i].GetComponent<Collider2D>();
        }

        trappers = FindObjectsByType<TrapperEnemy>(FindObjectsSortMode.None);
        trapperVisuals = new SpriteRenderer[trappers.Length];
        trapperColliders = new Collider2D[trappers.Length];
        for (int i = 0; i < trappers.Length; i++)
        {
            trapperVisuals[i] = trappers[i].GetComponent<SpriteRenderer>();
            trapperColliders[i] = trappers[i].GetComponent<Collider2D>();
        }

        treeVisuals = GetComponentsInChildren<SpriteRenderer>(true);
        trunks = new BoxCollider2D[treeVisuals.Length];
        for (int i = 0; i < treeVisuals.Length; i++)
            trunks[i] = treeVisuals[i].GetComponent<BoxCollider2D>();
    }

    private void LateUpdate()
    {
        if (playerVisual != null)
            SortActor(player.transform, playerVisual, playerCollider);

        for (int i = 0; i < mages.Length; i++)
            if (mages[i] != null && mageVisuals[i] != null)
            {
                SortActor(mages[i].transform, mageVisuals[i], mageColliders[i]);
                SortAgainstPlayer(mages[i].transform, mageVisuals[i], mageColliders[i]);
            }

        for (int i = 0; i < ringCreeps.Length; i++)
            if (ringCreeps[i] != null && ringVisuals[i] != null)
            {
                SortActor(ringCreeps[i].transform, ringVisuals[i], ringColliders[i]);
                SortAgainstPlayer(ringCreeps[i].transform, ringVisuals[i], ringColliders[i]);
            }

        for (int i = 0; i < ios.Length; i++)
            if (ios[i] != null && ioVisuals[i] != null)
            {
                SortActor(ios[i].transform, ioVisuals[i], ioColliders[i]);
                SortAgainstPlayer(ios[i].transform, ioVisuals[i], ioColliders[i]);
            }

        for (int i = 0; i < trappers.Length; i++)
            if (trappers[i] != null && trapperVisuals[i] != null)
            {
                SortActor(trappers[i].transform, trapperVisuals[i], trapperColliders[i]);
                SortAgainstPlayer(trappers[i].transform, trapperVisuals[i], trapperColliders[i]);
            }
    }

    private void SortAgainstPlayer(Transform enemy, SpriteRenderer enemyVisual, Collider2D enemyCollider)
    {
        if (playerVisual == null || !playerVisual.enabled ||
            !enemyVisual.bounds.Intersects(playerVisual.bounds)) return;

        float playerFootY = playerCollider != null ? playerCollider.bounds.min.y : player.transform.position.y;
        float enemyFootY = enemyCollider != null ? enemyCollider.bounds.min.y : enemy.position.y;
        if (enemyFootY < playerFootY - 0.01f)
            enemyVisual.sortingOrder = Mathf.Max(enemyVisual.sortingOrder, playerVisual.sortingOrder + 1);
        else if (enemyFootY > playerFootY + 0.01f)
            enemyVisual.sortingOrder = Mathf.Min(enemyVisual.sortingOrder, playerVisual.sortingOrder - 1);
    }

    private void SortActor(Transform actor, SpriteRenderer visual, Collider2D actorCollider)
    {
        float footY = actorCollider != null ? actorCollider.bounds.min.y : actor.position.y;
        Bounds actorBounds = visual.bounds;
        float closestTreeDistance = float.PositiveInfinity;
        int targetOrder = 0;

        for (int i = 0; i < treeVisuals.Length; i++)
        {
            SpriteRenderer tree = treeVisuals[i];
            BoxCollider2D trunk = trunks[i];
            if (tree == null || trunk == null || !tree.enabled || !trunk.enabled) continue;
            if (!tree.bounds.Intersects(actorBounds)) continue;

            Bounds trunkBounds = trunk.bounds;
            // A large crown can overlap more than one tree. Let the nearest
            // trunk decide so another crown cannot flip the entire character.
            float distance = (actor.position - trunkBounds.center).sqrMagnitude;
            if (distance >= closestTreeDistance) continue;
            closestTreeDistance = distance;
            bool besideTrunk = actor.position.x < trunkBounds.min.x - sideMargin ||
                               actor.position.x > trunkBounds.max.x + sideMargin;
            // The tree's pivot is at its roots, while the small physics box
            // is higher up on the trunk. Use the roots for a centered actor;
            // keep the wider side allowance when passing beside the trunk.
            float treeFootY = besideTrunk ? trunkBounds.min.y : tree.transform.position.y;
            bool inFrontOfTree = footY <= treeFootY + frontFootMargin;
            targetOrder = inFrontOfTree ? frontSortingOrder : -frontSortingOrder;
        }

        if (visual.sortingOrder != targetOrder) visual.sortingOrder = targetOrder;
    }

    private void OnDisable()
    {
        if (playerVisual != null) playerVisual.sortingOrder = 0;
        if (mageVisuals == null) return;
        for (int i = 0; i < mageVisuals.Length; i++)
            if (mageVisuals[i] != null) mageVisuals[i].sortingOrder = 0;
        if (ringVisuals == null) return;
        for (int i = 0; i < ringVisuals.Length; i++)
            if (ringVisuals[i] != null) ringVisuals[i].sortingOrder = 0;
        if (ioVisuals == null) return;
        for (int i = 0; i < ioVisuals.Length; i++)
            if (ioVisuals[i] != null) ioVisuals[i].sortingOrder = 0;
        if (trapperVisuals == null) return;
        for (int i = 0; i < trapperVisuals.Length; i++)
            if (trapperVisuals[i] != null) trapperVisuals[i].sortingOrder = 0;
    }
}
