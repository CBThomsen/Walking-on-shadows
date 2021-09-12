using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class FPSCounter : MonoBehaviour
{
    string display = "{0} FPS";
    private Text m_Text;

    private int fpsTick = 0;
    private float[] fpsList = new float[30];

    private void Start()
    {
        m_Text = GetComponent<Text>();
    }


    private void Update()
    {
        fpsList[fpsTick] = (1f / Time.deltaTime);
        fpsTick = (fpsTick + 1) % fpsList.Length;

        int avgFrameRate = Mathf.RoundToInt(fpsList.Average());
        m_Text.text = string.Format(display, avgFrameRate.ToString());
    }
}