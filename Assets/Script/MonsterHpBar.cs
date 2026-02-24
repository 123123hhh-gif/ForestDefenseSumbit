using UnityEngine;
using UnityEngine.UI;

public class MonsterHpBar : MonoBehaviour
{
    [Header("HP Bar Configuration")]
    public GameObject hpBarPrefab;
    public Transform hpFollowPoint;
    public float hpBarOffsetY = 1.5f;
    public float smoothFollowSpeed = 5f;

    [Header("Monster Attributes")]
    public float maxHp = 100f;
    private float currentHp;
    private Slider hpSlider;
    private GameObject hpBarInstance;

    void Start()
    {
        currentHp = maxHp;

        if (hpBarPrefab != null && hpFollowPoint != null)
        {
            // Calculate initial position (based only on follow point, no rotation impact)
            Vector3 targetPos = hpFollowPoint.position + new Vector3(0, hpBarOffsetY, 0);
            // Fix rotation when instantiating HP bar (set to 0, or your desired fixed angle)
            hpBarInstance = Instantiate(hpBarPrefab, targetPos, Quaternion.identity);

            hpSlider = hpBarInstance.GetComponentInChildren<Slider>();

            // Configure Canvas (only keep necessary camera association, remove rotation-related logic)
            Canvas hpCanvas = hpBarInstance.GetComponent<Canvas>();
            if (hpCanvas != null)
            {
                hpCanvas.worldCamera = Camera.main;
                hpCanvas.planeDistance = 2f;
                // Key: Set Canvas render mode to World Space (ensure UI does not rotate with camera)
                hpCanvas.renderMode = RenderMode.WorldSpace;
            }

            // Initialize HP bar values
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;

            // Completely remove the code for facing the camera!!
        }
    }

    void LateUpdate()
    {
        if (hpBarInstance != null && hpFollowPoint != null)
        {
            // Only calculate position, no rotation involved at all
            Vector3 targetPos = hpFollowPoint.position + new Vector3(0, hpBarOffsetY, 0);

            // Only update position, rotation always remains initial Quaternion.identity (no rotation)
            hpBarInstance.transform.position = Vector3.Lerp(
                hpBarInstance.transform.position,
                targetPos,
                Time.deltaTime * smoothFollowSpeed
            );

            // [Important] Force lock HP bar rotation to prevent any accidental rotation
            hpBarInstance.transform.rotation = Quaternion.identity; // Fixed to no rotation, or set to your desired angle (e.g., Quaternion.Euler(0, 90, 0))
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