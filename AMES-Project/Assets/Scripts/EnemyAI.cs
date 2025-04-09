using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform[] patrolPoints;
    public Transform player;
    private PlayerMovement playerMovement;
    private NavMeshAgent agent;

    [Header("Vision Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 180f;
    public LayerMask obstructionMask;

    [Header("Detection Settings")]
    public float memoryTime = 5f;
    private float memoryTimer;
    private Vector3 lastKnownPosition;
    private bool playerInSight;

    [Header("Adaptive Behavior Settings")]
    public int losesBeforeBehaviorChange = 3;
    private int timesLostPlayerHere = 0;

    [Header("Search Settings")]
    public float searchDuration = 3f;
    private float searchTimer;
    private bool searching;

    private int currentPatrolIndex;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerMovement = player.GetComponent<PlayerMovement>();
        GoToNextPatrolPoint();
    }

    private void Update()
    {
        if (CanSeePlayer())
        {
            playerInSight = true;
            lastKnownPosition = player.position;
            memoryTimer = memoryTime;

            ChasePlayer();
        }
        else if (playerInSight)
        {
            memoryTimer -= Time.deltaTime;

            if (memoryTimer > 0)
            {
                ChasePlayer();
            }
            else
            {
                playerInSight = false;
                StartSearching();
            }
        }
        else if (searching)
        {
            SearchForPlayer();
        }
        else
        {
            Patrol();
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < viewDistance)
        {
            float angleBetween = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleBetween < viewAngle / 2f)
            {
                // Raycast from enemy eye level
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstructionMask))
                {
                    // Modify detection based on player state
                    if (playerMovement.IsCrouching)
                        return distanceToPlayer < viewDistance * 0.5f;

                    if (playerMovement.IsSprinting)
                        return distanceToPlayer < viewDistance * 1.2f;

                    return true;
                }
            }
        }
        return false;
    }

    private void ChasePlayer()
    {
        searching = false;
        agent.SetDestination(player.position);
    }

    private void StartSearching()
    {
        // Check if the AI lost the player in the same spot
        if (Vector3.Distance(transform.position, lastKnownPosition) < 1f)
        {
            timesLostPlayerHere++;

            if (timesLostPlayerHere >= losesBeforeBehaviorChange)
            {
                DoUnpredictableBehavior();
                timesLostPlayerHere = 0; // Reset counter after behavior change
                return; // Exit early so it doesn't do regular searching
            }
        }
        else
        {
            timesLostPlayerHere = 0; // Reset if different location
        }

        searching = true;
        searchTimer = searchDuration;
        agent.SetDestination(lastKnownPosition);
    }

    private void SearchForPlayer()
    {
        if (Vector3.Distance(transform.position, lastKnownPosition) < 1f)
        {
            searchTimer -= Time.deltaTime;

            if (searchTimer <= 0)
            {
                searching = false;
                GoToNextPatrolPoint();
            }
        }
    }

    private void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
    private void DoUnpredictableBehavior()
    {
        searching = false;

        // 50/50 chance between two behaviors
        if (Random.value > 0.5f)
        {
            // Search a random patrol point (surprise!)
            int randomIndex = Random.Range(0, patrolPoints.Length);
            agent.SetDestination(patrolPoints[randomIndex].position);
        }
        else
        {
            // Search in a random position near last known
            Vector3 randomOffset = new Vector3(
                Random.Range(-5f, 5f),
                0,
                Random.Range(-5f, 5f)
            );

            agent.SetDestination(lastKnownPosition + randomOffset);
        }
    }
}
