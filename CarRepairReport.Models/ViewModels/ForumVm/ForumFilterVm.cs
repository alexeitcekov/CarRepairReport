﻿namespace CarRepairReport.Models.ViewModels.ForumVm
{
    using System.ComponentModel.DataAnnotations;

    public class ForumFilterVm
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
    }
}
