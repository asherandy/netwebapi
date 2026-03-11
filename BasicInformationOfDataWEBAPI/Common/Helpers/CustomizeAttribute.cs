// 引入数据注解命名空间
// ValidationAttribute 就在这里
using System.ComponentModel.DataAnnotations;

// 命名空间：公共工具类（Helpers）
// 一般用于存放通用工具、扩展方法、自定义特性等
namespace BasicInformationOfDataWEBAPI.Common.Helpers
{
    /// <summary>
    /// 自定义验证特性容器类
    /// 这里继承了 ValidationAttribute，但实际上当前类本身没有重写验证逻辑
    /// 主要用于组织内部的子验证特性类
    /// </summary>
    public class CustomizeAttribute : ValidationAttribute
    {
        /// <summary>
        /// 验证上传文件大小的特性
        /// 使用方式：
        /// [MaxFileSize(5 * 1024 * 1024)]
        /// </summary>
        /// 
        /// AttributeUsage：
        /// 指定该特性只能用于“属性”
        /// 例如：DTO 的属性
        [AttributeUsage(AttributeTargets.Property)]
        public class MaxFileSizeAttribute : ValidationAttribute
        {
            // 最大允许文件大小（单位：字节）
            private readonly long _maxFileSize;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="maxFileSize">
            /// 允许的最大文件大小（字节）
            /// 例如：5MB = 5 * 1024 * 1024
            /// </param>
            public MaxFileSizeAttribute(long maxFileSize)
            {
                _maxFileSize = maxFileSize;
            }

            /// <summary>
            /// 重写验证方法
            /// 当模型验证时会自动调用
            /// </summary>
            /// <param name="value">当前属性的值</param>
            /// <param name="validationContext">验证上下文</param>
            /// <returns>验证结果</returns>
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                // 如果未上传文件（value 为空或不是 IFormFile）
                // 这里不进行处理
                // 是否必须上传由 [Required] 特性负责
                if (value is IFormFile file)
                {
                    // 判断文件大小是否超过限制
                    if (file.Length > _maxFileSize)
                    {
                        // 如果超出限制，返回错误结果
                        // ErrorMessage 是父类 ValidationAttribute 的属性
                        return new ValidationResult(
                            ErrorMessage ?? $"文件大小不能超过 {_maxFileSize / 1024 / 1024} MB"
                        );
                    }
                }

                // 验证通过
                return ValidationResult.Success;
            }
        }

        /// <summary>
        /// 验证上传文件扩展名
        /// 使用方式：
        /// [AllowedExtensions(new string[] { ".jpg", ".png" })]
        /// </summary>
        [AttributeUsage(AttributeTargets.Property)]
        public class AllowedExtensionsAttribute : ValidationAttribute
        {
            // 允许的扩展名列表
            private readonly string[] _extensions;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="extensions">
            /// 允许的文件扩展名数组
            /// 例如：new string[] { ".jpg", ".png" }
            /// </param>
            public AllowedExtensionsAttribute(string[] extensions)
            {
                _extensions = extensions;
            }

            /// <summary>
            /// 重写验证方法
            /// </summary>
            /// <param name="value">当前属性的值</param>
            /// <param name="validationContext">验证上下文</param>
            /// <returns>验证结果</returns>
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                // 如果上传了文件
                if (value is IFormFile file)
                {
                    // 获取文件扩展名
                    // ToLowerInvariant 保证大小写不敏感
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                    // 判断是否在允许列表中
                    if (!_extensions.Contains(extension))
                    {
                        return new ValidationResult(
                            ErrorMessage ?? $"只允许上传文件类型：{string.Join(", ", _extensions)}"
                        );
                    }
                }

                // 验证通过
                return ValidationResult.Success;
            }
        }
    }
}