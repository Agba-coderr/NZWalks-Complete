using System;
using System.Collections.Generic;
using System.Text;

namespace NZWalks.Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }

        bool IsAuthenticated { get; }

        bool IsAdmin { get; }
    }
}
