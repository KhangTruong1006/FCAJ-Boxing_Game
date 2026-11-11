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

    private bool isDucking;
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
        HandleVerticalMovementInput();
    }

    // Prevent Jittering
    private void FixedUpdate()
    {
        Vector2 vector = new Vector2(movementInput * 5f, body.linearVelocity.y);
        body.linearVelocity = vector;
    }

    // ===== Movement =====
    private void HandleVerticalMovementInput()
    {
        if(Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) // Left
            {
                movementInput -= 1f;
                transform.localScale = new Vector3(-1f, transform.localScale.y, 1f);
            }
            if (Keyboard.current.dKey.isPressed) // Right
            {
                movementInput += 1f;
                transform.localScale = new Vector3(1f, transform.localScale.y, 1f);
            }
        }
    }
}
