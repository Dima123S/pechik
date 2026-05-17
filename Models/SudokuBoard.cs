namespace PugSudoku.Models;

public class SudokuBoard
{
    public int[,] Solution { get; } = new int[9, 9];
    public int[,] Puzzle { get; } = new int[9, 9];
    public int[,] Current { get; } = new int[9, 9];
    public bool[,] IsFixed { get; } = new bool[9, 9];
    public HashSet<int>[,] Pencil { get; } = new HashSet<int>[9, 9];
    public int HintsUsed { get; set; }
    public int ErrorCount { get; set; }

    public SudokuBoard()
    {
        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
            Pencil[r, c] = new HashSet<int>();
    }

    public void Setup(int[,] solution, int[,] puzzle)
    {
        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
        {
            Solution[r, c] = solution[r, c];
            Puzzle[r, c] = puzzle[r, c];
            Current[r, c] = puzzle[r, c];
            IsFixed[r, c] = puzzle[r, c] != 0;
            Pencil[r, c].Clear();
        }
        HintsUsed = 0;
        ErrorCount = 0;
    }

    public bool Place(int r, int c, int val)
    {
        if (IsFixed[r, c]) return false;
        Current[r, c] = val;
        Pencil[r, c].Clear();
        if (val != 0 && val != Solution[r, c])
        {
            ErrorCount++;
            return false;
        }
        return true;
    }

    public void Erase(int r, int c)
    {
        if (IsFixed[r, c]) return;
        Current[r, c] = 0;
    }

    public void TogglePencil(int r, int c, int val)
    {
        if (IsFixed[r, c] || Current[r, c] != 0) return;
        if (!Pencil[r, c].Remove(val))
            Pencil[r, c].Add(val);
    }

    public bool RevealHint()
    {
        var empties = new List<(int r, int c)>();
        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
            if (Current[r, c] != Solution[r, c])
                empties.Add((r, c));

        if (empties.Count == 0) return false;
        var (hr, hc) = empties[Random.Shared.Next(empties.Count)];
        Current[hr, hc] = Solution[hr, hc];
        IsFixed[hr, hc] = true;
        Pencil[hr, hc].Clear();
        HintsUsed++;
        return true;
    }

    public bool IsSolved()
    {
        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
            if (Current[r, c] != Solution[r, c]) return false;
        return true;
    }

    public bool HasConflict(int r, int c)
    {
        int v = Current[r, c];
        if (v == 0) return false;
        for (int i = 0; i < 9; i++)
        {
            if (i != c && Current[r, i] == v) return true;
            if (i != r && Current[i, c] == v) return true;
        }
        int br = r / 3 * 3, bc = c / 3 * 3;
        for (int dr = 0; dr < 3; dr++)
        for (int dc = 0; dc < 3; dc++)
        {
            int nr = br + dr, nc = bc + dc;
            if (nr != r || nc != c)
                if (Current[nr, nc] == v) return true;
        }
        return false;
    }

    public int EmptyCount()
    {
        int count = 0;
        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
            if (Current[r, c] == 0) count++;
        return count;
    }
}
