using System.ComponentModel.DataAnnotations;
using BlogPlatform.Application.DTOs;
using BlogPlatform.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogsController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogsController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogDto>>> GetAll()
    {
        var blogs = await _blogService.GetAllBlogsAsync();
        return Ok(blogs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BlogDto>> GetById(int id)
    {
        var blog = await _blogService.GetBlogByIdAsync(id);
        if (blog == null)
        {
            return NotFound(new { message = $"Blog with ID {id} not found" });
        }
        return Ok(blog);
    }

    [HttpPost]
    public async Task<ActionResult<BlogDto>> Create([FromBody] CreateBlogRequest request)
    {
        try
        {
            var blog = await _blogService.CreateBlogAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = blog.Id }, blog);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateBlogRequest request)
    {
        try
        {
            await _blogService.UpdateBlogAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _blogService.DeleteBlogAsync(id);
        return NoContent();
    }
}

