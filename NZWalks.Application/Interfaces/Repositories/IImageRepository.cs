using NZWalks.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace NZWalks.Application.Interfaces.Repositories
{
    public interface IImageRepository
    {
        Task<Image> Upload(Image image, Stream fileStream);
    }
}
