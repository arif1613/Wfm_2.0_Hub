using System;
using System.Net;
using System.Threading;
using CommonSystemTestLibrary.Models;
using NodaTime;

namespace CommonSystemTestLibrary.Services
{
    public class RequestedOperationsService
    {
        private readonly string _resourceUrl;

        public static void Initialize(string resourceUrl)
        {
            Instance = new RequestedOperationsService(resourceUrl);
        }

        private RequestedOperationsService(string resourceUrl)
        {
            _resourceUrl = resourceUrl;
        }

        public static RequestedOperationsService Instance { get; private set; }

        public RequestedOperationResponse WaitForRequestedOperation(Guid holderId, Guid operationId,
            string username, Duration? maxWaitTime = null)
        {
            return WaitForRequestedOperation(holderId, operationId, new AuthenticationInfo() {Username = username},
                maxWaitTime);
        }
        public RequestedOperationResponse WaitForRequestedOperation(Guid holderId, Guid operationId,
            AuthenticationInfo authentication, Duration? maxWaitTime = null)
        {
            const int interval = 1000;
            var totalWaitTime = 0;
            RequestedOperationResponse requestedOperation = null;

            while (maxWaitTime == null || totalWaitTime < maxWaitTime.Value.ToTimeSpan().TotalMilliseconds)
            {
                try
                {
                    var response =
                        RequestFactory.Instance.CreateGETRequest(_resourceUrl + "/" + holderId.ToString("n") +
                                                                 "/operations/" + operationId.ToString("n"), authentication)
                            .GetResponse();
                    requestedOperation = ResponseConverter.ConvertResponseToObject<RequestedOperationResponse>(response);
                    if (requestedOperation.Completed)
                        return requestedOperation;
                }
                catch (WebException e)
                {
                    if (!e.Message.Contains("Not Found"))
                        throw;
                }

                Thread.Sleep(interval);
                totalWaitTime += interval;
            }

            throw new TimeoutException("The requested operation did not complete" +
                                       (requestedOperation != null ? ": " + requestedOperation.Status : ""));
        }
    }
}
