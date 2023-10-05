using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Models.LMSModels;

namespace LMS.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : CommonController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subject">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var q = from a in db.Courses
                    where a.Department == subject
                    select new { number = a.Number, name = a.Name };

            return Json(q.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var q = from a in db.Professors
                    where a.WorksIn == subject
                    select new { lname = a.LName, fname = a.FName, uid = a.UId };

            return Json(q.ToArray());
        }

        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false},
        /// false if the Course already exists.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            Courses c = new Courses
            {
                Department = subject,
                Number = (uint)number,
                Name = name
            };

            db.Courses.Add(c);

            bool result = false;
            try
            {
                db.SaveChanges();
                result = true;
            }
            catch
            {
            }

            return Json(new { success = result });
        }

        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            var catalogID = (from c in db.Courses where c.Department == subject && c.Number == number select c.CatalogId).DefaultIfEmpty();

            if (catalogID is null)
                return Json(new { success = false });

            var courseOfferings = (from c in db.Classes
                                   where c.Season == season
                                      && c.Year == year
                                      && c.Location == location
                                   select new { start = c.StartTime, end = c.EndTime }).DefaultIfEmpty();

            foreach (var offering in courseOfferings)
            {
                if (offering is null)
                    continue;

                TimeSpan s = offering.start;
                TimeSpan e = offering.end;

                if ((start.TimeOfDay.CompareTo(s) >= 0 && start.TimeOfDay.CompareTo(e) <= 0) || (end.TimeOfDay.CompareTo(s) >= 0 && end.TimeOfDay.CompareTo(e) <= 0))
                    return Json(new { success = false });
            }

            Classes cl = new Classes
            {
                Season = season,
                Year = (uint)year,
                Location = location,
                StartTime = start.TimeOfDay,
                EndTime = end.TimeOfDay,
                Listing = (uint)catalogID.First(),
                TaughtBy = instructor
            };

            db.Classes.Add(cl);

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


        /*******End code to modify********/

    }
}