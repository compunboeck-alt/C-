using Tetris.Core;
using Xunit;

namespace Tetris.Core.Tests;

public class AiPlayerTests
{
    [Fact]
    public void Decide_ReturnsLegalPlacement()
    {
        var state = new GameState(seed: 1);
        var ai = new AiPlayer();

        var (rotation, x) = ai.Decide(state);

        var shape = state.Current!.Clone();
        shape.SetRotation(rotation);

        Assert.True(state.CanPlace(shape, x, state.CurrentY));
    }
}