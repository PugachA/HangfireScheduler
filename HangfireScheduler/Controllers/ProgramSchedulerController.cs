using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using HangfireScheduler.DTO;
using HangfireScheduler.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HangfireScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramSchedulerController : ControllerBase
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly ProgramRepository _programRepository;
        private readonly UserRepository _userRepository;
        private readonly ILogger<ProgramSchedulerController> _logger;

        public ProgramSchedulerController(ProgramRepository programRepository, UserRepository userRepository, IRecurringJobManager recurringJobManager, ILogger<ProgramSchedulerController> logger)
        {
            _recurringJobManager = recurringJobManager;
            _programRepository = programRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        // GET: api/<ProgrammSchedulerController>
        [HttpGet]
        public ActionResult<IEnumerable<ProgramDto>> GetAllProgramJobs()
        {
            _logger.LogInformation($"Поступил запрос на получение всех задач");

            var startRegex = new Regex(@"^Program-.+-Start$");
            var startProgramRecurrentJobs = JobStorage.Current.GetConnection().GetRecurringJobs().Where(j => startRegex.IsMatch(j.Id));

            var stopRegex = new Regex(@"^Program-.+-Stop$");
            var stopProgramRecurrentJobs = JobStorage.Current.GetConnection().GetRecurringJobs().Where(j => stopRegex.IsMatch(j.Id));

            var programDtoList = new List<ProgramDto>();
            foreach (var startJob in startProgramRecurrentJobs)
            {
                var stopJob = stopProgramRecurrentJobs.SingleOrDefault(s => s.Id == startJob.Id.Replace("-Start", "-Stop"));

                programDtoList.Add(ConvertToProgramDto(startJob, stopJob));
            }

            return Ok(programDtoList);
        }

        // POST api/<ProgrammSchedulerController>
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateProgramJob([FromBody] ProgramDto programDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Валидация модели не успешна {ModelState}");
                return BadRequest(ModelState);
            }

            _logger.LogInformation($"Поступил запрос на добавление программы {JsonSerializer.Serialize(programDto)}");

            var user = _userRepository.Get(programDto.UserName);

            var password = new NetworkCredential("", "Dub123456").SecurePassword;

            string startId = $"Program-{programDto.Name}-Start";
            _recurringJobManager.AddOrUpdate(
                    startId,
                    () => Process.Start("C:\\Program Files\\Notepad++\\notepad++.exe", "WebScraper", password, "DESKTOP-Q3JCQH4"),
                    programDto.StartScheduler,
                    TimeZoneInfo.Local);

            string stopId = $"Program-{programDto.Name}-Stop";
            _recurringJobManager.AddOrUpdate(
                    stopId,
                    () => (new Test()).StopProgram(programDto.Name),
                    programDto.StopScheduler,
                    TimeZoneInfo.Local);

            return Ok();
        }

        // PUT api/<ProgrammSchedulerController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }


        // DELETE api/<ProgrammSchedulerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private ProgramDto ConvertToProgramDto(RecurringJobDto startJob, RecurringJobDto stopJob)
        {
            if (startJob == null)
                throw new ArgumentNullException($"Argument {nameof(startJob)} can not be null");

            var name = ExtractProgramName(startJob.Id);
            var programSettings = _programRepository.Get(name);

            return new ProgramDto
            {
                Name = name,
                Path = programSettings.Path,
                UserName = programSettings.UserName,
                StartScheduler = startJob.Cron,
                StopScheduler = stopJob.Cron
            };
        }

        private string ExtractProgramName(string startJobId)
        {
            var nameRegex = new Regex(@"^(?:Program-)(.+)(?:-Start)$");

            var match = nameRegex.Match(startJobId);

            if (match.Groups.Count != 2)
                throw new ArgumentException($"Incorrect number of groups {match.Groups.Count} found for id={startJobId}. Valid regex {nameRegex}");

            return match.Groups[1].Value;
        }

        private SecureString ConvertToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            var securePassword = new SecureString();

            foreach (char c in password)
                securePassword.AppendChar(c);

            return securePassword;
        }
    }
}
