using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace HangfireScheduler.Models
{
    public class ProgramRepository : IDisposable, IRepository<ProgramSettings>
    {
        private readonly Mutex _mutex;
        private readonly Dictionary<string, ProgramSettings> _nameProgramDictionary;
        private readonly string _programsSettingsPath;

        public ProgramRepository(IConfiguration configuration)
        {
            _mutex = new Mutex();
            _programsSettingsPath = Path.Combine(AppContext.BaseDirectory, "programsSettings.json");

            if (!File.Exists(_programsSettingsPath))
                throw new FileNotFoundException($"File {_programsSettingsPath} not found");

            var programSettingsList = configuration.GetSection("Programs").Get<List<ProgramSettings>>();
            _nameProgramDictionary = ConvertToDictionary(programSettingsList);
        }

        private Dictionary<string, ProgramSettings> ConvertToDictionary(List<ProgramSettings> programSettingsList)
        {
            if (programSettingsList == null)
                throw new ArgumentNullException($"Argument {nameof(programSettingsList)} can not be null");

            if (!programSettingsList.Any())
                throw new ArgumentException($"Argument {nameof(programSettingsList)} can not be empty");

            var nameProgramDictionary = new Dictionary<string, ProgramSettings>(StringComparer.OrdinalIgnoreCase);
            foreach (var programSettings in programSettingsList)
            {
                if (nameProgramDictionary.ContainsKey(programSettings.Name))
                    throw new ArgumentException($"Record with the key {programSettings.Name} is already there. {nameof(programSettings.Name)} must be unique");

                nameProgramDictionary.Add(programSettings.Name, programSettings);
            }

            return nameProgramDictionary;
        }

        public ProgramSettings Get(string name)
        {
            if (!_nameProgramDictionary.TryGetValue(name, out ProgramSettings value))
                throw new KeyNotFoundException($"{nameof(ProgramSettings)} not found by key {name}");

            return value;
        }

        public void AddOrUpdate(ProgramSettings programSettings)
        {
            try
            {
                _mutex.WaitOne();

                if (_nameProgramDictionary.ContainsKey(programSettings.Name))
                    _nameProgramDictionary[programSettings.Name] = programSettings;
                else
                    _nameProgramDictionary.Add(programSettings.Name, programSettings);

                var programDtoList = _nameProgramDictionary.Values.AsEnumerable();

                File.WriteAllText(_programsSettingsPath, JsonSerializer.Serialize(programDtoList));
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public void Delete(string name)
        {
            try
            {
                _mutex.WaitOne();

                if (!_nameProgramDictionary.ContainsKey(name))
                    throw new KeyNotFoundException($"Could not find key {name} to delete");

                _nameProgramDictionary.Remove(name);

                var programDtoList = _nameProgramDictionary.Values.AsEnumerable();

                File.WriteAllText(_programsSettingsPath, JsonSerializer.Serialize(programDtoList));
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public void Dispose()
        {
            _mutex?.Dispose();
        }
    }
}

