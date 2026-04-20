USE [campus_test];
GO

BEGIN TRY
    BEGIN TRAN;

    -- Clean previous demo data so only requested people remain.
    DELETE FROM dbo.cmonitoringrow;
    DELETE FROM dbo.cmonitoringcolumn;
    DELETE FROM dbo.cmonitoringstudent;
    DELETE FROM dbo.cmonitoringemployee;
    DELETE FROM dbo.cmonitoring;
    DELETE FROM dbo.usercontact;
    DELETE FROM dbo.eemployees1;
    DELETE FROM dbo.useraccount;
    DELETE FROM dbo.spersonality1;

    -- Optional subject dictionary for visibility.
    IF OBJECT_ID(N'dbo.dcload', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.dcload
        (
            DcLoadId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            Name NVARCHAR(100) NOT NULL
        );
    END;

    DELETE FROM dbo.dcload;
    INSERT INTO dbo.dcload (Name) VALUES (N'Google Classroom Demo Subject');

    DECLARE @loadId INT = (SELECT TOP (1) DcLoadId FROM dbo.dcload ORDER BY DcLoadId);

    -- Only teacher: Vladislav
    DECLARE @teacherEmail NVARCHAR(255) = N'berezin.v.7930@gmail.com';
    DECLARE @uaTeacher INT;
    DECLARE @employeeId INT;

    INSERT INTO dbo.useraccount (Gmail)
    VALUES (@teacherEmail);
    SET @uaTeacher = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.eemployees1 (UserAccountId)
    VALUES (@uaTeacher);
    SET @employeeId = CAST(SCOPE_IDENTITY() AS INT);

    -- Monitoring context used by sync endpoint
    DECLARE @monitoringId INT;
    DECLARE @monitoringEmployeeId INT;

    INSERT INTO dbo.cmonitoring (CRNPRowId, RtStudyGroupId, VcActuality, VcChangeDate, SemesterYear)
    VALUES (5001, 2101, N'1', GETDATE(), 2026);
    SET @monitoringId = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.cmonitoringemployee (CMonitoringId, EEmployees1Id, Name, VcActuality, VcChangeDate)
    VALUES (@monitoringId, @employeeId, N'Владислав', N'1', GETDATE());
    SET @monitoringEmployeeId = CAST(SCOPE_IDENTITY() AS INT);

    -- Optional existing column for visibility
    INSERT INTO dbo.cmonitoringcolumn (CMonitoringEmployeeId, DcLoadId, DateMark, Description)
    VALUES (@monitoringEmployeeId, @loadId, GETDATE(), N'Google: Demo Assignment');

    -- Only student: Ivan
    DECLARE @studentId INT;

    INSERT INTO dbo.spersonality1 (STUDENT_ID, FIRST_NAME, LAST_NAME)
    VALUES (N'KB-21-001', N'Иван', N'Демо');
    SET @studentId = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.usercontact (UserAccountId, DcContactTypeId, UserContactValue, SPersonalityId)
    VALUES (0, 29, N'v08707@gmail.com', @studentId);

    INSERT INTO dbo.cmonitoringstudent (CMonitoringEmployeeId, SPersonalityId)
    VALUES (@monitoringEmployeeId, @studentId);

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRAN;
    THROW;
END CATCH;
GO

-- Verification: only requested people.
SELECT UserAccountId, Gmail FROM dbo.useraccount;
SELECT EEmployees1Id, UserAccountId FROM dbo.eemployees1;
SELECT Id, STUDENT_ID, FIRST_NAME, LAST_NAME FROM dbo.spersonality1;
SELECT UserContactId, DcContactTypeId, UserContactValue, SPersonalityId FROM dbo.usercontact;
SELECT CMonitoringEmployeeId, Name, CMonitoringId, EEmployees1Id FROM dbo.cmonitoringemployee;
GO

-- Sync test endpoint parameters:
-- /api/sync/google-classroom?googleCourseId=<COURSE_ID_OR_LINK>&cRNPRowId=5001&rtStudyGroupId=2101&semesterYear=2026
