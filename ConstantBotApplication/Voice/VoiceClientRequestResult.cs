using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConstantBotApplication.Voice;

public class VoiceClientRequestResult
{
    public readonly VoiceClient VoiceClient;

    public readonly VoiceClientResult Result;

    public readonly Exception Exception;

    public VoiceClientRequestResult(VoiceClient client, VoiceClientResult result, Exception exception)
    {
        VoiceClient = client;
        Result = result;
        Exception = exception;
    }
    public VoiceClientRequestResult(VoiceClient client, VoiceClientResult result)
    {
        VoiceClient = client;
        Result = result;
        Exception = null;
    }
    public VoiceClientRequestResult(VoiceClient client)
    {
        VoiceClient = client;
        Result = VoiceClientResult.AlredyConnected;
        Exception = null;
    }
}
public enum VoiceClientResult
{
    AlredyConnected,
    CreatedNewClient,
    ClientAlreadyInAnotherChannel,
    Error
}
