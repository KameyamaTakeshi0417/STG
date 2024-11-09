using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class barrier : MonoBehaviour
{
    public int disappearCount;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void startDisappear()
    {
        StartCoroutine("disappear");
    }

    // Update is called once per frame
    void Update()
    {

    }
    private IEnumerator disappear()
    {
        int count = 0;
        while (count < disappearCount)
        {
            count++;
            yield return new WaitForSeconds(1);
        }
        Destroy(this.gameObject);
    }
}
