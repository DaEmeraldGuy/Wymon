using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]

public class PlayerController : MonoBehaviour

{

    [Header("Movement")]

    public float moveSpeed = 5f;

    public float jumpForce = 12f;

    private bool isGrounded;


    [Header("Whip Attack")]

    public Transform whipPoint;

    public GameObject whipPrefab;

    public float whipCooldown = 0.5f;

    private float lastWhipTime;



    private Rigidbody2D rb;

    private Animator anim;

    private float horizontal;



    void Start()

    {

        rb = GetComponent<Rigidbody2D>();

        anim = GetComponent<Animator>();

    }



    void Update()

    {

        // Input

        horizontal = Input.GetAxisRaw("Horizontal");



        // Jump

        if (Input.GetButtonDown("Jump") && isGrounded)

        {

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        }



        // Attack

        if (Input.GetButtonDown("Fire1") && Time.time > lastWhipTime)

        {

            Attack();

            lastWhipTime = Time.time + whipCooldown;

        }



        // Update animations

        anim.SetFloat("Speed", Mathf.Abs(horizontal));

        anim.SetBool("IsGrounded", isGrounded);

    }



    void FixedUpdate()

    {

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        if (horizontal != 0)

            transform.localScale = new Vector3(Mathf.Sign(horizontal), 1, 1);

    }



    void Attack()

    {

        anim.SetTrigger("Attack");

        Instantiate(whipPrefab, whipPoint.position, whipPoint.rotation);

    }



    private void OnCollisionEnter2D(Collision2D collision)

    {

        if (collision.contacts[0].normal.y > 0.5f)

            isGrounded = true;

    }



    private void OnCollisionExit2D(Collision2D collision)

    {

        if (collision.contacts[0].normal.y > 0.5f)

            isGrounded = false;

    }

}