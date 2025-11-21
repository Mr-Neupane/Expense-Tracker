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
values ( 'Admin User', 'admin' ) ON CONFLICT (username) DO NOTHING
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
                                                                                    ID SERIAL PRIMARY KEY,
                                                                                    ParentId INT,
                                                                                    LedgerName VARCHAR(100) UNIQUE,
                                                                                    RecStatus CHAR NOT NULL,
                                                                                    Status INT,
                                                                                    RecById INT,
                                                                                    CONSTRAINT fk_ledger_parent
                                                                                        FOREIGN KEY (ParentId) REFERENCES accounting.coa(id)
                                                                                        )
                            ";
        await conn.ExecuteAsync(tablecreation);

        var BankQuery =
            @" create table if not exists bank.bank(id serial primary key, bankname varchar(100) , accountnumber varchar(50), bankcontactnumber int,ledgerid int,
                bankaddress varchar(100),accountopendate date,recstatus char,recdate timestamp, status int, unique(bankname,accountnumber) ) ";
        await conn.ExecuteAsync(BankQuery);

        var ledgeralter = @" alter table accounting.ledger add column if not exists subparentid int;";
        await conn.ExecuteAsync(ledgeralter);
        var alter = @" ALTER TABLE accounting.ledger ADD COLUMN if not exists code VARCHAR(50) NOT NULL UNIQUE;";
        await conn.ExecuteAsync(alter);
        var defaultledger = @"
DO $$
    DECLARE
        v_exists BOOLEAN;
    BEGIN
        SELECT EXISTS (
            SELECT 1
            FROM accounting.ledger
            WHERE id  IN (-1, -2, -3)
        ) INTO v_exists;

        IF v_exists THEN
            RAISE NOTICE 'Data already exists. No insert performed.';
            RETURN;
        END IF;

        WITH parentIns AS (
            INSERT INTO accounting.ledger
                (id, parentid, ledgername, recstatus, status, recbyid, code, subparentid)
                VALUES
                    (-1, 1, 'Cash Account', 'A', 1, 1, '80', NULL)
                RETURNING id AS cid
        ), OtherParent AS (
            INSERT INTO accounting.ledger
                (id, parentid, ledgername, recstatus, status, recbyid, code, subparentid)
                VALUES
                    (-2, 1, 'Bank Account', 'A', 1, 1, '90', NULL)
                RETURNING id
        )
        INSERT INTO accounting.ledger
        (id, parentid, ledgername, recstatus, status, recbyid, code, subparentid)
        SELECT
            -3,
            NULL,
            'Cash',
            'A',
            1,
            1,
            '80.1',
            cid
        FROM parentIns;

        RAISE NOTICE 'Inserts completed successfully.';
    END $$;
";
        await conn.ExecuteAsync(defaultledger);
        conn.Close();
    }
}