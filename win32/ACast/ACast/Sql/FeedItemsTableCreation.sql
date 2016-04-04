-- Script Date: 03.04.2016 05:51  - ErikEJ.SqlCeScripting version 3.5.2.58
CREATE TABLE [FeedItems] (
  [Id] INTEGER NOT NULL
, [ParentId] int NOT NULL
, [SyndicationItemId] nchar(100) NOT NULL
, [Title] nchar(100) NOT NULL
, [Summary] nchar(1000) NOT NULL
, [PublishedDate] datetime NOT NULL
, [Uri] nchar(100) NOT NULL
, [Path] nchar(100) NOT NULL
, [FileName] nchar(100) NOT NULL
, [PlayerPos] real NOT NULL
, CONSTRAINT [PK_FeedItems] PRIMARY KEY ([Id],[ParentId])
);
