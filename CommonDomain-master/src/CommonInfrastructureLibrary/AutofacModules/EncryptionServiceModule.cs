using System;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using CommonDomainLibrary;
using Microsoft.WindowsAzure;
using NLog;

namespace CommonInfrastructureLibrary.AutofacModules
{
    public class EncryptionServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var logger = LogManager.GetCurrentClassLogger();

            X509Certificate2 cert = null;
            var store = new X509Store(StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            var certCollection = store.Certificates;
            logger.Info("Found {0} certificates in the certificate store", certCollection.Count);
            foreach (var c in certCollection)
            {
                logger.Info("Certificate name '{0}', thumbprint '{1}'", c.FriendlyName, c.Thumbprint);
            }            
            var filtered = certCollection.Find(X509FindType.FindByThumbprint, CloudConfigurationManager.GetSetting("CertificateThumbprint"), false);
            if (filtered.Count > 0) cert = filtered[0];
            else throw new SystemException("No encryption certificate with thumbprint " + CloudConfigurationManager.GetSetting("CertificateThumbprint") + " found !!");

            builder.RegisterInstance(new CredentialsEncryptionService(cert)).As<ICredentialsEncryptionService>();
        }
    }
}
