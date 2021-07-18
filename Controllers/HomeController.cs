using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TournamentsProject.Models;
using PagedList;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TournamentsProject.Controllers
{
    public class HomeController : Controller
    {


        private TournmentsDbContext db = new TournmentsDbContext();
        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = sortOrder == "name_desc" ? "name_asc": "name_desc" ;
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            var tournments = from t in db.Tournments
                             select t;
            if (!String.IsNullOrEmpty(searchString))
            {
                tournments = tournments.Where(s => s.TournmentName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    tournments = tournments.OrderByDescending(s => s.TournmentName);
                    break;
                case "name_asc":
                    tournments = tournments.OrderBy(s => s.TournmentName);
                    break;
                case "Date":
                    tournments = tournments.OrderBy(s => s.TournmentDate);
                    break;
                case "date_desc":
                    tournments = tournments.OrderByDescending(s => s.TournmentDate);
                    break;
                default:  // Name ascending 
                    tournments = tournments.OrderByDescending(s => s.Id);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(tournments.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult MyTournments(string UserId)
        {
            if (String.Equals(User.Identity.GetUserId(), UserId)) {
                DateTime yesterday = DateTime.Now.AddDays(-1);
                var tournments = from t in db.Tournments where t.ParticipantIds.Any(p => p.Name == UserId) && (DateTime.Compare(t.TournmentDate,yesterday) > 0)
                                 select t;

                return View(tournments.ToList());
            }
            return View("Error");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}