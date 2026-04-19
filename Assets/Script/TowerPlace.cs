using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlace : MonoBehaviour
{
    [Header("视觉反馈")]
    public Color normalColor = Color.green;
    public Color occupiedColor = Color.red;
    public Renderer placeRenderer; 

    private bool isOccupied = false; 
    private BaseTower placedTower; 

    private void Start()
    {
        
        if (placeRenderer != null)
        {
            placeRenderer.material.color = normalColor;
        }
    }

    public void OnPlacePointClicked()
    {
        Debug.Log("放置点被点击了！");
        
        if (!isOccupied)
        {
            UIManager.Instance.onTowerSelectPanel(this);
           
            // GameManager.Instance.PlaceTower(this);
        }
        else
        {
          
            if (placedTower != null)
            {
                UIManager.Instance.ShowUpgradePanel(placedTower);
            }
        }
    }

   
    public void SetTower(BaseTower tower)
    {
        isOccupied = true;
        placedTower = tower;
        if (placeRenderer != null)
        {
            placeRenderer.material.color = occupiedColor;
        }
    }


    public void RemoveTower()
    {
        isOccupied = false;
        placedTower = null;
        if (placeRenderer != null)
        {
            placeRenderer.material.color = normalColor;
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (placedTower != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, placedTower.CurrentData.attackRange);
        }
    }


    public bool IsOccupied => isOccupied;
    public Vector3 PlacePosition => transform.position;
}