using Microsoft.EntityFrameworkCore;

namespace VAapiNestle.CustomModels
{
    /// <summary>Implemented by request models of list endpoints that support paging.</summary>
    public interface IPagedRequest
    {
        /// <summary>1-based page number. Leave empty to get the full list (unless a default page size is configured).</summary>
        int? PageNumber { get; }

        /// <summary>Rows per page (capped at <see cref="Pagination.MaxPageSize"/>).</summary>
        int? PageSize { get; }
    }

    /// <summary>
    /// Optional paging values, bound from the query string (e.g. <c>?pageNumber=2&amp;pageSize=50</c>) so the request
    /// and response bodies of existing endpoints stay unchanged. Paging totals are returned in X-* response headers.
    /// </summary>
    public sealed class PageQuery : IPagedRequest
    {
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

    public sealed class PageInfo
    {
        public PageInfo(int pageNumber, int pageSize, int totalCount)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        }

        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages { get; }
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    public sealed class PaginationSettings
    {
        public const string SectionName = "Pagination";

        /// <summary>
        /// Page size applied when a client does not send paging values. 0 = no default paging
        /// (clients that do not send pageNumber/pageSize keep receiving the full list).
        /// </summary>
        public int DefaultPageSize { get; set; }
    }

    public static class Pagination
    {
        public const int MaxPageSize = 500;

        public const string TotalCountHeader = "X-Total-Count";
        public const string PageNumberHeader = "X-Page-Number";
        public const string PageSizeHeader = "X-Page-Size";
        public const string TotalPagesHeader = "X-Total-Pages";
        public static readonly string[] Headers = [TotalCountHeader, PageNumberHeader, PageSizeHeader, TotalPagesHeader];

        /// <summary>Returns the (page, size) to apply, or null when the request should not be paged.</summary>
        public static (int Page, int Size)? Resolve(IPagedRequest? request, int defaultPageSize)
        {
            var size = request?.PageSize is > 0 ? request.PageSize.Value : defaultPageSize;
            var page = request?.PageNumber is > 0 ? request.PageNumber.Value : 1;

            // Paging is requested when the client sends either value, or a default is configured
            bool requested = request?.PageNumber is > 0 || request?.PageSize is > 0 || defaultPageSize > 0;
            if (!requested)
                return null;

            if (size <= 0)
                size = 50;
            return (page, Math.Min(size, MaxPageSize));
        }

        /// <summary>Pages a database query (COUNT + OFFSET/FETCH run in SQL). The query must be ordered.</summary>
        public static async Task<(List<T> Items, PageInfo? Page)> ToPageAsync<T>(
            this IQueryable<T> orderedQuery, IPagedRequest? request, int defaultPageSize, CancellationToken cancellationToken = default)
        {
            var paging = Resolve(request, defaultPageSize);
            if (paging == null)
                return (await orderedQuery.ToListAsync(cancellationToken), null);

            var (page, size) = paging.Value;
            var total = await orderedQuery.CountAsync(cancellationToken);
            var items = await orderedQuery.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken);
            return (items, new PageInfo(page, size, total));
        }

        /// <summary>Pages an in-memory sequence.</summary>
        public static (List<T> Items, PageInfo? Page) ToPage<T>(this IEnumerable<T> source, IPagedRequest? request, int defaultPageSize)
        {
            var list = source as IReadOnlyCollection<T> ?? source.ToList();
            var paging = Resolve(request, defaultPageSize);
            if (paging == null)
                return (list.ToList(), null);

            var (page, size) = paging.Value;
            return (list.Skip((page - 1) * size).Take(size).ToList(), new PageInfo(page, size, list.Count));
        }

        /// <summary>Paging metadata as response headers, so list bodies keep their existing shape.</summary>
        public static void WriteHeaders(this HttpResponse response, PageInfo? page)
        {
            if (page == null)
                return;
            response.Headers[TotalCountHeader] = page.TotalCount.ToString();
            response.Headers[PageNumberHeader] = page.PageNumber.ToString();
            response.Headers[PageSizeHeader] = page.PageSize.ToString();
            response.Headers[TotalPagesHeader] = page.TotalPages.ToString();
        }
    }
}
