using System;
using System.Text;
using TagLib;
using System.IO;

namespace MP3Player
{
    public class MP3EncodingAnalyzer
    {
        public static string AnalyzeFile(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MP3文件编码深度分析 ===");
            sb.AppendLine($"文件: {Path.GetFileName(filePath)}");
            sb.AppendLine($"路径: {filePath}");
            sb.AppendLine($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"大小: {new FileInfo(filePath).Length} 字节");
            sb.AppendLine();
            
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    // 分析标题
                    AnalyzeField("标题", file.Tag.Title, sb);
                    AnalyzeField("艺术家", file.Tag.FirstPerformer, sb);
                    AnalyzeField("专辑", file.Tag.Album, sb);
                    AnalyzeField("流派", string.Join(", ", file.Tag.Genres), sb);
                    AnalyzeField("作曲者", file.Tag.FirstComposer, sb);
                    AnalyzeField("备注", file.Tag.Comment, sb);
                    
                    // 分析文件结构
                    sb.AppendLine("=== 文件结构信息 ===");
                    sb.AppendLine($"ID3版本: {GetID3Version(file)}");
                    sb.AppendLine($"标签类型: {GetTagTypes(file)}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ 分析错误: {ex.Message}");
            }
            
            return sb.ToString();
        }
        
        private static void AnalyzeField(string fieldName, string value, StringBuilder sb)
        {
            sb.AppendLine($"=== {fieldName}分析 ===");
            sb.AppendLine($"值: {value ?? "(空)"}");
            
            if (!string.IsNullOrEmpty(value))
            {
                // UTF-8编码
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(value);
                sb.AppendLine($"UTF-8字节: {BitConverter.ToString(utf8Bytes).Replace("-", " ")}");
                
                // GBK编码尝试
                try
                {
                    byte[] gbkBytes = Encoding.GetEncoding("GBK").GetBytes(value);
                    sb.AppendLine($"GBK字节: {BitConverter.ToString(gbkBytes).Replace("-", " ")}");
                }
                catch
                {
                    sb.AppendLine($"GBK字节: 无法转换为GBK");
                }
                
                // 编码检测
                sb.AppendLine($"编码推测: {GuessEncoding(value)}");
                sb.AppendLine($"是否为中文: {IsChinese(value)}");
                sb.AppendLine($"字符数: {value.Length}");
            }
            sb.AppendLine();
        }
        
        private static string GuessEncoding(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "未知";
            
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            
            // 检查是否是有效的UTF-8
            try
            {
                string roundtrip = Encoding.UTF8.GetString(bytes);
                if (roundtrip == text)
                {
                    // 检查是否包含中文字符
                    if (ContainsChinese(text))
                    {
                        return "可能是UTF-8编码的中文";
                    }
                    return "有效的UTF-8";
                }
            }
            catch
            {
                return "无效的UTF-8";
            }
            
            // 检查是否包含常见的双重编码模式
            if (text.Contains("鐜") || text.Contains("澶") || text.Contains("鎴"))
                return "可能是双重编码的GBK";
            
            return "无法确定";
        }
        
        private static bool IsChinese(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            
            foreach (char c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FFF)
                    return true;
            }
            return false;
        }
        
        private static bool ContainsChinese(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            
            foreach (char c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FFF)
                    return true;
            }
            return false;
        }
        
        private static string GetID3Version(TagLib.File file)
        {
            try
            {
                if (file.Tag is TagLib.Id3v2.Tag id3v2)
                {
                    return $"ID3v2.{id3v2.Version}";
                }
                else if (file.Tag is TagLib.Id3v1.Tag)
                {
                    return "ID3v1";
                }
            }
            catch
            {
                // 忽略错误
            }
            return "未知";
        }
        
        private static string GetTagTypes(TagLib.File file)
        {
            var types = new List<string>();
            
            if (file.TagTypes.HasFlag(TagLib.TagTypes.Id3v1))
                types.Add("ID3v1");
            if (file.TagTypes.HasFlag(TagLib.TagTypes.Id3v2))
                types.Add("ID3v2");
            if (file.TagTypes.HasFlag(TagLib.TagTypes.Ape))
                types.Add("APE");
            
            return string.Join(", ", types);
        }
    }
}