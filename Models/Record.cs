using System;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace MttApi.Models
{
    public class Record
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string WorkerId { get; set; }
        public string AssignmentId { get; set; }
        public string HitId { get; set; }
        public bool IsMturk { get; set; }
        public bool IsSandbox { get; set; }
        public bool Verified { get; set; } = false;
        public RecordStatus Status { get; set; }
        public float Bonus { get; set; }
        public bool BonusPaid { get; set; }
        public string DataString { get; set; }
        public string ReviewLog { get; set; }
        public string BonusPaymentLog { get; set; }
        public string StatusUpdateLog { get; set; }
        
        [NotMapped]
        public JObject Data { get; set; }
    }

    public enum RecordStatus { Assigned, Started, Failed, Completed, Submitted, Approved, Rejected }
    
}