using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System;

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
    [Tooltip("Masukkan URL API prediksi kamu di sini")]
    public string apiURL = "http://54.255.108.159:8000/predict";

    [Header("Prefab Aksesoris")]
    public GameObject prefabPria;
    public GameObject prefabWanita;

    [Header("UI Komponen")]
    public GameObject infoPanel;
    public TextMeshProUGUI hasilText;
    public TextMeshProUGUI statusText;
    public GameObject startButton;

    private bool detectionActive = false;
    private bool sudahPunyaAksesoris = false;
    private GameObject aksesorisAktif;

    void Start()
    {
        if (infoPanel) infoPanel.SetActive(true);
        if (hasilText) hasilText.text = "GENDER: ?\nCONFIDENCE: ?";
        if (statusText) statusText.text = "Klik tombol untuk mulai deteksi.";

        // Warna awal tombol hijau
        UpdateButtonUI(false);
    }

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

    // 🔘 Fungsi utama tombol
    public void MulaiPrediksi()
    {
        if (!detectionActive)
        {
            // STATE: Mulai deteksi
            detectionActive = true;

            if (statusText != null)
                statusText.text = "Arahkan kamera ke wajah untuk mulai deteksi...";

            if (infoPanel != null)
            {
                infoPanel.SetActive(true);
                if (hasilText != null)
                    hasilText.text = "GENDER: ?\nCONFIDENCE: ?";
            }

            // Ubah tampilan tombol
            UpdateButtonUI(true);
        }
        else
        {
            // STATE: Reset
            ResetPrediksi();
        }
    }

    // 🔁 Fungsi Reset
    private void ResetPrediksi()
    {
        detectionActive = false;
        sudahPunyaAksesoris = false;

        if (aksesorisAktif != null)
        {
            Destroy(aksesorisAktif);
            aksesorisAktif = null;
        }

        if (hasilText != null)
            hasilText.text = "GENDER: ?\nCONFIDENCE: ?";
        if (statusText != null)
            statusText.text = "Klik tombol untuk mulai deteksi.";

        UpdateButtonUI(false);

        Debug.Log("Prediksi telah di reset.");
    }

    private void UpdateButtonUI(bool aktif)
    {
        if (startButton != null)
        {
            // Ubah teks tombol
            TextMeshProUGUI btnText = startButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = aktif ? "Reset Prediksi" : "Mulai Prediksi";

            // Ubah warna tombol
            Image btnImage = startButton.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = aktif ? Color.red : Color.green;
        }
    }

    // Event tracking wajah
    private void OnFacesChanged(ARTrackablesChangedEventArgs<ARFace> args)
    {
        if (!detectionActive) return;

        if (args.added.Count > 0 && !sudahPunyaAksesoris)
        {
            ARFace face = args.added[0];
            statusText.text = "Wajah terdeteksi. Mengirim ke server...";
            StartCoroutine(PrediksiGender(face));
        }
        else if (args.updated.Count > 0)
        {
            statusText.text = "Wajah masih terdeteksi...";
        }
        else if (args.removed.Count > 0)
        {
            sudahPunyaAksesoris = false;
            if (aksesorisAktif != null) Destroy(aksesorisAktif);
            statusText.text = "Wajah hilang. Arahkan kamera kembali.";
        }
        else
        {
            statusText.text = "Tidak ada wajah terdeteksi.";
        }
    }

    // Tangkap frame kamera
    private Texture2D CaptureCameraImage()
    {
        if (cameraManager == null || !cameraManager.TryAcquireLatestCpuImage(out var cpuImage))
        {
            statusText.text = "Gagal mengambil gambar kamera.";
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
        var rawTextureData = texture.GetRawTextureData<byte>();
        cpuImage.Convert(conversionParams, rawTextureData);
        cpuImage.Dispose();
        texture.Apply();

        return texture;
    }

    // Prediksi gender via API
    private IEnumerator PrediksiGender(ARFace face)
    {
        Texture2D screenshot = CaptureCameraImage();
        if (screenshot == null)
        {
            statusText.text = "Gagal mengambil gambar kamera.";
            yield break;
        }

        byte[] imageBytes = screenshot.EncodeToJPG();
        Destroy(screenshot);

        string base64 = Convert.ToBase64String(imageBytes);
        string json = $"{{\"image\":\"{base64}\"}}";

        using (UnityWebRequest req = new UnityWebRequest(apiURL, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<PredictionResponse>(req.downloadHandler.text);
                float.TryParse(response.confidence, out float conf);
                string persen = (conf * 100).ToString("F1");

                hasilText.text = $"GENDER: {response.prediction.ToUpper()}\nCONFIDENCE: {persen}%";
                statusText.text = "Deteksi berhasil!";
                BeriPrefabAksesoris(response.prediction, face);
            }
            else
            {
                statusText.text = "Gagal menghubungi server.";
            }
        }
    }

    private void BeriPrefabAksesoris(string gender, ARFace face)
    {
        if (sudahPunyaAksesoris) return;

        GameObject prefab = gender.ToLower().Contains("pria") ? prefabPria :
                            gender.ToLower().Contains("wanita") ? prefabWanita : null;

        if (prefab != null)
        {
            aksesorisAktif = Instantiate(prefab, face.transform);
            aksesorisAktif.transform.localPosition = new Vector3(0, 0.15f, 0);
            aksesorisAktif.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            sudahPunyaAksesoris = true;
            statusText.text = "Aksesoris ditambahkan!";
        }
        else
        {
            statusText.text = "Tidak ada prefab yang sesuai.";
        }
    }
}
