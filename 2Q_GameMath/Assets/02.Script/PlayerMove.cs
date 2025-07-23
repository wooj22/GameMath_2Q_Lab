using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // stat
    public float speed = 5f;

    private Vector3 moveDirection;
    private bool isMoving = false;

    // component
    private Rigidbody2D rb;
    private Camera cam;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void Update()
    {
        FollowMouse();
        ClampPosition();
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
            transform.position += moveDir * speed * Time.deltaTime;
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
}
