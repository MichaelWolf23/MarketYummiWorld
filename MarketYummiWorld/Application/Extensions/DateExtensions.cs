using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MarketYummiWorld.Application.Extensions;

public static class DateExtensions
{
    /// Преобразует DateTime в формат БД: YYYY-MM-DD
    public static string ToDbDate(this DateTime date) => date.ToString("yyyy-MM-dd");

    /// Преобразует DateTime в формат БД: YYYY-MM-DD HH:mm
    public static string ToDbDateTime(this DateTime dt) => dt.ToString("yyyy-MM-dd HH:mm");

    /// Парсит строку из БД в DateTime
    public static DateTime FromDbDate(string str) => DateTime.ParseExact(str, "yyyy-MM-dd", null);
    public static DateTime FromDbDateTime(string str) => DateTime.ParseExact(str, "yyyy-MM-dd HH:mm", null);
}