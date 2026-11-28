using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    private Rigidbody2D body;
    private BoxCollider2D boxCollider;

    private GameManager gameManager;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask boundaryLayer;
    [SerializeField] private LayerMask opponentLayer;

    [SerializeField] private PlayerStats player;
    [SerializeField] private Setting settings;

    private float serverMovementInput;
    private float localMovementInput;
    private float facingIndex = 1f;

    private Vector2 attackBoxSize;
    private Vector2 attackOffset;

    private float speed;
    private float attackDamage;
    private float attackSpeed;
    private float attackRange;

    private float duckDuration;

    private float lightPunchCost = 6f;
    private float mediumPunchCost = 12f;
    private float heavyPunchCost = 20f;
    private float duckCost = 8f;

    private float defaultDamageMultiplier;
    private float mediumDamageMultiplier;
    private float heavyDamageMultiplier;

    private bool isMenuOpen;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        if(player == null)
        {
            player = GetComponent<PlayerStats>();
        }   
        gameManager = Object.FindAnyObjectByType<GameManager>();
    }

    public override void OnNetworkSpawn()
    {
        if(gameManager == null)
        {
            gameManager = Object.FindAnyObjectByType<GameManager>();
        }

        initialCharacterStats();
        initialSettings();

        if (player != null)
        {
            player.SyncedSlot.OnValueChanged += handleSlotChanged;

            handleSlotChanged(-1, player.playerSlot);
        }
    }

    private void handleSlotChanged(int previousValue, int newValue)
    {
        if (gameManager != null)
        {
            gameManager.configurePlayer(this, newValue);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (player != null)
        {
            player.SyncedSlot.OnValueChanged -= handleSlotChanged;
        }
    }

    private void Update()
    {
        if (!IsSpawned)
        {
            return;
        }

        if (gameManager != null && gameManager.isGameEndedOrGracePeriod()) {
            localMovementInput = 0f;

            if (IsServer)
            {
                serverMovementInput = 0f;
            }
            
            return;
        }

        if (IsOwner)
        {
            handleOpenMenu();

            if (isMenuOpen) { 
                return;
            }

            handleMovementInput();
            handleBlockingInput();
            handlePunchInput();
            handleDuckingInput();
        }    
    }

    // Prevent Jittering
    private void FixedUpdate()
    {
        if(!IsServer || body == null)
        {
            return;
        }

        if (gameManager != null && gameManager.isGameEndedOrGracePeriod())
        {
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y); // Stop sliding when the gamme ends
            return;
        }
        Vector2 vector = new Vector2(serverMovementInput * speed, body.linearVelocity.y);

        if(isPlayerBlocking() || isPlayerExhausted())
        {
            vector.x /= 2f; //  Slow down movement when blocking
        }

        body.linearVelocity = vector;
    }

    // ===== Menu =====
    private void handleOpenMenu()
    {
        if (PauseMenuController.isOpen)
        {
            if (!isMenuOpen)
            {
                localMovementInput = 0f;

                SubmitMovementServerRpc(0f);
                setBlockingServerRpc(false);
            }

            isMenuOpen = true;
            return;
        }

        isMenuOpen = false;
    }

    // ===== Movement =====
    private void handleMovementInput()
    {
        float nextInput = 0;
        
        if(Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) // Left
            {
                nextInput -= 1f;
            }
            else if (Keyboard.current.dKey.isPressed) // Right
            {
                nextInput += 1f;
            }
        }

        if (!Mathf.Approximately(nextInput, localMovementInput))
        {
            localMovementInput = nextInput;
            SubmitMovementServerRpc(localMovementInput);
        }
        
    }

    [Rpc(SendTo.Server)]
    private void SubmitMovementServerRpc(float input)
    {
        serverMovementInput = Mathf.Clamp(input, -1f, 1f);
    }


    // ===== Defense =====
    // Blocking
    private void handleBlockingInput()
    {
        if (Keyboard.current == null || player == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isPlayerDucking()) // Blocking
        {
            setBlockingServerRpc(true);
        }
        else if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            setBlockingServerRpc(false);
        }
    }

    [Rpc(SendTo.Server)]
    private void setBlockingServerRpc(bool blocking)
    {
        if(player != null && player.isDucking)
        {
            return;
        }

        player.setBlocking(blocking);
    }

    // Ducking
    private void handleDuckingInput()
    {
        if (Keyboard.current != null && player != null && Keyboard.current.sKey.wasPressedThisFrame && !isDefending() && !isPlayerExhausted())
        {
            requestDuckingServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void requestDuckingServerRpc()
    {
        if (player == null || isDefending() || isPlayerExhausted() || isPlayerAttacking()) 
        { 
            return; 
        }

        if (player.consumeStamina(duckCost))
        {
            // Ducking timing is independent from update
            StartCoroutine(DuckCoroutine());
        }
    }

    private System.Collections.IEnumerator DuckCoroutine()
    {
        player.setDucking(true);
        

        yield return new WaitForSeconds(duckDuration); // Duck based on duration setting (0.1s)


        player.setDucking(false);
    }

    // ===== Attack =====
    private void handlePunchInput()
    {
        if (Keyboard.current == null || isDefending() || isPlayerAttacking() || isPlayerExhausted())
        {
            return;
        }

        if (Keyboard.current.jKey.wasPressedThisFrame) // Light Punch
        {
            requestPunchServerRpc(PunchType.Light);
        }

        else if (Keyboard.current.kKey.wasPressedThisFrame) // Medium Punch
        {
            requestPunchServerRpc(PunchType.Medium);
        }

        else if (Keyboard.current.lKey.wasPressedThisFrame) // Heavy Punch
        {
            requestPunchServerRpc(PunchType.Heavy);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void requestPunchServerRpc(PunchType type)
    {
        if (!IsServer || player == null || gameManager == null || gameManager.isGameEndedOrGracePeriod() || isDefending() || isPlayerAttacking() || isPlayerExhausted())
        {
            return;
        }

        float damageMultiplier;
        float staminaCost;

        switch (type)
        {
            case PunchType.Light:
                damageMultiplier = defaultDamageMultiplier;
                staminaCost = lightPunchCost;
                break;

            case PunchType.Medium:
                damageMultiplier = mediumDamageMultiplier;
                staminaCost = mediumPunchCost;
                break;

            case PunchType.Heavy:
                damageMultiplier = heavyDamageMultiplier;
                staminaCost = heavyPunchCost;
                break;

            default:
                return;
        }

        if (player.consumeStamina(staminaCost))
        {
            StartCoroutine(AttackCoroutine(attackDamage, damageMultiplier));
        }
    }

    private System.Collections.IEnumerator AttackCoroutine(float damage, float multiplier)
    {
        player.setAttacking(true);

        yield return new WaitForSeconds(attackSpeed);

        if (gameManager != null && !gameManager.isGameEndedOrGracePeriod())
        {
            Attack(damage, multiplier);
        }
        player.setAttacking(false);
    }
    private void Attack(float damage, float multiplier)
    {
        Vector2 attackPos = (Vector2)transform.position + attackOffset;
        Collider2D hit = Physics2D.OverlapBox(attackPos, attackBoxSize, 0f, opponentLayer);

        if (hit != null)
        {
            PlayerStats opponentStats = hit.GetComponent<PlayerStats>();
            
            if (opponentStats != null && opponentStats != player && gameManager != null)
            {
                float takenDamage = damage * multiplier;
                gameManager.processHit(opponentStats, takenDamage);
            }
        }
    }

    //===== Condition =====
    private bool isDefending()
    {
        return player != null && (player.isBlocking || player.isDucking);
    }

    private bool isPlayerDucking()
    {
        return player != null && player.isDucking;
    }

    private bool isPlayerBlocking()
    {
        return player != null && player.isBlocking;
    }

    private bool isPlayerAttacking()
    {
        return player != null && player.isAttacking;
    }

    private bool isPlayerExhausted()
    {
        return player != null && player.isExhausted;
    }

    // ===== Others =====
    private void initialCharacterStats()
    {
        if (player != null)
        {
            speed = player.speed;
            attackDamage = player.attackDamage;
            attackSpeed = player.attackSpeed;
            attackRange = player.attackRange;
        }
        else
        {
            speed = 5f;
            attackDamage = 10f;
            attackSpeed = 0.2f;
            attackRange = 1f;
        }
    }

    private void initialSettings()
    {
        if(settings != null)
        {
            duckDuration = settings.duck != null ? settings.duck.duration : 0.1f;
            defaultDamageMultiplier = settings.punch != null ? settings.punch.defaultMultiplier : 1f;
            mediumDamageMultiplier = settings.punch != null ? settings.punch.mediumMultiplier : 1.2f;
            heavyDamageMultiplier = settings.punch != null ? settings.punch.heavyMultiplier : 1.5f;
        }

        else 
        {
            duckDuration = 0.1f;
            defaultDamageMultiplier = 1f;
            mediumDamageMultiplier = 1.2f;
            heavyDamageMultiplier = 1.5f;
        }
    }

    private void setPlayerHitBox(float range)
    {
        float width = Mathf.Abs(1f + range);
        float height = Mathf.Abs(transform.localScale.y);
        float offsetX = (1f + range / 2f) * facingIndex;

        attackBoxSize = new Vector2(width, height);
        attackOffset = new Vector2(offsetX, 0f);
    }

    // 1f (P1 - left player)
    // -1f (P2 - right player)
    public void setPlayerDirection(float index = 1f)
    {
        // Return 1 when either 0 or positive 
        // Return -1 when negative
        facingIndex = Mathf.Sign(index); 
        setPlayerHitBox(attackRange);
    }

    // For multiplayer
    public void SetSelfLayer(int layerIndex)
    {
        gameObject.layer = layerIndex;
    }
    public void SetOpponentLayer(LayerMask targetOpponentLayer)
    {
        opponentLayer = targetOpponentLayer;
    }

    [Rpc(SendTo.Everyone)]
    public void configurePlayersDirectionRpc(int selfLayerIndex, int opponentLayerMaskValue, float directionIndex)
    {
        SetSelfLayer(selfLayerIndex);
        SetOpponentLayer(opponentLayerMaskValue);
        setPlayerDirection(directionIndex);
    }

    //===== Gizmos =====
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        Vector2 attackPos = (Vector2)transform.position + attackOffset;
        Gizmos.DrawWireCube(attackPos, attackBoxSize);
    }
}
