-- RaceDay Event Management System - Database Setup Script
-- Creates the complete database schema and seeds with sample data
-- Note: Passwords are hashed in the API layer (Part 2), not stored plaintext here

create database RaceDay;
Use RaceDay;

-- Lookup tables for status management
create table Event_Status (
    EVENT_STATUS_ID int primary key identity(1,1),
    STATUS_NAME varchar(50) not null unique
);

create table Enrolment_Status (
    STATUS_ID int primary key identity(1,1),
    STATUS_NAME varchar(50) not null unique
);

create table Event_Types (
    TYPE_ID int primary key identity(1,1),
    TYPE_NAME varchar(50) not null unique
);

-- Core users table with role-based access
create table Users (
    USER_ID int primary key identity(1,1),
    EMAIL varchar(100) not null unique,
    PASSWORD varchar(255) not null,
    FIRST_NAME varchar(50) not null,
    LAST_NAME varchar(50) not null,
    ROLE varchar(20) not null check (ROLE in ('Organiser', 'Participant'))
);

-- Events created and managed by organisers
create table Events (
    EVENT_ID int primary key identity(1,1),
    USER_ID int not null,
    TYPE_ID int not null,
    EVENT_STATUS_ID int not null default 2,
    NAME varchar(100) not null,
    DESCRIPTION varchar(500) not null,
    EVENT_DATE datetime not null,
    LOCATION varchar(200) not null,
    DISTANCE decimal(10,2) not null,
    foreign key (USER_ID) references Users(USER_ID),
    foreign key (TYPE_ID) references Event_Types(TYPE_ID),
    foreign key (EVENT_STATUS_ID) references Event_Status(EVENT_STATUS_ID)
);

-- Age or distance categories for each event
create table Categories (
    CATEGORY_ID int primary key identity(1,1),
    EVENT_ID int not null,
    CATEGORY_NAME varchar(100) not null,
    AGE_GROUP varchar(50) null,
    unique (EVENT_ID, CATEGORY_NAME),
    foreign key (EVENT_ID) references Events(EVENT_ID) on delete cascade
);

-- Participant enrollments in events with category selection
create table Enrolments (
    ENROLMENT_ID int primary key identity(1,1),
    USER_ID int not null,
    EVENT_ID int not null,
    CATEGORY_ID int not null,
    STATUS_ID int not null default 1,
    foreign key (USER_ID) references Users(USER_ID),
    foreign key (EVENT_ID) references Events(EVENT_ID) on delete cascade,
    foreign key (CATEGORY_ID) references Categories(CATEGORY_ID),
    foreign key (STATUS_ID) references Enrolment_Status(STATUS_ID)
);

-- Results captured after event completion
create table Results (
    RESULT_ID int primary key identity(1,1),
    ENROLMENT_ID int not null,
    FINISH_TIME time not null,
    FINISHING_POSITION int not null,
    foreign key (ENROLMENT_ID) references Enrolments(ENROLMENT_ID) on delete cascade
);

-- Seed status lookup data
insert into Event_Status (STATUS_NAME) values
('Draft'),
('Open'),
('Closed'),
('Completed'),
('Cancelled');

insert into Enrolment_Status (STATUS_NAME) values
('Pending'),
('Confirmed'),
('Cancelled'),
('Completed'),
('Waitlisted');

insert into Event_Types (TYPE_NAME) values
('Run'),
('Walk'),
('Cycle'),
('Triathlon'),
('Ultra Marathon');

-- Seed users - 5 organisers and 8 participants
insert into Users (EMAIL, PASSWORD, FIRST_NAME, LAST_NAME, ROLE) values
('thabo.mokoena@raceday.co.za', 'hashed_pass_1', 'Thabo', 'Mokoena', 'Organiser'),
('priya.naidoo@raceday.co.za', 'hashed_pass_2', 'Priya', 'Naidoo', 'Organiser'),
('sipho.ndlovu@raceday.co.za', 'hashed_pass_3', 'Sipho', 'Ndlovu', 'Organiser'),
('linda.mthembu@raceday.co.za', 'hashed_pass_4', 'Linda', 'Mthembu', 'Organiser'),
('peter.vanwyk@raceday.co.za', 'hashed_pass_5', 'Peter', 'Van Wyk', 'Organiser'),
('amara.williams@gmail.com', 'hashed_pass_6', 'Amara', 'Williams', 'Participant'),
('david.mathews@yahoo.com', 'hashed_pass_7', 'David', 'Mathews', 'Participant'),
('thandiwe.zuma@outlook.com', 'hashed_pass_8', 'Thandiwe', 'Zuma', 'Participant'),
('james.anderson@gmail.com', 'hashed_pass_9', 'James', 'Anderson', 'Participant'),
('lindiwe.zulu@yahoo.com', 'hashed_pass_10', 'Lindiwe', 'Zulu', 'Participant'),
('brian.smith@gmail.com', 'hashed_pass_11', 'Brian', 'Smith', 'Participant'),
('nomsa.dlamini@outlook.com', 'hashed_pass_12', 'Nomsa', 'Dlamini', 'Participant'),
('kevin.johnson@gmail.com', 'hashed_pass_13', 'Kevin', 'Johnson', 'Participant');

