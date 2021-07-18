using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TournamentsProject.Models
{
    public class Sponsor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Display(Name="Logo image")]
        public string LogoFilename { get; set; }
        public string Index { get; set; }
    }
}