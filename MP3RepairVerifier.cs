// MP3RepairVerifier.cs
using System;
using System.Text;
using TagLib;
using System.IO;
using System.Collections.Generic;

namespace MP3Player
{
        public class MP3RepairVerifier
    {
        public static VerificationResult VerifyRepair(string filePath)
        {
            var result = new VerificationResult();
            
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    result.FileExists = true;
                    result.FileSize = new System.IO.FileInfo(filePath).Length;
                    
                    // 检查各个字段
                    result.Title = VerifyField("标题", file.Tag.Title);
                    result.Artist = VerifyField("艺术家", file.Tag.FirstPerformer);
                    result.Album = VerifyField("专辑", file.Tag.Album);
                    
                    // 总体评估
                    result.IsRepaired = result.Title.IsCorrect && result.Artist.IsCorrect && result.Album.IsCorrect;
                    result.OverallStatus = result.IsRepaired ? "修复成功" : "仍需修复";
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }
            
            return result;
        }
        
        private static FieldVerification VerifyField(string fieldName, string value)
        {
            var verification = new FieldVerification
            {
                FieldName = fieldName,
                Value = value,
                IsEmpty = string.IsNullOrEmpty(value)
            };
            
            if (!verification.IsEmpty)
            {
                // 检查编码
                verification.IsValidUTF8 = IsValidUTF8(value);
                verification.ContainsChinese = ContainsChinese(value);
                verification.ByteCount = Encoding.UTF8.GetByteCount(value);
                
                // 判断是否正确
                verification.IsCorrect = verification.IsValidUTF8 && verification.ContainsChinese;
            }
            
            return verification;
        }
        
        private static bool IsValidUTF8(string text)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(text);
                string roundtrip = Encoding.UTF8.GetString(bytes);
                return roundtrip == text;
            }
            catch
            {
                return false;
            }
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
    }

    public class VerificationResult
    {
        public bool FileExists { get; set; }
        public long FileSize { get; set; }
        public FieldVerification Title { get; set; }
        public FieldVerification Artist { get; set; }
        public FieldVerification Album { get; set; }
        public bool IsRepaired { get; set; }
        public string OverallStatus { get; set; }
        public string Error { get; set; }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MP3修复验证结果 ===");
            sb.AppendLine($"文件存在: {FileExists}");
            sb.AppendLine($"文件大小: {FileSize} 字节");
            
            if (Title != null) sb.AppendLine($"\n标题: {Title}");
            if (Artist != null) sb.AppendLine($"\n艺术家: {Artist}");
            if (Album != null) sb.AppendLine($"\n专辑: {Album}");
            
            sb.AppendLine($"\n总体状态: {OverallStatus}");
            sb.AppendLine($"修复成功: {IsRepaired}");
            
            if (!string.IsNullOrEmpty(Error))
                sb.AppendLine($"\n错误: {Error}");
            
            return sb.ToString();
        }
    }

    public class FieldVerification
    {
        public string FieldName { get; set; }
        public string Value { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsValidUTF8 { get; set; }
        public bool ContainsChinese { get; set; }
        public bool IsCorrect { get; set; }
        public int ByteCount { get; set; }
        
        public override string ToString()
        {
            if (IsEmpty)
                return $"{FieldName}: (空)";
            
            return $"{FieldName}: '{Value}' | UTF-8有效: {IsValidUTF8} | 含中文: {ContainsChinese} | 正确: {IsCorrect} | 字节数: {ByteCount}";
        }
    }
}