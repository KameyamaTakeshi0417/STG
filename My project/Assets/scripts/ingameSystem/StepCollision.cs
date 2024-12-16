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
        if (
            collision.CompareTag("Player")
            && GameObject.Find("GameManager").GetComponent<GameManager>().getCleared()
        )
        {
            StartCoroutine(fadeOut());
        }
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
