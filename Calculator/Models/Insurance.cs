using Calculator.Models.Enums;

namespace Calculator.Models
{
    public class Insurance
    {
        public string Carrier { get; set; }

        public Plan[] Plans { get; set; }

        public State State { get; set; }

        public BirthMonth BirthMonth { get; set; }

        public int MinAge { get; set; }

        public int MaxAge { get; set; }

        public double Premium { get; set; }
    }
}
