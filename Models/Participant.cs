using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TournamentsProject.Models
{
    public class Participant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Participant CurrentOpponent { get; set; }
        [Required(ErrorMessage = "Winner required.")]
        public int RoundWinner { get; set; }

        public int CurrentRound { get; set; }
        public int InitialPos { get; set; }
        [Required]
        public virtual List<Position> BracketPosition { get; set; }
        public Participant(string name)
        {
            Name = name;
            Position p = new Position(0);
            BracketPosition = new List<Position>();
            CurrentOpponent = null;
            RoundWinner = 0;
            CurrentRound = 0;
        }
        public int Ranking { get; set; }
        public int LicenceNumber { get; set; }
        public Participant()
        {
            Position p = new Position(0);
            BracketPosition = new List<Position>();
            CurrentOpponent = null;
            RoundWinner = 0;
            CurrentRound = 0;
        }
    }
}