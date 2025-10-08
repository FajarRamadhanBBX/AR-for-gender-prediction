using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScaleUIController : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject panel;               // Panel kontrol (berisi slider)
    public Slider scaleSlider;             // Slider untuk ubah ukuran
    public Slider rotateSlider;            // Slider untuk rotasi
    public TextMeshProUGUI warningText;    // Teks peringatan

    [Header("Target")]
    private Transform targetObject;        // Objek prefab aksesoris
    private FaceAccessoryManager faceManager;  // Referensi ke FaceAccessoryManager

    private float defaultScale = 1f;
    private float defaultRotation = 0f;

    void Start()
    {
        faceManager = FindAnyObjectByType<FaceAccessoryManager>();

        if (scaleSlider != null)
            scaleSlider.onValueChanged.AddListener(OnScaleChanged);

        if (rotateSlider != null)
            rotateSlider.onValueChanged.AddListener(OnRotationChanged);

        // Sembunyikan warning di awal
        if (warningText != null)
            warningText.gameObject.SetActive(false);

        // Pastikan panel aktif dari awal
        if (panel != null)
            panel.SetActive(true);
    }

    // Panggil dari FaceAccessoryManager ketika prefab sudah muncul
    public void SetTargetObject(Transform target)
    {
        targetObject = target;
        if (targetObject != null)
        {
            // Reset slider ke nilai awal
            if (scaleSlider != null)
            {
                scaleSlider.value = 1f;
                defaultScale = targetObject.localScale.x;
            }

            if (rotateSlider != null)
            {
                rotateSlider.value = 0f;
                defaultRotation = targetObject.localEulerAngles.y;
            }

            if (warningText != null)
                warningText.gameObject.SetActive(false);
        }
    }

    // Slider Scale
    void OnScaleChanged(float value)
    {
        // Jika belum melakukan prediksi, tampilkan warning
        if (faceManager != null && !faceManager.DetectionActive)
        {
            ShowWarning("Lakukan prediksi terlebih dahulu!");
            return;
        }

        if (targetObject != null)
        {
            float scaleValue = defaultScale * value;
            targetObject.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        }
    }

    // Slider Rotate
    void OnRotationChanged(float value)
    {
        // Jika belum melakukan prediksi, tampilkan warning
        if (faceManager != null && !faceManager.DetectionActive)
        {
            ShowWarning("Lakukan prediksi terlebih dahulu!");
            return;
        }

        if (targetObject != null)
        {
            float rotY = defaultRotation + value;
            targetObject.localEulerAngles = new Vector3(
                targetObject.localEulerAngles.x,
                rotY,
                targetObject.localEulerAngles.z
            );
        }
    }

    // Munculkan pesan warning sementara
    private void ShowWarning(string msg)
    {
        if (warningText == null) return;

        warningText.text = msg;
        warningText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideWarning));
        Invoke(nameof(HideWarning), 2.5f); // Hilang otomatis setelah 2.5 detik
    }

    // Sembunyikan warning
    private void HideWarning()
    {
        if (warningText != null)
            warningText.gameObject.SetActive(false);
    }
}
