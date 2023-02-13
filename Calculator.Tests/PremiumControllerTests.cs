using Calculator.Controllers;
using Calculator.Models;
using Calculator.Models.Enums;
using Calculator.Models.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace Calculator.Tests
{
    [TestClass]
    public class PremiumControllerTests
    {
        [TestMethod]
        public async Task GetPremiums_ShouldReturnCorrectPremiums()
        {
            var testInsurances = GetTestInsurances();
            var controller = new PremiumController(testInsurances);

            var years = 30;
            var birthDate = DateTime.Now.AddYears(-years);
            var premiumRequest = new PremiumRequest()
            {
                BirthDate = birthDate,
                State = State.AK,
                Age = years,
                Plan = Plan.A
            };

            var result = controller.GetPremiums(premiumRequest);
            var insuranceResponses = await GetResultValue<List<InsuranceResponse>>(result);
            Assert.IsNotNull(insuranceResponses);
            Assert.IsTrue(insuranceResponses.Count == 1);

            years = 51;
            birthDate = DateTime.Now.AddYears(-years).AddMonths(-DateTime.Now.Month + /* January */ 1);
            premiumRequest = new PremiumRequest()
            {
                BirthDate = birthDate,
                State = State.AK,
                Age = years,
                Plan = Plan.B
            };

            result = controller.GetPremiums(premiumRequest);
            insuranceResponses = await GetResultValue<List<InsuranceResponse>>(result);
            Assert.IsNotNull(insuranceResponses);
            Assert.AreEqual(insuranceResponses[0].Carrier, testInsurances[2].Carrier);
        }

        [TestMethod]
        public async Task GetPremiums_ShouldReturnNoPremiums()
        {
            var testInsurances = GetTestInsurances();
            var controller = new PremiumController(testInsurances);

            var premiumRequest = new PremiumRequest()
            {
                BirthDate = DateTime.Today,
                State = State.AK,
                Age = 0,
                Plan = Plan.A
            };

            var result = controller.GetPremiums(premiumRequest);
            var insuranceResponses = await GetResultValue<List<InsuranceResponse>>(result);
            Assert.IsNotNull(insuranceResponses);
            Assert.IsTrue(insuranceResponses.Count == 0);
        }

        [TestMethod]
        public async Task GetPremiums_ShouldReturnError()
        {
            var testInsurances = GetTestInsurances();
            var controller = new PremiumController(testInsurances);

            var premiumRequest = new PremiumRequest()
            {
                BirthDate = DateTime.Parse("1901-01-01"),
                State = State.AK,
                Age = 10,
                Plan = Plan.A
            };

            var result = controller.GetPremiums(premiumRequest);
            var error = await GetResultValue<ErrorResponse>(result);
            Assert.IsNotNull(error);
        }

        private List<Insurance> GetTestInsurances()
        {
            var testInsurances = new List<Insurance>
            {
                new Insurance()
                {
                    Carrier = "Test Carrier 0",
                    Plans = new[] { Plan.A, Plan.C },
                    State = State.Any,
                    BirthMonth = BirthMonth.Any,
                    MinAge = 1,
                    MaxAge = 100,
                    Premium = 111
                },
                new Insurance()
                {
                    Carrier = "Test Carrier 1",
                    Plans = new[] { Plan.A },
                    State = State.AK,
                    BirthMonth = BirthMonth.January,
                    MinAge = 1,
                    MaxAge = 49,
                    Premium = 222
                },
                new Insurance()
                {
                    Carrier = "Test Carrier 2",
                    Plans = new[] {  Plan.B },
                    State = State.AK,
                    BirthMonth = BirthMonth.January,
                    MinAge = 51,
                    MaxAge = 100,
                    Premium = 333
                }
            };
            return testInsurances;
        }

        private int GetAgeFromBirthDate(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            var months = today.Month - birthDate.Month;
            if (months < 0 ||
                (months == 0 && today.Day < birthDate.Day))
            {
                age--;
            }
            return age;
        }

        // Workaround to read value of IResult (https://stackoverflow.com/a/71323287)
        private static async Task<T> GetResultValue<T>(IResult result)
        {
            var mockHttpContext = new DefaultHttpContext
            {
                RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider(),
                Response = { Body = new MemoryStream() }
            };

            await result.ExecuteAsync(mockHttpContext);

            string json;
            var bodyStream = mockHttpContext.Response.Body;
            bodyStream.Position = 0;
            using (var streamReader = new StreamReader(bodyStream))
            {
                json = await streamReader.ReadToEndAsync();
            }

            if (TryParseJson(json, out T value))
            {
                return value;
            }

            return default;
        }

        public static bool TryParseJson<T>(string json, out T value)
        {
            var success = true;

            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) =>
                {
                    success = false;
                    args.ErrorContext.Handled = true;
                },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            value = JsonConvert.DeserializeObject<T>(json, settings);

            return success;
        }
    }
}
