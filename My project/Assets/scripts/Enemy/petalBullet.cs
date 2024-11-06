using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class petalBullet : MonoBehaviour
{
    public int cycleCount;
    public int cycleCountMax;
    public float bulletSpeedMag;
    // Start is called before the first frame update
    void Awake()
    {
        cycleCount = 0;
    }

    // Update is called once per frame
    void Update()
    {

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
    private IEnumerator spread(Vector3 way)
    {
        //指定された角度に展開する
        int count = 0;
        int countClock = 30;
        while (true)
        {
            count++;
            if (count >= countClock)
            {
                yield return homing();
            }
            transform.position += way*bulletSpeedMag;//可能であれば壁を認識したい。
            yield return new WaitForEndOfFrame();
        }

    }
    private IEnumerator homing()
    {
        int count = 0;
        int countClock = 300;
        //制限時間が終わるまでプレイヤーに向かって追尾する
        Vector3 moveWay = new Vector3(0, 0, 0);
        moveWay = GameObject.Find("Player").transform.position - transform.position;
        Vector3.Normalize(moveWay);
        while (true)
        {
            transform.position += (moveWay) * (bulletSpeedMag * 0.25f);
            moveWay = GameObject.Find("Player").transform.position - transform.position;
            Vector3.Normalize(moveWay);
            count++;
            if (count >= countClock)
            {
                yield return move();
            }
            yield return new WaitForEndOfFrame();
        }

    }
    private IEnumerator move()
    {//直進する。

        int count = 0;
        int countClock = 100;
        //制限時間が終わるまでプレイヤーに向かって追尾する
        Vector3 moveWay = new Vector3(0, 0, 0);
        moveWay = GameObject.Find("Player").transform.position - transform.position;
        Vector3.Normalize(moveWay);
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
            Destroy(this.gameObject);
        }

    }
}
