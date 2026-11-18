using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EnglishCenterManagement_CSW306_Project.DTOs;

namespace EnglishCenterManagement_CSW306_Project.Tests.Integration
{
    // Kiểm tra API Student và Class chạy thật (routing, validation, middleware lỗi) trên database InMemory.
    // Quan trọng nhất: định dạng trả về phải GIỐNG các module khác
    //   - thành công: trả thẳng dữ liệu; sửa / xóa: 204
    //   - lỗi: body { "message": "..." } với đúng mã 400 / 404 / 409
    public class StudentsClassesApiTests : ApiTestBase
    {
        private static CreateStudentDto NewStudent(string email) => new()
        {
            FullName = "Nguyen Van Test",
            DateOfBirth = new DateTime(2004, 5, 10),
            Phone = "0912345678",
            Email = email,
            Level = "B1"
        };

        private static async Task<string?> ReadMessageAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            return body.TryGetProperty("message", out var message) ? message.GetString() : null;
        }

        // ---------- Student ----------

        [Fact]
        public async Task GetStudents_Returns200WithPlainList()
        {
            var students = await Client.GetFromJsonAsync<List<StudentDto>>("/api/Students");

            Assert.Equal(3, students!.Count); // 3 học viên seed
        }

        [Fact]
        public async Task GetStudent_NotFound_Returns404WithMessage()
        {
            var response = await Client.GetAsync("/api/Students/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.False(string.IsNullOrEmpty(await ReadMessageAsync(response)));
        }

        [Fact]
        public async Task PostStudent_Valid_Returns201WithLocation()
        {
            var response = await Client.PostAsJsonAsync("/api/Students", NewStudent("moi@student.example"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<StudentDto>();
            Assert.Equal("moi@student.example", created!.Email);

            var fetched = await Client.GetFromJsonAsync<StudentDto>(response.Headers.Location);
            Assert.Equal(created.Id, fetched!.Id);
        }

        [Fact]
        public async Task PostStudent_DuplicateEmail_Returns409WithMessage()
        {
            var response = await Client.PostAsJsonAsync("/api/Students", NewStudent("an.le@student.example"));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("Email đã tồn tại", await ReadMessageAsync(response));
        }

        [Fact]
        public async Task PostStudent_MissingFullName_Returns400WithMessageAndErrors()
        {
            var dto = NewStudent("khongten@student.example");
            dto.FullName = null;

            var response = await Client.PostAsJsonAsync("/api/Students", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(body.TryGetProperty("message", out _));
            Assert.True(body.GetProperty("errors").GetArrayLength() > 0);
        }

        [Fact]
        public async Task PutStudent_Valid_Returns204AndUpdates()
        {
            var dto = NewStudent("an.le@student.example"); // giữ email cũ của học viên 1
            dto.FullName = "Lê Hoàng An (đã sửa)";

            var response = await Client.PutAsJsonAsync("/api/Students/1", dto);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            var fetched = await Client.GetFromJsonAsync<StudentDto>("/api/Students/1");
            Assert.Equal("Lê Hoàng An (đã sửa)", fetched!.FullName);
        }

        [Fact]
        public async Task DeleteStudent_WithEnrollment_Returns409()
        {
            // Học viên 1 đã ghi danh lớp 1 trong dữ liệu seed
            var response = await Client.DeleteAsync("/api/Students/1");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task DeleteStudent_WithoutEnrollment_Returns204()
        {
            var response = await Client.DeleteAsync("/api/Students/3");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync("/api/Students/3")).StatusCode);
        }

        // ---------- Class ----------

        [Fact]
        public async Task GetClasses_Returns200WithCourseAndTeacherNames()
        {
            var classes = await Client.GetFromJsonAsync<List<ClassDto>>("/api/Classes");

            Assert.Equal(2, classes!.Count); // 2 lớp seed
            var ielts = classes.Single(c => c.Id == 1);
            Assert.Equal("IELTS Foundation", ielts.CourseName);
            Assert.Equal("Nguyễn Thị Hoa", ielts.TeacherName);
        }

        [Fact]
        public async Task GetClassesByCourse_Returns200WithOnlyThatCourse()
        {
            var classes = await Client.GetFromJsonAsync<List<ClassDto>>("/api/Classes/by-course/1");

            var only = Assert.Single(classes!);
            Assert.Equal("IELTS-F01", only.Name);
        }

        [Fact]
        public async Task GetClassesByCourse_UnknownCourse_Returns404WithMessage()
        {
            var response = await Client.GetAsync("/api/Classes/by-course/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.False(string.IsNullOrEmpty(await ReadMessageAsync(response)));
        }

        [Fact]
        public async Task PostClass_EndDateBeforeStartDate_Returns400()
        {
            var dto = new CreateClassDto
            {
                Name = "LOP-SAI-NGAY",
                CourseId = 1,
                TeacherId = 1,
                Schedule = "T2 18:00",
                MaxStudents = 10,
                StartDate = new DateTime(2027, 3, 1),
                EndDate = new DateTime(2027, 1, 1)
            };

            var response = await Client.PostAsJsonAsync("/api/Classes", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
