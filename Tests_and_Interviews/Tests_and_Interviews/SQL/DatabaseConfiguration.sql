DROP TABLE IF EXISTS Answers;
DROP TABLE IF EXISTS Slots;
DROP TABLE IF EXISTS LeaderboardEntries;
DROP TABLE IF EXISTS InterviewSessions;
DROP TABLE IF EXISTS TestAttempts;
DROP TABLE IF EXISTS Questions;
DROP TABLE IF EXISTS Recruiters;
DROP TABLE IF EXISTS Tests;
DROP TABLE IF EXISTS Users;


CREATE TABLE Users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NULL,
    email NVARCHAR(255) NULL
);

CREATE TABLE Tests (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(255) NULL,
    category NVARCHAR(255) NULL,
    created_at DATETIME2 NOT NULL
);

CREATE TABLE Recruiters (
    company_id INT PRIMARY KEY,
    name NVARCHAR(255) NULL
);


CREATE TABLE Questions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    position_id INT NULL,
    test_id INT NULL,
    question_text NVARCHAR(MAX) NULL,
    question_type_string NVARCHAR(50) NULL,
    question_score REAL NOT NULL,
    question_answer NVARCHAR(MAX) NULL,
    options_json NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Questions_Tests FOREIGN KEY (test_id) REFERENCES Tests(id) ON DELETE SET NULL
);

CREATE TABLE TestAttempts (
    id INT IDENTITY(1,1) PRIMARY KEY,
    test_id INT NOT NULL,
    external_user_id INT NULL,
    score DECIMAL(18,2) NULL,
    status NVARCHAR(50) NULL,
    started_at DATETIME2 NULL,
    completed_at DATETIME2 NULL,
    answers_file_path NVARCHAR(MAX) NULL,
    is_validated BIT NOT NULL DEFAULT 0,
    percentage_score DECIMAL(18,2) NULL,
    rejection_reason NVARCHAR(MAX) NULL,
    rejected_at DATETIME2 NULL,
    CONSTRAINT FK_TestAttempts_Tests FOREIGN KEY (test_id) REFERENCES Tests(id) ON DELETE CASCADE,
    CONSTRAINT FK_TestAttempts_Users FOREIGN KEY (external_user_id) REFERENCES Users(id) ON DELETE SET NULL
);

CREATE TABLE InterviewSessions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    position_id INT NOT NULL,
    external_user_id INT NULL,
    interviewer_id INT NOT NULL,
    date_start DATETIME2 NOT NULL,
    video NVARCHAR(MAX) NULL,
    status NVARCHAR(50) NULL,
    score DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_InterviewSessions_Users FOREIGN KEY (external_user_id) REFERENCES Users(id) ON DELETE SET NULL
);

CREATE TABLE LeaderboardEntries (
    id INT IDENTITY(1,1) PRIMARY KEY,
    test_id INT NOT NULL,
    user_id INT NOT NULL,
    normalized_score DECIMAL(18,2) NOT NULL,
    rank_position INT NOT NULL,
    tie_break_priority INT NOT NULL,
    last_recalculation_at DATETIME2 NOT NULL,
    CONSTRAINT FK_Leaderboard_Tests FOREIGN KEY (test_id) REFERENCES Tests(id) ON DELETE CASCADE,
    CONSTRAINT FK_Leaderboard_Users FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE CASCADE
);

CREATE TABLE Slots (
    id INT IDENTITY(1,1) PRIMARY KEY,
    recruiter_id INT NOT NULL,
    start_time DATETIME2 NOT NULL,
    end_time DATETIME2 NOT NULL,
    duration INT NOT NULL DEFAULT 30,
    status INT NOT NULL DEFAULT 0, -- Assuming Enum 0 = Free
    interview_type NVARCHAR(255) NULL,
    CONSTRAINT FK_Slots_Recruiters FOREIGN KEY (recruiter_id) REFERENCES Recruiters(company_id) ON DELETE CASCADE
);

CREATE TABLE Answers (
    id INT IDENTITY(1,1) PRIMARY KEY,
    attempt_id INT NOT NULL,
    question_id INT NOT NULL,
    value NVARCHAR(MAX) NULL,
    CONSTRAINT FK_Answers_TestAttempts FOREIGN KEY (attempt_id) REFERENCES TestAttempts(id) ON DELETE CASCADE,
    CONSTRAINT FK_Answers_Questions FOREIGN KEY (question_id) REFERENCES Questions(id) ON DELETE CASCADE
);


INSERT INTO Users (name, email) VALUES 
('Alice Johnson', 'alice@example.com'),
('Bob Smith', 'bob@example.com'),
('Carol Williams', 'carol@example.com'),
('Dan Ionescu', 'dan@example.com'),
('Elena Popescu', 'elena@example.com');

SET IDENTITY_INSERT Tests ON;
INSERT INTO Tests (id, title, category, created_at) VALUES 
(1, 'C# Fundamentals', 'Programming', '2026-01-10T09:00:00Z'),
(2, 'SQL Basics', 'Database', '2026-02-05T10:00:00Z'),
(3, 'OOP Principles', 'Programming', '2026-03-01T09:00:00Z'),
(4, 'Data Structures', 'Computer Science', '2026-03-10T09:00:00Z'),
(5, 'Database Design', 'Database', '2026-03-15T09:00:00Z');
SET IDENTITY_INSERT Tests OFF;

