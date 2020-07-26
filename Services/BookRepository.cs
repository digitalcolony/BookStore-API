using BookStore_API.Contracts;
using BookStore_API.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace BookStore_API.Services
{
    public class BookRepository : IBookRepository
    {

        private readonly ApplicationDbContext _db;

        public BookRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Create(Book entity)
        {
            await _db.Books.AddAsync(entity);
            return await Save();
        }

        public Task<bool> Delete(Book entity)
        {
            _db.Books.Remove(entity);
            return Save();
        }

        public async Task<IList<Book>> FindAll()
        {
            var books = await _db.Books.ToListAsync();
            return books;
        }

        public async Task<Book> FindById(int id)
        {
            var book = await _db.Books.FindAsync(id);
            return book;
        }

        public async Task<bool> isExists(int id)
        {
            return await _db.Books.AnyAsync(q => q.Id == id);
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(Book entity)
        {
            _db.Books.Update(entity);
            return await Save();
        }
    }
}
