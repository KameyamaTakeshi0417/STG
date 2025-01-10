using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treasureBox : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public GameObject rewardObj;
    private bool nowOpen;
    public float checkInterval = 1.0f; // チェック間隔（秒）
    public GameObject rewardUIObj;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        nowOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Return)) && nowOpen)
        {
            audioSource.Play();
            showUI();
            StartCoroutine(DestroyAfterAudio());
        }
    }

    private IEnumerator DestroyAfterAudio()
    {
        // 音声の再生が終了するまで待つ
        yield return new WaitForSeconds(audioSource.clip.length * 0.25f);
        Destroy(this.gameObject);
        yield return null;
    }

    public void setNowOpen(bool set)
    {
        nowOpen = set;
    }

    public bool getNowOpen()
    {
        return nowOpen;
    }

    void OnTriggerEnter2D(Collider2D other) { }

    void OnTriggerExit2D(Collider2D other)
    {
        nowOpen = false;
    }

    private void showUI()
    {
        GameObject rewardUI = Instantiate(rewardUIObj, new Vector3(0, 0, 0), Quaternion.identity);
        //  rewardUI.GetComponent<rewardUIManager>().
    }

    public void developReward()
    {
        GameObject reward = Instantiate(
            rewardObj,
            gameObject.transform.position,
            Quaternion.identity
        );
        gameObject.GetComponent<ChangeTextureOnTouch>().disappearObject();
        Destroy(this.gameObject);
    }
}
