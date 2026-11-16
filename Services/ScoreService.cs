using EnglishCenterManagement_CSW306_Project.Constants;
using EnglishCenterManagement_CSW306_Project.Data;
using EnglishCenterManagement_CSW306_Project.DTOs.ScoreDTOs;
using EnglishCenterManagement_CSW306_Project.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public class ScoreService : IScoreService
    {
        private readonly AppDbContext _context;

        public ScoreService(AppDbContext context)
        {
            _context = context;
        }

        private static readonly Expression<Func<Score, ScoreDto>> ToDto = s => new ScoreDto
        {
            Id = s.Id,
            EnrollmentId = s.EnrollmentId ?? 0,
            StudentId = s.Enrollment!.StudentId,
            StudentName = s.Enrollment.Student!.FullName,
            ClassId = s.Enrollment.ClassId,
            ClassName = s.Enrollment.Class!.Name,
            Type = s.Type,
            Value = s.Value ?? 0,
            Comment = s.Comment
        };

        // ===================== CRUD =====================

        public async Task<IEnumerable<ScoreDto>> GetScoresAsync(int? enrollmentId, int? classId, int? studentId, string? type)
        {
            var query = _context.Scores.AsQueryable();

            if (enrollmentId.HasValue) query = query.Where(s => s.EnrollmentId == enrollmentId.Value);
            if (classId.HasValue) query = query.Where(s => s.Enrollment!.ClassId == classId.Value);
            if (studentId.HasValue) query = query.Where(s => s.Enrollment!.StudentId == studentId.Value);
            if (!string.IsNullOrWhiteSpace(type))
            {
                // Gõ "final" hay "Final" đều được; loại lạ thì Normalize trả null -> không có kết quả
                var normalized = ScoreType.Normalize(type) ?? type;
                query = query.Where(s => s.Type == normalized);
            }

            return await query
                .OrderBy(s => s.Enrollment!.StudentId)
                .ThenBy(s => s.Type)
                .Select(ToDto)
                .ToListAsync();
        }

        public async Task<ScoreDto?> GetScoreByIdAsync(int id)
        {
            return await _context.Scores
                .Where(s => s.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync();
        }

        public async Task<FeatureResult<ScoreDto>> CreateScoreAsync(CreateScoreDto dto)
        {
            var type = ScoreType.Normalize(dto.Type);
            if (type == null)
                return FeatureResult<ScoreDto>.Invalid($"Loại điểm không hợp lệ. Chỉ chấp nhận: {string.Join(", ", ScoreType.All)}");

            var enrollment = await _context.Enrollments.FindAsync(dto.EnrollmentId);
            if (enrollment == null)
                return FeatureResult<ScoreDto>.NotFound($"Không tìm thấy ghi danh có Id = {dto.EnrollmentId}");

            if (enrollment.Status == EnrollmentStatus.Cancelled)
                return FeatureResult<ScoreDto>.Invalid("Ghi danh này đã bị hủy, không thể nhập điểm");

            var duplicated = await _context.Scores
                .AnyAsync(s => s.EnrollmentId == dto.EnrollmentId && s.Type == type);
            if (duplicated)
                return FeatureResult<ScoreDto>.Conflict($"Học viên đã có điểm {type} ở lớp này, hãy dùng PUT để sửa");

            var score = new Score
            {
                EnrollmentId = dto.EnrollmentId,
                Type = type,
                Value = dto.Value,
                Comment = dto.Comment
            };
            _context.Scores.Add(score);
            if (!await _context.TrySaveChangesAsync())
                return FeatureResult<ScoreDto>.Conflict("Dữ liệu vừa được người khác thay đổi (có thể đã bị trùng), vui lòng thử lại");

            var result = await GetScoreByIdAsync(score.Id);
            return FeatureResult<ScoreDto>.Ok(result!);
        }

        public async Task<FeatureResult<bool>> UpdateScoreAsync(int id, UpdateScoreDto dto)
        {
            var score = await _context.Scores.FindAsync(id);
            if (score == null)
                return FeatureResult<bool>.NotFound($"Không tìm thấy điểm có Id = {id}");

            score.Value = dto.Value;
            score.Comment = dto.Comment;
            await _context.SaveChangesAsync();
            return FeatureResult<bool>.Ok(true);
        }

        public async Task<FeatureResult<bool>> DeleteScoreAsync(int id)
        {
            var score = await _context.Scores.FindAsync(id);
            if (score == null)
                return FeatureResult<bool>.NotFound($"Không tìm thấy điểm có Id = {id}");

            _context.Scores.Remove(score);
            await _context.SaveChangesAsync();
            return FeatureResult<bool>.Ok(true);
        }

        // ===================== Bảng điểm =====================

        // Mỗi lớp học viên tham gia là 1 mục, kèm điểm giữa kỳ / cuối kỳ của lớp đó.
        // (Chưa tính điểm trung bình vì đề chưa quy định trọng số giữa kỳ / cuối kỳ.)
        public async Task<FeatureResult<TranscriptDto>> GetTranscriptAsync(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
                return FeatureResult<TranscriptDto>.NotFound($"Không tìm thấy học viên có Id = {studentId}");

            var classes = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .OrderBy(e => e.Class!.Name)
                .Select(e => new TranscriptClassDto
                {
                    EnrollmentId = e.Id,
                    ClassId = e.ClassId,
                    ClassName = e.Class!.Name,
                    EnrollmentStatus = e.Status,
                    Scores = e.Scores
                        .OrderBy(s => s.Type)
                        .Select(s => new TranscriptScoreDto
                        {
                            Id = s.Id,
                            Type = s.Type,
                            Value = s.Value ?? 0,
                            Comment = s.Comment
                        }).ToList()
                }).ToListAsync();

            return FeatureResult<TranscriptDto>.Ok(new TranscriptDto
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                Classes = classes
            });
        }
    }
}
