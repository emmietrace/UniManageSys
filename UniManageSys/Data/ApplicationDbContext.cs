using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using UniManageSys.Models;

namespace UniManageSys.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<ClassSession> ClassSessions { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<TimetableEvent> TimetableEvents { get; set; }
        public DbSet<StudentResult> StudentResults { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<CourseAssignment> CourseAssignments { get; set; }
        public DbSet<CourseRegistration> CourseRegistrations { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<Semester> Semesters { get; set; }

        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Programme> Programmes { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Creating a composite PK for Prerequisites
            // Ensures you can't add exact same prerequisite twice to a course
            builder.Entity<CoursePrerequisite>()
                .HasKey(cp => new
                {
                    cp.CourseId,
                    cp.PrerequisiteId
                });
            // Configure the first half of the rel.
            builder.Entity<CoursePrerequisite>()
                .HasOne(cp => cp.Course)
                .WithMany(c => c.Prerequisites)
                .HasForeignKey(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure second half of the rel.
            builder.Entity<CoursePrerequisite>()
                .HasOne(cp => cp.Prerequisite)
                .WithMany()
                .HasForeignKey(cp => cp.PrerequisiteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enforcing Unique Matric Numbers
            builder.Entity<Student>()
                .HasIndex(s => s.MatriculationNumber)
                .IsUnique()
                .HasFilter("[MatriculationNumber] IS NOT NULL");

            builder.Entity<Student>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            // 6. Prevent Double Course Registration
            builder.Entity<CourseRegistration>()
                .HasIndex(cr => new { cr.StudentId, cr.CourseId, cr.SemesterId })
                .IsUnique();

            // 7. Prevent cascade delete crashes on Registrations
            builder.Entity<CourseRegistration>()
                .HasOne(cr => cr.Student)
                .WithMany()
                .HasForeignKey(cr => cr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseRegistration>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.CourseRegistrations)
                .HasForeignKey(cr => cr.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. Enforce Unique Staff ID
            builder.Entity<Lecturer>()
                .HasIndex(l => l.StaffId)
                .IsUnique();

            // 9. Enforce 1-to-1 relationship between User and Lecturer Profile
            builder.Entity<Lecturer>()
                .HasIndex(l => l.UserId)
                .IsUnique();

            // 10. Prevent Duplicate Course Assignments
            // A lecturer cannot be assigned to the exact same course twice in the same semester
            builder.Entity<CourseAssignment>()
                .HasIndex(ca => new { ca.LecturerId, ca.CourseId, ca.SemesterId })
                .IsUnique();

            // 11. Prevent cascade delete crashes on Assignments
            builder.Entity<CourseAssignment>()
                .HasOne(ca => ca.Lecturer)
                .WithMany(l => l.CourseAssignments)
                .HasForeignKey(ca => ca.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseAssignment>()
                .HasOne(ca => ca.Course)
                .WithMany(c => c.CourseAssignments)
                .HasForeignKey(ca => ca.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 12. Enforce 1-to-1 relationship between Registration and Result
            builder.Entity<StudentResult>()
                .HasIndex(sr => sr.CourseRegistrationId)
                .IsUnique();

            // 13. Prevent cascade delete crashes on Results
            builder.Entity<StudentResult>()
                .HasOne(sr => sr.Registration)
                .WithOne(cr => cr.Result)
                .HasForeignKey<StudentResult>(sr => sr.CourseRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentResult>()
                .HasOne(sr => sr.GradedBy)
                .WithMany()
                .HasForeignKey(sr => sr.GradedByLecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TimetableEvent>()
            .HasOne(t => t.Lecturer)
            .WithMany()
            .HasForeignKey(t => t.LecturerId)
            .OnDelete(DeleteBehavior.Restrict); // No cascade delete

            builder.Entity<TimetableEvent>()
                .HasOne(t => t.Course)
                .WithMany()
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TimetableEvent>()
                .HasOne(t => t.Venue)
                .WithMany()
                .HasForeignKey(t => t.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TimetableEvent>()
                .HasOne(t => t.Semester)
                .WithMany()
                .HasForeignKey(t => t.SemesterId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.Entity<Venue>().HasData(
                new Venue { Id = 1, Name = "CST Lecture Theatre 1", Capacity = 200, IsLab = false },
                new Venue { Id = 2, Name = "Main Auditorium", Capacity = 1000, IsLab = false },
                new Venue { Id = 3, Name = "Advanced Programming Lab", Capacity = 50, IsLab = true },
                new Venue { Id = 4, Name = "Engineering Block A, Room 101", Capacity = 100, IsLab = false }
            );

            
            builder.Entity<Faculty>().HasData(
                new Faculty { Id = 2, Code = "ENG", Name = "Faculty of Engineering" },
                new Faculty { Id = 3, Code = "SMS", Name = "Faculty of Social and Management Sciences" },
                new Faculty { Id = 4, Code = "ART", Name = "Faculty of Arts and Humanities" },
                new Faculty { Id = 5, Code = "MED", Name = "Faculty of Basic Medical Sciences" },
                new Faculty { Id = 6, Code = "LAW", Name = "Faculty of Law" },
                new Faculty { Id = 7, Code = "EDU", Name = "Faculty of Education" },
                new Faculty { Id = 8, Code = "AGR", Name = "Faculty of Agriculture" },
                new Faculty { Id = 9, Code = "ENV", Name = "Faculty of Environmental Sciences" },
                new Faculty { Id = 10, Code = "CIT", Name = "Faculty of Computing and Information Technology" }
            );

            builder.Entity<AttendanceRecord>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
