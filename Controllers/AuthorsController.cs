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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempted Get All Authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Successfully got all Authors");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }
        /// <summary>
        /// Get a single Author
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Author</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted Get Author {id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"Author with id {id} was not found");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo($"Successully got Author {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

#pragma warning disable CS1572 // XML comment has a param tag, but there is no parameter by that name
        /// <summary>
        /// Creates an Author
        /// </summary>
        /// <param name="author"></param>
        /// <returns>new Author</returns>
        [HttpPost]
#pragma warning restore CS1572 // XML comment has a param tag, but there is no parameter by that name
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Attempted to Create Author");
                if (authorDTO == null)
                {
                    _logger.LogWarn($"Empty request was submitted for Create Author");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Data incomplete for Create Author");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);
                if (!isSuccess)
                {
                    return InternalError($"Author Create Failed");
                }

                _logger.LogInfo($"Successfully created Author");
                return Created($"Author created", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }

        }
        /// <summary>
        /// Update an Author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo($"Attempted to Update Author");
                if (id < 1 || authorDTO == null || id != authorDTO.Id)
                {
                    _logger.LogWarn($"Empty request was submitted for Update Author {id}");
                    return BadRequest();
                }
                var isExists = await _authorRepository.isExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"Author Not Found {id}");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"Data incomplete for Update Author {id}");
                    return BadRequest(ModelState);
                }
               

                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);
                if (!isSuccess)
                {
                   return InternalError($"Author {id} Update Failed");
                }

                _logger.LogInfo($"Successfully updated Author {id}");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Delete an Author
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted to Delete Author");
                if(id < 1)
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
                    return InternalError($"Author {id} delete Failed");
                }

                _logger.LogInfo($"Successfully deleted Author {id}");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }


        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Contact the Admin.");
        }

    }

}
