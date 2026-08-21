using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SongList.Models.ViewModels;
using Index.Service;
using Details.Service;
using Terms.Service;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace SongList.Controllers
{
    public class HomeController : Controller
    {
        private readonly IndexService _IndexService;
        private readonly DetailsService _DetailsService;
        private readonly TermsService _TermsService;
        private readonly IMemoryCache _Cache;
        private sealed class DetailsCondition
        {
            public long SongId { get; init; }
            public string MemberCode  { get; init; } = string.Empty;
        }
        public HomeController(IndexService indexService, DetailsService detailsService, TermsService termsService, IMemoryCache chche)
        {
            _IndexService = indexService;
            _DetailsService = detailsService;
            _TermsService = termsService;
            _Cache = chche;
        }

        [HttpGet]
        [EnableRateLimiting("ip-rate-limit")]
        public IActionResult Index(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                var initialViewModel = _IndexService.CreateIndexViewModel(new SearchCondition(), search: false);
                return View(initialViewModel);
            }
            if (!_Cache.TryGetValue($"index:{token}", out SearchCondition? condition) || condition is null)
            {
                return NotFound();
            }
            var viewmodel = _IndexService.CreateIndexViewModel(condition, search: true);
            return View(viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("ip-rate-limit")]
        public IActionResult Index([Bind(Prefix = "searchCondition")] SearchCondition searchCondition)
        {
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            _Cache.Set($"index:{token}",searchCondition, TimeSpan.FromMinutes(10));
            return RedirectToAction(nameof(Index), new {token});
        }

        [HttpGet]
        [EnableRateLimiting("ip-rate-limit")]
        public IActionResult Details(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || !_Cache.TryGetValue($"detail:{token}", out DetailsCondition? condition) || condition is null)
            {
                return RedirectToAction(nameof(Index));
            }
            var viewmodel = _DetailsService.CreateDetailsViewModel(condition.SongId, condition.MemberCode);

            // ViewにViewModelを渡す
            return View(viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("ip-rate-limit")]
        public IActionResult Details(long songId, string memberCode)
        {
            if (songId <= 0 || string.IsNullOrWhiteSpace(memberCode))
            {
                return BadRequest();
            }
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            _Cache.Set($"detail:{token}", new DetailsCondition
            {
                SongId = songId,
                MemberCode = memberCode
            }
            , TimeSpan.FromMinutes(10));

            return RedirectToAction(nameof(Details), new { token });
        }

        [HttpGet]
        [EnableRateLimiting("ip-rate-limit")]
        public IActionResult Terms(string kind)
        {
            var viewmodel = _TermsService.CreateTermsViewModel(kind);
            // ViewにViewModelを渡す
            return View(viewmodel);
        } 
   }
}