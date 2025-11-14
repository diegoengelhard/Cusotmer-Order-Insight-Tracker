using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Creativa.Web.Models;
using Microsoft.AspNetCore.Hosting;

namespace Creativa.Web.Services
{
    public class WebTrackerService
    {
        private readonly List<WebTrackerEntry> _entries;
        private readonly string _csvPath;
        private int _nextId;

        public WebTrackerService(IWebHostEnvironment env)
        {
            // Route path for data storage
            var dataRoot = Path.GetFullPath(
                Path.Combine(env.ContentRootPath, "..", "..", "data")
            );

            if (!Directory.Exists(dataRoot))
            {
                Directory.CreateDirectory(dataRoot);
            }

            _csvPath = Path.Combine(dataRoot, "webtracker.csv");

            // Load existing records or create a new file
            _entries = File.Exists(_csvPath)
                ? LoadEntries(_csvPath)
                : new List<WebTrackerEntry>();

            _nextId = _entries.Any() ? _entries.Max(e => e.Id) + 1 : 1;
        }

        public void Track(string urlRequest, string sourceIp)
        {
            var entry = new WebTrackerEntry
            {
                Id = _nextId++,
                UrlRequest = urlRequest,
                SourceIp = sourceIp,
                TimeOfAction = DateTime.UtcNow
            };

            _entries.Add(entry);

            // Persist immediately to CSV
            AppendToCsv(entry);
        }

        public IEnumerable<WebTrackerEntry> GetAll()
        {
            return _entries.AsReadOnly();
        }

        private List<WebTrackerEntry> LoadEntries(string path)
        {
            var lines = File.ReadAllLines(path);
            var entries = new List<WebTrackerEntry>();

            // Skip header if exists
            var dataLines = lines.Skip(1);

            foreach (var line in dataLines)
            {
                var parts = line.Split(',');
                if (parts.Length >= 4)
                {
                    entries.Add(new WebTrackerEntry
                    {
                        Id = int.Parse(parts[0]),
                        UrlRequest = parts[1],
                        SourceIp = parts[2],
                        TimeOfAction = DateTime.Parse(parts[3], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    });
                }
            }

            return entries;
        }

        private void AppendToCsv(WebTrackerEntry entry)
        {
            // If the file does not exist, write header
            if (!File.Exists(_csvPath))
            {
                File.WriteAllText(_csvPath, "Id,URLRequest,SourceIp,TimeOfAction\n");
            }

            // Append entry
            var line = $"{entry.Id},{entry.UrlRequest},{entry.SourceIp},{entry.TimeOfAction:O}\n";
            File.AppendAllText(_csvPath, line);
        }
    }
}