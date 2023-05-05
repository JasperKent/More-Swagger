using Microsoft.AspNetCore.Mvc;
using Swagger.DataAccess.Entities;
using Swagger.DataAccess.Repositories;

namespace Swagger.Controllers.V1
{
    [Route("[controller]")]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0", Deprecated = true)]
    [ApiController]
    [Obsolete]
    public class BookReviewsController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;

        private static bool ValidateReview(BookReview bookReview)
        {
            return !string.IsNullOrWhiteSpace(bookReview.Title)
                && bookReview.Rating >= 1
                && bookReview.Rating <= 5;
        }

        public BookReviewsController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookReview>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<BookReview>> Get()
        {
            return Ok(_reviewRepository.AllReviews);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookReview))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<BookReview> Get(int id)
        {
            var result = _reviewRepository.AllReviews.SingleOrDefault(r => r.Id == id);

            if (result == null)
                return NotFound();
            else
                return Ok(result);
        }

        /// <summary>
        /// Provides a summary of all book reviews
        /// </summary>
        /// <returns>A collection of all the books, with the average rating for each</returns>
        [HttpGet("summary")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookReview>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<BookReview>> Summary()
        {
            var summaries = _reviewRepository.AllReviews.GroupBy(r => r.Title).Select(g =>
                new BookReview
                {
                    Title = g.Key,
                    Rating = Math.Round(g.Average(r => r.Rating), 2)
                });

            return Ok(summaries);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<int> Post([FromBody] BookReview review)
        {
            if (!ValidateReview(review))
                return UnprocessableEntity();

            _reviewRepository.Create(review);
            _reviewRepository.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = review.Id }, review.Id);
        }

        /// <summary>
        /// Updates a single book review
        /// </summary>
        /// <param name="id">The id of the review</param>
        /// <param name="review">The updated review</param>
        /// <returns>The HTTP status code</returns>
        /// <response code="200">The update was sucessful</response>
        /// <response code="404">The reivew was not found</response>
        /// <response code="400">The new review was not in the correct format</response>
        /// <response code="422">The new rating was out of range</response>
        /// <response code="500">An exception was thrown</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Put(int id, [FromBody] BookReview review)
        {
            if (!ValidateReview(review))
                return UnprocessableEntity();

            var result = _reviewRepository.AllReviews.SingleOrDefault(r => r.Id == id);

            if (result == null)
                return NotFound();
            else
            {
                result.Rating = review.Rating;
                result.Title = review.Title;

                _reviewRepository.SaveChanges();

                return Ok();
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Delete(int id)
        {
            var result = _reviewRepository.AllReviews.SingleOrDefault(r => r.Id == id);

            if (result == null)
                return NotFound();
            else
            {
                _reviewRepository.Remove(result);
                _reviewRepository.SaveChanges();

                return Ok();
            }
        }
    }
}
