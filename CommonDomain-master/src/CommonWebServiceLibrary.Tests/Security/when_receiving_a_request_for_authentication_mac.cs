using CommonDomainLibrary.Security;
using CommonWebServiceLibrary.Security;
using Machine.Specifications;
using System;

namespace CommonWebServiceLibrary.Tests.Security
{
    public class when_receiving_a_request_for_authentication_mac
    {
        private static string _userName;
        private static long _timeStamp;
        private static byte[] _clientAuthenticationKey;
        private static AuthenticationMac _authenticationMac;
        private static CryptoProvider _cryptoProvider;
        private static Guid _client;
        private static string _mac;
        private static string _nonce;
        private static string _resource;

        private Establish context = () =>
        {
            _userName = "root";
            _client = new Guid("08be68c997504ab8b9560fda3b08698f");
            _timeStamp = 1399625872352;
            _cryptoProvider = new CryptoProvider();
            _clientAuthenticationKey = Convert.FromBase64String("1BWg/d6IIwDNQUCbT2M+gyebYlCn8DCYBnoiR398b3M=");
            _mac = "3jPy5xC8bWBc9ldnfVtms+mobL68EUV88Yy7xOEaGwQ=";
            _nonce = "9bab";
            _resource = "/0519ac98853f4032a052686d1e90d0bb/live-events/dcefb335d3b14a7a81ba640668582ab0";
        };

        private Because of = () =>
        {
            _authenticationMac = new AuthenticationMac(_userName, _client, _timeStamp, _nonce, "GET",
                                                              _resource, _cryptoProvider, _clientAuthenticationKey);
        };

        private It should_have_the_correct_mac =
            () => _authenticationMac.Mac.ShouldEqual(_mac);
    }
}
