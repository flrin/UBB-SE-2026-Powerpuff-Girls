using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;
using Tests_and_Interviews.Models.Enums;

namespace Tests_and_Interviews.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<TestAttempt> TestAttempts { get; set; }
        public DbSet<InterviewSession> InterviewSessions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Port=5432;Database=WinUIDevDb;Username=devuser;Password=devpassword");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.TestAttempt)
                .WithMany(ta => ta.Answers)
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TestAttempt>()
                .HasOne(ta => ta.Test)
                .WithMany(t => t.Attempts)
                .HasForeignKey(ta => ta.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TestAttempt>()
                .HasOne(ta => ta.User)
                .WithMany()
                .HasForeignKey(ta => ta.ExternalUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TestAttempt>()
                .Property(ta => ta.Id)
                .UseIdentityAlwaysColumn();
        }

        public void SeedDatabase()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();

            if (!Users.Any())
            {
                Users.AddRange(
                    new User { Name = "Alice Johnson", Email = "alice@example.com" },
                    new User { Name = "Bob Smith", Email = "bob@example.com" },
                    new User { Name = "Carol Williams", Email = "carol@example.com" },
                    new User { Name = "Dan Ionescu", Email = "dan@example.com" },
                    new User { Name = "Elena Popescu", Email = "elena@example.com" }
                );
                SaveChanges();
            }

            if (!Tests.Any())
            {
                Tests.AddRange(
                    new Test { Id = 1, Title = "C# Fundamentals", Category = "Programming", CreatedAt = new DateTime(2025, 1, 10, 9, 0, 0, DateTimeKind.Utc) },
                    new Test { Id = 2, Title = "SQL Basics", Category = "Database", CreatedAt = new DateTime(2025, 2, 5, 10, 0, 0, DateTimeKind.Utc) },
                    new Test { Id = 3, Title = "OOP Principles", Category = "Programming", CreatedAt = new DateTime(2025, 3, 1, 9, 0, 0, DateTimeKind.Utc) },
                    new Test { Id = 4, Title = "Data Structures", Category = "Computer Science", CreatedAt = new DateTime(2025, 3, 10, 9, 0, 0, DateTimeKind.Utc) },
                    new Test { Id = 5, Title = "Database Design", Category = "Database", CreatedAt = new DateTime(2025, 3, 15, 9, 0, 0, DateTimeKind.Utc) }
                );
                SaveChanges();
            }

            if (!Questions.Any())
            {
                Questions.AddRange(
                    new Question { PositionId = 1, TestId = null, QuestionText = "Tell us about your favourite project.", QuestionTypeString = QuestionType.INTERVIEW.ToString(), QuestionScore = 4f, QuestionAnswer = null },
                    new Question { PositionId = 1, TestId = null, QuestionText = "How do you handle conflict in a team?", QuestionTypeString = QuestionType.INTERVIEW.ToString(), QuestionScore = 4f, QuestionAnswer = null },
                    new Question { PositionId = 2, TestId = null, QuestionText = "Where do you see yourself in 5 years?", QuestionTypeString = QuestionType.INTERVIEW.ToString(), QuestionScore = 4f, QuestionAnswer = null }
                );
                SaveChanges();

                var csharpTrueFalse = new (string text, string answer)[]
                {
                    ("C# is a statically typed language.", "true"),
                    ("In C#, a string is a value type.", "false"),
                    ("C# supports multiple inheritance through classes.", "false"),
                    ("The 'var' keyword in C# means the variable is dynamic.", "false"),
                    ("In C#, 'int' is an alias for System.Int32.", "true"),
                    ("A struct in C# is a reference type.", "false"),
                    ("C# interfaces can contain method implementations.", "true"),
                    ("In C#, arrays are zero-indexed.", "true"),
                    ("The 'sealed' keyword prevents a class from being inherited.", "true"),
                    ("In C#, a constructor can be private.", "true"),
                    ("C# does not support operator overloading.", "false"),
                    ("The 'readonly' keyword means a field can only be set in the constructor.", "true"),
                    ("In C#, 'null' can be assigned to a value type directly.", "false"),
                    ("A delegate in C# is a type-safe function pointer.", "true"),
                    ("In C#, 'abstract' classes can be instantiated directly.", "false"),
                    ("C# enums are value types.", "true"),
                    ("In C#, properties can have different access levels for get and set.", "true"),
                    ("The 'using' statement is only used for importing namespaces.", "false"),
                    ("In C#, a 'List<T>' is part of the System.Collections.Generic namespace.", "true"),
                    ("C# does not support lambda expressions.", "false"),
                    ("In C#, 'override' is used to redefine a virtual method.", "true"),
                    ("Static methods can access instance members directly.", "false"),
                    ("In C#, exception handling uses try, catch, and finally blocks.", "true"),
                    ("The 'base' keyword refers to the current class.", "false"),
                    ("In C#, LINQ stands for Language Integrated Query.", "true"),
                };
                foreach (var (text, answer) in csharpTrueFalse)
                    Questions.Add(new Question { PositionId = null, TestId = 1, QuestionText = text, QuestionTypeString = QuestionType.TRUE_FALSE.ToString(), QuestionScore = 4f, QuestionAnswer = answer });
                SaveChanges();

                var sqlText = new (string text, string answer)[]
                {
                    ("What SQL keyword is used to retrieve data from a table?", "SELECT"),
                    ("What SQL clause is used to filter rows?", "WHERE"),
                    ("What SQL clause filters rows after grouping?", "HAVING"),
                    ("What SQL keyword is used to sort results?", "ORDER BY"),
                    ("What SQL keyword groups rows with the same values?", "GROUP BY"),
                    ("What SQL keyword is used to insert new data?", "INSERT"),
                    ("What SQL keyword updates existing data?", "UPDATE"),
                    ("What SQL keyword removes rows from a table?", "DELETE"),
                    ("What SQL keyword creates a new table?", "CREATE"),
                    ("What SQL keyword removes a table entirely?", "DROP"),
                    ("What SQL keyword removes duplicate rows from results?", "DISTINCT"),
                    ("What SQL keyword renames a column or table in a result?", "AS"),
                    ("What SQL keyword checks if a value is within a range?", "BETWEEN"),
                    ("What SQL keyword checks if a value matches a list?", "IN"),
                    ("What SQL keyword is used for pattern matching?", "LIKE"),
                    ("What SQL aggregate function counts the number of rows?", "COUNT"),
                    ("What SQL aggregate function returns the sum of values?", "SUM"),
                    ("What SQL aggregate function returns the highest value?", "MAX"),
                    ("What SQL aggregate function returns the lowest value?", "MIN"),
                    ("What SQL aggregate function returns the average?", "AVG"),
                    ("What SQL keyword combines results of two SELECT statements?", "UNION"),
                    ("What SQL join returns all rows from the left table?", "LEFT JOIN"),
                    ("What SQL join returns only matching rows from both tables?", "INNER JOIN"),
                    ("What SQL keyword is used to define a primary key constraint?", "PRIMARY KEY"),
                    ("What SQL keyword defines a relationship between two tables?", "FOREIGN KEY"),
                };
                foreach (var (text, answer) in sqlText)
                    Questions.Add(new Question { PositionId = null, TestId = 2, QuestionText = text, QuestionTypeString = QuestionType.TEXT.ToString(), QuestionScore = 4f, QuestionAnswer = answer });
                SaveChanges();

                var oopSingle = new (string text, string answer, string options)[]
                {
                    ("Which OOP principle hides internal implementation details?", "1", "[\"Inheritance\",\"Encapsulation\",\"Polymorphism\",\"Abstraction\",\"Composition\",\"Delegation\"]"),
                    ("Which OOP principle allows a class to inherit from another?", "0", "[\"Inheritance\",\"Encapsulation\",\"Polymorphism\",\"Abstraction\",\"Composition\",\"Delegation\"]"),
                    ("Which OOP principle allows objects to take multiple forms?", "2", "[\"Inheritance\",\"Encapsulation\",\"Polymorphism\",\"Abstraction\",\"Composition\",\"Delegation\"]"),
                    ("Which keyword in C# is used to inherit a class?", "3", "[\"implements\",\"extends\",\"inherits\",\":\",\"base\",\"derived\"]"),
                    ("Which OOP concept groups data and methods together?", "0", "[\"Class\",\"Interface\",\"Module\",\"Package\",\"Namespace\",\"Assembly\"]"),
                    ("Which principle is achieved using access modifiers?", "1", "[\"Inheritance\",\"Encapsulation\",\"Polymorphism\",\"Abstraction\",\"Coupling\",\"Cohesion\"]"),
                    ("Which keyword is used to call a parent class constructor?", "2", "[\"this\",\"super\",\"base\",\"parent\",\"root\",\"origin\"]"),
                    ("Which OOP concept allows method overriding?", "3", "[\"Encapsulation\",\"Abstraction\",\"Composition\",\"Polymorphism\",\"Delegation\",\"Coupling\"]"),
                    ("Which concept describes a class that cannot be instantiated?", "0", "[\"Abstract class\",\"Static class\",\"Sealed class\",\"Interface\",\"Partial class\",\"Generic class\"]"),
                    ("Which OOP term describes a class blueprint for objects?", "1", "[\"Object\",\"Class\",\"Instance\",\"Method\",\"Field\",\"Property\"]"),
                    ("Which keyword defines an abstract method in C#?", "2", "[\"virtual\",\"override\",\"abstract\",\"sealed\",\"static\",\"extern\"]"),
                    ("Which concept allows different classes to share an interface?", "3", "[\"Inheritance\",\"Encapsulation\",\"Composition\",\"Polymorphism\",\"Coupling\",\"Cohesion\"]"),
                    ("Which access modifier makes a member visible only within the class?", "0", "[\"private\",\"public\",\"protected\",\"internal\",\"sealed\",\"static\"]"),
                    ("Which OOP principle reduces code duplication through reuse?", "1", "[\"Encapsulation\",\"Inheritance\",\"Polymorphism\",\"Abstraction\",\"Coupling\",\"Cohesion\"]"),
                    ("Which concept describes a class implementing multiple interfaces?", "2", "[\"Multiple inheritance\",\"Composition\",\"Multiple interface implementation\",\"Delegation\",\"Aggregation\",\"Association\"]"),
                    ("Which keyword in C# prevents a method from being overridden?", "3", "[\"abstract\",\"virtual\",\"override\",\"sealed\",\"static\",\"readonly\"]"),
                    ("Which OOP term describes the process of creating an object?", "0", "[\"Instantiation\",\"Inheritance\",\"Encapsulation\",\"Abstraction\",\"Delegation\",\"Composition\"]"),
                    ("Which principle is violated when a class has too many responsibilities?", "1", "[\"DRY\",\"Single Responsibility\",\"Open/Closed\",\"Liskov Substitution\",\"Interface Segregation\",\"Dependency Inversion\"]"),
                    ("Which concept allows a method to have multiple signatures?", "2", "[\"Overriding\",\"Inheritance\",\"Overloading\",\"Encapsulation\",\"Abstraction\",\"Delegation\"]"),
                    ("Which keyword is used to define an interface in C#?", "3", "[\"abstract\",\"class\",\"struct\",\"interface\",\"enum\",\"delegate\"]"),
                    ("Which concept describes hiding data using private fields?", "0", "[\"Encapsulation\",\"Inheritance\",\"Polymorphism\",\"Abstraction\",\"Coupling\",\"Cohesion\"]"),
                    ("Which OOP term describes a child class?", "1", "[\"Base class\",\"Derived class\",\"Abstract class\",\"Sealed class\",\"Static class\",\"Partial class\"]"),
                    ("Which principle encourages programming to interfaces?", "2", "[\"DRY\",\"SOLID\",\"Dependency Inversion\",\"Open/Closed\",\"Liskov\",\"Cohesion\"]"),
                    ("Which concept allows extending a class without modifying it?", "3", "[\"Encapsulation\",\"Inheritance\",\"Polymorphism\",\"Open/Closed Principle\",\"Abstraction\",\"Delegation\"]"),
                    ("Which keyword is used to implement an interface in C#?", "0", "[\":\",\"implements\",\"extends\",\"inherits\",\"base\",\"using\"]"),
                };
                foreach (var (text, answer, options) in oopSingle)
                    Questions.Add(new Question { PositionId = null, TestId = 3, QuestionText = text, QuestionTypeString = QuestionType.SINGLE_CHOICE.ToString(), QuestionScore = 4f, QuestionAnswer = answer, OptionsJson = options });
                SaveChanges();

                var dsMultiple = new (string text, string answer, string options)[]
                {
                    ("Which of the following are linear data structures?", "[0,1]", "[\"Array\",\"Linked List\",\"Tree\",\"Graph\",\"Heap\",\"Trie\"]"),
                    ("Which of the following use LIFO ordering?", "[1,3]", "[\"Queue\",\"Stack\",\"Deque\",\"Call Stack\",\"Priority Queue\",\"Circular Buffer\"]"),
                    ("Which data structures allow duplicate values?", "[0,2]", "[\"List\",\"Set\",\"Bag\",\"Map\",\"HashSet\",\"TreeSet\"]"),
                    ("Which of the following are tree-based structures?", "[2,3]", "[\"Array\",\"Stack\",\"Binary Search Tree\",\"Heap\",\"Queue\",\"Linked List\"]"),
                    ("Which structures allow O(1) access by index?", "[0,1]", "[\"Array\",\"ArrayList\",\"Linked List\",\"Tree\",\"Stack\",\"Queue\"]"),
                    ("Which of the following are sorting algorithms?", "[0,1,2]", "[\"Merge Sort\",\"Quick Sort\",\"Bubble Sort\",\"BFS\",\"DFS\",\"Dijkstra\"]"),
                    ("Which data structures use nodes and pointers?", "[1,2,3]", "[\"Array\",\"Linked List\",\"Tree\",\"Graph\",\"Stack Array\",\"Queue Array\"]"),
                    ("Which of the following are valid graph representations?", "[0,2]", "[\"Adjacency Matrix\",\"Linked List\",\"Adjacency List\",\"Stack\",\"Queue\",\"Array\"]"),
                    ("Which structures are used to implement recursion?", "[1,3]", "[\"Queue\",\"Stack\",\"Array\",\"Call Stack\",\"Heap\",\"Tree\"]"),
                    ("Which of the following have O(log n) search time?", "[2,3]", "[\"Array\",\"Linked List\",\"Binary Search Tree\",\"Balanced BST\",\"Stack\",\"Queue\"]"),
                    ("Which data structures are based on hashing?", "[0,3]", "[\"HashMap\",\"Array\",\"Linked List\",\"HashSet\",\"Stack\",\"Queue\"]"),
                    ("Which of the following are examples of queues?", "[1,2]", "[\"Stack\",\"FIFO Queue\",\"Circular Queue\",\"Deque\",\"Priority Queue\",\"Call Stack\"]"),
                    ("Which structures preserve insertion order?", "[0,1,2]", "[\"ArrayList\",\"LinkedList\",\"Queue\",\"HashSet\",\"TreeSet\",\"HashMap\"]"),
                    ("Which of the following are search algorithms?", "[0,1,3]", "[\"Binary Search\",\"Linear Search\",\"Bubble Sort\",\"BFS\",\"Merge Sort\",\"Quick Sort\"]"),
                    ("Which data structures support fast insertion at both ends?", "[1,2]", "[\"Array\",\"Deque\",\"Doubly Linked List\",\"Stack\",\"Queue\",\"Tree\"]"),
                    ("Which of the following are heap types?", "[0,3]", "[\"Min Heap\",\"Stack\",\"Queue\",\"Max Heap\",\"Array\",\"Tree\"]"),
                    ("Which structures are used in BFS traversal?", "[0,2]", "[\"Queue\",\"Stack\",\"Visited Array\",\"Heap\",\"Tree\",\"Graph\"]"),
                    ("Which of the following are balanced tree types?", "[1,3]", "[\"Binary Tree\",\"AVL Tree\",\"BST\",\"Red-Black Tree\",\"Trie\",\"Heap\"]"),
                    ("Which data structures have O(1) push and pop?", "[0,1]", "[\"Stack\",\"Queue\",\"Array\",\"Linked List\",\"Tree\",\"Graph\"]"),
                    ("Which of the following describe a linked list?", "[0,2,3]", "[\"Dynamic size\",\"Fixed size\",\"Node-based\",\"Pointer-based\",\"Index-based\",\"Contiguous memory\"]"),
                    ("Which structures can be used to detect cycles in a graph?", "[1,2]", "[\"Queue\",\"DFS\",\"Visited Set\",\"Stack\",\"Array\",\"Heap\"]"),
                    ("Which of the following are divide-and-conquer algorithms?", "[0,3]", "[\"Merge Sort\",\"Bubble Sort\",\"Insertion Sort\",\"Quick Sort\",\"Selection Sort\",\"Counting Sort\"]"),
                    ("Which data structures are used in DFS traversal?", "[1,3]", "[\"Queue\",\"Stack\",\"Array\",\"Recursion Stack\",\"Heap\",\"Tree\"]"),
                    ("Which of the following describe a binary tree?", "[0,1,2]", "[\"Each node has at most 2 children\",\"Has a root node\",\"Can be empty\",\"Always balanced\",\"Always sorted\",\"Always complete\"]"),
                    ("Which structures allow O(1) average lookup time?", "[2,3]", "[\"Array\",\"Linked List\",\"HashMap\",\"HashSet\",\"Tree\",\"Stack\"]"),
                };
                foreach (var (text, answer, options) in dsMultiple)
                    Questions.Add(new Question { PositionId = null, TestId = 4, QuestionText = text, QuestionTypeString = QuestionType.MULTIPLE_CHOICE.ToString(), QuestionScore = 4f, QuestionAnswer = answer, OptionsJson = options });
                SaveChanges();

                var dbDesignTrueFalse = new (string text, string answer)[]
                {
                    ("A primary key can contain NULL values.", "false"),
                    ("A foreign key references the primary key of another table.", "true"),
                    ("Second normal form eliminates partial dependencies.", "true"),
                    ("A table can have multiple primary keys.", "false"),
                    ("Third normal form eliminates transitive dependencies.", "true"),
                    ("An index always speeds up both reads and writes.", "false"),
                    ("A unique constraint allows NULL values.", "true"),
                    ("Denormalization can improve read performance.", "true"),
                    ("A composite key is made up of a single column.", "false"),
                    ("BCNF is a stricter version of 3NF.", "true"),
                    ("A view stores data physically in the database.", "false"),
                    ("Referential integrity is enforced by foreign keys.", "true"),
                    ("A surrogate key has a natural business meaning.", "false"),
                    ("An ER diagram is used to model database structure.", "true"),
                    ("NULL means zero in a database.", "false"),
                    ("A clustered index determines the physical order of data.", "true"),
                    ("A database can have multiple clustered indexes per table.", "false"),
                    ("Normalization always improves query performance.", "false"),
                    ("A check constraint limits the values allowed in a column.", "true"),
                    ("Stored procedures are precompiled SQL statements.", "true"),
                    ("A trigger is executed automatically on a table event.", "true"),
                    ("Joins can only be performed between two tables at a time.", "false"),
                    ("A self-join joins a table with itself.", "true"),
                    ("CASCADE delete removes child records when a parent is deleted.", "true"),
                    ("A non-clustered index contains the actual data rows.", "false"),
                };
                foreach (var (text, answer) in dbDesignTrueFalse)
                    Questions.Add(new Question { PositionId = null, TestId = 5, QuestionText = text, QuestionTypeString = QuestionType.TRUE_FALSE.ToString(), QuestionScore = 4f, QuestionAnswer = answer });
                SaveChanges();
            }

            if (!TestAttempts.Any())
            {
                var alice = Users.First(u => u.Name == "Alice Johnson");
                var bob = Users.First(u => u.Name == "Bob Smith");
                var carol = Users.First(u => u.Name == "Carol Williams");
                var dan = Users.First(u => u.Name == "Dan Ionescu");
                var elena = Users.First(u => u.Name == "Elena Popescu");

                TestAttempts.AddRange(
                    new TestAttempt { TestId = 1, ExternalUserId = bob.Id, Score = 25m, Status = TestStatus.SUBMITTED.ToString(), StartedAt = new DateTime(2025, 3, 1, 10, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2025, 3, 1, 10, 45, 0, DateTimeKind.Utc), AnswersFilePath = "answers/attempt_1.json" },
                    new TestAttempt { TestId = 2, ExternalUserId = bob.Id, Score = 18m, Status = TestStatus.REVIEWED.ToString(), StartedAt = new DateTime(2025, 3, 5, 14, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2025, 3, 5, 14, 30, 0, DateTimeKind.Utc), AnswersFilePath = "answers/attempt_2.json" },
                    new TestAttempt { TestId = 3, ExternalUserId = carol.Id, Score = 40m, Status = TestStatus.SUBMITTED.ToString(), StartedAt = new DateTime(2025, 3, 10, 9, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2025, 3, 10, 9, 28, 0, DateTimeKind.Utc), AnswersFilePath = "answers/attempt_3.json" },
                    new TestAttempt { TestId = 4, ExternalUserId = dan.Id, Score = 35m, Status = TestStatus.REVIEWED.ToString(), StartedAt = new DateTime(2025, 3, 12, 11, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2025, 3, 12, 11, 25, 0, DateTimeKind.Utc), AnswersFilePath = "answers/attempt_4.json" },
                    new TestAttempt { TestId = 5, ExternalUserId = elena.Id, Score = 50m, Status = TestStatus.SUBMITTED.ToString(), StartedAt = new DateTime(2025, 3, 15, 13, 0, 0, DateTimeKind.Utc), CompletedAt = new DateTime(2025, 3, 15, 13, 29, 0, DateTimeKind.Utc), AnswersFilePath = "answers/attempt_5.json" }
                );
                SaveChanges();
            }

            if (!Answers.Any())
            {
                var attempt_bob_t1 = TestAttempts.First(ta => ta.TestId == 1);
                var attempt_bob_t2 = TestAttempts.First(ta => ta.TestId == 2);

                var q_tf1 = Questions.First(q => q.QuestionText == "C# is a statically typed language.");
                var q_tf2 = Questions.First(q => q.QuestionText == "In C#, a string is a value type.");
                var q_txt1 = Questions.First(q => q.QuestionText == "What SQL keyword is used to retrieve data from a table?");
                var q_txt2 = Questions.First(q => q.QuestionText == "What SQL clause is used to filter rows?");

                Answers.AddRange(
                    new Answer { AttemptId = attempt_bob_t1.Id, QuestionId = q_tf1.Id, Value = "true" },
                    new Answer { AttemptId = attempt_bob_t1.Id, QuestionId = q_tf2.Id, Value = "false" },
                    new Answer { AttemptId = attempt_bob_t2.Id, QuestionId = q_txt1.Id, Value = "SELECT" },
                    new Answer { AttemptId = attempt_bob_t2.Id, QuestionId = q_txt2.Id, Value = "WHERE" }
                );
                SaveChanges();
            }

            if (!InterviewSessions.Any())
            {
                var alice = Users.First(u => u.Name == "Alice Johnson");
                var bob = Users.First(u => u.Name == "Bob Smith");
                var carol = Users.First(u => u.Name == "Carol Williams");

                InterviewSessions.AddRange(
                    new InterviewSession { PositionId = 1, ExternalUserId = alice.Id, InterviewerId = 2, DateStart = new DateTime(2025, 4, 1, 10, 0, 0, DateTimeKind.Utc), Video = "recordings/session_1.mp4", Status = InterviewStatus.Completed.ToString(), Score = 8.5m },
                    new InterviewSession { PositionId = 2, ExternalUserId = carol.Id, InterviewerId = 2, DateStart = new DateTime(2025, 4, 15, 14, 0, 0, DateTimeKind.Utc), Video = "", Status = InterviewStatus.Scheduled.ToString(), Score = 0m },
                    new InterviewSession { PositionId = 1, ExternalUserId = bob.Id, InterviewerId = 3, DateStart = new DateTime(2025, 4, 20, 11, 0, 0, DateTimeKind.Utc), Video = "recordings/session_3.mp4", Status = InterviewStatus.Completed.ToString(), Score = 7.0m }
                );
                SaveChanges();
            }
        }

        public async Task<List<Question>> GetInterviewQuestionsByPositionAsync(int positionId)
        {
            return await Questions
                .Where(q => q.QuestionTypeString == QuestionType.INTERVIEW.ToString()
                         && q.PositionId == positionId)
                .ToListAsync();
        }

        public async Task<InterviewSession> GetInterviewSessionByIdAsync(int id)
        {
            var session = await InterviewSessions.FirstOrDefaultAsync(s => s.Id == id);
            if (session == null)
                throw new KeyNotFoundException($"InterviewSession with ID {id} was not found.");
            return session;
        }

        public async Task UpdateInterviewSessionAsync(InterviewSession updated)
        {
            var existing = await InterviewSessions.FindAsync(updated.Id);
            if (existing == null) return;
            existing.InterviewerId = updated.InterviewerId;
            existing.PositionId = updated.PositionId;
            existing.ExternalUserId = updated.ExternalUserId;
            existing.Status = updated.Status;
            existing.DateStart = updated.DateStart;
            existing.Video = updated.Video;
            existing.Score = updated.Score;
            await SaveChangesAsync();
        }
    }
}