using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Modals;

public class SearchModal : IModal
{
    public string Title => "Search";

    [InputLabel("Name or url of track")]
    [ModalTextInput("track",TextInputStyle.Short)]
    public string Track { get; set; }

}
