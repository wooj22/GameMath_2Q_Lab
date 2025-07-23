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
    public void Attack3() => StartCoroutine(HermiteFanVolley());

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

        // 색상 랜덤
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

    // Attack 3 : Hermite Curve
    IEnumerator HermiteFanVolley()
    {
        int repeatCount = 3;                 // 몇 번 반복할지
        int projectileCount = 7;            // 한번에 몇 발 쏠지
        float duration = 2f;                // 탄환 하나의 지속시간
        float spreadAngle = 60f;            // 퍼짐 각도 증가시켜 더 멋지게
        float delayBetweenVolleys = 0.4f;   // volley 간 간격

        for (int repeat = 0; repeat < repeatCount; repeat++)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                float t = (float)i / (projectileCount - 1);
                float angleOffset = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);

                Quaternion rotation = Quaternion.Euler(0, 0, angleOffset);
                Vector3 dir = rotation * -transform.up;

                Vector3 p0 = (i % 2 == 0 ? firePoint_L.position : firePoint_R.position);
                Vector3 p1 = p0 + dir * Random.Range(8f, 12f); // 도착점 거리도 살짝 랜덤

                // Hermite 탄젠트 벡터를 더 화려하게
                Vector3 randomTangentOffset = new Vector3(
                    Random.Range(-2f, 2f),
                    Random.Range(-2f, 2f),
                    0f
                );
                Vector3 m0 = dir * Random.Range(3f, 6f) + randomTangentOffset;
                Vector3 m1 = -dir * Random.Range(2f, 4f) + randomTangentOffset;

                GameObject bullet = Instantiate(bullet3, p0, Quaternion.identity);
                StartCoroutine(MoveHermiteAndRotate(bullet.transform, p0, p1, m0, m1, duration));
            }

            yield return new WaitForSeconds(delayBetweenVolleys);
        }
    }

    IEnumerator MoveHermiteAndRotate(Transform bullet, Vector3 p0, Vector3 p1, Vector3 m0, Vector3 m1, float duration)
    {
        float elapsed = 0f;

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

            // 방향 회전
            if (t < 0.99f)
            {
                float dt = 0.01f;
                float tNext = Mathf.Min(t + dt, 1f);
                float tNext2 = tNext * tNext;
                float tNext3 = tNext2 * tNext;

                Vector3 nextPos =
                    (2 * tNext3 - 3 * tNext2 + 1) * p0 +
                    (tNext3 - 2 * tNext2 + tNext) * m0 +
                    (-2 * tNext3 + 3 * tNext2) * p1 +
                    (tNext3 - tNext2) * m1;

                Vector3 dir = (nextPos - pos).normalized;
                bullet.up = dir;
            }

            bullet.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(bullet.gameObject);
    }

}

