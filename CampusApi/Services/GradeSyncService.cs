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

        public async Task<string> SyncGradesAsync(string courseId, int cMonitoringEmployeeId)
        {
            // Отримуємо список усіх студентів та їхні оцінки за КОЖНЕ завдання
            var allGrades = await _googleApi.GetAllGradesForCourseAsync(courseId);
            int syncedCount = 0;

            foreach (var studentGrade in allGrades)
            {
                string email = studentGrade.Key;
                var gradesList = studentGrade.Value;

                // Шукаємо студента в Кампусі за Email
                var userContact = await _db.UserContacts.FirstOrDefaultAsync(uc => uc.DcContactTypeId == 29 && uc.UserContactValue == email);
                if (userContact == null) continue; // Якщо студента немає в Кампусі - пропускаємо

                int personalityId = userContact.SPersonalityId;

                // Перевіряємо, чи є студент у цій конкретній відомості викладача
                var monitoringStudent = await _db.MonitoringStudents.FirstOrDefaultAsync(ms => ms.SPersonalityId == personalityId && ms.CMonitoringEmployeeId == cMonitoringEmployeeId);
                if (monitoringStudent == null) continue;

                // Перебираємо кожну оцінку студента
                foreach (var grade in gradesList)
                {
                    string colDescription = $"Google: {grade.AssignmentTitle}";

                    // Шукаємо колонку під конкретне завдання (або створюємо її)
                    var column = await _db.MonitoringColumns.FirstOrDefaultAsync(c =>
                        c.CMonitoringEmployeeId == cMonitoringEmployeeId &&
                        c.Description == colDescription);

                    if (column == null)
                    {
                        column = new CMonitoringColumn
                        {
                            CMonitoringEmployeeId = cMonitoringEmployeeId,
                            DcLoadId = 26, // 26 = Завдання Google класу
                            DateMark = DateTime.Now,
                            Description = colDescription
                        };
                        _db.MonitoringColumns.Add(column);
                        await _db.SaveChangesAsync(); // Зберігаємо одразу, щоб отримати ID згенерованої колонки
                    }

                    // Записуємо оцінку студенту саме в ЦЮ колонку
                    var row = await _db.MonitoringRows.FirstOrDefaultAsync(r =>
                        r.CMonitoringColumnId == column.CMonitoringColumnId &&
                        r.SPersonalityId == personalityId);

                    if (row == null)
                    {
                        row = new CMonitoringRow
                        {
                            CMonitoringColumnId = column.CMonitoringColumnId,
                            SPersonalityId = personalityId,
                            CMonitoringStudentId = monitoringStudent.CMonitoringStudentId,
                            Mark = grade.Mark,
                            VcActuality = '1',
                            VcChangeDate = DateTime.Now
                        };
                        _db.MonitoringRows.Add(row);
                    }
                    else
                    {
                        // Оновлюємо існуючу оцінку
                        row.Mark = grade.Mark;
                        row.VcChangeDate = DateTime.Now;
                        _db.MonitoringRows.Update(row);
                    }

                    syncedCount++;
                }
            }

            await _db.SaveChangesAsync();
            return $"Синхронізація успішна! Перенесено індивідуальних оцінок: {syncedCount}.";
        }
    }
}