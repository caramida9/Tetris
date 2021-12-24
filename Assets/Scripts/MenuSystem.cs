using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSystem : MonoBehaviour
{
    public void PlayAgain()
    {
        SceneManager.LoadScene("Level");
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("GameMenu");
    }
}
