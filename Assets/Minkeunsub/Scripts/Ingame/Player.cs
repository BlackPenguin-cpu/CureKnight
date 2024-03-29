using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerAttackState
{
    SHOCKWAVE,
    ELECTRONIC,
}

public enum PlayerState
{
    Idle,
    Attack,
    Skill,
    Dash,
    OnDamaged,
    Die,
}

public class Player : Entity
{
    CameraController mainCamera;

    PlayerAttackState playerSkill;
    public PlayerState playerState;
    Animator anim;
    Rigidbody2D RB;
    SpriteRenderer SR;
    bool attackAble = true;

    public GameObject[] attackCollider = new GameObject[3];
    public GameObject shockWaveCollider;

    public ParticleSystem shockWave;
    public float shockWaveDelay;
    float shockCurDelay;

    bool isMoving;
    bool waiting;
    bool gameoverChk;

    public float DashForce;
    public float StartDashTimer;

    float CurrentDashTimer;
    float DashDirection;
    float lifeTime;

    public int direction = 1;

    bool isDashing;
    bool isGround = true;
    bool isCharged = false;

    public int maxFenceCount;
    public int curFenceCount;

    [Header("Fence")]
    public Transform FenceTransform;
    public GameObject FencePrefab;
    public float fenceDelay;
    float fenceCur;
    bool fenceAble;

    [Header("Particles")]
    public ParticleSystem hitEffect;
    public ParticleSystem enemySlashLeft;
    public ParticleSystem enemySlashRight;
    public ParticleSystem shockWaveCharge;
    public ParticleSystem deadParticle;
    public ParticleSystem dashParticle;
    public ParticleSystem fenceParticle;

    [Header("UI Object")]
    public Image barrier_gauge;
    public Text barrier_count;

    MonsterWave monster;

    public int killCount;

    protected override void Die()
    {
        if (!gameoverChk)
        {
            deadParticle.Play();
            SoundManager.Instance.PlaySound("����");
        }
        playerState = PlayerState.Die;
        gameoverChk = true;
    }

    protected override void Start()
    {
        base.Start();

        GameManager.Instance.IsGameOver = false;

        shockWave.Stop();
        hitEffect.Stop();
        deadParticle.Stop();
        dashParticle.Stop();
        fenceParticle.Stop();
        enemySlashLeft.Stop();
        enemySlashRight.Stop();
        shockWaveCharge.Stop();

        _hp = MaxHp;
        fenceCur = fenceDelay;
        curFenceCount = maxFenceCount;
        shockCurDelay = shockWaveDelay;
        MaxHp = GameManager.Instance.PlayerHp;

        anim = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        SR = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main.GetComponent<CameraController>();
        monster = FindObjectOfType(typeof(MonsterWave)) as MonsterWave;

        shockWaveCollider.SetActive(false);

        foreach (var item in attackCollider)
        {
            item.SetActive(false);
        }
    }

    void UIController()
    {
        barrier_count.text = curFenceCount.ToString();
        barrier_gauge.fillAmount = 1 - fenceCur / fenceDelay;
    }

    protected override void Update()
    {
        base.Update();
        anim.SetBool("IsGround", isGround);
        InGameUIManager.Instance.SetValue(monster.WaveLevel, lifeTime, _hp, shockWaveDelay, shockCurDelay, killCount);
        anim.SetInteger("PlayerState", (int)playerState);
        UIController();
        if (playerState != PlayerState.Die)
        {
            fenceCur += Time.deltaTime;
            lifeTime += Time.deltaTime;
            anim.SetBool("IsMove", isMoving);
            shockCurDelay += Time.deltaTime;
            hitEffect.transform.position = transform.position;

            if (Input.GetKeyDown(KeyCode.A) && shockCurDelay >= shockWaveDelay && !waiting && playerState != PlayerState.Attack)
            {
                playerState = PlayerState.Skill;
                playerSkill = PlayerAttackState.SHOCKWAVE;
                entityState = EntityState.IDLE;
            }
            if (shockCurDelay >= shockWaveDelay && isCharged)
            {
                shockWaveCharge.Play();
                isCharged = false;
            }
        }
        if (entityState == EntityState.DIE)
        {
            Debug.Log("Dead");
            playerState = PlayerState.Die;
        }
        if (!gameoverChk && playerState == PlayerState.Die)
        {
            InGameUIManager.Instance.SetValue(monster.WaveLevel, lifeTime, _hp, shockWaveDelay, shockCurDelay, killCount);
            StartCoroutine(PlayerDeadEffect());
            gameoverChk = true;
        }

        switch (playerState)
        {
            case PlayerState.Idle:
                IdleController();
                break;
            case PlayerState.Attack:
                if (isGround)
                    PlayerAttack();
                break;
            case PlayerState.Skill:
                switch (playerSkill)
                {
                    case PlayerAttackState.SHOCKWAVE:
                        if (shockCurDelay >= shockWaveDelay)
                        {
                            SoundManager.Instance.PlaySound("�ĵ�");
                            ShockWaveAnimation();
                            shockCurDelay = 0;
                            isCharged = true;
                        }
                        break;
                    case PlayerAttackState.ELECTRONIC:
                        break;
                }
                break;
            case PlayerState.OnDamaged:
                SR.color = new Color(0.5f, 0.5f, 0.5f);
                break;
            case PlayerState.Die:
                GameManager.Instance.IsGameOver = true;
                break;
            case PlayerState.Dash:
                Dash();
                break;
            default:
                break;
        }
    }