-- Seed events (6 events - all distances in km)
insert into Events (USER_ID, TYPE_ID, EVENT_STATUS_ID, NAME, DESCRIPTION, EVENT_DATE, LOCATION, DISTANCE) values
(1, 1, 4, 'Cape Town Marathon 2026', 'The most scenic marathon in South Africa along the Atlantic Seaboard', '2026-05-16 06:00:00', 'Cape Town Stadium, Cape Town', 42.20),
(2, 3, 2, 'Durban Cycle Challenge 2026', 'Fast and flat cycle race along the Durban beachfront', '2026-06-21 07:00:00', 'Moses Mabhida Stadium, Durban', 95.00),
(3, 2, 4, 'Johannesburg Charity Walk 2026', 'Annual charity walk raising funds for children with disabilities', '2026-07-12 08:00:00', 'Mary Fitzgerald Square, Johannesburg', 10.00),
(4, 5, 2, 'Comrades Marathon 2026', 'The ultimate human race - 90km from Pietermaritzburg to Durban', '2026-08-29 05:30:00', 'Pietermaritzburg City Hall, KZN', 90.00),
(5, 1, 4, 'Pretoria Night Run 2026', 'A unique night run through the illuminated streets of Pretoria', '2026-09-05 19:00:00', 'Union Buildings, Pretoria', 15.00),
(1, 3, 1, 'Cape Winelands Cycle Tour 2026', 'Cycle through the scenic vineyards of Stellenbosch and Franschhoek', '2026-10-18 06:30:00', 'Stellenbosch Town Hall, Western Cape', 70.00);

-- Cape Town Marathon categories
insert into Categories (EVENT_ID, CATEGORY_NAME, AGE_GROUP) values
(1, 'Elite Women', '18-34'),
(1, 'Elite Men', '18-34'),
(1, 'Open Women', '35-49'),
(1, 'Open Men', '35-49'),
(1, 'Masters Women', '50-59'),
(1, 'Masters Men', '50-59'),
(1, 'Grand Masters', '60+');

-- Durban Cycle Challenge categories
insert into Categories (EVENT_ID, CATEGORY_NAME, AGE_GROUP) values
(2, 'Professional Men', '18-30'),
(2, 'Professional Women', '18-30'),
(2, 'Amateur Men', '31-45'),
(2, 'Amateur Women', '31-45'),
(2, 'Junior Boys', '15-17'),
(2, 'Junior Girls', '15-17');

-- Johannesburg Charity Walk categories
insert into Categories (EVENT_ID, CATEGORY_NAME, AGE_GROUP) values
(3, 'Under 12 Boys', '8-11'),
(3, 'Under 12 Girls', '8-11'),
(3, 'Open Men', '18-40'),
(3, 'Open Women', '18-40'),
(3, 'Senior Men', '41-60'),
(3, 'Senior Women', '41-60'),
(3, 'Veteran Men', '61+'),
(3, 'Veteran Women', '61+');

-- Comrades Marathon categories
insert into Categories (EVENT_ID, CATEGORY_NAME, AGE_GROUP) values
(4, 'Pro Men', '25-39'),
(4, 'Pro Women', '25-39'),
(4, 'Amateur Men', '40-54'),
(4, 'Amateur Women', '40-54'),
(4, 'Veteran Men', '55-64'),
(4, 'Veteran Women', '55-64'),
(4, 'Legends', '65+');

-- Pretoria Night Run categories
insert into Categories (EVENT_ID, CATEGORY_NAME, AGE_GROUP) values
(5, 'Youth Boys', '13-17'),
(5, 'Youth Girls', '13-17'),
(5, 'Elite Men', '18-40'),
(5, 'Elite Women', '18-40'),
(5, 'Masters Men', '41-60'),
(5, 'Masters Women', '41-60');

-- Cape Winelands Cycle Tour categories
insert into Categories (EVENT_ID, CATEGORY_NAME, AGE_GROUP) values
(6, 'Open Men', '18-39'),
(6, 'Open Women', '18-39'),
(6, 'Veteran Men', '40-54'),
(6, 'Veteran Women', '40-54'),
(6, 'Grand Veteran Men', '55+'),
(6, 'Grand Veteran Women', '55+');

-- Participant enrollments in events
insert into Enrolments (USER_ID, EVENT_ID, CATEGORY_ID, STATUS_ID) values
(6, 1, 1, 2),   -- Amara - Elite Women - Confirmed
(7, 1, 2, 2),   -- David - Elite Men - Confirmed
(8, 1, 3, 2),   -- Thandiwe - Open Women - Confirmed
(9, 1, 4, 1),   -- James - Open Men - Pending
(10, 2, 11, 2), -- Lindiwe - Amateur Women - Confirmed
(11, 2, 10, 1), -- Brian - Amateur Men - Pending
(12, 2, 11, 3), -- Nomsa - Amateur Women - Cancelled
(13, 3, 16, 2), -- Kevin - Open Men - Confirmed
(6, 3, 17, 1),  -- Amara - Open Women - Pending
(7, 3, 18, 2),  -- David - Senior Men - Confirmed
(8, 4, 25, 2),  -- Thandiwe - Amateur Women - Confirmed
(9, 4, 24, 1),  -- James - Amateur Men - Pending
(10, 5, 32, 2), -- Lindiwe - Elite Women - Confirmed
(11, 5, 31, 2), -- Brian - Elite Men - Confirmed
(12, 6, 38, 1), -- Nomsa - Veteran Women - Pending
(13, 6, 39, 2); -- Kevin - Grand Veteran Men - Confirmed

-- Results for completed events
insert into Results (ENROLMENT_ID, FINISH_TIME, FINISHING_POSITION) values
(1, '03:45:22', 127),   -- Amara - Cape Town Marathon - 127th overall
(2, '04:12:45', 256),   -- David - Cape Town Marathon - 256th overall
(3, '03:28:15', 89),    -- Thandiwe - Cape Town Marathon - 89th overall
(8, '01:15:30', 45),    -- Kevin - Johannesburg Walk - 45th overall
(10, '01:22:10', 78),   -- David - Johannesburg Walk - 78th overall
(13, '02:05:40', 34),   -- Lindiwe - Pretoria Night Run - 34th overall
(14, '02:12:55', 56);   -- Brian - Pretoria Night Run - 56th overall
