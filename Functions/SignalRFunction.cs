using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace SignalRAcsFunctionApp.Functions
{
    public class SignalRFunction : ServerlessHub
    {
        [FunctionName(nameof(Negotiate))]
        public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            // userId is the email address
            var userId = req.Headers["x-ms-signalr-user-id"];
            // return user access token
            return Negotiate(userId);
        }
    }
}