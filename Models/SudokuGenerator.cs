namespace PugSudoku.Models;

public static class SudokuGenerator
{
    public static (int[,] solution, int[,] puzzle) Generate(string difficulty)
    {
        var solution = new int[9, 9];
        FillBoard(solution);

        var puzzle = (int[,])solution.Clone();
        int remove = difficulty switch
        {
            "easy" => 36,
            "hard" => 54,
            _ => 45,
        };

        var cells = new List<(int r, int c)>();
        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
            cells.Add((r, c));

        Shuffle(cells);

        int removed = 0;
        foreach (var (r, c) in cells)
        {
            if (removed >= remove) break;
            int backup = puzzle[r, c];
            puzzle[r, c] = 0;
            if (CountSolutions(puzzle) == 1)
                removed++;
            else
                puzzle[r, c] = backup;
        }

        return (solution, puzzle);
    }

    private static bool FillBoard(int[,] board)
    {
        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
        {
            if (board[r, c] != 0) continue;
            var nums = Enumerable.Range(1, 9).ToList();
            Shuffle(nums);
            foreach (int n in nums)
            {
                if (!IsValid(board, r, c, n)) continue;
                board[r, c] = n;
                if (FillBoard(board)) return true;
                board[r, c] = 0;
            }
            return false;
        }
        return true;
    }

    private static bool IsValid(int[,] board, int row, int col, int num)
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[row, i] == num) return false;
            if (board[i, col] == num) return false;
        }
        int br = row / 3 * 3, bc = col / 3 * 3;
        for (int r = br; r < br + 3; r++)
        for (int c = bc; c < bc + 3; c++)
            if (board[r, c] == num) return false;
        return true;
    }

    private static int CountSolutions(int[,] board, int limit = 2)
    {
        int count = 0;
        Solve(board, ref count, limit);
        return count;
    }

    private static bool Solve(int[,] board, ref int count, int limit)
    {
        for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
        {
            if (board[r, c] != 0) continue;
            for (int n = 1; n <= 9; n++)
            {
                if (!IsValid(board, r, c, n)) continue;
                board[r, c] = n;
                if (Solve(board, ref count, limit)) return true;
                board[r, c] = 0;
            }
            return false;
        }
        count++;
        return count >= limit;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
