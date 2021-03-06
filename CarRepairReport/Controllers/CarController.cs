﻿namespace CarRepairReport.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using CarRepairReport.Globals;
    using CarRepairReport.Managers.Interfaces;
    using CarRepairReport.Models.BindingModels;
    using CarRepairReport.Models.Enums;
    using CarRepairReport.Models.ViewModels.CarVms;
    using Microsoft.AspNet.Identity;

    [Authorize]
    [RoutePrefix("Cars")]
    public class CarController : BaseController
    {
        private ICarManager carManager;

        public CarController(ICarManager carManager, IMyUserManager myUserManager, ILanguageManager languageManager) : base(myUserManager,languageManager)
        {
            this.carManager = carManager;
        }

        [HttpGet]
        [Route("Add")]
        public ActionResult Add()
        {
            var vm = new CreateCarVm();
            
            var fuelTypes = Enum.GetValues(typeof(FuelType)).Cast<FuelType>();

            foreach (var fuelType in fuelTypes)
            {
                vm.FuelTypeValues.Add((int)fuelType, fuelType.ToString());
            }

            var gearboxTypes = Enum.GetValues(typeof(GearBoxType)).Cast<GearBoxType>();

            foreach (var gearboxType in gearboxTypes)
            {
                vm.GearBoxValues.Add((int)gearboxType, gearboxType.ToString());
            }


            vm.LanguageCode = this.CurrentLanguageCode;
            return View(vm);
        }

        [HttpPost]
        [Route("Add")]
        public ActionResult Add(CreateCarBm bm)
        {
            if (!this.ModelState.IsValid)
            {
                // return model with this data
            }

            var appUserId = this.User.Identity.GetUserId();

            bool isCreated = this.carManager.CreateCar(bm, appUserId);

            if (!isCreated)
            {
                // error page
            }

            return this.RedirectToAction("UserProfile", "User");
        }

        [HttpGet]
        [Route("Edit")]
        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [Route("Edit")]
        public ActionResult Edit(int a)
        {
            return View();
        }

        [HttpGet]
        [Route("Remove")]
        public ActionResult Remove(int carId)
        {
            var appUserId = this.User.Identity.GetUserId();

            SimpleCarVm vm = this.carManager.GetSimpleVm(appUserId, carId);

            if (vm == null)
            {
                // error page
            }

            vm.LanguageCode = this.CurrentLanguageCode;
            return this.View(vm);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Route("Remove")]
        public ActionResult Remove(int id, string model)
        {
            var appUserId = this.User.Identity.GetUserId();

            bool isRemoved = this.carManager.RemoveCarFromUser(appUserId, id);

            if (!isRemoved)
            {
                // error page
            }

            return this.RedirectToAction("UserProfile", "User");
        }

        [HttpGet]
        [AllowAnonymous]
        [ChildActionOnly]
        [Route("lastserviced")]
        //[OutputCache(Duration = 60 * 5)]
        public ActionResult GetLastServicedParts()
        {
            var vms = this.carManager.LastServicedCarParts(this.CurrentLanguageCode);

            

            return this.PartialView(vms);
        }
    }
}