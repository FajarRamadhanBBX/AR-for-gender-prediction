using UnityEngine;

public class Spinner : MonoBehaviour
{
    public float speed = 180f; // derajat per detik
    void Update() => transform.Rotate(0f, 0f, -speed * Time.deltaTime);
}
