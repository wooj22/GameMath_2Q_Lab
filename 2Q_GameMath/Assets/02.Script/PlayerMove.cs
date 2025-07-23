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

    // 마우스를 바라보며 이동
    void FollowMouse()
    {
        // 마우스까지의 방향
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3 dir = (mouseWorldPos - transform.position);
        float distance = dir.magnitude;

        // 회전
        if (dir.sqrMagnitude > 0.001f)
            transform.up = dir;

        // 이동
        if (distance > 0.5f)
        {
            Vector3 moveDir = dir.normalized;
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
