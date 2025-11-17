using Dapper;

namespace ExpenseTracker.Controllers;

public class SeededData
{
    public static async Task SeededQuery()
    {
        var conn = DapperConnectionProvider.GetConnection();

        var accschema = @"CREATE SCHEMA IF NOT EXISTS accounting;";
        await conn.ExecuteAsync(accschema);
        var bankschema = @"CREATE SCHEMA IF NOT EXISTS bank;";
        await conn.ExecuteAsync(bankschema);

        var createUserQuery = @"

                    CREATE TABLE IF NOT EXISTS users (
                        id SERIAL PRIMARY KEY,
                        username VARCHAR(100) NOT NULL UNIQUE,
                        password VARCHAR(255) NOT NULL
                    );";
        await conn.ExecuteAsync(createUserQuery);

        var defuser = @"INSERT INTO public.users ( username, password)
values ( 'admin', 'admin' )
";

        var parentTable =
            @"Create Table if not exists accounting.COA(ID serial primary key, Name varchar(50) UNIQUE, RecStatus char,Status int,RecById int)";

        await conn.ExecuteAsync(parentTable);

        var cOAIns = @"Insert into accounting.coa( Name, RecStatus, Status,RecById )
                                        values ('Assets', 'A', 1,1),
                                         ('Liabilities', 'A', 1,1),
                                         ('Income', 'A', 1,1),
                                         ('Expenses', 'A', 1,1) 
                                       ON CONFLICT (Name) DO NOTHING;
                    ";
        await conn.ExecuteAsync(cOAIns);

        var tablecreation = @"
                                    CREATE TABLE IF NOT EXISTS accounting.ledger (
                            ID SERIAL PRIMARY KEY, ParentId int, LedgerName VARCHAR(100) Unique, RecStatus char ,Status int,RecById int ) 
                            ";
        await conn.ExecuteAsync(tablecreation);

        var BankQuery =
            @" create table if not exists bank.bank(id serial primary key, bankname varchar(100) , accountnumber varchar(50), bankcontactnumber int,
                bankaddress varchar(100),accountopendate date,recstatus char,recdate timestamp, status int, unique(bankname,accountnumber) ) ";
        await conn.ExecuteAsync(BankQuery);

        conn.Close();
    }
}