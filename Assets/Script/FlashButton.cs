using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class FlashButton : MonoBehaviour
{

    public bool useColorFlash = true;

    public Color normalColor = Color.white;

    public Color flashColor = new Color(1, 1, 0.8f, 1); 

    public float colorFlashSpeed = 1f;


    public bool useScaleFlash = true;

    public Vector3 normalScale = Vector3.one;

    public Vector3 flashScale = new Vector3(1.1f, 1.1f, 1.1f);

    public float scaleFlashSpeed = 1f;

    private Image buttonImage; 
    private Text buttonText;   
    private RectTransform rectTrans;
    private float colorTimer; 
    private float scaleTimer;  

    void Start()
    {
       
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<Text>();
        rectTrans = GetComponent<RectTransform>();

       
        if (buttonImage != null) buttonImage.color = normalColor;
        if (buttonText != null) buttonText.color = normalColor;
        if (rectTrans != null) rectTrans.localScale = normalScale;
    }

    void Update()
    {
        
        if (useColorFlash)
        {
            colorTimer += Time.deltaTime;
            float t = Mathf.PingPong(colorTimer / colorFlashSpeed, 1f);
            Color targetColor = Color.Lerp(normalColor, flashColor, t);
            
            if (buttonImage != null) buttonImage.color = targetColor;
            if (buttonText != null) buttonText.color = targetColor;
        }

        
        if (useScaleFlash)
        {
            scaleTimer += Time.deltaTime;
            float t = Mathf.PingPong(scaleTimer / scaleFlashSpeed, 1f);
            rectTrans.localScale = Vector3.Lerp(normalScale, flashScale, t);
        }
    }

    
    public void PauseFlash()
    {
        useColorFlash = false;
        useScaleFlash = false;
    }

    public void ResumeFlash()
    {
        useColorFlash = true;
        useScaleFlash = true;
    }
}