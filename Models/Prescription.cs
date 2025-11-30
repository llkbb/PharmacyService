using System.ComponentModel.DataAnnotations;

namespace PharmacyChain.Models
{
    public class Prescription
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int DrugId { get; set; }
        public Drug Drug { get; set; }

        public int PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; }

        public string DoctorName { get; set; }
        public DateTime DateIssued { get; set; }
        public int ValidDays { get; set; }
    }


}
