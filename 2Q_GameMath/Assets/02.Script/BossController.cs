using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    // stat
    [SerializeField] private int hp;
    [SerializeField] private int maxhp = 10000;
    [SerializeField] private int attackCoolTime = 6;

    private int attackIndex = 1;        // 1, 2, 3

    // player
    private GameObject player;
    private Transform playerPos;

    // component
    BossAttackHandler attackHandler;

    void Start()
    {
        hp = maxhp;
        player = GameObject.Find("Player");
        playerPos = player.transform;
        attackHandler = GetComponent<BossAttackHandler>();

        // attack
        InvokeRepeating(nameof(Attack), 0, attackCoolTime);

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

    void Attack()
    {
        switch (attackIndex)
        {
            case 1:
                attackHandler.Attack1();
                break;
            case 2:
                attackHandler.Attack2();
                break;
            case 3:
                attackHandler.Attack3();
                break;
            default:
                break;
        }

        attackIndex++;
        if (attackIndex > 3) attackIndex = 1;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp < 0) hp = 0;
        UIManager.Instance.UpdateBossHp(hp);
    }
}
