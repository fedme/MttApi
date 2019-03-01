using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using MttApi.Models;

namespace MttApi.Controllers
{
    [Route("api/forbiddenWorkers")]
    [ApiController]

    public class ForbiddenWorkersController: ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ForbiddenWorkersController(ApplicationDbContext context)
        {
            _context = context;

            if (_context.BlacklistedWorkers.Count() == 0)
            {
               SeedDb();
            }
        }

        [HttpGet]
        public ActionResult<List<string>> GetAll()
        {       
            return GetIds();
        }

        [HttpGet("{workerId}", Name = "CheckById")]
        public ActionResult<Object> CheckById(string workerId)
        {
            var ids = GetIds();
            return new { forbidden = ids.Contains(workerId) };
        }

        private List<string> GetIds()
        {
            var usedIds = from r in _context.Records 
                where (
                    (r.Status == RecordStatus.Approved 
                        || r.Status == RecordStatus.Submitted 
                        || r.Status == RecordStatus.Completed) 
                    && r.WorkerId != null
                    && r.WorkerId != ""
                )
                select r.WorkerId;

            var blacklistedIds = from w in _context.BlacklistedWorkers select w.WorkerId;

            var ids = usedIds.ToList();
            ids.AddRange(blacklistedIds.ToList());

            return ids;
        }

        private void SeedDb()
        {
            foreach(var id in BlacklistedWorker.GetInitialIds())
            {
                _context.BlacklistedWorkers.Add(new BlacklistedWorker { WorkerId = id });
            }
            _context.SaveChanges();
        }

    }

}