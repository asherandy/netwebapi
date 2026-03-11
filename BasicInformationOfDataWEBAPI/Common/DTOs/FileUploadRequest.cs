using System.ComponentModel.DataAnnotations;
using static BasicInformationOfDataWEBAPI.Common.Helpers.CustomizeAttribute;

namespace BasicInformationOfDataWEBAPI.Common.DTOs
{
    /// <summary>
    /// 文件上传请求数据传输对象(DTO)，用于接收客户端上传的文件
    /// </summary>
    public class FileUploadRequest
    {
        /// <summary>
        /// 文件流
        /// </summary>
        // File属性，类型为IFormFile，用于接收HTTP表单上传的文件
        [Required(ErrorMessage = "请上传有效文件")]
        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "文件不能超过 5MB")]
        [AllowedExtensions([".jpg", ".png", ".pdf"], ErrorMessage = "只允许上传 jpg/png/pdf 文件")]
        public IFormFile File { get; set; } = null!;


    }
}
