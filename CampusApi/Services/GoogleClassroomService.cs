using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CampusApi.Services
{
    public interface IGoogleClassroomService
    {
        Task<Dictionary<string, float>> GetTotalGradesForCourseAsync(string courseId);
        Task<List<string>> GetMyCoursesIdsAsync();

    }

    public class GoogleClassroomService : IGoogleClassroomService
    {
        private static readonly string[] Scopes = {
            ClassroomService.Scope.ClassroomCourseworkStudentsReadonly,
            ClassroomService.Scope.ClassroomRostersReadonly,
            ClassroomService.Scope.ClassroomCoursesReadonly
        };

        public async Task<Dictionary<string, float>> GetTotalGradesForCourseAsync(string courseId)
        {
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // ДОБАВИЛ НОВЫЙ SCOPE: ClassroomProfileEmails (чтобы Гугл точно отдал почты)
                string[] scopes = {
            ClassroomService.Scope.ClassroomCourseworkStudentsReadonly,
            ClassroomService.Scope.ClassroomRostersReadonly,
            ClassroomService.Scope.ClassroomCoursesReadonly,
            ClassroomService.Scope.ClassroomProfileEmails
        };

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes, "user", CancellationToken.None, new FileDataStore("TokenStore", true));
            }

            var service = new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Campus Sync API",
            });

            var studentGrades = new Dictionary<string, float>();
            var userEmailMap = new Dictionary<string, string>();

            // 1. Получаем список студентов
            var studentsResponse = await service.Courses.Students.List(courseId).ExecuteAsync();

            if (studentsResponse.Students != null)
            {
                foreach (var student in studentsResponse.Students)
                {
                    // ЗАЩИТА: проверяем, что UserId и Email не пустые
                    string userId = student.UserId;
                    string email = student.Profile?.EmailAddress;

                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(email))
                    {
                        userEmailMap[userId] = email;
                        studentGrades[email] = 0; // Начинаем с 0 баллов
                    }
                }
            }

            // 2. Получаем все задания курса
            var courseWorkResponse = await service.Courses.CourseWork.List(courseId).ExecuteAsync();

            if (courseWorkResponse.CourseWork != null)
            {
                // 3. Вытягиваем оценки по каждому заданию
                foreach (var work in courseWorkResponse.CourseWork)
                {
                    var submissionsResponse = await service.Courses.CourseWork.StudentSubmissions.List(courseId, work.Id).ExecuteAsync();

                    if (submissionsResponse.StudentSubmissions != null)
                    {
                        foreach (var sub in submissionsResponse.StudentSubmissions)
                        {
                            // ЗАЩИТА: проверяем, что UserId есть и преподаватель ВЕРНУЛ оценку (AssignedGrade)
                            if (!string.IsNullOrEmpty(sub.UserId) && sub.AssignedGrade.HasValue)
                            {
                                if (userEmailMap.TryGetValue(sub.UserId, out var email) && !string.IsNullOrEmpty(email))
                                {
                                    studentGrades[email] += (float)sub.AssignedGrade.Value;
                                }
                            }
                        }
                    }
                }
            }

            return studentGrades;
        }
        public async Task<List<string>> GetMyCoursesIdsAsync()
        {
            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes, "user", CancellationToken.None, new FileDataStore("TokenStore", true));
            }

            var service = new ClassroomService(new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "Campus Sync API" });
            var response = await service.Courses.List().ExecuteAsync();

            var list = new List<string>();
            if (response.Courses != null)
            {
                foreach (var course in response.Courses)
                {
                    list.Add($"Курс: {course.Name} | ТВОЙ ПРАВИЛЬНЫЙ ID: {course.Id}");
                }
            }
            return list;
        }

    }
}