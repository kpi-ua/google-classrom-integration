using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CampusApi.Services
{
    // Модель для backward compatibility старої логіки оцінок.
    public class StudentGradeItem
    {
        public string AssignmentTitle { get; set; }
        public float Mark { get; set; }
    }

    public class ClassroomSubmissionInfo
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public float? AssignedGrade { get; set; }
    }

    public class ClassroomCourseWorkInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<ClassroomSubmissionInfo> Submissions { get; set; } = new();
    }

    public class ClassroomStudentInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ClassroomCourseSyncData
    {
        public string CourseId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<ClassroomStudentInfo> Students { get; set; } = new();
        public List<ClassroomCourseWorkInfo> CourseWorks { get; set; } = new();
    }

    public class ClassroomCourseInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public interface IGoogleClassroomService
    {
        Task<ClassroomCourseSyncData> GetCourseSyncDataAsync(string courseId);
        Task<Dictionary<string, List<StudentGradeItem>>> GetAllGradesForCourseAsync(string courseId);
        Task<List<string>> GetTeacherEmailsForCourseAsync(string courseId);
        Task<List<ClassroomCourseInfo>> GetMyCoursesAsync();
    }

    public class GoogleClassroomService : IGoogleClassroomService
    {
        // Необхідні дозволи (Scopes) для читання оцінок та пошт студентів
        private static readonly string[] Scopes = {
            ClassroomService.Scope.ClassroomCourseworkStudentsReadonly,
            ClassroomService.Scope.ClassroomRostersReadonly,
            ClassroomService.Scope.ClassroomCoursesReadonly,
            ClassroomService.Scope.ClassroomProfileEmails
        };

        // Метод для автоматичного розшифрування Base64 посилань у цифрові ID
        private string DecodeCourseIdIfNeeded(string courseId)
        {
            var normalized = NormalizeCourseIdInput(courseId);
            if (normalized.All(char.IsDigit)) return normalized; // Якщо вже цифри - віддаємо як є

            try
            {
                string base64 = normalized.Replace('-', '+').Replace('_', '/');
                switch (base64.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }
                byte[] data = Convert.FromBase64String(base64);
                string decoded = System.Text.Encoding.UTF8.GetString(data);
                if (decoded.All(char.IsDigit)) return decoded;
            }
            catch { }

            return normalized;
        }

        private string NormalizeCourseIdInput(string courseId)
        {
            var input = courseId.Trim();
            if (string.IsNullOrWhiteSpace(input)) return input;

            // Supports full links like https://classroom.google.com/c/<token>?...
            if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                var cIndex = Array.FindIndex(segments, s => string.Equals(s, "c", StringComparison.OrdinalIgnoreCase));
                if (cIndex >= 0 && cIndex + 1 < segments.Length)
                {
                    return segments[cIndex + 1];
                }
            }

            return input;
        }

        private async Task<ClassroomService> CreateServiceAsync()
        {
            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes, "user", CancellationToken.None, new FileDataStore("TokenStore", true));
            }

            return new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Campus Sync API"
            });
        }

        public async Task<ClassroomCourseSyncData> GetCourseSyncDataAsync(string courseId)
        {
            courseId = DecodeCourseIdIfNeeded(courseId);
            var service = await CreateServiceAsync();

            var course = await service.Courses.Get(courseId).ExecuteAsync();

            var result = new ClassroomCourseSyncData
            {
                CourseId = courseId,
                Name = course?.Name ?? string.Empty
            };

            var studentsResponse = await service.Courses.Students.List(courseId).ExecuteAsync();
            if (studentsResponse.Students != null)
            {
                foreach (var student in studentsResponse.Students)
                {
                    if (!string.IsNullOrWhiteSpace(student.UserId))
                    {
                        result.Students.Add(new ClassroomStudentInfo
                        {
                            UserId = student.UserId,
                            Email = student.Profile?.EmailAddress?.Trim().ToLower() ?? string.Empty
                        });
                    }
                }
            }

            var courseWorkResponse = await service.Courses.CourseWork.List(courseId).ExecuteAsync();
            if (courseWorkResponse.CourseWork == null)
            {
                return result;
            }

            foreach (var work in courseWorkResponse.CourseWork)
            {
                if (string.IsNullOrWhiteSpace(work.Id))
                {
                    continue;
                }

                var workInfo = new ClassroomCourseWorkInfo
                {
                    Id = work.Id,
                    Title = work.Title ?? string.Empty
                };

                var subResponse = await service.Courses.CourseWork.StudentSubmissions.List(courseId, work.Id).ExecuteAsync();
                if (subResponse.StudentSubmissions != null)
                {
                    foreach (var sub in subResponse.StudentSubmissions)
                    {
                        if (string.IsNullOrWhiteSpace(sub.Id) || string.IsNullOrWhiteSpace(sub.UserId))
                        {
                            continue;
                        }

                        workInfo.Submissions.Add(new ClassroomSubmissionInfo
                        {
                            Id = sub.Id,
                            UserId = sub.UserId,
                            State = sub.State ?? string.Empty,
                            AssignedGrade = sub.AssignedGrade.HasValue ? (float)sub.AssignedGrade.Value : null
                        });
                    }
                }

                result.CourseWorks.Add(workInfo);
            }

            return result;
        }

        public async Task<Dictionary<string, List<StudentGradeItem>>> GetAllGradesForCourseAsync(string courseId)
        {
            var syncData = await GetCourseSyncDataAsync(courseId);

            var studentGrades = new Dictionary<string, List<StudentGradeItem>>();
            var userEmailMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var student in syncData.Students)
            {
                if (!string.IsNullOrWhiteSpace(student.UserId) && !string.IsNullOrWhiteSpace(student.Email))
                {
                    userEmailMap[student.UserId] = student.Email;
                    studentGrades[student.Email] = new List<StudentGradeItem>();
                }
            }

            foreach (var work in syncData.CourseWorks)
            {
                foreach (var sub in work.Submissions)
                {
                    if (sub.State == "RETURNED" && sub.AssignedGrade.HasValue && userEmailMap.TryGetValue(sub.UserId, out var email))
                    {
                        studentGrades[email].Add(new StudentGradeItem
                        {
                            AssignmentTitle = work.Title,
                            Mark = sub.AssignedGrade.Value
                        });
                    }
                }
            }

            return studentGrades;
        }

        public async Task<List<string>> GetTeacherEmailsForCourseAsync(string courseId)
        {
            courseId = DecodeCourseIdIfNeeded(courseId);
            var service = await CreateServiceAsync();

            var response = await service.Courses.Teachers.List(courseId).ExecuteAsync();
            var result = new List<string>();

            if (response.Teachers == null)
            {
                return result;
            }

            foreach (var teacher in response.Teachers)
            {
                var email = teacher.Profile?.EmailAddress;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    result.Add(email.Trim().ToLower());
                }
            }

            return result;
        }

        public async Task<List<ClassroomCourseInfo>> GetMyCoursesAsync()
        {
            var service = await CreateServiceAsync();

            var request = service.Courses.List();
            request.PageSize = 100;

            var response = await request.ExecuteAsync();
            var result = new List<ClassroomCourseInfo>();

            if (response.Courses == null)
            {
                return result;
            }

            foreach (var course in response.Courses)
            {
                if (!string.IsNullOrWhiteSpace(course.Id) && !string.IsNullOrWhiteSpace(course.Name))
                {
                    result.Add(new ClassroomCourseInfo
                    {
                        Id = course.Id,
                        Name = course.Name
                    });
                }
            }

            return result;
        }
    }
}