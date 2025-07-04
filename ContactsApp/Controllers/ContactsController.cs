﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactsApp.Data;
using ContactsApp.Models;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using ContactsApp.Service;

namespace ContactsApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly ContactsAppDbContext _context;
        private ILogger<ContactsController> _logger;
        private ContactMetrics _metrics;
        private UserMetrics _user_metrics;
        private readonly IContactTraces _traces; 
        public ContactsController(ILogger<ContactsController> logger, ContactMetrics metrics, UserMetrics userMetrics, ContactsAppDbContext context, Tracer tracer, IContactTraces contactTraces)
        {
            _context = context;
            _logger = logger;
            _metrics = metrics;
            _traces = contactTraces;
            _user_metrics = userMetrics;
        }

        // GET: api/Contacts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
        {  
            if (_context.Contacts == null)
          {
              return NotFound();
          }
            return await _context.Contacts.ToListAsync();
        }


        // GET: api/Contacts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetContact(int id)
        {
          if (_context.Contacts == null)
          {
              return NotFound();
          }
            var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

            return contact;
        }

        // PUT: api/Contacts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContact(int id, Contact contact)
        { 
            if (id != contact.Id)
            {
                return BadRequest();
            }

            _context.Entry(contact).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Contacts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Contact>> PostContact(Contact contact)
        {
            using (Activity activity = _traces.CreateActivity().StartActivity("{title} Activity"))
            {
                var watch = new Stopwatch();
                watch.Start();

                activity?.SetTag("title", "start watch");

                using (Activity child1 = _traces.CreateActivity().StartActivity("{title} Activity"))
                {
                    child1?.SetTag("title", "GET Contacts");

                    child1?.AddEvent(new ActivityEvent("Get contacts count"));
                    int contacts = _context.Contacts.Count();

                    child1?.AddEvent(new ActivityEvent("Calculate Metric"));
                    _metrics.ToTalContactUpdate(contacts + 1);
                    _logger.LogInformation("INCRIMENT Counter" + _metrics.TotalContact());

                    if (_context.Contacts == null)
                    {
                        child1?.SetStatus(ActivityStatusCode.Error, "Something bad happened!");
                        child1?.RecordException(new Exception("Entity set 'ContactsAppDbContext.Contacts'  is null."));
                        return Problem("Entity set 'ContactsAppDbContext.Contacts'  is null.");
                    }
                    using (Activity child2 = _traces.CreateActivity().StartActivity("{title} Activity"))
                    {
                        child2?.SetTag("title", "Create contact");

                        child2?.AddEvent(new ActivityEvent("Add contact to database"));
                        _context.Contacts.Add(contact);
                        child2?.AddEvent(new ActivityEvent("Save changes"));
                        await _context.SaveChangesAsync();

                        _user_metrics.RecordNumberOfContactsPerUser(_context.Contacts.Where(c=>c.userId==contact.userId).Count());

                        watch.Stop();
                        long responseTimeForCompleteRequest = watch.ElapsedMilliseconds;

                        _metrics.RecordContactCreationProcess(responseTimeForCompleteRequest);
                        _logger.LogInformation("Recorded Contact Creation Time" + _metrics.RecordedContactCreationProcess());
                    }
                }
            }

            return CreatedAtAction("GetContact", new { id = contact.Id }, contact);
        }

        //[HttpPost]
        //public async Task<IList<Contact>> PostContacts(IList<Contact> cts)
        //{
        //    var watch = new Stopwatch();
        //    watch.Start();

        //    foreach (var contact in cts)
        //    {
        //        int contacts = _context.Contacts.Count();

        //        _metrics.ToTalContactUpdate(contacts + 1);
        //        _logger.LogInformation("INCRIMENT Counter" + _metrics.TotalContact());

        //        _context.Contacts.Add(contact);
        //        await _context.SaveChangesAsync(); 
        //    }
        //    watch.Stop();
        //    long responseTimeForCompleteRequest = watch.ElapsedMilliseconds;

        //    _metrics.RecordContactCreationProcess(responseTimeForCompleteRequest);
        //    _logger.LogInformation("Recorded Contact Creation Time" + _metrics.RecordedContactCreationProcess());

        //    return await _context.Contacts.ToListAsync();
        //}

        // DELETE: api/Contacts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            _metrics.ToTalContactUpdate(-1);
            if (_context.Contacts == null)
            {
                return NotFound();
            }
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContactExists(int id)
        {
            return (_context.Contacts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
