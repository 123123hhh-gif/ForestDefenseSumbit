using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TowerPlace : MonoBehaviour
{

    public Color normalColor = Color.green;   
    public Color occupiedColor = Color.red;    
    public Renderer placeRenderer; 


    public float flashInterval = 0.5f;

    private bool isOccupied = false; 
    private BaseTower placedTower; 
    private Coroutine flashCoroutine; 

    private void Start()
    {
      
        if (placeRenderer != null)
        {
            StartRedGreenFlash();
        }
    }

    public void OnPlacePointClicked()
    {
        // Debug.Log("放置点被点击了！");
        
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
        

        StopFlashCoroutine();
        

        if (placeRenderer != null)
        {
            placeRenderer.material.color = occupiedColor;
        }
    }

    public void RemoveTower(bool isSell = false)
    {
        isOccupied = false;

        if (placedTower != null)
        {
            placedTower.DestroyTower(isSell);
            placedTower = null;
        }

        

        if (placeRenderer != null)
        {
            StartRedGreenFlash();
        }
    }


    private void StartRedGreenFlash()
    {

        StopFlashCoroutine();
        

        flashCoroutine = StartCoroutine(RedGreenFlashCoroutine());
    }


    private void StopFlashCoroutine()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
    }


    private IEnumerator RedGreenFlashCoroutine()
    {
      
        while (true)
        {

            placeRenderer.material.color = normalColor;
            yield return new WaitForSeconds(flashInterval);
            

            placeRenderer.material.color = occupiedColor;
            yield return new WaitForSeconds(flashInterval);
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


    private void OnDestroy()
    {
        StopFlashCoroutine();
    }

    public bool IsOccupied => isOccupied;
    public Vector3 PlacePosition => transform.position;
}