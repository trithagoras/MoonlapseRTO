using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    public int minute;
    float targetTime;

    TextMeshProUGUI timeText;

    // Start is called before the first frame update
    void Start()
    {
        timeText = GetComponent<TextMeshProUGUI>();
        targetTime = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        targetTime -= Time.deltaTime;

        if (targetTime <= 0f)  // 2 seconds
        {
            minute++;
            minute %= 1440;     // 24*60=1440
            targetTime = 2f;
            UpdateLabel();
        }
    }

    public void UpdateLabel()
    {
        timeText.text = FormatMinuteToString(minute);
    }

    string FormatMinuteToString(int minute)
    {
        int hour = minute / 60;
        int residue = minute % 60;

        bool pm = false;

        if (hour >= 12)
        {
            pm = true;
            if (hour >= 13)
            {
                hour %= 12;
            }
        }

        var minString = $"{residue}";
        minString = minString.PadLeft(2, '0');

        return $"{hour}:{minString} {(pm ? "pm" : "am")}";
    }
}
