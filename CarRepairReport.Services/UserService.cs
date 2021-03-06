﻿namespace CarRepairReport.Services
{
    using System;
    using System.Collections.Generic;
    using CarRepairReport.Data;
    using CarRepairReport.Models.Models;
    using CarRepairReport.Models.Models.UserModels;
    using CarRepairReport.Services.Interfaces;

    public class UserService : Service, IUserService
    {
        public UserService(ICarRepairReportData context) : base(context)
        {
        }

        public bool Add(User user)
        {
            this.context.MyUsers.Add(user);

            this.context.Commit();

            return true;
        }

        public User GetUserById(string userId)
        {
            var user = this.context.MyUsers.FirstOrDefault(x => x.Id == userId);

            return user;
        }

        public bool Update(User user)
        {
            var entity = this.context.MyUsers.FirstOrDefault(x => x.Id == user.Id);

            if (entity == null)
            {
                return false;
            }

            entity.FirstName = user.FirstName;
            entity.LastName = user.LastName;
            //entity.Birthday = user.Birthday;
            entity.ImageUrl = entity.ImageUrl;

            try
            {
                this.context.MyUsers.Update(entity);

                this.context.Commit();
            }
            catch (Exception ex)
            {
                return this.LogError(ex);
            }

            return true;
        }

        public User GetUserByAppId(string appUserId)
        {
            return this.context.MyUsers.FirstOrDefault(x => x.ApplicationUserId == appUserId);
        }

        public bool IsUserExists(string appUserId)
        {
            return this.context.MyUsers.Any(x => x.ApplicationUserId == appUserId);
        }

        public string GetUserImgUrl(string appUserId)
        {
            var user = this.context.MyUsers.FirstOrDefault(x => x.ApplicationUserId == appUserId);

            if (user == null)
            {
                return null;
            }

            return user.ImageUrl;
        }

        public bool UpdatePersonalInfo(string firstName, string lastName,string imageUrl, string appUserId)
        {
            var entity = this.context.MyUsers.FirstOrDefault(x => x.ApplicationUserId == appUserId);

            if (entity == null)
            {
                return false;
            }

            entity.FirstName = firstName;
            entity.LastName = lastName;

            if (!string.IsNullOrEmpty(imageUrl))
            {
                entity.ImageUrl = imageUrl;
            }

            try
            {
                this.context.MyUsers.Update(entity);
                this.context.Commit();
            }
            catch (Exception ex)
            {
                return this.LogError(ex);
            }
            
            return true;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return this.context.MyUsers.All();
        }
    }
}
