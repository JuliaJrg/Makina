using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float dayDurationInSeconds = 30f;
    private float currentTime = 0f;
    private static int currentDay = 0;

    public delegate void DayChanged(int newDay);
    public static event DayChanged OnDayChanged;

    public delegate void TimeUpdated(float newTime);
    public static event TimeUpdated OnTimeUpdated;

    void Start()
    {
        StartCoroutine(UpdateDay());
    }

    IEnumerator UpdateDay()
    {
        while (true)
        {
            yield return new WaitForSeconds(dayDurationInSeconds);
            currentDay++;
            currentTime = 0f;
            OnDayChanged?.Invoke(currentDay);
        }
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        OnTimeUpdated?.Invoke(currentTime);
    }

    public static int GetCurrentDay()
    {
        return currentDay;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }
}