using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voltpropagation_Effect : MonoBehaviour
{
    // Start is called before the first frame update
    public int IterationCount = 1; // 繰り返し回数
    private float dmg = 30f;
    private int voltTime = 50;
    public Queue<GameObject> hitQueue = new Queue<GameObject>(); // 当たったオブジェクトを格納するキュー

    void OnTriggerEnter2D(Collider2D collision)
    {
        // キューに存在していないオブジェクトのみ追加し、IterationCountが0でない場合に処理を行う
        if (collision.CompareTag("Enemy") && (!hitQueue.Contains(collision.gameObject) && IterationCount > 0))
        {
            hitQueue.Enqueue(collision.gameObject); // キューにオブジェクトを追加
            IterationCount--; // IterationCountを減少

            // CreateVolt関数を呼び出し
            GameObject voltPrefab = Instantiate(Resources.Load<GameObject>("Objects/Effect_Volt"), collision.transform.position, Quaternion.identity);

            voltPrefab.GetComponent<Effect_Volt>().startVolt(dmg, voltTime, IterationCount - 1, hitQueue);
        }
    }

    // CreateVolt関数
    public void CreateVolt(Queue<GameObject> queue, int remainingIterations)
    {
        hitQueue = queue;
        IterationCount = remainingIterations;
        if (remainingIterations > 0)
        {
            // キュー内のオブジェクトに対して何らかの処理を行う
            foreach (GameObject obj in queue)
            {
                Debug.Log("Processing object: " + obj.name);
                // ここに具体的なエフェクトや処理を追加する
            }
        }
        else
        {
            Debug.Log("Iteration complete or reached limit.");
        }
    }
}
