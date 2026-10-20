
using library.Data;
using Microsoft.AspNetCore.Mvc;

namespace library.Controllers;

public class NotesController : Controller
{
    private readonly NoteRepository _noteRepository;

    public NotesController(NoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<IActionResult> Index()
    {
        var notes = await _noteRepository.GetAllAsync();
        return View(notes);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(library.Models.Note note)
    {
        if (!ModelState.IsValid)
        {
            return View(note);
        }

        await _noteRepository.AddAsync(note);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var note = await _noteRepository.GetByIdAsync(id);

        if (note == null)
        {
            return NotFound();
        }

        return View("EditNote", note);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(library.Models.Note note)
    {
        if (!ModelState.IsValid)
        {
            return View("EditNote", note);
        }

        await _noteRepository.UpdateAsync(note);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        var note = await _noteRepository.GetByIdAsync(id);

        if (note == null)
        {
            return NotFound();
        }

        return View(note);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        await _noteRepository.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
