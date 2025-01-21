using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteEnemyBattleManager : MonoBehaviour
{
    void Awake()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().setCleared(false);
    }
}
