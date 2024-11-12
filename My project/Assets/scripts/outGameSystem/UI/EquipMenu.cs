using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipMenu : MonoBehaviour
{
    public Bullet_Base myBullet;
    public Primer_Base myPrimer;
    public Case_Base myCase;
    public GameObject gameManager;
    private EquipManager attachedEquipManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void setBullet(Bullet_Base targetBullet)
    {
        myBullet = targetBullet;
    }
    public void setCase(Case_Base targetCase)
    {
        myCase = targetCase;
    }
    public void setPrimer(Primer_Base targetPrimer)
    {
        myPrimer = targetPrimer;
    }
    public Bullet_Base getBullet() { return myBullet; }
    public Case_Base getCase() { return myCase; }
    public Primer_Base getPrimer() { return myPrimer; }

}
