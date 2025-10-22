using UnityEngine;

[CreateAssetMenu(menuName = "Game of Life/Pattern")]
public class Pattern : ScriptableObject {
  public Vector2Int[] cells;
  public int randomFillPercent = 9;

  public Vector2Int GetCenter() {
    if (cells == null || cells.Length == 0) {
			return Vector2Int.zero;
		}
    Vector2Int min = cells[0];
    Vector2Int max = cells[0];
    foreach (var cell in cells) {
      min.x = Mathf.Min(min.x, cell.x);
      min.y = Mathf.Min(min.y, cell.y);
      max.x = Mathf.Max(max.x, cell.x);
      max.y = Mathf.Max(max.y, cell.y);
    }
		return (min + max) / 2;
  }

  public void GenerateRandom(Vector2Int min, Vector2Int max) {
    var newCells = new System.Collections.Generic.List<Vector2Int>();
    for (int x = min.x; x < max.x; ++x) {
      for (int y = min.y; y < max.y; ++y) {
      	if (Random.Range(0, 100) < randomFillPercent) {
          newCells.Add(new Vector2Int(x, y));
        }
      }
    }
    cells = newCells.ToArray();
  }

  public Vector2Int[] GetCellsCopy() {
    Vector2Int[] copy = new Vector2Int[cells.Length];
    cells.CopyTo(copy, 0);
    return copy;
  }
}