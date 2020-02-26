using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    private float dashTimeLeft; // how much long dash will be happend
    private float lastImageXpos;
    private float lastDash = -100f;

    private int amountOfJumpsLeft;
    private int facingDirection = 1;
    private int lastWallJumpDirection;


    public float movementSpeed = 10;
    public float jumpForce = 16;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlidingSpeed;
    public float movementForceInAir; 
    public float airDragMultiplayer = 0.9f;
    public float variableJumpHeightMultiplier = 0.5f;
    // wall jump 
    public float wallHopForce;
    public float wallJumpForce;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    // ledge climb
    public float wallJumpTimerSet = 0.5f;
    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbYOffset2 = 0f;
    // dash
    public float dashTime; // how long dash take
    public float dashSpeed; // how fast char will move 
    public float distanceBetweenImages; // how far images will be placed
    public float dashCoolDown; // how long we have to wait for next dash


    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    public int amountOfJumps = 1;

    private Rigidbody2D rb;
    private Animator anim;

    private bool IsFacingRight = true;
    private bool isWalking; 
    private bool isGround;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canWallJump;
    private bool canNormalJump;
    private bool isAttempingToJump;
    private bool CheckIfCanJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJump;
    private bool isTouchingLedge;
    private bool canClimbLedge = false;
    private bool ledgeDeceted;
    private bool isDashing;

    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;
    

    public Transform wallCheck;
    public Transform groundCheck;
    public Transform ledgeCheck;

    public LayerMask whatIsGround;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
        CheckLedgeClimb();
        CheckDash();
    }

    private void FixedUpdate() 
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGround);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void CheckSurroundings()
    {
        isGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if(isTouchingWall && !isTouchingLedge && !ledgeDeceted)
        {
            ledgeDeceted = true;
            ledgePosBot = wallCheck.position;
        }

    }


    private void CheckMovementDirection()
    {
        if (IsFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if (!IsFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        if(rb.velocity.x != 0)
        {
            isWalking = true;
        }
        else 
        {
            isWalking = false;
        }
    }

    private void CheckIfCanJump() {
        if(isGround && rb.velocity.y <= 0.01f)
        {
           amountOfJumpsLeft = amountOfJumps;
        } 
        if (isTouchingWall)
        {
            canWallJump = true;
        }


        if(amountOfJumpsLeft <= 0)
        {
            canNormalJump = false;
        }
        else 
        {
            canNormalJump = true;
        }
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0 && !canClimbLedge)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckLedgeClimb() // when we decect ledge make climb 
    {
        if(ledgeDeceted && !canClimbLedge)
        {
            canClimbLedge = true;

            if (IsFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;

            anim.SetBool("canClimbLedge", canClimbLedge);
        }

        if (canClimbLedge)
        {
            transform.position = ledgePos1;
        }
    }

    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDeceted = false;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    private void CheckInput() 
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            if(isGround || (amountOfJumpsLeft > 0 && isTouchingWall))
            {
                NormalJump();
            }
            else 
            {
                jumpTimer = jumpTimerSet;
                isAttempingToJump = true;
            }
        }

        if (Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if(!isGround && movementInputDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if (turnTimer >= 0)
        {
            turnTimer -= Time.deltaTime;

            if(turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }

        }

        if(CheckIfCanJumpMultiplier && !Input.GetButton("Jump"))
        {
            CheckIfCanJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
        
        if(Input.GetButtonDown("Dash"))
        {   
            if(Time.time >= (lastDash + dashCoolDown))
            AttemptToDash();
        }
    }

    private void AttemptToDash()
    {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXpos = transform.position.x;
    }

    private void CheckDash()
    {
        if(isDashing)
        {
            if(dashTimeLeft > 0)
            {
                canMove = false; // cant control meanwhile dash
                canFlip = false;
                rb.velocity = new Vector2(dashSpeed * facingDirection, rb.velocity.y); // if you dont want to fall while dash, set to 0
                dashTimeLeft -= Time.deltaTime;

                if (Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXpos = transform.position.x;
                }
            }
            if (dashTimeLeft < 0 || isTouchingWall)
            {
                isDashing = false;
                canMove = true;
                canFlip = true;
            }
        }
    }

    private void CheckJump()
    {
      if(jumpTimer > 0)
      {
          if(!isGround && isTouchingWall && movementInputDirection != 0 && movementInputDirection != facingDirection)
          {
              WallJump();
          }
          else if (isGround)
          {
              NormalJump();
          }
      }

      if(isAttempingToJump) 
        {
            jumpTimer -= Time.deltaTime;
        }
    
        if (wallJumpTimer < 0)
        {
            if(hasWallJump && movementInputDirection == -lastWallJumpDirection)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJump = false;    
            }
        } else if(wallJumpTimer <= 0)
        {
            hasWallJump = false;
        }
        else
        {
            wallJumpTimer -= wallJumpTimer;
        }
    
      
    }

    private void NormalJump()
    {
        if (canNormalJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
            jumpTimer = 0;
            isAttempingToJump = false;
            CheckIfCanJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        if (canWallJump)
        {   
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpsLeft = amountOfJumps;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttempingToJump = false;
            CheckIfCanJumpMultiplier = true;
            canMove = true;
            canFlip = true;
            hasWallJump = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void ApplyMovement()
    {

        if (!isGround && !isWallSliding && movementInputDirection == 0)
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }
        else if(canMove)
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }
       

        if (isWallSliding)
        {
            if(rb.velocity.y < -wallSlidingSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
            }
        }
    }
        

   private void Flip() 
   {
       if(!isWallSliding && canFlip)
       {
       facingDirection *= -1; 
       IsFacingRight = !IsFacingRight;
       transform.Rotate(0.0f, 180.0f, 0.0f);
       }
   }

  private void  OnDrawGizmos() 
  {
      Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

      Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
  }

 
}
