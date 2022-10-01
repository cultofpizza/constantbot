using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modals;

public class AttachmentModal : IModal
{
    public string Title => "Attachments";

    [InputLabel("Enter attachment image urls")]
    [ModalTextInput("urls", TextInputStyle.Paragraph, "Enter urls each in new line")]
    public string Urls { get; set; }

}
