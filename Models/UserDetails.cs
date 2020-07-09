using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Wis2Biz.Models
{
    public class UserDetails
    {
      [Key]
      public long UserID { get; set; }
      public string IdentityString { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string Email { get; set; }
      public string PhoneNumber { get; set; }
      public string Password { get; set; }
      public DateTime Birthdate { get; set; }
      public int GenderID { get; set; }
      public int UserRoleID { get; set; }
    }
}
