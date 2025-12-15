using Dapper;
using ExpenseTracker;

public class DateHelper
{
    public static async Task<string> GetNepaliDate(DateTime date)
    {
        var engdate = date.ToString("yyyy-MM-dd").Trim();
        var con = DapperConnectionProvider.GetConnection();
        var query = @"select nepalidate from tbl_NepaliDate where englishdate = CAST(@engdate AS DATE) ";
        var nepdate = await con.QueryFirstAsync<DateTime>(query, new { engdate });
        var res = nepdate.ToString("yyyy-MM-dd").Trim();
        con.Close();
        return res;
    }

    public static async Task<DateTime> GetEnglishDate(DateTime date)
    {
        var nepalidate = date.ToString("yyyy-MM-dd").Trim();
        var con = DapperConnectionProvider.GetConnection();
        var query = @"select englishdate from tbl_NepaliDate where nepalidate = @nepalidate ";
        var englishdate = await con.QueryFirstAsync<DateTime>(query, new { nepalidate });
        return englishdate;
    }
}