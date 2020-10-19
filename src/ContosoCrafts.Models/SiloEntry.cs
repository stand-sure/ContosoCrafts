namespace ContosoCrafts.Models
{
    public class SiloEntry
    {
        public string[] Tags { get; }
        public string SiloName { get; }
        public int SiloAddressHash { get; }
        public bool IsClient { get; }

        public SiloEntry(int siloAddressHash, string siloName, bool isClient, string[] tags)
        {
            Tags = tags;
            SiloAddressHash = siloAddressHash;
            SiloName = siloName;
            IsClient = isClient;
        }
    }
}