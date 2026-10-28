using Tetris.Core;
using Xunit;

namespace Tetris.Core.Tests;

public class HighScoreRepositoryTests
{
    [Fact]
    public void Load_WhenFileMissing_ReturnsZero()
    {
        var directory = Directory.CreateTempSubdirectory();

        try
        {
            var path = Path.Combine(directory.FullName, "highscore.xml");
            var repository = new HighScoreRepository(path);

            Assert.Equal(0, repository.Load());
        }
        finally
        {
            directory.Delete(true);
        }
    }   
    [Fact]
    public void Load_WhenXmlIsCorrupt_ReturnsZero()
    {
        var directory = Directory.CreateTempSubdirectory();

        try
        {
            var path = Path.Combine(directory.FullName, "highscore.xml");
            File.WriteAllText(path, "<HighScore><Value>broken");

            var repository = new HighScoreRepository(path);

            Assert.Equal(0, repository.Load());
        }
        finally
        {
            directory.Delete(true);
        }
    }[Fact]
public void SaveThenLoad_ReturnsSavedScore()
{
    var directory = Directory.CreateTempSubdirectory();

    try
    {
        var path = Path.Combine(directory.FullName, "highscore.xml");
        var repository = new HighScoreRepository(path);

        repository.Save(1234);

        Assert.Equal(1234, repository.Load());
    }
    finally
    {
        directory.Delete(true);
    }
}
}