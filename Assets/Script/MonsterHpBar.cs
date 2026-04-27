using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 怪物血条组件，挂载在怪物预制体上。
/// 通过监听敌人的生命值变化来更新UI，不自行维护血量数据。
/// </summary>
public class MonsterHpBar : MonoBehaviour
{
    [Header("血条预制体与挂点")]
    [SerializeField] private GameObject hpBarPrefab;      // 血条UI预制体（World Space Canvas）
    [SerializeField] private Transform hpFollowPoint;    // 血条跟随的骨骼点（通常是头顶）
    [SerializeField] private float hpBarOffsetY = 1.5f;  // 相对于跟随点的垂直偏移

    [Header("平滑跟随")]
    [SerializeField] private float smoothFollowSpeed = 5f;

    // 运行时生成的UI实例
    private GameObject _hpBarInstance;
    private Slider _hpSlider;
    private Canvas _hpCanvas;

    // 缓存的敌人组件（用于读取生命值）
    private BaseEnemy _enemy;

    // ================================================================
    // 初始化（2025-03-17 [优化]）
    // ================================================================
    private void Start()
    {
        // 1. 获取敌人组件（必须）
        _enemy = GetComponentInParent<BaseEnemy>();
        if (_enemy == null)
        {
            Debug.LogError($"[MonsterHpBar] 未找到 BaseEnemy 组件，血条将无法工作！", this);
            enabled = false;
            return;
        }

        // 2. 生成血条实例
        if (hpBarPrefab != null && hpFollowPoint != null)
        {
            CreateHpBarInstance();
        }
        else
        {
            Debug.LogError($"[MonsterHpBar] hpBarPrefab 或 hpFollowPoint 未赋值！", this);
            enabled = false;
        }
    }

    // 创建血条实例并初始化UI组件
    private void CreateHpBarInstance()
    {
        // 计算初始位置
        Vector3 spawnPos = hpFollowPoint.position + Vector3.up * hpBarOffsetY;

        // 实例化（旋转固定为无旋转，符合世界空间固定要求）
        _hpBarInstance = Instantiate(hpBarPrefab, spawnPos, Quaternion.identity);

        // 获取Slider组件
        _hpSlider = _hpBarInstance.GetComponentInChildren<Slider>();
        if (_hpSlider == null)
        {
            Debug.LogError($"[MonsterHpBar] 血条预制体中未找到 Slider 组件！", this);
            return;
        }

        // 获取Canvas组件并配置（世界空间，固定不面向相机）
        _hpCanvas = _hpBarInstance.GetComponent<Canvas>();
        if (_hpCanvas != null)
        {
            _hpCanvas.renderMode = RenderMode.WorldSpace;
            _hpCanvas.worldCamera = Camera.main;      // 可选：使事件系统工作
            _hpCanvas.planeDistance = 2f;
        }

        // 初始化Slider范围（从敌人读取当前最大生命值）
        _hpSlider.minValue = 0;
        _hpSlider.maxValue = _enemy.MaxHealth;
        _hpSlider.value = _enemy.CurrentHealth;

        // 默认显示血条（可调整）
        _hpBarInstance.SetActive(true);
        UpdateHealth(100, 100);
    }

    // ================================================================
    // 每帧：平滑跟随 + 强制固定旋转（2025-03-17 [优化]）
    // ================================================================
    private void LateUpdate()
    {
        if (_hpBarInstance == null || hpFollowPoint == null || _enemy == null)
            return;

        // 目标位置 = 跟随点 + 垂直偏移
        Vector3 targetPos = hpFollowPoint.position + Vector3.up * hpBarOffsetY;

        // 平滑移动
        _hpBarInstance.transform.position = Vector3.Lerp(
            _hpBarInstance.transform.position,
            targetPos,
            Time.deltaTime * smoothFollowSpeed
        );

        // 【固定世界旋转】始终不旋转（保持与预制体一致的全局朝向）
        _hpBarInstance.transform.rotation = Quaternion.identity;
    }

    // ================================================================
    // 公开方法：供敌人调用，更新血量显示（2025-03-17 [新增]）
    // ================================================================
    /// <summary>
    /// 更新血条数值（由敌人受伤/治疗时主动调用）
    /// </summary>
    public void UpdateHealth(int currentHp, int maxHp)
    {
        if (_hpSlider == null || _enemy == null)
            return;

        // 更新Slider范围（最大生命值可能受Buff影响而变化）
        if (_hpSlider.maxValue != maxHp)
            _hpSlider.maxValue = maxHp;

        // Debug.Log($"[MonsterHpBar] 更新血量显示：当前HP={currentHp}，最大HP={maxHp}  _hpSlider.maxValue={_hpSlider.maxValue}", this);
        _hpSlider.value = currentHp;

        // 自动显示/隐藏逻辑（可选）
        ShowTemporarily(3f);
    }

    // 临时显示血条（例如受伤后显示几秒）
    private void ShowTemporarily(float duration)
    {
        if (_hpBarInstance != null)
        {
            _hpBarInstance.SetActive(true);
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), duration);
        }
    }

    private void Hide()
    {
        if (_hpBarInstance != null)
            _hpBarInstance.SetActive(false);
    }

    // ================================================================
    // 清理（2025-03-17 [优化]）
    // ================================================================
    private void OnDestroy()
    {
        if (_hpBarInstance != null)
            Destroy(_hpBarInstance);
    }

    // ================================================================
    // Editor 辅助（保持不变）
    // ================================================================
    private void OnDrawGizmosSelected()
    {
        if (hpFollowPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hpFollowPoint.position + Vector3.up * hpBarOffsetY, 0.1f);
        }
    }
}