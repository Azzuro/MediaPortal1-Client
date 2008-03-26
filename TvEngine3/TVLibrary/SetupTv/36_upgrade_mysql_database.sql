USE TvLibrary;
#

/*Insert the upgrade statements below */
ALTER TABLE `CanceledSchedule` CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC;

ALTER TABLE `Card` MODIFY COLUMN `devicePath` VARCHAR(2000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `name` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `recordingFolder` VARCHAR(256) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `timeshiftingFolder` VARCHAR(256) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `CardGroup` MODIFY COLUMN `name` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `CardGroupMap` CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `Channel` MODIFY COLUMN `name` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `externalId` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `displayName` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci; 
 
ALTER TABLE `Channel` 
DROP INDEX `IDX_Channel1`; 

ALTER TABLE `ChannelGroup` MODIFY COLUMN `groupName` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci; 

ALTER TABLE `ChannelLinkageMap` CHARACTER SET utf8 COLLATE utf8_general_ci; 

ALTER TABLE `ChannelMap` CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `Conflict` CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `DiSEqCMotor` CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `Favorite` CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `GroupMap` CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `History` MODIFY COLUMN `title` VARCHAR(1000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `description` VARCHAR(1000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `genre` VARCHAR(1000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci; 

ALTER TABLE `Keyword` MODIFY COLUMN `keywordName` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci; 

ALTER TABLE `KeywordMap` CHARACTER SET utf8 COLLATE utf8_general_ci; 

ALTER TABLE `PersonalTVGuideMap` CHARACTER SET utf8 COLLATE utf8_general_ci; 

DELETE FROM `Program`; 

ALTER TABLE `Program` MODIFY COLUMN `description` VARCHAR(8000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `Program` 
 ADD UNIQUE INDEX `idProgramBeginEnd` USING BTREE(`idChannel`, `startTime`, `endTime`); 

ALTER TABLE `RadioChannelGroup` MODIFY COLUMN `groupName` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci; 

ALTER TABLE `RadioGroupMap` CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `Recording` MODIFY COLUMN `title` VARCHAR(2000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `description` VARCHAR(8000) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `genre` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `fileName` VARCHAR(1024) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `Satellite` MODIFY COLUMN `satelliteName` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `transponderFileName` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `Schedule` MODIFY COLUMN `programName` VARCHAR(256) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `directory` VARCHAR(1024) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

ALTER TABLE `Server` MODIFY COLUMN `hostName` VARCHAR(256) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 
 
ALTER TABLE `Setting` MODIFY COLUMN `tag` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `value` VARCHAR(4096) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci; 

ALTER TABLE `Timespan` CHARACTER SET utf8 COLLATE utf8_general_ci; 

ALTER TABLE `TuningDetail` MODIFY COLUMN `name` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `provider` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `url` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci; 
 
ALTER TABLE `TvMovieMapping` MODIFY COLUMN `stationName` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `timeSharingStart` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
 MODIFY COLUMN `timeSharingEnd` VARCHAR(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL, 
 CHARACTER SET utf8 COLLATE utf8_general_ci; 

ALTER TABLE `Version` CHARACTER SET utf8 COLLATE utf8_general_ci, 
 ROW_FORMAT = DYNAMIC; 

/* Set the new schema version here */
UPDATE `Version` SET `versionNumber`=36;
