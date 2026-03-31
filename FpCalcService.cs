using System.Diagnostics;
using System.IO;

public class FpCalcService
{
    public (string fingerprint, int duration) CalculateFingerprint(string filePath)
    {
        var fpcalcPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "fpcalc.exe");
        
        if (!File.Exists(fpcalcPath))
            throw new FileNotFoundException("fpcalc.exe not found in tools directory");
        
        var processInfo = new ProcessStartInfo
        {
            FileName = fpcalcPath,
            Arguments = $"-length 120 \"{filePath}\"", // 只分析前2分钟
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        
        using (var process = Process.Start(processInfo))
        {
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            return ParseFpcalcOutput(output);
        }
    }
    
    private (string, int) ParseFpcalcOutput(string output)
    {
        // 解析fpcalc的输出格式
        var lines = output.Split('\n');
        string fingerprint = null;
        int duration = 0;
        
        foreach (var line in lines)
        {
            if (line.StartsWith("FINGERPRINT="))
                fingerprint = line.Substring(12);
            else if (line.StartsWith("DURATION="))
                int.TryParse(line.Substring(9), out duration);
        }
        
        return (fingerprint, duration);
    }
}