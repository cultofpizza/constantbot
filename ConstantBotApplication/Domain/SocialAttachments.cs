using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Domain;

public class SocialAttachments
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public ulong GuildId { get; set; }
    public SocialActionType Action { get; set; }
    public string Url { get; set; }
}
