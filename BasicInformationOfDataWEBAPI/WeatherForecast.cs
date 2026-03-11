// 定义命名空间 BasicInformationOfDataWEBAPI，通常与项目名称一致，用于组织和管理代码
namespace BasicInformationOfDataWEBAPI
{
    // 定义一个公共类 WeatherForecast（天气预报）
    public class WeatherForecast
    {
        // 定义一个 DateOnly 类型的属性 Date
        // 表示天气预报的日期（仅包含日期，不包含时间）
        public DateOnly Date { get; set; }

        // 定义一个整型属性 TemperatureC
        // 表示摄氏温度（Celsius）
        public int TemperatureC { get; set; }

        // 定义一个只读属性 TemperatureF
        // 表示华氏温度（Fahrenheit）
        // 使用表达式主体语法，根据摄氏温度计算华氏温度
        // 公式：F = 32 + C / 0.5556
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        // 定义一个可空字符串属性 Summary
        // 用于描述天气情况（如 "Sunny", "Cold", "Hot" 等）
        // ? 表示该字符串可以为 null（启用了可空引用类型的情况下）
        public string? Summary { get; set; }
    }
}