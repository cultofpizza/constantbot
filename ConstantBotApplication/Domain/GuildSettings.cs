using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Domain
{
    public class GuildSettings
    {
        public ulong GuilId { get; set; }
        public bool MonitoringEnable { get; set; }
        public ulong? MonitorChannelId { get; set; }
        public ulong? ReportChannelId { get; set; }
    }
}
