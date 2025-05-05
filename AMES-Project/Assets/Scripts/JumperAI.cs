using UnityEngine;
using System.Collections;

public class JumperEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private Rigidbody rb;

    [Header("Jump Settings")]
    public float forwardForce = 10f;
    public float upwardForce = 15f;
    public float detectionRange = 20f;
    public float jumpCooldown = 3f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckRadius = 0.3f;
    public Transform groundCheckPoint;

    [Header("Drag Settings")]
    public float landingDrag = 10f;
    public float dragResetDelay = 0.2f;

    [Header("Teleport Settings")]
    public float teleportCooldown = 5f;
    public float teleportRadius = 20f;
    public float teleportDelay = 1f;
    public LayerMask teleportGroundMask;

    private float lastTeleportTime = -Mathf.Infinity;
    private float initialDrag;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isPouncing = false;
    private float cooldownTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialDrag = rb.drag;
        LockRotation();
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f && isGrounded && PlayerInRange())
        {
            JumpAtPlayer();
            cooldownTimer = jumpCooldown;
        }
    }

    void FixedUpdate()
    {
        wasGrounded = isGrounded;
        CheckGrounded();

        if (isPouncing && isGrounded && !wasGrounded)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            rb.drag = landingDrag;
            StartCoroutine(ResetDragAfterDelay());
            isPouncing = false;
        }

        if (!PlayerInRange() && isGrounded && Time.time - lastTeleportTime > teleportCooldown)
        {
            StartCoroutine(TeleportAfterDelay());
            lastTeleportTime = Time.time;
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundMask);
    }

    bool PlayerInRange()
    {
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }

    void JumpAtPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        Vector3 force = directionToPlayer * forwardForce + Vector3.up * upwardForce;

        rb.velocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);

        isPouncing = true;
    }

    IEnumerator ResetDragAfterDelay()
    {
        yield return new WaitForSeconds(dragResetDelay);
        rb.drag = initialDrag;
    }

    IEnumerator TeleportAfterDelay()
    {
        yield return new WaitForSeconds(teleportDelay);

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomOffset = Random.insideUnitSphere * teleportRadius;
            randomOffset.y = 10f;
            Vector3 targetPos = transform.position + randomOffset;

            if (Physics.Raycast(targetPos, Vector3.down, out RaycastHit hit, 20f, teleportGroundMask))
            {
                transform.position = hit.point + Vector3.up * 1f;
                rb.velocity = Vector3.zero;
                break;
            }
        }
    }

    void LockRotation()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, teleportRadius);
    }
}
