using HangfireScheduler.DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HangfireScheduler.Models
{
    public class ProgramRepository : IDisposable
    {
        private readonly Mutex _mutex;
        private readonly Dictionary<string, ProgramDto> _nameProgramDictionary;
        private readonly string _programsSettingsPath;
        public ProgramRepository(IConfiguration configuration)
        {
            _mutex = new Mutex();
            _programsSettingsPath = Path.Combine(AppContext.BaseDirectory, "programsSettings.json");

            if (!File.Exists(_programsSettingsPath))
                throw new FileNotFoundException($"File {_programsSettingsPath} not find");

            var programDtoList = configuration.GetSection("Programs").Get<List<ProgramDto>>();
            _nameProgramDictionary = ConvertToDictionary(programDtoList);
        }

        private Dictionary<string, ProgramDto> ConvertToDictionary(List<ProgramDto> programDtoList)
        {
            if (programDtoList == null)
                throw new ArgumentNullException($"Argument {nameof(programDtoList)} can not be null");

            if (!programDtoList.Any())
                throw new ArgumentException($"Argument {nameof(programDtoList)} can not be empty");

            var nameProgramDictionary = new Dictionary<string, ProgramDto>(StringComparer.OrdinalIgnoreCase);
            foreach (var programDto in programDtoList)
            {
                if (nameProgramDictionary.ContainsKey(programDto.Name))
                    throw new ArgumentException($"Record with the key {programDto.Name} is already there. {nameof(programDto.Name)} must be unique");

                nameProgramDictionary.Add(programDto.Name, programDto);
            }

            return nameProgramDictionary;
        }

        public void AddOrUpdate(ProgramDto programDto)
        {
            try
            {
                _mutex.WaitOne();

                if (_nameProgramDictionary.ContainsKey(programDto.Name))
                    _nameProgramDictionary[programDto.Name] = programDto;
                else
                    _nameProgramDictionary.Add(programDto.Name, programDto);

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

