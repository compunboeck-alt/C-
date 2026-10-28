namespace Tetris.Core;

/// <summary>
/// 启发式 AI：枚举每个旋转 × 落点，模拟落底，评估高度/空洞/消行，选最优。
/// 不依赖 UI，可单独测试。
/// </summary>
public sealed class AiPlayer
{
    /// <summary>
    /// 枚举当前方块的旋转状态和可能的横坐标，选择评价分最高的落点。
    /// </summary>
    public (int Rotation, int X) Decide(GameState state)
    {
        var current = state.Current!;
        double bestScore = double.MinValue;
        var best = (Rotation: 0, X: state.CurrentX);

        for (int rot = 0; rot < 4; rot++)
        {
            var shape = current.Clone();
            shape.SetRotation(rot);

            for (int x = -3; x < Board.Width + 3; x++)
            {
                if (!state.CanPlace(shape, x, state.CurrentY)) continue;
                int y = state.CurrentY;
                while (state.CanPlace(shape, x, y + 1)) y++;

                var (score, _) = Evaluate(state, shape, x, y);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = (rot, x);
                }
            }
        }
        return best;
    }

    /// <summary>执行一步 AI 决策：旋转到目标，再左右移到目标列。返回是否成功。</summary>
    /// <summary>执行一次 AI 决策，将方块旋转并移动到目标位置。</summary>
    public bool Step(GameState state)
    {
        var (targetRot, targetX) = Decide(state);
        bool moved = false;
        for (int i = 0; i < 4 && state.Current!.Rotation != targetRot; i++)
        {
            if (state.Rotate()) moved = true;
            else break;
        }
        while (state.CurrentX < targetX) { if (!state.MoveRight()) break; moved = true; }
        while (state.CurrentX > targetX) { if (!state.MoveLeft()) break; moved = true; }
        return moved;
    }

    /// <summary>
    /// 评估模拟落底后的棋盘，分数越高表示棋盘越平整、空洞越少、消行越多。
    /// </summary>
    private (double Score, int Lines) Evaluate(GameState state, Tetromino shape, int x, int y)
    {
        // 在副本上模拟落底后的棋盘
        var heights = new int[Board.Width];
        var holes = 0;
        var filled = new bool[Board.Height, Board.Width];

        for (int bx = 0; bx < Board.Width; bx++)
            for (int by = 0; by < Board.Height; by++)
                filled[by, bx] = state.Board[bx, by] is not null;

        foreach (var (cx, cy) in shape.Cells)
        {
            int bx = x + cx, by = y + cy;
            if (by >= 0 && by < Board.Height && bx >= 0 && bx < Board.Width)
                filled[by, bx] = true;
        }

        for (int bx = 0; bx < Board.Width; bx++)
        {
            int h = 0;
            bool foundBlock = false;
            for (int by = 0; by < Board.Height; by++)
            {
                if (filled[by, bx])
                {
                    if (!foundBlock) { foundBlock = true; h = Board.Height - by; }
                }
                else if (foundBlock) holes++;
            }
            heights[bx] = h;
        }

        int aggregateHeight = heights.Sum();
        int bumpiness = 0;
        for (int i = 0; i < Board.Width - 1; i++)
            bumpiness += Math.Abs(heights[i] - heights[i + 1]);

        int lines = 0;
        for (int by = 0; by < Board.Height; by++)
        {
            bool full = true;
            for (int bx = 0; bx < Board.Width; bx++)
                if (!filled[by, bx]) { full = false; break; }
            if (full) lines++;
        }

        double score = -0.51 * aggregateHeight - 0.36 * holes - 0.18 * bumpiness + 0.76 * lines;
        return (score, lines);
    }
}
