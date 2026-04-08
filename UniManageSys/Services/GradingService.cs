using Microsoft.EntityFrameworkCore;
using UniManageSys.Data;
using UniManageSys.Models;

namespace UniManageSys.Services
{
    public class GradingService : IGradingService
    {
        private readonly ApplicationDbContext _context;

        public GradingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GradingResult> ProcessScoreAsync(int registrationId, int lecturerId, decimal caScore, decimal examScore)
        {
            // 1. Validate Input Ranges
            if (caScore < 0 || caScore > 30) return new GradingResult { IsSuccess = false, Message = "CA Score must be between 0 and 30." };
            if (examScore < 0 || examScore > 70) return new GradingResult { IsSuccess = false, Message = "Exam Score must be between 0 and 70." };

            // 2. Verify the Registration exists
            var registration = await _context.CourseRegistrations
                .Include(cr => cr.Result)
                .FirstOrDefaultAsync(cr => cr.Id == registrationId);

            if (registration == null) return new GradingResult { IsSuccess = false, Message = "Course registration not found." };

            // 3. Calculate Total and Nigerian NUC Grade
            decimal totalScore = caScore + examScore;
            string grade;
            decimal gradePoint;

            if (totalScore >= 70)
            {
                grade = "A";
                gradePoint = 5.0m;
            }
            else if (totalScore >= 60)
            {
                grade = "B";
                gradePoint = 4.0m;
            }
            else if (totalScore >= 50)
            {
                grade = "C";
                gradePoint = 3.0m;
            }
            else if (totalScore >= 45)
            {
                grade = "D";
                gradePoint = 2.0m;
            }
            else if (totalScore >= 40)
            {
                grade = "E";
                gradePoint = 1.0m;
            }
            else
            {
                grade = "F";
                gradePoint = 0.0m;
            }

            // 4. Update or Create the Result Record
            if (registration.Result != null)
            {
                // Update existing score (e.g., lecturer made a correction)
                registration.Result.ContinuousAssessment = caScore;
                registration.Result.ExamScore = examScore;
                registration.Result.TotalScore = totalScore;
                registration.Result.Grade = grade;
                registration.Result.GradePoint = gradePoint;
                registration.Result.GradedByLecturerId = lecturerId;
                registration.Result.DateUploaded = DateTime.UtcNow;
            }
            else
            {
                // Insert brand new score
                var newResult = new StudentResult
                {
                    CourseRegistrationId = registrationId,
                    GradedByLecturerId = lecturerId,
                    ContinuousAssessment = caScore,
                    ExamScore = examScore,
                    TotalScore = totalScore,
                    Grade = grade,
                    GradePoint = gradePoint,
                    IsPublished = false // Hidden from student until Senate/HOD approval
                };
                _context.StudentResults.Add(newResult);
            }

            await _context.SaveChangesAsync();

            return new GradingResult
            {
                IsSuccess = true,
                Message = $"Score saved: {totalScore} ({grade})."
            };
        }
    }
}