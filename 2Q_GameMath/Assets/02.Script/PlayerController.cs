using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // stat
    [SerializeField] private int hp;
    [SerializeField] private int maxhp = 100;

    [SerializeField] private float curSpeed;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float speedBoost = 12f;
    [SerializeField] private float boostTime = 2f;
    [SerializeField] private float boostCoolTime = 5;

    [SerializeField] private float shootCoolTime = 0.05f;

    // contol
    private float boostTimeDelta = 0;
    private float shootCoolTimeDelta = 0;
    private bool isHit;
    private bool isBoost;
    private bool isMoveStop;

    // asset
    [SerializeField] private SpriteRenderer sr1;
    [SerializeField] private SpriteRenderer sr2;
    [SerializeField] private GameObject bullet;

    // key
    [SerializeField] private KeyCode boostKey;

    // component
    private Rigidbody2D rb;
    private Camera cam;

    private void Start()
    {
        hp = maxhp;
        curSpeed = moveSpeed;
        boostTimeDelta = boostCoolTime;
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        // ui
        UIManager.Instance.SetPlayerMaxHp(maxhp);
        UIManager.Instance.UpdatePlayerHp(hp);

        UIManager.Instance.SetBoostCoolTime(boostCoolTime);
        UIManager.Instance.UpdatePlayerBoost(boostTimeDelta);
    }

    void Update()
    {
        boostTimeDelta += Time.deltaTime;
        shootCoolTimeDelta += Time.deltaTime;

        Attack();
        FollowMouse();
        Boost();
        ClampPosition();

        // test
        if (Input.GetMouseButtonDown(0))
        {
            GameObject.Find("Boss").GetComponent<BossController>().TakeDamage(100);
        }

        // ui
        UIManager.Instance.UpdatePlayerBoost(boostTimeDelta);
    }

    // 마우스를 바라보며 이동
    void FollowMouse()
    {
        // 마우스까지의 방향
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3 mouseDir = (mouseWorldPos - transform.position);
        float distance = mouseDir.magnitude;

        // 회전
        if (mouseDir.sqrMagnitude > 0.001f)
            transform.up = mouseDir;

        // 이동
        if (distance > 0.5f && !isMoveStop)
        {
            Vector3 moveDir = mouseDir.normalized;
            transform.position += moveDir * curSpeed * Time.deltaTime;
        }
    }

    // 화면 경계 제어
    void ClampPosition()
    {
        Camera cam = Camera.main;

        // 화면의 좌하단(0,0)과 우상단(1,1)을 월드 좌표로 변환
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        // 현재 위치를 min~max 사이로 클램핑
        Vector3 clamped = transform.position;
        clamped.x = Mathf.Clamp(clamped.x, min.x, max.x);
        clamped.y = Mathf.Clamp(clamped.y, min.y, max.y);

        transform.position = clamped;
    }

    // Attack
    void Attack()
    {
        if (Input.GetMouseButton(0))
        {
            isMoveStop = true;
            if (shootCoolTimeDelta >= shootCoolTime)
            {
                Shoot();
                shootCoolTimeDelta = 0;
            }
        }
        else
        {
            isMoveStop = false;
        }
    }

    void Shoot()
    {
        Instantiate(bullet, this.transform.position, Quaternion.identity);
    }

    // Move Boost
    void Boost()
    {
        if(Input.GetKeyDown(boostKey) && boostTimeDelta >= boostCoolTime)
        {
            isBoost = true;
            curSpeed = speedBoost;
            boostTimeDelta = 0;

            sr1.color = Color.green;
            sr2.color = Color.green;

            Invoke(nameof(RevertSpeed), boostTime);
        }
    }

    void RevertSpeed()
    {
        isBoost = false;
        curSpeed = moveSpeed;

        sr1.color = Color.gray;
        sr2.color = Color.gray;
    }

    // Hit
    public void TakeDamage(int damage)
    {
        // 부스트중 무적
        if (isBoost) return;
 
        hp -=damage;
        if (hp <= 0)
        {
            damage = 0;
            GameManager.Instance.GameOver();
            Destroy(this.gameObject);
        }

        UIManager.Instance.UpdatePlayerHp(hp);
        if (!isHit) StartCoroutine(BlinkRed());
        Debug.Log("Player Hit! 남은 체력 : " + hp);
    }

    private IEnumerator BlinkRed()
    {
        isHit = true;

        sr1.color = Color.red;
        sr2.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr1.color = Color.gray;
        sr2.color = Color.gray;

        isHit = false;
    }
}
