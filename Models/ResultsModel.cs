using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace FerramentariaTest.Models
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Success result cannot have an error");
            if (!isSuccess && string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Failure result must have an error");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new Result(true, string.Empty);
        public static Result Failure(string error) => new Result(false, error);
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        private Result(bool isSuccess, T value, string error) : base(isSuccess, error)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, string.Empty);
        public static new Result<T> Failure(string error) => new Result<T>(false, default!, error);
    }


    public class ProcessErrorException : Exception
    {
        public ProcessErrorException() : base() { }
        public ProcessErrorException(string message) : base(message) { }
        public ProcessErrorException(string message, Exception innerException) : base(message, innerException) { }

        public ProcessErrorException(string message, string paramName)
            : base(string.IsNullOrEmpty(paramName) ? message : $"{message} (Parameter: {paramName})")
        {
        }
    }

    public class ModifiedErrorException : Exception
    {
        public ModifiedErrorException() : base() { }
        public ModifiedErrorException(string message) : base(message) { }
        public ModifiedErrorException(string message, Exception innerException) : base(message, innerException) { }

        public ModifiedErrorException(string message, string paramName)
            : base(string.IsNullOrEmpty(paramName) ? message : $"{message} (Parameter: {paramName})")
        {
        }
    }


    public class UserContextException : Exception
    {
        public UserContextException() : base() { }
        public UserContextException(string message) : base(message) { }
    }

    public class PageAccessAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _pages;

        public PageAccessAuthorizeAttribute(params string[] pages)
        {
            _pages = pages ?? throw new ArgumentNullException(nameof(pages));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip authorization if action has [AllowAnonymous] attribute
            if (context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute))
                return;

            var user = context.HttpContext.User;

            // Check if user is authenticated
            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Get all PageAccess claims for the user
            var userPages = user.FindAll("PageAccess").Select(c => c.Value).ToHashSet();

            // Check if user has ALL required pages
            var hasAllPages = _pages.All(page => userPages.Contains(page));

            if (!hasAllPages)
            {
                context.Result = new ForbidResult();
            }
        }
    }

}
