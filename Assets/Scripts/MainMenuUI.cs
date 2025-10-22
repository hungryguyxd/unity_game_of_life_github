using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {
  [SerializeField] private Button classicButton;
  [SerializeField] private Button pvpButton;
  [SerializeField] private FadeController fadeController;

  private void Start() {
    classicButton.onClick.AddListener(() => fadeController.FadeToScene("Classic"));
    pvpButton.onClick.AddListener(() => fadeController.FadeToScene("PVP"));
  }
}