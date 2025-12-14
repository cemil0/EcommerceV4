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
            SET @NewBalance = @CurrentBalance + @Amount; -- Balance is "Used Amount", so debit increases it
            
            -- Check limit logic (NewBalance > CreditLimit means over limit)
            IF @IsForce = 0 AND @NewBalance > @CreditLimit
            BEGIN
                THROW 50002, 'Credit limit exceeded.', 1;
            END
        END
        ELSE IF @TransactionType = 'CREDIT'
        BEGIN
            SET @NewBalance = @CurrentBalance - @Amount; -- Credit decreases "Used Amount"
            
            -- Optional: Don't allow negative balance (meaning company owed us money but paid too much?) 
            -- For now allowing negative as "prepaid" balance depending on business logic, 
            -- but assuming standard credit limit usage:
            -- 0 means no credit used. Credit Limit means max used.
            -- So balance shouldn't technically go below 0 unless they prepay.
        END
        ELSE
        BEGIN
            THROW 50003, 'Invalid transaction type.', 1;
        END

        -- Update the balance
        UPDATE Companies
        SET CurrentBalance = @NewBalance,
            UpdatedAt = GETUTCDATE()
        WHERE CompanyId = @CompanyId;

        -- Log transaction (Optional if you have a separate ledger table, but good for audit)
        -- INSERT INTO CompanyBalanceHistory ...

        COMMIT TRANSACTION;
        
        -- Return result
        SELECT 
            CAST(1 AS BIT) as Success, 
            'Transaction successful.' as Message,
            @NewBalance as NewBalance,
            @CreditLimitRemaining as (@CreditLimit - @NewBalance) as RemainingLimit;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
