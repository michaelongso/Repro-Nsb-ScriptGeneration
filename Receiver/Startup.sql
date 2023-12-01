if object_id('dbo.SubmittedOrder', 'U') is null
    create table [dbo].[SubmittedOrder] (
        Id uniqueidentifier not null primary key,
        Value int not null
    )