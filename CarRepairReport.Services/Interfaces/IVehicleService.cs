﻿namespace CarRepairReport.Services.Interfaces
{
    using System.Collections.Generic;
    using CarRepairReport.Models.Models.UserModels;

    public interface IVehicleServiceService
    {
        IEnumerable<VehicleService> GetAllVehicleServices();
        bool AddVehicleService(VehicleService vehicleService);
        bool IsServiceNameUnique(string serviceName);
    }
}
