using System.Net;
using System.Net.Http.Json;
using EnglishCenterManagement_CSW306_Project.Constants;
using EnglishCenterManagement_CSW306_Project.DTOs.AttendanceDTOs;

namespace EnglishCenterManagement_CSW306_Project.Tests.Integration
{
    public class AttendancesApiTests : ApiTestBase
    {
        private Task<HttpResponseMessage> PostAttendance(int enrollmentId, DateTime sessionDate, string status) =>
            Client.PostAsJsonAsync("/api/Attendances",
                new CreateAttendanceDto { EnrollmentId = enrollmentId, SessionDate = sessionDate, Status = status });

        // ---------- POST api/Attendances ----------

        [Fact]
        public async Task Post_ValidAttendance_Returns201WithDateOnlyAndNormalizedStatus()
        {
            // Gửi kèm giờ và trạng thái viết thường
            var response = await PostAttendance(1, new DateTime(2026, 10, 5, 18, 30, 0), "present");

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<AttendanceDto>();
            Assert.Equal(new DateTime(2026, 10, 5), created!.SessionDate); // chỉ giữ phần ngày
            Assert.Equal(AttendanceStatus.Present, created.Status);
            Assert.Equal("Lê Hoàng An", created.StudentName);
        }

        [Fact]
        public async Task Post_SameEnrollmentSameDayTwice_Returns409()
        {
            await PostAttendance(1, new DateTime(2026, 10, 5, 18, 0, 0), AttendanceStatus.Present);

            // Cùng ngày, khác giờ vẫn tính là trùng
            var response = await PostAttendance(1, new DateTime(2026, 10, 5, 19, 0, 0), AttendanceStatus.Absent);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            WithDb(db => Assert.Single(db.Attendances));
        }

        [Fact]
        public async Task Post_UnknownStatus_Returns400()
        {
            var response = await PostAttendance(1, new DateTime(2026, 10, 5), "Late");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            WithDb(db => Assert.Empty(db.Attendances));
        }

        // ---------- GET api/Attendances/stats/class/{classId} ----------

        [Fact]
        public async Task StatsByClass_CountsEachStatusAndComputesAttendanceRate()
        {
            // Học viên 1 (ghi danh 1): 2 buổi có mặt, 1 buổi vắng. Học viên 2 (ghi danh 2): chưa điểm danh.
            await PostAttendance(1, new DateTime(2026, 10, 5), AttendanceStatus.Present);
            await PostAttendance(1, new DateTime(2026, 10, 7), AttendanceStatus.Present);
            await PostAttendance(1, new DateTime(2026, 10, 12), AttendanceStatus.Absent);

            var stats = await Client.GetFromJsonAsync<List<AttendanceStatDto>>("/api/Attendances/stats/class/1");

            Assert.Equal(2, stats!.Count);
            var an = stats.Single(s => s.StudentId == 1);
            Assert.Equal(3, an.TotalSessions);
            Assert.Equal(2, an.PresentCount);
            Assert.Equal(1, an.AbsentCount);
            Assert.Equal(66.7, an.AttendanceRate);

            var bao = stats.Single(s => s.StudentId == 2);
            Assert.Equal(0, bao.TotalSessions);
            Assert.Equal(0, bao.AttendanceRate); // không chia cho 0
        }

        [Fact]
        public async Task StatsByClass_ClassNotFound_Returns404()
        {
            var response = await Client.GetAsync("/api/Attendances/stats/class/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
