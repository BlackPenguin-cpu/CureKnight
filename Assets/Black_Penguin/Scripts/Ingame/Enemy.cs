using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Entity
{
    Animator anim;
    public GameObject player;
    float distance;
    [SerializeField] float nowAttackCooldown;
    public float AttackCooldown = 0;
    public float range;

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    protected override void Update()
    {
        base.Update();
        anim.SetInteger("State", (int)entityState);
        switch (entityState)
        {
            case EntityState.IDLE:
                break;
            case EntityState.MOVING:
                Move();
                break;
            case EntityState.ONDAMAGE:
                Hit();
                break;
            case EntityState.ATTACK:
                if (AttackCooldown < nowAttackCooldown)
                    Attack();
                break;
            case EntityState.DIE:
                Die();
                break;
            default:
                break;
        }
        nowAttackCooldown += Time.deltaTime;
    }
    protected override void Die()
    {
        Debug.Log($"{gameObject}�� ����");
    }
    protected virtual void Attack()
    {
        nowAttackCooldown = 0;
    }

    //private void Attack()
    //{
    //    var rayhit = Physics2D.RaycastAll(transform.position, Vector3.right, distance);
    //    Debug.DrawRay(transform.position, Vector3.right * distance);
    //    foreach(var hit in rayhit)
    //    {
    //        if (hit.collider.gameObject.tag == "Player")
    //        {
    //            hit.collider.gameObject.GetComponent<Entity>()._hp -= Damage;
    //        }
    //    }
    //}

    private void Move()
    {
        float x = player.transform.position.x;
        distance = Mathf.Abs(gameObject.transform.position.x - x);
        if (distance <= range)
        {
            entityState = EntityState.ATTACK;
        }
        else
        {
            if (gameObject.transform.position.x < x)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
                transform.position += new Vector3(Speed * Time.deltaTime, 0, 0);
            }
            if (gameObject.transform.position.x > x)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
                transform.position -= new Vector3(Speed * Time.deltaTime, 0, 0);
            }
        }
    }

    protected override void Hit()
    {
        Debug.Log($"{gameObject.name} �ǰ�");
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        if (collision.tag == "PlayerAttack" && entityState != EntityState.ONDAMAGE)
        {
            if (collision.name == "ShockWaveAttack")
            {
                Debug.Log("shockwave");
                _hp -= player.GetComponent<Player>().Damage * 2 - (Mathf.Abs(transform.position.x - player.transform.position.x));
            }
            else
            {
                _hp -= player.GetComponent<Player>().Damage;
            }
        }
    }
}
