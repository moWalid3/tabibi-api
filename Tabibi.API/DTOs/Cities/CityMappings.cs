using Tabibi.API.Entities;

namespace Tabibi.API.DTOs.Cities
{
    public static class CityMappings
    {
        public static CityDto ToDto(this City city)
        {
            return new CityDto
            {
                Id = city.Id,
                Name = city.Name,
            };
        }

        public static City ToEntity(this CreateCityDto dto)
        {
            return new City
            {
                Id = Guid.CreateVersion7(),
                Name = dto.Name
            };
        }

        public static void UpdateFromDto(this City city, UpdateCityDto dto)
        {
            city.Name = dto.Name;
        }
    }
}
