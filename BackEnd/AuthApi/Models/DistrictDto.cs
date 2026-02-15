namespace AuthApi.Models;

public class DistrictDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CityId { get; set; }
}
