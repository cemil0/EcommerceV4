using Microsoft.EntityFrameworkCore.Migrations;

namespace ECommerce.Infrastructure.Migrations
{
    public partial class AddUpdateCompanyBalanceSP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
CREATE OR ALTER PROCEDURE usp_UpdateCompanyBalance
    @CompanyId INT,
    @Amount DECIMAL(18,2),
    @TransactionType NVARCHAR(20), -- 'CREDIT' (Limit Increase/Refund) or 'DEBIT' (Purchase)
    @Description NVARCHAR(MAX),
    @IsForce BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    TRY
        DECLARE @CurrentBalance DECIMAL(18,2);
        DECLARE @CreditLimit DECIMAL(18,2);
        DECLARE @NewBalance DECIMAL(18,2);

        -- Lock the company row for update to prevent race conditions
        SELECT 
            @CurrentBalance = CurrentBalance,
            @CreditLimit = CreditLimit
        FROM Companies WITH (UPDLOCK, ROWLOCK)
        WHERE CompanyId = @CompanyId;

        IF @CurrentBalance IS NULL
        BEGIN
            THROW 50001, 'Company not found.', 1;
        END

        IF @TransactionType = 'DEBIT'
        BEGIN
            SET @NewBalance = @CurrentBalance + @Amount; -- Balance is 'Used Amount'
            
            IF @IsForce = 0 AND @NewBalance > @CreditLimit
            BEGIN
                THROW 50002, 'Credit limit exceeded.', 1;
            END
        END
        ELSE IF @TransactionType = 'CREDIT'
        BEGIN
            SET @NewBalance = @CurrentBalance - @Amount;
        END
        ELSE
        BEGIN
            THROW 50003, 'Invalid transaction type.', 1;
        END

        UPDATE Companies
        SET CurrentBalance = @NewBalance
        WHERE CompanyId = @CompanyId;

        COMMIT TRANSACTION;
        
        SELECT @NewBalance as NewBalance;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS usp_UpdateCompanyBalance");
        }
    }
}
