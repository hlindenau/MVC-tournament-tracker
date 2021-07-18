using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TournamentsProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity.Migrations;

namespace TournamentsProject.Controllers
{
    public class TournmentController : Controller
    {
        private TournmentsDbContext db = new TournmentsDbContext();
        private ApplicationDbContext adb = new ApplicationDbContext();

        // GET: Tournment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TournmentModel tournmentModel = db.Tournments.Include("ParticipantIds.BracketPosition").Where(t => t.Id == id).FirstOrDefault <TournmentModel>();
            if (tournmentModel == null)
            {
                return HttpNotFound();
            }


            
            if (!tournmentModel.BracketCreated && DateTime.Compare(tournmentModel.TournmentDate,DateTime.Now) > 0)
            {
                PrepareBracket(tournmentModel);
                UpdateBracket(tournmentModel);
            }
            //PrepareBracket(tournmentModel);
            tournmentModel.ParticipantIds = tournmentModel.ParticipantIds.OrderBy(x => x.Ranking).ToList();
            //UpdateBracket(tournmentModel);
            return View(tournmentModel);
        }

        private void PrepareBracket(TournmentModel tournmentModel)
        {
            tournmentModel.ParticipantIds = tournmentModel.ParticipantIds.OrderBy(x => x.Ranking).ToList();
            List<Participant> ordered = tournmentModel.ParticipantIds;
            int offset = 0;
            if (tournmentModel.ParticipantIds.Count % 2 == 1)
            {
                offset = 1;
                tournmentModel.ParticipantIds[0].BracketPosition.Add(new Position(1));
                ordered[0] = tournmentModel.ParticipantIds[0];
                tournmentModel.ParticipantIds[0].InitialPos = 0;
            }
            for (int i = 0 + offset; i < (tournmentModel.ParticipantLimit + offset) / 2; i++)
            {
                if (i < (tournmentModel.ParticipantIds.Count + offset) / 2)
                {
                    tournmentModel.ParticipantIds[i].BracketPosition.Clear();
                    tournmentModel.ParticipantIds[i].CurrentRound = 0;
                    tournmentModel.ParticipantIds[i].RoundWinner = 0;
                    tournmentModel.ParticipantIds[i].CurrentOpponent = tournmentModel.ParticipantIds[tournmentModel.ParticipantIds.Count() - i - 1 + offset];
                    tournmentModel.ParticipantIds[i].InitialPos = i * 2;
                    tournmentModel.ParticipantIds[tournmentModel.ParticipantIds.Count() - i - 1 + offset].CurrentOpponent = tournmentModel.ParticipantIds[i];
                    tournmentModel.ParticipantIds[tournmentModel.ParticipantIds.Count() - i - 1 + offset].InitialPos = i * 2 + 1;
                    tournmentModel.ParticipantIds[tournmentModel.ParticipantIds.Count() - i - 1 + offset].CurrentRound = 0;
                    tournmentModel.ParticipantIds[tournmentModel.ParticipantIds.Count() - i - 1 + offset].RoundWinner = 0;
                    tournmentModel.ParticipantIds[tournmentModel.ParticipantIds.Count() - i - 1 + offset].BracketPosition.Clear();
                }
            }
            tournmentModel.BracketCreated = true;
            db.Entry(tournmentModel).State = EntityState.Modified;
            db.SaveChanges();
        }
        private void UpdateBracket(TournmentModel tournmentModel)
        {
            CheckForWinners(tournmentModel);
            PickNewOpponents(tournmentModel);
            ResetVotes(tournmentModel);
        }

