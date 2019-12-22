using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ksl_classifieds_scraper.Helpers.StringHelpers;

namespace ksl_classifieds_scraper.Controllers
{
    [Route("api/[controller]")]
    public class ListingController : Controller
    {
        // GET api/search/nintendo+switch
        [HttpGet("{id}")]
        public async Task<string> GetAsync(string id)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = $"https://classifieds.ksl.com/listing/{id}";
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(address);

            //Grab items array from script tag and delete surround JS method text to serialize into JSON object
            var itemsScript = document.QuerySelectorAll("*").FirstOrDefault(m => m.LocalName == "script" && m.InnerHtml.Contains("window.detailPage = window.detailPage"));
            var itemsJson = StringHelpers.getBetween(itemsScript.InnerHtml,"window.detailPage.listingData = ",";");
            var json = JObject.Parse(itemsJson);

            return json.ToString();

        }
    }
}
