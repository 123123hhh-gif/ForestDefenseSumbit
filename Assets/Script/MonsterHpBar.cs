using UnityEngine;
using UnityEngine.UI;


public class MonsterHpBar : MonoBehaviour
{
    [Header("Blood bar prefab and hanging point")]
    [SerializeField] private GameObject hpBarPrefab;      
    [SerializeField] private Transform hpFollowPoint;    
    [SerializeField] private float hpBarOffsetY = 1.5f;  

    [Header("Smooth following")]
    [SerializeField] private float smoothFollowSpeed = 5f;


    private GameObject _hpBarInstance;
    private Slider _hpSlider;
    private Canvas _hpCanvas;


    private BaseEnemy _enemy;


    private void Start()
    {

        _enemy = GetComponentInParent<BaseEnemy>();
        if (_enemy == null)
        {
            Debug.LogError($"[MonsterHpBar] BaseEnemy component not found, health bar will not work!", this);
            enabled = false;
            return;
        }


        if (hpBarPrefab != null && hpFollowPoint != null)
        {
            CreateHpBarInstance();
        }
        else
        {
            Debug.LogError($"[MonsterHpBar] hpBarPrefab or hpFollowPoint is not assigned!", this);
            enabled = false;
        }
    }


    private void CreateHpBarInstance()
    {

        Vector3 spawnPos = hpFollowPoint.position + Vector3.up * hpBarOffsetY;


        _hpBarInstance = Instantiate(hpBarPrefab, spawnPos, Quaternion.identity);


        _hpSlider = _hpBarInstance.GetComponentInChildren<Slider>();
        if (_hpSlider == null)
        {
            Debug.LogError($"[MonsterHpBar] Health bar prefab does not contain a Slider component!", this);
            return;
        }


        _hpCanvas = _hpBarInstance.GetComponent<Canvas>();
        if (_hpCanvas != null)
        {
            _hpCanvas.renderMode = RenderMode.WorldSpace;
            _hpCanvas.worldCamera = Camera.main;     
            _hpCanvas.planeDistance = 2f;
        }


        _hpSlider.minValue = 0;
        _hpSlider.maxValue = _enemy.MaxHealth;
        _hpSlider.value = _enemy.CurrentHealth;


        _hpBarInstance.SetActive(true);
        UpdateHealth(100, 100);
    }


    private void LateUpdate()
    {
        if (_hpBarInstance == null || hpFollowPoint == null || _enemy == null)
            return;


        Vector3 targetPos = hpFollowPoint.position + Vector3.up * hpBarOffsetY;


        _hpBarInstance.transform.position = Vector3.Lerp(
            _hpBarInstance.transform.position,
            targetPos,
            Time.deltaTime * smoothFollowSpeed
        );

 
        _hpBarInstance.transform.rotation = Quaternion.identity;
    }


    public void UpdateHealth(int currentHp, int maxHp)
    {
        if (_hpSlider == null || _enemy == null)
            return;


        if (_hpSlider.maxValue != maxHp)
            _hpSlider.maxValue = maxHp;

        // Debug.Log($"[MonsterHpBar] Updating health display: Current HP={currentHp}, Max HP={maxHp}  _hpSlider.maxValue={_hpSlider.maxValue}", this);
        _hpSlider.value = currentHp;


        ShowTemporarily(3f);
    }


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


    private void OnDestroy()
    {
        if (_hpBarInstance != null)
            Destroy(_hpBarInstance);
    }

    private void OnDrawGizmosSelected()
    {
        if (hpFollowPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hpFollowPoint.position + Vector3.up * hpBarOffsetY, 0.1f);
        }
    }
}