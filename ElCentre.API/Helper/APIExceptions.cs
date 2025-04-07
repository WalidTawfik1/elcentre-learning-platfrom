namespace ElCentre.API.Helper
{
    public class APIExceptions : APIResponse
    {
        public APIExceptions(int statusCode, string message = null, string details = null) : base(statusCode, message)
        {
            Details = details;
        }
        public String Details { get; set; }
    }
}
