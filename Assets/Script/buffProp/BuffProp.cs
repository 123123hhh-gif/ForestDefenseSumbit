using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum Prop_BuffType
{
    //加属性的4种类型
    MoveSpeed,
    Health,
}
public class BuffProp : MonoBehaviour
{
    public Prop_BuffType buffType;
   public int changeValue = 2;

    //获取特效
    public GameObject getEff;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 从碰撞体中获取敌人的 BaseEnemy 组件
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy == null)
            {
                Debug.LogWarning("未找到 BaseEnemy 组件！");
                return;
            }

            // 根据 buffType 对敌人施加效果
            switch (buffType)
            {
                case Prop_BuffType.MoveSpeed:
                    // 给敌人施加减速/加速Buff，持续5秒
                    enemy.ApplyBuff(Buff.BuffType.MoveSpeed, changeValue, 5f);
                    break;
                case Prop_BuffType.Health:
                    // 给敌人治疗（或增加最大生命值）
                    enemy.ApplyBuff(Buff.BuffType.MaxHealth, changeValue);
                    // 或者直接治疗当前生命值
                    // enemy.Heal(changeValue);
                    break;
            }

            // 播放特效
            if (getEff != null)
            {
                GameObject eff = Instantiate(getEff, this.transform.position, this.transform.rotation);
                AudioSource audioS = eff.GetComponent<AudioSource>();
                if (audioS != null)
                {
                    // audioS.volume = GameDataMgr.Instance.musicData.soundValue;
                    // audioS.mute = !GameDataMgr.Instance.musicData.isOpenSound;
                }
                // 自动销毁特效
                Destroy(eff, 2f);
            }

            // 销毁道具
            Destroy(this.gameObject);
        }
    }
}
