using BasicInformationOfDataWEBAPI.Common.DTOs;

namespace BasicInformationOfDataWEBAPI.Services
{
    public interface IUserService
    {   /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="name">用户名</param>
        /// <returns>用户列表</returns>
        Task<List<UserDto>> GetUsersAsync(int uid, string name);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        Task<UserSessionDto?> LoginAsync(string username, string password);



        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
      Task<UserSessionDto?> GetUserInfoAsync(int userId);



    }
}
