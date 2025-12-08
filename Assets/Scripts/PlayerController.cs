using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{


    [Header("Variables")]
    public float moveSpeed;
    public float jumpForce;
    public float rollForce;


    [HideInInspector]
    public Rigidbody2D rb;


    [HideInInspector]
    public Animator anim;


    [SerializeField]
    bool isGrounded;
    //[HideInInspector]
    public bool isAttacking;


    [HideInInspector]
    public bool onLadder;
    bool isClimping;


    //[HideInInspector]
    public string areaTransitionName;
    private Vector3 boundary1;
    private Vector3 boundary2;
    public bool canMove = true;
    private GameObject playerStart;
    public static PlayerController instance;


    [Header("Squashing Variables")]
    public float TimeInvulnerable = 0;
    private float mBlinkTimer = 0;
    private SpriteRenderer spriteRenderer;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();


        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }


        areaTransitionName = "";
        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += OnSceneChange;
    }


    private void OnSceneChange(Scene current, Scene next)
    {
        playerStart = GameObject.Find("PlayerStart");
        if (playerStart != null)
        {
            transform.position = playerStart.transform.position;
        }
    }
    private void Update()
    {
        Rotation();
        Jump();
        Roll();
        Attack();
        Animation();
        Blink();
    }


    public void Blink()
    {
        // If Invulnerable mode in ON, make player blink
        if (TimeInvulnerable > 0)
        {
            TimeInvulnerable -= Time.deltaTime;
            TimeInvulnerable = Mathf.Max(0, TimeInvulnerable);
            mBlinkTimer += Time.deltaTime;
            float limit = 0.1f;
            if (!spriteRenderer.enabled)   // Make it stay a bit longer when the sprite is visible
                limit = 0.05f;
            if (mBlinkTimer > limit)
            {
                mBlinkTimer = 0f;
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
        }
        else spriteRenderer.enabled = true;
    }


    public void FixedUpdate()
    {
        Move();
        //LadderClimb();
        GroundCheck();
    }


    public void Move()
    {
        bool isIdle = (Input.GetAxisRaw("Horizontal") == 0) && (Input.GetAxisRaw("Vertical") == 0);

        if (!canMove)
        {
            isIdle = true;
        }


        if (!isAttacking && !isIdle)
            transform.Translate(new Vector2(Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime, 0));
    }


    public void Rotation()
    {
        if (Input.GetAxisRaw("Horizontal") != 0) //if player has any horizontal movement
        {
            if (Input.GetAxisRaw("Horizontal") < 0)
            {
                transform.localScale = new Vector2(-1, 1);
            }
            else
                transform.localScale = new Vector2(1, 1);
        }
    }


    void Roll()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.LeftShift) && !isAttacking && !isClimping)
        {
            if (Mathf.Round(Input.GetAxisRaw("Horizontal")) != 0)
            {
                anim.SetTrigger("Roll");
                rb.velocity = Vector2.right * Input.GetAxisRaw("Horizontal") * rollForce;
            }
        }
    }


    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking && !isClimping)
        {
            anim.SetTrigger("Jump");
            rb.velocity = Vector2.up * jumpForce;
        }
    }

    //Attack method
    public void Attack()
    {
        if (isGrounded && (Input.GetButtonDown("Attack") || Input.GetButtonDown("Ranged Attack")) && !isAttacking && !isClimping)
        {
            isAttacking = true; //attack status
            if (Input.GetButtonDown("Attack"))
            {
                anim.SetBool("MeleeAttack", isAttacking); //Set animator bool
            }
            else if (Input.GetButtonDown("Ranged Attack"))
            {
                anim.SetBool("RangedAttack", isAttacking);
            }
        }
    }


    //Animator method
    public void Animation()
    {
        if (!isClimping && Input.GetAxisRaw("Horizontal") != 0)
            anim.SetBool("Move", true);
        else
        {
            anim.SetBool("Move", false);
        }
        anim.SetBool("isGrounded", isGrounded);
    }


    // void LadderClimb()
    // {
    //     if (onLadder) //check lader status
    //     {
    //         if (inputManager.Vertical != 0)
    //         {
    //             isClimping = true;
    //             rigid2D.velocity = Vector2.up * inputManager.Vertical * moveSpeed; //move up or down
    //         }
    //         else
    //         {
    //             rigid2D.velocity = Vector2.zero;
    //         }


    //     }
    //     else //if leave ladder
    //     {
    //         isClimping = false;
    //     }


    //     if (onLadder)
    //         rigid2D.gravityScale = 0;
    //     else
    //         rigid2D.gravityScale = 1;
    // }


    //Death event
    public void Death()
    {


    }


    //Check ground
    void GroundCheck()
    {
        if (rb.velocity.y <= .1 && rb.velocity.y >= -.1)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }


        if (rb.velocity.y < -0.2)
        {
            anim.SetBool("isFalling", true);
        }
        else
        {
            anim.SetBool("isFalling", false);
        }


        if (rb.velocity.y > 0.2)
        {
            anim.SetBool("isJumping", true);
        }
        else
        {
            anim.SetBool("isJumping", false);
        }
    }


    //Method to set up the bounds which the player can not cross
    public void SetBounds(Vector3 bound1, Vector3 bound2)
    {
        boundary1 = bound1 + new Vector3(.5f, 1f, 0f);
        boundary2 = bound2 + new Vector3(-.5f, -1f, 0f);
    }


}



