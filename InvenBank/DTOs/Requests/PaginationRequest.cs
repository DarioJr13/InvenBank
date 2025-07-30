﻿namespace InvenBank.API.DTOs.Requests
{
    public class PaginationRequest
    {
        private int _pageSize = 20;
        private const int MaxPageSize = 100;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
    }
}
