using Microsoft.EntityFrameworkCore;
using WorkforceApi.Models;

namespace WorkforceApi.Data;

/// <summary>
/// Fills an empty database with demo departments, employees and shifts so the portal has
/// something to show on first run. Does nothing when departments already exist.
/// </summary>
public static class DbSeeder
{
    // Darwin has no daylight saving, so a fixed offset is enough to turn "9am local" into UTC.
    private static readonly TimeSpan LocalUtcOffset = TimeSpan.FromHours(9.5);

    private static readonly (string Name, string Description, string[] Titles, int MinSalary, int MaxSalary)[] DepartmentSeeds =
    [
        ("Engineering", "Software development and platform teams.", ["Software Engineer", "Senior Engineer", "QA Analyst", "DevOps Engineer"], 90_000, 160_000),
        ("Operations", "Day-to-day delivery and logistics.", ["Operations Coordinator", "Site Supervisor", "Logistics Officer"], 65_000, 105_000),
        ("Customer Support", "First and second line customer care.", ["Support Agent", "Support Lead", "Onboarding Specialist"], 55_000, 85_000),
        ("Finance", "Accounting, payroll and reporting.", ["Accountant", "Payroll Officer", "Financial Analyst"], 75_000, 130_000),
        ("Human Resources", "Recruitment, culture and compliance.", ["HR Advisor", "Recruiter", "HR Manager"], 70_000, 120_000),
        ("Marketing", "Brand, campaigns and communications.", ["Marketing Coordinator", "Content Designer", "Campaign Manager"], 65_000, 115_000)
    ];

    private static readonly string[] FirstNames =
    [
        "Olivia", "Liam", "Charlotte", "Noah", "Amelia", "Oliver", "Isla", "William", "Mia", "Jack",
        "Ava", "Henry", "Grace", "Lucas", "Ruby", "Ethan", "Chloe", "Mason", "Sophie", "Archie",
        "Priya", "Arjun", "Mei", "Kenji", "Aisha", "Omar", "Sienna", "Leo", "Zara", "Hamish"
    ];

    private static readonly string[] LastNames =
    [
        "Smith", "Jones", "Williams", "Brown", "Wilson", "Taylor", "Anderson", "Thompson", "Nguyen", "Patel",
        "Martin", "Kelly", "Ryan", "Campbell", "Murray", "Stewart", "Singh", "Chen", "Walker", "Hughes"
    ];

    private static readonly (string Title, int StartHour, int Hours)[] ShiftPatterns =
    [
        ("Morning shift", 6, 8),
        ("Day shift", 9, 8),
        ("Late shift", 13, 8)
    ];

    public static async Task SeedAsync(WorkforceDbContext db)
    {
        if (await db.Departments.AnyAsync())
        {
            return;
        }

        // Fixed seed: every fresh database gets the same people, so demos and docs stay predictable.
        var random = new Random(681);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var departments = DepartmentSeeds
            .Select(d => new Department { Name = d.Name, Description = d.Description })
            .ToList();
        db.Departments.AddRange(departments);

        var employees = new List<Employee>();
        var usedEmails = new HashSet<string>();

        for (var i = 0; i < 40; i++)
        {
            var seedIndex = i % DepartmentSeeds.Length;
            var seed = DepartmentSeeds[seedIndex];

            var first = FirstNames[random.Next(FirstNames.Length)];
            var last = LastNames[random.Next(LastNames.Length)];
            var email = $"{first}.{last}@workforce.example".ToLowerInvariant();
            if (!usedEmails.Add(email))
            {
                email = $"{first}.{last}{i}@workforce.example".ToLowerInvariant();
                usedEmails.Add(email);
            }

            employees.Add(new Employee
            {
                FirstName = first,
                LastName = last,
                Email = email,
                JobTitle = seed.Titles[random.Next(seed.Titles.Length)],
                Department = departments[seedIndex],
                HireDate = today.AddDays(-random.Next(30, 365 * 8)),
                Salary = Math.Round(random.Next(seed.MinSalary, seed.MaxSalary) / 500m) * 500m,
                IsActive = random.NextDouble() > 0.1,
                CreatedAt = DateTime.UtcNow
            });
        }

        db.Employees.AddRange(employees);

        // One shift per person per day at most, so seeded data never trips the overlap rule.
        for (var offset = -7; offset <= 14; offset++)
        {
            var day = today.AddDays(offset);
            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            foreach (var employee in employees.Where(e => e.IsActive))
            {
                if (random.NextDouble() > 0.45)
                {
                    continue;
                }

                var pattern = ShiftPatterns[random.Next(ShiftPatterns.Length)];
                var localStart = day.ToDateTime(new TimeOnly(pattern.StartHour, 0));
                var start = DateTime.SpecifyKind(localStart - LocalUtcOffset, DateTimeKind.Utc);

                employee.Shifts.Add(new Shift
                {
                    Title = pattern.Title,
                    Start = start,
                    End = start.AddHours(pattern.Hours)
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
