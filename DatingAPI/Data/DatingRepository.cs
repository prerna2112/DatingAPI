using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingAPI.Helpers;
using DatingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingAPI.Data
{
    public class DatingRepository : IDatingRepository
    {
        private DataContext _dataContext;
        public DatingRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public void Add<T>(T entity) where T : class
        {
            _dataContext.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _dataContext.Remove(entity);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _dataContext.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }
        public async Task<Photo> GetMainPhotoForUser(int UserId)
        {
            var photo = await _dataContext.Photos.Where(u => u.UserId == UserId).FirstOrDefaultAsync(p=> p.IsMain);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user =  await _dataContext.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users =  _dataContext.Users.Include(p => p.Photos).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId && u.Gender == userParams.Gender );
            users = users.Where(u => u.DateOFBirth.Age() >= userParams.MinAge && u.DateOFBirth.Age() <= userParams.MaxAge);
            if(!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            else
            {
                users = users.OrderByDescending(u => u.LastActive);
            }
            return await PagedList<User>.CreateAsync(users,userParams.PageNumber,userParams.pageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }
    }
}
