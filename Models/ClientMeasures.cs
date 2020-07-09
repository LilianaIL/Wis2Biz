using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Wis2Biz.Models
{
    public class ClientMeasures
    {
      [Key]
      public long MeasureID { get; set; }
      public long ClientID { get; set; }
      public int ParameterID { get; set; }
      public DateTime ChangeDateTime { get; set; }
      public double ParameterValue { get; set; }

      [NotMapped]
      public double ChangeFromLastWeight { get; set; }
      [NotMapped]
      public double LastWeight { get; set; }

        
    }
}
