select  datanodeid
,timevalue
,sensorvalues
from public.history
LIMIT 1000

SELECT count(*) from public.history

SELECT *
FROM history
WHERE 
    -- Filter data for March 2022
    TimeValue >= '2022-03-01'::timestamp
    AND TimeValue < '2022-03-04'::timestamp;


SELECT *
FROM history
WHERE 
    -- Filter data for March 2022, and the first 3 hours of March 1st, 2022
    TimeValue >= '2022-03-01 00:00:00'::timestamp
    AND TimeValue < '2022-03-01 00:20:00'::timestamp;


SELECT 
    time_bucket('1 hour', TimeValue) AS hour_bucket,
    COUNT(*) AS num_records
FROM history
WHERE 
    -- Filter data for March 1st, 2022, first 3 hours
    TimeValue >= '2022-03-01 00:00:00'::timestamp
    AND TimeValue < '2022-07-01 03:00:00'::timestamp
GROUP BY hour_bucket
ORDER BY hour_bucket;
