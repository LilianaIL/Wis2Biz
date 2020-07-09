using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Wis2Biz.Models
{
    public class ClientDetails
    {
        [Key]
        public long ClientID { get; set; }
        public long UserID { get; set; }
        public DateTime MemberShipStartDate { get; set; }
        public int Status { get; set; }
        public int GroupID { get; set; }
    }
}
