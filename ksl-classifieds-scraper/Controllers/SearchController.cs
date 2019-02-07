using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ksl_classifieds_scraper.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        // GET api/search/nintendo+switch
        [HttpGet("{query}")]
        public async Task<string> GetAsync(string query)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = $"https://classifieds.ksl.com/search?category[]=&subCategory[]=&keyword={query}&priceFrom=&priceTo=&zip=&miles=25&sellerType[]=Private&marketType[]=Sale&hasPhotos[]=Has%20Photos&postedTimeFQ[]=&perPage=1000";
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(address);

            //Grab items array from script tag and delete surround JS method text to serialize into JSON object
            var itemsScript = document.QuerySelectorAll("*").FirstOrDefault(m => m.LocalName == "script" && m.InnerHtml.Contains("window.renderSearchSection("));
            var itemsJson = itemsScript.InnerHtml.Replace("window.renderSearchSection(", "");
            var items = itemsJson.Replace("kslBaseDomain: 'ksl.com',\n                })", "}");
            var json = JObject.Parse(items);

            //Determine whether there are more pages. Ksl only grabs a max of 96 items at a time.
            var pagination = document.QuerySelectorAll("*").FirstOrDefault(m => m.ClassName == "pagination").Children.Where(m => m.ClassName == "link");


            foreach(var page in pagination)
            {
                var href = page.GetAttribute("href");
                var additionalPage = await context.OpenAsync(address);
                var additionalPageScript = document.QuerySelectorAll("*").FirstOrDefault(m => m.LocalName == "script" && m.InnerHtml.Contains("window.renderSearchSection("));
                var additionalJson = itemsScript.InnerHtml.Replace("window.renderSearchSection(", "");
                var additionalItems = itemsJson.Replace("kslBaseDomain: 'ksl.com',\n                })", "}");
                var additionalItemJson = JObject.Parse(additionalItems);
                json.Merge(additionalItemJson);
            }

            return json.ToString();

        }
    }
}