        private void PickNewOpponents(TournmentModel tournmentModel)
        {
            foreach(var participant in tournmentModel.ParticipantIds)
            {
                if(participant.CurrentRound > 0 && participant.BracketPosition.Count > 0)
                {
                    if (participant.BracketPosition[participant.CurrentRound - 1].Pos % 2 == 0)
                    {
                        foreach (var participant2 in tournmentModel.ParticipantIds)
                        {
                            if (participant2.CurrentRound > 0 && participant2.BracketPosition.Count > 0)
                            {
                                if (participant.BracketPosition[participant.CurrentRound - 1].Pos == participant2.BracketPosition[participant2.CurrentRound - 1].Pos + 1)
                                    participant.CurrentOpponent = participant2;
                            }

                        }
                    }
                    else
                    {
                        foreach (var participant2 in tournmentModel.ParticipantIds)
                        {
                            if (participant2.CurrentRound > 0 && participant2.BracketPosition.Count > 0)
                            {
                                if (participant.BracketPosition[participant.CurrentRound - 1].Pos == participant2.BracketPosition[participant2.CurrentRound - 1].Pos - 1)
                                    participant.CurrentOpponent = participant2;
                            }

                        }
                    }
                }
            }
            db.Entry(tournmentModel).State = EntityState.Modified;
            db.SaveChanges();
        }
        private void CheckForWinners(TournmentModel tournmentModel)
        {
            int offset = 0;
            if (tournmentModel.ParticipantIds.Count % 2 == 1)
            {
                offset = 1;
                tournmentModel.ParticipantIds[0].BracketPosition.Add(new Position(1));
            }

            for (int i = offset; i < tournmentModel.ParticipantIds.Count; i++)
            {
                int round = tournmentModel.ParticipantIds[i].CurrentRound;
                if (tournmentModel.ParticipantIds[i].Id == tournmentModel.ParticipantIds[i].RoundWinner && tournmentModel.ParticipantIds[i].Id == tournmentModel.ParticipantIds[i].CurrentOpponent.RoundWinner)
                {
                    if (tournmentModel.ParticipantIds[i].CurrentRound == tournmentModel.ParticipantIds[i].CurrentOpponent.CurrentRound)
                    {

                        if(tournmentModel.ParticipantIds[i].CurrentRound > 0)
                        {
                            tournmentModel.ParticipantIds[i].BracketPosition.Add(new Position(((tournmentModel.ParticipantIds[i].BracketPosition[tournmentModel.ParticipantIds[i].CurrentRound-1].Pos + 1)/ 2)));
                        }
                        else
                        {
                            tournmentModel.ParticipantIds[i].BracketPosition.Add(new Position((tournmentModel.ParticipantIds[i].InitialPos / 2) + 1));
                        }

                        tournmentModel.ParticipantIds[i].CurrentRound ++;
                        tournmentModel.ParticipantIds[i].RoundWinner = -1;
                        tournmentModel.ParticipantIds[i].CurrentOpponent.CurrentOpponent = null;
                        tournmentModel.ParticipantIds[i].CurrentOpponent = null;
                    }
                }
            }
            db.Entry(tournmentModel).State = EntityState.Modified;
            db.SaveChanges();
        }
        private void ResetVotes(TournmentModel tournmentModel)
        {
            for (int i = 0; i < tournmentModel.ParticipantIds.Count; i++)
            {
                if(tournmentModel.ParticipantIds[i].CurrentOpponent != null && tournmentModel.ParticipantIds[i].RoundWinner > 0 && tournmentModel.ParticipantIds[i].CurrentOpponent.RoundWinner > 0)
                {
                    tournmentModel.ParticipantIds[i].RoundWinner = -1;
                    tournmentModel.ParticipantIds[i].CurrentOpponent.RoundWinner = -1;
                }
            }
            db.Entry(tournmentModel).State = EntityState.Modified;
            db.SaveChanges();
        }


