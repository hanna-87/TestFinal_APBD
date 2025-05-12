using Microsoft.AspNetCore.Mvc;
using TestAPBD.Exceptions;
using TestAPBD.Models.DTOs;
using TestAPBD.Services;

namespace TestAPBD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitsController : ControllerBase
    {
      private readonly IVisitService _visitService;

      public VisitsController(IVisitService visitService)
      {
          _visitService = visitService;
      }
      
      
      
       [HttpGet("{id}")]
          public async Task<IActionResult> GetVisit(int id)
          {
              try
              {
                  var visit = await _visitService.GetVisitAsync(id);
                  return Ok(visit);
              }
              catch (NotFoundException e)
              {
                  return NotFound(e.Message);
              }
              catch (ConflictException e)
              {
                  return Conflict(e.Message);
              }
              catch (BadRequestException e)
              {
                  return BadRequest(e.Message);
              }
              catch (Exception e)
              {
                  return StatusCode(500, e.Message);
              }
            
          }



          [HttpPost()]
          public async Task<IActionResult> CreateVisit([FromBody] CreateVisitDto visit)
          {
              if (!ModelState.IsValid)
              {
                  return BadRequest($"Data for Visit was provided incorrectly: {ModelState}");
              }
          
              try
              {
                  await _visitService.CreateVisitAsync(visit);
                  return Created(string.Empty, visit);
              }
              catch (BadRequestException e)
              {
                  return BadRequest(e.Message);
              }
              catch (ConflictException e)
              {
                  return Conflict(e.Message);
              }
              catch (Exception e)
              {
                  return StatusCode(500, e.Message);
              }
          }
      
    }
    
   
}

