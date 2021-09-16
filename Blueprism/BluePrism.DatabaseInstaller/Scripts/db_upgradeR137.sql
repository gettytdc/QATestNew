/*
SCRIPT         : 137
PROJECT NAME   : Automate
DATABASE NAME  : BPA
AUTHOR         : CG
PURPOSE        : Add tables for process validation improvement
*/

IF OBJECT_ID('chk') IS NOT NULL
    DROP PROC chk
GO

CREATE PROC chk @id int,@cat int,@typeid int,@msg nvarchar(255)
AS
    insert into BPAValCheck (checkid,catid,typeid,description)
        values (@id,@cat,@typeid,@msg)
GO

create table BPAValCategory (
    catid int
        constraint PK_BPAValCategory primary key,
    description varchar(255) not null       
)

DECLARE @StageValidation int
SET @StageValidation=0
DECLARE @FlowValidation int
SET @FlowValidation=1
DECLARE @DocumentationControl int
SET @DocumentationControl=2
insert into BPAValCategory (catid,description) values (@StageValidation,'Stage Validation')
insert into BPAValCategory (catid,description) values (@FlowValidation,'Flow Validation')
insert into BPAValCategory (catid,description) values (@DocumentationControl,'Documentation Control')

create table BPAValType (
    typeid int
        constraint PK_BPAValType primary key,
    description varchar(255) not null       
)

DECLARE @Error int
SET @Error=0
DECLARE @Warning int
SET @Warning=1
DECLARE @Advice int
SET @Advice=2
insert into BPAValType (typeid,description) values (@Error,'Error')
insert into BPAValType (typeid,description) values (@Warning,'Warning')
insert into BPAValType (typeid,description) values (@Advice,'Advice')

create table BPAValAction (
    actionid int
        constraint PK_BPAValAction primary key,
    description varchar(255) not null       
)

insert into BPAValAction (actionid,description) values (0,'Ignore')
insert into BPAValAction (actionid,description) values (1,'Validate')
insert into BPAValAction (actionid,description) values (2,'Validate and Prevent Publication')
insert into BPAValAction (actionid,description) values (3,'Validate and Prevent Save')

create table BPAValActionMap (
    catid int not null
        references BPAValCategory(catid),
    typeid int not null
        references BPAValType(typeid),
    actionid int not null
        references BPAValAction(actionid),
    constraint PK_BPAValActionMap primary key (catid, typeid, actionid)
)

insert into BPAValActionMap (catid,typeid,actionid) values (0,0,1)
insert into BPAValActionMap (catid,typeid,actionid) values (0,1,1)
insert into BPAValActionMap (catid,typeid,actionid) values (0,2,1)
insert into BPAValActionMap (catid,typeid,actionid) values (1,0,1)
insert into BPAValActionMap (catid,typeid,actionid) values (1,1,1)
insert into BPAValActionMap (catid,typeid,actionid) values (1,2,1)
insert into BPAValActionMap (catid,typeid,actionid) values (2,0,1)
insert into BPAValActionMap (catid,typeid,actionid) values (2,1,1)
insert into BPAValActionMap (catid,typeid,actionid) values (2,2,0)

create table BPAValCheck (
    checkid int
        constraint PK_BPAValCheck primary key,
    catid int not null
        references BPAValCategory(catid),
    typeid int not null
        references BPAValType(typeid),
    description varchar(255) not null       
)

