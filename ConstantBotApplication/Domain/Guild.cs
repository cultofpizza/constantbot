using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Domain
{
    public class Guild
    {
        public ulong GuildId { get; set; }
        public ulong? MonitorChannelId { get; set; }
        public ulong? ReportChannelId { get; set; }
        public ushort? Volume { get; set; }
        public bool UserMonitoring { get; set; } = false;
        public bool VoiceMonitoring { get; set; } = false;
        public bool ReactionsMonitoring { get; set; } = false;
        public bool ChannelMonitoring { get; set; } = false;
        public bool RolesMonitoring { get; set; } = false;

        [NotMapped] 
        public BitArray MonitoringConfig
        {
            get 
            {
                var bools = new bool[] { UserMonitoring, VoiceMonitoring, ReactionsMonitoring, ChannelMonitoring, RolesMonitoring };
                return new BitArray(bools);
            }
            set 
            {
                UserMonitoring = value[0];
                VoiceMonitoring = value[1]; 
                ReactionsMonitoring = value[2];
                ChannelMonitoring = value[3];
                RolesMonitoring = value[4];
            }
        }

    }
}
