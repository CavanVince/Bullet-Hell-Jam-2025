using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject menuScreen;

    [SerializeField]
    private GameObject creditScreen;

    public void StartGame()
    {
        SceneManager.LoadScene("Map Generation");
    }

    public void LoadCredits()
    {
        menuScreen.SetActive(false);
        creditScreen.SetActive(true);
    }

    public void BackButton()
    {
        menuScreen.SetActive(true);
        creditScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
