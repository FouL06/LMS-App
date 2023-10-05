using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : CommonController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var studentList = (from crs in db.Courses
                               join cl in db.Classes
                               on crs.CatalogId equals cl.Listing
                               join e in db.Enrolled
                               on cl.ClassId equals e.Class
                               join s in db.Students
                               on e.Student equals s.UId
                               where crs.Department == subject
                               && crs.Number == num
                               && cl.Season == season
                               && cl.Year == year
                               select new
                               {
                                   fname = s.FName,
                                   lname = s.LName,
                                   uid = s.UId,
                                   dob = s.Dob,
                                   grade = e.Grade
                               }).DefaultIfEmpty();

            return Json(studentList.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category is null)
            {


                var assList = (from crs in db.Courses
                               join cl in db.Classes
                               on crs.CatalogId equals cl.Listing
                               join cat in db.AssignmentCategories
                               on cl.ClassId equals cat.InClass
                               join ass in db.Assignments
                               on cat.CategoryId equals ass.Category
                               where crs.Department == subject
                               && crs.Number == num
                               && cl.Season == season
                               && cl.Year == year
                               select new
                               {
                                   aname = ass.Name,
                                   cname = cat.Name,
                                   due = ass.Due,
                                   submissions = ass.Submissions.Count()
                               });

                var t = assList.ToArray();

                return Json(assList.ToArray());
            }
            else
            {
                var assList = (from crs in db.Courses
                               join cl in db.Classes
                               on crs.CatalogId equals cl.Listing
                               join cat in db.AssignmentCategories
                               on cl.ClassId equals cat.InClass
                               join ass in db.Assignments
                               on cat.CategoryId equals ass.Category
                               where crs.Department == subject
                               && crs.Number == num
                               && cl.Season == season
                               && cl.Year == year
                               && cat.Name == category
                               select new
                               {
                                   aname = ass.Name,
                                   cname = cat.Name,
                                   due = ass.Due,
                                   submissions = ass.Submissions.Count()
                               });
                return Json(assList.ToArray());
            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var catList = (from crs in db.Courses
                           join cl in db.Classes
                           on crs.CatalogId equals cl.Listing
                           join cat in db.AssignmentCategories
                           on cl.ClassId equals cat.InClass
                           where crs.Department == subject
                           && crs.Number == num
                           && cl.Season == season
                           && cl.Year == year
                           select new
                           {
                               name = cat.Name,
                               weight = cat.Weight
                           }).DefaultIfEmpty();

            return Json(catList.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false},
        ///	false if an assignment category with the same name already exists in the same class.</returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var classID = (from crs in db.Courses
                           join cl in db.Classes
                           on crs.CatalogId equals cl.Listing
                           where crs.Department == subject
                           && crs.Number == num
                           && cl.Season == season
                           && cl.Year == year
                           select cl.ClassId).DefaultIfEmpty();

            if (classID is null)
                return Json(new { success = false });

            AssignmentCategories assCat = new AssignmentCategories
            {
                Name = category,
                Weight = (uint)catweight,
                InClass = classID.First()
            };

            db.AssignmentCategories.Add(assCat);

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
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false,
        /// false if an assignment with the same name already exists in the same assignment category.</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var catID = (from crs in db.Courses
                         join cl in db.Classes
                         on crs.CatalogId equals cl.Listing
                         join cat in db.AssignmentCategories
                         on cl.ClassId equals cat.InClass
                         where crs.Department == subject
                         && crs.Number == num
                         && cl.Season == season
                         && cl.Year == year
                         && cat.Name == category
                         select cat.CategoryId).DefaultIfEmpty();

            if (catID is null)
                return Json(new { success = false });

            Assignments ass = new Assignments
            {
                Name = asgname,
                Category = catID.First(),
                MaxPoints = (uint)asgpoints,
                Due = asgdue,
                Contents = asgcontents
            };

            db.Assignments.Add(ass);

            try
            {
                int gradesToChange = db.SaveChanges();
                if (gradesToChange > 0)
                {
                    UpdateGrades(subject, num, season, year);
                }
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Helper Method to Update Grades for Students from CreateAssignment
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="num"></param>
        /// <param name="season"></param>
        /// <param name="year"></param>
        private void UpdateGrades(string subject, int num, string season, int year)
        {
            var getGrades = from c in db.Courses
                            join cl in db.Classes
                            on c.CatalogId equals cl.ClassId
                            join e in db.Enrolled
                            on cl.ClassId equals e.Class
                            where c.Department == subject
                            && c.Number == num
                            && cl.Season == season
                            && cl.Year == year
                            select new
                            {
                                uid = e.Student
                            };
            foreach (var uid in getGrades)
            {
                UpdateGrades(subject, num, season, year, uid.uid);
            }
        }

        /// <summary>
        /// Updates grades for each individual student
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="num"></param>
        /// <param name="season"></param>
        /// <param name="year"></param>
        /// <param name="uid"></param>
        private void UpdateGrades(string subject, int num, string season, int year, string uid)
        {
            var updateGrade = (from crs in db.Courses
                               join cl in db.Classes
                               on crs.CatalogId equals cl.Listing
                               join cat in db.AssignmentCategories
                               on cl.ClassId equals cat.InClass
                               join ass in db.Assignments
                               on cat.CategoryId equals ass.Category
                               join sub in db.Submissions
                               on ass.AssignmentId equals sub.Assignment
                               join std in db.Students
                               on sub.Student equals uid
                               where crs.Department == subject
                               && crs.Number == num
                               && cl.Season == season
                               && cl.Year == year
                               && sub.Student == uid
                               select new
                               {
                                   assID = ass.AssignmentId,
                                   cID = cl.ClassId,
                                   catID = cat.CategoryId,
                                   score = sub.Score
                               }).GroupBy(x => x.assID).DefaultIfEmpty();

            var maxes = (from crs in db.Courses
                         join cl in db.Classes
                         on crs.CatalogId equals cl.Listing
                         join cat in db.AssignmentCategories
                         on cl.ClassId equals cat.InClass
                         join ass in db.Assignments
                         on cat.CategoryId equals ass.Category
                         where crs.Department == subject
                         && crs.Number == num
                         && cl.Season == season
                         && cl.Year == year
                         select new
                         {
                             assID = ass.AssignmentId,
                             catID = cat.CategoryId,
                             weight = cat.Weight,
                             maxPoint = ass.MaxPoints
                         }).DefaultIfEmpty();

            Dictionary<uint, uint> weights = new Dictionary<uint, uint>();
            Dictionary<uint, uint> sumScores = new Dictionary<uint, uint>();
            Dictionary<uint, uint> sumMax = new Dictionary<uint, uint>();
            uint classID = 0;

            foreach(var max in maxes)
            {
                if (max is null)
                    continue;

                //Get all weights
                if (!weights.ContainsKey(max.catID))
                {
                    weights.Add(max.catID, max.weight);
                }

                //Add all scores to max
                if (sumMax.ContainsKey(max.catID))
                {
                    sumMax[max.catID] += max.maxPoint;
                }
                else
                {
                    sumMax.Add(max.catID, max.maxPoint);
                }
            }

            foreach(var grade in updateGrade)
            {
                if (grade is null)
                    continue;

                var g = grade.First();

                classID = g.cID;
                //Add all scores
                if (sumScores.ContainsKey(g.catID))
                {
                    sumScores[g.catID] += g.score;
                }
                else
                {
                    sumScores.Add(g.catID, g.score);
                }
            }


            float scaledTotals = 0;
            uint weightSum = 0;
            foreach(var key in weights.Keys)
            {
                weightSum += weights[key];
                scaledTotals += weights[key] * ((float)sumScores[key] / (float)sumMax[key]);
            }

            float classPercentage = 100 * (scaledTotals / weightSum);

            string letterGrade;
            if (classPercentage >= 93)
                letterGrade = "A";
            else if (classPercentage >= 90)
                letterGrade = "A-";
            else if (classPercentage >= 87)
                letterGrade = "B+";
            else if (classPercentage >= 83)
                letterGrade = "B";
            else if (classPercentage >= 80)
                letterGrade = "B-";
            else if (classPercentage >= 77)
                letterGrade = "C+";
            else if (classPercentage >= 73)
                letterGrade = "C";
            else if (classPercentage >= 70)
                letterGrade = "C-";
            else if (classPercentage >= 67)
                letterGrade = "D+";
            else if (classPercentage >= 63)
                letterGrade = "D";
            else if (classPercentage >= 60)
                letterGrade = "D-";
            else
                letterGrade = "E";

            var updateLetterGrade = (from e in db.Enrolled
                                    where e.Class == classID
                                    && e.Student == uid
                                    select e).ToList()[0];

            updateLetterGrade.Grade = letterGrade;
            db.SaveChanges();
        }

        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var subList = (from crs in db.Courses
                           join cl in db.Classes
                           on crs.CatalogId equals cl.Listing
                           join cat in db.AssignmentCategories
                           on cl.ClassId equals cat.InClass
                           join ass in db.Assignments
                           on cat.CategoryId equals ass.Category
                           join sub in db.Submissions
                           on ass.AssignmentId equals sub.Assignment
                           join std in db.Students
                           on sub.Student equals std.UId
                           where crs.Department == subject
                           && crs.Number == num
                           && cl.Season == season
                           && cl.Year == year
                           && cat.Name == category
                           && ass.Name == asgname
                           select new
                           {
                               fname = std.FName,
                               lname = std.LName,
                               uid = std.UId,
                               time = sub.Time,
                               score = sub.Score
                           }).DefaultIfEmpty();

            return Json(subList.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var updateSub = (from crs in db.Courses
                             join cl in db.Classes
                             on crs.CatalogId equals cl.Listing
                             join cat in db.AssignmentCategories
                             on cl.ClassId equals cat.InClass
                             join ass in db.Assignments
                             on cat.CategoryId equals ass.Category
                             join sub in db.Submissions
                             on ass.AssignmentId equals sub.Assignment
                             join std in db.Students
                             on sub.Student equals std.UId
                             where crs.Department == subject
                             && crs.Number == num
                             && cl.Season == season
                             && cl.Year == year
                             && cat.Name == category
                             && ass.Name == asgname
                             && sub.Student == uid
                             select sub).ToList()[0];

            updateSub.Score = (uint)score;

            try
            {
                int gradeChanged = db.SaveChanges();
                if (gradeChanged > 0)
                {
                    UpdateGrades(subject, num, season, year, uid);
                }
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {

            var classList = (from crs in db.Courses
                             join cl in db.Classes
                             on crs.CatalogId equals cl.Listing
                             where cl.TaughtBy == uid
                             select new
                             {
                                 subject = crs.Department,
                                 number = crs.Number,
                                 name = crs.Name,
                                 season = cl.Season,
                                 year = cl.Year
                             }).DefaultIfEmpty();

            return Json(classList.ToArray());
        }


        /*******End code to modify********/

    }
}