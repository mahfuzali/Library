﻿using AutoMapper;
using Library.Application.Books.ResourceParameters;
using Library.Application.Dtos.Models;
using Library.Domain.Entities;
using Library.Infrastructure.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Library.API.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly ILibraryRepository _libraryRepository;
        private readonly IMapper _mapper;

        public BooksController(ILibraryRepository libraryRepository, IMapper mapper)
        {
            _libraryRepository = libraryRepository ??
                throw new ArgumentNullException(nameof(libraryRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet()]
        [HttpHead]
        public IActionResult GetBooks(
            [FromQuery] BooksResourceParameters booksResourceParameters)
        {
            var booksFromRepo = _libraryRepository.GetBooks(booksResourceParameters);
            return Ok(_mapper.Map<IEnumerable<BookDto>>(booksFromRepo));
        }

        [HttpGet("{bookId}", Name = "GetBook")]
        public IActionResult GetBook(Guid bookId)
        {
            var bookFromRepo = _libraryRepository.GetBook(bookId);

            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<BookDto>(bookFromRepo));
        }

        [HttpPost]
        public ActionResult<BookDto> CreateBook(BookForCreationDto book)
        {
            var bookEntity = _mapper.Map<Book>(book);
                        
            _libraryRepository.AddBook(bookEntity);

            foreach (var author in book.Authors)
            { 
                var authorEntity = _mapper.Map<Author>(author);
                _libraryRepository.AddAuthor(authorEntity);
                //_libraryRepository.Save();

                _libraryRepository.AddBookAuthor(bookEntity, authorEntity);
            }

            _libraryRepository.Save();

            var bookToReturn = _mapper.Map<BookDto>(bookEntity);
            return CreatedAtRoute("GetBook",
                new { bookId = bookToReturn.BookId },
                bookToReturn);
        }

        [HttpPut("{bookId}")]
        public IActionResult UpdateBook(Guid bookId, BookForUpdateDto book)
        {
            if (!_libraryRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var bookEntity = _libraryRepository.GetBook(bookId);

            if (bookEntity == null)
            { 
                var bookToAdd = _mapper.Map<Book>(book);

                bookToAdd.BookId = bookId;

                _libraryRepository.AddBook(bookToAdd);

                foreach (var author in book.Authors)
                {
                    var authorEntity = _mapper.Map<Author>(author);
                    _libraryRepository.AddAuthor(authorEntity);

                    _libraryRepository.AddBookAuthor(bookToAdd, authorEntity);
                }

                _libraryRepository.Save();

                var bookToReturn = _mapper.Map<BookDto>(bookToAdd);
                return CreatedAtRoute("GetBook",
                    new { bookId = bookToReturn.BookId },
                    bookToReturn);
            }

            _mapper.Map(book, bookEntity);
            _libraryRepository.UpdateBook(bookEntity);
            
            _libraryRepository.Save();
            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        /*
        [HttpPatch("{bookId}")]
        public ActionResult PartiallyUpdateBook(Guid authorId,
            Guid bookId,
            JsonPatchDocument<BookForUpdateDto> patchDocument)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookForAuthorFromRepo = _libraryRepository.GetBook(authorId, bookId);

            if (bookForAuthorFromRepo == null)
            {
                var bookDto = new BookForUpdateDto();
                patchDocument.ApplyTo(bookDto);

                if (!TryValidateModel(bookDto))
                {
                    return ValidationProblem(ModelState);
                }

                var bookToAdd = _mapper.Map<Book>(bookDto);
                bookToAdd.BookId = bookId;

                _libraryRepository.AddBook(authorId, bookToAdd);
                _libraryRepository.Save();

                var bookToReturn = _mapper.Map<BookDto>(bookToAdd);

                return CreatedAtRoute("GetBook",
                    new { bookId = bookToReturn.BookId },
                    bookToReturn);
            }

            var bookToPatch = _mapper.Map<BookForUpdateDto>(bookForAuthorFromRepo);
            // add validation
            patchDocument.ApplyTo(bookToPatch);

            if (!TryValidateModel(bookToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(bookToPatch, bookForAuthorFromRepo);

            _libraryRepository.UpdateBook(bookForAuthorFromRepo);

            _libraryRepository.Save();

            return NoContent();
        } 

        public override ActionResult ValidationProblem(
        [ActionResultObjectValue] ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices
                .GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }
        */

        [HttpDelete("{bookId}")]
        public ActionResult DeleteBook(Guid bookId)
        {
            if (!_libraryRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var bookFromRepo = _libraryRepository.GetBook(bookId);

            if (bookFromRepo == null)
            {
                return NotFound();
            }

            _libraryRepository.DeleteBook(bookFromRepo);
            _libraryRepository.Save();

            return NoContent();
        }
    }
}