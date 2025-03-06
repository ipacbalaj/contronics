CREATE TABLE `analoguehistorys` (
  `DataNodeID` int unsigned NOT NULL COMMENT 'Foriegn key to parent DataNode',
  `Time` datetime NOT NULL COMMENT 'Time the value was read',
  `SensorValues` varchar(200) NOT NULL COMMENT 'Values received / 100 and status per active sensor',
  PRIMARY KEY (`Time`,`DataNodeID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='General analogue history';