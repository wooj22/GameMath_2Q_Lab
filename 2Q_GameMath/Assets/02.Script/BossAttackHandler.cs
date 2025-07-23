using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackHandler : MonoBehaviour
{
    // asset
    [SerializeField] private GameObject bullet1;        // circle bullet
    [SerializeField] private GameObject bullet2;        // diamond bullet
    [SerializeField] private GameObject bullet3;        // arrow bullet
    [SerializeField] private Transform firePoint_L;
    [SerializeField] private Transform firePoint_R;

    // player
    private GameObject player;

    private void Start()
    {
        player = GameObject.Find("Player");
    }

    public void Attack1() => StartCoroutine(CubicBezierDoubleSpread());
    public void Attack2() => StartCoroutine(QuadraticSpread());
    public void Attack3() => StartCoroutine(HermiteAOE());

    // Attack 1
    IEnumerator CubicBezierDoubleSpread()
    {
        int burstCount = 5;
        float fireRate = 0.2f;
        float duration = 1.5f;
        float spreadAmount = 2.5f;

        Transform[] hands = new Transform[] { firePoint_L, firePoint_R, firePoint_L, firePoint_R };

        foreach (var hand in hands)
        {
            for (int i = 0; i < burstCount; i++)
            {
                Vector3 curveOffset = hand.right * Random.Range(-spreadAmount, spreadAmount);
                Vector3 targetPos = player.transform.position;
                StartCoroutine(CubicMoveBullet(hand.position, targetPos, curveOffset, duration));
                yield return new WaitForSeconds(fireRate);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator CubicMoveBullet(Vector3 start, Vector3 end, Vector3 curveOffset, float duration)
    {
        GameObject bullet = Instantiate(bullet1, start, Quaternion.identity);

        Vector3 control1 = start + transform.up * 2f + curveOffset;
        Vector3 control2 = end + transform.up * -1f + curveOffset;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;

            Vector3 pos =
                Mathf.Pow(1 - t, 3) * start +
                3 * Mathf.Pow(1 - t, 2) * t * control1 +
                3 * (1 - t) * t * t * control2 +
                t * t * t * end;

            bullet.transform.position = pos;

            if (t < 0.99f)
            {
                float tNext = Mathf.Min(t + 0.01f, 1f);
                Vector3 nextPos =
                    Mathf.Pow(1 - tNext, 3) * start +
                    3 * Mathf.Pow(1 - tNext, 2) * tNext * control1 +
                    3 * (1 - tNext) * Mathf.Pow(tNext, 2) * control2 +
                    Mathf.Pow(tNext, 3) * end;

                Vector3 dir = (nextPos - pos).normalized;
                bullet.transform.up = dir;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(bullet);
    }

    // 🔸 Attack 2: Quadratic Bézier - 부채꼴 곡선 탄막 (여러 발)
    IEnumerator QuadraticSpread()
    {
        int shotCount = 10;
        float interval = 0.08f;
        float spreadOffset = 1.5f;
        float duration = 3f;

        for (int i = 0; i < shotCount; i++)
        {
            // 교차 발사: 왼손과 오른손 번갈아 사용
            Transform firePoint = (i % 2 == 0) ? firePoint_L : firePoint_R;
            Vector3 start = firePoint.position;

            // 사선 방향으로 퍼지도록 곡선 설정
            float xOffset = ((i % 5) - 2) * spreadOffset;
            Vector3 control = start + transform.up * 3f + transform.right * xOffset;
            Vector3 end = start + transform.up * 7f + transform.right * xOffset * 1.2f;

            GameObject bullet = Instantiate(bullet2, start, Quaternion.identity);
            StartCoroutine(MoveQuadratic(bullet.transform, start, control, end, duration));

            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator MoveQuadratic(Transform bullet, Vector3 start, Vector3 control, Vector3 end, float duration)
    {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Vector3 pos =
                    Mathf.Pow(1 - t, 2) * start +
                    2 * (1 - t) * t * control +
                    t * t * end;

                bullet.position = pos;
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(bullet.gameObject);
        }

    // 🔸 Attack 3: Hermite Curve - AOE 예고 + 돌진 (직선 느낌)
    IEnumerator HermiteAOE()
    {
        Vector3 p0 = firePoint_L.position;
        Vector3 p1 = player.transform.position + Vector3.down * 1f;

        Vector3 m0 = transform.up * 5f;  // 초기 방향성
        Vector3 m1 = Vector3.zero;

        float duration = 1.5f;
        float elapsed = 0f;

        GameObject marker = Instantiate(bullet3, p0, Quaternion.identity);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float t2 = t * t;
            float t3 = t2 * t;

            Vector3 pos =
                (2 * t3 - 3 * t2 + 1) * p0 +
                (t3 - 2 * t2 + t) * m0 +
                (-2 * t3 + 3 * t2) * p1 +
                (t3 - t2) * m1;

            marker.transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(marker);
        // 여기에 폭발 이펙트 또는 충돌 처리 가능
    }
}

