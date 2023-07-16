using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class player : Entity
{

    [SerializeField] private float health;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float jumpForce = 1f;
    private bool isGrounded = false;

    [SerializeField] private Image[] hearts;

    [SerializeField] private Sprite aliveHeart;
    [SerializeField] private Sprite deadHeart;

    public bool isAttacking = false; // Атакуем ли сейчас  
    public bool isRecharged = true; // Перезерадились ли сейчас

    public Transform attackPos; // Позиция аттаки
    public float attackRange; // Дальность аттаки
    public LayerMask enemy; // Слой с врагами

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;


    public static player Instance { get; set; }


    private States State
    {
        get { return (States)anim.GetInteger("State"); }
        set { anim.SetInteger("State", (int)value); }
    }


    private void Awake()
    {
        lives = 5;
        health = lives;
        Instance = this;
        sprite = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        isRecharged = true; // В начале игры герой перезаряжен
    }

    private void Run()
    {
        if (isGrounded) State = States.Run;

        Vector3 dir = transform.right * Input.GetAxis("Horizontal");
        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);
        sprite.flipX = dir.x < 0.0f;
    }
    private void Update()

    {
        if (isGrounded && !isAttacking) State = States.Idle;

        if (!isAttacking && Input.GetButton("Horizontal"))
            Run();
        if (!isAttacking && isGrounded && Input.GetButtonDown("Jump"))
            Jump();
        if (Input.GetButtonDown("Fire1"))
            Attack();

        if (health > lives)
            health = lives;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < Mathf.RoundToInt(health))
            {
                hearts[i].sprite = aliveHeart;
            }
            else
            {
                hearts[i].sprite = deadHeart;
            }

            if (i < lives)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = deadHeart;
            }
            if (health < 1)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
    private void FixedUpdate()
    {
        CheckGround();
    }

    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
    }

    private void CheckGround()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        isGrounded = collider.Length > 1;

        if (!isGrounded) State = States.Jump;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }

    private IEnumerator AttackAnimation()
    {
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
    }

    private IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(0.5f);
        isRecharged = true;
    }

    private void Attack()
    {
        if (isGrounded && isRecharged)
        {
            State = States.Attack;
            isAttacking = true;
            isRecharged = false;

            StartCoroutine(AttackAnimation());
            StartCoroutine(AttackCoolDown());
        }
    }
    private void OnAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPos.position, attackRange, enemy);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].GetComponent<Entity>().GetDamage();
        }
    }

    public override void GetDamage()
    {
        lives -= 1;
        if (health < 1)
        {
            Die();
            foreach (var h in hearts)
                h.sprite = deadHeart;
            State = States.Dead;
        }
 
    }

}


public enum States
{
    Idle,
    Run,
    Jump,
    Attack,
    Dead

}