using System.Drawing;
using System.Windows.Forms;
using Tetris.Core;

namespace Tetris.app;

public partial class Form1 : Form
{
    private const int CellSize = 26;
    private const int BoardLeft = 20;
    private const int BoardTop = 20;

    private GameState _state = null!;
    private HighScoreRepository _repo = null!;
    private System.Windows.Forms.Timer _timer = null!;
    private int _highScore;
    private readonly HashSet<Keys> _heldKeys = new();
    private int _moveCooldown;
    private bool _aiEnabled;
    private readonly AiPlayer _ai = new();

    public Form1()
    {
        Text = "俄罗斯方块";
        ClientSize = new Size(BoardLeft * 2 + Board.Width * CellSize + 160,
                              BoardTop * 2 + Board.Height * CellSize + 20);
        DoubleBuffered = true;
        KeyPreview = true;

        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TetrisHomework", "highscore.xml");
        _repo = new HighScoreRepository(path);
        _highScore = _repo.Load();

        StartNewGame();
        SetupTimer();
    }

    private void SetupTimer()
    {
        _timer = new System.Windows.Forms.Timer();
        _timer.Interval = _state.TickIntervalMs;
        _timer.Tick += (_, _) => GameTick();
        _timer.Start();
    }

    private void StartNewGame()
    {
        _state = new GameState();
        _heldKeys.Clear();
        _timer?.Stop();
        if (_timer is not null)
        {
            _timer.Interval = _state.TickIntervalMs;
            _timer.Start();
        }
        Invalidate();
    }

    private void GameTick()
    {
        if (_state.IsGameOver || _state.IsPaused) return;

        if (_aiEnabled)
        {
            _ai.Step(_state);
            _state.HardDrop();
            Invalidate();
            return;
        }

        if (_moveCooldown > 0) _moveCooldown--;

        if (_heldKeys.Contains(Keys.Left) && _moveCooldown == 0)
        {
            _state.MoveLeft(); _moveCooldown = 3;
        }
        else if (_heldKeys.Contains(Keys.Right) && _moveCooldown == 0)
        {
            _state.MoveRight(); _moveCooldown = 3;
        }

        _state.Tick();

        if (_state.IsGameOver)
        {
            _timer.Stop();
            if (_state.Score > _highScore)
            {
                _highScore = _state.Score;
                _repo.Save(_highScore);
            }
        }
        else
        {
            int newInterval = _state.TickIntervalMs;
            if (_timer.Interval != newInterval) _timer.Interval = newInterval;
        }
        Invalidate();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.KeyCode)
        {
            case Keys.Left:
                if (!_aiEnabled && !_state.IsPaused && !_state.IsGameOver)
                {
                    _state.MoveLeft();
                    Invalidate();
                }
                break;
            case Keys.Right:
                if (!_aiEnabled && !_state.IsPaused && !_state.IsGameOver)
                {
                    _state.MoveRight();
                    Invalidate();
                }
                break;
            case Keys.Up:
                if (!_aiEnabled && !_state.IsPaused && !_state.IsGameOver) _state.Rotate();
                Invalidate();
                break;
            case Keys.Down:
                if (!_aiEnabled) { _state.Tick(); Invalidate(); }
                break;
            case Keys.Space:
                if (!_aiEnabled) { _state.HardDrop(); Invalidate(); }
                break;
            case Keys.P:
                _state.TogglePause();
                Invalidate();
                break;
            case Keys.A:
                _aiEnabled = !_aiEnabled;
                Invalidate();
                break;
            case Keys.R:
                StartNewGame();
                break;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        _heldKeys.Remove(e.KeyCode);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.FromArgb(30, 30, 30));

        // 棋盘背景
        using var boardBg = new SolidBrush(Color.FromArgb(45, 45, 45));
        g.FillRectangle(boardBg, BoardLeft, BoardTop, Board.Width * CellSize, Board.Height * CellSize);

