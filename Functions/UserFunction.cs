using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SignalRAcsFunctionApp.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace SignalRAcsFunctionApp.Functions
{
    public class UserFunction
    {
        private readonly AppSettings _settings;

        public UserFunction(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        /// <summary>
        /// Get the user object from CosmosDB by the email address
        /// /api/users/{email}
        /// </summary>
        /// <returns>User</returns>
        [FunctionName(nameof(GetUser))]
        public static IActionResult GetUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: Constants.CosmosDB.DatabaseName,
                collectionName: Constants.CosmosDB.UsersContainerName,
                ConnectionStringSetting = Constants.CosmosDB.ConnectionStringName,
                Id = "{id}",
                PartitionKey = "{id}")] User user,
            string id,
            ILogger log)
        {
            if (user == null)
            {
                log.LogWarning($"User: {id} not found");
                return new NotFoundResult();
            }
            log.LogInformation(JsonConvert.SerializeObject(user));
            return new OkObjectResult(user);
        }

        /// <summary>
        /// Update the user object in CosmosDB with the ACSUserId
        /// /api/users/{email}
        /// </summary>
        /// <returns>User</returns>
        [FunctionName(nameof(UpdateUser))]
        public async Task<IActionResult> UpdateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "users/{id}")] HttpRequest req,
            [CosmosDB(
                databaseName: Constants.CosmosDB.DatabaseName,
                collectionName: Constants.CosmosDB.UsersContainerName,
                ConnectionStringSetting = Constants.CosmosDB.ConnectionStringName,
                Id = "{id}",
                PartitionKey = "{id}")] User user,
            string id,
            ILogger log)
        {
            if (user == null)
            {
                log.LogWarning($"User: {id} not found");
                return new NotFoundResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic userData = JsonConvert.DeserializeObject<User>(requestBody);

            user.AcsUserId = userData?.AcsUserId;

            log.LogInformation(JsonConvert.SerializeObject(user));
            return new OkObjectResult(user);
        }

        /// <summary>
        /// Transfer the user to a new meeting by updating the user's meeting URL
        /// /api/users/{email}/transfer/{meetingUrl}
        /// </summary>
        /// <returns>void</returns>
        [FunctionName(nameof(TransferUser))]
        public async Task TransferUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "users/{id}/transfer")] HttpRequest req,
            [CosmosDB(
                databaseName: Constants.CosmosDB.DatabaseName,
                collectionName: Constants.CosmosDB.UsersContainerName,
                ConnectionStringSetting = Constants.CosmosDB.ConnectionStringName,
                Id = "{id}",
                PartitionKey = "{id}")] User user,
            string id,
            [SignalR(HubName = "signalrfunction")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            if (user == null)
            {
                log.LogWarning($"User: {id} not found");
                return;
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic userData = JsonConvert.DeserializeObject<User>(requestBody);
            
            // update the meeting URL for the user in CosmosDB
            user.MeetingUrl = userData?.MeetingUrl;
            log.LogInformation(JsonConvert.SerializeObject(user));

            // send the user object, with updated meeting URL to the user
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    UserId = user.Id, // message will be sent to this SignalR user
                    Target = "newMessage",
                    Arguments = new[] { user }
                });

            return;
        }
    }
}
