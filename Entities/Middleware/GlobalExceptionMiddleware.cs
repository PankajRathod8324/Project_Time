using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Entities.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Proceed to next middleware
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught by middleware");

                context.Response.ContentType = "application/json";

                var (statusCode, message) = GetHttpStatusAndMessage(ex);

                context.Response.StatusCode = statusCode;

                var response = new
                {
                    success = false,
                    message
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }

        private (int StatusCode, string Message) GetHttpStatusAndMessage(Exception ex)
        {
            switch (ex)
            {
                case ArgumentNullException:
                    return (StatusCodes.Status400BadRequest, "Required input was missing.");

                case ArgumentException:
                    return (StatusCodes.Status400BadRequest, "Invalid input provided.");

                case KeyNotFoundException:
                    return (StatusCodes.Status404NotFound, "The requested item was not found.");

                case UnauthorizedAccessException:
                    return (StatusCodes.Status401Unauthorized, "You are not authorized to perform this action.");

                case DbUpdateException dbUpdateEx:
                    if (dbUpdateEx.InnerException is SqlException sqlEx)
                    {
                        // duplicate key value violates unique constraint "waiting_list_email_key"
                        if(sqlEx.Number == 547)
                        {
                            // 547: Foreign key violation
                            return (StatusCodes.Status400BadRequest, "The provided data violates a foreign key constraint.");
                        }
                        else if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                        {
                            // 2601: Unique index or primary key violation
                            // 2627: Unique constraint violation
                            return (StatusCodes.Status409Conflict, "A record with the same value already exists.");
                        }
                    }
                    return (StatusCodes.Status500InternalServerError, "A database error occurred.");

                default:
                    return (StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}