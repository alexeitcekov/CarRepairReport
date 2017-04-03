﻿namespace CarRepairReport.Managers
{
    using System;
    using System.Threading.Tasks;
    using System.Web;
    using AutoMapper;
    using CarRepairReport.Managers.Interfaces;
    using CarRepairReport.Models.Models;
    using CarRepairReport.Models.ViewModels;
    using CarRepairReport.Services.Interfaces;

    public class MyUserManager : IMyUserManager
    {
        private ILanguageManager langManager;
        private IUserService userService;

        public MyUserManager(ILanguageManager langManager, IUserService userService)
        {
            this.langManager = langManager;
            this.userService = userService;
        }
        public Task CreateMyUserAsync(ApplicationUser appUser, HttpContextBase httpContext)
        {
            return Task.Run(() =>
            {
                var lang = this.langManager.GetCurrentLang(httpContext);

                if (lang == null)
                {
                    lang = new Language()
                    {
                        CreatedOn = DateTime.UtcNow,
                        Name = "English",
                        TwoLetterCode = "en"
                    };
                }

                var language = new Language()
                {
                    Name = lang.Name,
                    TwoLetterCode = lang.TwoLetterCode
                };

                var userSetting = new UserSetting()
                {
                    Language = language,
                    LanguageId = language.Id
                };

                var myUser = new User()
                {
                    Id = Guid.NewGuid().ToString(),
                    UserSetting = userSetting,
                    //ApplicationUser = appUser,
                    ApplicationUserId = appUser.Id,
                    Birthday = new DateTime(1901, 1, 1)
                };

                this.userService.Add(myUser);
            });
        }

        public UserProfileVm GetUserProfileById(string userId)
        {
            var user = this.userService.GetUserById(userId);

            if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName))
            {
                return null;
            }

            var vm = Mapper.Map<User, UserProfileVm>(user);

            return vm;
        }
    }
}