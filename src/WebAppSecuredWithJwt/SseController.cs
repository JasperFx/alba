using System.Linq;
using System.Net.ServerSentEvents;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi
{
    public class SseController : ControllerBase
    {
        [Authorize]
        [HttpGet("/sse/secured")]
        public IResult GetEvents()
        {
            var foo = User.FindFirstValue("foo") ?? "none";
            var items = Enumerable.Range(1, 2)
                .Select(i => new SseItem<string>($"{foo}-{i}") { EventId = i.ToString() })
                .ToAsyncEnumerable();

            return TypedResults.ServerSentEvents(items);
        }
    }
}
