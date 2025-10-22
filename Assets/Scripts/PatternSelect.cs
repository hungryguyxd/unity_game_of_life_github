using UnityEngine;
using UnityEngine.UI;

public class PatternSelector : MonoBehaviour {
  public GameBoardPVP gameBoard;
  public Button[] patternButtons;
  private void Start() {
    for (int i = 0; i < patternButtons.Length; ++i) {
      int index = i;
      patternButtons[i].onClick.AddListener(() => SelectPattern(index));
    }
  }
  public void SelectPattern(int index) {
    gameBoard.SelectPattern(index);
  }
}