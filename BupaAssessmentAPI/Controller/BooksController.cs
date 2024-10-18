using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApi.Models;
using System.Net.Http;
using System;

[ApiController]
[Route("api/[controller]")]
public class BookOwnersController : ControllerBase
{
    private readonly BookOwnerService _bookOwnerService;

    public BookOwnersController(BookOwnerService bookOwnerService)
    {
        _bookOwnerService = bookOwnerService;
    }
  [HttpGet("Get")]
    public async Task<IActionResult> GetBooks()
    {
        try
        {
            var bookOwners = await _bookOwnerService.GetBookOwnersAsync();

            if (bookOwners == null || !bookOwners.Any())
            {
                return NotFound("No book owners found.");
            }

            var categorizedBooks = bookOwners.GroupBy(owner => owner.age >= 18 ? "Adults" : "Children")
                                             .ToDictionary(group => group.Key, group => group.ToList());

            return Ok(categorizedBooks);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error fetching data from  API: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
        }
    }
}
