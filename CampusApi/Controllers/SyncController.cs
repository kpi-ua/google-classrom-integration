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
        public async Task<IActionResult> SyncClassroom([FromQuery] string googleCourseId, [FromQuery] int employeeMonitoringId)
        {
            try
            {
                // googleCourseId - ID або Base64 посилання курсу
                // employeeMonitoringId - ID відомості у БД кампуса
                var result = await _syncService.SyncGradesAsync(googleCourseId, employeeMonitoringId);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}