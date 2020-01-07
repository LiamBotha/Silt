using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float gravity = -15f;
    public float targetJumpHeight = 3f; // jump value
    public float lowJumpMultiplier =  2f; // tap jump
    public float fallMultiplier = 3f;
    public float runSpeed = 8f;
    public float dashSpeed = 30f;

    private float lastWallTouchTime = 0.0f;
    private float lastGroundTouchTime = 0.0f;

    private float maxHorizSpeed = 30.0f; 
    private float maxVertSpeed = 35.0f;

    private float gravityMultiplier = 1; // increments fall speed over time

    private int currWallDir = 0;
    private int numDashes = 1;

    private bool canMove = true;
    private bool gravityActive = true;
    private bool hasDashed = false;
    private bool isWallJumping = false;
    private bool isDashing = false;

    private Vector2 playerVelocity;
    private Vector2 dashStartPoint; // assigned when the player starts to dash

    private CharacterController2D controller;
    private SpriteRenderer sprite;
    private PlayerCollisions playerColl;
    private Animator anim;

    void Awake()
    {
        controller = GetComponent<CharacterController2D>();
        sprite = GetComponent<SpriteRenderer>();
        playerColl = GetComponent<PlayerCollisions>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");

        if (!controller.isGrounded && !(playerColl.OnRightWall || playerColl.OnLeftWall) && lastWallTouchTime < 0.3f)
        {
            lastWallTouchTime += Time.deltaTime;
        }
        else if (controller.isGrounded || playerColl.OnRightWall || playerColl.OnLeftWall)
        {
            lastWallTouchTime = 0.0f;
        }

        if (!controller.isGrounded && lastGroundTouchTime < 0.1f)
        {
            lastGroundTouchTime += Time.deltaTime;
        }
        else if (controller.isGrounded)
        {
            lastGroundTouchTime = 0.0f;
        }

        if (lastWallTouchTime > 0.25f)
        {
            isWallJumping = false;
            currWallDir = 0;
        }

        CheckCollisions(); // checks if right, left, or ground and sets bools relevant. 

        HandleMovement(xRaw, yRaw);
    }

    private void CheckCollisions()
    {
        if (playerColl.OnRightWall && currWallDir != 1)
        {
            currWallDir = 1;
            isWallJumping = false;

            ResetCoroutines();

            if (isDashing)
            {
                playerVelocity.x = playerVelocity.x / 1.5f;
                playerVelocity.y = playerVelocity.y / 1.5f;

                controller.move(playerVelocity * Time.deltaTime);

                isDashing = false;
            }
        }
        else if (playerColl.OnLeftWall && currWallDir != -1)
        {
            currWallDir = -1;
            isWallJumping = false;

            ResetCoroutines();

            if (isDashing)
            {
                playerVelocity.x = playerVelocity.x / 1.5f;
                playerVelocity.y = playerVelocity.y / 1.5f;

                controller.move(playerVelocity * Time.deltaTime);

                isDashing = false;
            }
        }

        if (isWallJumping && controller.isGrounded)
        {
            isWallJumping = false;
            currWallDir = 0;
        }
    }

    private void HandleMovement(float xRaw, float yRaw)
    {
        playerVelocity = controller.velocity;

        if (controller.isGrounded)
        {
            playerVelocity.y = 0;
            gravityMultiplier = 1;

            hasDashed = false;

            anim.SetBool("isDashing", false);
            anim.SetBool("isJumping", false);
        }
        else
        {
            gravityMultiplier += Time.deltaTime;
        }

        //Input
        if (canMove && xRaw == 1) // TODO - still cancels out current momentum
        {
            playerVelocity.x = Mathf.Max(playerVelocity.x, runSpeed);
            //playerVelocity.x = runSpeed;
            FlipSprite(1);
        }
        else if (canMove && xRaw == -1)
        {
            playerVelocity.x = Mathf.Min(playerVelocity.x, -runSpeed);
            //playerVelocity.x = -runSpeed;
            FlipSprite(-1);
        }
        else if (canMove && controller.isGrounded) // TODO - msybe check if player is pressing down to go straight down
        {
            playerVelocity.x = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && (controller.isGrounded || lastGroundTouchTime < 0.1f))
        {
            playerVelocity.y = Mathf.Sqrt(2f * targetJumpHeight * -gravity);

            anim.SetBool("isJumping", true);
        }
        else if(Input.GetKeyDown(KeyCode.Space) && !isWallJumping && !controller.isGrounded)
        {
            WallJump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !hasDashed)
        {
            StartCoroutine(DashCoroutine(xRaw, yRaw));
        }

        //Gravity
        if(gravityActive)
        {   
            if(!controller.isGrounded && playerVelocity.y < 0 && (playerColl.OnRightWall || playerColl.OnLeftWall)) // wall slide
            {
                playerVelocity.y += (gravity / 2) * gravityMultiplier * Time.deltaTime;
            }
            else if (playerVelocity.y < 0)
            {
                playerVelocity.y += gravity * gravityMultiplier * (fallMultiplier) * Time.deltaTime;
            }
            else if (playerVelocity.y > 0 && !Input.GetKey(KeyCode.Space) && !isWallJumping && !isDashing)
            {
                playerVelocity.y += gravity * gravityMultiplier * (lowJumpMultiplier) * Time.deltaTime;
            }
            else
            {
                playerVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
            }
        }

        //Capping speed
        if (playerVelocity.x > 0)
            playerVelocity.x = Mathf.Min(playerVelocity.x, maxHorizSpeed);
        else if(playerVelocity.x < 0)
            playerVelocity.x = Mathf.Max(playerVelocity.x, -maxHorizSpeed);

        if (playerVelocity.y > 0)
            playerVelocity.y = Mathf.Min(playerVelocity.y, maxVertSpeed);
        else if(playerVelocity.y < 0)
            playerVelocity.y = Mathf.Max(playerVelocity.y, -maxVertSpeed);

        //Setting Movement
        controller.move(playerVelocity * Time.deltaTime);
    }

    private void WallJump() // TODO - Check xraw for smoother wall runs
    {
        if(lastWallTouchTime < 0.1f)
        {
            StopCoroutine(DisableMovement(0.0f));
            StartCoroutine(DisableMovement(0.3f));

            isWallJumping = true;
            gravityMultiplier = 1.5f;
            playerVelocity.y = 15f;
            playerVelocity.x = runSpeed * -currWallDir * 1.4f;

            sprite.flipX = !sprite.flipX;
        }
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;

        yield return new WaitForSeconds(time);

        canMove = true;
    }

    IEnumerator DisableGravity(float time)
    {
        gravityActive = false;

        yield return new WaitForSeconds(time);

        gravityActive = true;
    }

    IEnumerator DashCoroutine(float xRaw, float yRaw)
    {
        numDashes -= 1;
        hasDashed = true;
        isDashing = true;

        anim.SetBool("isDashing", true);

        dashStartPoint = transform.position;

        StopCoroutine(DisableGravity(0f));
        StopCoroutine(DisableMovement(0f));

        StartCoroutine(DisableGravity(0.25f));
        StartCoroutine(DisableMovement(0.2f));

        gravityMultiplier = 1;
        playerVelocity.x = 0;
        playerVelocity.y = 0;

        var tempDashSpeed = dashSpeed;

        if (xRaw != 0 && yRaw != 0)
        {
            tempDashSpeed = dashSpeed / 1.75f;

            playerVelocity.x = xRaw * tempDashSpeed;
            playerVelocity.y = yRaw * tempDashSpeed;
        }
        else if (xRaw != 0 || yRaw != 0)
        {
            playerVelocity.x = xRaw * tempDashSpeed;
            playerVelocity.y = yRaw * (tempDashSpeed / 1.75f);
        }
        else
        {
            if (sprite.flipX)
                playerVelocity.x = -1 * tempDashSpeed;
            else
                playerVelocity.x = 1 * tempDashSpeed;
        }

        yield return new WaitForSeconds(0.16f);

        playerVelocity.x = playerVelocity.x / 3;
        playerVelocity.y = playerVelocity.y / 3;

        controller.move(playerVelocity * Time.deltaTime);

        isDashing = false;
    }

    private void Dash(float xRaw, float yRaw) // TODO - use a coroutine to apply this overtime and reset speed after dash ends
    {
        hasDashed = true;

        StopCoroutine(DisableGravity(0f));
        StartCoroutine(DisableGravity(0.18f));

        StopCoroutine(DisableMovement(0f));
        StartCoroutine(DisableMovement(0.3f));

        gravityMultiplier = 1;

        playerVelocity.x = 0;
        playerVelocity.y = 0;

        var tempDashSpeed = (runSpeed * 2);

        if (xRaw != 0 && yRaw != 0)
        {
            tempDashSpeed = (runSpeed * 2f) / 1.25f;
        }

        if (xRaw != 0 || yRaw != 0)
        {
            playerVelocity.x = xRaw * tempDashSpeed;
            playerVelocity.y = yRaw * tempDashSpeed;
        }
        else
        {
            if (sprite.flipX)
                playerVelocity.x = -1 * runSpeed * 2;
            else
                playerVelocity.x = 1 * runSpeed * 2;
        }

        Debug.Log(xRaw + ", " + yRaw);
    }

    private void BouncePlayer(RaycastHit2D obj)
    {
        if (isDashing)
        {
            isDashing = false;

            var contactPoint = obj.point;
            var playerDirection = contactPoint - dashStartPoint;

            Vector2 newDir = Vector2.Reflect(playerDirection, obj.normal).normalized;

            StopAllCoroutines();

            StartCoroutine(DisableGravity(0.03f));
            StartCoroutine(DisableMovement(0.2f));

            playerVelocity.x = newDir.x * Mathf.Abs(playerVelocity.x / 1.5f);
            playerVelocity.y = newDir.y * Mathf.Abs(playerVelocity.y / 1.5f);

            controller.move(playerVelocity * Time.deltaTime);
        }
    }

    private void FlipSprite(int side)
    {
        sprite.flipX = side == -1;
    }

    private void ResetCoroutines()
    {
        StopAllCoroutines();

        canMove = true;
        gravityActive = true;

        anim.SetBool("isDashing", false);
        anim.SetBool("isJumping", false);
    }
}
