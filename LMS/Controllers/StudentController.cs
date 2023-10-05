using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Models.LMSModels;

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : CommonController
    {
        private static Dictionary<string, float> GradeWeights = new Dictionary<string, float>()
        {
            { "A",  4.0f },
            { "A-", 3.7f },
            { "B+", 3.3f },
            { "B",  3.0f },
            { "B-", 2.7f },
            { "C+", 2.3f },
            { "C",  2.0f },
            { "C-", 1.7f },
            { "D+", 1.3f },
            { "D",  1.0f },
            { "D-", 0.7f },
            { "E",  0.0f }
        };

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var getClasses = from c in db.Classes
                             join e in db.Enrolled
                             on c.ClassId equals e.Class
                             join cor in db.Courses
                             on c.Listing equals cor.CatalogId
                             where e.Student == uid
                             select new
                             {
                                 subject = cor.Department,
                                 number = cor.Number,
                                 name = cor.Name,
                                 season = c.Season,
                                 year = c.Year,
                                 grade = e.Grade == "" ? "--" : e.Grade
                             };
            return Json(getClasses.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var getAssignments = from c in db.Classes
                                 join cor in db.Courses
                                 on c.ClassId equals cor.CatalogId
                                 join ac in db.AssignmentCategories
                                 on c.ClassId equals ac.InClass
                                 join a in db.Assignments
                                 on ac.CategoryId equals a.Category
                                 where cor.Department == subject
                                 && cor.Number == num
                                 && c.Season == season
                                 && c.Year == year
                                 select new
                                 {
                                     aname = a.Name,
                                     cname = ac.Name,
                                     due = a.Due,
                                     score = (from sub in db.Submissions
                                              where sub.Student == uid
                                              && sub.Assignment == a.AssignmentId
                                              select (uint?)sub.Score).FirstOrDefault<uint?>()
                                 };

            return Json(getAssignments.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// Does *not* automatically reject late submissions.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}.</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            //temp assignment id
            uint aID;

            //Get assignment student is trying to submit to
            var assID = (from c in db.Classes
                         join cor in db.Courses
                         on c.ClassId equals cor.CatalogId
                         join ac in db.AssignmentCategories
                         on c.ClassId equals ac.InClass
                         join a in db.Assignments
                         on ac.CategoryId equals a.Category
                         where cor.Department == subject
                         && cor.Number == num
                         && c.Season == season
                         && c.Year == year
                         && ac.Name == category
                         && a.Name == asgname
                         select a.AssignmentId).DefaultIfEmpty();

            if (assID is null)
                return Json(new { success = false });

            Submissions submission = new Submissions
            {
                Assignment = assID.First(),
                Student = uid,
                Score = 0,
                SubmissionContents = contents,
                Time = DateTime.Now
            };

            try
            {
                db.Submissions.Add(submission);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false},
        /// false if the student is already enrolled in the Class.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var enroll = (from crs in db.Courses
                          join cl in db.Classes
                          on crs.CatalogId equals cl.Listing
                          where crs.Department == subject
                          && crs.Number == num
                          && cl.Season == season
                          && cl.Year == year
                          select cl.ClassId).DefaultIfEmpty();

            if (enroll is null)
                return Json(new { success = false });

            //Enroll user in class
            Enrolled enrolled = new Enrolled
            {
                Student = uid,
                Class = enroll.First(),
                Grade = ""
            };

            db.Enrolled.Add(enrolled);

            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }


        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student does not have any grades, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var gradeList = (from e in db.Enrolled
                             where e.Student == uid
                             select e.Grade).DefaultIfEmpty();

            if (gradeList is null)
                return Json(new { gpa = 0.0f });

            float sum = 0.0f;
            float numClasses = 0;

            foreach (string c in gradeList)
            {
                if (!(c is null) && c != "")
                {
                    sum += GradeWeights[c];
                    numClasses++;
                }
            }

            if (numClasses == 0)
                return Json(new { gpa = 0.0f });
            else
                return Json(new { gpa = sum / numClasses });
        }

        /*******End code to modify********/

    }
}