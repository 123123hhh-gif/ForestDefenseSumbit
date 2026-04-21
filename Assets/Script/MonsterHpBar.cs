using UnityEngine;
using UnityEngine.UI;

public class MonsterHpBar : MonoBehaviour
{
    [Header("血条配置")]
    public GameObject hpBarPrefab; 
    public Transform hpFollowPoint; 
    public float hpBarOffsetY = 1.5f; 
    public float smoothFollowSpeed = 5f; 

    [Header("怪物属性")]
    public float maxHp = 100f;
    private float currentHp;
    private Slider hpSlider; 
    private GameObject hpBarInstance; 

    void Start()
    {
        currentHp = maxHp;
        
        if (hpBarPrefab != null && hpFollowPoint != null)
        {
            // 计算初始位置（仅基于跟随点，无旋转影响）
            Vector3 targetPos = hpFollowPoint.position + new Vector3(0, hpBarOffsetY, 0);
            // 实例化血条时固定旋转（设为0，或你想要的固定角度）
            hpBarInstance = Instantiate(hpBarPrefab, targetPos, Quaternion.identity);
            
            hpSlider = hpBarInstance.GetComponentInChildren<Slider>();
            
            // 配置Canvas（仅保留必要的相机关联，移除旋转相关）
            Canvas hpCanvas = hpBarInstance.GetComponent<Canvas>();
            if (hpCanvas != null)
            {
                hpCanvas.worldCamera = Camera.main;
                hpCanvas.planeDistance = 2f;
                // 关键：将Canvas的渲染模式设为World Space（确保UI不随相机旋转）
                hpCanvas.renderMode = RenderMode.WorldSpace;
            }
            
            // 初始化血条数值
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;

            // 彻底移除朝向相机的代码！！
        }
    }

    void LateUpdate()
    {
        if (hpBarInstance != null && hpFollowPoint != null)
        {
            // 仅计算位置，完全不涉及旋转
            Vector3 targetPos = hpFollowPoint.position + new Vector3(0, hpBarOffsetY, 0);
            
            // 只更新位置，旋转始终保持初始的Quaternion.identity（无旋转）
            hpBarInstance.transform.position = Vector3.Lerp(
                hpBarInstance.transform.position, 
                targetPos, 
                Time.deltaTime * smoothFollowSpeed
            );

            // 【重要】强制锁定血条旋转，防止任何意外旋转
            hpBarInstance.transform.rotation = Quaternion.identity; // 固定为无旋转，也可设为你想要的角度（如Quaternion.Euler(0, 90, 0)）
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp = Mathf.Clamp(currentHp - damage, 0, maxHp);
        if (hpSlider != null)
        {
            hpSlider.value = currentHp;
        }

        ShowHpBar();
        
        if (currentHp <= 0)
        {
            HideHpBar();
        }
    }

    public void ShowHpBar()
    {
        if (hpBarInstance != null)
        {
            hpBarInstance.SetActive(true);
            
            CancelInvoke(nameof(HideHpBar));
            Invoke(nameof(HideHpBar), 5f);
        }
    }

    public void HideHpBar()
    {
        if (hpBarInstance != null)
        {
            // hpBarInstance.SetActive(false);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (hpFollowPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hpFollowPoint.position + new Vector3(0, hpBarOffsetY, 0), 0.1f);
        }
    }

    private void OnDestroy()
    {
        if (hpBarInstance != null)
        {
            Destroy(hpBarInstance);
        }
    }
}