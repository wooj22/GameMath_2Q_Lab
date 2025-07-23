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
    [SerializeField] private Transform[] controlPoints;

    // player
    private GameObject player;

    private void Start()
    {
        player = GameObject.Find("Player");
    }

    public void Attack1() => StartCoroutine(CubicBezierDoubleSpread());
    public void Attack2() => CubicBezierDoubleStream();
    public void Attack3() => StartCoroutine(SpawnMultipleSpear());

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

    // Attack 3
    IEnumerator SpawnMultipleSpear()
    {
        int spearCount = 6;        // 6개 생성
        float spawnInterval = 1f;

        for (int i = 0; i < spearCount; i++)
        {
            Transform firePoint = (i % 2 == 0) ? firePoint_L : firePoint_R;

            GameObject bullet = Instantiate(bullet3, firePoint.position, Quaternion.identity);
            StartCoroutine(MoveAlongRandomSpline(bullet.transform, 5f)); // 10초 -> 5초로 절반 (속도 2배)

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator MoveAlongRandomSpline(Transform bullet, float totalDuration)
    {
        float elapsed = 0f;
        Vector3 currentDir = bullet.up;

        Vector3[] points = new Vector3[controlPoints.Length];
        for (int i = 0; i < controlPoints.Length; i++)
            points[i] = controlPoints[i].position;

        ShuffleArray(points);

        while (elapsed < totalDuration)
        {
            float t = elapsed / totalDuration;

            float scaledT = t * (points.Length - 1);
            int segment = Mathf.Min(Mathf.FloorToInt(scaledT), points.Length - 2);
            float segmentT = scaledT - segment;

            Vector3 p0 = points[Mathf.Clamp(segment - 1, 0, points.Length - 1)];
            Vector3 p1 = points[segment];
            Vector3 p2 = points[segment + 1];
            Vector3 p3 = points[Mathf.Clamp(segment + 2, 0, points.Length - 1)];

            Vector3 pos = CatmullRom(p0, p1, p2, p3, segmentT);
            bullet.position = pos;

            Vector3 tangent = CatmullRomTangent(p0, p1, p2, p3, segmentT).normalized;
            currentDir = Vector3.Slerp(currentDir, tangent, Time.deltaTime * 5f);
            bullet.up = currentDir;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(bullet.gameObject);
    }

    void ShuffleArray(Vector3[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector3 temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    // Catmull-Rom spline 계산 함수
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * ((2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    // Catmull-Rom 접선 계산 함수
    Vector3 CatmullRomTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;

        return 0.5f * ((-p0 + p2) +
            2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t +
            3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t2);
    }
}

