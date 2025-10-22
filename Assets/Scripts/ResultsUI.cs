using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultsUI : MonoBehaviour {
  [SerializeField] private GameObject resultsPanel;
  [SerializeField] private TextMeshProUGUI resultText;

  private void Awake() {
    resultsPanel.SetActive(false);
  }

  public void ShowResult(string text) {
    resultText.text = text;
    resultsPanel.SetActive(true);
  }

  public void HideResult(){
    resultsPanel.SetActive(false);
  }
}