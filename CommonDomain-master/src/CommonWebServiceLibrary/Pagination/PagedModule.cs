using Nancy;

namespace CommonWebServiceLibrary.Pagination
{
    public class PagedModule : NancyModule
    {
        protected int skip { get; set; }
        protected int take { get; set; }

        public void SetSkipAndTake(dynamic parameters)
        {
            take = 100;

            skip = parameters.from_index != null ? parameters.from_index : 0;
            int toValue = parameters.to_index != null ? parameters.to_index : 0;
            if (toValue != 0 && toValue - skip < take)
            {
                take = toValue - skip;
            }
        }
    }
}
