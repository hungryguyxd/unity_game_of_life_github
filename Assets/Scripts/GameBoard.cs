using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameBoard : MonoBehaviour {
  [SerializeField] private Tilemap currentState;
  [SerializeField] private Tilemap nextState;
  [SerializeField] private Tile aliveTile;
  [SerializeField] private Pattern pattern;
  [SerializeField] private float updateInterval = 0.5f;
  [SerializeField] private TextMeshProUGUI statusText;
  [SerializeField] private TextMeshProUGUI speedText;

  
  private HashSet<Vector3Int> aliveCells = new HashSet<Vector3Int>();
  private HashSet<Vector3Int> cellsToCheck = new HashSet<Vector3Int>();
  private bool isPaused = true;
  private Coroutine simulationCoroutine;
  private Camera cam;

  private void Awake() {
    cam = Camera.main;
  }

  private void Start() {
    SetPattern(pattern);
    UpdateText();
  }

  private void Update() {
    InputHandler();
  }

  private void InputHandler() {
    if (Input.GetKeyDown(KeyCode.Space)) {
      PauseHandler();
    }
    if (Input.GetKeyDown(KeyCode.Equals)) {
      updateInterval = Mathf.Max(0.05f, updateInterval - 0.05f);
      UpdateText();
    }
    if (Input.GetKeyDown(KeyCode.Minus)) {
      updateInterval += 0.05f;
      UpdateText();
    }
    if (isPaused && Input.GetKeyDown(KeyCode.R)) {
      GenerateRandomPattern();
    }
    if (isPaused && Input.GetMouseButtonDown(0)) {
      CellClickUpdate(true);
    }
    if (isPaused && Input.GetMouseButtonDown(1)) {
      CellClickUpdate(false);
    }
  }

  private void PauseHandler() {
    isPaused = !isPaused;
    if (isPaused) {
      if (simulationCoroutine != null) {
        StopCoroutine(simulationCoroutine);
      }
    } else {
      RebuildAliveCellsFromTiles();
      simulationCoroutine = StartCoroutine(Simulate());
    }
    UpdateText();
  }

  private void RebuildAliveCellsFromTiles() {
    aliveCells.Clear();
    foreach (var pos in currentState.cellBounds.allPositionsWithin) {
      if (currentState.GetTile(pos) == aliveTile) {
        aliveCells.Add(pos);
      }
    }
  }

  private void CellClickUpdate(bool alive) {
    Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
    Vector3Int cell = currentState.WorldToCell(mouseWorld);
    if (alive) {
      currentState.SetTile(cell, aliveTile);
      aliveCells.Add(cell);
    } else {
      currentState.SetTile(cell, null);
      aliveCells.Remove(cell);
    }
  }

  private void UpdateText() {
    if (statusText) {
      statusText.text = isPaused ? "PAUSED" : "RUNNING";
    }
    if (speedText) {
      speedText.text = $"Time of change: {updateInterval:F2}";
    }
  }

  private void SetPattern(Pattern pattern) {
    Clear();
    Vector2Int center = pattern.GetCenter();
    foreach (Vector2Int c in pattern.cells) {
      Vector3Int cell = (Vector3Int)(c - center);
      currentState.SetTile(cell, aliveTile);
      aliveCells.Add(cell);
    }
  }

  private void Clear() {
    currentState.ClearAllTiles();
    nextState.ClearAllTiles();
    aliveCells.Clear();
    cellsToCheck.Clear();
  }

  private IEnumerator Simulate() {
    while (!isPaused) {
      UpdateState();
      yield return new WaitForSeconds(updateInterval);
    }
  }

  private void UpdateState() {
    cellsToCheck.Clear();
    foreach (Vector3Int cell in aliveCells) {
      for (int x = -1; x <= 1; ++x) {
        for (int y = -1; y <= 1; ++y) {
          cellsToCheck.Add(cell + new Vector3Int(x, y, 0));
        }
      }
    }
    HashSet<Vector3Int> newAlive = new HashSet<Vector3Int>();
    foreach (Vector3Int cell in cellsToCheck) {
      int neighbors = CountNeighbors(cell);
      bool alive = IsAlive(cell);
      if (alive && (neighbors == 2 || neighbors == 3)) {
        newAlive.Add(cell);
      } else if (!alive && neighbors == 3) {
        newAlive.Add(cell);
      }
    }
    currentState.ClearAllTiles();
    foreach (Vector3Int cell in newAlive) {
      currentState.SetTile(cell, aliveTile); //
    }
    aliveCells = newAlive;
  }

  private int CountNeighbors(Vector3Int cell) {
    int count = 0;
    for (int x = -1; x <= 1; ++x) {
      for (int y = -1; y <= 1; ++y) {
        if (x == 0 && y == 0) {
          continue;
        }
        Vector3Int neighbor = cell + new Vector3Int(x, y, 0);
        if (IsAlive(neighbor)) {
          ++count;
        }
      }
    }
    return count;
  }

  private bool IsAlive(Vector3Int cell) {
    return currentState.GetTile(cell) == aliveTile;
  }
  
  private void GenerateRandomPattern() {
    Vector3 camPos = cam.transform.position;
    float height = 2f * cam.orthographicSize;
    float width = height * cam.aspect;
    Vector2Int min = new Vector2Int(
      Mathf.FloorToInt(camPos.x - width / 2),
      Mathf.FloorToInt(camPos.y - height / 2)
    );
    Vector2Int max = new Vector2Int(
      Mathf.CeilToInt(camPos.x + width / 2),
      Mathf.CeilToInt(camPos.y + height / 2)
    );
    Pattern tempPattern = ScriptableObject.CreateInstance<Pattern>();
    tempPattern.randomFillPercent = pattern.randomFillPercent;
    tempPattern.GenerateRandom(min, max);
    SetPattern(tempPattern);
  }
}