        // 已固定方块
        for (int y = 0; y < Board.Height; y++)
            for (int x = 0; x < Board.Width; x++)
                if (_state.Board[x, y] is { } t)
                    DrawCell(g, BoardLeft + x * CellSize, BoardTop + y * CellSize, ColorFor(t));

        // 当前方块
        if (_state.Current is not null && !_state.IsGameOver)
        {
            foreach (var (cx, cy) in _state.Current.Cells)
            {
                int bx = _state.CurrentX + cx, by = _state.CurrentY + cy;
                if (by >= 0) DrawCell(g, BoardLeft + bx * CellSize, BoardTop + by * CellSize,
                                      ColorFor(_state.Current.Type));
            }
        }

        // 网格线
        using var gridPen = new Pen(Color.FromArgb(60, 60, 60));
        for (int x = 0; x <= Board.Width; x++)
            g.DrawLine(gridPen, BoardLeft + x * CellSize, BoardTop,
                       BoardLeft + x * CellSize, BoardTop + Board.Height * CellSize);
        for (int y = 0; y <= Board.Height; y++)
            g.DrawLine(gridPen, BoardLeft, BoardTop + y * CellSize,
                       BoardLeft + Board.Width * CellSize, BoardTop + y * CellSize);

        // 侧栏
        int sideX = BoardLeft + Board.Width * CellSize + 20;
        using var font = new Font("Consolas", 11);
        using var white = new SolidBrush(Color.White);
        using var yellow = new SolidBrush(Color.Gold);

        g.DrawString($"分数: {_state.Score:X}", font, yellow, sideX, BoardTop);
        g.DrawString($"最高: {_highScore:X}", font, white, sideX, BoardTop + 26);
        g.DrawString($"等级: {_state.Level}", font, white, sideX, BoardTop + 52);
        g.DrawString($"消行: {_state.LinesCleared}", font, white, sideX, BoardTop + 78);
        g.DrawString("下一个:", font, white, sideX, BoardTop + 110);

        // 下一个方块预览
        var next = Tetromino.Create(_state.NextType);
        foreach (var (cx, cy) in next.Cells)
            DrawCell(g, sideX + cx * CellSize, BoardTop + 140 + cy * CellSize, ColorFor(next.Type));

        g.DrawString("← → 移动", font, white, sideX, BoardTop + 240);
        g.DrawString("↑ 旋转", font, white, sideX, BoardTop + 262);
        g.DrawString("空格 硬降", font, white, sideX, BoardTop + 284);
        g.DrawString("P 暂停  A AI", font, white, sideX, BoardTop + 306);
        g.DrawString("R 重开", font, white, sideX, BoardTop + 328);

        if (_state.IsPaused)
            g.DrawString("已暂停", new Font("Consolas", 24, FontStyle.Bold), yellow,
                         BoardLeft + 60, BoardTop + Board.Height * CellSize / 2);
        if (_state.IsGameOver)
            g.DrawString("游戏结束", new Font("Consolas", 24, FontStyle.Bold),
                         new SolidBrush(Color.OrangeRed),
                         BoardLeft + 50, BoardTop + Board.Height * CellSize / 2);
        if (_aiEnabled)
            g.DrawString("[AI 模式]", font, new SolidBrush(Color.Lime),
                         BoardLeft + 60, BoardTop + Board.Height * CellSize / 2 - 40);
    }

    private static void DrawCell(Graphics g, int x, int y, Color color)
    {
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, x + 1, y + 1, CellSize - 2, CellSize - 2);
    }

    private static Color ColorFor(TetrominoType t) => t switch
    {
        TetrominoType.I => Color.Cyan,
        TetrominoType.O => Color.Yellow,
        TetrominoType.T => Color.MediumPurple,
        TetrominoType.S => Color.LimeGreen,
        TetrominoType.Z => Color.IndianRed,
        TetrominoType.J => Color.RoyalBlue,
        TetrominoType.L => Color.Orange,
        _ => Color.Gray
    };
}
