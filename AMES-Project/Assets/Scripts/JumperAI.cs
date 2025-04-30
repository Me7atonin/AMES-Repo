using UnityEngine;
using System.Collections;

public class JumpyEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private Rigidbody rb;
    private Animator animator;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float jumpCooldown = 3f;
    public float detectionRange = 15f;
    public float lungeHeightOffset = 2f;

    [Header("Ground Detection")]
    public LayerMask groundMask;
    public float groundCheckDistance = 1.1f;

    private float cooldownTimer;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Ensure rigidbody settings are valid
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;

        cooldownTimer = jumpCooldown;
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // Check if grounded using raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (cooldownTimer <= 0 && isGrounded && distanceToPlayer <= detectionRange)
        {
            LungeAtPlayer();
            cooldownTimer = jumpCooldown;
        }
    }

    void LungeAtPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        // Calculate boosted lunge
        float boostedHorizontalForce = jumpForce * 1.5f;
        float boostedVerticalForce = jumpForce + (lungeHeightOffset * 2f);

        Vector3 lunge = directionToPlayer * boostedHorizontalForce + Vector3.up * boostedVerticalForce;

        // Stop current velocity
        rb.velocity = Vector3.zero;

        // Face the player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // Trigger animation
        if (animator)
            animator.SetTrigger("Lunge");

        // Apply force after a short delay to sync with physics update
        StartCoroutine(DelayedJump(lunge));
    }

    IEnumerator DelayedJump(Vector3 force)
    {
        yield return new WaitForFixedUpdate();

        // Apply lunge force directly as velocity
        rb.velocity = force;

        Debug.Log("Lunge Force Applied: " + force);
        Debug.Log("Rigidbody Velocity After Lunge: " + rb.velocity);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}
