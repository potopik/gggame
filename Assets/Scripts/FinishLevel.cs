using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLevel : MonoBehaviour
{
    public void FinishLvl()
    {
        PlayerPrefs.SetInt("VillageSun", SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(1);
    }
}
