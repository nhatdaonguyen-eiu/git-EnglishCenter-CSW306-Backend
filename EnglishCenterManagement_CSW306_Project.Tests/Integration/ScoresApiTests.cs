using System.Net;
using System.Net.Http.Json;
using EnglishCenterManagement_CSW306_Project.Constants;
using EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs;

namespace EnglishCenterManagement_CSW306_Project.Tests.Integration
{
    public class ScoresApiTests : ApiTestBase
    {
        private Task<HttpResponseMessage> PostScore(int enrollmentId, string type, decimal value, string? comment = null) =>
            Client.PostAsJsonAsync("/api/Scores",
                new CreateScoreDto { EnrollmentId = enrollmentId, Type = type, Value = value, Comment = comment });

        // ---------- POST api/Scores ----------

        [Fact]
        public async Task Post_ValidScore_Returns201()
        {
            var response = await PostScore(1, "midterm", 8.5m, "Tốt");

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<ScoreDto>();
            Assert.Equal(ScoreType.Midterm, created!.Type); // "midterm" được chuẩn hóa
            Assert.Equal(8.5m, created.Value);
            Assert.Equal("Lê Hoàng An", created.StudentName);
            Assert.Equal("IELTS-F01", created.ClassName);
        }

        [Fact]
        public async Task Post_ValueOutOfRange_Returns400AndSavesNothing()
        {
            var response = await PostScore(1, ScoreType.Midterm, 11m);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // bị chặn ở model validation
            WithDb(db => Assert.Empty(db.Scores));
        }

        [Fact]
        public async Task Post_SameTypeTwiceForOneEnrollment_Returns409()
        {
            await PostScore(1, ScoreType.Final, 7m);

            var response = await PostScore(1, ScoreType.Final, 9m);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            WithDb(db => Assert.Equal(7m, db.Scores.Single().Value)); // điểm cũ không bị ghi đè
        }

        // ---------- GET api/Scores/transcript/student/{studentId} ----------

        [Fact]
        public async Task Transcript_ReturnsScoresGroupedByClass()
        {
            await PostScore(1, ScoreType.Midterm, 6.5m);
            await PostScore(1, ScoreType.Final, 8m);
            await PostScore(2, ScoreType.Final, 10m); // điểm của học viên khác, không được lẫn vào

            var transcript = await Client.GetFromJsonAsync<TranscriptDto>("/api/Scores/transcript/student/1");

            Assert.Equal("Lê Hoàng An", transcript!.StudentName);
            var ielts = Assert.Single(transcript.Classes);
            Assert.Equal("IELTS-F01", ielts.ClassName);
            Assert.Equal(2, ielts.Scores.Count);
            Assert.Equal(6.5m, ielts.Scores.Single(s => s.Type == ScoreType.Midterm).Value);
            Assert.Equal(8m, ielts.Scores.Single(s => s.Type == ScoreType.Final).Value);
        }

        [Fact]
        public async Task Transcript_StudentNotFound_Returns404()
        {
            var response = await Client.GetAsync("/api/Scores/transcript/student/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
