﻿using System;
using System.Linq;
using System.Net.Sockets;
using System.ServiceModel;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;
using NLog;

namespace Bus
{
    public class TransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns>
        /// True if the specified exception is considered as transient, otherwise false.
        /// </returns>
        public bool IsTransient(Exception ex)
        {
            //if (ex is MessagingEntityAlreadyExistsException || ex is MessagingEntityNotFoundException)
            //{
            //}
            //else
            //{
            //    Logger.ErrorException("Exception error in TransietErrorDectection", ex);
            //}

            if (ex is ServerBusyException)
                return true;
            if (ex is TimeoutException)
                return true;
            if (ex is ServerTooBusyException)
                return true;
            if (ex is MessagingCommunicationException)
                return ((MessagingCommunicationException) ex).IsTransient;
            if (ex is MessagingException)
                return ex.Message.Contains("please retry the operation") ||
                       ex.Message.Contains("The token provider service was not avaliable") ||
                       ex.Message.Contains("service was not avaliable") ||
                       ex.Message.Contains("remote server returned an error: (500) Internal Server Error")
                       || ex.Message.Contains("(409)");
            if (ex is CommunicationException)
                return true;
            if (ex is SocketException)
                return ((SocketException) ex).ErrorCode == (int) SocketError.TimedOut;
            if (ex is UnauthorizedAccessException)
                return ex.Message.Contains("The remote name could not be resolved") ||
                       ex.Message.Contains("The underlying connection was closed");
            if (ex is AggregateException)
                return ((AggregateException) ex).InnerExceptions.All(IsTransient);

            return false;
        }
    }
}
