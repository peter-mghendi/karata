namespace Karata.Server.Engine.Exceptions;

public class CardRequestedException : TurnValidationException
{
    public override string Message =>
        "A card has been requested. Your turn must either start with the requested card or block the request.";
}