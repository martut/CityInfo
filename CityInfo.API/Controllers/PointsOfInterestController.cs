using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers;

[Route("api/cities/{cityId}/pointsofinterest")]
[ApiController]
public class PointsOfInterestController : ControllerBase
{
    private readonly ILogger<PointsOfInterestController> _logger;
    private readonly IMailService _mailService;
    private readonly ICityInfoRepository _cityInfoRepository;
    private readonly IMapper _mapper;

    public PointsOfInterestController(ILogger<PointsOfInterestController> logger, 
        IMailService mailService, 
        ICityInfoRepository cityInfoRepository, 
        IMapper mapper)
    {
        _logger = logger;
        _mailService = mailService;
        _cityInfoRepository = cityInfoRepository;
        _mapper = mapper;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            _logger.LogInformation("City with id {cityId} wasn't found when accessing points of interest.", cityId);
            return NotFound();
        }
        var city = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);

        
        return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(city));
    }

    [HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")]
    public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int pointOfInterestId)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }

        var pointOfInterest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            
        if (pointOfInterest == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
    }

    [HttpPost]
    public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId,
        PointOfInterestForCreationDto pointOfInterest)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }
        
        var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

        await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
        
        await _cityInfoRepository.SaveAsync();
        
        var createdPointOfInterestToReturn = _mapper.Map<PointOfInterestDto>(finalPointOfInterest);

        return CreatedAtRoute("GetPointOfInterest",
            new
            {
                cityId,
                pointOfInterestId = createdPointOfInterestToReturn.Id
            }, createdPointOfInterestToReturn);
    }

    [HttpPut("{pointOfInterestId}")]
    public async Task<IActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId,
        PointOfInterestForUpdateDto pointOfInterest)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }
        
        var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
        if (pointOfInterestEntity == null)
        {
            return NotFound();
        }
        
        _mapper.Map(pointOfInterest, pointOfInterestEntity);
        
        await _cityInfoRepository.SaveAsync();
        
        return NoContent();
    }
    
    [HttpPatch("{pointOfInterestId}")]
    public async Task<IActionResult> PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId,
        JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }

        var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
        
        if (pointOfInterestEntity == null)
        {
            return NotFound();
        }

        var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

        patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        if (!TryValidateModel(pointOfInterestToPatch))
        {
            return ValidationProblem(ModelState);
        }

        _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);
        
        await _cityInfoRepository.SaveAsync();

        return NoContent();
    }
    
    [HttpDelete("{pointOfInterestId}")]
    public async Task<IActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
    {
        if (!await _cityInfoRepository.CityExistsAsync(cityId))
        {
            return NotFound();
        }
        
        var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

        if (pointOfInterestEntity == null)
        {
            return NotFound();
        }

        _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
        
        await _cityInfoRepository.SaveAsync();
        
        _mailService.Send("Point of interest deleted.",
            $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");
        
        return NoContent();
    }
}