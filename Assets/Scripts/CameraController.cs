using UnityEngine;

public class CameraController : MonoBehaviour {
  [SerializeField] private float zoomSpeed = 1f;
  [SerializeField] private float minZoom = 5f;
  [SerializeField] private float maxZoom = 500f;

  [SerializeField] private float moveSpeed = 10f;

  private Camera cam;

  private void Awake() {
    cam = GetComponent<Camera>();
  }

  private void Update() {
    HandleZoom();
    HandleMovement();
  }

  private void HandleZoom() {
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    if (Mathf.Abs(scroll) > 0.001f) {
      cam.orthographicSize -= scroll * zoomSpeed;
      cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }
  }

  private void HandleMovement() {
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");
    if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f) {
      cam.transform.position += new Vector3(h, v, 0) * moveSpeed * Time.deltaTime;
    }
  }
}