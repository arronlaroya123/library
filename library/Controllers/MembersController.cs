
using Npgsql;
using library.Data;
using library.Models;
using Microsoft.AspNetCore.Mvc;

namespace library.Controllers;

public class MembersController : Controller
{
    private readonly MemberRepository _members;
    private readonly LoanRepository _loans;

    public MembersController(MemberRepository members, LoanRepository loans)
    {
        _members = members;
        _loans = loans;
    }

    public async Task<IActionResult> Index()
    {
        var members = await _members.GetAllAsync();
        return View(members);
    }

    // GET: Members/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Member());
    }

    // POST: Members/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }

        try
        {
            await _members.AddAsync(member);
        }
        catch (PostgresException ex) when (
            ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("Email", "Another member already uses this email.");
            return View(member);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Members/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var member = await _members.GetByIdAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }

    // POST: Members/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Member member)
    {
        if (id != member.MemberId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(member);
        }

        try
        {
            await _members.UpdateAsync(member);
        }
        catch (PostgresException ex) when (
            ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError("Email", "Another member already uses this email.");
            return View(member);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Members/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _members.DeleteAsync(id);
        }
        catch (PostgresException ex) when (
            ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            var member = await _members.GetByIdAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            ViewBag.Error = "This member has loans on record, so the database will not delete them.";

            return View("Delete", member);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Members/Details/5
    public async Task<IActionResult> Details(long id)
    {
        var member = await _members.GetByIdAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        var details = new MemberDetails
        {
            Member = member,
            Loans = await _loans.GetByMemberAsync(id)
        };

        return View(details);
    }
}
    