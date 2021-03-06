﻿using Library.Application.Authors.Models;
using Library.Application.Authors.ResourceParameters;
using Library.Application.Common.Helpers;
using Library.Application.Common.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Common.Helpers;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Infrastructure.Repositories
{
    public class AuthorRepository : Repository<Author>, IAuthorRepository
    {
        public AuthorRepository(ApplicationDbContext context, IPropertyMappingService propertyMappingService) 
            : base(context, propertyMappingService)
        {
        }

        public ApplicationDbContext ApplicationDbContext
        {
            get { return _context as ApplicationDbContext; }
        }

        public async Task<Author> GetAnAuthorByABook(Guid authorId, Guid? bookId)
        {
            if (authorId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(authorId));
            }

            if (bookId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(bookId));
            }

            var bookAuthor = ApplicationDbContext.BookAuthors.FirstOrDefault(b => b.AuthorId == authorId && b.BookId == bookId);

            return await ApplicationDbContext.Authors
                            .Include(ba => ba.BookAuthors)
                                .ThenInclude(b => b.Book)
                            .FirstOrDefaultAsync(a => a.AuthorId == bookAuthor.AuthorId);
        }

        public async Task<IEnumerable<Author>> GetAuthors()
        {
            return await ApplicationDbContext.Authors
                            .Include(a => a.BookAuthors)
                        .ThenInclude(ba => ba.Book).ToListAsync();
        }

        public async Task<IEnumerable<Author>> GetAuthors(IEnumerable<Guid> authorIds)
        {
            if (authorIds == null)
            {
                throw new ArgumentNullException(nameof(authorIds));
            }

            return await ApplicationDbContext.Authors
                            .Where(a => authorIds.Contains(a.AuthorId))
                                .Include(a => a.BookAuthors)
                                    .ThenInclude(ba => ba.Book)
                            .OrderBy(a => a.FirstName)
                            .OrderBy(a => a.LastName)
                                .ToListAsync();
        }

        public PagedList<Author> GetAuthors(AuthorsResourceParameters authorsResourceParameters)
        {
            if (authorsResourceParameters == null)
            {
                throw new ArgumentNullException(nameof(authorsResourceParameters));
            }

            var collectionOfAuthors = ApplicationDbContext.Authors
                                                .Include(ba => ba.BookAuthors)
                                                    .ThenInclude(b => b.Book) as IQueryable<Author>;

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.Name))
            {
                var name = authorsResourceParameters.Name.Trim();
                collectionOfAuthors = collectionOfAuthors.Where(author => author.FirstName == name || author.LastName == name);
            }

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.SearchQuery))
            {
                var searchQuery = authorsResourceParameters.SearchQuery.Trim();
                collectionOfAuthors = collectionOfAuthors.Where(author =>
                    author.FirstName.Contains(searchQuery) ||
                    author.LastName.Contains(searchQuery)
                );
            }

            if (!string.IsNullOrWhiteSpace(authorsResourceParameters.OrderBy))
            {
                var authorPropertyMappingDictionary =
                    _propertyMappingService.GetPropertyMapping<AuthorDto, Author>();

                collectionOfAuthors = collectionOfAuthors.ApplySort(authorsResourceParameters.OrderBy,
                    authorPropertyMappingDictionary);
            }

            return PagedList<Author>.Create(collectionOfAuthors,
                        authorsResourceParameters.PageNumber,
                        authorsResourceParameters.PageSize);
        }


    }
}
