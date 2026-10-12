namespace Soccer.Api.DTOs
{
    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int PlayersQuantity { get; set; }
        public bool Enabled { get; set; }
    }
}