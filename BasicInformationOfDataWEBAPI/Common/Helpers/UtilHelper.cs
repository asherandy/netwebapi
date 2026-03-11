using System.Text;

namespace BasicInformationOfDataWEBAPI.Common.Helpers
{
    public class UtilHelper
    {

        /// <summary>
        /// 使用 SHA256 进行哈希加密，并转换为 Base64 字符串
        /// </summary>
        /// <param name="Source">需要加密的原始字符串</param>
        /// <returns>加密后的字符串（Base64 格式）</returns>
        public static string EncrypToSHA(string Source) =>

            // Convert.ToBase64String：
            // 将字节数组转换为 Base64 字符串
            Convert.ToBase64String(

                // SHA256.HashData：
                // 对输入字节数组进行 SHA256 哈希运算
                // 返回 32 字节（256 位）的哈希值
                System.Security.Cryptography.SHA256.HashData(
                    // 将原始字符串转换为 UTF8 字节数组
                    UTF8Encoding.UTF8.GetBytes(Source)
                )
            )

            // Base64 中可能包含 "/" 字符
            // 如果用于 URL 或路径，可能不安全
            // 这里替换为 "A"
            .Replace("/", "A")

            // 替换 "\" 字符
            .Replace("\\", "A");


        /// <summary>
        /// 使用 MD5 对字符串进行哈希，并返回 16 进制字符串
        /// </summary>
        /// <param name="Source">需要加密的原始字符串</param>
        /// <returns>32 位 16 进制字符串</returns>
        public static string GetPathEncryp(string Source)
        {
            // 使用 MD5 算法对字符串进行哈希
            // Encoding.Default 表示系统默认编码（不推荐）
            byte[] data = System.Security.Cryptography.MD5.HashData(
                Encoding.Default.GetBytes(Source)
            );

            // 创建字符串构建器（用于拼接结果）
            StringBuilder sb = new();

            // 遍历 MD5 生成的字节数组
            for (int i = 0; i < data.Length; i++)
            {
                // "x2" 表示转换为两位小写 16 进制字符串
                // 例如 15 => "0f"
                sb.Append(data[i].ToString("x2"));
            }

            // 返回最终的 32 位 16 进制字符串
            return sb.ToString();
        }





    }
}
