
CREATE DATABASE Soccer;
GO

USE Soccer;
GO

CREATE TABLE League (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(50) NOT NULL,
    Country VARCHAR(50) NOT NULL,
    StartDate DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    EndDate DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    Enabled BIT NOT NULL DEFAULT 1,

    CONSTRAINT CHK_League_StartDate
        CHECK (StartDate <= EndDate),

    CONSTRAINT CHK_League_Name
        CHECK (LEN(LTRIM(RTRIM(Name))) >= 2),

    CONSTRAINT CHK_League_Country
        CHECK (LEN(LTRIM(RTRIM(Country))) >= 2)
);
GO

CREATE TABLE Team (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(50) NOT NULL,
    Country VARCHAR(50) NOT NULL,
    PlayersQuantity INT NOT NULL,
    Enabled BIT NOT NULL DEFAULT 1,

    CONSTRAINT CHK_Team_PlayersQuantity
        CHECK (PlayersQuantity BETWEEN 11 AND 22),

    CONSTRAINT CHK_Team_Name
        CHECK (LEN(LTRIM(RTRIM(Name))) >= 2),

    CONSTRAINT CHK_Team_Country
        CHECK (LEN(LTRIM(RTRIM(Country))) >= 2)
);
GO

CREATE TABLE LEAGUE_TEAM (
    LeagueId INT NOT NULL,
    TeamId INT NOT NULL,

    CONSTRAINT PK_LEAGUE_TEAM
        PRIMARY KEY (LeagueId, TeamId),

    CONSTRAINT FK_LEAGUE_TEAM_League
        FOREIGN KEY (LeagueId) REFERENCES League(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_LEAGUE_TEAM_Team
        FOREIGN KEY (TeamId) REFERENCES Team(Id)
        ON DELETE CASCADE
);
GO