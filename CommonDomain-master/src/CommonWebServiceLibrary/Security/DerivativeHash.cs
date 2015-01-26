namespace CommonWebServiceLibrary.Security
{
    public class DerivativeHash
    {
        public DerivativeHash(byte[] hash, byte[] salt, int iterations)
        {
            Hash = hash;
            Salt = salt;
            Iterations = iterations;
        }

        public byte[] Hash { get; private set; }
        public byte[] Salt { get; private set; }
        public int Iterations { get; private set; }
    }
}