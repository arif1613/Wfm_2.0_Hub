using System;
using System.Text;
using CommonWebServiceLibrary.Pagination;

namespace CommonWebServiceLibrary.Search
{
    public class SearchModule : PagedModule
    {
        protected string Keyword { private set; get; }
        protected string SortBy { private set; get; }
        protected SortDirections SortDirection { private set; get; }

        public void SetSearchParameters(dynamic parameters)
        {
            SetKeyword(parameters);
            SortBy = string.IsNullOrEmpty(parameters.sortby) ? string.Empty : parameters.sortby;
            SortDirection = !string.IsNullOrEmpty(parameters.sortorder) &&
                            parameters.sortorder.Value.ToLower().Equals("desc")
                ? SortDirections.Descending
                : SortDirections.Ascending;
        }

        private void SetKeyword(dynamic parameters)
        {
            if (!string.IsNullOrEmpty(parameters.keyword))
            {
                byte[] encodedDataAsBytes = Convert.FromBase64String(parameters.keyword);
                Keyword = Encoding.UTF8.GetString(encodedDataAsBytes);
                Keyword = "*" + Keyword.Replace("_", "?") + "*";
            }
        }

        protected bool HasKeyword()
        {
            return !string.IsNullOrEmpty(Keyword);
        }

        protected bool HasSortBy()
        {
            return !string.IsNullOrEmpty(SortBy);
        }
    }

    public enum SortDirections
    {
        Ascending,
        Descending
    }
}
