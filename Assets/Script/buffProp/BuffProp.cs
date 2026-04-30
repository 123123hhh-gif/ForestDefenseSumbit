using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum Prop_BuffType
{
    
    MoveSpeed,
    Health,
}
public class BuffProp : MonoBehaviour
{
    public Prop_BuffType buffType;
   public int changeValue = 2;

   
    public GameObject getEff;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy == null)
            {
                Debug.LogWarning("未找到 BaseEnemy 组件！");
                return;
            }

            
            switch (buffType)
            {
                case Prop_BuffType.MoveSpeed:
                    
                    enemy.ApplyBuff(Buff.BuffType.MoveSpeed, changeValue, 5f);
                    break;
                case Prop_BuffType.Health:
                   
                    enemy.ApplyBuff(Buff.BuffType.MaxHealth, changeValue);
                    
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
               
                Destroy(eff, 2f);
            }

           
            Destroy(this.gameObject);
        }
    }
}
