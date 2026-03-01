using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Festify.Api.Data;
using Festify.Api.Models;

namespace Festify.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController(FestifyDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Events.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await db.Events.FindAsync(id);
        return ev is null ? NotFound() : Ok(ev);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Event ev)
    {
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ev);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Event ev)
    {
        if (id != ev.Id) return BadRequest();
        var existing = await db.Events.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Name = ev.Name;
        existing.Location = ev.Location;
        existing.StartDate = ev.StartDate;
        existing.EndDate = ev.EndDate;
        existing.Capacity = ev.Capacity;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        db.Events.Remove(ev);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
