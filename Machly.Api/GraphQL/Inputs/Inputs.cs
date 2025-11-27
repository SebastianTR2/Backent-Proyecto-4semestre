namespace Machly.Api.GraphQL.Inputs
{
    public class CreateBookingInput
    {
        public string MachineId { get; set; } = null!;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class CreateMachineInput
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PricePerDay { get; set; }
        public string Category { get; set; } = null!;
        public string Type { get; set; } = null!;
        public double Lat { get; set; }
        public double Lng { get; set; }
        public List<string> Photos { get; set; } = new();
    }
}
