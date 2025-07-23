using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    [SerializeField] private int damage = 100;

    private void Start()
    {
        // 마우스 방향
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3 dir = (mouseWorldPos - transform.position).normalized;
        transform.up = dir;
    }

    void Update()
    {
        this.transform.Translate(transform.up * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Boss"))
        {
            collision.gameObject.GetComponent<BossController>().TakeDamage(damage);
            Destroy(this.gameObject);
        }
    }
}
