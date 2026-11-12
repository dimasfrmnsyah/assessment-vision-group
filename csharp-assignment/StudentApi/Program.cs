using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var databasePath = Path.Combine(builder.Environment.ContentRootPath, "Data", "students.db");
var students = new StudentStore(databasePath);
students.Initialize();

app.MapGet("/", () => Results.Ok(new { message = "Student API is running." }));

app.MapGet("/students", () => Results.Ok(students.GetAll()));

app.MapGet("/students/{id:int}", (int id) =>
    students.GetById(id) is Student student
        ? Results.Ok(student)
        : Results.NotFound(new { message = "Student not found." }));

app.MapPost("/students", (StudentInput input) =>
{
    var errors = input.Validate();
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    var student = students.Create(input);
    return Results.Created($"/students/{student.Id}", student);
});

app.MapPut("/students/{id:int}", (int id, StudentInput input) =>
{
    var errors = input.Validate();
    if (errors.Count > 0) return Results.ValidationProblem(errors);

    return students.Update(id, input) is Student updated
        ? Results.Ok(updated)
        : Results.NotFound(new { message = "Student not found." });
});

app.MapDelete("/students/{id:int}", (int id) =>
    students.Delete(id)
        ? Results.NoContent()
        : Results.NotFound(new { message = "Student not found." }));

app.Run();

record Student(int Id, string Name, string Email, string Major);

record StudentInput(string? Name, string? Email, string? Major)
{
    public Dictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(Name)) errors["name"] = ["Name is required."];
        if (string.IsNullOrWhiteSpace(Email) || !new EmailAddressAttribute().IsValid(Email.Trim()))
            errors["email"] = ["A valid email address is required."];
        if (string.IsNullOrWhiteSpace(Major)) errors["major"] = ["Major is required."];
        return errors;
    }
}
