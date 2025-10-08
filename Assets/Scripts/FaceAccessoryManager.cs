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

    public bool DetectionActive => detectionActive; // buat dicek di UIController
    public GameObject AksesorisAktif => aksesorisAktif;

    void Start()
    {
        if (infoPanel) infoPanel.SetActive(true);
        if (hasilText) hasilText.text = "GENDER: ?\nCONFIDENCE: ?";
        if (statusText) statusText.text = "Klik tombol untuk mulai deteksi.";

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

    public void MulaiPrediksi()
    {
        if (!detectionActive)
        {
            detectionActive = true;
            statusText.text = "Arahkan kamera ke wajah untuk mulai deteksi...";
            hasilText.text = "GENDER: ?\nCONFIDENCE: ?";
            UpdateButtonUI(true);
        }
        else
        {
            ResetPrediksi();
        }
    }

    private void ResetPrediksi()
    {
        detectionActive = false;
        sudahPunyaAksesoris = false;

        if (aksesorisAktif != null)
        {
            Destroy(aksesorisAktif);
            aksesorisAktif = null;
        }

        hasilText.text = "GENDER: ?\nCONFIDENCE: ?";
        statusText.text = "Klik tombol untuk mulai deteksi.";
        UpdateButtonUI(false);
    }

    private void UpdateButtonUI(bool aktif)
    {
        if (startButton != null)
        {
            TextMeshProUGUI btnText = startButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = aktif ? "Reset Prediksi" : "Mulai Prediksi";

            Image btnImage = startButton.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = aktif ? Color.red : Color.green;
        }
    }

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
            statusText.text = "Kamera mencari wajah...";
        }
    }

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

            // kirim ke UI controller
            ScaleUIController uiController = FindAnyObjectByType<ScaleUIController>();
            if (uiController != null)
            {
                uiController.SetTargetObject(aksesorisAktif.transform);
            }
        }
        else
        {
            statusText.text = "Tidak ada prefab yang sesuai.";
        }
    }
}
