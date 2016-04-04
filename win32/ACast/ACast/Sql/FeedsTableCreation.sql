-- Script Date: 03.04.2016 05:48  - ErikEJ.SqlCeScripting version 3.5.2.58
CREATE TABLE [Feeds] (
  [Id] INTEGER NOT NULL
, [Title] nchar(100) NOT NULL
, [Uri] nchar(100) NOT NULL
, CONSTRAINT [PK_Feeds] PRIMARY KEY ([Id])
);
