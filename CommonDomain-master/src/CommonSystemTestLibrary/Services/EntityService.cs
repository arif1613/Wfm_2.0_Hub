using System;
using System.Net;
using System.Threading;
using CommonSystemTestLibrary.Models;
using NodaTime;

namespace CommonSystemTestLibrary.Services
{
    public abstract class EntityService
    {
        protected readonly string _resourceUrl;
        protected readonly Guid _rootHolderId;

        protected EntityService(string resourceUrl, Guid rootHolderId)
        {
            _resourceUrl = resourceUrl;
            _rootHolderId = rootHolderId;
        }

        protected RequestedOperationResponse Create(Guid holderId, object data, string senderUsername)
        {
            return Create(holderId, data, new AuthenticationInfo {Username = senderUsername});
        }
        protected RequestedOperationResponse Create(Guid holderId, object data, AuthenticationInfo authentication)
        {
            var request = RequestFactory.Instance.CreatePOSTRequest(GetEndpointForHolder(holderId), data, authentication);

            var response = request.GetResponse();

            var operationResponse = ResponseConverter.ConvertResponseToObject<OperationResponse>(response);
            return RequestedOperationsService.Instance.WaitForRequestedOperation(_rootHolderId, operationResponse.Id,
                authentication, Duration.FromSeconds(60));
        }

        protected T Get<T>(Guid holderId, Guid id, AuthenticationInfo authentication, RetryMode retry = RetryMode.NoRetry) where T : Response
        {
            var startTime = DateTime.Now;

            WebException lastException = null;

            while ((DateTime.Now - startTime).Seconds < 30)
            {
                try
                {
                    var request =
                        RequestFactory.Instance.CreateGETRequest(
                            GetEndpointForHolder(holderId) + "/" + id.ToString("N"),
                            authentication);
                    var response = request.GetResponse();

                    return ResponseConverter.ConvertResponseToObject<T>(response);
                }
                catch (WebException exception)
                {
                    lastException = exception;

                    switch (retry)
                    {
                        case RetryMode.NoRetry:
                            throw;

                        case RetryMode.RetryOnNotFound:
                            if (!exception.Message.Contains("Not Found"))
                                throw;
                            break;

                        case RetryMode.RetryOnUnauthorized:
                            if (!exception.Message.Contains(HttpStatusCode.Unauthorized.ToString()))
                                throw;
                            break;

                        case RetryMode.RetryOnAllExceptions:
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    Thread.Sleep(1000);
                }
            }

            if (lastException != null) throw lastException;
            throw new Exception("Unexpected behaviour in method Get");
        }

        public RequestedOperationResponse Delete(Guid holderId, Guid id, string senderUsername)
        {
            return Delete(holderId, id, new AuthenticationInfo {Username = senderUsername});
        }
        public RequestedOperationResponse Delete(Guid holderId, Guid id, AuthenticationInfo authentication)
        {
            var request =
                RequestFactory.Instance.CreateDELETERequest(GetEndpointForHolder(holderId) + "/" + id,
                    authentication);

            var response = request.GetResponse();

            var operationResponse = ResponseConverter.ConvertResponseToObject<OperationResponse>(response);
            return RequestedOperationsService.Instance.WaitForRequestedOperation(_rootHolderId, operationResponse.Id,
                authentication, Duration.FromSeconds(30));
        }

        protected abstract string GetEndpointForHolder(Guid holderId);
    }

    public abstract class EntityService<T> : EntityService where T : Response
    {
        protected EntityService(string resourceUrl, Guid rootHolderId) : base(resourceUrl, rootHolderId)
        {
        }

        public T Get(Guid holderId, Guid id, AuthenticationInfo authentication, RetryMode retry = RetryMode.NoRetry)
        {
             return Get<T>(holderId, id, authentication, retry);
        }

        public T Get(Guid holderId, Guid id, string username, RetryMode retry = RetryMode.NoRetry)
        {
            return Get(holderId, id, new AuthenticationInfo {Username = username}, retry);
        }
    }
}
