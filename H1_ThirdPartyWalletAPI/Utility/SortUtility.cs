using System.Collections.Generic;

namespace H1_ThirdPartyWalletAPI.Utility
{
    public class SortUtility
    {
        /// <summary>
        /// Sort
        /// </summary>
        /// <param name="IncrementColumnName">建議是IncrementColumn</param>
        /// <param name="sortColumnName"></param>
        /// <param name="isAsc"></param>
        /// <returns></returns>
        public static string SortWithIncrement(string incrementColumnName, SortType incrementSortType, string? sortColumnName, SortType? sortType)
        {
            HashSet<string> sortCondition = new HashSet<string>();
            if (string.IsNullOrEmpty(sortColumnName) == false)
            {
                sortCondition.Add($"{sortColumnName} {sortType.ToString().ToLower()}");
            }
            sortCondition.Add($"{incrementColumnName} {incrementSortType.ToString().ToLower()}");
            return $" order by {string.Join(",", sortCondition)} ";
        }
    }

    public enum SortType
    {
        Asc = 0, Desc = 1,
    }
}
