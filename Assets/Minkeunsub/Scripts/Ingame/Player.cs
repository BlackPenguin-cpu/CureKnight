using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAttackState
{
    SHOCKWAVE,
    ELECTRONIC,
    COMMONATTACK
}

public class Player : Entity
{
    PlayerAttackState playerSkill;
    Animator anim;
    bool attackAble = true;

    public GameObject[] attackCollider = new GameObject[3];
    public GameObject shockWaveCollider;

    public ParticleSystem shockWave;
    public float shockWaveDelay;
    float shockCurDelay;

    float StunSpeed;
    bool restoreTime;
    Coroutine timeDelayCoroutine;

    protected override void Die()
    {
    }

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        shockWave.Stop();
        shockWaveCollider.SetActive(false);
        restoreTime = false;
        foreach (var item in attackCollider)
        {
            item.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();
        anim.SetInteger("PlayerState", (int)entityState);
        shockCurDelay += Time.deltaTime;
        shockWaveCollider.SetActive(false);
        switch (entityState)
        {
            case EntityState.IDLE:
                IdleController();
                break;
            case EntityState.MOVING:
                break;
            case EntityState.ONDAMAGE:
                break;
            case EntityState.ATTACK:
                switch (playerSkill)
                {
                    case PlayerAttackState.SHOCKWAVE:
                        ShockWaveAttack();
                        break;
                    case PlayerAttackState.ELECTRONIC:
                        break;
                    case PlayerAttackState.COMMONATTACK:
                        PlayerAttack();
                        break;
                    default:
                        break;
                }
                break;
            case EntityState.DIE:
                break;
            default:
                break;
        }
    }

    void OnDamagedTime()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.deltaTime * Speed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }

    private void FixedUpdate()
    {
        switch (entityState)
        {
            case EntityState.IDLE:
                break;
            case EntityState.MOVING:
                IdleController();
                PlayerMove();
                break;
            case EntityState.ONDAMAGE:
                break;
            case EntityState.ATTACK:
                break;
            case EntityState.DIE:
                break;
            default:
                break;
        }
    }

    void ShockWaveAttack()
    {
        if (attackAble && shockCurDelay >= shockWaveDelay)
        {
            shockWave.Play();
            shockWaveCollider.SetActive(true);
            attackAble = false;
            anim.SetInteger("AttackIndex", 3); // 3 == shockwave attack
            entityState = EntityState.IDLE;
            shockCurDelay = 0;
        }
    }

    void PlayerAttack()
    {
        if (attackAble)
        {
            int attack_index = Random.Range(0, 3);
            anim.SetInteger("AttackIndex", attack_index);
            attackAble = false;
        }
    }

    public void AttackColliderActive(int n)
    {
        attackCollider[n].SetActive(true);
    }

    public void EndAttack()
    {
        entityState = EntityState.IDLE;
        foreach (var item in attackCollider)
        {
            item.SetActive(false);
        }
    }

    void IdleController()
    {
        attackAble = true;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            entityState = EntityState.MOVING;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            entityState = EntityState.ATTACK;
            playerSkill = PlayerAttackState.COMMONATTACK;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            entityState = EntityState.ATTACK;
            playerSkill = PlayerAttackState.SHOCKWAVE;
        }
    }

    void PlayerMove()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector3.right * Speed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(Vector3.right * Speed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            entityState = EntityState.IDLE;
        }
    }

    protected override void Hit()
    {
        StopTime(0.05f, 10, 0.1f);
    }

    public void StopTime(float changeTime, int RestorSpeed, float Delay)
    {
        Speed = RestorSpeed;
        if (Delay > 0)
        {
            StopCoroutine(timeDelayCoroutine);
            timeDelayCoroutine = StartCoroutine(StartTimeAgain(Delay));
        }
        else
        {
            restoreTime = true;
        }
        Time.timeScale = changeTime;
    }

    IEnumerator StartTimeAgain(float amt)
    {
        restoreTime = true;
        yield return new WaitForSecondsRealtime(amt);
    }
}
