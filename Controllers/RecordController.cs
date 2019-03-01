using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using MttApi.Models;
using Microsoft.AspNetCore.Authorization;


namespace MttApi.Controllers
{
    [Route("api/records")]
    [ApiController]
    public class RecordController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecordController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet(nameof(GetAll), Name = nameof(GetAll))]
        [Authorize]
        public ActionResult<List<Record>> GetAll()
        {      
            var records = _context.Records.ToList();

            // Parse Datastring json into each record untracked Data field
            // TODO: should be refactored inside a Record getter
            foreach (var record in records)
            {
                if (!string.IsNullOrEmpty(record.DataString))
                {
                    record.Data = JObject.Parse(record.DataString);
                    record.DataString = null; // empty the Datastring field
                }
            }

            return records;
        }

        [HttpGet(nameof(GetApproved), Name = nameof(GetApproved))]
        [Authorize]
        public ActionResult<List<Record>> GetApproved()
        {      
            var records_query = from r in _context.Records
                            where (
                                r.Status == RecordStatus.Approved
                                && r.IsMturk == true
                                && r.IsSandbox == false
                                && r.Verified == true
                            )
                            select r;
            
            var records = records_query.ToList();

            // Parse Datastring json into each record untracked Data field
            // TODO: should be refactored inside a Record getter
            foreach (var record in records)
            {
                if (!string.IsNullOrEmpty(record.DataString))
                {
                    record.Data = JObject.Parse(record.DataString);
                    record.DataString = null; // empty the Datastring field
                }
            }

            return records;
        }

        [HttpGet(nameof(GetVerificationCode), Name = nameof(GetVerificationCode))]
        public ActionResult<string> GetVerificationCode(
            [FromQuery] string workerId, 
            [FromQuery] string assignmentId, 
            [FromQuery] string hitId)
        {
            // Try to query Record
            var record = _context.Records.FirstOrDefault(r => 
                r.AssignmentId == assignmentId
                && r.WorkerId == workerId
                && r.HitId == hitId
                && (r.Status == RecordStatus.Completed || r.Status == RecordStatus.Approved)
            );

            if (record == null)
            {
                return NotFound();
            }

            return record.Id.ToString().ToLower().Substring(0, 5);
        }

        [HttpPost]
        public ActionResult<Record> CreateUpdate(Record item)
        {

            // // try some json parsing...
            // if (!string.IsNullOrEmpty(item.DataString)) {
            //     JToken token = JToken.Parse(item.DataString);
            //     string jstring = token.ToString();
            //     JObject json = JObject.Parse(jstring);
            //     Console.WriteLine(json);
            // }

            // Try to query Record
            var record = _context.Records.FirstOrDefault(r => 
                r.AssignmentId == item.AssignmentId
                && r.WorkerId == item.WorkerId
                && r.HitId == item.HitId
            );

            // If it doesn't exist, create it
            if (record == null)
            {
                item.BonusPaid = false;
                item.CreatedAt = DateTime.Now;
                item.UpdatedAt = DateTime.Now;
                _context.Records.Add(item);
                _context.SaveChanges();

                return item;
            }

            // Do not update stutus if failed
            if (record.Status == RecordStatus.Failed)
            {
                return item;
            }

            // Otherwise, update record status
            record.Status = item.Status;
            record.UpdatedAt = DateTime.Now;

            // Update entire record if status is "Completed"
            if (item.Status == RecordStatus.Completed)
            {

                if (string.IsNullOrEmpty(record.AssignmentId))
                    record.AssignmentId = item.AssignmentId;
                
                if (string.IsNullOrEmpty(record.HitId))
                    record.HitId = item.HitId;
                
                if (string.IsNullOrEmpty(record.WorkerId))
                    record.WorkerId = item.WorkerId;
                    
                record.Status = item.Status;
                record.IsMturk = item.IsMturk;
                record.IsSandbox = item.IsSandbox;
                if (item.Bonus > 2.0) {
                    record.Bonus = 2.0f;
                }
                else 
                {
                    record.Bonus = item.Bonus;
                }
                record.DataString = item.DataString;
            }

            _context.SaveChanges();

            return record;
        }

    }

}