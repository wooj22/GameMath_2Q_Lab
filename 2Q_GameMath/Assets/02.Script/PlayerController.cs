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

    // contol
    private float boostTimeDelta = 0;
    private bool isHit;
    private bool isBoost;

    // asset
    [SerializeField] private SpriteRenderer sr1;
    [SerializeField] private SpriteRenderer sr2;

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

    // ���콺�� �ٶ󺸸� �̵�
    void FollowMouse()
    {
        // ���콺������ ����
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3 dir = (mouseWorldPos - transform.position);
        float distance = dir.magnitude;

        // ȸ��
        if (dir.sqrMagnitude > 0.001f)
            transform.up = dir;

        // �̵�
        if (distance > 0.5f)
        {
            Vector3 moveDir = dir.normalized;
            transform.position += moveDir * curSpeed * Time.deltaTime;
        }
    }

    // ȭ�� ��� ����
    void ClampPosition()
    {
        Camera cam = Camera.main;

        // ȭ���� ���ϴ�(0,0)�� ����(1,1)�� ���� ��ǥ�� ��ȯ
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        // ���� ��ġ�� min~max ���̷� Ŭ����
        Vector3 clamped = transform.position;
        clamped.x = Mathf.Clamp(clamped.x, min.x, max.x);
        clamped.y = Mathf.Clamp(clamped.y, min.y, max.y);

        transform.position = clamped;
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
        // �ν�Ʈ�� ����
        if (isBoost) return;
 
        hp -=damage;
        if (hp < 0)
        {
            damage = 0;
            Destroy(this.gameObject);
        }

        UIManager.Instance.UpdatePlayerHp(hp);
        if (!isHit) StartCoroutine(BlinkRed());
        Debug.Log("Player Hit! ���� ü�� : " + hp);
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
