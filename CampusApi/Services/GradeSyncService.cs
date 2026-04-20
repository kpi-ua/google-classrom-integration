using CampusApi.Data;
using CampusApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusApi.Services
{
    public class GradeSyncService
    {
        private readonly CampusDbContext _db;
        private readonly IGoogleClassroomService _googleApi;

        public GradeSyncService(CampusDbContext db, IGoogleClassroomService googleApi)
        {
            _db = db;
            _googleApi = googleApi;
        }

        public async Task<string> SyncGradesAsync(
            string courseId,
            string? teacherEmail,
            int? cRNPRowId,
            int? rtStudyGroupId,
            int? semesterYear)
        {
            var resolvedTeacherEmail = await ResolveTeacherEmailAsync(courseId, teacherEmail);

            int cMonitoringEmployeeId;
            if (cRNPRowId.HasValue && rtStudyGroupId.HasValue)
            {
                cMonitoringEmployeeId = await ResolveOrCreateMonitoringEmployeeIdAsync(
                    courseId,
                    resolvedTeacherEmail,
                    cRNPRowId.Value,
                    rtStudyGroupId.Value,
                    semesterYear);
            }
            else
            {
                cMonitoringEmployeeId = await ResolveOrCreateDefaultMonitoringEmployeeIdAsync(
                    courseId,
                    resolvedTeacherEmail,
                    semesterYear);
            }

            return await SyncGradesAsync(courseId, cMonitoringEmployeeId);
        }

        public async Task<string> SyncGradesAsync(string courseId, int cMonitoringEmployeeId)
        {
            // Отримуємо повний зріз даних: курс, завдання, сабмішени та студентів.
            var courseSyncData = await _googleApi.GetCourseSyncDataAsync(courseId);
            int syncedCount = 0;

            var monitoringEmployee = await _db.MonitoringEmployees
                .FirstOrDefaultAsync(me => me.CMonitoringEmployeeId == cMonitoringEmployeeId)
                ?? throw new InvalidOperationException($"Не знайдено cmonitoringemployee з id={cMonitoringEmployeeId}");

            var monitoring = await _db.Monitorings
                .FirstOrDefaultAsync(m => m.CMonitoringId == monitoringEmployee.CMonitoringId)
                ?? throw new InvalidOperationException($"Не знайдено cmonitoring з id={monitoringEmployee.CMonitoringId}");

            if (!string.Equals(monitoring.GoogleCourseId, courseSyncData.CourseId, StringComparison.Ordinal))
            {
                monitoring.GoogleCourseId = courseSyncData.CourseId;
                monitoring.VcChangeDate = DateTime.Now;
            }

            var studentByGoogleId = new Dictionary<string, CMonitoringStudent>(StringComparer.OrdinalIgnoreCase);
            foreach (var googleStudent in courseSyncData.Students)
            {
                if (string.IsNullOrWhiteSpace(googleStudent.UserId) || string.IsNullOrWhiteSpace(googleStudent.Email))
                {
                    continue;
                }

                var email = googleStudent.Email.Trim().ToLower();
                int personalityId = await ResolveOrCreateStudentPersonalityIdAsync(email, googleStudent.UserId);

                var monitoringStudent = await _db.MonitoringStudents.FirstOrDefaultAsync(ms =>
                    ms.SPersonalityId == personalityId &&
                    ms.CMonitoringEmployeeId == cMonitoringEmployeeId);
                if (monitoringStudent == null)
                {
                    monitoringStudent = new CMonitoringStudent
                    {
                        CMonitoringEmployeeId = cMonitoringEmployeeId,
                        SPersonalityId = personalityId,
                        GoogleStudentId = googleStudent.UserId
                    };

                    _db.MonitoringStudents.Add(monitoringStudent);
                    await _db.SaveChangesAsync();
                }
                else if (!string.Equals(monitoringStudent.GoogleStudentId, googleStudent.UserId, StringComparison.Ordinal))
                {
                    monitoringStudent.GoogleStudentId = googleStudent.UserId;
                    _db.MonitoringStudents.Update(monitoringStudent);
                }

                studentByGoogleId[googleStudent.UserId] = monitoringStudent;
            }

            foreach (var courseWork in courseSyncData.CourseWorks)
            {
                var colDescription = $"Google: {courseWork.Title}";

                var column = await _db.MonitoringColumns.FirstOrDefaultAsync(c =>
                    c.CMonitoringEmployeeId == cMonitoringEmployeeId &&
                    c.GoogleCourseWorkId == courseWork.Id);

                if (column == null)
                {
                    column = await _db.MonitoringColumns.FirstOrDefaultAsync(c =>
                        c.CMonitoringEmployeeId == cMonitoringEmployeeId &&
                        c.Description == colDescription);
                }

                if (column == null)
                {
                    column = new CMonitoringColumn
                    {
                        CMonitoringEmployeeId = cMonitoringEmployeeId,
                        DcLoadId = 26,
                        DateMark = DateTime.Now,
                        Description = colDescription,
                        GoogleCourseWorkId = courseWork.Id
                    };

                    _db.MonitoringColumns.Add(column);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    column.GoogleCourseWorkId = courseWork.Id;
                    column.Description = colDescription;
                    column.DateMark = DateTime.Now;
                    _db.MonitoringColumns.Update(column);
                }

                foreach (var submission in courseWork.Submissions)
                {
                    if (!studentByGoogleId.TryGetValue(submission.UserId, out var monitoringStudent))
                    {
                        continue;
                    }

                    var row = await _db.MonitoringRows.FirstOrDefaultAsync(r =>
                        r.GoogleStudentSubmissionId == submission.Id);

                    if (row == null)
                    {
                        row = await _db.MonitoringRows.FirstOrDefaultAsync(r =>
                            r.CMonitoringColumnId == column.CMonitoringColumnId &&
                            r.CMonitoringStudentId == monitoringStudent.CMonitoringStudentId);
                    }

                    if (row == null)
                    {
                        row = new CMonitoringRow
                        {
                            CMonitoringColumnId = column.CMonitoringColumnId,
                            SPersonalityId = monitoringStudent.SPersonalityId,
                            CMonitoringStudentId = monitoringStudent.CMonitoringStudentId,
                            GoogleStudentSubmissionId = submission.Id,
                            Mark = submission.AssignedGrade,
                            VcActuality = '1',
                            VcChangeDate = DateTime.Now
                        };

                        _db.MonitoringRows.Add(row);
                    }
                    else
                    {
                        row.CMonitoringColumnId = column.CMonitoringColumnId;
                        row.SPersonalityId = monitoringStudent.SPersonalityId;
                        row.CMonitoringStudentId = monitoringStudent.CMonitoringStudentId;
                        row.GoogleStudentSubmissionId = submission.Id;
                        row.Mark = submission.AssignedGrade;
                        row.VcChangeDate = DateTime.Now;
                        _db.MonitoringRows.Update(row);
                    }

                    syncedCount++;
                }
            }

            await _db.SaveChangesAsync();
            return $"Синхронізація успішна! Перенесено індивідуальних оцінок: {syncedCount}.";
        }

        private async Task<string> ResolveTeacherEmailAsync(string courseId, string? teacherEmail)
        {
            if (!string.IsNullOrWhiteSpace(teacherEmail))
            {
                return teacherEmail.Trim().ToLower();
            }

            var classroomTeacherEmails = await _googleApi.GetTeacherEmailsForCourseAsync(courseId);
            if (!classroomTeacherEmails.Any())
            {
                throw new InvalidOperationException("У Google Classroom не знайдено жодного викладача для цього курсу.");
            }

            var normalizedTeacherEmails = classroomTeacherEmails
                .Select(x => x.Trim().ToLower())
                .Distinct()
                .ToList();

            var matchedAccount = await _db.UserAccounts.FirstOrDefaultAsync(u =>
                normalizedTeacherEmails.Contains(u.Gmail.ToLower()));

            if (matchedAccount != null)
            {
                return matchedAccount.Gmail.Trim().ToLower();
            }

            // Якщо в Campus ще немає useraccount для викладача, беремо перший email з Google Classroom.
            return normalizedTeacherEmails.First();
        }

        private async Task<int> ResolveOrCreateMonitoringEmployeeIdAsync(
            string courseId,
            string teacherEmail,
            int cRNPRowId,
            int rtStudyGroupId,
            int? semesterYear)
        {
            var normalizedEmail = teacherEmail.Trim().ToLower();
            var employeeId = await ResolveOrCreateTeacherEmployeeIdAsync(normalizedEmail);

            var monitoring = await _db.Monitorings.FirstOrDefaultAsync(m =>
                m.CRNPRowId == cRNPRowId &&
                m.RtStudyGroupId == rtStudyGroupId &&
                m.SemesterYear == semesterYear);

            if (monitoring == null)
            {
                monitoring = new CMonitoring
                {
                    CRNPRowId = cRNPRowId,
                    RtStudyGroupId = rtStudyGroupId,
                    SemesterYear = semesterYear,
                    GoogleCourseId = courseId,
                    VcActuality = '1',
                    VcChangeDate = DateTime.Now
                };

                _db.Monitorings.Add(monitoring);
                await _db.SaveChangesAsync();
            }
            else if (!string.Equals(monitoring.GoogleCourseId, courseId, StringComparison.Ordinal))
            {
                monitoring.GoogleCourseId = courseId;
                monitoring.VcChangeDate = DateTime.Now;
                _db.Monitorings.Update(monitoring);
                await _db.SaveChangesAsync();
            }

            var monitoringEmployee = await _db.MonitoringEmployees.FirstOrDefaultAsync(me =>
                me.CMonitoringId == monitoring.CMonitoringId &&
                me.EEmployees1Id == employeeId);

            if (monitoringEmployee == null)
            {
                monitoringEmployee = new CMonitoringEmployee
                {
                    CMonitoringId = monitoring.CMonitoringId,
                    EEmployees1Id = employeeId,
                    VcActuality = '1',
                    VcChangeDate = DateTime.Now
                };

                _db.MonitoringEmployees.Add(monitoringEmployee);
                await _db.SaveChangesAsync();
            }

            return monitoringEmployee.CMonitoringEmployeeId;
        }

        private async Task<int> ResolveOrCreateDefaultMonitoringEmployeeIdAsync(
            string courseId,
            string teacherEmail,
            int? semesterYear)
        {
            var normalizedEmail = teacherEmail.Trim().ToLower();
            var employeeId = await ResolveOrCreateTeacherEmployeeIdAsync(normalizedEmail);

            var existingMonitoringEmployee = await _db.MonitoringEmployees
                .Join(_db.Monitorings,
                    me => me.CMonitoringId,
                    m => m.CMonitoringId,
                    (me, m) => new { me, m })
                .Where(x => x.me.EEmployees1Id == employeeId)
                .OrderByDescending(x => x.m.VcChangeDate)
                .Select(x => x.me.CMonitoringEmployeeId)
                .FirstOrDefaultAsync();

            if (existingMonitoringEmployee != 0)
            {
                return existingMonitoringEmployee;
            }

            var fallbackSemesterYear = semesterYear ?? DateTime.Now.Year;

            return await ResolveOrCreateMonitoringEmployeeIdAsync(
                courseId,
                teacherEmail,
                cRNPRowId: 5001,
                rtStudyGroupId: 2101,
                semesterYear: fallbackSemesterYear);
        }

        private async Task<int> ResolveOrCreateTeacherEmployeeIdAsync(string teacherEmail)
        {
            var normalizedEmail = teacherEmail.Trim().ToLower();

            var userAccount = await _db.UserAccounts
                .FirstOrDefaultAsync(u => u.Gmail.ToLower() == normalizedEmail);

            if (userAccount == null)
            {
                userAccount = new UserAccount
                {
                    Gmail = normalizedEmail
                };

                _db.UserAccounts.Add(userAccount);
                await _db.SaveChangesAsync();
            }

            var employee = await _db.Employees
                .FirstOrDefaultAsync(e => e.UserAccountId == userAccount.UserAccountId);

            if (employee == null)
            {
                employee = new EEmployee
                {
                    UserAccountId = userAccount.UserAccountId
                };

                _db.Employees.Add(employee);
                await _db.SaveChangesAsync();
            }

            return employee.EEmployees1Id;
        }

        private async Task<int> ResolveOrCreateStudentPersonalityIdAsync(string studentEmail, string googleStudentId)
        {
            var normalizedEmail = studentEmail.Trim().ToLower();

            var existingContact = await _db.UserContacts.FirstOrDefaultAsync(uc =>
                uc.DcContactTypeId == 29 &&
                uc.UserContactValue.ToLower() == normalizedEmail);

            if (existingContact != null)
            {
                return existingContact.SPersonalityId;
            }

            var userAccount = await _db.UserAccounts
                .FirstOrDefaultAsync(u => u.Gmail.ToLower() == normalizedEmail);

            if (userAccount == null)
            {
                userAccount = new UserAccount
                {
                    Gmail = normalizedEmail
                };

                _db.UserAccounts.Add(userAccount);
                await _db.SaveChangesAsync();
            }

            var localPart = normalizedEmail.Split('@')[0];
            var nameParts = localPart.Split(new[] { '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : "Google";
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "Student";

            var personality = await _db.Personalities.FirstOrDefaultAsync(p => p.STUDENT_ID == $"GOOGLE-{googleStudentId}");
            if (personality == null)
            {
                personality = new SPersonality
                {
                    STUDENT_ID = $"GOOGLE-{googleStudentId}",
                    FIRST_NAME = firstName,
                    LAST_NAME = lastName
                };

                _db.Personalities.Add(personality);
                await _db.SaveChangesAsync();
            }

            var userContact = new UserContact
            {
                UserAccountId = userAccount.UserAccountId,
                DcContactTypeId = 29,
                UserContactValue = normalizedEmail,
                SPersonalityId = personality.Id
            };

            _db.UserContacts.Add(userContact);
            await _db.SaveChangesAsync();

            return personality.Id;
        }
    }
}