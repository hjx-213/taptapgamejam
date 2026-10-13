
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject galleryPanel;

    public void StartGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.StartNewGame("Tutorial");
        }
        else
        {
            Debug.LogError("找不到 SaveManager，请检查 MainMenu 场景中的绑定。");
        }
    }

    public void ContinueGame()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ContinueGame();
        }
        else
        {
            Debug.LogError("找不到 SaveManager，请检查 MainMenu 场景中的绑定。");
        }
    }

    public void OpenGallery()
    {
        mainMenuPanel.SetActive(false);
        galleryPanel.SetActive(true);

        FindObjectOfType<GalleryManager>().RefreshEndingButtons();
    }

    public void CloseGallery()
    {
        galleryPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
