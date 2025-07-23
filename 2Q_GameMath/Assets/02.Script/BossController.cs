using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    // stat
    [SerializeField] private int hp;
    [SerializeField] private int maxhp = 10000;

    // player
    private GameObject player;
    private Transform playerPos;

    void Start()
    {
        hp = maxhp;
        player = GameObject.Find("Player");
        playerPos = player.transform;

        // ui
        UIManager.Instance.SetBossMaxHp(maxhp);
        UIManager.Instance.UpdateBossHp(hp);
    }

    void Update()
    {
        LookPlayer();
    }

    void LookPlayer()
    {
        if (playerPos == null)
            return;

        Vector3 dir = (playerPos.position - transform.position).normalized;

        if (dir.sqrMagnitude > 0.001f)
            transform.up = -dir;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp < 0) hp = 0;
        UIManager.Instance.UpdateBossHp(hp);
    }
}
