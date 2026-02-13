using UnityEngine;
using UnityEngine.UI;

public class MonsterHpBar : MonoBehaviour
{
    [Header("HP Bar Settings")]
    public GameObject hpBarPrefab; // Prefab of the HP bar UI
    public Transform hpFollowPoint; // Transform point that the HP bar follows
    public float hpBarOffsetY = 1.5f; // Y-axis offset of HP bar from follow point
    public float smoothFollowSpeed = 5f; // Smooth follow speed of HP bar

    [Header("Monster Attributes")]
    public float maxHp = 100f; // Maximum HP of the monster
    private float currentHp; // Current HP of the monster
    private Slider hpSlider; // Slider component of the HP bar
    private GameObject hpBarInstance; // Instantiated HP bar game object

    void Start()
    {
        currentHp = maxHp;

        if (hpBarPrefab != null && hpFollowPoint != null)
        {
            // Calculate initial position (based only on follow point, no rotation influence)
            Vector3 targetPos = hpFollowPoint.position + new Vector3(0, hpBarOffsetY, 0);
            // Instantiate HP bar with fixed rotation (set to 0, or your desired fixed angle)
            hpBarInstance = Instantiate(hpBarPrefab, targetPos, Quaternion.identity);

            hpSlider = hpBarInstance.GetComponentInChildren<Slider>();

            // Configure Canvas (only keep necessary camera association, remove rotation-related settings)
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

            // Completely remove code for facing camera!!
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
            hpBarInstance.transform.rotation = Quaternion.identity; // Fixed to no rotation, or set to your desired angle (e.g. Quaternion.Euler(0, 90, 0))
        }
    }

    // Apply damage to the monster and update HP bar
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

    // Show HP bar and set auto-hide timer
    public void ShowHpBar()
    {
        if (hpBarInstance != null)
        {
            hpBarInstance.SetActive(true);

            CancelInvoke(nameof(HideHpBar));
            Invoke(nameof(HideHpBar), 5f);
        }
    }

    // Hide HP bar (currently commented out)
    public void HideHpBar()
    {
        if (hpBarInstance != null)
        {
            // hpBarInstance.SetActive(false);
        }
    }

    // Draw gizmo for HP bar follow position in Scene view (when object is selected)
    void OnDrawGizmosSelected()
    {
        if (hpFollowPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hpFollowPoint.position + new Vector3(0, hpBarOffsetY, 0), 0.1f);
        }
    }

    // Clean up HP bar instance when monster is destroyed
    private void OnDestroy()
    {
        if (hpBarInstance != null)
        {
            Destroy(hpBarInstance);
        }
    }
}