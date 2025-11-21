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

        var defuser = @"INSERT INTO public.users ( id,username, password)
values ( -1,'Admin User', 'admin' ) on conflict (username) do nothing
";
        await conn.ExecuteAsync(defuser);

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
            @" create table if not exists bank.bank
(
    id                serial primary key,
    bankname          varchar(100),
    accountnumber     varchar(50),
    bankcontactnumber int,
    remainingbalance  decimal(18, 2) default 0,
    bankaddress       varchar(100),
    accountopendate   date,
    recstatus         char,
    recdate           timestamp,
    status            int,
    unique (bankname, accountnumber)
)  ";
        await conn.ExecuteAsync(BankQuery);

        var banktransaction = @"create table if not exists bank.banktransactions
(
    id       serial  primary key,
    bankid    int not null ,
    txndate   date,
    amount    decimal not null,
    type varchar(20) not null,
    remarks   varchar(100),
    recdate   timestamp default now(),
recbyid int not null ,
    recstatus char(1)      default 'A',
    status    int       default 1,
CONSTRAINT fk_bankid FOREIGN KEY (bankid) REFERENCES bank.bank(id),
CONSTRAINT fk_recbyid FOREIGN KEY (recbyid) REFERENCES users(id)
) ;";

        await conn.ExecuteAsync(banktransaction);

        var acctransactions = @"
create table if not exists accounting.transactions
(
    id        serial primary key,
    txndate   date        not null,
    amount    decimal     not null,
    type      varchar(50) not null,
    typeid    int         not null,
    remarks   varchar(100),
    recstatus char(1)              default 'A',
    recdate   timestamp   not null default now(),
    status    int                  default 1,
    recbyid   int,
    constraint fk_recbyid foreign key (recbyid) references users (id)
);
";
        await conn.ExecuteAsync(acctransactions);

        var acctxndtl = @"create table if not exists accounting.transactiondetails
(
    id            serial primary key,
    transactionid int     not null,
    ledgerid      int     not null,
    dramount      decimal not null,
    cramount      decimal not null,
    drcr          char    not null,
    recstatus     char(1) default 'A',
    status        int     default 1,
    recbyid       int,
    constraint fk_transactionid foreign key (transactionid) references accounting.transactions (id),
    constraint fk_ledgerid foreign key (ledgerid) references accounting.ledger(id)
) ;";
        await conn.ExecuteAsync(acctxndtl);

        conn.Close();
    }
}