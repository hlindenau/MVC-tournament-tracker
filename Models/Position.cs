using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TournamentsProject.Models
{
    public class Position
    {
        public int Id { get; set; }
        public int Pos { get; set; }
        public Position(int pos)
        {
            Pos = pos;
        }
        public Position()
        {

        }
    }
}