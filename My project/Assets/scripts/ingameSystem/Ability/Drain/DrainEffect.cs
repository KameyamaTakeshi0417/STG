using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrainEffect : _EffectAttachBase
{
    public float DrainPoint = 0;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public override void MakeEffect()
    {
        // HPを持つコンポーネントを取得
        PlayerHealth health = GameObject.Find("Player").GetComponent<PlayerHealth>();
        health.AddCurrentHP(DrainPoint);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            MakeEffect();
        }
    }
}