        //GET: Tournment/ResolveMatch/5
        public ActionResult ResolveMatch(int? id)
        {
            if (id == null || String.IsNullOrEmpty(User.Identity.Name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Check if user has confirmed their email address
            if (String.IsNullOrEmpty(User.Identity.GetUserId()) == false)
            {
                if (HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(User.Identity.GetUserId()).EmailConfirmed == false)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TournmentModel tournmentModel = db.Tournments.Include("ParticipantIds.BracketPosition").Where(t => t.Id == id).FirstOrDefault<TournmentModel>();
            Participant participant = tournmentModel.ParticipantIds.FirstOrDefault(p => p.Name == User.Identity.GetUserId());
            if (tournmentModel == null)
            {
                return HttpNotFound();
            }
            return View(participant);
        }

        // POST: Tournment/ResolveMatch/5
        [HttpPost, ActionName("ResolveMatch")]
        [ValidateAntiForgeryToken]
        public ActionResult ResolveConfirmed(int id, [Bind(Include = "Id,Name,LicenceNumber,Ranking,CurrentOpponent,BracketPosition,RoundWinner,CurrentRound,InitialPos")] Participant participant)
        {
            TournmentModel tournmentModel = db.Tournments.Include("ParticipantIds.BracketPosition").Where(t => t.Id == id).FirstOrDefault<TournmentModel>();
            if (tournmentModel.ParticipantIds.Count() <= tournmentModel.ParticipantLimit)
            {
                tournmentModel.ParticipantIds.FirstOrDefault(p => p.Name == User.Identity.GetUserId()).RoundWinner = participant.RoundWinner;
                    UpdateBracket(tournmentModel);
                    db.Entry(tournmentModel).State = EntityState.Modified;
                    db.SaveChanges();

                return RedirectToAction("Details", new { id = tournmentModel.Id });
            }
            return View("Error");
        }

        // GET: Tournment/JoinTournment/5
        public ActionResult JoinTournment(int? id)
        {
            if (id == null || String.IsNullOrEmpty(User.Identity.Name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Check if user has confirmed their email address
            if (String.IsNullOrEmpty(User.Identity.GetUserId()) == false){
                if(HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(User.Identity.GetUserId()).EmailConfirmed == false)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TournmentModel tournmentModel = db.Tournments.Include("ParticipantIds.BracketPosition").Where(t => t.Id == id).FirstOrDefault<TournmentModel>();
            Participant newParticipant = new Participant(User.Identity.GetUserId());
            if (tournmentModel == null )
            {
                return HttpNotFound();
            }
            return View(newParticipant);
        }

        // POST: Tournment/JoinTournment/5
        [HttpPost, ActionName("JoinTournment")]
        [ValidateAntiForgeryToken]
        public ActionResult JoinConfirmed(int id, [Bind(Include = "Id,Name,LicenceNumber,Ranking,CurrentOpponent,BracketPosition")] Participant participant)
        {
            TournmentModel tournmentModel = db.Tournments.Find(id);
            if(tournmentModel.ParticipantIds.Count() <= tournmentModel.ParticipantLimit) {
                bool c = tournmentModel.ParticipantIds.Any(p => p.Name == participant.Name);
                if (c == false)
                {
                    tournmentModel.ParticipantIds.Add(participant);
                    db.Entry(tournmentModel).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = tournmentModel.Id });
                }
            }
            return View("Error");
        }

        // GET: Tournment/Create
        public ActionResult Create()
        {
            ViewBag.Sponsors = db.Sponsors.ToList();
            TournmentModel tournmentModel = new TournmentModel();
            tournmentModel.TournmentDate = DateTime.Now;
            tournmentModel.AssignmentDeadline = DateTime.Now;
            tournmentModel.ParticipantIds = new List<Participant>();
            return View(tournmentModel);
        }

        // POST: Tournment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TournmentName,Discipline,ParticipantLimit,ParticipantIds,LocationAddress,Longitude,Latitude,SponsorLogoFile,AssignmentDeadline,TournmentDate,BracketCreated")] TournmentModel tournmentModel)
        {

            ViewBag.Sponsors = db.Sponsors.ToList();
            if (ModelState.IsValid)
            {
                var UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                tournmentModel.OrganizerId = User.Identity.GetUserId();
                tournmentModel.OrganizerName = User.FirstName();
                tournmentModel.BracketCreated = false;
                db.Tournments.Add(tournmentModel);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = tournmentModel.Id });
            }

            return View(tournmentModel);
        }


        // GET: Tournment/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.Sponsors = db.Sponsors.ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TournmentModel tournmentModel = db.Tournments.Find(id);
            if (tournmentModel == null)
            {
                return HttpNotFound();
            }
            if(String.Equals(tournmentModel.OrganizerId,User.Identity.GetUserId()))
            {
                return View(tournmentModel);
            }
            else
            {
                return View("Error");
            }

        }

        // POST: Tournment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TournmentName,Discipline,ParticipantLimit,ParticipantIds,LocationAddress,Longitude,Latitude,SponsorLogoFile,AssignmentDeadline,TournmentDate")] TournmentModel tournmentModel)
        {
            ViewBag.Sponsors = db.Sponsors.ToList();
            if (ModelState.IsValid)
            {
                var UserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                tournmentModel.OrganizerId = User.Identity.GetUserId();
                tournmentModel.OrganizerName = User.FirstName();

                db.Entry(tournmentModel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = tournmentModel.Id });
            }
            else
            {
                return View("Error");
            }

        }

        // GET: Tournment/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TournmentModel tournmentModel = db.Tournments.Find(id);
            if (tournmentModel == null)
            {
                return HttpNotFound();
            }
            if (String.Equals(tournmentModel.OrganizerId, User.Identity.GetUserId()))
            {
                return View(tournmentModel);
            }
            else
            {
                // Only organizers can delete tournment details (do something about it)
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Tournment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
                        
            TournmentModel tournmentModel = db.Tournments.Include(t => t.ParticipantIds).FirstOrDefault(t => t.Id == id);
            if (String.Equals(tournmentModel.OrganizerId, User.Identity.GetUserId())) { 
                db.Tournments.Remove(tournmentModel);  
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
