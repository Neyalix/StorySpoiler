using System;
using System.Net;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using StorySpoiler.Models;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers;
using static System.Formats.Asn1.AsnWriter;


namespace StroySpoilerTests
{

    [TestFixture]

    public class StoryApiTests
    {
        private RestClient client;
        public static string lastCreatedStoryId;
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

      
        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("Phill93", "Neyalix93@07");
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this.client  =new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = tempClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }


        [Test, Order(1)]
        public void CreateNewStory_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Story/Create", Method.Post);
            var storyRequest = new StoryDTO()
            {

                Title = "New Story Line",
                Description = "Description for the Exam",
                Url = ""
            };

            request.AddJsonBody(storyRequest);
            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createResponse.Msg, Does.Contain("Successfully created!"));


            lastCreatedStoryId = createResponse.StoryId;
        }


        [Test, Order(2)]
        
        public void EditCreatedStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Edit/{lastCreatedStoryId}", Method.Put);

            var editRequest = new StoryDTO
            {                
                Title = "Edited story line",
                Description = "Edited description",
                Url = ""
            };

            request.AddJsonBody(editRequest);

            var response = client.Execute(request);
            var editedRespone = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));
            Assert.That(editedRespone.Msg, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]

        public void GetAllStories_ShouldReturnOK()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var stories = JsonSerializer.Deserialize<List<StoryDTO>>(response.Content);
            Assert.That(stories, Is.Not.Null.And.Not.Empty);
        }

        [Test, Order(4)]

        public void DeleteStory_ShouldReturnOK()
        {
            var request = new RestRequest($"/api/Story/Delete/{lastCreatedStoryId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var jsonResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(jsonResponse.Msg,Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]

        public void CreateStoryWuthoutReqFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Story/Create", Method.Post);
            var storyRequest = new StoryDTO()
            {

                Title = "New Story Line",
                Url = ""
            };

            request.AddJsonBody(storyRequest);
            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            

        }

        [Test, Order(6)]

        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            var fakeId = "1234";
            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);

            var editRequest = new StoryDTO
            {
                Title = "Edited story line",
                Description = "Edited description",
                Url = ""
            };

            request.AddJsonBody(editRequest);

            var response = client.Execute(request);
            var editedRespone = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.NotFound));
            Assert.That(editedRespone.Msg, Does.Contain("No spoilers..."));
        }
        [Test, Order(7)]

        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            var fakeId = "1235";
            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var jsonResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(jsonResponse.Msg, Does.Contain("Unable to delete this story spoiler!"));
        }
        



        [OneTimeTearDown]

        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}