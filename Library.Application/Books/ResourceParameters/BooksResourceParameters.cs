﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Application.Books.ResourceParameters
{
    public class BooksResourceParameters
    {
        const int maxPageSize = 20;
        public string Title { get; set; }
        public string SearchQuery { get; set; }

        public int PageNumber { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public string OrderBy { get; set; } = "Title";
    }
}
