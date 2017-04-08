﻿namespace CarRepairReport.Managers.Interfaces
{
    using System.Threading.Tasks;
    using System.Web;
    using CarRepairReport.Models.BindingModels;
    using CarRepairReport.Models.Models;
    using CarRepairReport.Models.ViewModels;

    public interface IMyUserManager
    {
        Task CreateMyUserAsync(ApplicationUser appUser, HttpContextBase httpContext);
        //void CreateMyUserAsync(ApplicationUser appUser, HttpContextBase httpContext);
        UserProfileVm GetUserProfileByAppUserId(string appUserId);
        bool AddUserDetails(UserProfileBm bm, string appUserId);
        EditUserVm GetEditModelByAppId(string appUserId);

        bool EditUserPersonalDetails(EditUserBm bm, string appUserId);
    }
}