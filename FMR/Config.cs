using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FMR
{
    internal class Config
    {
        // key
        public const string MidiPath = "midiPath";
        public const string OutputPath = "outputPath";
        public const string ColorPath = "colorPath";

        public const string ConfigurationFilePath = "config.json";

        internal string configPath;
        internal Dictionary<string, object> configurations;
        public Config() : this(ConfigurationFilePath)
        {

        }
        public Config(string path)
        {
            configPath = path;
            configurations = new()
            {
                { "midiPath", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) },
                { "outputPath", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) },
                { "colorPath", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) }
            };
            if (File.Exists(path))
            {
                string txt = File.ReadAllText(path);
                Dictionary<string, object>? parsedConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(txt);
                if (parsedConfig != null)
                {
                    configurations = parsedConfig;
                }
            }
            else
            {
                using FileStream fs = File.Create(path);
                JsonSerializer.Serialize(fs, configurations);
            }
        }
        public void Save()
        {
            File.WriteAllText(configPath, JsonSerializer.Serialize(configurations));
        }
        public void Update(string key, object? value)
        {
            if (value is not null)
            {
                configurations[key] = value;
            }
            Save();
        }
        public object Get(string key)
        {
            if (configurations.TryGetValue(key, out object? value))
            {
                return value;
            }
            return string.Empty;
        }
    }
}
