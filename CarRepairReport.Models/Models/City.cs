﻿namespace CarRepairReport.Models.Models
{
    public class City : BaseModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public int CountryId { get; set; }
        public virtual Country Country { get; set; }
    }
}