using UnityEngine;

public class ARPrefabManipulator : MonoBehaviour
{
    private float initialDistance;
    private Vector3 initialScale;

    void Update()
    {
        // Cek jika ada dua jari yang menyentuh layar
        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            // --- ROTASI (dengan menggerakkan kedua jari ke arah berlawanan) ---
            if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                // Vektor perpindahan (delta) untuk setiap jari
                Vector2 t1PrevPos = t1.position - t1.deltaPosition;
                Vector2 t2PrevPos = t2.position - t2.deltaPosition;

                Vector2 currentDir = t2.position - t1.position;
                Vector2 previousDir = t2PrevPos - t1PrevPos;

                // Hitung sudut rotasi
                float angle = Vector2.SignedAngle(previousDir, currentDir);

                // Terapkan rotasi pada objek
                transform.Rotate(0, -angle * 0.5f, 0); 
            }

            // --- RESIZE (dengan pinch in/out) ---
            if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(t1.position, t2.position);
                initialScale = transform.localScale;
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(t1.position, t2.position);
                float scaleFactor = currentDistance / initialDistance;

                // Batasi skala agar tidak terlalu besar/kecil
                transform.localScale = Vector3.ClampMagnitude(initialScale * scaleFactor, 3f); 
            }
        }
    }
}
