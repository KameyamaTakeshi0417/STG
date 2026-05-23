using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Explosion : MonoBehaviour, IAlphaPoolable
{
    public GameObject sourcePrefab; // プール返却用の参照

    public void OnRentFromPool()
    {
        damagedCount = 3;
        isExploding = false;
        currentFrame = 0;
    }

    public void OnReturnToPool()
    {
        isExploding = false;
    }
    private float dmg = 30f;
    public int explosionTime = 10;
    private Vector3 scale = new Vector3(1, 1, 0);
    int damagedCount = 3;

    private bool isExploding = false;
    private int currentFrame = 0;

    public void startExplosion(float setdmg, int setExplosionTime)
    {
      //  dmg = setdmg;
        explosionTime = setExplosionTime;
        isExploding = true;
        currentFrame = 0;
    }

    void Update()
    {
        if (isExploding)
        {
            currentFrame++;
            if (currentFrame >= explosionTime)
            {
                if (Alpha_ObjectPoolManager.Instance != null && sourcePrefab != null)
                {
                    Alpha_ObjectPoolManager.Instance.Return(this.gameObject, sourcePrefab);
                }
                else
                {
                    Destroy(this.gameObject);
                }
            }
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (damagedCount > 0)
            {
                _Health_Base health = collision.gameObject.GetComponent<_Health_Base>();
                if (health != null) health.TakeDamage(dmg);
                damagedCount -= 1;
            }
        }
    }
}
