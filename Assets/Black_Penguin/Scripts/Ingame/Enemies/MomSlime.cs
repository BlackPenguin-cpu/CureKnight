using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MomSlime : Enemy
{
    [SerializeField] GameObject babySlime;
    float Movecooldown;
    protected override void Start()
    {
        base.Start();
        Movecooldown = -0.5f;
        entityState = EntityState.MOVING;
    }
    protected override void Update()
    {
        base.Update();
    }
    IEnumerator hitevent(SpriteRenderer sprite)
    {
        sprite.color = new Color(0.5f, 0.5f, 0.5f);
        yield return new WaitForSeconds(0.7f);
        sprite.color = new Color(1, 1, 1);
    }

    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(realAttack());
    }
    IEnumerator realAttack()
    {
        RaycastHit2D[] rayhit;
        yield return new WaitForSeconds(0.9f);
        if (player.transform.position.x > transform.position.x)
        {
            rayhit = Physics2D.RaycastAll(transform.position, Vector3.right, range);
        }
        else
        {
            rayhit = Physics2D.RaycastAll(transform.position, Vector3.left, range);
        }
        Debug.DrawRay(transform.position, Vector3.right * range);
        foreach (var hit in rayhit)
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                Debug.Log("플레이어 공격");
                hit.collider.gameObject.GetComponent<Entity>()._hp -= Damage;
            }
            else
            {
                entityState = EntityState.MOVING;
            }
        }
    }
    protected override void Move()
    {
        base.Move();
    }
    //소환 관련 함수 애니매이션
    void Summon()
    {
        GetComponent<Rigidbody2D>().gravityScale = 0;
        GetComponent<Collider2D>().isTrigger = true;
        GameObject baby = Instantiate(babySlime, transform.position, Quaternion.identity);
        Entity BabySlime = baby.GetComponent<Entity>();
        BabySlime.MaxHp = 80;
    }
    void summonEnd()
    {
        GetComponent<Rigidbody2D>().gravityScale = 1;
        GetComponent<Collider2D>().isTrigger = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && entityState != EntityState.ONDAMAGE)
        {
            collision.gameObject.GetComponent<Entity>()._hp -= Damage;
        }
    }
    protected override void Die()
    {
        base.Die();
        Destroy(gameObject);
    }
    protected override void Hit()
    {
        base.Hit();
        SpriteRenderer sprite = gameObject.GetComponent<SpriteRenderer>();
        StartCoroutine(hitevent(sprite));
    }

}
