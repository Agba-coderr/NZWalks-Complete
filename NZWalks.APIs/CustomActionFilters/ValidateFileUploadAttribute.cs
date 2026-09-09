using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NZWalks.Application.DTOs;

namespace NZWalks.APIs.CustomActionFilters
{
    public class ValidateFileUploadAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Retrieve the DTO object from the action arguments
            var requestDto = context.ActionArguments.Values
                .OfType<ImageUploadRequestDto>()
                .FirstOrDefault();

            if (requestDto == null || requestDto.FileStream == null)
            {
                context.ModelState.AddModelError("File", "Please upload a valid file.");
            }
            else
            {
                var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(requestDto.FileName).ToLower();

                // 2. Extension validation
                if (!allowedExtensions.Contains(extension))
                {
                    context.ModelState.AddModelError("File", "Only .jpg, .jpeg, .png files are allowed.");
                }

                // 3. File size validation (10MB limit = 10 * 1024 * 1024 bytes)
                if (requestDto.FileStream.Length > 10485760)
                {
                    context.ModelState.AddModelError("File", "File size cannot exceed 10 MB.");
                }
            }

            // 4. Short-circuit request if ModelState is invalid
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
    }
}
