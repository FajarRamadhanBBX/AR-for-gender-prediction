using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashController : MonoBehaviour
{
    [Header("Next Scene")]
    public string nextSceneName = "SampleScene";   

    [Header("Durations (≈ total 3s)")]
    public float fadeInDuration  = 0.25f;          
    public float fadeOutDuration = 1.5f;           
    public float targetTotalDuration = 3.0f;      
    public bool  lockTotalToTarget = true;

    [Header("Fade Overlay")]
    public CanvasGroup fadeCanvas;                 

    float minShowTime;                             

    void Awake()
    {
        if (fadeCanvas) DontDestroyOnLoad(fadeCanvas.gameObject);
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        if (lockTotalToTarget)
            minShowTime = Mathf.Max(0f, targetTotalDuration - fadeInDuration - fadeOutDuration);

        // Fade-in splash
        if (fadeCanvas) { fadeCanvas.alpha = 0f; yield return Fade(0f, 1f, fadeInDuration); }

        // Mulai load scene utama
        var op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f) yield return null;
        yield return new WaitForSeconds(minShowTime);

        op.allowSceneActivation = true;
        yield return null;                        

        yield return new WaitForSeconds(0.3f);

        if (fadeCanvas) yield return Fade(1f, 0f, fadeOutDuration);

        if (fadeCanvas) Destroy(fadeCanvas.gameObject);
        Destroy(gameObject);
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            if (fadeCanvas) fadeCanvas.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }
        if (fadeCanvas) fadeCanvas.alpha = to;
    }
}
