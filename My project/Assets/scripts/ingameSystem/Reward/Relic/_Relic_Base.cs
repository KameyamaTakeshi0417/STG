using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Relic_Base : Reward
{
    protected GameObject m_Player;
    protected Player m_PlayerScript;
    protected PlayerHealth m_Playerhealth;

    // Start is called before the first frame update
    void Start() { }

    void Awake()
    {
        SetPlayerData();
    }

    // Update is called once per frame
    void Update() { }

    public virtual void GetEffect() { } //取得時のみ呼び出す

    public virtual void EquipEffect()
    {
        SetPlayerData();
    } //フロア開始時に呼び出す。

    public virtual void EquipHitEffect() { } //装備中発動し続ける効果。

    protected void SetPlayerData()
    {
        m_Player = GameObject.Find("Player");
        m_PlayerScript = m_Player.GetComponent<Player>();
        m_Playerhealth = m_Player.GetComponent<PlayerHealth>();
    }
}
