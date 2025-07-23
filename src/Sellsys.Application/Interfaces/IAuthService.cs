using Sellsys.Application.DTOs.Auth;
using Sellsys.CrossCutting.Common;

namespace Sellsys.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(string username, string password);
    }
}
