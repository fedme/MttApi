using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using MttApi.Models;

namespace MttApi.Controllers
{
    [Route("api/conditions")]
    [ApiController]
    public class ConditionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConditionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<List<Condition>> GetAll()
        {
            if (_context.Conditions.Count() == 0)
               SeedDb();
            return _context.Conditions.ToList();
        }

        [HttpGet("random", Name = "GetRandomConditionId")]
        public ActionResult<Condition> GetRandom()
        {
            if (_context.Conditions.Count() == 0)
                SeedDb();

            // Get first condition from db
            var item = _context.Conditions.FirstOrDefault();
            if (item == null)
                return NotFound();

            // Remove it from db
            _context.Conditions.Remove(item);
            _context.SaveChanges();

            // Return it
            return item;
        }

        private void SeedDb()
        {
            foreach(var id in Condition.GetInitialIds())
            {
                _context.Conditions.Add(new Condition { ConditionId = id });
            }
            _context.SaveChanges();
        }

    }

}