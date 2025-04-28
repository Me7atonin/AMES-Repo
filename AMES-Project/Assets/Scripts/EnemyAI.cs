using UnityEngine;
using UnityEngine.AI;

public class AggressiveEnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;

    [Header("Collision Settings")]
    public LayerMask wallMask;

    [Header("Stats")]
    public float detectionRange = 25f;
    public float fieldOfView = 140f;
    public float chaseSpeed = 6f;
    public float memoryDuration = 3f;
    public LayerMask obstacleMask;

    [Header("Player Above Settings")]
    public float elevationThreshold = 2f; // How far above the enemy the player has to be
    public float circleRadius = 2f;
    public float circleSpeed = 3f;

    [Header("Wandering Settings")]
    public float wanderRadius = 10f; // Radius within which it can wander
    public float wanderCooldown = 5f; // Time before it picks a new random target
    private float wanderTimer;
    private Vector3 wanderTarget;
    private float wanderTimerThreshold = 3f; // Time threshold to stop wandering when stuck

    private float memoryTimer;
    private bool playerInSight;
    private Vector3 lastKnownPlayerPosition;
    private float circleAngle;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority = 50; // Avoid collisions with other agents
        agent.radius = 1.5f; // Adjust to match the size of the enemy
        agent.height = 2.0f; // Adjust based on the enemy's height
        agent.stoppingDistance = 0.2f; // Ensure agent doesn't stop too far from the target
        agent.angularSpeed = 500f; // Allows smoother turning
        agent.acceleration = 8f; // Allows faster acceleration

        wanderTimer = wanderCooldown; // Initialize wander timer
        wanderTarget = transform.position; // Start from current position
    }

    void Update()
    {
        // Check if the player is directly above the enemy or near the enemy
        if (PlayerIsOnHead())
        {
            SpinAroundPlayer();
            return;
        }

        if (PlayerIsAbove())
        {
            // If the player is above but not directly on the head, move beneath them
            MoveBeneathPlayer();
            return;
        }

        playerInSight = CanSeePlayer();

        if (playerInSight)
        {
            lastKnownPlayerPosition = player.position;
            memoryTimer = memoryDuration;
            Chase(player.position);
        }
        else if (memoryTimer > 0)
        {
            memoryTimer -= Time.deltaTime;
            Chase(lastKnownPlayerPosition);
        }
        else
        {
            // Handle wandering behavior with a smoother approach
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0)
            {
                WanderRandomly(); // Wander randomly within the specified radius
                wanderTimer = wanderCooldown; // Reset wander timer
            }
            else
            {
                // If the enemy is stuck for too long, force it to wander in a new direction
                if (wanderTimer <= wanderTimerThreshold)
                {
                    WanderRandomly();
                }
            }
        }
    }
    void LateUpdate()
    {
        if (Physics.CheckSphere(transform.position + transform.forward * 0.5f, 0.4f, wallMask))
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 dirToPlayer = player.position - transform.position;
        float distToPlayer = dirToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (distToPlayer <= detectionRange && angle <= fieldOfView * 0.5f)
        {
            if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer.normalized, distToPlayer, obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    void Chase(Vector3 destination)
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(destination);
    }

    void WanderRandomly()
    {
        agent.speed = chaseSpeed * 0.5f; // Slow down while wandering

        // If the wander target is too close to the current position, pick a new target
        if (Vector3.Distance(transform.position, wanderTarget) < 2f)
        {
            // Pick a random position within the wander radius
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas);

            wanderTarget = hit.position; // Set new wander target
        }

        agent.SetDestination(wanderTarget);
    }

    bool PlayerIsOnHead()
    {
        float verticalOffset = player.position.y - transform.position.y;
        float horizontalDistance = Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), new Vector3(transform.position.x, 0, transform.position.z));
        return verticalOffset > elevationThreshold && horizontalDistance < 1.5f;
    }

    bool PlayerIsAbove()
    {
        float verticalOffset = player.position.y - transform.position.y;
        float horizontalDistance = Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), new Vector3(transform.position.x, 0, transform.position.z));
        return verticalOffset > elevationThreshold && horizontalDistance < 3f; // If player is close but not directly above
    }

    void SpinAroundPlayer()
    {
        agent.speed = circleSpeed;

        // Smooth orbit behavior around the player
        circleAngle += Time.deltaTime * circleSpeed;
        Vector3 offset = new Vector3(Mathf.Cos(circleAngle), 0, Mathf.Sin(circleAngle)) * circleRadius;
        Vector3 circleTarget = new Vector3(player.position.x, transform.position.y, player.position.z) + offset;

        agent.SetDestination(circleTarget);
    }

    void MoveBeneathPlayer()
    {
        // Try to move beneath the player if they are above the enemy but not directly on its head
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, player.position.z);
        agent.SetDestination(targetPosition);
        agent.speed = chaseSpeed; // Move at full speed to get beneath the player
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
