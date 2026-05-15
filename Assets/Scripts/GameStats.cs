using UnityEngine;

public class GameStats : MonoBehaviour
{
    private float sessionTime = 0f;
    private bool isTracking = false;

    void Start()
    {
        isTracking = true;
    }

    void Update()
    {
        if (isTracking)
        {
            sessionTime += Time.deltaTime;
        }
    }

    void OnDestroy()
    {
        SaveSessionTime();
    }

    void OnApplicationQuit()
    {
        SaveSessionTime();
    }

    void SaveSessionTime()
    {
        float totalTime = PlayerPrefs.GetFloat("GameTime", 0f);
        totalTime += sessionTime;
        PlayerPrefs.SetFloat("GameTime", totalTime);
        PlayerPrefs.Save();
    }

    public static void AddDeath()
    {
        int deathCount = PlayerPrefs.GetInt("DeathCount", 0);
        deathCount++;
        PlayerPrefs.SetInt("DeathCount", deathCount);
        PlayerPrefs.Save();
    }
}