using Microsoft.Data.Sqlite;

// A new connection per operation keeps concurrent HTTP requests independent.
sealed class StudentStore(string databasePath)
{
    private readonly string connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = databasePath,
        DefaultTimeout = 30
    }.ToString();

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }

    public void Initialize()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS Students (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL,
                Major TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public List<Student> GetAll()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Email, Major FROM Students ORDER BY Id;";
        using var reader = command.ExecuteReader();
        var students = new List<Student>();
        while (reader.Read()) students.Add(ReadStudent(reader));
        return students;
    }

    public Student? GetById(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Email, Major FROM Students WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadStudent(reader) : null;
    }

    public Student Create(StudentInput input)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Students (Name, Email, Major) VALUES ($name, $email, $major)
            RETURNING Id, Name, Email, Major;
            """;
        AddInput(command, input);
        using var reader = command.ExecuteReader();
        reader.Read();
        return ReadStudent(reader);
    }

    public Student? Update(int id, StudentInput input)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Students SET Name = $name, Email = $email, Major = $major WHERE Id = $id
            RETURNING Id, Name, Email, Major;
            """;
        command.Parameters.AddWithValue("$id", id);
        AddInput(command, input);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadStudent(reader) : null;
    }

    public bool Delete(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Students WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return command.ExecuteNonQuery() > 0;
    }

    private static void AddInput(SqliteCommand command, StudentInput input)
    {
        // Parameters ensure user input is treated as data, not executable SQL.
        command.Parameters.AddWithValue("$name", input.Name!.Trim());
        command.Parameters.AddWithValue("$email", input.Email!.Trim());
        command.Parameters.AddWithValue("$major", input.Major!.Trim());
    }

    private static Student ReadStudent(SqliteDataReader reader) =>
        new(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
}
