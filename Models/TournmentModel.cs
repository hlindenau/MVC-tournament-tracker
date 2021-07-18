using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TournamentsProject.Models
{
    public class TournmentModel
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name ="Tournment Name")]
        public string TournmentName { get; set; }
        public string OrganizerId { get; set; }
        public string OrganizerName { get; set; }
        
        [Required]
        public string Discipline { get; set; }
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Required]
        [Display(Name = "Participant Limit")]
        public int ParticipantLimit { get; set; }

        [Required]
        [ValidateDateRange]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddThh:mm:ss}")]
        [Display(Name = "Assignment Deadline")]
        public DateTime AssignmentDeadline { get; set; }

        [Required]
        [ValidateDateRange]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddThh:mm:ss}")]
        [Display(Name = "Tournment Date")]
        public DateTime TournmentDate { get; set; }

        internal object OrderBy(Func<object, object> p)
        {
            throw new NotImplementedException();
        }

        [Display(Name = "Participants")]
        public virtual List<Participant> ParticipantIds { get; set; }
        public string SponsorLogoFile { get; set; }

        public string LocationAddress { get; set; }

        public bool BracketCreated { get; set; }
    }

    public class ValidateDateRange : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value == null)
                return new ValidationResult("Field required.");
            if ((DateTime)value >= DateTime.Today && (DateTime)value <= DateTime.Today.AddMonths(12))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Tournments can only be planned up to one year in the future.");
            }
        }
    }
}