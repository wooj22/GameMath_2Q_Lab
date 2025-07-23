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
    public void Attack2() => CubicBezierDoubleStream();
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

    // Attack 2
    void CubicBezierDoubleStream()
    {
        int bulletCount = 30;
        float duration = 5f;
        float spiralScale = 10f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angleOffset = 360f / bulletCount * i;

            // 좌우 손 랜덤 사용
            Transform firePoint = (i % 2 == 0) ? firePoint_L : firePoint_R;

            GameObject bullet = Instantiate(bullet2, firePoint.position, Quaternion.identity);
            StartCoroutine(SpiralBulletMove(bullet.transform, firePoint.position, duration, angleOffset, spiralScale));
        }
    }

    IEnumerator SpiralBulletMove(Transform bullet, Vector3 origin, float duration, float angleOffset, float spiralScale)
    {
        float time = 0f;

        // 색상도 랜덤하게 줄 수 있음
        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();
        sr.color = Random.ColorHSV(0, 1, 0.8f, 1f, 1f, 1f);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // 탄환의 반경 증가 (멀어짐)
            float radius = t * spiralScale;

            // 나선 각도 회전
            float angle = (t * 720f) + angleOffset; // 한탄환 기준 각도 오프셋

            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;

            bullet.position = origin + offset;

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

