namespace Machly.Api.DTOs
{
    public class ReviewRequest
    {
        public int Rating { get; set; } // 1-5
        public string Comment { get; set; } = "";
    }
}

