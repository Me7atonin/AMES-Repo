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

    [Header("Vision Settings")]
    public float sightRange = 10f;
    public float fieldOfView = 120f;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;

    private bool playerInSight;
    private Vector3 lastKnownPlayerPosition;
    private bool isPatrolling = true;

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
        else if (!playerInSight && isPatrolling)
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
                    lastKnownPlayerPosition = player.position; // Optional, in case you want to use it later.
                }
            }
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
        isPatrolling = false; // Stop patrolling once we start chasing.
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
