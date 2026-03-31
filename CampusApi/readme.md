CREATE DATABASE IF NOT EXISTS campus_test;
USE campus_test;

CREATE TABLE cmonitoringcolumn (
  CMonitoringColumnId int NOT NULL AUTO_INCREMENT,
  CMonitoringEmployeeId int NOT NULL,
  DcLoadId int NOT NULL,
  DateMark datetime NOT NULL,
  Description varchar(255) DEFAULT NULL,
  PRIMARY KEY (CMonitoringColumnId)
);

CREATE TABLE cmonitoringrow (
  CMonitoringRowId int NOT NULL AUTO_INCREMENT,
  CMonitoringColumnId int NOT NULL,
  SPersonalityId int NOT NULL,
  CMonitoringStudentId int NOT NULL,
  Mark float DEFAULT NULL,
  VcActuality char(1) NOT NULL,
  VcChangeDate datetime NOT NULL,
  PRIMARY KEY (CMonitoringRowId)
);

CREATE TABLE cmonitoringstudent (
  CMonitoringStudentId int NOT NULL AUTO_INCREMENT,
  CMonitoringEmployeeId int NOT NULL,
  SPersonalityId int NOT NULL,
  PRIMARY KEY (CMonitoringStudentId)
);

CREATE TABLE spersonality1 (
  Id int NOT NULL AUTO_INCREMENT,
  STUDENT_ID varchar(12) DEFAULT NULL,
  FIRST_NAME varchar(30) DEFAULT NULL,
  LAST_NAME varchar(30) DEFAULT NULL,
  PRIMARY KEY (Id)
);

CREATE TABLE usercontact (
  UserContactId int NOT NULL AUTO_INCREMENT,
  UserAccountId int DEFAULT NULL,
  DcContactTypeId int NOT NULL,
  UserContactValue varchar(255) DEFAULT NULL,
  SPersonalityId int NOT NULL,
  PRIMARY KEY (UserContactId)
);

-- Тестові студенти для перевіряючого
INSERT INTO spersonality1 (Id, STUDENT_ID, FIRST_NAME, LAST_NAME) VALUES (10, 'KB-21', 'Денис', 'Денисов');
INSERT INTO spersonality1 (Id, STUDENT_ID, FIRST_NAME, LAST_NAME) VALUES (11, 'IK-21', 'Олексій', 'Величко');

-- Зв'язок їхніх Email (DcContactTypeId = 29)

INSERT INTO usercontact (UserContactId, UserAccountId, DcContactTypeId, UserContactValue, SPersonalityId) 
VALUES (10, 110, 29, 'horoshoved@gmail.com', 10);
INSERT INTO usercontact (UserContactId, UserAccountId, DcContactTypeId, UserContactValue, SPersonalityId) 
VALUES (11, 111, 29, 'velickoaleksej360@gmail.com', 11);

-- Додаємо студентів у відомість викладача з ID = 999
INSERT INTO cmonitoringstudent (CMonitoringStudentId, CMonitoringEmployeeId, SPersonalityId) VALUES (1010, 999, 10);
INSERT INTO cmonitoringstudent (CMonitoringStudentId, CMonitoringEmployeeId, SPersonalityId) VALUES (1011, 999, 11);