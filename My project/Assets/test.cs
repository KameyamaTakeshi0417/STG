using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public float num;
    public GameObject textObj;

    // Start is called before the first frame update
    void Start() { }

    public void createObj()
    {
        GameObject damageTextInstance = Instantiate(textObj, gameObject.transform);
        damageTextInstance.GetComponent<DamageUI3D>().damage = num;
    }

    // Update is called once per frame
    void Update() { }
}
