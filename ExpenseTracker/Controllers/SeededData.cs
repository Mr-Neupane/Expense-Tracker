using Dapper;
using Npgsql;

namespace ExpenseTracker.Controllers;

public class SeededData
{
    public static async Task SeededQuery()
    {
        using (NpgsqlConnection conn = (NpgsqlConnection)DapperConnectionProvider.GetConnection())
        {
            using (var txn = conn.BeginTransaction())
            {
                try
                {
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
values ( -1,'Admin User', 'admin' ) ON CONFLICT (username) DO NOTHING
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
                                 CREATE TABLE IF NOT EXISTS accounting.ledger
(
    ID         SERIAL PRIMARY KEY,
    ParentId   int,
    ledgername VARCHAR(100),
    RecStatus  char,
    Status     int,
    RecById    int,
    code varchar(10) not null ,
    subparentid int  ,
    unique (ledgername, code),
    CONSTRAINT fk_parentid foreign key (ParentId) references accounting.coa (id),
    CONSTRAINT fk_recbyid foreign key (RecById) references users (id)
) 
                            ";
                    await conn.ExecuteAsync(tablecreation);

                    var BankQuery =
                        @" create table if not exists bank.bank
(
    id                serial primary key,
    bankname          varchar(100) not null ,
    accountnumber     varchar(50) not null ,
    bankcontactnumber int,
    ledgerid          int not null ,
    remainingbalance  decimal   default 0,
    bankaddress       varchar(100),
    accountopendate   date,
    recstatus         char,
    recdate           timestamp default now(),
    status            int default 1,
    recbyid int not null ,
    unique (bankname, accountnumber),
    constraint fk_ledgerid foreign key (ledgerid) references accounting.ledger(id),
    constraint fk_recbyid foreign key (recbyid) references users(id)
) ";
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
    voucherno varchar(15) not null,
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
    ledgerid int    not null,
    dramount      decimal not null,
    cramount      decimal not null,
    drcr          char    not null,
    recstatus     char(1) default 'A',
    status        int     default 1,
    recbyid       int,
    constraint fk_transactionid foreign key (transactionid) references accounting.transactions (id),
    constraint fk_ledgerid foreign key (ledgerid) references accounting.ledger (id),
    constraint fk_recbyid foreign key (recbyid) references users (id)
    
) ;";
                    await conn.ExecuteAsync(acctxndtl);
                    var defaultledger = @"
DO
$$
    DECLARE
        v_exists BOOLEAN;
    BEGIN
        SELECT EXISTS (SELECT 1
                       FROM accounting.ledger
                       WHERE id IN (-1, -2, -3, -4, -5, -6, -7, -8, -9))
        INTO v_exists;

        IF v_exists THEN
            RAISE NOTICE 'Data already exists. No insert performed.';
            RETURN;
        END IF;

        WITH parentIns AS (
            INSERT INTO accounting.ledger
                (id, parentid, ledgername, recstatus, status, recbyid, code, subparentid)
                VALUES (-1, 1, 'Cash Account', 'A', 1, -1, '80', NULL)
                RETURNING id AS cid),
             BankParent AS (
                 INSERT INTO accounting.ledger
                     (id, parentid, ledgername, recstatus, status, recbyid, code, subparentid)
                     VALUES (-2, 1, 'Bank Account', 'A', 1, -1, '90', NULL)),
             LiabilityParent as (
                 INSERT INTO accounting.ledger
                     (id, parentid, ledgername, recstatus, status, recbyid, code, subparentid)
                     VALUES (-4, 2, 'Current Liabilities', 'A', 1, -1, '60', NULL),
                            (-5, 2, 'Other Liabilities', 'A', 1, -1, '70', NULL)),
             IncomeParent as (
                 INSERT INTO accounting.ledger
                     (id, parentid, ledgername, recstatus, status, recbyid, code, subparentid)
                     VALUES (-6, 3, 'Other Income', 'A', 1, -1, '160.1', NULL),
                            (-7, 3, 'Investment Interest', 'A', 1, -1, '160.2', NULL)),
             ExpenseParent as (
                 INSERT INTO accounting.ledger
                     (id, parentid, ledgername, recstatus, status, recbyid, code, subparentid)
                     VALUES (-8, 4, 'Other Expenses', 'A', 1, -1, '150.1', NULL),
                            (-9, 4, 'Interest Expenses', 'A', 1, -1, '150.2', NULL))
        INSERT
        INTO accounting.ledger
        (id, parentid, ledgername, recstatus, status, recbyid, code, subparentid)
        SELECT -3,
               NULL,
               'Cash',
               'A',
               1,
               -1,
               '80.1',
               cid
        FROM parentIns;

        RAISE NOTICE 'Inserts completed successfully.';
    END
$$;
";
                    await conn.ExecuteAsync(defaultledger);
                    await txn.CommitAsync();
                }
                catch (Exception e)
                {
                    await txn.RollbackAsync();
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}