using System;
using System.Collections.Generic;
using System.Linq;

namespace MttApi.Models
{
    public class Condition
    {
        public Guid Id { get; set; }
        public int ConditionId { get; set; }

        public static IList<int> GetInitialIds()
        {
            // Generate ids
            var ids = Enumerable.Range(0, 16).ToList();

            // Shuffle ids
            var rng = new Random();
            int n = ids.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int value = ids[k];
                ids[k] = ids[n];
                ids[n] = value;
            }

            return ids;
        }
    }
}