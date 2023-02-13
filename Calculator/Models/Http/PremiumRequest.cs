using Calculator.Models.Enums;

namespace Calculator.Models.Http
{
    public class PremiumRequest
    {
        public DateTime BirthDate { get; set; }

        public State State { get; set; }

        public int Age { get; set; }

        public Plan Plan { get; set; }
    }
}
