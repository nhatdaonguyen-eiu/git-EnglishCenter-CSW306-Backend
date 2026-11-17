using System.Net;
using System.Net.Http.Json;
using EnglishCenterManagement_CSW306_Project.Constants;
using EnglishCenterManagement_CSW306_Project.DTOs.EnrollmentDTOs;

namespace EnglishCenterManagement_CSW306_Project.Tests.Integration
{
    public class EnrollmentsApiTests : ApiTestBase
    {
        private static CreateEnrollmentDto Dto(int studentId, int classId) =>
            new() { StudentId = studentId, ClassId = classId };

        // ---------- POST api/Enrollments ----------

        [Fact]
        public async Task Post_ValidStudentAndClass_Returns201WithLocation()
        {
            var response = await Client.PostAsJsonAsync("/api/Enrollments", Dto(3, 1));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<EnrollmentDto>();
            Assert.Equal(EnrollmentStatus.Active, created!.Status);
            Assert.Equal("Vũ Ngọc Châu", created.StudentName);
            Assert.Equal("IELTS-F01", created.ClassName);

            // Header Location phải trỏ đúng tới bản ghi vừa tạo
            var fetched = await Client.GetFromJsonAsync<EnrollmentDto>(response.Headers.Location);
            Assert.Equal(created.Id, fetched!.Id);
        }

        [Fact]
        public async Task Post_StudentAlreadyInClass_Returns409()
        {
            // Học viên 1 đã ghi danh lớp 1 trong dữ liệu seed
            var response = await Client.PostAsJsonAsync("/api/Enrollments", Dto(1, 1));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            WithDb(db => Assert.Equal(2, db.Enrollments.Count())); // không tạo thêm bản ghi
        }

        [Fact]
        public async Task Post_ClassIsFull_Returns409()
        {
            WithDb(db =>
            {
                db.Classes.Find(2)!.MaxStudents = 1;
                db.SaveChanges();
            });
            var first = await Client.PostAsJsonAsync("/api/Enrollments", Dto(1, 2)); // lớp 2 đầy 1/1
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            var response = await Client.PostAsJsonAsync("/api/Enrollments", Dto(2, 2));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // ---------- PUT api/Enrollments/{id}/cancel ----------

        [Fact]
        public async Task Cancel_ActiveEnrollment_Returns200ThenSecondCancelReturns400()
        {
            var first = await Client.PutAsync("/api/Enrollments/1/cancel", null);

            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
            var cancelled = await first.Content.ReadFromJsonAsync<EnrollmentDto>();
            Assert.Equal(EnrollmentStatus.Cancelled, cancelled!.Status);

            var second = await Client.PutAsync("/api/Enrollments/1/cancel", null);
            Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        }

        // ---------- PUT api/Enrollments/{id}/transfer ----------

        [Fact]
        public async Task Transfer_ToAnotherClass_Returns200AndCancelsOldEnrollment()
        {
            var response = await Client.PutAsJsonAsync("/api/Enrollments/1/transfer",
                new TransferEnrollmentDto { NewClassId = 2 });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var moved = await response.Content.ReadFromJsonAsync<EnrollmentDto>();
            Assert.Equal(2, moved!.ClassId);
            Assert.Equal(1, moved.StudentId);
            Assert.Equal(EnrollmentStatus.Active, moved.Status);

            var old = await Client.GetFromJsonAsync<EnrollmentDto>("/api/Enrollments/1");
            Assert.Equal(EnrollmentStatus.Cancelled, old!.Status);
        }

        [Fact]
        public async Task Transfer_ToSameClass_Returns400AndKeepsEnrollmentActive()
        {
            var response = await Client.PutAsJsonAsync("/api/Enrollments/1/transfer",
                new TransferEnrollmentDto { NewClassId = 1 });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var current = await Client.GetFromJsonAsync<EnrollmentDto>("/api/Enrollments/1");
            Assert.Equal(EnrollmentStatus.Active, current!.Status);
        }
    }
}
