using UnityEngine;
using UnityEngine.SceneManagement;

public class AlphaButtons : MonoBehaviour
{
    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
