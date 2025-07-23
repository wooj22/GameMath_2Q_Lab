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

    public void Attack1() => StartCoroutine(CubicBezierShot());       // bullet1 사용
    public void Attack2() => StartCoroutine(QuadraticSpread());       // bullet2 사용
    public void Attack3() => StartCoroutine(HermiteAOE());            // bullet3 사용

    // 🔸 Attack 1: Cubic Bézier - 유도탄 (1발)
    IEnumerator CubicBezierShot()
    {
        Vector3 start = firePoint_L.position;
        Vector3 end = player.transform.position;

        Vector3 control1 = start + transform.up * 2f + transform.right * 3f;
        Vector3 control2 = end + Vector3.left * 2f;

        float duration = 2f;
        float elapsed = 0f;

        GameObject bullet = Instantiate(bullet1, start, Quaternion.identity);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 pos =
                Mathf.Pow(1 - t, 3) * start +
                3 * Mathf.Pow(1 - t, 2) * t * control1 +
                3 * (1 - t) * t * t * control2 +
                t * t * t * end;

            bullet.transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(bullet);
    }

    // 🔸 Attack 2: Quadratic Bézier - 부채꼴 곡선 탄막 (여러 발)
    IEnumerator QuadraticSpread()
    {
        int count = 5;
        float spacing = 1.2f;

        for (int i = -count; i <= count; i++)
        {
            Vector3 start = firePoint_R.position;
            Vector3 mid = start + transform.up * 2f + transform.right * i * spacing;
            Vector3 end = start + transform.up * 5f + transform.right * i * spacing * 1.5f;

            GameObject bullet = Instantiate(bullet2, start, Quaternion.identity);
            StartCoroutine(MoveQuadratic(bullet.transform, start, mid, end, 5f));
        }

        yield return null;
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

