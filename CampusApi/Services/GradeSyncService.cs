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
            // 1. Получаем суммы балов из гугла
            var googleGrades = await _googleApi.GetTotalGradesForCourseAsync(courseId);

            // 2. Ищем или создаем колонку в ведомости для "Итоговой суммы из Google Classroom"
            var column = await _db.MonitoringColumns.FirstOrDefaultAsync(c =>
                c.CMonitoringEmployeeId == cMonitoringEmployeeId &&
                c.DcLoadId == 26); // 26 - по ТЗ "Завдання Google класу"

            if (column == null)
            {
                column = new CMonitoringColumn
                {
                    CMonitoringEmployeeId = cMonitoringEmployeeId,
                    DcLoadId = 26,
                    DateMark = DateTime.Now,
                    Description = "Итоговая оценка синхронизированная из Google Classroom"
                };
                _db.MonitoringColumns.Add(column);
                await _db.SaveChangesAsync();
            }

            int syncedCount = 0;

            // 3. Идем по всем студентам из Гугла
            foreach (var googleStudent in googleGrades)
            {
                string email = googleStudent.Key;
                float totalMark = googleStudent.Value;

                // Ищем студента в Кампусе по Email (DcContactTypeId = 29)
                var userContact = await _db.UserContacts
                    .FirstOrDefaultAsync(uc => uc.DcContactTypeId == 29 && uc.UserContactValue == email);

                if (userContact == null) continue; // Если email нет в Кампусе - пропускаем

                int personalityId = userContact.SPersonalityId;

                // Проверяем, есть ли студент в этой ведомости
                var monitoringStudent = await _db.MonitoringStudents
                    .FirstOrDefaultAsync(ms => ms.SPersonalityId == personalityId && ms.CMonitoringEmployeeId == cMonitoringEmployeeId);

                if (monitoringStudent == null) continue;

                // 4. Ищем существующую оценку в ячейке или создаем новую
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
                        Mark = totalMark,
                        VcActuality = '1',
                        VcChangeDate = DateTime.Now
                    };
                    _db.MonitoringRows.Add(row);
                }
                else
                {
                    row.Mark = totalMark; // Обновляем оценку
                    row.VcChangeDate = DateTime.Now;
                    _db.MonitoringRows.Update(row);
                }

                syncedCount++;
            }

            await _db.SaveChangesAsync();
            return $"Синхронизация успешна! Перенесено оценок: {syncedCount}.";
        }
    }
}