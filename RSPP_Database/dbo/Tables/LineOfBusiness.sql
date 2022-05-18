CREATE TABLE [dbo].[LineOfBusiness]
(
	[LineOfBusinessId] INT NOT NULL , 
    [LineOfBusinessName] VARCHAR(100) NOT NULL, 
    [Amount] DECIMAL(18, 2) NOT NULL, 
    [FormTypeId] INT NULL, 
    [OrderId] INT NOT NULL, 
    CONSTRAINT [PK_LineOfBusiness] PRIMARY KEY ([OrderId])
)
