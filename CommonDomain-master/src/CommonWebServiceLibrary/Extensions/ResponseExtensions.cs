using CommonDomainLibrary;
using CommonReadModelLibrary;
using CommonReadModelLibrary.Models;
using Nancy;

namespace CommonWebServiceLibrary.Extensions
{
    public static class ResponseExtensions
    {
        public static Response AsOperationRedirect(this IResponseFormatter responseFormatter, IMessage message)
        {
            return responseFormatter.AsJson(new
                {
                    operation = string.Concat("/operations/", message.CorrelationId.ToString("n")),
                    id = message.CorrelationId.AsId(typeof(RequestedOperation))
                });
        }
    }
}