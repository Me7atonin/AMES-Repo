using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private bool isPatrolling = true;

    [Header("Vision Settings")]
    public float sightRange = 10f;
    public float fieldOfView = 120f;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;
    public float memoryDuration = 5f;
    private float memoryTimer = 0f;

    private bool playerInSight;
    private Vector3 lastKnownPlayerPosition;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.speed = patrolSpeed;
    }

    private void Update()
    {
        DetectPlayer();

        if (playerInSight)
        {
            ChasePlayer();
        }
        else if (memoryTimer > 0)
        {
            SearchLastKnownPosition();
        }
        else if (isPatrolling)
        {
            Patrol();
        }
    }

    private void DetectPlayer()
    {
        playerInSight = false;

        Vector3 directionToPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (directionToPlayer.magnitude <= sightRange && angle <= fieldOfView * 0.5f)
        {
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, sightRange, whatIsPlayer | whatIsGround))
            {
                Debug.DrawRay(transform.position, directionToPlayer * sightRange, Color.red);
                if (hit.transform.CompareTag("Player"))
                {
                    playerInSight = true;
                    lastKnownPlayerPosition = player.position;
                    memoryTimer = memoryDuration;
                }
            }
        }
        else if (memoryTimer > 0)
        {
            memoryTimer -= Time.deltaTime;
        }
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }

    private void ChasePlayer()
    {
        isPatrolling = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void SearchLastKnownPosition()
    {
        agent.speed = patrolSpeed;
        agent.SetDestination(lastKnownPlayerPosition);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isPatrolling = true;
        }
    }

    // Optional: Extend this for unique enemies
    public virtual void OnPlayerCaught()
    {
        Debug.Log("Player caught! Override this in child classes.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
