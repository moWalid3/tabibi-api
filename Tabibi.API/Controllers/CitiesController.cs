using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tabibi.API.Common;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Cities;
using Tabibi.API.DTOs.Departments;
using Tabibi.API.Entities;

namespace Tabibi.API.Controllers
{
    [Authorize]
    [Route("cities")]
    [ApiController]
    public class CitiesController(AppDbContext dbContext) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<List<CityDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll()
        {
            List<CityDto> cities = await dbContext.Cities.AsNoTracking()
                .Select(c => c.ToDto())
                .ToListAsync();

            return Ok(cities);
        }

        [HttpGet("{id}")]
        [ProducesResponseType<CityDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            City? city = await dbContext.Cities.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id.ToString() == id);

            if (city == null)
            {
                return NotFound();
            }

            CityDto cityDto = city.ToDto();

            return Ok(cityDto);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            CreateCityDto createCityDto,
            IValidator<CreateCityDto> validator)
        {
            await validator.ValidateAndThrowAsync(createCityDto);

            City city = createCityDto.ToEntity();

            await dbContext.Cities.AddAsync(city);
            await dbContext.SaveChangesAsync();

            CityDto cityDto = city.ToDto();

            return CreatedAtAction(nameof(GetById), new { id = cityDto.Id }, cityDto);
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            string id,
            UpdateCityDto updateCityDto,
            IValidator<UpdateCityDto> validator)
        {
            await validator.ValidateAndThrowAsync(updateCityDto);

            City? city = await dbContext.Cities
                .FirstOrDefaultAsync(d => d.Id.ToString() == id);

            if (city == null)
            {
                return NotFound();
            }

            city.UpdateFromDto(updateCityDto);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string id)
        {
            City? city = await dbContext.Cities
                .FirstOrDefaultAsync(d => d.Id.ToString() == id);

            if (city == null)
            {
                return NotFound();
            }

            dbContext.Cities.Remove(city);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
