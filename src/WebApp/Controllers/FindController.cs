using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Authorize(Policy = Constants.Policy.Read)]
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    [ApiController]
    public class FindController : ControllerBase
    {
        private readonly IWordService _wordService;

        public FindController(IWordService wordService)
        {
            _wordService = wordService;
        }

        /// <summary>
        /// Finds words contianing the input characters
        /// </summary>
        /// <param name="characters"></param>
        /// <returns>List of found words</returns>
        [HttpGet]
        public IActionResult Find([FromQuery]string characters)
        {
            var words = _wordService.Find(characters);
            return Ok(words);
        }
    }
}