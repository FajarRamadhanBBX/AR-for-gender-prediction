using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using UnityEngine.Networking;
using System.Collections;
using System;
using TMPro;

[Serializable]
public class PredictionResponse
{
    public string prediction;
    public string confidence;
}

public class FaceAccessoryManager : MonoBehaviour
{
    [Header("Komponen AR")]
    public ARCameraManager cameraManager;
    public ARFaceManager faceManager;

    [Header("API Prediksi")]
    public string apiURL;

    [Header("Prefab Aksesoris")]
    public GameObject prefabPria;
    public GameObject prefabWanita;

    [Header("UI")]
    public TextMeshProUGUI logText;
    public TextMeshProUGUI changedText;

    private bool sudahPunyaAksesoris = false;

    void OnEnable()
    {
        if (faceManager != null)
            faceManager.trackablesChanged.AddListener(OnFacesChanged);
    }

    void OnDisable()
    {
        if (faceManager != null)
            faceManager.trackablesChanged.RemoveListener(OnFacesChanged);
    }

    private void OnFacesChanged(ARTrackablesChangedEventArgs<ARFace> args)
    {
        if (args.added.Count > 0 && !sudahPunyaAksesoris)
        {
            ARFace face = args.added[0]; // Ambil wajah pertama yang terdeteksi
            StartCoroutine(PrediksiGender(face)); // Kirim referensi wajah ke coroutine
        }

        if (args.removed.Count > 0)
        {
            sudahPunyaAksesoris = false;
            if (logText) logText.text = "Wajah hilang, menunggu...";
        }
    }

    private Texture2D CaptureCameraImage()
    {
        if (cameraManager == null || !cameraManager.TryAcquireLatestCpuImage(out var cpuImage))
        {
            Debug.LogWarning("Gagal mendapatkan ARCamera frame.");
            if (logText) logText.text = "Gagal mendapatkan frame kamera";
            return null;
        }

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
            outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorY
        };

        Texture2D texture = new Texture2D(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y, conversionParams.outputFormat, false);
        if (logText) logText.text = "Texture terbuat";

        var rawTextureData = texture.GetRawTextureData<byte>();
        cpuImage.Convert(conversionParams, rawTextureData); 

        cpuImage.Dispose();
        texture.Apply();
        if (logText) logText.text = "texture di-apply";
        return texture;
    }

    // Coroutine menerima ARFace sebagai parameter
    private IEnumerator PrediksiGender(ARFace face)
    {
        if (logText) logText.text = "Menganalisis wajah...";

        Texture2D screenshot = CaptureCameraImage();
        if (screenshot == null)
        {
            if (logText) logText.text = "Gagal mengambil gambar kamera.";
            yield break;
        }

        byte[] imageBytes = screenshot.EncodeToJPG();
        Destroy(screenshot);
        string base64 = Convert.ToBase64String(imageBytes);
        string json = $"{{\"image\":\"{base64}\"}}";
        if (logText) logText.text = $"json terbuat: {json}";

        using (UnityWebRequest req = new UnityWebRequest(apiURL, "POST"))
        {
            if (logText) logText.text = $"lakukan post dari json: {json}";
            req.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();
            if (logText) logText.text = $"send web request";

            if (req.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<PredictionResponse>(req.downloadHandler.text);
                if (logText)
                {
                    logText.text = $"Prediksi: {response.prediction}\nKeyakinan: {response.confidence}";
                }

                BeriPrefabAksesoris(response.prediction, face);
            }
            else if (logText)
                logText.text = "Error koneksi ke server.";
        }
    }

    // Menerima ARFace agar tahu di mana harus menempelkan prefab
    private void BeriPrefabAksesoris(string gender, ARFace face)
    {
        if (sudahPunyaAksesoris) return;
        
        GameObject prefab = null;
        if (logText) logText.text = "Beri prefab aksesoris";

        if (gender.ToLower().Contains("pria")) prefab = prefabPria;
        else if (gender.ToLower().Contains("wanita")) prefab = prefabWanita;

        if (prefab != null)
        {
            GameObject topi = Instantiate(prefab, face.transform);
            topi.transform.localPosition = new Vector3(0, 0.2f, 0);
            sudahPunyaAksesoris = true;
        }
    }
}