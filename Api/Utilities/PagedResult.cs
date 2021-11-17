﻿namespace Api.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class PagedResult<T>
{
    public int CurrentPage { get; set; } 
    public int PageCount { get; set; } 
    public int PageSize { get; set; } 
    public int RowCount { get; set; }

    public int FirstRowOnPage => (CurrentPage - 1) * PageSize + 1;
    public int LastRowOnPage => Math.Min(CurrentPage * PageSize, RowCount);
        
    public List<T> Results { get; set; }

    public PagedResult()
    {
        Results = new List<T>();
    }
}

public static class PagedResultUtils
{
    public static async Task<PagedResult<T>> GetPagedAsync<T>(this IQueryable<T> query, int page, int pageSize) where T : class
    {
        var result = new PagedResult<T>
        {
            CurrentPage = page,
            PageSize = pageSize,
            RowCount = await query.CountAsync()
        };
            
        var pageCount = (double) result.RowCount / pageSize;
        result.PageCount = (int) Math.Ceiling(pageCount);
 
        var skip = (page - 1) * pageSize;     
        result.Results = await query.Skip(skip).Take(pageSize).ToListAsync();
 
        return result;
    }
}