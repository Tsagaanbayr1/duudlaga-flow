using System.IO;
using System.Text.Json;
using DuudlagaFlow.Models;
using DuudlagaFlow.Utilities;

namespace DuudlagaFlow.Services;

public class HistoryStore
{
    private static readonly string HistoryDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        Constants.App.AppDataFolder);

    private static readonly string HistoryPath = Path.Combine(HistoryDir, Constants.App.HistoryFile);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private List<TranscriptionRecord> _records = new();
    private bool _loaded;

    public IReadOnlyList<TranscriptionRecord> AllRecords
    {
        get
        {
            EnsureLoaded();
            return _records.AsReadOnly();
        }
    }

    public void Add(TranscriptionRecord record)
    {
        EnsureLoaded();
        _records.Insert(0, record);
        Save();
    }

    public void Delete(Guid id)
    {
        EnsureLoaded();
        _records.RemoveAll(r => r.Id == id);
        Save();
    }

    public void DeleteAll()
    {
        _records.Clear();
        Save();
    }

    public IReadOnlyList<TranscriptionRecord> Search(string query)
    {
        EnsureLoaded();
        if (string.IsNullOrWhiteSpace(query))
            return _records.AsReadOnly();

        return _records
            .Where(r => r.Text.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        try
        {
            if (File.Exists(HistoryPath))
            {
                var json = File.ReadAllText(HistoryPath);
                _records = JsonSerializer.Deserialize<List<TranscriptionRecord>>(json, JsonOptions) ?? new();
            }
        }
        catch
        {
            _records = new();
        }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(HistoryDir);
            var json = JsonSerializer.Serialize(_records, JsonOptions);
            File.WriteAllText(HistoryPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryStore] Save failed: {ex.Message}");
        }
    }
}
