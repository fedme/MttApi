using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MttApi.Models;
using Microsoft.EntityFrameworkCore;
using Amazon.MTurk;
using Amazon.MTurk.Model;

namespace MttApi.Services
{
    public class MturkService
    {

        // TODO: move to config
        private readonly string AwsAccessKeyId = "";
        private readonly string AwsSecretAccessKey = "";
        private readonly string SANDBOX_URL = "https://mturk-requester-sandbox.us-east-1.amazonaws.com";
        private readonly string PROD_URL = "https://mturk-requester.us-east-1.amazonaws.com";
        private AmazonMTurkClient MturkClient = null;
        private AmazonMTurkClient MturkClientSandbox = null;

        private readonly ApplicationDbContext _context;

        public MturkService(ApplicationDbContext context)
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

        public async Task<int> SyncHitFromMturk(string hitId, bool production = false)
        {
            var updates = 0;
            
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
                if (production)
                {
                    res = await MturkClient.ListAssignmentsForHITAsync(request);
                }    
                else
                {
                    res = await MturkClientSandbox.ListAssignmentsForHITAsync(request);
                }

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
           
            return updates;
        }

        
    }
}