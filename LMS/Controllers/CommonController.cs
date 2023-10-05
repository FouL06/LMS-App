using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CommonController : Controller
    {

        /*******Begin code to modify********/

        protected Team122LMSContext db;

        public CommonController()
        {
            db = new Team122LMSContext();
        }

        public void UseLMSContext(Team122LMSContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var getDepartments = from dep in db.Departments
                                 select new
                                 {
                                     name = dep.Name,
                                     subject = dep.Subject
                                 };

            return Json(getDepartments.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var getCatalog = from dep in db.Departments
                             select new
                             {
                                 subject = dep.Subject,
                                 dname = dep.Name,
                                 courses = dep.Courses.Select((key, index) => new { number = key.Number, cname = key.Name })
                             };

            return Json(getCatalog.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var getClassOfferings = from c in db.Classes
                                    join cor in db.Courses
                                    on c.Listing equals cor.CatalogId
                                    join p in db.Professors
                                    on c.TaughtBy equals p.UId
                                    where cor.Department == subject
                                    && cor.Number == number
                                    select new
                                    {
                                        season = c.Season,
                                        year = c.Year,
                                        location = c.Location,
                                        start = c.StartTime,
                                        end = c.EndTime,
                                        fname = p.FName,
                                        lname = p.LName
                                    };
            return Json(getClassOfferings.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var getAssignmentContents = from cor in db.Courses
                                        join c in db.Classes
                                        on cor.CatalogId equals c.Listing
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
                                        select new
                                        {
                                            a.Contents
                                        };

            return Content("" + getAssignmentContents.FirstOrDefault().Contents);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var getSubText = from cor in db.Courses
                             join c in db.Classes
                             on cor.CatalogId equals c.Listing
                             join ac in db.AssignmentCategories
                             on c.ClassId equals ac.InClass
                             join a in db.Assignments
                             on ac.CategoryId equals a.Category
                             join sub in db.Submissions
                             on a.AssignmentId equals sub.Assignment
                             where cor.Department == subject
                             && cor.Number == num
                             && c.Season == season
                             && c.Year == year
                             && ac.Name == category
                             && a.Name == asgname
                             && sub.Student == uid
                             select new
                             {
                                 sub.SubmissionContents
                             };

            //If an assignment has contents and is not null return the contents
            if (getSubText.Count() >= 1 && getSubText.FirstOrDefault().SubmissionContents != null)
            {
                return Content("" + getSubText.FirstOrDefault().SubmissionContents);
            }

            return Content("");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            //Admin
            var findAdmin = from a in db.Administrators
                            where a.UId == uid
                            select new
                            {
                                fname = a.FName,
                                lname = a.LName,
                                uid = a.UId
                            };
            foreach (var admin in findAdmin)
            {
                return Json(admin);
            }

            //Professor
            var findProf = from p in db.Professors
                           where p.UId == uid
                           select new
                           {
                               fname = p.FName,
                               lname = p.LName,
                               uid = p.UId,
                               department = p.WorksIn
                           };
            foreach (var prof in findProf)
            {
                return Json(prof);
            }

            //Student
            var findStud = from s in db.Students
                           where s.UId == uid
                           select new
                           {
                               fname = s.FName,
                               lname = s.LName,
                               uid = s.UId,
                               department = s.Major
                           };
            foreach (var stud in findStud)
            {
                return Json(stud);
            }


            return Json(new { success = false });
        }


        /*******End code to modify********/

    }
}