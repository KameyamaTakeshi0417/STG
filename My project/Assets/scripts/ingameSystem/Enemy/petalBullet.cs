using System.Collections;
using UnityEngine;

public class petalBullet : MonoBehaviour
{
    public int cycleCount;
    public int cycleCountMax;
    public float bulletSpeedMag;
    public int involuteTimeMax = 1000;
    public float involuteRadius = 1.0f;
    public float damage;

    void Awake()
    {
        cycleCount = 0;
    }

    void Update()
    {
        if (GameObject.Find("GameManager").GetComponent<GameManager>().getCleared())
        {
            Destroy(this.gameObject);
        }
    }

    public void shoot(int shootWay)
    {
        StartCoroutine("spread", getShootWayAsClock(shootWay));
    }

    public Vector3 getShootWayAsClock(int num)
    {
        Vector3 ret = new Vector3(0, 0, 0);
        switch (num)
        {
            case 0:
            case 12:
                ret = new Vector3(0, 1, 0);
                break;
            case 1:
                ret = new Vector3(0.5f, 0.866f, 0);
                break;
            case 2:
                ret = new Vector3(0.866f, 0.5f, 0);
                break;
            case 3:
                ret = new Vector3(1, 0, 0);
                break;
            case 4:
                ret = new Vector3(0.866f, -0.5f, 0);
                break;
            case 5:
                ret = new Vector3(0.5f, -0.866f, 0);
                break;
            case 6:
                ret = new Vector3(0, -1, 0);
                break;
            case 7:
                ret = new Vector3(-0.5f, -0.866f, 0);
                break;
            case 8:
                ret = new Vector3(-0.866f, -0.5f, 0);
                break;
            case 9:
                ret = new Vector3(-1, 0, 0);
                break;
            case 10:
                ret = new Vector3(-0.866f, 0.5f, 0);
                break;
            case 11:
                ret = new Vector3(-0.5f, 0.866f, 0);
                break;
            default:
                ret = new Vector3(0, -1, 0);
                break;
        }
        return ret;
    }

    public void ShootInvolute(int wayClock, bool rotationWay)
    {
        Vector3 target;
        target = getShootWayAsClock(wayClock);
        Vector2 ret = new Vector2(target.x, target.y);
        float checker = 1;
        if (!rotationWay)
        {
            checker = -1;
        }

        StartCoroutine(Involute(ret, checker));
    }

    private IEnumerator Involute(Vector2 startPos, float rotationWay)
    {
        float speed = bulletSpeedMag * 1000.0f;
        float angle = 0.0f;
        int count = 0;
        Vector2 collectorPos;
        collectorPos =
            GameObject.Find("PetalCollector").transform.position + ((Vector3)startPos * 2.0f);

        while (count < involuteTimeMax)
        {
            Vector2 involutePosition = CalculateInvolutePosition(involuteRadius, angle);
            transform.position = collectorPos + involutePosition;

            // 回転をインボリュートの接線方向に設定
            float rotationAngle =
                Mathf.Atan2(involutePosition.y, involutePosition.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(
                new Vector3(0, 0, rotationAngle + 270 * rotationWay)
            );

            angle += (speed * Time.deltaTime) * rotationWay;

            count++;
            yield return new WaitForEndOfFrame();
        }
        yield return homing();
    }

    private Vector2 CalculateInvolutePosition(float radius, float theta)
    {
        float x = radius * (Mathf.Cos(theta) + theta * Mathf.Sin(theta));
        float y = radius * (Mathf.Sin(theta) - theta * Mathf.Cos(theta));
        return new Vector2(x, y);
    }

    private IEnumerator spread(Vector3 way)
    {
        int count = 0;
        int countClock = 600;

        while (true)
        {
            count++;
            if (count >= countClock)
            {
                yield return homing();
            }

            transform.position += way * bulletSpeedMag;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator homing()
    {
        int count = 0;
        int countClock = 1000;

        while (true)
        {
            Vector3 moveWay = GameObject.Find("Player").transform.position - transform.position;
            moveWay.Normalize();

            // プレイヤーに向けてオブジェクトの向きを変更
            float rotationAngle = Mathf.Atan2(moveWay.y, moveWay.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationAngle + 90));

            transform.position += moveWay * (bulletSpeedMag * 2.0f);
            count++;

            if (count >= countClock)
            {
                yield return move();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator move()
    {
        int count = 0;
        int countClock = 100;

        Vector3 moveWay = GameObject.Find("Player").transform.position - transform.position;
        moveWay.Normalize();

        while (true)
        {
            transform.position += moveWay * bulletSpeedMag;
            count++;

            if (count >= countClock)
            {
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        cycleCount++;
        if (cycleCount > cycleCountMax)
        {
            Destroy(this.gameObject);
        }
        yield return homing();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // HPを持つコンポーネントを取得
            _Health_Base health = collision.GetComponent<_Health_Base>();
            if (health != null)
            {
                // HPを減らす
                health.TakeDamage(damage);
            }
            Destroy(this.gameObject);
        }
    }
}