INSERT INTO Recruiters (company_id, name) VALUES
(1, 'Google'),
(2, 'Amazon');


INSERT INTO Questions (position_id, test_id, question_text, question_type_string, question_score, question_answer, options_json) VALUES 

(1, NULL, 'Tell us about your favourite project.', 'INTERVIEW', 4.0, NULL, NULL),
(1, NULL, 'How do you handle conflict in a team?', 'INTERVIEW', 4.0, NULL, NULL),
(2, NULL, 'Where do you see yourself in 5 years?', 'INTERVIEW', 4.0, NULL, NULL),

(NULL, 1, 'C# is a statically typed language.', 'TRUE_FALSE', 4.0, 'true', NULL),
(NULL, 1, 'In C#, a string is a value type.', 'TRUE_FALSE', 4.0, 'false', NULL),
(NULL, 1, 'C# supports multiple inheritance through classes.', 'TRUE_FALSE', 4.0, 'false', NULL),
(NULL, 1, 'The var keyword in C# means the variable is dynamic.', 'TRUE_FALSE', 4.0, 'false', NULL),
(NULL, 1, 'In C#, int is an alias for System.Int32.', 'TRUE_FALSE', 4.0, 'true', NULL),

(NULL, 2, 'What SQL keyword is used to retrieve data from a table?', 'TEXT', 4.0, 'SELECT', NULL),
(NULL, 2, 'What SQL clause is used to filter rows?', 'TEXT', 4.0, 'WHERE', NULL),
(NULL, 2, 'What SQL clause filters rows after grouping?', 'TEXT', 4.0, 'HAVING', NULL),
(NULL, 2, 'What SQL keyword is used to sort results?', 'TEXT', 4.0, 'ORDER BY', NULL),
(NULL, 2, 'What SQL keyword groups rows with the same values?', 'TEXT', 4.0, 'GROUP BY', NULL),

(NULL, 3, 'Which OOP principle hides internal implementation details?', 'SINGLE_CHOICE', 4.0, '1', '["Inheritance","Encapsulation","Polymorphism","Abstraction","Composition","Delegation"]'),
(NULL, 3, 'Which OOP principle allows a class to inherit from another?', 'SINGLE_CHOICE', 4.0, '0', '["Inheritance","Encapsulation","Polymorphism","Abstraction","Composition","Delegation"]'),
(NULL, 3, 'Which OOP principle allows objects to take multiple forms?', 'SINGLE_CHOICE', 4.0, '2', '["Inheritance","Encapsulation","Polymorphism","Abstraction","Composition","Delegation"]'),

(NULL, 4, 'Which of the following are linear data structures?', 'MULTIPLE_CHOICE', 4.0, '[0,1]', '["Array","Linked List","Tree","Graph","Heap","Trie"]'),
(NULL, 4, 'Which of the following use LIFO ordering?', 'MULTIPLE_CHOICE', 4.0, '[1,3]', '["Queue","Stack","Deque","Call Stack","Priority Queue","Circular Buffer"]'),
(NULL, 4, 'Which data structures allow duplicate values?', 'MULTIPLE_CHOICE', 4.0, '[0,2]', '["List","Set","Bag","Map","HashSet","TreeSet"]'),

(NULL, 5, 'A primary key can contain NULL values.', 'TRUE_FALSE', 4.0, 'false', NULL),
(NULL, 5, 'A foreign key references the primary key of another table.', 'TRUE_FALSE', 4.0, 'true', NULL),
(NULL, 5, 'Second normal form eliminates partial dependencies.', 'TRUE_FALSE', 4.0, 'true', NULL);


INSERT INTO TestAttempts (test_id, external_user_id, score, status, started_at, completed_at, answers_file_path, is_validated, percentage_score) VALUES 
(1, 2, 25.0, 'COMPLETED', '2026-03-01T10:00:00Z', '2026-03-01T10:45:00Z', 'answers/attempt_1.json', 1, 25.0),
(2, 2, 18.0, 'COMPLETED', '2026-03-05T14:00:00Z', '2026-03-05T14:30:00Z', 'answers/attempt_2.json', 1, 18.0),
(3, 3, 40.0, 'COMPLETED', '2026-03-10T09:00:00Z', '2026-03-10T09:28:00Z', 'answers/attempt_3.json', 1, 40.0),
(5, 4, 35.0, 'COMPLETED', '2026-03-12T11:00:00Z', '2026-03-12T11:25:00Z', 'answers/attempt_4.json', 1, 35.0),
(5, 5, 50.0, 'COMPLETED', '2026-03-15T13:00:00Z', '2026-03-15T13:29:00Z', 'answers/attempt_5.json', 1, 50.0);

INSERT INTO Answers (attempt_id, question_id, value) VALUES 
(1, 4, 'true'),
(1, 5, 'false'),
(2, 9, 'SELECT'),
(2, 10, 'WHERE');

INSERT INTO InterviewSessions (position_id, external_user_id, interviewer_id, date_start, video, status, score) VALUES 
(1, 1, 2, '2025-04-01T10:00:00Z', 'recordings/session_1.mp4', 'Completed', 8.5),
(2, 3, 2, '2025-04-15T14:00:00Z', '', 'Scheduled', 0.0),
(1, 2, 3, '2025-04-20T11:00:00Z', 'recordings/session_3.mp4', 'Completed', 7.0);