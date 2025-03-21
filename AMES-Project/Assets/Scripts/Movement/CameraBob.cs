using UnityEngine;

public class ViewBobbing : MonoBehaviour
{
    public float bobbingSpeed = 0.18f; // Speed of bobbing
    public float bobbingAmount = 0.05f; // Height of bobbing
    private Vector3 initialPosition;
    private float timer = 0f;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        HandleBobbing();
    }

    void HandleBobbing()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
        {
            // Player is moving
            timer += Time.deltaTime * bobbingSpeed;
            float bobbingY = Mathf.Sin(timer) * bobbingAmount;
            transform.localPosition = new Vector3(initialPosition.x, initialPosition.y + bobbingY, initialPosition.z);
        }
        else
        {
            // Player is idle
            timer = 0f;
            transform.localPosition = initialPosition;
        }
    }
}
