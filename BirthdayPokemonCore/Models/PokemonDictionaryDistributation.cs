namespace BirthdayPokemonCore.Models
{
    public class PokemonDictionaryDistributation
    {
        public Dictionary<int, PokemonDistributionEntry> Dictionary { get; set; } = [];

        public int TotalEntries
        {
            get
            {
                return Dictionary.Values.Sum(entry => entry.Count);
            }
        }

        //OrderByDescending
        public IEnumerable<PokemonDistributionEntry> GetOrderedEntries()
        {
            return Dictionary.Values.OrderByDescending(entry => entry.Count);
        }

    }
}
