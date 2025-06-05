//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace APIEvents.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class EventsController : ControllerBase
//    {
//    }
//}
using APIEvents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // למזג אוויר עם cache
using System.Net.Http;

namespace APIEvents.Controllers
{
    [Route("event")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly EventsContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;

        public EventsController(EventsContext context, IMemoryCache memoryCache, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
        }

        // POST: /event (צור אירוע חדש)
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event ev)
        {
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvent), new { id = ev.Id }, ev);
        }

        // GET: /event/{id} (שלוף אירוע לפי מזהה)
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var ev = await _context.Events
                .Include(e => e.EventUsers)
                .ThenInclude(eu => eu.UserRefNavigation)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            return ev;
        }

        // PUT: /event/{id} (עדכן אירוע)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, Event ev)
        {
            if (id != ev.Id)
                return BadRequest();

            _context.Entry(ev).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Events.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: /event/{id} (מחק אירוע)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: /event/{id}/registration (שלוף את כל המשתמשים שנרשמו לאירוע)
        [HttpGet("{id}/registration")]
        public async Task<ActionResult<IEnumerable<User>>> GetEventUsers(int id)
        {
            var users = await _context.EventUsers
                .Where(eu => eu.EventRef == id)
                .Select(eu => eu.UserRefNavigation)
                .ToListAsync();

            return users;
        }

        // POST: /event/{id}/registration (רשום משתמש לאירוע)
        [HttpPost("{id}/registration")]
        public async Task<IActionResult> RegisterUser(int id, [FromBody] int userId)
        {
            // בדיקה אם האירוע קיים
            var ev = await _context.Events
                .Include(e => e.EventUsers)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound("Event not found.");

            // בדיקה אם המשתמש קיים
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            // בדיקה אם המשתמש כבר רשום
            if (ev.EventUsers.Any(eu => eu.UserRef == userId))
                return BadRequest("User already registered.");

            // בדיקה אם עברת מגבלת משתתפים
            if (ev.EventUsers.Count >= ev.MaxRegistrations)
                return BadRequest("Event is full.");

            // יצירת הרשמה
            var registration = new EventUser
            {
                EventRef = id,
                UserRef = userId,
                Creation = DateTime.Now
            };
            _context.EventUsers.Add(registration);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

        // GET: /schedule (שלוף את כל האירועים)
        [HttpGet("/schedule")]
        public async Task<ActionResult<IEnumerable<Event>>> GetSchedule()
        {
            return await _context.Events.ToListAsync();
        }

        // GET: /event/{id}/weather (שלוף תחזית מזג אוויר עם caching)
        [HttpGet("{id}/weather")]
        public async Task<IActionResult> GetWeather(int id)
        {
            // מצא את האירוע
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound();

            string cacheKey = $"weather_{id}";
            if (!_memoryCache.TryGetValue(cacheKey, out string weather))
            {
                // כאן שים את הקריאה ל-API החיצוני שאתה בוחר (דוגמה עם open-meteo)
                var httpClient = _httpClientFactory.CreateClient();
                // נניח שאתה רוצה תחזית לפי עיר, כאן תצטרך להתאים את הקוד שלך:
                var response = await httpClient.GetAsync("https://api.open-meteo.com/v1/forecast?latitude=32.1&longitude=34.8&hourly=temperature_2m");
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode);

                weather = await response.Content.ReadAsStringAsync();

                // שמור ב-Cache ל-5 דקות
                _memoryCache.Set(cacheKey, weather, TimeSpan.FromMinutes(5));
            }

            return Ok(weather);
        }
    }
}
