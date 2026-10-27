using System.Xml.Serialization;

namespace Tetris.Core;

[XmlRoot("HighScore")]
public sealed class HighScoreData
{
    [XmlElement("Value")]
    public int Value { get; set; }
}

public sealed class HighScoreRepository
{
    private readonly string _path;

    public HighScoreRepository(string path) => _path = path;

    public int Load()
    {
        try
        {
            if (!File.Exists(_path)) return 0;
            using var stream = File.OpenRead(_path);
            var serializer = new XmlSerializer(typeof(HighScoreData));
            var data = serializer.Deserialize(stream) as HighScoreData;
            return data?.Value ?? 0;
        }
        catch
        {
            // 文件损坏、被占用、权限不足、格式错误 → 一律回退 0
            return 0;
        }
    }

    public void Save(int score)
    {
        string tmp = _path + ".tmp";
        try
        {
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            using (var stream = File.Create(tmp))
            {
                var serializer = new XmlSerializer(typeof(HighScoreData));
                serializer.Serialize(stream, new HighScoreData { Value = score });
            }
            File.Move(tmp, _path, overwrite: true);
        }
        catch
        {
            // 写入失败不影响游戏继续
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
        }
    }
}