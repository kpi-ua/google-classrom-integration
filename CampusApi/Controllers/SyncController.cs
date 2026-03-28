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
                // googleCourseId - ID курса в гугле (например "s4ce34m")
                // employeeMonitoringId - ID ведомости в БД кампуса (cMonitoringEmployeeId)
                var result = await _syncService.SyncGradesAsync(googleCourseId, employeeMonitoringId);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetCourses([FromServices] IGoogleClassroomService googleApi)
        {
            var list = await googleApi.GetMyCoursesIdsAsync();
            return Ok(list);
        }
    }
}