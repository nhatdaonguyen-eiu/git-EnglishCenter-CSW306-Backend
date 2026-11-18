# git-EnglishCenter-CSW306-Backend
# English Center Management API

Backend REST API for managing an English language center: courses, teachers, students, classes, enrollments, attendance and scores.

Built for the **CSW306 – Back-End Development** project at Eastern International University (School of Computing and Information Technology).

- **Stack:** ASP.NET Core Web API (.NET 8), Entity Framework Core 8 (Code First), SQL Server, ASP.NET Core Identity + JWT, Swagger, xUnit
- **Scope:** backend only (API, database, tests). No front end.

---

## Table of contents

1. [Features](#features)
2. [Architecture](#architecture)
3. [Database](#database)
4. [Getting started](#getting-started)
5. [API overview](#api-overview)
6. [Business rules](#business-rules)
7. [Error handling](#error-handling)
8. [Security](#security)
9. [Testing](#testing)
10. [Project status](#project-status)
11. [Team](#team)
12. [Git workflow](#git-workflow)

---

## Features

| Area | What it does |
|---|---|
| Authentication | Register and log in, JWT access token, roles `Admin`, `Teacher`, `Student`, profile view and update |
| Courses, teachers | CRUD |
| Students, classes | CRUD; list classes by course or by teacher |
| Enrollment | Enroll a student in a class, block duplicates, check class capacity, cancel, transfer to another class |
| Attendance | Record `Present` / `Absent` / `Excused` per session; statistics per student and per class |
| Scores | Midterm and Final scores (0–10); student transcript |
| Documentation | Swagger UI |

## Architecture

The API uses a layered structure. Each layer talks only to the layer below it, and every dependency is injected through an interface.

```
Client (Swagger / Postman)
   │
Controllers      receive requests, model validation, turn results into HTTP responses
   │
Services         business rules
   │
Repository / AppDbContext     data access with EF Core
   │
SQL Server
```

- **DTOs** define exactly what goes in and out of the API. Entities are never exposed directly.
- Simple CRUD modules use the generic `IRepository<T>`. The Enrollment, Attendance and Score services use `AppDbContext` directly because they need joins, counts and several changes saved in one call.
- Services are registered in `Program.cs`; the Enrollment, Attendance and Score services are registered with one line: `builder.Services.AddEnrollmentAttendanceScoreServices();`

### Folder structure

| Folder | Contents |
|---|---|
| `Controllers/` | One controller per resource |
| `Services/` | Service interfaces and implementations (business rules) |
| `Repositories/` | Generic `IRepository<T>` and `Repository<T>` |
| `DTOs/` | Request and response objects with validation attributes |
| `Entities/` | Database entities |
| `Data/` | `AppDbContext`: Fluent API configuration and seed data |
| `Migrations/` | EF Core migrations |
| `Constants/` | Allowed values for statuses and score types |
| `EnglishCenterManagement_CSW306_Project.Tests/` | Test project |

## Database

Code First with EF Core migrations. Main tables: `Courses`, `Teachers`, `Students`, `Classes`, `Enrollments`, `Attendances`, `Scores`, plus the ASP.NET Identity tables.

### Relationships

- `Course` 1 – N `Class`, `Teacher` 1 – N `Class`
- `Student` N – N `Class` through `Enrollment` (which also stores the enrollment date and status)
- `Enrollment` 1 – N `Attendance`, `Enrollment` 1 – N `Score`
- `Student` / `Teacher` can be linked to one `AppUser` (login account)

### Constraints enforced in the database

- **Unique indexes:** course name, student email, teacher email, `(StudentId, ClassId)`, `(EnrollmentId, SessionDate)`, `(EnrollmentId, Type)`
- **Check constraints:** `Fee >= 0`, `TotalSessions > 0`, `MaxStudents > 0`, score value between 0 and 10
- **Delete behavior:** `No Action` on class, student and enrollment foreign keys; attendance and scores are deleted with their enrollment (cascade)

### Seed data

The `InitialCreate` migration inserts sample data for the demo:

- 3 courses, 2 teachers, 3 students, 2 classes
- 2 enrollments (students 1 and 2 are enrolled in class 1)

## Getting started

### Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express or Developer edition)
- EF Core command-line tool: `dotnet tool install --global dotnet-ef`
- Visual Studio 2022 or later, or Visual Studio Code (optional)

### Steps

1. **Clone the repository** and switch to the `main` branch.

   ```bash
   git clone <repository-url>
   cd EnglishCenterManagement_CSW306_Project
   ```

2. **Set the connection string.** Copy the `DefaultConnection` entry from `appsettings.json` into `appsettings.Development.json` (this file is not committed) and fill in your SQL Server name, user and password. Example:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=EnglishCenterDb;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

3. **Create the database** (this also inserts the sample data):

   ```bash
   dotnet ef database update
   ```

4. **Run the API:**

   ```bash
   dotnet run
   ```

   or press `F5` in Visual Studio.

5. **Open Swagger UI** at <http://localhost:5289/swagger> to see and try every endpoint.

> The port comes from the project's launch settings. If your API starts on a different port, use the one printed in the console.

## API overview

All endpoints are under `/api`, use JSON, and follow REST conventions: `GET` reads, `POST` creates, `PUT` updates, `DELETE` removes. List endpoints accept optional query filters.

| Module | Base route | Endpoints |
|---|---|---|
| Auth | `/api/Auth` | 2 |
| Profile | `/api/Profile` | 3 |
| Courses | `/api/Courses` | 5 |
| Teachers | `/api/Teachers` | 5 |
| Students | `/api/Students` | 5 |
| Classes | `/api/Classes` | 7 |
| Enrollments | `/api/Enrollments` | 5 |
| Attendances | `/api/Attendances` | 7 |
| Scores | `/api/Scores` | 6 |
| **Total** | | **45** |

### Enrollments

| Method | Route | Description |
|---|---|---|
| GET | `/api/Enrollments?studentId=&classId=&status=` | List enrollments (filters are optional) |
| GET | `/api/Enrollments/{id}` | Get one enrollment |
| POST | `/api/Enrollments` | Enroll a student in a class |
| PUT | `/api/Enrollments/{id}/cancel` | Cancel an enrollment |
| PUT | `/api/Enrollments/{id}/transfer` | Transfer to another class (returns the new enrollment) |

### Attendances

| Method | Route | Description |
|---|---|---|
| GET | `/api/Attendances?enrollmentId=&classId=&studentId=&date=` | List attendance records |
| GET | `/api/Attendances/{id}` | Get one record |
| POST | `/api/Attendances` | Record attendance |
| PUT | `/api/Attendances/{id}` | Update date or status |
| DELETE | `/api/Attendances/{id}` | Delete a record |
| GET | `/api/Attendances/stats/student/{studentId}` | Attendance statistics of one student, per class |
| GET | `/api/Attendances/stats/class/{classId}` | Attendance statistics of one class, per student |

### Scores

| Method | Route | Description |
|---|---|---|
| GET | `/api/Scores?enrollmentId=&classId=&studentId=&type=` | List scores |
| GET | `/api/Scores/{id}` | Get one score |
| POST | `/api/Scores` | Enter a score |
| PUT | `/api/Scores/{id}` | Change value and comment |
| DELETE | `/api/Scores/{id}` | Delete a score |
| GET | `/api/Scores/transcript/student/{studentId}` | Transcript: every class of the student with its scores |

### Example requests

Enroll student 3 in class 1:

```http
POST /api/Enrollments
Content-Type: application/json

{ "studentId": 3, "classId": 1 }
```

Transfer enrollment 1 to class 2:

```http
PUT /api/Enrollments/1/transfer
Content-Type: application/json

{ "newClassId": 2 }
```

Record attendance:

```http
POST /api/Attendances
Content-Type: application/json

{ "enrollmentId": 1, "sessionDate": "2026-10-05", "status": "Present" }
```

Enter a final score:

```http
POST /api/Scores
Content-Type: application/json

{ "enrollmentId": 1, "type": "Final", "value": 8.5, "comment": "Good work" }
```

## Business rules

- A student cannot be enrolled in the same class twice while the first enrollment is `Active` or `Completed`.
- A class cannot have more `Active` enrollments than its `MaxStudents`.
- Only an `Active` enrollment can be cancelled or transferred. A cancelled enrollment frees a seat.
- A transfer cancels the old enrollment and creates the new one in a single save. If the new class is full, nothing changes.
- If a student re-enrolls in a class they cancelled earlier, the old record is reused (the database allows one row per student and class).
- A student has at most one attendance record per class per day. Only the date is stored, not the time.
- Attendance and scores cannot be added to a cancelled enrollment.
- Each enrollment has at most one `Midterm` and one `Final` score. A second score of the same type returns `409`; use `PUT` to change it.
- Scores are between 0 and 10.
- Attendance rate = `Present` sessions ÷ recorded sessions × 100, rounded to one decimal. `Excused` does not count as present. An enrollment with no sessions has a rate of 0.

## Error handling

Services return a result object with an error type, and the controller base class turns it into an HTTP response with the body `{ "message": "..." }`.

| Code | Meaning |
|---|---|
| 200 OK | Read or action succeeded; the body contains the data |
| 201 Created | A record was created; the `Location` header points to it |
| 204 No Content | Update or delete succeeded; no body |
| 400 Bad Request | Invalid input, or an action not allowed in the current state |
| 401 Unauthorized | Missing, invalid or expired token |
| 403 Forbidden | Logged in, but the role is not allowed |
| 404 Not Found | The requested record does not exist |
| 409 Conflict | A uniqueness or capacity rule is broken (duplicate enrollment, class full, duplicate score) |
| 500 Internal Server Error | Unexpected error; details are logged, not shown to the client |

If two requests break a unique index at the same moment, the service catches the database error and returns `409` instead of `500`.

## Security

| Concern | How it is handled |
|---|---|
| Password storage | ASP.NET Identity stores salted PBKDF2 hashes |
| Authentication | JWT signed with HMAC-SHA256; key, issuer, audience and lifetime come from configuration |
| Authorization | `[Authorize]` and `[Authorize(Roles = ...)]` on controllers and actions |
| SQL injection | All data access goes through EF Core LINQ (parameterized queries); no raw SQL |
| XSS | The API only returns JSON and never renders HTML |
| CSRF | Not applicable: no HTML forms or cookie logins; the token is sent in the `Authorization` header |
| Input validation | Data annotations on every DTO; `[ApiController]` returns `400` for invalid input |
| Data integrity | Unique indexes, check constraints and `No Action` deletes in the database |
| Secrets | The committed `appsettings.json` only holds a placeholder; real values stay on each machine |

User roles:

| Role | Main permissions |
|---|---|
| Admin | Manage courses, teachers, students and classes; enroll, cancel and transfer; view everything |
| Teacher | View their classes; record attendance; enter and update scores |
| Student | View their classes, attendance and transcript; update their own profile |

## Testing

Tests use xUnit, Moq and the EF Core InMemory provider. **No test needs SQL Server.**

| Level | What is real | What is replaced |
|---|---|---|
| Service unit tests | The service | Repository mocked with Moq, or an InMemory database |
| Controller unit tests | The controller | Service mocked with Moq |
| Integration tests | The whole app (routing, validation, controller, service, EF Core) | SQL Server replaced by InMemory, using `WebApplicationFactory` |

Run the tests from the repository folder:

```bash
dotnet test EnglishCenterManagement_CSW306_Project.Tests
dotnet test EnglishCenter.Tests
```

In Visual Studio: **Test → Test Explorer → Run All**.

43 automated tests in total (27 unit, 16 integration).

## Project status

| Item | Status |
|---|---|
| CRUD, relationships, REST, validation, migrations, seed data | Done |
| Enrollment, Attendance, Score modules | Done |
| Unit and integration tests | Done |
| Registration, login, JWT, role-based authorization, profile | In progress: still being wired into `Program.cs`; `[Authorize]` still to be enabled on all controllers |
| Swagger UI | Done (JWT "Authorize" button still to be added) |
| Postman collection | Not started |
| Bonus: pagination, file upload, caching, API versioning, Docker, CI/CD | Not started (logging partly done by the global error handler) |

### Planned work

- Finish Identity and JWT and protect every controller with the right roles
- One error response format across all modules
- Pagination, filtering and sorting on list endpoints
- Avatar upload, caching for the course list, request logging
- Dockerfile and a GitHub Actions pipeline that runs the tests

## Team

| Member | Role | Main work |
|---|---|---|
| Đào Nguyễn Long Nhật | Data Lead | Entities, `AppDbContext`, Fluent API, generic repository, migrations, seed data; Course and Teacher APIs |
| Lê Đoàn Minh Nhật | Security Lead | ASP.NET Identity, JWT, roles, input validation; Auth and Profile APIs |
| Bạch Gia Huy | Core API Lead | Common response format and global error handling; Student and Class APIs; StudentService tests |
| Nguyễn Thành Như | Feature Lead | Enrollment, Attendance and Score APIs; EnrollmentService tests |
| Phạm Thanh Long | QA, Docs and Git Lead | Git workflow, Swagger, Postman, controller and integration tests, report and demo |
| Phạm Tuyên | Docs, Presentation | README, presentation and demo |

## Git workflow

- `main`: the version that is submitted and demonstrated
- `develop`: integration of everyone's work
- `feature/...`: one branch per task, merged into `develop` through a pull request reviewed by another member
- Commit messages use a prefix: `feat:`, `fix:`, `test:`, `docs:`
- Tell the group before editing shared files such as `Program.cs` or `AppDbContext.cs` to avoid merge conflicts
