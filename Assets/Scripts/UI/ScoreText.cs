using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI highscore;
    [SerializeField] TextMeshProUGUI totalscore;

    void Start()
    {
        highscore.text = $"Biggest Paycheck: {PlayerPrefs.GetFloat("Highscore").ToString("0.## M€")}";
        totalscore.text = $"Total Money Earned: {PlayerPrefs.GetFloat("AllTimeScore").ToString("0.## M€")}";

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
