using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour {
  [SerializeField] private CanvasGroup fadeGroup;
  [SerializeField] private float fadeDuration = 0.75f;
  
  private void Start() {
    StartCoroutine(FadeIn());
  }

  public void FadeToScene(string sceneName) {
    StartCoroutine(FadeOut(sceneName));
  }

  private IEnumerator FadeIn() {
    fadeGroup.alpha = 1;
    float t = 0;
    while (t < fadeDuration) {
      t += Time.deltaTime;
      fadeGroup.alpha = 1 - (t / fadeDuration);
      yield return null;
    }
    fadeGroup.alpha = 0;
  }

  private IEnumerator FadeOut(string sceneName) {
    fadeGroup.alpha = 0;
    float t = 0;
    while (t < fadeDuration) {
      t += Time.deltaTime;
      fadeGroup.alpha = t / fadeDuration;
      yield return null;
    }
    fadeGroup.alpha = 1;
    SceneManager.LoadScene(sceneName);
  }
}