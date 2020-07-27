using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NLog;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoing used to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempted call");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo($"{location}: Successful");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }
        /// <summary>
        /// Get a single Author
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Author</returns>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempted for {id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"{location} for id {id} was not found");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"{location} : Successful for {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns>new Author</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempted call");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"{location}: Empty request was submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data incomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Failed");
                }

                _logger.LogInfo($"{location}: Successfully created");
                return Created($"Author created", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }

        }
        /// <summary>
        /// Update an Author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempted call");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn($"{location}: Empty request was submitted for {id}");
                    return BadRequest();
                }
                var isExists = await _authorRepository.isExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"{location}: Not Found for {id}");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data incomplete for { id}");

                    return BadRequest(ModelState);
                }
               
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);
                if (!isSuccess)
                {
                   return InternalError($"{location}: Update Failed for {id}");
                }
                _logger.LogInfo($"{location}: Successfully updated for { id}");

                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Delete an Author
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();

            try
            {
                _logger.LogInfo($"{location}: Attempted call");
                if (id < 1)
                {
                    return BadRequest();
                }
                var author = await _authorRepository.FindById(id);
                if(author == null)
                {
                    return NotFound();
                }
                var isSuccess = await _authorRepository.Delete(author);
                if (!isSuccess)
                {
                    return InternalError($"{location}: Failed for {id}");
                }

                _logger.LogInfo($"{location}: Successful for { id}");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Contact the Admin.");
        }

    }

}
