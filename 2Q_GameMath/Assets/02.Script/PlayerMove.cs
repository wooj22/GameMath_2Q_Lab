using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // stat
    [SerializeField] private float curSpeed;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float speedBoost = 12f;
    [SerializeField] private float boostTime = 2f;
    [SerializeField] private float boostCoolTime = 5;

    // contol
    private float boostTimeDelta = 0;

    // key
    [SerializeField] private KeyCode boostKey;

    // component
    private Rigidbody2D rb;
    private Camera cam;

    private void Start()
    {
        curSpeed = moveSpeed;
        boostTimeDelta = boostCoolTime;
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        // ui
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
            curSpeed = speedBoost;
            boostTimeDelta = 0;

            Invoke(nameof(RevertSpeed), boostTime);
        }
    }

    void RevertSpeed()
    {
        curSpeed = moveSpeed;
    }
}
