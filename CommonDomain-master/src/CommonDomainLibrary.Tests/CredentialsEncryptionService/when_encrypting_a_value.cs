using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Machine.Specifications;

namespace CommonDomainLibrary.Tests.CredentialsEncryptionService
{
    public class when_encrypting_a_value
    {
        private static CommonDomainLibrary.CredentialsEncryptionService _service;
        private static string _code;
        private static string _value;
        private static X509Certificate2 _cert;

        private Establish context = () =>
            {
                _value = "testValue";

                using (var ms = new MemoryStream())
                {
                    var embedderCert =
                        Assembly.GetExecutingAssembly()
                                .GetManifestResourceStream("CommonDomainLibrary.Tests.testCertificate.pfx");
                    embedderCert.CopyTo(ms);
                    _cert = new X509Certificate2(ms.ToArray(), "test");
                    _service = new CommonDomainLibrary.CredentialsEncryptionService(_cert);
                }
            };

        private Because of = () => _code = _service.Encrypt(_value);

        private It the_value_should_be_encrypted_using_the_certificate = () =>
            {
                var rsaEncryptor = (RSACryptoServiceProvider)_cert.PrivateKey;
                byte[] plainData = rsaEncryptor.Decrypt(Convert.FromBase64String(_code), true);
                var decrypted = Encoding.UTF8.GetString(plainData);
                decrypted.ShouldEqual(_value);
            };        
    }
}
