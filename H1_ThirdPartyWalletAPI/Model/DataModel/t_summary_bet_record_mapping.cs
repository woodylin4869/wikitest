using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace H1_ThirdPartyWalletAPI.Model.DataModel
{
    public class t_summary_bet_record_mapping : IEquatable<t_summary_bet_record_mapping>
    {
        public Guid summary_id { get; set; }

        public DateTime report_time { get; set; }

        public DateTime partition_time { get; set; }

        public bool Equals(t_summary_bet_record_mapping? other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return summary_id == other.summary_id && report_time == other.report_time && partition_time == other.partition_time;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(summary_id, report_time, partition_time);
        }
    }
}
