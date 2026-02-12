using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScript : MonoBehaviour
{
    public AudioClip bgmWarriors; // Background music clip for main menu 

    void Start()
    {
        AudioManager.Instance.PlayBGM(bgmWarriors); // Play menu BGM
    }

    void Update()
    {
        
    }

    public void onBtnLevel1()
    {
        SceneManager.LoadScene("L1"); // Placeholder scene loading; full level structure and progression system not finalized yet
    }

    public void onBtnLevel2()
    {
        SceneManager.LoadScene("L2"); // Temporary direct scene jump before implementing proper level selection logic
    }

    public void onBtnLevel3()
    {
        SceneManager.LoadScene("L3"); // Scene entry defined early; gameplay content may still be under development
    }

    public void onBtnLevel4()
    {
        SceneManager.LoadScene("L4"); // Reserved level slot; structure defined before full content implementation
    }

    public void onBtnLevel5()
    {
        SceneManager.LoadScene("L5"); // Scene reference prepared in advance for future expansion
    }

    public void onBtnLevel6()
    {
        SceneManager.LoadScene("SampleScene"); 
    }
}
