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
                    var date = HomeController.NepaliDate();
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


                    var banktransaction = @"
create table if not exists bank.banktransactions
(
    id             serial primary key,
    bank_id        int         not null,
    txn_date       date,
    amount         decimal     not null,
    type           varchar(20) not null,
    remarks        varchar(100),
    rec_date       timestamp default now(),
    rec_by_id      int         not null,
    rec_status     char(1)   default 'A',
    status         int       default 1,
    transaction_id int         not null,
    CONSTRAINT fk_bankid FOREIGN KEY (bank_id) REFERENCES bank.bank (id),
    CONSTRAINT fk_recbyid FOREIGN KEY (rec_by_id) REFERENCES users (id)
) ;";

                    await conn.ExecuteAsync(banktransaction);

                    var acctransactions = @"
create table if not exists accounting.transactions
(
    id         serial primary key,
    txn_date   date        not null,
    voucher_no varchar(15) not null,
    amount     decimal     not null,
    type       varchar(50) not null,
    type_id    int         not null,
    remarks    varchar(100),
    rec_status char(1)              default 'A',
    rec_date   timestamp   not null default now(),
    status     int                  default 1,
    rec_by_id  int,
    constraint fk_rec_by_id foreign key (rec_by_id) references users (id)
)
;
";
                    await conn.ExecuteAsync(acctransactions);

                    var acctxndtl = @"create table if not exists accounting.transaction_details
(
    id            serial primary key,
    transaction_id int     not null,
    ledger_id int    not null,
    dr_amount      decimal not null,
    cr_amount      decimal not null,
    dr_cr          char    not null,
    rec_status     char(1) default 'A',
    status        int     default 1,
    rec_by_id       int,
    constraint fk_transaction_id foreign key (transaction_id) references accounting.transactions (id),
    constraint fk_ledger_id foreign key (ledger_id) references accounting.ledger (id),
    constraint fk_rec_by_id foreign key (rec_by_id) references users (id)

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

                    var expennsetable = @"
create table if not exists accounting.expenses
(
    id         serial primary key ,
    ledger_id  int       not null,
    dr_amount  decimal   not null,
    cr_amount  decimal   not null,
    txn_date   date      not null,
    rec_status char(1)   not null default 'A',
    status     int       not null default 1,
    rec_date   timestamp not null default now(),
    rec_by_id  int       not null,
    constraint fk_ledger_id foreign key (ledger_id) references accounting.ledger (id),
    constraint fk_rec_by_id foreign key (rec_by_id) references users (id)
) ;";
                    await conn.ExecuteAsync(expennsetable);

                    var incometable = @"
create table if not exists accounting.income
(
    id         serial primary key ,
    ledger_id  int       not null,
    dr_amount  decimal   not null,
    cr_amount  decimal   not null,
    txn_date   date      not null,
    rec_status char(1)   not null default 'A',
    status     int       not null default 1,
    rec_date   timestamp not null default now(),
    rec_by_id  int       not null,
    constraint fk_ledger_id foreign key (ledger_id) references accounting.ledger (id),
    constraint fk_rec_by_id foreign key (rec_by_id) references users (id)
)";
                    await conn.ExecuteAsync(incometable);

                    var liab = @"
create table if not exists accounting.liability
(
    id         serial primary key,
    ledger_id  int       not null,
    dr_amount  decimal   not null,
    cr_amount  decimal   not null,
    txn_date   date      not null,
    rec_status char(1)   not null default 'A',
    status     int       not null default 1,
    rec_date   timestamp not null default now(),
    rec_by_id  int       not null,
    constraint fk_ledger_id foreign key (ledger_id) references accounting.ledger (id),
    constraint fk_rec_by_id foreign key (rec_by_id) references users (id)
)";
                    await conn.ExecuteAsync(liab);
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