Project Title
RaceDay Event Management System

Student Information
Name: Ives Mukadi
Student Number: st10365478
Module: Programming 2B

Project Overview
RaceDay is a platform for managing running, walking and cycling events. Organisers create events and record results, and participants browse events, enrol in a category and view their own results. The project is built in three parts. This repository currently contains Part 1: the planning documents and the SQL database script.

Roles
Organiser: can create, edit and delete events, manage event categories, view all enrolments for their events and capture participant results.
Participant: can create an account, browse events, enrol in an event by choosing a category, view their own enrolments and track their personal results.

Part 1 Details
Part 1 covers planning only. No API code has been written yet.

Files in Part1/docs:

ERD.png - entity relationship diagram (8 entities)
API_Endpoint_Plan.pdf - table of all 21 API endpoints
raceday_setup.sql - creates the RaceDay database, tables and sample data

How to run the SQL script
1. Open SQL Server Management Studio and connect to your server.
2. Open Part1/docs/raceday_setup.sql.
3. Click Execute (F5).
4. Refresh Databases and check that RaceDay has 8 tables.

The script creates the database itself, so run it on a clean server. If RaceDay already exists, drop it first.

Differences between the ERD and the SQL script
The SQL script was improved after the ERD was drawn. These are the deliberate differences:

Some foreign keys use ON DELETE CASCADE (Categories to Events, Enrolments to Events and Results to Enrolments), so deleting an event also deletes its categories, enrolments and results. The other foreign keys do not cascade because SQL Server does not allow multiple cascade paths. The ERD does not show this.
Categories has a UNIQUE constraint on (EVENT_ID, CATEGORY_NAME) so an event cannot have two categories with the same name.
Events.EVENT_STATUS_ID and Enrolments.STATUS_ID are NOT NULL with defaults (2 and 1). The ERD does not show this.
The primary key of Event_Status is named EVENT_STATUS_ID in the SQL script. The ERD calls it STATUS_ID.
Seed passwords are placeholders (hashed_pass_1 and so on). Real hashing will be done in the API in Part 2.


