using Calculator.Models;
using Calculator.Models.Enums;
using Calculator.Models.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using System.Text;

namespace Calculator.Controllers
{
    public class PremiumController
    {
        private readonly List<Insurance> _insurances;

        public PremiumController(List<Insurance> insurances = null)
        {
            if (insurances == null)
            {
                var premiumsJson = File.ReadAllText(@"Models\Insurances.json", Encoding.UTF8);
                _insurances = JsonConvert.DeserializeObject<List<Insurance>>(premiumsJson.Replace("\"*\"", "\"Any\""));
            }
            else
            {
                _insurances = insurances;
            }
        }

        public IResult GetPremiums(PremiumRequest premiumRequest)
        {
            if (!IsValidBirthDateAndAge(premiumRequest.BirthDate, premiumRequest.Age))
            {
                var error = new ErrorResponse()
                {
                    Message = "Date of Birth and Age don't match."
                };
                return Results.BadRequest(error);
            }

            var validInsurances = new List<InsuranceResponse>();
            for (var i = 0; i < _insurances.Count; i++)
            {
                var insurance = _insurances[i];
                if (IsValidInsurance(insurance, premiumRequest.BirthDate, premiumRequest.State, premiumRequest.Age, premiumRequest.Plan))
                {
                    validInsurances.Add(new InsuranceResponse()
                    {
                        Carrier = insurance.Carrier,
                        Premium = insurance.Premium
                    });
                }
            }

            return Results.Ok(validInsurances);
        }

        private bool IsValidBirthDateAndAge(DateTime birthDate, int age)
        {
            var now = DateTime.Now;
            var years = now.Year - birthDate.Year;
            if (now < birthDate.AddYears(years))
            {
                years--;
            }

            return years == age;
        }

        private bool IsValidInsurance(Insurance insurance, DateTime birthDate, State state, int age, Plan plan)
        {
            if (insurance.BirthMonth != BirthMonth.Any &&
               (int)insurance.BirthMonth != birthDate.Month - 1)
            {
                return false;
            }

            if (insurance.State != State.Any &&
                insurance.State != state)
            {
                return false;
            }

            if (insurance.MinAge > age || insurance.MaxAge < age)
            {
                return false;
            }

            if (!insurance.Plans.Contains(plan))
            {
                return false;
            }

            return true;
        }
    }
}
