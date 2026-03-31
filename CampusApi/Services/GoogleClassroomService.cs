using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CampusApi.Services
{
    // Модель для зберігання конкретної оцінки за конкретне завдання
    public class StudentGradeItem
    {
        public string AssignmentTitle { get; set; }
        public float Mark { get; set; }
    }

    public interface IGoogleClassroomService
    {
        Task<Dictionary<string, List<StudentGradeItem>>> GetAllGradesForCourseAsync(string courseId);
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
            if (courseId.All(char.IsDigit)) return courseId; // Якщо вже цифри - віддаємо як є
            try
            {
                string base64 = courseId.Replace('-', '+').Replace('_', '/');
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
            return courseId;
        }

        public async Task<Dictionary<string, List<StudentGradeItem>>> GetAllGradesForCourseAsync(string courseId)
        {
            // Декодуємо ID курсу, якщо передано зашифроване посилання
            courseId = DecodeCourseIdIfNeeded(courseId);

            UserCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes, "user", CancellationToken.None, new FileDataStore("TokenStore", true));
            }

            var service = new ClassroomService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Campus Sync API"
            });

            var studentGrades = new Dictionary<string, List<StudentGradeItem>>();
            var userEmailMap = new Dictionary<string, string>();

            // 1. Отримуємо список студентів та їхні Email
            var studentsResponse = await service.Courses.Students.List(courseId).ExecuteAsync();
            if (studentsResponse.Students != null)
            {
                foreach (var student in studentsResponse.Students)
                {
                    string userId = student.UserId;
                    string email = student.Profile?.EmailAddress;

                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(email))
                    {
                        userEmailMap[userId] = email;
                        studentGrades[email] = new List<StudentGradeItem>();
                    }
                }
            }

            // 2. Отримуємо всі завдання курсу
            var courseWorkResponse = await service.Courses.CourseWork.List(courseId).ExecuteAsync();
            if (courseWorkResponse.CourseWork != null)
            {
                foreach (var work in courseWorkResponse.CourseWork)
                {
                    var subResponse = await service.Courses.CourseWork.StudentSubmissions.List(courseId, work.Id).ExecuteAsync();
                    if (subResponse.StudentSubmissions != null)
                    {
                        foreach (var sub in subResponse.StudentSubmissions)
                        {
                            // ФІЛЬТР: Беремо тільки ПОВЕРНЕНІ ("RETURNED") оцінки
                            if (sub.State == "RETURNED" && sub.AssignedGrade.HasValue && !string.IsNullOrEmpty(sub.UserId))
                            {
                                if (userEmailMap.TryGetValue(sub.UserId, out var email))
                                {
                                    studentGrades[email].Add(new StudentGradeItem
                                    {
                                        AssignmentTitle = work.Title,
                                        Mark = (float)sub.AssignedGrade.Value
                                    });
                                }
                            }
                        }
                    }
                }
            }

            return studentGrades;
        }
    }
}