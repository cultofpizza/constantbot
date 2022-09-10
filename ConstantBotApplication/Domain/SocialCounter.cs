using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Domain
{
    public class SocialCounter
    {
        public ulong GiverId { get; set; }
        public ulong TakerId { get; set; }
        public SocialActionType Action { get; set; }
        public int Count { get; set; }
    }
    public enum SocialActionType
    {
        Cookies,
        Slaps,
        Hugs
    }
}
