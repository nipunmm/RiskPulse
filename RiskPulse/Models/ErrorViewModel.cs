namespace RiskPulse.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public int StatusCode { get; set; } = 500;
        public string? ErrorMessage { get; set; }
        public string? TechnicalDetails { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string FriendlyMessage => StatusCode switch
        {
            400 => "Bad Request \u2014 The server could not understand your request.",
            401 => "Unauthorized \u2014 You need to log in to access this resource.",
            403 => "Forbidden \u2014 You don\u2019t have permission to access this page.",
            404 => "Not Found \u2014 The page you\u2019re looking for doesn\u2019t exist or has been moved.",
            408 => "Request Timeout \u2014 The server took too long to respond.",
            500 => "Internal Server Error \u2014 The server encountered an unexpected issue.",
            502 => "Bad Gateway \u2014 The server received an invalid response from upstream.",
            503 => "Service Unavailable \u2014 The system is temporarily unavailable. Please try again later.",
            _ => ErrorMessage ?? "An unexpected error occurred."
        };
    }

}
