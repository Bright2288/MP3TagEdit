using System;
using System.Collections.Generic;
using System.Text;

namespace MP3Player
{
    public static class GBKDoubleEncodingFixer
    {
        /// <summary>
        /// 修复双重编码的GBK文本
        /// </summary>
        public static string FixDoubleEncodedGBK(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            // 检查是否是典型的双重编码GBK
            if (IsDoubleEncodedGBK(text))
            {
                // 尝试修复
                return DecodeDoubleEncodedGBK(text);
            }
            
            return text;
        }
        
        /// <summary>
        /// 检查是否是双重编码的GBK
        /// </summary>
        private static bool IsDoubleEncodedGBK(string text)
        {
            // 检查是否包含中文字符但看起来是乱码
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            int c3c2Count = 0;
            
            foreach (byte b in bytes)
            {
                if (b == 0xC3 || b == 0xC2)
                {
                    c3c2Count++;
                }
            }
            
            return c3c2Count > bytes.Length * 0.1;
        }
        
        /// <summary>
        /// 解码双重编码的GBK
        /// </summary>
        private static string DecodeDoubleEncodedGBK(string text)
        {
            try
            {
                // 将UTF-8文本转换为字节
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
                List<byte> gbkBytes = new List<byte>();
                
                // 手动转换C3/C2字节
                for (int i = 0; i < utf8Bytes.Length; i++)
                {
                    byte currentByte = utf8Bytes[i];
                    
                    if (currentByte == 0xC3 && i + 1 < utf8Bytes.Length)
                    {
                        // C3 XX -> (XX + 0x40)
                        gbkBytes.Add((byte)(utf8Bytes[i + 1] + 0x40));
                        i++; // 跳过下一个字节
                    }
                    else if (currentByte == 0xC2 && i + 1 < utf8Bytes.Length)
                    {
                        // C2 XX -> XX
                        gbkBytes.Add(utf8Bytes[i + 1]);
                        i++; // 跳过下一个字节
                    }
                    else
                    {
                        gbkBytes.Add(currentByte);
                    }
                }
                
                // 将GBK字节解码为字符串
                Encoding gbkEncoding = GetGBKEncoding();
                if (gbkEncoding != null)
                {
                    string result = gbkEncoding.GetString(gbkBytes.ToArray());
                    
                    // 验证结果
                    if (IsValidChinese(result))
                    {
                        return result;
                    }
                }
            }
            catch
            {
                // 如果转换失败，返回原始文本
            }
            
            return text;
        }
        
        /// <summary>
        /// 获取GBK编码
        /// </summary>
        private static Encoding GetGBKEncoding()
        {
            try
            {
                // 尝试注册编码提供程序
                Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding("GBK");
            }
            catch
            {
                try
                {
                    return Encoding.GetEncoding(936); // GBK代码页
                }
                catch
                {
                    return null;
                }
            }
        }
        
        /// <summary>
        /// 检查是否是有效的中文文本
        /// </summary>
        public static bool IsValidChinese(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            
            int chineseCount = 0;
            
            foreach (char c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FFF) // 基本汉字
                {
                    chineseCount++;
                }
                else if (c == '�' || c == '?' || c == '\uFFFD')
                {
                    // 包含无效字符
                    return false;
                }
            }
            
            return chineseCount > 0;
        }
        
        /// <summary>
        /// 手动修复常见的双重编码模式
        /// </summary>
        public static string ManualFix(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            
            // 常见双重编码映射表
            var mapping = GetCommonDoubleEncodingMapping();
            
            foreach (var item in mapping)
            {
                if (text.Contains(item.Key))
                {
                    text = text.Replace(item.Key, item.Value);
                }
            }
            
            return text;
        }
        
        /// <summary>
        /// 获取常见的双重编码映射
        /// </summary>
        private static Dictionary<string, string> GetCommonDoubleEncodingMapping()
        {
            return new Dictionary<string, string>
            {
                // 你的文件中的模式
                {"澶╀娇", "天使"},
                {"鐜嬭彶", "王菲"},
                {"鎴戠殑", "我的"},
                {"鏃朵唬", "时代"},
                
                // 其他常见模式
                {"涓枃", "中文"},
                {"鏂囨湰", "文本"},
                {"缂栫爜", "编码"},
                {"淇", "修正"},
                
                // 单个字符映射
                {"澶", "天"},
                {"╀", ""},  // 这个是无效字符
                {"娇", "使"},
                {"鐜", "王"},
                {"嬭", "菲"}, // 注意："王菲"是两个字
                {"彶", ""},   // 这个是无效字符
                {"鎴", "我"},
                {"戠", "的"},
                {"殑", ""},   // 这个是无效字符
                {"鏃", "时"},
                {"朵", "代"},
                {"唬", ""}    // 这个是无效字符
            };
        }
        
        /// <summary>
        /// 分析文本编码
        /// </summary>
        public static string AnalyzeEncoding(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "空文本";
            
            var result = new StringBuilder();
            result.AppendLine($"文本: {text}");
            result.AppendLine($"长度: {text.Length} 字符");
            
            // 显示UTF-8字节
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            result.AppendLine($"UTF-8字节: {BitConverter.ToString(utf8Bytes).Replace("-", " ")}");
            
            // 分析可能的编码
            result.AppendLine("\n可能的编码分析:");
            
            // 尝试作为GBK解码
            try
            {
                Encoding gbk = GetGBKEncoding();
                if (gbk != null)
                {
                    string gbkDecoded = gbk.GetString(utf8Bytes);
                    result.AppendLine($"作为GBK解码: {gbkDecoded}");
                    
                    // 检查是否是有效中文
                    if (IsValidChinese(gbkDecoded))
                    {
                        result.AppendLine("✅ 看起来是有效的GBK编码");
                    }
                }
            }
            catch
            {
                result.AppendLine("❌ 无法解码为GBK");
            }
            
            return result.ToString();
        }
    }
}