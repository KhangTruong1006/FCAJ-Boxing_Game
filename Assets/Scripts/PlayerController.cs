using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask boundaryLayer;
    [SerializeField] private LayerMask opponentLayer;

    private float movementInput;

    public bool isDucking;
    public bool isBlocking;
    private bool isAttacking;
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        movementInput = 0f;
        handleVerticalMovementInput();
        handleBlockingInput();
        handlePunchInput();
        handleDuckingInput();
    }

    // Prevent Jittering
    private void FixedUpdate()
    {
        Vector2 vector = new Vector2(movementInput * 5f, body.linearVelocity.y);
        if(isBlocking)
        {
            vector.x /= 2f; //  Slow down movement when blocking
        }

        body.linearVelocity = vector;
    }

    // ===== Movement =====
    private void handleVerticalMovementInput()
    {
        if(Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) // Left
            {
                movementInput -= 1f;
            }
            else if (Keyboard.current.dKey.isPressed) // Right
            {
                movementInput += 1f;
            }
        }
    }

    private void handleBlockingInput()
    {
        if (Keyboard.current != null && !isDucking)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) // Blocking
            {
                isBlocking = true;
                Debug.Log("Blocking");
            }
            else if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                isBlocking = false;
                Debug.Log("Release Blocking");
            }
        }
    }

    private void handlePunchInput()
    {
        if (Keyboard.current != null && !isDefending())
        {
            if (Keyboard.current.jKey.wasPressedThisFrame) // Light Punch
            {
                Debug.Log("Light Punch");
            }

            else if (Keyboard.current.kKey.wasPressedThisFrame) // Medium Punch
            {
                Debug.Log("Medium Punch");
            }

            else if (Keyboard.current.lKey.wasPressedThisFrame) // Heavy Punch
            {
                Debug.Log("Heavy Punch");
            }
        }
    }

    private void handleDuckingInput()
    {
        if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame && !isBlocking)
        {
            // Ducking timing is independent from update
            StartCoroutine(DuckCoroutine());
        }
    }

    private System.Collections.IEnumerator DuckCoroutine()
    {
        isDucking = true;
        print("Duck");


        yield return new WaitForSeconds(0.5f); // Duck for 0.5 seconds


        isDucking = false;
        print("Release Duck");
    }
    
    //===== Condition =====
    private bool isDefending()
    {
        return isBlocking || isDucking;
    }


    //===== Gizmos =====
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
