using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;

    private GameManager gameManager;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask boundaryLayer;
    [SerializeField] private LayerMask opponentLayer;

    [SerializeField] private PlayerStats player;
    [SerializeField] private Setting settings;

    public float movementInput;

    public bool isDucking;
    public bool isBlocking;
    private bool isAttacking;

    [Header("Hitting Box Range")]
    [SerializeField] private Vector2 attackBoxSize;
    [SerializeField] private Vector2 attackOffset;


    public float speed;
    public float attackDamage;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    //REMEMBER TO ADD ANIMATION IN LATER STAGES

    private void Start()
    {
        speed = player.speed;
        attackDamage = player.attackDamage;

        // REMEMBER TO MAKE A EQUATION FOR OFFSET WITH DIFFERENT CHARACTER'S ATTACK RANGE
        attackBoxSize = new Vector2(1f, 0.5f);
        attackOffset = new Vector2(0.8f, 0f);  
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
        Vector2 vector = new Vector2(movementInput * speed, body.linearVelocity.y);
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
            }
            else if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                isBlocking = false;
            }

            player.isBlocking = isBlocking;
        }
    }

    private void handlePunchInput()
    {
        if (Keyboard.current != null && !isDefending())
        {
            if (Keyboard.current.jKey.wasPressedThisFrame) // Light Punch
            {
                Attack(attackDamage, settings.punch.defaultMultiplier);
            }

            else if (Keyboard.current.kKey.wasPressedThisFrame) // Medium Punch
            {
                Attack(attackDamage, settings.punch.mediumMultiplier);
            }

            else if (Keyboard.current.lKey.wasPressedThisFrame) // Heavy Punch
            {
                Attack(attackDamage, settings.punch.heavyMultiplier);
            }
        }
    }

    private void Attack(float damage, float multiplier)
    {
        Vector2 attackPos = (Vector2)transform.position + attackOffset;
        Collider2D hit = Physics2D.OverlapBox(attackPos, attackBoxSize, 0f, opponentLayer);

        if (hit != null) {
            float takenDamage = damage * multiplier;

            PlayerStats opponentStats = hit.GetComponent<PlayerStats>();
            gameManager.ProcessHitDetection(opponentStats, takenDamage);
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
        player.isDucking = isDucking;
        print("Duck");


        yield return new WaitForSeconds(settings.duck.duration); // Duck for 0.1 seconds


        isDucking = false;
        player.isDucking = isDucking;
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
        
        Vector2 attackPos = (Vector2)transform.position + attackOffset;
        Gizmos.DrawWireCube(attackPos, attackBoxSize);
    }
}
