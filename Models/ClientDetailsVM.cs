using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wis2Biz.Models
{
    public class ClientDetailsVM
    {
        public long ClientID { get; set; }
        public string IdentityString { get; set; }
        public string ClientFullName { get; set; }
        public DateTime MemberShipStartDate { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public double? BMI { get; set; }
        public double Height { get; set; }
        public double CurrentWeight { get; set; }
        public List<ClientMeasures> ClientMeasures { get; set; }
    }
}
