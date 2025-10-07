using UnityEngine;
using TMPro;
using System.Collections;

public class UIInfoController : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI genderText;
    public TextMeshProUGUI confidenceText;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f; 
    public void UpdateUI(string gender, float confidence)
    {
        if (genderText != null)
        {
            genderText.text = $"Gender: {gender}";
            StartCoroutine(FadeInText(genderText));
        }

        if (confidenceText != null)
        {
            confidenceText.text = $"Confidence: {confidence:F2}%";
            StartCoroutine(FadeInText(confidenceText));
        }
    }

    // Coroutine animasi fade-in
    private IEnumerator FadeInText(TextMeshProUGUI text)
    {
        //  transparan
        Color c = text.color;
        c.a = 0;
        text.color = c;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            text.color = c;
            yield return null;
        }

        c.a = 1;
        text.color = c;
    }
}
