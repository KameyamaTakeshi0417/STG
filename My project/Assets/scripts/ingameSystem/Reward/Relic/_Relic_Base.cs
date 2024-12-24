using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Relic_Base : Reward
{
    protected GameObject m_Player;
    protected Player m_PlayerScript;

    // Start is called before the first frame update
    void Start() { }

    void Awake()
    {
        m_Player = GameObject.Find("Player");
        m_PlayerScript = m_Player.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update() { }

    public virtual void GetEffect() { } //取得時のみ呼び出す

    public virtual void EquipEffect() { } //装備時、フロア開始時に呼び出す。
}
