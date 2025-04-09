using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStealthChase : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player’s transform. Make sure the player GameObject is tagged as 'Player'.")]
    public Transform player;  // Assign your player here in the Inspector
    private NavMeshAgent agent;

    [Header("Vision Settings")]
    [Tooltip("Field of view angle (in degrees).")]
    public float viewAngle = 90f;
    [Tooltip("Maximum distance the enemy can see.")]
    public float viewDistance = 10f;
    [Tooltip("The layers that block the enemy's vision (e.g., walls, obstacles).")]
    public LayerMask obstructionMask;
    [Tooltip("Enemy's eye height (from their position). This is where the raycast originates.")]
    public float eyeHeight = 1.5f;

    [Header("Patrol Settings (Optional)")]
    [Tooltip("Assign patrol points if you want the enemy to move when not chasing.")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;

    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Check for player detection each frame
        if (PlayerInView())
        {
            if (HasLineOfSight())
            {
                ChasePlayer();
            }
            else
            {
                LosePlayer();
            }
        }
        else
        {
            LosePlayer();
        }
    }

    // Determines if the player is within the enemy's field of view and detection range.
    bool PlayerInView()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle < viewAngle / 2f && Vector3.Distance(transform.position, player.position) < viewDistance;
    }

    // Uses raycasting to check if there is a clear line-of-sight to the player.
    bool HasLineOfSight()
    {
        // The ray starts at the enemy's "eye" level.
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;
        Vector3 directionToPlayer = (player.position - eyePos).normalized;

        RaycastHit hit;
        // Perform the raycast using the defined obstructionMask.
        if (Physics.Raycast(eyePos, directionToPlayer, out hit, viewDistance, obstructionMask))
        {
            // If the ray hits the player, then the enemy has a clear line of sight.
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    // Sets the enemy to chase the player.
    void ChasePlayer()
    {
        isChasing = true;
        agent.SetDestination(player.position);
    }

    // Called when the enemy loses sight of the player.
    // For now, the enemy immediately gives up chasing and switches to patrolling.
    void LosePlayer()
    {
        if (isChasing)
        {
            isChasing = false;
            Patrol();
        }
        else
        {
            // Continue patrolling if not chasing.
            Patrol();
        }
    }

    // Basic patrol behavior: the enemy moves between predefined patrol points.
    void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
}

