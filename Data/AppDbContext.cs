using EnglishCenterManagement_CSW306_Project.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EnglishCenterManagement_CSW306_Project.Data
{
    // Kế thừa IdentityDbContext<AppUser> thay vì DbContext để hỗ trợ ASP.NET Identity
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Score> Scores { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Bắt buộc phải gọi khi dùng IdentityDbContext

            // ==========================================
            // 1. CẤU HÌNH FLUENT API (Ràng buộc dữ liệu)
            // ==========================================

            // Course: Tên khóa học là duy nhất, check constraint học phí & số buổi
            builder.Entity<Course>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Fee).HasPrecision(18, 2);
                entity.ToTable(t => t.HasCheckConstraint("CK_Course_Fee", "Fee >= 0"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Course_TotalSessions", "TotalSessions > 0"));
            });

            // Teacher: Email là duy nhất, UserId (nếu có) là duy nhất (quan hệ 1-1)
            builder.Entity<Teacher>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            // Student: Email là duy nhất, UserId (nếu có) là duy nhất (quan hệ 1-1)
            builder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            // Class: Sĩ số > 0, cấu hình xóa không xếp tầng (NoAction) để tránh lỗi cascade path
            builder.Entity<Class>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_Class_MaxStudents", "MaxStudents > 0"));

                entity.HasOne(c => c.Course)
                      .WithMany(course => course.Classes)
                      .HasForeignKey(c => c.CourseId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.Teacher)
                      .WithMany(t => t.Classes)
                      .HasForeignKey(c => c.TeacherId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Enrollment: 1 học viên không thể đăng ký cùng 1 lớp 2 lần
            builder.Entity<Enrollment>(entity =>
            {
                entity.HasIndex(e => new { e.StudentId, e.ClassId }).IsUnique();

                entity.HasOne(e => e.Student)
                      .WithMany(s => s.Enrollments)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Class)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(e => e.ClassId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Attendance: 1 học viên không thể điểm danh 2 lần trong cùng 1 ngày ở 1 lớp
            builder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(a => new { a.EnrollmentId, a.SessionDate }).IsUnique();
            });

            // Score: 1 học viên chỉ có 1 điểm thi giữa kỳ/cuối kỳ cho 1 lớp
            builder.Entity<Score>(entity =>
            {
                entity.HasIndex(s => new { s.EnrollmentId, s.Type }).IsUnique();
                entity.Property(s => s.Value).HasPrecision(4, 2);
                entity.ToTable(t => t.HasCheckConstraint("CK_Score_Value", "[Value] >= 0 AND [Value] <= 10"));
            });

            // ==========================================
            // 2. DATA SEEDING (Dữ liệu mẫu)
            // ==========================================

            builder.Entity<Course>().HasData(
                new Course { Id = 1, Name = "IELTS Foundation", Description = "Nền tảng IELTS cho người mới bắt đầu", Fee = 6000000, TotalSessions = 48 },
                new Course { Id = 2, Name = "TOEIC 450+", Description = "Luyện thi TOEIC cho sinh viên", Fee = 4500000, TotalSessions = 36 },
                new Course { Id = 3, Name = "Giao tiếp cơ bản", Description = "Tiếng Anh giao tiếp", Fee = 3000000, TotalSessions = 24 }
            );

            builder.Entity<Teacher>().HasData(
                new Teacher { Id = 1, FullName = "Nguyễn Thị Hoa", Specialization = "IELTS", Phone = "0901000001", Email = "hoa.nguyen@englishcenter.example" },
                new Teacher { Id = 2, FullName = "Trần Văn Minh", Specialization = "TOEIC và Giao tiếp", Phone = "0901000002", Email = "minh.tran@englishcenter.example" }
            );

            builder.Entity<Student>().HasData(
                new Student { Id = 1, FullName = "Lê Hoàng An", DateOfBirth = new DateTime(2005, 3, 12), Phone = "0912000001", Email = "an.le@student.example", Level = "A2" },
                new Student { Id = 2, FullName = "Phạm Gia Bảo", DateOfBirth = new DateTime(2004, 7, 25), Phone = "0912000002", Email = "bao.pham@student.example", Level = "A2" },
                new Student { Id = 3, FullName = "Vũ Ngọc Châu", DateOfBirth = new DateTime(2006, 11, 2), Phone = "0912000003", Email = "chau.vu@student.example", Level = "A1" }
            );

            builder.Entity<Class>().HasData(
                new Class { Id = 1, Name = "IELTS-F01", CourseId = 1, TeacherId = 1, Schedule = "T2, T4 18:00-19:30", MaxStudents = 15, StartDate = new DateTime(2026, 10, 5), EndDate = new DateTime(2027, 1, 29) },
                new Class { Id = 2, Name = "TOEIC-T01", CourseId = 2, TeacherId = 2, Schedule = "T3, T5 18:30-20:00", MaxStudents = 20, StartDate = new DateTime(2026, 10, 6), EndDate = new DateTime(2027, 1, 28) }
            );

            builder.Entity<Enrollment>().HasData(
                new Enrollment { Id = 1, StudentId = 1, ClassId = 1, EnrolledAt = new DateTime(2026, 9, 28, 9, 0, 0), Status = "Active" },
                new Enrollment { Id = 2, StudentId = 2, ClassId = 1, EnrolledAt = new DateTime(2026, 9, 28, 10, 15, 0), Status = "Active" }
            );
        }
    }
}