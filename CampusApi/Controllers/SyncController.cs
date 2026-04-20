using Microsoft.AspNetCore.Mvc;
using CampusApi.Services;

namespace CampusApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly GradeSyncService _syncService;

        public SyncController(GradeSyncService syncService)
        {
            _syncService = syncService;
        }

        // POST: api/sync/google-classroom
        [HttpPost("google-classroom")]
        public async Task<IActionResult> SyncClassroom(
            [FromQuery] string googleCourseId,
            [FromQuery] int? employeeMonitoringId,
            [FromQuery] string? teacherEmail,
            [FromQuery] int? cRNPRowId,
            [FromQuery] int? rtStudyGroupId,
            [FromQuery] int? semesterYear)
        {
            try
            {
                // Сценарій 1: ID відомості викладача вже відомий.
                if (employeeMonitoringId.HasValue)
                {
                    var resultById = await _syncService.SyncGradesAsync(googleCourseId, employeeMonitoringId.Value);
                    return Ok(new { message = resultById });
                }

                // Сценарій 2: шукаємо/створюємо зв'язку викладача автоматично.
                var result = await _syncService.SyncGradesAsync(
                    googleCourseId,
                    teacherEmail,
                    cRNPRowId,
                    rtStudyGroupId,
                    semesterYear);

                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}