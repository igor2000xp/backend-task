using System.ComponentModel.DataAnnotations;
using BlogPlatform.Application.DTOs;
using BlogPlatform.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetAll()
    {
        var posts = await _postService.GetAllPostsAsync();
        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetById(int id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
        {
            return NotFound(new { message = $"Post with ID {id} not found" });
        }
        return Ok(post);
    }

    [HttpGet("blog/{blogId}")]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetByBlogId(int blogId)
    {
        var posts = await _postService.GetPostsByBlogIdAsync(blogId);
        return Ok(posts);
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostRequest request)
    {
        try
        {
            var post = await _postService.CreatePostAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
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

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdatePostRequest request)
    {
        try
        {
            await _postService.UpdatePostAsync(id, request);
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
        await _postService.DeletePostAsync(id);
        return NoContent();
    }
}