    void FenceController()
    {
        if (curFenceCount > maxFenceCount) curFenceCount = maxFenceCount;
        if (Input.GetKeyDown(KeyCode.S) && fenceCur >= fenceDelay && curFenceCount > 0)
        {
            SoundManager.Instance.PlaySound("�ҽ�");
            Quaternion temp_rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            Instantiate(FencePrefab, FenceTransform.position, temp_rotation);
            fenceParticle.transform.position = FenceTransform.position;
            fenceParticle.Play();
            fenceCur = 0;
            curFenceCount--;
        }
    }

    IEnumerator PlayerDeadEffect()
    {
        yield return new WaitForSeconds(3);
        InGameUIManager.Instance.GameOver();
    }

    public void DieAnimationEnd()
    {
        gameoverChk = true;
    }

    void Dash()
    {
        if (isDashing)
        {
            RB.velocity = transform.right * DashForce;

            CurrentDashTimer -= Time.deltaTime;


            if (CurrentDashTimer <= 0)
            {
                isDashing = false;
                playerState = PlayerState.Idle;
                RB.velocity = Vector2.zero;
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerState != PlayerState.Die && playerState != PlayerState.Skill)
            PlayerMove();
    }

    void ShockWaveAnimation()
    {
        if (attackAble)
        {
            attackAble = false;
            anim.SetInteger("AttackIndex", 3); // 3 == shockwave attack
            shockCurDelay = 0;
        }
    }

    public void ShockWaveAttackStart()
    {
        shockWave.Play();
        mainCamera.ShakeForTime(0.5f);
        shockWaveCollider.SetActive(true);
    }

    public void ShockWaveAttackEnd()
    {
        playerState = PlayerState.Idle;
        shockWaveCollider.SetActive(false);
        attackAble = true;
        isMoving = true;
    }

    void PlayerAttack()
    {
        if (attackAble)
        {
            int attack_index = Random.Range(0, 3);
            switch (attack_index)
            {
                case 0:
                    SoundManager.Instance.PlaySound("�ֵθ���1");
                    break;
                case 1:
                    SoundManager.Instance.PlaySound("�ֵθ���2");
                    break;
                case 2:
                    SoundManager.Instance.PlaySound("�ֵθ���3");
                    break;
                default:
                    break;
            }
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
        playerState = PlayerState.Idle;
        foreach (var item in attackCollider)
        {
            item.SetActive(false);
        }
        ShockWaveAttackEnd();
    }

    void IdleController()
    {
        FenceController();
        attackAble = true;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            isMoving = true;
        }
        else isMoving = false;
        if (Input.GetKeyDown(KeyCode.Z))
        {
            playerState = PlayerState.Attack;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SoundManager.Instance.PlaySound("�뽬");
            playerState = PlayerState.Dash;
            isDashing = true;
            CurrentDashTimer = StartDashTimer;
            RB.velocity = Vector2.zero;
            CurrentDashTimer -= Time.deltaTime;

            int i_temp = direction == 1 ? 1 : 0;
            dashParticle.GetComponent<ParticleSystemRenderer>().flip = new Vector3(i_temp, 0, 0);
            dashParticle.Play();
        }
    }

    void PlayerMove()
    {
        if (!isDashing && playerState != PlayerState.OnDamaged)
        {
            if (Input.GetKey(KeyCode.LeftArrow) && playerState != PlayerState.Attack)
            {
                transform.Translate(Vector3.right * Speed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 180, 0);
                direction = 1;
                playerState = PlayerState.Idle;
            }
            else if (Input.GetKey(KeyCode.RightArrow) && playerState != PlayerState.Attack)
            {
                transform.Translate(Vector3.right * Speed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, 0);
                direction = -1;
                playerState = PlayerState.Idle;
            }
        }
    }

    protected override void Hit()
    {
        if (playerState != PlayerState.Die && playerState != PlayerState.Skill)
        {
            SoundManager.Instance.PlaySound("�÷��̾��ǰ�");
            mainCamera.ShakeForTime(0.3f);
            hitEffect.Play();
            playerState = PlayerState.OnDamaged;
            anim.SetInteger("PlayerState", (int)playerState);
            isMoving = false;
            anim.SetBool("IsMove", isMoving);
            Stop(0.25f);
        }

    }

    public void Stop(float duration)
    {
        if (waiting)
            return;
        StartCoroutine(Wait(duration));
    }

    IEnumerator Wait(float duration)
    {
        isDashing = false;
        waiting = true;
        yield return new WaitForSeconds(0.05f);
        Time.timeScale = 0.1f;

        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
        foreach (var item in attackCollider)
        {
            item.SetActive(false);
        }
        RB.velocity = new Vector2(5 * direction, 5);
        waiting = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (playerState == PlayerState.OnDamaged)
            {
                RB.velocity = Vector2.zero;
                playerState = PlayerState.Idle;
            }
            SR.color = new Color(1, 1, 1);
            isGround = true;
            if (playerState == PlayerState.Die)
            {
                gameoverChk = false;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGround = false;
        }
    }

    public ParticleSystem Attack()
    {
        ParticleSystem slashParticle = direction == 1 ? enemySlashLeft : enemySlashRight;
        slashParticle.Stop();
        slashParticle.Play();
        return slashParticle;
    }
}
