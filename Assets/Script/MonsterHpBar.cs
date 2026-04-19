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
           
            Vector3 targetPos = hpFollowPoint.position + new Vector3(0, hpBarOffsetY, 0);
           
            hpBarInstance = Instantiate(hpBarPrefab, targetPos, Quaternion.identity);
            
            hpSlider = hpBarInstance.GetComponentInChildren<Slider>();
            
          
            Canvas hpCanvas = hpBarInstance.GetComponent<Canvas>();
            if (hpCanvas != null)
            {
                hpCanvas.worldCamera = Camera.main;
                hpCanvas.planeDistance = 2f;
            }
            
            
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
            
           
            hpBarInstance.transform.LookAt(Camera.main.transform);
            hpBarInstance.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            
           
        }
    }

    void LateUpdate()
    {
       
        if (hpBarInstance != null && hpFollowPoint != null)
        {
           
            Vector3 targetPos = hpFollowPoint.position + new Vector3(0, hpBarOffsetY, 0);
           
            hpBarInstance.transform.position = Vector3.Lerp(
                hpBarInstance.transform.position, 
                targetPos, 
                Time.deltaTime * smoothFollowSpeed
            );
            
            
            hpBarInstance.transform.LookAt(Camera.main.transform);
            hpBarInstance.transform.rotation = Quaternion.LookRotation(
                Camera.main.transform.forward
            );
        }
    }


    public void TakeDamage(float damage)
    {
        currentHp = Mathf.Clamp(currentHp - damage, 0, maxHp);
        hpSlider.value = currentHp;
        

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