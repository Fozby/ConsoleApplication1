namespace ConsoleApplication1
{
    public enum RiotHttpStatusCode
    {
        OK = 200,
        Unauthorized = 401,
        GameDataNotFound = 404,
        RateLimitExceeded = 429,
        InternalServerError = 500,
        ServiceUnavailable = 503,
    }
}
