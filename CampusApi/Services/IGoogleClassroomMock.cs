namespace CampusApi.Services
{
    public interface IGoogleClassroomMock
    {
        // Возвращает словарь: Key = Email студента, Value = Сумма балов за все задания
        Task<Dictionary<string, float>> GetTotalGradesForCourseAsync(string courseId);
    }
}