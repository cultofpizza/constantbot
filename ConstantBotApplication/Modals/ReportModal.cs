using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modals;

public class ReportModal : IModal
{
    public string Title => "Report";

    [InputLabel("Describe your problem")]
    [ModalTextInput("content",TextInputStyle.Paragraph,"Write your problem here")]
    public string Content { get; set; }

}
