using System.Linq;
using Nancy.Bootstrapper;

namespace CommonWebServiceLibrary.Security
{
    public static class Cors
    {
        public static void Enable(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(c =>
            {
                var headers = c.Request.Headers["Access-Control-Request-Headers"];

                if (headers != null)
                {
                    c.Response.Headers["Access-Control-Allow-Headers"] = headers.FirstOrDefault() ?? string.Empty;
                }

                c.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS";
                c.Response.Headers["Access-Control-Allow-Origin"] = "*";
                c.Response.Headers["Access-Control-Allow-Credentials"] = "true";
                c.Response.Headers["Access-Control-Max-Age"] = "300";
            });
        }
    }
}