EXEC chk 0,@StageValidation,@Error,"Blank choice expression on {0}"
EXEC chk 1,@StageValidation,@Error,"Invalid choice expression on {0}"
EXEC chk 2,@StageValidation,@Error,"Expression does not result in a Flag value on {0}"
EXEC chk 3,@StageValidation,@Warning,"No name assigned to {0}"
EXEC chk 4,@FlowValidation,@Error,"No link from {0}"
EXEC chk 5,@FlowValidation,@Error,"Invalid link from {0}"
EXEC chk 6,@FlowValidation,@Error,"Link to a different page from {0}"
EXEC chk 7,@StageValidation,@Error,"Cannot store data in an environment variable"
EXEC chk 8,@StageValidation,@Error,"Can not determine resulting data type of expression{0}"
EXEC chk 9,@StageValidation,@Error,"Expression does not evaluate to a compatible target data type{0}"
EXEC chk 10,@StageValidation,@Error,"Field '{0}' does not exist in the collection{1}"
EXEC chk 11,@StageValidation,@Error,"Stage to store result in is not accessible from this page{0}"
EXEC chk 12,@StageValidation,@Error,"Stage to store in is not a Data or Collection stage{0}"
EXEC chk 13,@StageValidation,@Error,"Stage to store result in does not exist{0}"
EXEC chk 14,@StageValidation,@Error,"Decision expression does not result in a Flag value"
EXEC chk 15,@StageValidation,@Error,"The chosen action in row {0} is not permitted for the corresponding element type"
EXEC chk 16,@StageValidation,@Warning,"Row {0} contains an action which is outdated. Please consider updating."
EXEC chk 17,@StageValidation,@Error,"Internal Error: No type information found on the element in row {0}."
EXEC chk 18,@StageValidation,@Error,"There is no action selected in row {0}"
EXEC chk 19,@StageValidation,@Error,"Failed to find referenced application element in row {0}. Element ID is {1}"
EXEC chk 20,@StageValidation,@Error,"No application element chosen in row {0}"
EXEC chk 21,@StageValidation,@Error,"Expression evaluates to the wrong datatype"
EXEC chk 22,@StageValidation,@Warning,"Expression{0} is valid, but its data type cannot be resolved until runtime"
EXEC chk 23,@StageValidation,@Error,"Invalid expression{0} - {1}"
EXEC chk 24,@StageValidation,@Error,"Expression{0} is blank"
EXEC chk 25,@StageValidation,@Error,"Block overlaps another block"
EXEC chk 26,@StageValidation,@Error,"Stage is only partially within a block"
EXEC chk 27,@StageValidation,@Warning,"Page '{0}' is not called by any other page"
EXEC chk 28,@StageValidation,@Error,"Resume stage is not in a recovery section"
EXEC chk 29,@FlowValidation,@Error,"Recovery stage is linked back to main process"
EXEC chk 30,@StageValidation,@Error,"Exception stage wants to use existing exception information, but there can be no existing exception there"
EXEC chk 31,@StageValidation,@Error,"Wrong type of parameter"
EXEC chk 32,@StageValidation,@Warning,"Blank value supplied to input parameter '{0}'"
EXEC chk 33,@StageValidation,@Error,"Bad map type for an expression-type parameter"
EXEC chk 34,@StageValidation,@Error,"Failed to validate input expression to {0}parameter '{1}' - {2}"
EXEC chk 35,@StageValidation,@Error,"Expression to {0}parameter '{1}' results in a bad data type - {2}"
EXEC chk 36,@StageValidation,@Warning,"Expression uses the function '{0}', which is deprecated"
EXEC chk 37,@StageValidation,@Warning,"Failed to determine the data type of the expression to input parameter '{0}'"
EXEC chk 38,@StageValidation,@Error,"Bad map type for a stage-type parameter"
EXEC chk 39,@StageValidation,@Error,"Data item to Store In for {0}parameter '{1}' does not exist"
EXEC chk 40,@StageValidation,@Error,"Data item to Store In for {0}parameter '{1}' is out of scope"
EXEC chk 41,@StageValidation,@Error,"Collection mismatch for {0}parameter '{1}': {2}"
EXEC chk 42,@StageValidation,@Error,"Cannot store data in an environment variable"
EXEC chk 43,@StageValidation,@Error,"No 'Store In' mapping set for {0}parameter '{1}'"
EXEC chk 44,@StageValidation,@Error,"Expression is not valid{0} - {1}"
EXEC chk 45,@StageValidation,@Warning,"Expression{0} uses the function {1}, which is deprecated"
EXEC chk 46,@StageValidation,@Error,"Blank expression{0}"
EXEC chk 47,@StageValidation,@Error,"Can not determine resulting data type of expression"
EXEC chk 48,@StageValidation,@Warning,"Expression uses the function '{0}', which is deprecated"
EXEC chk 49,@StageValidation,@Error,"Invalid expression"
EXEC chk 50,@StageValidation,@Error,"Blank decision expression"
EXEC chk 51,@FlowValidation,@Error,"No link when the decision is True"
EXEC chk 52,@FlowValidation,@Error,"Invalid link for the True result"
EXEC chk 53,@FlowValidation,@Error,"True link goes to a different page"
EXEC chk 54,@FlowValidation,@Error,"No link when the decision is False"
EXEC chk 55,@FlowValidation,@Error,"Invalid link for the False result"
EXEC chk 56,@FlowValidation,@Error,"False link goes to a different page"
EXEC chk 57,@FlowValidation,@Error,"Missing link"
EXEC chk 58,@StageValidation,@Warning,"Stage has blank name"
EXEC chk 59,@StageValidation,@Error,"Invalid expression in field Exception Detail -  {0}"
EXEC chk 60,@StageValidation,@Error,"Invalid data type match in row {0}."
EXEC chk 61,@StageValidation,@Error,"The stage referenced in row {0} is out of scope."
EXEC chk 62,@StageValidation,@Error,"The stage referenced in row {0} does not exist."
EXEC chk 63,@StageValidation,@Error,"The element referenced in row {0} has no data type."
EXEC chk 64,@StageValidation,@Error,"Failed to find referenced application element in row {0}. Element ID is {1}"
EXEC chk 65,@StageValidation,@Warning,"The stage calls a process with parameter '{0}' which is not defined in the stage"
EXEC chk 66,@StageValidation,@Warning,"The Page Reference stage provides a value for the input parameter '{0}' which is no longer defined by the target page."
EXEC chk 67,@StageValidation,@Error,"The Page Reference stage provides a mapping for the Output parameter '{0}' which is no longer defined by the target page."
EXEC chk 68,@StageValidation,@Error,"Row {0} does not have a valid target element"
EXEC chk 69,@StageValidation,@Error,"Row {0} does not specify a condition"
EXEC chk 70,@StageValidation,@Warning,"Row {0} contains a condition which is outdated. Please consider updating."
EXEC chk 71,@StageValidation,@Error,"The timeout expression on this wait stage contains an error"
EXEC chk 72,@StageValidation,@Error,"Row {0} has no application element set"
EXEC chk 73,@StageValidation,@Warning,"Blank value supplied to input parameter '{0}'{1}"
EXEC chk 74,@StageValidation,@Error,"Failed to validate input expression to parameter '{0}'{2} - {1}"
EXEC chk 75,@StageValidation,@Error,"Expression to parameter '{0}'{2} results in a bad data type - {1}"
EXEC chk 76,@StageValidation,@Warning,"Failed to determine the data type of the expression to input parameter '{0}'{1}"
EXEC chk 77,@StageValidation,@Error,"Row {0} does not have a condition which is appropriate to the target element"
EXEC chk 78,@StageValidation,@Error,"Row {0} has no read action defined."
EXEC chk 79,@StageValidation,@Error,"The chosen read action in row {0} is not permitted for the corresponding element type"
EXEC chk 80,@StageValidation,@Warning,"Row {0} contains an action which is outdated. Please consider updating."
EXEC chk 81,@StageValidation,@Error,"Row {0} has an invalid store in location - the stage '{1}' does not exist."
EXEC chk 82,@StageValidation,@Error,N'Row {0} has an invalid store in location - the collection stage ''{1}'' has some fields defined, whereas an empty (dynamic) collection is required.'
EXEC chk 83,@StageValidation,@Error,"Row {0} has a blank store in location"
EXEC chk 84,@StageValidation,@Error,"Row {0} has an invalid store in location - the stage '{1}' cannot be found.{2}"
EXEC chk 85,@StageValidation,@Error,"Row {0} has an invalid store in location - the stage '{1}' is located on another page and is marked as hidden."
EXEC chk 86,@StageValidation,@Error,"Row {0} does not refer to a valid application element"
EXEC chk 87,@StageValidation,@Error,"Row {0} supplies an expression to argument '{1}' which evaluates to the wrong datatype"
EXEC chk 88,@StageValidation,@Warning,"Row {0} supplies an expression to argument '{1}' which is valid, but whose data type cannot be resolved until runtime"
EXEC chk 89,@StageValidation,@Error,"Row {0} supplies an invalid expression to argument '{1}'"
EXEC chk 90,@StageValidation,@Error,"Row {0} supplies a blank expression to argument '{1}'"
EXEC chk 91,@StageValidation,@Error,"Row {0} does not supply a value to the compulsory parameter '{1}'"
EXEC chk 92,@StageValidation,@Error,"Row {0} contains an argument '{1}' that is not defined in the application model"
EXEC chk 93,@StageValidation,@Error,"Parameter for attribute '{0}' in row {1} contains an invalid expression"
EXEC chk 94,@StageValidation,@Error,"Missing element parameter for attribute '{0}' in row {1}"
EXEC chk 95,@StageValidation,@Error,"The data item '{0}' on page '{1}' conflicts with the data item '{2}' on page '{3}'"
EXEC chk 96,@StageValidation,@Error,"Data item does not yet have a data type"
EXEC chk 97,@StageValidation,@Error,"The Action has no Business Object (or Action) set."
EXEC chk 98,@StageValidation,@Error,"The Action refers to the Business Object '{0}' which does not seem to be installed."
EXEC chk 99,@StageValidation,@Error,"The referenced business object has been retired."
EXEC chk 100,@StageValidation,@Error,N'The Action refers to the action ''{0}'' within the Business Object ''{1}'' but this Business Object does not contain an action with that name.'
EXEC chk 101,@FlowValidation,@Error,"Invalid link"
EXEC chk 102,@FlowValidation,@Error,"Link to a different page"
EXEC chk 103,@FlowValidation,@Error,"Missing group ID"
EXEC chk 104,@FlowValidation,@Error,"Missing start stage for loop"
EXEC chk 105,@FlowValidation,@Error,"Loop start is on a different page"
EXEC chk 106,@FlowValidation,@Error,"Missing group ID on loop start stage"
EXEC chk 107,@FlowValidation,@Error,"Missing end stage for loop"
EXEC chk 108,@FlowValidation,@Error,"Loop end is on a different page"
EXEC chk 109,@StageValidation,@Error,"The Page Reference stage does not refer to any page."
EXEC chk 110,@StageValidation,@Error,"The Page Reference stage refers to a page which does not exist."
EXEC chk 111,@StageValidation,@Error,"The Page Reference stage does not acknowledge the input parameter '{0}' defined by the target page."
EXEC chk 112,@StageValidation,@Error,"The Page Reference stage does not acknowledge the output parameter '{0}' defined by the target page."
EXEC chk 113,@StageValidation,@Error,"Collection field definition '{0}' has an invalid name"
EXEC chk 114,@StageValidation,@Error,"Initial value field '{0}' has an invalid name"
EXEC chk 115,@StageValidation,@Error,"Data item refers to a missing environment variable '{0}'"
EXEC chk 116,@StageValidation,@Error,"Undefined loop type"
EXEC chk 117,@StageValidation,@Error,"Unrecognised loop type"
EXEC chk 118,@StageValidation,@Error,"No collection set for loop stage"
EXEC chk 119,@StageValidation,@Error,"Can not find collection stage '{0}' referenced in loop stage"
EXEC chk 120,@StageValidation,@Error,"Loop stages references a collection which is not accessible - it is private and resides on another page"
EXEC chk 121,@StageValidation,@Error,"The Process stage has not yet set the subprocess."
EXEC chk 122,@StageValidation,@Error,"The Process stage references a subprocess which has been retired."
EXEC chk 123,@StageValidation,@Error,"Compiler warning at line {0}: {1}"
EXEC chk 124,@StageValidation,@Error,"Compiler error at line {0}: {1}"
EXEC chk 125,@StageValidation,@Error,"Compiler warning at top section line {0}: {1}"
EXEC chk 126,@StageValidation,@Error,"Compiler error at top section line {0}: {1}"

--set DB version
INSERT INTO BPADBVersion VALUES (
  '137',
  GETUTCDATE(),
  'db_upgradeR137.sql UTC',
  'Add tables for process validation improvement'
)

