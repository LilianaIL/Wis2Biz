using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Wis2Biz.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Wis2Biz.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ClientsController : ControllerBase
    {
        ClientsContext db;
        public ClientsController(ClientsContext context)
        {
            db = context;            
        }
        

        // GET api/clients/5
        [HttpGet("{id}")]
        public ActionResult<ClientDetailsVM> GetClientDetails(string id)
        {
            try
            {
                var maxDateChangesQuery = db.ClientMeasures
                .Where(x => (new List<int> { 1, 2, 9 }).Contains(x.ParameterID))
                .GroupBy(c => new { c.ParameterID, c.ClientID })
                .Select(g => new
                {
                    g.Key.ClientID,
                    g.Key.ParameterID,
                    Date = g.Max(t => t.ChangeDateTime)
                });


                var clientParametersQuery = db.ClientMeasures
                    .Join(
                        maxDateChangesQuery,
                        cm => new { Id = cm.ClientID, paramId = cm.ParameterID, Date = cm.ChangeDateTime },
                        md => new { Id = md.ClientID, paramId = md.ParameterID, Date = md.Date },
                        (cm, md) => new
                        {
                            cm.ClientID,
                            cm.ParameterID,
                            cm.ParameterValue
                        });


                var clientParameters = clientParametersQuery.AsEnumerable()
                    .GroupBy(c => c.ClientID)
                    .Select(g => new {
                        ClientID = g.Key,
                        Weight = g.Where(c => c.ParameterID == 1).Select(c => c.ParameterValue).FirstOrDefault(),
                        Height = g.Where(c => c.ParameterID == 2).Select(c => c.ParameterValue).FirstOrDefault(),
                        NextPaymentDate = g.Where(c => c.ParameterID == 9).Select(c => c.ParameterValue).FirstOrDefault()
                    });


                ClientDetailsVM client = db.UserDetails
                    .Where(ud => ud.IdentityString.Equals(id))
                    .Join(
                        db.ClientDetails,
                        ud => ud.UserID,
                        cd => cd.UserID,
                        (ud, cd) => new
                        {
                            ClientID = cd.ClientID,
                            ClientFullName = $"{ud.LastName} {ud.FirstName}",
                            MemberShipStartDate = cd.MemberShipStartDate,
                            IdentityString = ud.IdentityString
                        })
                    .AsEnumerable()
                    .GroupJoin(
                          clientParameters.AsEnumerable(),
                          tab1 => tab1.ClientID,
                          tab2 => tab2.ClientID,
                          (tab1, tab2) => new { tab1, tab2 }
                    )
                    .SelectMany(
                        x => x.tab2.DefaultIfEmpty(),
                        (x, y) => new ClientDetailsVM
                        {
                            ClientID = x.tab1.ClientID,
                            IdentityString = x.tab1.IdentityString,
                            ClientFullName = x.tab1.ClientFullName,
                            MemberShipStartDate = x.tab1.MemberShipStartDate,
                            Height = y == null ? 0 : y.Height,
                            NextPaymentDate = UnixTimeStampToDateTime(y == null ? 0 : y.NextPaymentDate),
                            BMI = y == null ? null : GetBMI(y.Height, y.Weight),
                            CurrentWeight = y == null ? 0 : y.Weight,
                            ClientMeasures = db.ClientMeasures
                                               .Where(m => m.ClientID == x.tab1.ClientID && m.ParameterID == 1)
                                               .OrderBy(m => m.ChangeDateTime)
                                               .ToList()
                        }).FirstOrDefault();

                if (client == null)
                    return Ok(new { status = false, result = "Client not found" });

                client.ClientMeasures = FillClientMeasures(client.ClientMeasures);
                return Ok(new { status = true, result = client });
            }
            catch (Exception ex)
            {

                return Ok(new { status = false, result = ex });
            }
           
        }

        // GET api/clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientMeasures>> GetWeight(long id)
        {
            try
            {
                ClientMeasures measure = await db.ClientMeasures.FirstOrDefaultAsync(x => x.MeasureID == id);
                if (measure == null)
                    return Ok(new { status = false, result = "Measure not found" });
                return Ok(new { status = true, result = measure });
            }
            catch (Exception ex)
            {

                return Ok(new { status = false, result = ex });
            }            
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<ClientMeasures>>> GetWeightsList(long clientID)
        {
            var measures = await db.ClientMeasures
                               .Where(m => m.ClientID == clientID && m.ParameterID == 1)
                               .OrderBy(m => m.ChangeDateTime)
                               .ToListAsync();

            return FillClientMeasures(measures);

        }

        // POST api/clients
        [HttpPost]
        public async Task<ActionResult<ClientMeasures>> AddWeight(ClientMeasures item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            try
            {
                item.ChangeDateTime = DateTime.Now;
                db.ClientMeasures.Add(item);
                await db.SaveChangesAsync();

                item.ChangeFromLastWeight = item.ParameterValue - item.LastWeight;

                return Ok(new { status = true, result = item });

            }
            catch (Exception ex)
            {
                return Ok(new { status = false, result = ex });
            }
            
        }

        // POST api/clients
        [HttpPost]
        public async Task<ActionResult<IEnumerable<ClientMeasures>>> EditWeight(ClientMeasures item)
        {
            if (item == null)
            {
                return BadRequest();
            }

            try
            {
                if (!db.ClientMeasures.Any(x => x.MeasureID == item.MeasureID))
                {
                    return Ok(new { status = false, result = "Measure not found" });
                }

                db.Update(item);
                await db.SaveChangesAsync();

                var result = await GetWeightsList(item.ClientID);

                return Ok(new { status = true, result = result });
            }
            catch (Exception ex)
            {

                return Ok(new { status = false, result = ex });
            }
            
        }

        // DELETE api/clients/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<IEnumerable<ClientMeasures>>> DeleteWeight(int id)
        {
            try
            {
                ClientMeasures measure = db.ClientMeasures.FirstOrDefault(x => x.MeasureID == id);
                if (measure == null)
                {
                    return Ok(new { status = false, result = "Measure not found" });
                }
                db.ClientMeasures.Remove(measure);
                await db.SaveChangesAsync();

                var result = await GetWeightsList(measure.ClientID);

                return Ok(new { status = true, result = result });
            }
            catch (Exception ex)
            {

                return Ok(new { status = false, result = ex });
            }
            
        }



        private List<ClientMeasures> FillClientMeasures(List<ClientMeasures> measures)
        {
            var changeFromLastWeight = measures.First().ParameterValue;
            double lastWeight = 0;

            foreach (var item in measures)
            {
                item.ChangeFromLastWeight = item.ParameterValue - changeFromLastWeight;
                item.LastWeight = lastWeight;
                changeFromLastWeight = item.ParameterValue;
                lastWeight = item.ParameterValue;
            }

            return measures.OrderByDescending(m => m.ChangeDateTime).ToList();
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private double? GetBMI(double height, double weight)
        {
            if (height == 0 || weight == 0)
            {
                return null;
            }
            return weight / Math.Pow(height/100, 2);
        }

    }
}
