using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackToMenuButton : MonoBehaviour {
  [SerializeField] private Button backButton;

  private void Start() {
    if (backButton != null) backButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
  }
}