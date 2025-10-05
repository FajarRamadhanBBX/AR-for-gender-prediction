using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections; // <-- PERBAIKAN: Menggunakan ini, bukan LowLevel.Unsafe
using UnityEngine.Networking;
using System.Collections;
using System;
using TMPro;

[Serializable]
public class PredictionResponse
{
    public string prediction;
    public string confidence; // Pastikan tipe data ini cocok dengan JSON Anda. Jika backend mengirim float, ini harus 'public float confidence;'
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
        // PERBAIKAN: Ambil referensi wajah yang baru terdeteksi
        if (args.added.Count > 0 && !sudahPunyaAksesoris)
        {
            ARFace face = args.added[0]; // Ambil wajah pertama yang terdeteksi
            StartCoroutine(PrediksiGender(face)); // Kirim referensi wajah ke coroutine
        }

        if (args.removed.Count > 0)
        {
            sudahPunyaAksesoris = false;
            if (logText) logText.text = "Wajah hilang, menunggu...";
            // Anda mungkin juga perlu menghancurkan aksesori yang sudah ada
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

        // <-- PERBAIKAN UTAMA UNTUK ERROR CS0214
        var rawTextureData = texture.GetRawTextureData<byte>();
        cpuImage.Convert(conversionParams, rawTextureData); 
        // ------------------------------------------

        cpuImage.Dispose();
        texture.Apply();
        if (logText) logText.text = "texture di-apply";
        return texture;
    }

    // PERBAIKAN: Coroutine sekarang menerima ARFace sebagai parameter
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
                    // Pastikan confidence di backend adalah float (bukan string persen)
                    // Jika backend mengirim string persen, parsing JSON akan gagal
                    logText.text = $"Prediksi: {response.prediction}\nKeyakinan: {response.confidence}";
                }

                // PERBAIKAN: Kirim referensi wajah ke metode pemasang aksesori
                BeriPrefabAksesoris(response.prediction, face);
            }
            else if (logText)
                logText.text = "Error koneksi ke server.";
        }
    }

    // PERBAIKAN: Metode ini sekarang menerima ARFace agar tahu di mana harus menempelkan prefab
    private void BeriPrefabAksesoris(string gender, ARFace face)
    {
        if (sudahPunyaAksesoris) return;
        
        GameObject prefab = null;
        if (logText) logText.text = "Beri prefab aksesoris";

        // Gunakan Contains agar lebih fleksibel (misal: "laki-laki" vs "Pria")
        if (gender.ToLower().Contains("pria")) prefab = prefabPria;
        else if (gender.ToLower().Contains("wanita")) prefab = prefabWanita;

        if (prefab != null)
        {
            // PERBAIKAN: Instantiate prefab sebagai anak dari 'face.transform', bukan 'transform'
            Instantiate(prefab, face.transform);
            sudahPunyaAksesoris = true;
        }
    }
}