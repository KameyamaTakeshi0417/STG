using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepCollision : MonoBehaviour
{
    public bool Onstep;
    public bool canEnter;
    public int stepNum;
    public GameObject fadeCanvas;
    public Animator animator; // アニメーターの参照

    // Start is called before the first frame update
    void Start()
    {
        Onstep = false;
        GameObject.Find("GameManager").GetComponent<GameManager>().AblePlayerInput();
    }

    // Update is called once per frame
    void Update()
    {
        if (
            animator.GetBool("cleared") == false
            && GameObject.Find("GameManager").GetComponent<GameManager>().getCleared()
        )
        {
            animator.SetBool("cleared", true);
        }
    }

    public void setEnterable(bool set)
    {
        canEnter = set;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (collision.CompareTag("Player") && manager.getCleared())
        {
            saveStatus(collision.gameObject, GameObject.Find("GameManager"));
            manager.DisablePlayerInput();
            StartCoroutine(fadeOut());
        }
    }

    private void saveStatus(GameObject playerObj, GameObject managerObj)
    {
        PlayerHealth playerHPScript = playerObj.GetComponent<PlayerHealth>();
        Player playerStatusScript = playerObj.GetComponent<Player>();
        PlayerStatusManager statusManagerScript = managerObj.GetComponent<PlayerStatusManager>();

        statusManagerScript.saveStatus(playerObj);
    }

    private IEnumerator fadeOut()
    {
        FadeBoard targetScript;
        targetScript = fadeCanvas.GetComponent<FadeBoard>();
        targetScript.StartFadeIn();
        yield return new WaitForEndOfFrame();
        while (targetScript.nowFade)
        {
            yield return new WaitForEndOfFrame();
        }
        GameObject.Find("GameManager").GetComponent<GameManager>().ChangeScene(stepNum);
        yield return null;
    }
}
