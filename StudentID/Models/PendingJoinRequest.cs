using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
    public class PendingJoinRequest
    {
        public string StudentNo { get; set; }

        [Key]
        public string RequestHash { get; set; }

        public bool IsApproved { get; set; } = false;

    }
}
