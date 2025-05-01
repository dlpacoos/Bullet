using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Player : Agent
{
    private Rigidbody2D rBody;
    private PlayerHealth playerHealth;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private const float InputThreshold = 0.1f;

    [Header("Rewards")]
    public float hitPenalty = -10.0f;
    public float dodgeReward = 5.0f;
    public float timeRewardPerSecond = 1.0f;
    public float bigBulletRewardMultiplier = 5.0f;

    public float survivalMultiplier = 5.0f;


    [Header("Bullet Dodging")]
    public string bulletTag = "Bullet";
    public string bigBulletTag = "BigBullet";
    public float dodgeRadius = 2.0f;

    [Header("Observations")]
    public float observationRadius = 5.0f;
    public int maxBulletsToObserve = 5;
    public LayerMask bulletLayerMask;

    [Header("Normalization")]
    public float mapHalfWidth = 4f;
    public float mapHalfHeight = 2f;
    public float maxBulletSpeed = 5f;

    [Header("Score Management")]
    public ScoreManager scoreManager;

    private Key keyScript;
    private TilemapCollider2D topTilemapCollider;
    private HashSet<GameObject> bulletsPreviouslyNear = new HashSet<GameObject>();

    private float episodeTime = 0f;

    void Awake()
    {
        rBody = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();

        if (rBody == null) Debug.LogError("Rigidbody2D component not found!", this);
        if (playerHealth == null) Debug.LogError("PlayerHealth component not found!", this);

        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager == null)
            Debug.LogWarning("ScoreManager not found in scene! Timer will not reset/stop.", this);

        keyScript = FindObjectOfType<Key>();
        if (keyScript == null)
            Debug.LogError("Key script not found in scene!", this);

        GameObject top = GameObject.Find("top");
        if (top != null)
            topTilemapCollider = top.GetComponent<TilemapCollider2D>();
        if (topTilemapCollider == null)
            Debug.LogError("TilemapCollider2D not found on 'top' tilemap!", this);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position & velocity (normalized)
        Vector2 pos = transform.localPosition;
        sensor.AddObservation(pos.x / mapHalfWidth);
        sensor.AddObservation(pos.y / mapHalfHeight);
        sensor.AddObservation(rBody.linearVelocity.x / moveSpeed);
        sensor.AddObservation(rBody.linearVelocity.y / moveSpeed);

        // Bullet observations
        var hits = Physics2D.OverlapCircleAll(transform.position, observationRadius, bulletLayerMask)
            .Where(c => c.CompareTag(bulletTag) || c.CompareTag(bigBulletTag))
            .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
            .Take(maxBulletsToObserve)
            .ToList();

        foreach (var c in hits)
        {
            Vector2 rel = (Vector2)c.transform.position - (Vector2)transform.position;
            sensor.AddObservation(rel.x / observationRadius);
            sensor.AddObservation(rel.y / observationRadius);

            var rbB = c.GetComponent<Rigidbody2D>();
            Vector2 vel = rbB != null ? rbB.linearVelocity : Vector2.zero;
            sensor.AddObservation(vel.x / maxBulletSpeed);
            sensor.AddObservation(vel.y / maxBulletSpeed);

            sensor.AddObservation(c.CompareTag(bigBulletTag) ? 1f : 0f);
        }
        // Pad if fewer bullets seen
        int pad = maxBulletsToObserve - hits.Count;
        for (int i = 0; i < pad; i++)
        {
            sensor.AddObservation(0f); sensor.AddObservation(0f); // position
            sensor.AddObservation(0f); sensor.AddObservation(0f); // velocity
            sensor.AddObservation(0f);                           // type
        }

        // Distance to nearest bullet
        float minDist = hits.Count > 0
            ? hits.Min(c => Vector2.Distance(transform.position, c.transform.position))
            : observationRadius;
        sensor.AddObservation(minDist / observationRadius);

        // Key observations
        if (keyScript != null && keyScript.gameObject.activeSelf)
        {
            Vector2 kRel = (Vector2)keyScript.transform.position - (Vector2)transform.position;
            sensor.AddObservation(kRel.x / mapHalfWidth);
            sensor.AddObservation(kRel.y / mapHalfHeight);
            sensor.AddObservation(keyScript.isCollectible ? 1f : 0f);
        }
        else
        {
            sensor.AddObservation(0f); sensor.AddObservation(0f); sensor.AddObservation(0f);
        }
    }

    
    public override void OnEpisodeBegin()
    {
        // Reset timer

        episodeTime = 0f;
        scoreManager?.ResetScore();

        // Reset physics & position
        rBody.linearVelocity = Vector2.zero;
        rBody.angularVelocity = 0f;
        transform.localPosition = new Vector2(0, 0);
        // Reset health
        playerHealth?.ResetHealth();

        // Clear bullets
        bulletsPreviouslyNear.Clear();
        foreach (var b in FindObjectsOfType<Bullet>()) Destroy(b.gameObject);

        // Reset key
        keyScript?.ResetAndStartTimer();
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // If dead, don’t move or accumulate time
        if (playerHealth != null && playerHealth.currentHealth <= 0)
        {
            rBody.linearVelocity = Vector2.zero;
            return;
        }

        // Movement (unchanged)
        Vector2 signal = actionBuffers.ContinuousActions.Length >= 2
            ? new Vector2(actionBuffers.ContinuousActions[0],
                          actionBuffers.ContinuousActions[1])
            : Vector2.zero;
        rBody.linearVelocity = signal.magnitude > InputThreshold
            ? signal.normalized * moveSpeed
            : Vector2.zero;

        // 1) Accumulate simulation time
        float dt = Time.fixedDeltaTime;
        episodeTime += dt;

        // 2) Small per-step reward to encourage staying alive
        AddReward(timeRewardPerSecond * dt);
    }


    void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.currentHealth <= 0)
        {
            rBody.linearVelocity = Vector2.zero;
            return;
        }

       

        CheckForDodgedBullets();

        // Key pickup
        if (keyScript != null && keyScript.isCollectible && keyScript.gameObject.activeSelf)
        {
            var kc = keyScript.GetComponent<Collider2D>();
            if (kc != null && kc.bounds.Contains(rBody.position))
            {
                AddReward(keyScript.pickupReward);
                keyScript.HideKey();
            }
        }
    }

    public void NotifyDamageTaken(bool isBigBullet)
    {
        // 1) Hit penalty (small or big)
        float penalty = isBigBullet
            ? hitPenalty * bigBulletRewardMultiplier
            : hitPenalty;
        AddReward(penalty);

        // 2) If this last hit killed the agent
        if (playerHealth != null && playerHealth.currentHealth <= 0)
        {
            scoreManager?.StopScoring();

            // 3) Final survival bonus:
            float bonus = episodeTime * survivalMultiplier;
            AddReward(bonus);
            Debug.Log($"[EPISODE END] Survived {episodeTime:F2}s → bonus {bonus:F2} pts; total R={GetCumulativeReward():F2}");

            EndEpisode();
        }
    }


    private void CheckForDodgedBullets()
    {
        var playerCol = GetComponent<Collider2D>();
        if (playerCol == null) return;

        var center = playerCol.bounds.center;
        var nearby = Physics2D.OverlapCircleAll(center, dodgeRadius)
            .Where(c => c.CompareTag(bulletTag) || c.CompareTag(bigBulletTag))
            .Select(c => c.gameObject)
            .ToHashSet();

        foreach (var prev in bulletsPreviouslyNear.ToList())
        {
            if (prev == null) { bulletsPreviouslyNear.Remove(prev); continue; }
            if (!nearby.Contains(prev))
            {
                float rwd = prev.CompareTag(bigBulletTag)
                    ? dodgeReward * bigBulletRewardMultiplier
                    : dodgeReward;
                AddReward(rwd);
                Debug.Log($"Dodged a {(prev.CompareTag(bigBulletTag) ? "BigBullet" : "Bullet")} — reward: {rwd}");
                bulletsPreviouslyNear.Remove(prev);
            }
        }

        foreach (var cur in nearby)
            bulletsPreviouslyNear.Add(cur);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var outArr = actionsOut.ContinuousActions;
        outArr[0] = Input.GetAxisRaw("Horizontal");
        outArr[1] = Input.GetAxisRaw("Vertical");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, dodgeRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, observationRadius);
    }
}
