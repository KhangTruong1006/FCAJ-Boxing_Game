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

    private float movementInput;

    private Vector2 attackBoxSize;
    private Vector2 attackOffset;

    private float speed;
    private float attackDamage;
    private float attackSpeed;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        player = GetComponent<PlayerStats>();
        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    //REMEMBER TO ADD ANIMATION IN LATER STAGES

    private void Start()
    {
        speed = player.speed;
        
        attackDamage = player.attackDamage;
        attackSpeed = player.attackSpeed;

        setPlayerHitBox(player.attackRange);
    }

    private void Update()
    {
        movementInput = 0f;

        if (gameManager != null && isGameEndedOrGracePeriod()) { 
            return;
        }

        handleVerticalMovementInput();
        handleBlockingInput();
        handlePunchInput();
        handleDuckingInput();
    }

    // Prevent Jittering
    private void FixedUpdate()
    {
        if (gameManager != null && isGameEndedOrGracePeriod())
        {
            return;
        }
        Vector2 vector = new Vector2(movementInput * speed, body.linearVelocity.y);
        if(isPlayerBlocking())
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


    // ===== Defense =====
    private void handleBlockingInput()
    {
        if (Keyboard.current != null && !isPlayerDucking())
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) // Blocking
            {
                player.setBlocking(true);  
            }
            else if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                player.setBlocking(false);
            }    
        }
    }

    private void handleDuckingInput()
    {
        if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame && !isDefending())
        {
            // Ducking timing is independent from update
            StartCoroutine(DuckCoroutine());
        }
    }
    
    private System.Collections.IEnumerator DuckCoroutine()
    {
        player.setDucking(true);


        yield return new WaitForSeconds(settings.duck.duration); // Duck based on duration setting (0.1s)


        player.setDucking(false);
    }

    // ===== Attack =====
    private void handlePunchInput()
    {
        if (Keyboard.current == null || isDefending() || isPlayerAttacking())
        {
            return;
        }

        if (Keyboard.current.jKey.wasPressedThisFrame) // Light Punch
        {
            StartCoroutine(AttackCoroutine(attackDamage, settings.punch.defaultMultiplier));
        }

        else if (Keyboard.current.kKey.wasPressedThisFrame) // Medium Punch
        {
            StartCoroutine(AttackCoroutine(attackDamage, settings.punch.mediumMultiplier));
        }

        else if (Keyboard.current.lKey.wasPressedThisFrame) // Heavy Punch
        {
            StartCoroutine(AttackCoroutine(attackDamage, settings.punch.heavyMultiplier));
        }
    }
    private void Attack(float damage, float multiplier)
    {
        Vector2 attackPos = (Vector2)transform.position + attackOffset;
        Collider2D hit = Physics2D.OverlapBox(attackPos, attackBoxSize, 0f, opponentLayer);

        if (hit != null) {
            float takenDamage = damage * multiplier;

            PlayerStats opponentStats = hit.GetComponent<PlayerStats>();
            gameManager.ProcessHit(opponentStats, takenDamage);
        }
    }

    private System.Collections.IEnumerator AttackCoroutine(float damage, float multiplier)
    {
        player.setAttacking(true);

        yield return new WaitForSeconds(attackSpeed);

        Attack(damage, multiplier);
        player.setAttacking(false);
    }
   
    //===== Condition =====
    private bool isDefending()
    {
        return player.isBlocking || player.isDucking;
    }

    private bool isPlayerDucking()
    {
        return player.isDucking;
    }

    private bool isPlayerBlocking()
    {
        return player.isBlocking;
    }

    private bool isPlayerAttacking()
    {
        return player.isAttacking;
    }

    private bool isGameEndedOrGracePeriod()
    {
        return gameManager.isGameEnded || gameManager.isGracePeriod;
    }

    // ===== Other =====
    private void setPlayerHitBox(float range)
    {
        float boxSize = 1f + range;
        float offset = 1f + range / 2;

        attackBoxSize = new Vector2(boxSize, transform.localScale.y);
        attackOffset = new Vector2(offset, 0f);
    }

    //===== Gizmos =====
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        Vector2 attackPos = (Vector2)transform.position + attackOffset;
        Gizmos.DrawWireCube(attackPos, attackBoxSize);
    }
}
