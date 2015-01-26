using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Machine.Specifications;

namespace CommonDomainLibrary.Tests.CredentialsEncryptionService
{
    public class when_decrypting_a_cypher
    {
        private static CommonDomainLibrary.CredentialsEncryptionService _service;
        private static string _code;
        private static string _value;
        private static string _decryptedValue;
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
                var rsaEncryptor = (RSACryptoServiceProvider)_cert.PrivateKey;
                byte[] cipherData = rsaEncryptor.Encrypt(Encoding.UTF8.GetBytes(_value), true);
                _code = Convert.ToBase64String(cipherData);
            }
        };

        private Because of = () => _decryptedValue = _service.Decrypt(_code);

        private It the_value_should_be_decrypted_using_the_certificate = () => _decryptedValue.ShouldEqual(_value); 
    }
}
