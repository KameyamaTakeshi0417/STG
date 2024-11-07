using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treasureBox : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public GameObject rewardObj;
    private bool nowOpen;
    public float checkInterval = 1.0f; // チェック間隔（秒）
    // Start is called before the first frame update
    void Start()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        nowOpen = false;

    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKey(KeyCode.Return)) && nowOpen)
        {
            developReward();
        }
    }
    public void setNowOpen(bool set) { nowOpen = set; }
    public bool getNowOpen(){return nowOpen;}
    void OnTriggerEnter2D(Collider2D other)
    {

    }
    void OnTriggerExit2D(Collider2D other)
    {
        nowOpen = false;
    }
    public void developReward()
    {
        GameObject reward = Instantiate(rewardObj, gameObject.transform.position, Quaternion.identity);
        gameObject.GetComponent<ChangeTextureOnTouch>().disappearObject();
    }

}
