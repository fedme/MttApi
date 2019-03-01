using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using MttApi.Models;
using Microsoft.AspNetCore.Authorization;
using Amazon.MTurk;
using Amazon.MTurk.Model;
using System.Threading.Tasks;


// TODO: This controller need major refactoring, extract method bodies to helpers/services


namespace MttApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MturkController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // TODO: move to config
        private readonly string AwsAccessKeyId = "";
        private readonly string AwsSecretAccessKey = "";
        private readonly string SANDBOX_URL = "https://mturk-requester-sandbox.us-east-1.amazonaws.com";
        private readonly string PROD_URL = "https://mturk-requester.us-east-1.amazonaws.com";
        private AmazonMTurkClient MturkClient = null;
        private AmazonMTurkClient MturkClientSandbox = null;

        public MturkController(ApplicationDbContext context)
        {
            _context = context;

            // Production Mturk client
            AmazonMTurkConfig config = new AmazonMTurkConfig();
            config.ServiceURL = PROD_URL;
            MturkClient = new AmazonMTurkClient(AwsAccessKeyId, AwsSecretAccessKey, config);

            // Sandbox Mturk client
            AmazonMTurkConfig configSandbox = new AmazonMTurkConfig();
            configSandbox.ServiceURL = SANDBOX_URL;
            MturkClientSandbox = new AmazonMTurkClient(AwsAccessKeyId, AwsSecretAccessKey, configSandbox);
        }


        [HttpGet]
        public ActionResult<string> Index()
        {
            return "Mturk Client";
        }


        ////////////////////////////////////////////////////
        // GetBalance (Sandbox)
        ////////////////////////////////////////////////////

        [HttpGet(nameof(GetBalanceSandbox), Name = nameof(GetBalanceSandbox))]
        public async Task<ActionResult<GetAccountBalanceResponse>> GetBalanceSandbox()
        {
            GetAccountBalanceRequest request = new GetAccountBalanceRequest();
            return await MturkClientSandbox.GetAccountBalanceAsync(request);
        }


        ////////////////////////////////////////////////////
        // GetBalance
        ////////////////////////////////////////////////////

        [HttpGet(nameof(GetBalance), Name = nameof(GetBalance))]
        public async Task<ActionResult<GetAccountBalanceResponse>> GetBalance()
        {
            GetAccountBalanceRequest request = new GetAccountBalanceRequest();
            return await MturkClient.GetAccountBalanceAsync(request);
        }


        ////////////////////////////////////////////////////
        // SyncHitFromMturk (Sandbox)
        ////////////////////////////////////////////////////

        [HttpGet("[action]/{hitId}", Name = nameof(SyncHitFromMturkSandbox))]
        public async Task<ActionResult<int>> SyncHitFromMturkSandbox(string hitId)
        {
            var updates = 0;
            try
            {

                ListAssignmentsForHITRequest request = new ListAssignmentsForHITRequest();
                ListAssignmentsForHITResponse res = new ListAssignmentsForHITResponse();
                
                // Loop over request pages...
                do
                {
                    // Prepare request (with pagination)
                    request.HITId = hitId;
                    if (!string.IsNullOrEmpty(res.NextToken))
                        request.NextToken = res.NextToken;

                    // Request list of assignments for this HIT
                    res = await MturkClientSandbox.ListAssignmentsForHITAsync(request);

                    // Foreach assignment in the HIT...
                    foreach (var assignment in res.Assignments)
                    {

                        // Find correpsonding record in our db
                        var record = _context.Records.FirstOrDefault(r =>
                            r.AssignmentId == assignment.AssignmentId
                            && r.WorkerId == assignment.WorkerId
                        );

                        if (record != null)
                        {
                            // Verifiy record
                            record.Verified = true;

                            // Update record status
                            if (assignment.AssignmentStatus == AssignmentStatus.Approved)
                            {
                                record.Status = RecordStatus.Approved;
                            }
                            else if (assignment.AssignmentStatus == AssignmentStatus.Submitted
                                && record.Status != RecordStatus.Failed)
                            {
                                record.Status = RecordStatus.Submitted;
                            }
                            else if (assignment.AssignmentStatus == AssignmentStatus.Rejected)
                            {
                                record.Status = RecordStatus.Rejected;
                            }

                            record.StatusUpdateLog =  $"Synced on {DateTime.Now:dd/MM/yyyy HH:mm}";
                            updates++;
                        }

                        _context.SaveChanges(); // Save changes to db
                    }

                }
                while (!string.IsNullOrEmpty(res.NextToken));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return updates;
        }


        ////////////////////////////////////////////////////
        // SyncHitFromMturk
        ////////////////////////////////////////////////////

        [HttpGet("[action]/{hitId}", Name = nameof(SyncHitFromMturk))]
        public async Task<ActionResult<int>> SyncHitFromMturk(string hitId)
        {
            var updates = 0;
            try
            {

                ListAssignmentsForHITRequest request = new ListAssignmentsForHITRequest();
                ListAssignmentsForHITResponse res = new ListAssignmentsForHITResponse();
                
                // Loop over request pages...
                do
                {
                    // Prepare request (with pagination)
                    request.HITId = hitId;
                    if (!string.IsNullOrEmpty(res.NextToken))
                        request.NextToken = res.NextToken;

                    // Request list of assignments for this HIT
                    res = await MturkClient.ListAssignmentsForHITAsync(request);

                    // Foreach assignment in the HIT...
                    foreach (var assignment in res.Assignments)
                    {

                        // Find correpsonding record in our db
                        var record = _context.Records.FirstOrDefault(r =>
                            r.AssignmentId == assignment.AssignmentId
                            && r.WorkerId == assignment.WorkerId
                        );

                        if (record != null)
                        {
                            // Verifiy record
                            record.Verified = true;

                            // Update record status
                            if (assignment.AssignmentStatus == AssignmentStatus.Approved)
                            {
                                record.Status = RecordStatus.Approved;
                            }
                            else if (assignment.AssignmentStatus == AssignmentStatus.Submitted
                                && record.Status != RecordStatus.Failed)
                            {
                                record.Status = RecordStatus.Submitted;
                            }
                            else if (assignment.AssignmentStatus == AssignmentStatus.Rejected)
                            {
                                record.Status = RecordStatus.Rejected;
                            }

                            record.StatusUpdateLog = $"Synced on {DateTime.Now:dd/MM/yyyy HH:mm}";
                            updates++;
                        }

                        _context.SaveChanges(); // Save changes to db
                    }

                }
                while (!string.IsNullOrEmpty(res.NextToken));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return updates;
        }


        ////////////////////////////////////////////////////
        // ListHitAssignments (Sandbox)
        ////////////////////////////////////////////////////

        [HttpGet("[action]/{hitId}", Name = nameof(ListHitAssignmentsSandbox))]
        public async Task<ActionResult<List<Assignment>>> ListHitAssignmentsSandbox(string hitId)
        {
            var assignments = new List<Assignment>();
            try
            {
                ListAssignmentsForHITRequest request = new ListAssignmentsForHITRequest();
                ListAssignmentsForHITResponse res = new ListAssignmentsForHITResponse();
                do
                {
                    request.HITId = hitId;
                    if (!string.IsNullOrEmpty(res.NextToken))
                        request.NextToken = res.NextToken;
                    res = await MturkClientSandbox.ListAssignmentsForHITAsync(request);
                    assignments.AddRange(res.Assignments);
                }
                while (!string.IsNullOrEmpty(res.NextToken));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            return assignments;
        }


        ////////////////////////////////////////////////////
        // ListHitAssignments
        ////////////////////////////////////////////////////

        [HttpGet("[action]/{hitId}", Name = nameof(ListHitAssignments))]
        public async Task<ActionResult<List<Assignment>>> ListHitAssignments(string hitId)
        {
            var assignments = new List<Assignment>();
            try
            {
                ListAssignmentsForHITRequest request = new ListAssignmentsForHITRequest();
                ListAssignmentsForHITResponse res = new ListAssignmentsForHITResponse();
                do
                {
                    request.HITId = hitId;
                    if (!string.IsNullOrEmpty(res.NextToken))
                        request.NextToken = res.NextToken;
                    res = await MturkClient.ListAssignmentsForHITAsync(request);
                    assignments.AddRange(res.Assignments);
                }
                while (!string.IsNullOrEmpty(res.NextToken));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            return assignments;
        }
        

        ////////////////////////////////////////////////////
        // ApproveAll (Sandbox)
        ////////////////////////////////////////////////////

        [HttpGet(nameof(ApproveAllSandbox), Name = nameof(ApproveAllSandbox))]
        public async Task<ActionResult<List<string>>> ApproveAllSandbox()
        {
            // Get records to approve (Sandbox)
            var recordsToApproveSandbox = from r in _context.Records
                                          where (
                                              r.Status == RecordStatus.Submitted
                                              && r.IsMturk == true
                                              && r.IsSandbox == true
                                              && r.Verified == true
                                              //&& !string.IsNullOrEmpty(r.DataString)
                                          )
                                          select r;

            var approvedWorkerIds = new List<string>();

            // Aprrove them and log result in db
            ApproveAssignmentRequest request = new ApproveAssignmentRequest();
            foreach (var record in recordsToApproveSandbox)
            {
                try
                {
                    request.AssignmentId = record.AssignmentId;
                    ApproveAssignmentResponse response = await MturkClientSandbox.ApproveAssignmentAsync(request);
                    record.Status = RecordStatus.Approved;
                    record.ReviewLog = $"Approved on {DateTime.Now:dd/MM/yyyy HH:mm}";
                    approvedWorkerIds.Add(record.WorkerId);
                }
                catch (Exception exception)
                {
                    record.ReviewLog = DateTime.Now.ToString("dd/MM/yyyy HH:mm") + " " + exception.Message;
                }
            }

            _context.SaveChanges();

            // Return updated recors
            return approvedWorkerIds;
        }


        ////////////////////////////////////////////////////
        // ApproveAll
        ////////////////////////////////////////////////////

        [HttpGet(nameof(ApproveAll), Name = nameof(ApproveAll))]
        public async Task<ActionResult<List<string>>> ApproveAll()
        {
            // Get records to approve
            var recordsToApprove = from r in _context.Records
                                   where (
                                       r.Status == RecordStatus.Submitted
                                       && r.IsMturk == true
                                       && r.IsSandbox == false
                                       && r.Verified == true
                                       //&& !string.IsNullOrEmpty(r.DataString)
                                   )
                                   select r;

            var approvedWorkerIds = new List<string>();

            // Aprrove them and log result in db
            ApproveAssignmentRequest request = new ApproveAssignmentRequest();
            foreach (var record in recordsToApprove)
            {
                try
                {
                    request.AssignmentId = record.AssignmentId;
                    ApproveAssignmentResponse response = await MturkClient.ApproveAssignmentAsync(request);
                    record.Status = RecordStatus.Approved;
                    record.ReviewLog = $"Approved on {DateTime.Now:dd/MM/yyyy HH:mm}";
                    approvedWorkerIds.Add(record.WorkerId);
                }
                catch (Exception exception)
                {
                    record.ReviewLog = DateTime.Now.ToString("dd/MM/yyyy HH:mm") + " " + exception.Message;
                }
            }

            _context.SaveChanges();

            // Return updated recors
            return approvedWorkerIds;
        }


        ////////////////////////////////////////////////////
        // BonusAll (Sandbox)
        ////////////////////////////////////////////////////

        [HttpGet(nameof(BonusAllSandbox), Name = nameof(BonusAllSandbox))]
        public async Task<ActionResult<List<string>>> BonusAllSandbox()
        {
            // Get records to bonus (Sandbox)
            var recordsToBonusSandbox = from r in _context.Records
                                        where (
                                            r.Status == RecordStatus.Approved
                                            && r.IsMturk == true
                                            && r.IsSandbox == true
                                            && r.BonusPaid == false
                                            && r.Verified == true
                                            && r.DataString != null
                                            && r.DataString != ""
                                        )
                                        select r;

            var bonusedWorkerIds = new List<string>();

            // Bonus them and log errors in db
            SendBonusRequest request = new SendBonusRequest();
            foreach (var record in recordsToBonusSandbox)
            {
                try
                {
                    request.AssignmentId = record.AssignmentId;
                    request.WorkerId = record.WorkerId;
                    request.BonusAmount = record.Bonus.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                    request.Reason = "Here is your bonus";
                    SendBonusResponse response = await MturkClientSandbox.SendBonusAsync(request);
                    record.BonusPaid = true;
                    record.BonusPaymentLog = $"Bonused on {DateTime.Now:dd/MM/yyyy HH:mm}";
                    bonusedWorkerIds.Add(record.WorkerId);
                }
                catch (Exception exception)
                {
                    record.BonusPaymentLog = DateTime.Now.ToString("dd/MM/yyyy HH:mm") + " " + exception.Message;
                }
            }

            _context.SaveChanges();

            // Return updated recors
            return bonusedWorkerIds;
        }


        ////////////////////////////////////////////////////
        // BonusAll
        ////////////////////////////////////////////////////

        [HttpGet(nameof(BonusAll), Name = nameof(BonusAll))]
        public async Task<ActionResult<List<string>>> BonusAll()
        {
            // Get records to bonus
            var recordsToBonus = from r in _context.Records
                                 where (
                                     r.Status == RecordStatus.Approved
                                     && r.IsMturk == true
                                     && r.IsSandbox == false
                                     && r.BonusPaid == false
                                     && r.Verified == true
                                     && r.DataString != null
                                     && r.DataString != ""
                                 )
                                 select r;

            var bonusedWorkerIds = new List<string>();

            // Bonus them and log errors in db
            SendBonusRequest request = new SendBonusRequest();
            foreach (var record in recordsToBonus)
            {
                try
                {
                    request.AssignmentId = record.AssignmentId;
                    request.WorkerId = record.WorkerId;
                    request.BonusAmount = record.Bonus.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                    request.Reason = "Here is your bonus";
                    SendBonusResponse response = await MturkClient.SendBonusAsync(request);
                    record.BonusPaid = true;
                    record.BonusPaymentLog = $"Bonused on {DateTime.Now:dd/MM/yyyy HH:mm}";
                    bonusedWorkerIds.Add(record.WorkerId);
                }
                catch (Exception exception)
                {
                    record.BonusPaymentLog = DateTime.Now.ToString("dd/MM/yyyy HH:mm") + " " + exception.Message;
                }
            }

            _context.SaveChanges();

            // Return updated recors
            return bonusedWorkerIds;
        }

    }

}