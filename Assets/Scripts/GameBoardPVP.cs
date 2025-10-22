using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GameBoardPVP : MonoBehaviour {
  [SerializeField] private Tilemap currentState;
  [SerializeField] private Tilemap borderTilemap;
  [SerializeField] private Tile aliveTilePlayer1;
  [SerializeField] private Tile aliveTilePlayer2;
  [SerializeField] private Tile borderTile;

  [SerializeField] private int boardSize = 50;
  [SerializeField] private float updateInterval = 0.2f;

  [SerializeField] private TextMeshProUGUI statusText;
  [SerializeField] private TextMeshProUGUI scoreText;

  [SerializeField] private Pattern[] availablePatterns;
  [SerializeField] private ResultsUI resultsUI;

  private int selectedPatternIndex = 0;

  [SerializeField] private int historyLimit = 10;
  private List<HashSet<Vector3Int>> history = new List<HashSet<Vector3Int>>();

  private HashSet<Vector3Int> aliveCells = new HashSet<Vector3Int>();
  private Dictionary<Vector3Int, int> cellOwners = new Dictionary<Vector3Int, int>();
  private bool isPaused = true;
  private bool setupPhase = true;
  private int currentPlayer = 1;
  private bool gameOver = false;
  private int scorePlayer1 = 0;
  private int scorePlayer2 = 0;
  private Coroutine simulationCoroutine;
  private Camera cam;

  private void Awake() {
    cam = Camera.main;
  }

  private void Start() {
    Clear();
    DrawBorders();
    UpdateText();
  }

  private void Update() {
    HandleInput();
  }

  private void HandleInput() {
    if (Input.GetKeyDown(KeyCode.Space)) {
      TogglePause();
    }
    if (isPaused && Input.GetMouseButtonDown(0) && !gameOver) {
      Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
      Vector3Int cell = currentState.WorldToCell(mouseWorld);
      if (!InBounds(cell)) {
        return;
      }
      if (setupPhase) {
        PlacePatternAtCell(cell);
      } else {
        ToggleCell(cell);
      }
    }
  }

  private void TogglePause() {
    isPaused = !isPaused;
    if (isPaused) {
      if (simulationCoroutine != null) {
        StopCoroutine(simulationCoroutine);
      }
    } else {
      simulationCoroutine = StartCoroutine(Simulate());
    }
    UpdateText();
  }

  private void PlaceCellForCurrentPlayer(Vector3Int cell) {
    Tile tile = currentPlayer == 1 ? aliveTilePlayer1 : aliveTilePlayer2;
    currentState.SetTile(cell, tile);
    aliveCells.Add(cell);
    cellOwners[cell] = currentPlayer;
    currentPlayer = currentPlayer == 1 ? 2 : 1;
    UpdateText();
  }

  private bool InBounds(Vector3Int cell) {
    int half = boardSize / 2;
    return Mathf.Abs(cell.x) <= half && Mathf.Abs(cell.y) <= half;
  }

  private void Clear() {
    currentState.ClearAllTiles();
    // nextState.ClearAllTiles();
    aliveCells.Clear();
    cellOwners.Clear();
    scorePlayer1 = 0;
    scorePlayer2 = 0;
    currentPlayer = 1;
    setupPhase = true;
    isPaused = true;
    gameOver = false;
  }

  private void DrawBorders() {
    int half = boardSize / 2;
    for (int x = -half - 1; x <= half + 1; ++x) {
      borderTilemap.SetTile(new Vector3Int(x, -half - 1, 0), borderTile);
      borderTilemap.SetTile(new Vector3Int(x, half + 1, 0), borderTile);
    }
    for (int y = -half - 1; y <= half + 1; ++y) {
      borderTilemap.SetTile(new Vector3Int(-half - 1, y, 0), borderTile);
      borderTilemap.SetTile(new Vector3Int(half + 1, y, 0), borderTile);
    }
  }

  private void UpdateText() {
    if (statusText) {
      if (setupPhase) {
        statusText.text = $"SETUP — Player {currentPlayer}";
      } else {
        statusText.text = isPaused ? "PAUSED" : "RUNNING";
      }
    }
    if (scoreText) {
      scoreText.text = $"P1: {scorePlayer1} | P2: {scorePlayer2}";
    }
  }

  private IEnumerator Simulate() {
    while (!isPaused) {
      UpdateState();
      yield return new WaitForSeconds(updateInterval);
    }
  }
  private void UpdateState() {
    var cellsToCheck = new HashSet<Vector3Int>();
    foreach (var cell in aliveCells) {
      for (int x = -1; x <= 1; ++x) {
        for (int y = -1; y <= 1; ++y) {
          cellsToCheck.Add(cell + new Vector3Int(x, y, 0));
        }
      }
    }
    var newAlive = new HashSet<Vector3Int>();
    var newOwners = new Dictionary<Vector3Int, int>();
    foreach (var cell in cellsToCheck) {
      if (!InBounds(cell)) {
        continue;
      }
      int neighbors = 0;
      int count1 = 0;
      int count2 = 0;
      for (int x = -1; x <= 1; ++x) {
        for (int y = -1; y <= 1; ++y) {
          if (x == 0 && y == 0) {
            continue;
          }
          Vector3Int n = cell + new Vector3Int(x, y, 0);
          if (aliveCells.Contains(n)) {
            ++neighbors;
            if (cellOwners.TryGetValue(n, out int owner)) {
              if (owner == 1) {
                ++count1;
              }
              if (owner == 2) {
                ++count2;
              }
            }
          }
        }
      }
      bool alive = aliveCells.Contains(cell);
      int newOwner = 0;
      if (alive && (neighbors == 2 || neighbors == 3)) {
        newAlive.Add(cell);
        newOwner = cellOwners[cell];
      } else if (!alive && neighbors == 3) {
        newAlive.Add(cell);
        newOwner = (count1 > count2) ? 1 : (count2 > count1 ? 2 : Random.Range(1, 3));
        if (newOwner == 1) {
          ++scorePlayer1;
        } else {
          ++scorePlayer2;
        }
      }
      if (newOwner != 0) {
        newOwners[cell] = newOwner;
      }
    }
    bool noChange = newAlive.SetEquals(aliveCells);
    bool isCycle = false;
    foreach (var prev in history) {
      if (prev.SetEquals(newAlive)) {
        isCycle = true;
        break;
      }
    }
    currentState.ClearAllTiles();
    foreach (var kvp in newOwners) {
      currentState.SetTile(kvp.Key, kvp.Value == 1 ? aliveTilePlayer1 : aliveTilePlayer2);
    }
    aliveCells = newAlive;
    cellOwners = newOwners;
    history.Insert(0, new HashSet<Vector3Int>(aliveCells));
    if (history.Count > historyLimit) history.RemoveAt(history.Count - 1);
    if (aliveCells.Count == 0 || noChange || isCycle) {
      isPaused = true;
      if (simulationCoroutine != null) {
        StopCoroutine(simulationCoroutine);
      }
      string resultText = (scorePlayer1 == scorePlayer2) ? "DRAW!" : (scorePlayer1 > scorePlayer2 ? "PLAYER 1 WINS!" : "PLAYER 2 WINS!");
      resultsUI.ShowResult(resultText);
      gameOver = true;
    }
    UpdateText();
  }

  private void ToggleCell(Vector3Int cell) {
    if (!aliveCells.Contains(cell)) {
      currentState.SetTile(cell, aliveTilePlayer1);
      aliveCells.Add(cell);
      cellOwners[cell] = 1;
    } else {
      currentState.SetTile(cell, null);
      aliveCells.Remove(cell);
      cellOwners.Remove(cell);
    }
  }

  private void PlacePatternAtCell(Vector3Int startCell) {
    Pattern pattern = availablePatterns[selectedPatternIndex];
    Vector2Int[] cells = pattern.cells;
    Vector2Int center = pattern.GetCenter();
    foreach (Vector2Int offset in cells) {
      Vector3Int cell = startCell + (Vector3Int)(offset - center);
      if (!InBounds(cell) || currentState.GetTile(cell) != null) {
        continue;
      }
      Tile tile = currentPlayer == 1 ? aliveTilePlayer1 : aliveTilePlayer2;
      currentState.SetTile(cell, tile);
      aliveCells.Add(cell);
      cellOwners[cell] = currentPlayer;
    }
    currentPlayer = currentPlayer == 1 ? 2 : 1;
    UpdateText();
  }

  public void SelectPattern(int index) {
    if (index >= 0 && index < availablePatterns.Length) {
      selectedPatternIndex = index;
    }
  }
}
