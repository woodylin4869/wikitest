using System.Collections.Generic;
using System.Linq;

namespace H1_ThirdPartyWalletAPI.Extensions
{
    public static class DictionaryExtension
    {
        public static Dictionary<TKey, TValue> MergeDictionary<TKey, TValue>(this Dictionary<TKey, TValue>[] collections)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            foreach (var each in collections)
            {
                result.Concat(each);
            }
            return result;
        }
    }
}
