using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointManager : MonoBehaviour
{
    public int money;
    public int exp;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void addMoney(int mny)
    {
        money += mny;
    }

    public void createMoney(int createCount, Vector3 position)
    {
        GameObject moneyObj;
        for (int i = 0; i < createCount; i++)
        {
            moneyObj = Instantiate(
                Resources.Load<GameObject>("Objects/money"),
                position + CreateMoneyPos(),
                Quaternion.identity
            );
        }
    }

    private Vector3 CreateMoneyPos()
    {
        Vector3 ret;
        ret = new Vector3(0, 0, 0);
        float randomPos; //乱数ベクトル作るための一時的なもの
        randomPos = Random.Range(-2f, 2f);
        ret.x = randomPos;
        randomPos = Random.Range(-2f, 2f);
        ret.y = randomPos;
        ret += transform.localPosition;
        return ret;
    }
}
