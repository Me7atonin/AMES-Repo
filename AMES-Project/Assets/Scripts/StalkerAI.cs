using UnityEngine;
using UnityEngine.AI;

public class StalkerAI : MonoBehaviour
{
    [Header("AI Settings")]
    public Transform player;  // The player character
    public float detectionRange = 10f; // How far it can detect the player
    public float fieldOfViewAngle = 120f; // Angle of detection
    public float memoryDuration = 5f; // Time it remembers the last seen player position
    public float speed = 3.5f; // Speed at which the stalker moves
    public float patrolSpeed = 2f; // Speed during patrol mode
    public float chaseSpeed = 5f; // Speed when chasing the player
    private Vector3 lastSeenPosition; // Last position of the player
    private float memoryTimer;

    [Header("Detection Settings")]
    public LayerMask obstacleLayer; // What the AI can "see" through (walls, etc.)

    private NavMeshAgent agent;
    private bool playerInSight = false;
    private bool isChasing = false;
    private bool playerHeard = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed; // Set initial speed to patrol speed
    }

    void Update()
    {
        playerInSight = CanSeePlayer();

        // If player is in sight, start chasing or stalking
        if (playerInSight && !isChasing)
        {
            StartChase();
        }
        // If the player is not in sight but was previously seen, use memory to chase
        else if (!playerInSight && memoryTimer > 0)
        {
            memoryTimer -= Time.deltaTime;
            agent.SetDestination(lastSeenPosition);
        }
        // If memory has expired, stop the chase and return to patrol
        else if (!playerInSight && memoryTimer <= 0)
        {
            Patrol();
        }

        // If we are chasing the player, keep pursuing
        if (isChasing)
        {
            agent.SetDestination(player.position);
        }

        // Look for player when idle or patrolling
        if (!isChasing)
        {
            Patrol();
        }

        // If the player makes noise (e.g., moving quickly or triggering traps), detect them
        if (playerHeard)
        {
            HearPlayer();
        }
    }

    // Can the stalker see the player?
    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = player.position - transform.position;
        float distance = dirToPlayer.magnitude;

        if (distance < detectionRange)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle < fieldOfViewAngle * 0.5f)
            {
                RaycastHit hit;
                if (!Physics.Raycast(transform.position, dirToPlayer.normalized, out hit, distance, obstacleLayer))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Start chasing the player when detected
    void StartChase()
    {
        isChasing = true;
        agent.speed = chaseSpeed;
        lastSeenPosition = player.position;
        memoryTimer = memoryDuration;
    }

    // When the stalker loses the player, return to a patrol state
    void Patrol()
    {
        // Random patrol point (you can customize patrol logic)
        Vector3 randomPatrolTarget = new Vector3(
            transform.position.x + Random.Range(-10f, 10f),
            transform.position.y,
            transform.position.z + Random.Range(-10f, 10f)
        );
        agent.SetDestination(randomPatrolTarget);
        agent.speed = patrolSpeed;
    }

    // Handle the stalker hearing the player (triggered by noise or proximity)
    void HearPlayer()
    {
        // Logic for handling hearing the player (could be triggered by a noise system or proximity)
        // For example, if the player is running too loud, the stalker hears them
        agent.SetDestination(player.position);
    }

    // Call this when the player triggers some noise
    public void PlayerHeard()
    {
        playerHeard = true;
    }

    // If the stalker catches the player, you could trigger some event here
    void CatchPlayer()
    {
        // Code for when the player is caught (game over, jump scare, etc.)
        Debug.Log("Player caught!");
    }
}
