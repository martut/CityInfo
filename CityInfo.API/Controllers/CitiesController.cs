using System.Text.Json;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ICityInfoRepository _cityInfoRepository;
    private readonly IMapper _mapper;
    const int maxCitiesPageSize = 20;

    public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
    {
        _cityInfoRepository = cityInfoRepository;
        _mapper = mapper;
    }

    [HttpGet()]
    public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities(
        string? name,
        string? searchQuery,
        int pageNumber = 1,
        int pageSize = 10)
    {
        if (pageSize > maxCitiesPageSize)
        {
            pageSize = maxCitiesPageSize;
        }

        var (cities, paginationMetadata) =
            await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

        var results = _mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cities);

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
    {
        var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);

        if (city == null)
        {
            return NotFound();
        }

        if (includePointsOfInterest)
        {
            return Ok(_mapper.Map<CityDto>(city));
        }

        return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));
    }
}